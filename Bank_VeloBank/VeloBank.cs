using BankService.Bank_ING;
using BankService.LocalTools;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Tools;
using ToolsNugetExtension;
using static BankService.Bank_GetinBank.GetinBankJsonResponse;
using static BankService.Bank_VeloBank.VeloBankJsonRequest;
using static BankService.Bank_VeloBank.VeloBankJsonResponse;

namespace BankService.Bank_VeloBank
{
    [BankTypeAttribute(BankType.VeloBank)]
    public class VeloBank : BankBase<VeloBankHistoryItem, VeloBankHistoryFilter>
    {
        public const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/115.0";

        private VeloBankJsonResponseAccounts accountsDetails;
        private VeloBankJsonResponseAccounts AccountsDetails
        {
            get => accountsDetails ?? (accountsDetails = GetAccountsDetails());
            set
            {
                accountsDetails = value;
                if (accountsDetails == null)
                    CallAvailableFundsClear();
            }
        }

        private string defaultContextHash { get; set; }

        protected override int HeartbeatInterval => 180;

        public override bool FastTransferMandatoryTransferId => true;
        public override bool FastTransferMandatoryBrowserCookies => false;
        public override bool FastTransferMandatoryCookie => false;
        public override bool TransferMandatoryTitle => true;
        public override bool PrepaidTransferMandatoryRecipient => true;

        protected override string BaseAddress => "https://secure.velobank.pl/api/v004/";

        private void CleanHttpClient()
        {
            httpClient.DefaultRequestHeaders.Authorization = null;
            defaultContextHash = null;
        }

        protected override bool LoginRequest(string login, string password)
        {
            return LoginRequest(login, password, null);
        }

        private bool LoginRequest(string login, string password, string transferId)
        {
            VeloJsonResponseLoginLogin loginLoginResponse = PerformRequest<VeloJsonResponseLoginLogin>($"Users/passwordType/login/{login}", HttpMethod.Get, null, null);

            if (loginLoginResponse.CheckErrorExists(10036))
                return CheckFailed("Odblokuj dostęp na stronie internetowej");

            bool isPasswordMasked = !loginLoginResponse.is_password_plain;

            VeloJsonRequestLoginPassword jsonRequestLoginPassword = new VeloJsonRequestLoginPassword();

            jsonRequestLoginPassword.login = login;
            jsonRequestLoginPassword.method = "PASSWORD";
            if (isPasswordMasked)
            {
                List<int> mask = loginLoginResponse.password_combinations.Split(new string[] { "," }, StringSplitOptions.None).Select(s => Int32.Parse(s)).ToList();

                if (mask.Max() > password.Length - 1)
                    return CheckFailed("Niepoprawne hasło");

                StringBuilder maskedPassword = new StringBuilder();
                foreach (int i in mask)
                    maskedPassword.Append(password[i]);
                jsonRequestLoginPassword.password = maskedPassword.ToString();
            }
            else
            {
                jsonRequestLoginPassword.password = password;
            }

            if (transferId == null)
            {
                jsonRequestLoginPassword.module = "BANKING";
            }
            else
            {
                (FastTransferType? type, (string key, string hash) paData, string pblData) fastTransferData = GetDataFromFastTransfer(transferId);
                if (fastTransferData.type == FastTransferType.PA)
                {
                    jsonRequestLoginPassword.module = "PA";
                    jsonRequestLoginPassword.consent_request_data = new VeloJsonRequestLoginPasswordConsentRequestData() { authorize_request_key = fastTransferData.paData.key, hash = fastTransferData.paData.hash };
                }
                else if (fastTransferData.type == FastTransferType.PayByLink)
                {
                    jsonRequestLoginPassword.module = "PBL";
                }
            }

            VeloJsonResponseLoginPassword loginPasswordResponse = PerformRequest<VeloJsonResponseLoginPassword>(
               "Session/create", HttpMethod.Post, JsonConvert.SerializeObject(jsonRequestLoginPassword), null);

            if (loginPasswordResponse.CheckErrorExists(10096))
                return CheckFailed("Niepoprawne hasło");
            else if (loginPasswordResponse.CheckErrorExists(10036))
                return CheckFailed("Niepoprawne hasło. Odblokuj dostęp na stronie internetowej");
            if (loginPasswordResponse.CheckErrorExists(10351)) //fast transfer
                return CheckFailed("Niepoprawny kod");

            if (loginPasswordResponse.access_token != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginPasswordResponse.access_token);
                defaultContextHash = loginPasswordResponse.default_context_hash;
            }
            else
            {
                (bool, VeloJsonResponseConfirm) confirm = Confirm(loginPasswordResponse);
                if (!confirm.Item1)
                    return false;

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", confirm.Item2.response.session_create.access_token);
                defaultContextHash = confirm.Item2.response.session_create.default_context_hash;

                if (PromptYesNo("Dodać urządzenie do zarejestrowanych?"))
                {
                    VeloJsonRequestRememberDevice jsonRequestRememberDevice = new VeloJsonRequestRememberDevice();
                    jsonRequestRememberDevice.option = "PERMANENT";

                    VeloJsonResponseRememberDevice rememberDeviceResponse = PerformRequest<VeloJsonResponseRememberDevice>(
                       "Banking/rememberDevice", HttpMethod.Post, JsonConvert.SerializeObject(jsonRequestRememberDevice), null);

                    if (!Confirm(rememberDeviceResponse).Item1)
                        return false;
                }
            }

            return true;
        }

        protected override bool LogoutRequest()
        {
            PerformRequest<VeloJsonResponseLogout>("Session/delete", HttpMethod.Delete, null, null);

            return true;
        }

        protected override void PostLogoutRequest()
        {
            base.PostLogoutRequest();
            AccountsDetails = null;
            CleanHttpClient();
        }

        protected override bool TryExtendSession()
        {
            VeloJsonResponseHeartbeat heartbeatResponse = PerformRequest<VeloJsonResponseHeartbeat>("Session/extend", HttpMethod.Post, null, null);

            if (heartbeatResponse == null)
                return true;

            return true;
        }

        //TODO pay-by-link
        protected override string CleanFastTransferUrl(string transferId)
        {
            string newTransferId = transferId
                .Replace("https://secure.velobank.pl/login/pa/", String.Empty)
                
                .Replace("https://secure.velobank.pl/login/pbl/", String.Empty)
                .Replace("/mobile", String.Empty);

            (FastTransferType? type, (string key, string hash) pblData, string paData) fastTransferData = GetDataFromFastTransfer(newTransferId);

            if (fastTransferData.type == null)
                return null;

            return newTransferId;
        }

        protected override bool LoginRequestForFastTransfer(string login, string password, string transferId, string cookie)
        {
            return LoginRequest(login, password, transferId) && PostLoginRequest();
        }

        protected override string MakeFastTransfer(string transferId, string cookie)
        {
                (FastTransferType? type, (string key, string hash) paData, string pblData) fastTransferData = GetDataFromFastTransfer(transferId);

            if (fastTransferData.type == FastTransferType.PA)
            {
                VeloBankJsonResponseFastTransferAuthorize fastTransferAuthorizeResponse = PerformRequest<VeloBankJsonResponseFastTransferAuthorize>(
                    $"Consent/get?authorize_request_key={fastTransferData.paData.key}", HttpMethod.Get, null, null);

                if (fastTransferAuthorizeResponse.CheckErrorExists(502))
                {
                    CheckFailed("Kod nieważny");
                    return null;
                }

                VeloJsonRequestFastTransferAcceptAuthorize jsonRequestFastTransferAuthorize = new VeloJsonRequestFastTransferAcceptAuthorize();
                jsonRequestFastTransferAuthorize.privilege_details = fastTransferAuthorizeResponse.agreements.Select(a => new VeloJsonRequestFastTransferAcceptAuthorizePrivilegeDetail() { id = a.id_agreement, products = a.products.Select(p => new VeloJsonRequestAccountNumber() { account_number = p.account_number.account_number, country_code = p.account_number.country_code }).ToList() }).ToList();

                VeloBankJsonResponseFastTransferAcceptAuthorize fastTransferAcceptAuthorizeResponse = PerformRequest<VeloBankJsonResponseFastTransferAcceptAuthorize>(
                    $"Consent/accept/authorize_request_key/{fastTransferData.paData.key}", HttpMethod.Put, JsonConvert.SerializeObject(jsonRequestFastTransferAuthorize), null);

                if (!Confirm(fastTransferAcceptAuthorizeResponse).Item1)
                    return null;

                return fastTransferAcceptAuthorizeResponse.redirect_uri;
            }
            else if (fastTransferData.type == FastTransferType.PayByLink)
            {
                VeloBankJsonResponseFastTransferPBL fastTransferPBLResponse = PerformRequest<VeloBankJsonResponseFastTransferPBL>(
                    $"PayByLink/details/hash/{fastTransferData.pblData}", HttpMethod.Get, null, null);

                VeloJsonRequestFastTransferAcceptPBL jsonRequestFastTransferAcceptPBL = new VeloJsonRequestFastTransferAcceptPBL();
                jsonRequestFastTransferAcceptPBL.id_product = fastTransferPBLResponse.products.First().id;

                VeloBankJsonResponseFastTransferAcceptPBL fastTransferAcceptPBLResponse = PerformRequest<VeloBankJsonResponseFastTransferAcceptPBL>(
                    $"PayByLink/accept/hash/{fastTransferData.paData.key}", HttpMethod.Put, JsonConvert.SerializeObject(jsonRequestFastTransferAcceptPBL), null);

                if (!Confirm(fastTransferAcceptPBLResponse).Item1)
                    return null;

                return fastTransferPBLResponse.redirect_uri;
            }
            else
                throw new NotImplementedException();
        }

        protected override void PostFastTransfer()
        {
            base.PostFastTransfer();
            AccountsDetails = null;
            CleanHttpClient();
        }

        private VeloBankJsonResponseAccounts GetAccountsDetails()
        {
            VeloBankJsonResponseAccounts getBankNameResponse = PerformRequest<VeloBankJsonResponseAccounts>(
                //"https://secure.velobank.pl/api/v006/Users/finances?type=DASHBOARD"
                "Users/finances?type=DASHBOARD", HttpMethod.Get, null, null);

            return getBankNameResponse;
        }

        private VeloBankJsonResponseAccountsAccountsSummaryProduct GetAccount()
        {
            return AccountsDetails.accounts.summary.First().products.First();
        }

        public override (string accountNumber, double availableFunds) GetAccountData()
        {
            return (GetAccount().account_number.account_number, GetAccount().available_funds.amount);
        }

        public override bool MakeTransfer(string recipient, string address, string accountNumber, string title, double amount)
        {
            string currency = "PLN";
            string formattedAccountNumber = accountNumber.SimplifyAccountNumber();

            VeloJsonRequestTransferInfo jsonRequestTransferInfo = new VeloJsonRequestTransferInfo();
            jsonRequestTransferInfo.amount = new VeloJsonRequestAmount() { amount = amount.ToString("F", CultureInfo.CreateSpecificCulture("en-CA")), currency = currency };
            jsonRequestTransferInfo.sender_account_number = new VeloJsonRequestAccountNumber() { account_number = GetAccountData().accountNumber, country_code = null };
            jsonRequestTransferInfo.recipient_account_number = new VeloJsonRequestAccountNumber() { account_number = formattedAccountNumber, country_code = "PL" };
            jsonRequestTransferInfo.transfer_mode = "ELIXIR";
            jsonRequestTransferInfo.transfer_type = "TRANSFER";
            jsonRequestTransferInfo.transfer_date = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.CreateSpecificCulture("es-ES"));

            VeloBankJsonResponseTransferInfo transferInfoResponse = PerformRequest<VeloBankJsonResponseTransferInfo>(
                "Transfers/info", HttpMethod.Post, JsonConvert.SerializeObject(jsonRequestTransferInfo), null);

            if (transferInfoResponse.recipient_bank == null)
                return CheckFailed("Niepoprawny numer konta");

            VeloJsonRequestTransferCheck jsonRequestTransferCheck = new VeloJsonRequestTransferCheck();
            jsonRequestTransferCheck.amount = new VeloJsonRequestAmount() { amount = amount.ToString("F", CultureInfo.CreateSpecificCulture("en-CA")), currency = currency };
            jsonRequestTransferCheck.recipient_account_number = new VeloJsonRequestAccountNumber() { account_number = formattedAccountNumber, country_code = "PL" };
            jsonRequestTransferCheck.source_product = GetAccount().id;
            jsonRequestTransferCheck.title = title;
            jsonRequestTransferCheck.transfer_mode = "ELIXIR";
            jsonRequestTransferCheck.transfer_type = "TRANSFER";
            jsonRequestTransferCheck.payment_date = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.CreateSpecificCulture("es-ES"));

            VeloBankJsonResponseTransferCheck transferCheckResponse = PerformRequest<VeloBankJsonResponseTransferCheck>(
                "Transfers/check", HttpMethod.Post, JsonConvert.SerializeObject(jsonRequestTransferCheck), null);

            VeloJsonRequestTransferDomestic jsonRequestTransferDomestic = new VeloJsonRequestTransferDomestic();
            jsonRequestTransferDomestic.amount = new VeloJsonRequestAmount() { amount = amount.ToString("F", CultureInfo.CreateSpecificCulture("en-CA")), currency = currency };
            jsonRequestTransferDomestic.recipient_account_number = new VeloJsonRequestAccountNumber() { account_number = formattedAccountNumber, country_code = "PL" };
            jsonRequestTransferDomestic.recipient_address = address;
            jsonRequestTransferDomestic.recipient_id = null;
            jsonRequestTransferDomestic.recipient_name = recipient;
            jsonRequestTransferDomestic.retry_if_lack_of_funds = false;
            jsonRequestTransferDomestic.save_recipient = false;
            jsonRequestTransferDomestic.send_notification_to_email = false;
            jsonRequestTransferDomestic.source_product = GetAccount().id;
            jsonRequestTransferDomestic.title = title;
            jsonRequestTransferDomestic.type = "ELIXIR";
            jsonRequestTransferDomestic.payment_date = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.CreateSpecificCulture("es-ES"));

            VeloBankJsonResponseTransferDomestic transferDomesticResponse = PerformRequest<VeloBankJsonResponseTransferDomestic>(
                "Transfers/domestic", HttpMethod.Post, JsonConvert.SerializeObject(jsonRequestTransferDomestic), null);

            if (transferDomesticResponse.CheckErrorExists(10031) || transferDomesticResponse.CheckErrorExists(20808) || transferDomesticResponse.CheckErrorExists(22006))
                return CheckFailed("Błędne dane");

            if (!Confirm(transferDomesticResponse).Item1)
                return false;

            return true;
        }

        protected override void PostTransfer()
        {
            base.PostTransfer();
            AccountsDetails = null;
        }

        protected override bool MakePrepaidTransfer(string recipient, string phoneNumber, double amount)
        {
            string currency = "PLN";

            VeloBankJsonResponseTransferPrepaidInfo transferPrepaidInfoResponse = PerformRequest<VeloBankJsonResponseTransferPrepaidInfo>(
                "Transfers/prepaidInfo", HttpMethod.Get, null, null);

            (string name, VeloBankJsonResponseTransferPrepaidInfoOperator data) operatorItem = PromptComboBox<VeloBankJsonResponseTransferPrepaidInfoOperator>("Operator", transferPrepaidInfoResponse.operators.Select(o => new PrepaidOperatorComboBoxItem<VeloBankJsonResponseTransferPrepaidInfoOperator>(o.display_name, o)));
            if (operatorItem.data == null)
                return false;

            if ((operatorItem.data.amount_min != null && amount < operatorItem.data.amount_min.amount)
                || (operatorItem.data.amount_max != null && amount > operatorItem.data.amount_max.amount))
                return CheckFailed($"Kwota powinna znajdować się w zakresie {operatorItem.data.amount_min?.amount}-{operatorItem.data.amount_max?.amount}");
            if (operatorItem.data.amounts.Count != 0 && !operatorItem.data.amounts.Select(a => a.amount).Contains(amount))
                return CheckFailed($"Kwota powinna być jedną z {String.Join(", ", operatorItem.data.amounts.Select(a => a.amount))}");

            VeloJsonRequestTransferCheck jsonRequestTransferCheck = new VeloJsonRequestTransferCheck();
            jsonRequestTransferCheck.amount = new VeloJsonRequestAmount() { amount = amount.ToString("F", CultureInfo.CreateSpecificCulture("en-CA")), currency = currency };
            jsonRequestTransferCheck.recipient_phone = new VeloJsonRequestPhone() { id_operator = operatorItem.data.id, prefix = "+48", phone_number = phoneNumber };
            jsonRequestTransferCheck.source_product = GetAccount().id;
            jsonRequestTransferCheck.transfer_mode = "ELIXIR";
            jsonRequestTransferCheck.transfer_type = "PREPAID_TRANSFER";
            jsonRequestTransferCheck.payment_date = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.CreateSpecificCulture("es-ES"));

            VeloBankJsonResponseTransferCheck transferCheckResponse = PerformRequest<VeloBankJsonResponseTransferCheck>(
                "Transfers/check", HttpMethod.Post, JsonConvert.SerializeObject(jsonRequestTransferCheck), null);

            if (transferCheckResponse.CheckErrorExists(22006))
                return CheckFailed("Błędne dane");

            VeloJsonRequestTransferPrepaid jsonRequestTransferPrepaid = new VeloJsonRequestTransferPrepaid();
            jsonRequestTransferPrepaid.amount = new VeloJsonRequestAmount() { amount = amount.ToString("F", CultureInfo.CreateSpecificCulture("en-CA")), currency = currency };
            jsonRequestTransferPrepaid.phone_number = new VeloJsonRequestPhone() { id_operator = operatorItem.data.id, prefix = "+48", phone_number = phoneNumber };
            jsonRequestTransferPrepaid.source_product = GetAccount().id;
            jsonRequestTransferPrepaid.recipient_id = null;
            jsonRequestTransferPrepaid.recipient_name = recipient;
            jsonRequestTransferPrepaid.save_recipient = false;
            jsonRequestTransferPrepaid.payment_date = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.CreateSpecificCulture("es-ES"));

            VeloBankJsonResponseTransferPrepaid transferPrepaidResponse = PerformRequest<VeloBankJsonResponseTransferPrepaid>(
                "Transfers/prepaid", HttpMethod.Post, JsonConvert.SerializeObject(jsonRequestTransferPrepaid), null);

            if (transferPrepaidResponse.CheckErrorExists(20808) || transferPrepaidResponse.CheckErrorExists(22006))
                return CheckFailed("Błędne dane");

            if (!Confirm(transferPrepaidResponse).Item1)
                return false;

            return true;
        }

        public override void GetDetailsFile(HistoryItem item, FileStream file)
        {
            VeloJsonRequestDetailsFile jsonRequestDetailsFile = new VeloJsonRequestDetailsFile();
            jsonRequestDetailsFile.id = new List<string>() { item.Id };
            jsonRequestDetailsFile.product = GetAccount().id;
            jsonRequestDetailsFile.type = "TRANSFER";

            VeloBankJsonResponseDetailsFile detailsFileResponse = PerformRequest<VeloBankJsonResponseDetailsFile>(
                "File/get", HttpMethod.Post, JsonConvert.SerializeObject(jsonRequestDetailsFile), null);

            PerformRequest<VeloJsonResponseBase>("https://secure.velobank.pl" + detailsFileResponse.path, HttpMethod.Get, null, (Stream contentStream) =>
            {
                contentStream.CopyTo(file);
            });
        }

        protected override VeloBankHistoryFilter CreateFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact)
        {
            return new VeloBankHistoryFilter() { DateFrom = dateFrom, DateTo = dateTo, Direction = direction, AmountExact = amountExact, Title = title };
        }

        protected override List<VeloBankHistoryItem> GetHistoryItems(VeloBankHistoryFilter filter = null)
        {
            List<VeloBankHistoryItem> result = new List<VeloBankHistoryItem>();

            if (filter != null)
            {
                if (filter.FindAccountNumber)
                {
                    //TODO to samo co w getinbank
                    VeloBankAcountNumbersHistory.Download(this);

                    Dictionary<(DateTime dateFrom, DateTime dateTo), List<string>> ranges = new Dictionary<(DateTime dateFrom, DateTime dateTo), List<string>>();
                    foreach (DictionaryEntry entry in Properties.Settings.Default.VeloBankAcountNumbers)
                    {
                        if (AccountNumberTools.CompareAccountNumbers((string)entry.Value, filter.AccountNumber))
                        {
                            DateTime transferDate = GetDateFromReferenceNumber((string)entry.Key);
                            if ((filter.DateFrom == null || filter.DateFrom <= transferDate) && (filter.DateTo == null || transferDate <= filter.DateTo))
                            {
                                List<string> refNumbers;
                                DateTime rangeDateFrom;
                                DateTime rangeDateTo;
                                KeyValuePair<(DateTime dateFrom, DateTime dateTo), List<string>> range = ranges.SingleOrDefault(r => r.Key.dateFrom <= transferDate && transferDate <= r.Key.dateTo);
                                if (!range.Equals(default(KeyValuePair<(DateTime dateFrom, DateTime dateTo), List<string>>)))
                                {
                                    refNumbers = range.Value;
                                    rangeDateFrom = range.Key.dateFrom;
                                    rangeDateTo = range.Key.dateTo;
                                }
                                else
                                {
                                    refNumbers = new List<string>();
                                    rangeDateFrom = transferDate;
                                    rangeDateTo = transferDate;
                                }
                                refNumbers.Add((string)entry.Key);
                                ranges[(rangeDateFrom, rangeDateTo)] = refNumbers;

                                KeyValuePair<(DateTime dateFrom, DateTime dateTo), List<string>> bottomRange = ranges.SingleOrDefault(r => r.Key.dateTo == transferDate.AddDays(-1));
                                if (!bottomRange.Equals(default(KeyValuePair<(DateTime dateFrom, DateTime dateTo), List<string>>)))
                                {
                                    List<string> refNumbersMerge = bottomRange.Value;
                                    refNumbers.AddRange(refNumbersMerge);
                                    ranges.Remove((rangeDateFrom, rangeDateTo));
                                    ranges.Remove(bottomRange.Key);
                                    rangeDateFrom = bottomRange.Key.dateFrom;
                                    ranges[(rangeDateFrom, rangeDateTo)] = refNumbers;
                                }
                                KeyValuePair<(DateTime dateFrom, DateTime dateTo), List<string>> topRange = ranges.SingleOrDefault(r => r.Key.dateFrom == transferDate.AddDays(1));
                                if (!topRange.Equals(default(KeyValuePair<(DateTime dateFrom, DateTime dateTo), List<string>>)))
                                {
                                    List<string> refNumbersMerge = topRange.Value;
                                    refNumbers.AddRange(refNumbersMerge);
                                    ranges.Remove((rangeDateFrom, rangeDateTo));
                                    ranges.Remove(topRange.Key);
                                    rangeDateTo = topRange.Key.dateTo;
                                    ranges[(rangeDateFrom, rangeDateTo)] = refNumbers;
                                }
                            }
                        }
                    }

                    foreach (KeyValuePair<(DateTime dateFrom, DateTime dateTo), List<string>> range in ranges.OrderByDescending(r => r.Key.dateFrom))
                    {
                        List<VeloBankHistoryItem> items = GetHistoryItems(new VeloBankHistoryFilter()
                        {
                            DateFrom = range.Key.dateFrom,
                            DateTo = range.Key.dateTo,
                            AmountExact = filter.AmountExact,
                            AmountFrom = filter.AmountFrom,
                            AmountTo = filter.AmountTo,
                            OperationType = filter.OperationType,
                            Title = filter.Title
                        });
                        if (items != null)
                            result.AddRange(items.Where(i => range.Value.Contains(i.ReferenceNumber)));
                    }
                }
                else
                {
                    bool fetchedAll = false;
                    for (int page = 1; !fetchedAll && (filter.CounterLimit == 0 || result.Count < filter.CounterLimit); page++)
                    {
                        VeloJsonRequestHistory jsonRequestHistory = new VeloJsonRequestHistory();
                        jsonRequestHistory.show_blockades = true;
                        jsonRequestHistory.search = filter.Title ?? String.Empty;
                        jsonRequestHistory.paginator = new VeloJsonRequestHistoryPaginator() { limit = 15, page = page };
                        jsonRequestHistory.filters = new VeloJsonRequestHistoryFilters() { products = new List<string>() { GetAccount().id }, cards = new List<string>() };
                        if (filter.DateFrom != null)
                            jsonRequestHistory.filters.date_from = ((DateTime)filter.DateFrom).ToString("yyyy-MM-dd", CultureInfo.CreateSpecificCulture("es-ES"));
                        if (filter.DateTo != null)
                            jsonRequestHistory.filters.date_to = ((DateTime)filter.DateTo).ToString("yyyy-MM-dd", CultureInfo.CreateSpecificCulture("es-ES"));
                        if (filter.AmountFrom != null)
                            jsonRequestHistory.filters.min_amount = ((double)filter.AmountFrom).ToString("F", CultureInfo.CreateSpecificCulture("en-CA"));
                        if (filter.AmountTo != null)
                            jsonRequestHistory.filters.max_amount = ((double)filter.AmountTo).ToString("F", CultureInfo.CreateSpecificCulture("en-CA"));
                        jsonRequestHistory.filters.type = filter.OperationType == null ? String.Empty : AttributeOperations.GetEnumAttribute((VeloBankFilterOperation)filter.OperationType, (FilterEnumParameterAttribute parameter) => parameter.Parameter, String.Empty);
                        jsonRequestHistory.filters.kind = filter.KindType == null ? String.Empty : AttributeOperations.GetEnumAttribute((VeloBankFilterKind)filter.KindType, (FilterEnumParameterAttribute parameter) => parameter.Parameter, String.Empty);
                        jsonRequestHistory.filters.status = filter.StatusType == null ? String.Empty : AttributeOperations.GetEnumAttribute((VeloBankFilterStatus)filter.StatusType, (FilterEnumParameterAttribute parameter) => parameter.Parameter, String.Empty);

                        VeloBankJsonResponseHistory historyResponse = PerformRequest<VeloBankJsonResponseHistory>(
                            "Transfers/history", HttpMethod.Post, JsonConvert.SerializeObject(jsonRequestHistory), null);

                        fetchedAll = !historyResponse.pagination.has_next_page;

                        result.AddRange(historyResponse.list.Select(i => new VeloBankHistoryItem(i)));
                    }
                }
            }

            return result;
        }

        private (bool, VeloJsonResponseConfirm) Confirm(VeloJsonResponseConfirmable response)
        {
            VeloJsonResponseConfirm confirmResponse = null;

            switch (response.TypeValue)
            {
                case VeloBankJsonConfirmType.SMS:
                    bool codeProceeded = false;
                    while (!codeProceeded)
                    {
                        string SMSCode = GetSMSCode($"Kod SMS nr {response.sms_no}");
                        if (SMSCode == null)
                            return (false, null);

                        VeloJsonRequestConfirm jsonRequestConfirm = new VeloJsonRequestConfirm();
                        jsonRequestConfirm.token = SMSCode;

                        confirmResponse = PerformRequest<VeloJsonResponseConfirm>(
                          $"Confirmations/confirm/uuid/{response.uuid}", HttpMethod.Put, JsonConvert.SerializeObject(jsonRequestConfirm), null);

                        if (confirmResponse.CheckErrorExists(10012))
                            return (CheckFailed(confirmResponse.errors.First(e => e.error == 10012).error_description), null);

                        if (confirmResponse.CheckErrorExists(10000))
                            Message("Nieprawidłowy kod SMS");
                        else
                            codeProceeded = true;
                    }
                    break;
                case VeloBankJsonConfirmType.Mobile:
                    bool confirmed = false;
                    while (!confirmed)
                    {
                        if (!PromptOKCancel("Potwierdź operację na urządzeniu mobilnym"))
                            return (false, null);

                        confirmResponse = PerformRequest<VeloJsonResponseConfirm>(
                          $"Confirmations/info/uuid/{response.uuid}", HttpMethod.Get, null, null);

                        if (confirmResponse.StatusValue == VeloBankJsonConfirmationStatusType.Accepted)
                            confirmed = true;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return (true, confirmResponse);
        }

        private T PerformRequest<T>(string requestUri, HttpMethod method, string jsonContent, Action<Stream> useStream)
            where T : VeloJsonResponseBase
        {
            try
            {
                using (HttpRequestMessage message = CreateHttpRequestMessage(requestUri, method, jsonContent))
                {
                    using (HttpResponseMessage response = httpClient.SendAsync(message).Result)
                    {
                        using (HttpContent content = response.Content)
                        {
                            string responseStr = null;

                            if (useStream == null)
                            {
                                responseStr = content.ReadAsStringAsync().Result;

                                if (!CheckNotExpired(responseStr))
                                {
                                    NoteExpiredSession();
                                    return null;
                                }
                            }

                            useStream?.Invoke(content.ReadAsStreamAsync().Result);

                            return useStream == null ? JsonConvert.DeserializeObject<T>(responseStr) : null;
                        }
                    }
                }
            }
            catch (AggregateException aggregateException)
            {
                if (aggregateException.InnerExceptions.Count == 1)
                {
                    if (aggregateException.InnerExceptions[0] is HttpRequestException httpRequestException)
                    {
                        if (httpRequestException.InnerException != null)
                        {
                            if (httpRequestException.InnerException is WebException)
                            {
                                CheckFailed("Brak internetu");
                                return null;
                            }
                        }
                    }
                }

                throw;
            }
        }

        private HttpRequestMessage CreateHttpRequestMessage(string requestUri, HttpMethod method, string jsonContent = null)
        {
            HttpRequestMessage message = new HttpRequestMessage(method, requestUri);
            message.Headers.Add("X-Client-Hash", "b2-hash");

            if (defaultContextHash != null)
                message.Headers.Add("X-CONTEXT", defaultContextHash);

            //TODO adres automatycznie z httpClient
            Cookie hostCsrfCookie = Cookies.GetCookie("secure.velobank.pl", "__Host-csrf");
            if (hostCsrfCookie != null)
                message.Headers.Add("X-CSRF-TOKEN", hostCsrfCookie.Value);

            if (jsonContent != null)
                message.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            return message;
        }

        private string GetSMSCode(string text)
        {
            return PromptString(text, @"^\d{6}$");
        }

        private bool CheckNotExpired(string responseStr)
        {
            VeloJsonResponseBase response = JsonConvert.DeserializeObject<VeloJsonResponseBase>(responseStr);

            if (response?.CheckErrorExists(10011) ?? false)
                return CheckFailed("Utracono połączenie");

            return true;
        }

        public static DateTime GetDateFromReferenceNumber(string referenceNumber)
        {
            string dateString = referenceNumber.SubstringToEx("/");
            return new DateTime(Int32.Parse(dateString.Substring(0, 4)), Int32.Parse(dateString.Substring(4, 2)), Int32.Parse(dateString.Substring(6, 2)));
        }

        private static (FastTransferType? type, (string key, string hash) paData, string pblData) GetDataFromFastTransfer(string transferId)
        {
            string newTransferId = transferId.SubstringToEx("?");

            bool pbl = !newTransferId.Contains("/") && transferId.Length == 64;
            bool pa = newTransferId.Contains("/") && newTransferId.SubstringToEx("/").Length==32 && newTransferId.SubstringFromEx("/").Length==28;
            FastTransferType? type = null;
            if (pbl)
                type = FastTransferType.PayByLink;
            if (pa)
                type = FastTransferType.PA ;
            return (type, (newTransferId.SubstringToEx("/"), newTransferId.SubstringFromEx("/")), newTransferId);
        }
    }
}
