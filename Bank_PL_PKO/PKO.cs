using BankService.BankCountry;
using BankService.ConfirmText;
using BankService.LocalTools;
using BankService.SMSCodes;
using BankService.Tax.TaxCreditorIdentifiers;
using BankService.Tax.TaxPeriods;
using Fido2Authenticator;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Tools;
using Tools.Enums;
using ToolsWeb;
using static BankService.Bank_PL_PKO.PKOJsonRequest;
using static BankService.Bank_PL_PKO.PKOJsonResponse;

namespace BankService.Bank_PL_PKO
{
    [BankTypeAttribute(BankType.PKO)]
    public class PKO : BankPoland<PKOAccountData, PKOHistoryItem, PKOHistoryFilter, PKOJsonResponseAccountInit>
    {
        private string xSessionId;
        PKOJsonResponseLoginInitResponseDataAfterLoginAction loginActionAddedSecurity;
        private bool authenticated;

        protected override int HeartbeatInterval => 80;
        protected override SMSCodeValidator SMSCodeValidator => new SMSCodeValidatorCypher(6);

        public override bool AllowAlternativeLoginMethod => false;

        public override bool TransferMandatoryRecipient => true;
        public override bool TransferMandatoryTitle => true;
        public override bool PrepaidTransferMandatoryRecipient => false;

        protected override string BaseAddress => "https://www.ipko.pl";

        protected override void CleanHttpClient()
        {
            xSessionId = null;
            loginActionAddedSecurity = null;
            authenticated = false;
        }

        protected override bool LoginRequest(string login, string password, List<object> additionalAuthorization)
        {
            string uuid = Guid.NewGuid().ToString("D");

            string url = WebOperations.BuildUrlWithQuery("nudatasecurity/2.2/w/w-573441/init/js",
                new List<(string key, string value)> { ("q", Nsejsnfwmi(JsonConvert.SerializeObject(PKOJsonRequestNudatasecurity.Create(uuid)))) });
            PerformPlainRequest(url, HttpMethod.Get);

            (PKOJsonResponseLogin response, bool requestProcessed) loginResponse = PerformRequest<PKOJsonResponseLogin>("ipko3/login", HttpMethod.Post,
                JsonConvert.SerializeObject(PKOJsonRequestLogin.Create("submit", "login", login)));
            if (!loginResponse.requestProcessed)
                return false;

            //TODO image verification (it has additional datetime on it) and datetime from it - loginResponse.response.response.data.image.src

            (PKOJsonResponsePassword response, bool requestProcessed) passwordResponse = PerformRequest<PKOJsonResponsePassword>("ipko3/login", HttpMethod.Post,
                JsonConvert.SerializeObject(PKOJsonRequestPassword.Create(loginResponse.response.token, loginResponse.response.flow_id, "submit", "password", password, uuid)));
            if (!passwordResponse.requestProcessed)
                return false;

            if (passwordResponse.response.state_id == "one_time_password")
                throw new NotImplementedException();

            if (passwordResponse.response.state_id == "webauthn_device")
            {
                string authorizationData = Fido2Manager.GetAuthorizationData<Fido2Options, Fido2AuthorizationData>(passwordResponse.response.response.data.challenge_data, "https://www.ipko.pl",
                    (Fido2Options options, string authenticatorData, string clientDataJSON, string signature, string id) =>
                        new Fido2AuthorizationData
                        {
                            requestId = options.data.requestId,
                            assertion = new AuthorizationDataAssertion()
                            {
                                authenticatorData = authenticatorData,
                                clientDataJSON = clientDataJSON,
                                signature = signature,
                            }
                        });
                if (authorizationData == null)
                    return false;

                (PKOJsonResponseAuthorizeKey response, bool requestProcessed) authorizeKeyResponse = PerformRequest<PKOJsonResponseAuthorizeKey>("ipko3/login", HttpMethod.Post,
                    JsonConvert.SerializeObject(PKOJsonRequestAuthorizeKey.Create(passwordResponse.response.token, passwordResponse.response.flow_id, "submit", "webauthn_device", authorizationData)));
                if (!authorizeKeyResponse.requestProcessed)
                    return false;
            }

            (PKOJsonResponseLoginInit response, bool requestProcessed) initResponse = PerformRequest<PKOJsonResponseLoginInit>("ipko3/init", HttpMethod.Post,
                JsonConvert.SerializeObject(PKOJsonRequestLoginInit.Create()));
            if (!initResponse.requestProcessed)
                return false;

            loginActionAddedSecurity = initResponse.response.response.data.after_login_actions.FirstOrDefault(a => a.id == "ref_added_security_choice");

            if (initResponse.response.response.data.after_login_actions.Any(a => a.id == "ref_trusted_device_after_login"))
            {
                //TODO + in other banks
                return CheckFailed("Aby używać aplikacji, wyłącz śledzenie behawioralne");

                if (PromptYesNo("Dodać urządzenie do zarejestrowanych?"))
                {
                    //(PKOJsonResponseAddTrustedDevice response, bool requestProcessed) addTrustedDeviceResponse = PerformRequest<PKOJsonResponseAddTrustedDevice>("ipko3/trusted-device/add", HttpMethod.Post,
                    (PKOJsonResponseAddTrustedDevice response, bool requestProcessed) addTrustedDeviceResponse = PerformRequest<PKOJsonResponseAddTrustedDevice>("ipko3/trusted-device/add/after-login", HttpMethod.Post,
                    JsonConvert.SerializeObject(PKOJsonRequestAddTrustedDevice.Create("DESKTOP", "Windows", "Firefox", "Moj komputer")));
                    if (!addTrustedDeviceResponse.requestProcessed)
                        return false;

                    //(PKOJsonResponseAddTrustedDeviceSubmit response, bool requestProcessed) addTrustedDeviceSubmitResponse = PerformRequest<PKOJsonResponseAddTrustedDeviceSubmit>("ipko3/trusted-device/add", HttpMethod.Post,
                    (PKOJsonResponseAddTrustedDeviceSubmit response, bool requestProcessed) addTrustedDeviceSubmitResponse = PerformRequest<PKOJsonResponseAddTrustedDeviceSubmit>("ipko3/trusted-device/add/after-login", HttpMethod.Post,
                        JsonConvert.SerializeObject(PKOJsonRequestAddTrustedDeviceSubmit.Create(addTrustedDeviceResponse.response.token, addTrustedDeviceResponse.response.flow_id, "fill_form", "submit", (string)addTrustedDeviceResponse.response.response.fields.name.value)));
                    if (!addTrustedDeviceResponse.requestProcessed)
                        return false;

                    //if (!ConfirmSMSLocal(addTrustedDeviceResponse.response.response, "ipko3/trusted-device/add", new ConfirmTextAddDevice(null)))
                    if (!ConfirmSMSLocal(addTrustedDeviceSubmitResponse.response, "ipko3/trusted-device/add/after-login", new ConfirmTextAddDevice(null)))
                        return false;
                }
                else
                {
                    //(PKOJsonResponseAddedSecurity response, bool requestProcessed) addedSecurityResponse = PerformRequest<PKOJsonResponseAddedSecurity>("ipko3/after-login/added-security", HttpMethod.Post,
                    //    JsonConvert.SerializeObject(PKOJsonRequestAddedSecurity.Create()));
                    //if (!addedSecurityResponse.requestProcessed)
                    //    return false;

                    (PKOJsonResponseAuthVerify response, bool requestProcessed) authVerifyResponse = PerformRequest<PKOJsonResponseAuthVerify>("ipko3/auth-verify", HttpMethod.Post,
                        //JsonConvert.SerializeObject(PKOJsonRequestAuthVerify.Create(addedSecurityResponse.response.response.data.ref_skip_with_auth_verify.data.access_limiter_id, addedSecurityResponse.response.response.data.ref_skip_with_auth_verify.data.operation_type)));
                        JsonConvert.SerializeObject(PKOJsonRequestAuthVerify.Create(loginActionAddedSecurity.data.access_limiter_id, loginActionAddedSecurity.data.operation_type)));
                    if (!authVerifyResponse.requestProcessed)
                        return false;

                    if (!ConfirmSMSLocal(authVerifyResponse.response, "ipko3/auth-verify", new ConfirmTextAuthorize(null)))
                        return false;
                }
            }

            //TODO run Authenticate now?

            return true;
        }

        private string Nsejsnfwmi(string a)
        {
            return Regex.Replace(a, "[A-Za-z]", new MatchEvaluator(delegate (Match match)
            {
                char c = match.Value[0];
                return ((char)(c + (char.ToUpper(c) <= 'M' ? 13 : -13))).ToString();
            }));
        }

        private bool Authenticate()
        {
            if (!authenticated)
            {
                (PKOJsonResponseAuthVerify response, bool requestProcessed) authVerifyResponse = PerformRequest<PKOJsonResponseAuthVerify>("ipko3/auth-verify", HttpMethod.Post,
                    JsonConvert.SerializeObject(PKOJsonRequestAuthVerify.Create(loginActionAddedSecurity.data.access_limiter_id, loginActionAddedSecurity.data.operation_type)));
                if (!authVerifyResponse.requestProcessed)
                    return false;

                ConfirmTextBase confirmText = new ConfirmTextLogin();

                switch (authVerifyResponse.response.response.auth.AuthMethodValue)
                {
                    case PKOAuthMethod.SMS:
                        if (!ConfirmSMSLocal(authVerifyResponse.response, "ipko3/auth-verify", confirmText))
                            return false;
                        break;
                    case PKOAuthMethod.Mobile:
                        if (!ConfirmMobileLocal(authVerifyResponse.response, "ipko3/auth-verify", confirmText))
                            return false;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                authenticated = true;
            }

            return true;
        }

        protected override bool LogoutRequest()
        {
            (PKOJsonResponseLogout response, bool requestProcessed) logoutResponse = PerformRequest<PKOJsonResponseLogout>("ipko3/logout", HttpMethod.Post,
                JsonConvert.SerializeObject(PKOJsonRequestLogout.Create("CLIENT-LOGOUT")));

            return logoutResponse.requestProcessed;
        }

        protected override bool TryExtendSession()
        {
            (PKOJsonResponseRefresh response, bool requestProcessed) refreshResponse = PerformRequest<PKOJsonResponseRefresh>("ipko3/session/refresh", HttpMethod.Post,
                JsonConvert.SerializeObject(PKOJsonRequestRefresh.Create()));

            return refreshResponse.requestProcessed;
        }

        protected override PKOJsonResponseAccountInit GetAccountsDetails()
        {
            (PKOJsonResponseAccountInit response, bool requestProcessed) initResponse = PerformRequest<PKOJsonResponseAccountInit>("ipko3/init", HttpMethod.Post,
                JsonConvert.SerializeObject(PKOJsonRequestAccountsInit.Create()));
            if (!initResponse.requestProcessed)
                throw new NotImplementedException();

            return initResponse.response;
        }

        protected override List<PKOAccountData> GetAccountsDataMainMain(PKOJsonResponseAccountInit accountsDetails)
        {
            return accountsDetails.response.data.accounts.Select(a => new PKOAccountData(a.Value.name, a.Value.number.value, a.Value.currency, a.Value.Balance, a.Key)).ToList();
        }

        protected override bool MakeTransfer(string recipient, string address, string accountNumber, string title, double amount)
        {
            if (!Authenticate())
                return false;

            (PKOJsonResponseTransferInit response, bool requestProcessed) transferInitResponse = PerformRequest<PKOJsonResponseTransferInit>("ipko3/transactions/transfers/normal/shortcut", HttpMethod.Post,
                JsonConvert.SerializeObject(PKOJsonRequestTransferInit.Create(SelectedAccountData.AccountNumber)));
            if (!transferInitResponse.requestProcessed)
                return false;

            (PKOJsonResponseTransfer response, bool requestProcessed) transferResponse = PerformRequest<PKOJsonResponseTransfer>("ipko3/transactions/transfers/normal/shortcut", HttpMethod.Post,
                JsonConvert.SerializeObject(PKOJsonRequestTransfer.Create(transferInitResponse.response.token, transferInitResponse.response.flow_id, "fill_form", "submit", amount, SelectedAccountData.Currency, Today, PKOPaymentType.Elixir, accountNumber, recipient, address, title, SelectedAccountData.AccountId)));
            if (!transferResponse.requestProcessed)
                return false;

            return ConfirmMobileLocal(transferResponse.response, "ipko3/transactions/transfers/normal/shortcut", new ConfirmTextTransfer(amount, SelectedAccountData.Currency, transferResponse.response.response.data.recipient_bank, accountNumber.SimplifyAccountNumber()));
        }

        protected override bool MakeTaxTransfer(string taxType, string accountNumber, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier, string creditorName, string obligationId, double amount)
        {
            if (!Authenticate())
                return false;

            (PKOJsonResponseTaxTransferInit response, bool requestProcessed) taxTransferInitResponse = PerformRequest<PKOJsonResponseTaxTransferInit>("ipko3/transactions/transfers/tax/create", HttpMethod.Post,
                JsonConvert.SerializeObject(PKOJsonRequestTaxTransferInit.Create()));
            if (!taxTransferInitResponse.requestProcessed)
                return false;

            if (!taxTransferInitResponse.response.response.fields.symbol.widget.items.Any(s => s.id == taxType))
                return CheckFailed("Nie znaleziono podanego typu formularza");

            string tax_type_group = null;

            PKOJsonResponseTaxTransferInit lastResponseInit = taxTransferInitResponse.response;
            foreach (string taxTypeGroup in new List<string>() { "FREQUENT", "TICKETS", "OTHER" })
            {
                (PKOJsonResponseTaxTransferInit response, bool requestProcessed) taxTransferTypeResponse = PerformRequest<PKOJsonResponseTaxTransferInit>("ipko3/transactions/transfers/tax/create", HttpMethod.Post,
                    JsonConvert.SerializeObject(PKOJsonRequestTaxTransferType.Create(lastResponseInit.token, lastResponseInit.flow_id, "fill_form", "partial", taxTypeGroup)));
                if (!taxTransferTypeResponse.requestProcessed)
                    return false;

                lastResponseInit = taxTransferTypeResponse.response;

                if (taxTransferTypeResponse.response.response.fields.symbol.widget.items.Any(s => s.id == taxType))
                {
                    tax_type_group = taxTypeGroup;
                    break;
                }
            }

            string individual_tax_account = null;
            string recipient_account_type = null;
            if (taxTransferInitResponse.response.response.data.symbols_with_irp.Contains(taxType))
            {
                individual_tax_account = accountNumber;
                recipient_account_type = "INDIVIDUAL";
            }

            (PKOJsonResponseTaxTransferAccount response, bool requestProcessed) taxTransferAccountResponse = PerformRequest<PKOJsonResponseTaxTransferAccount>("ipko3/transactions/transfers/tax/create", HttpMethod.Post,
                JsonConvert.SerializeObject(PKOJsonRequestTaxTransferAccount.Create(lastResponseInit.token, lastResponseInit.flow_id, "fill_form", "partial", taxType, tax_type_group, individual_tax_account, recipient_account_type)));
            if (!taxTransferAccountResponse.requestProcessed)
                return false;

            string recipient_bank = null;
            string tax_office_account = null;
            if (taxTransferInitResponse.response.response.data.symbols_with_irp.Contains(taxType))
            {
                recipient_bank = taxTransferAccountResponse.response.response.data.recipient_name;

                //TODO there is option to search office instead + in other banks: when must/can search office?
            }
            else
            {
                tax_office_account = (string)taxTransferAccountResponse.response.response.fields.tax_office_account.value;
                if (tax_office_account == null)
                {
                    string city = null;
                    //TODO cannot search everything, default city is residence
                    (PKOJsonResponseTaxOfficeSearch response, bool requestProcessed) taxOfficeSearchResponse = PerformRequest<PKOJsonResponseTaxOfficeSearch>("ipko3/transactions/transfers/tax-office-search", HttpMethod.Post,
                        JsonConvert.SerializeObject(PKOJsonRequestTaxOfficeSearch.Create(taxType, city)));
                    if (!taxOfficeSearchResponse.requestProcessed)
                        return false;

                    PKOJsonResponseTaxOfficeSearchResponseDataItem taxOffice = PromptComboBox<PKOJsonResponseTaxOfficeSearchResponseDataItem>("Urząd", taxOfficeSearchResponse.response.response.data.items.Select(o => new SelectComboBoxItem<PKOJsonResponseTaxOfficeSearchResponseDataItem>($"{o.city}: {o.name}", o)), true).data;
                    if (taxOffice == null)
                        return false;

                    tax_office_account = taxOffice.account.number;
                }
            }

            PKOJsonRequestTaxTransferDataPayerIdentifier payerIdentifier = GetTaxCreditorIdentifier(creditorIdentifier);

            PKOJsonRequestTaxTransferDataPeriod dataPeriod = null;
            if (taxTransferInitResponse.response.response.data.symbols_with_period.Contains(taxType))
            {
                (string type, string number, string month, string year) = GetTaxPeriodValue(period);

                dataPeriod = new PKOJsonRequestTaxTransferDataPeriod() { type = type, number = number, month = month, year = year };
            }

            (PKOJsonResponseTaxTransfer response, bool requestProcessed) taxTransferResponse = PerformRequest<PKOJsonResponseTaxTransfer>("ipko3/transactions/transfers/tax/create", HttpMethod.Post,
                JsonConvert.SerializeObject(PKOJsonRequestTaxTransfer.Create(taxTransferAccountResponse.response.token, taxTransferAccountResponse.response.flow_id, "fill_form", "submit", taxType, tax_type_group, amount, payerIdentifier, Today, "ELIXIR", obligationId, dataPeriod, accountNumber, recipient_account_type, recipient_bank, tax_office_account, SelectedAccountData.AccountId)));
            if (!taxTransferResponse.requestProcessed)
                return false;

            return ConfirmMobileLocal(taxTransferResponse.response, "ipko3/transactions/transfers/tax/create", new ConfirmTextTaxTransfer(amount, SelectedAccountData.Currency, taxTransferResponse.response.response.data.recipient_name));
        }

        public static PKOJsonRequestTaxTransferDataPayerIdentifier GetTaxCreditorIdentifier(TaxCreditorIdentifier creditorIdentifier)
        {
            PKOJsonRequestTaxTransferDataPayerIdentifier payerIdentifier = new PKOJsonRequestTaxTransferDataPayerIdentifier();

            string id = creditorIdentifier.GetId();

            if (creditorIdentifier is TaxCreditorIdentifierNIP)
            {
                payerIdentifier.type = "NIP";
                payerIdentifier.nip = id;
            }
            else if (creditorIdentifier is TaxCreditorIdentifierIDCard)
            {
                payerIdentifier.type = "ID-CARD";
                payerIdentifier.id_card = id;
            }
            else if (creditorIdentifier is TaxCreditorIdentifierPESEL)
            {
                payerIdentifier.type = "PESEL";
                payerIdentifier.pesel = id;
            }
            else if (creditorIdentifier is TaxCreditorIdentifierREGON)
            {
                payerIdentifier.type = "REGON";
                payerIdentifier.regon = id;
            }
            else if (creditorIdentifier is TaxCreditorIdentifierPassport)
            {
                payerIdentifier.type = "PASSPORT";
                payerIdentifier.passport = id;
            }
            else if (creditorIdentifier is TaxCreditorIdentifierOther)
            {
                payerIdentifier.type = "OTHER-DOCUMENT";
                payerIdentifier.other_document = id;
            }
            else
                throw new ArgumentException();

            return payerIdentifier;
        }

        public static (string type, string number, string month, string year) GetTaxPeriodValue(TaxPeriod period)
        {
            if (period is TaxPeriodDay taxPeriodDay)
                return ("DAY", GetTaxPeriodNumberValue(taxPeriodDay.Day.Day), GetTaxPeriodNumberValue(taxPeriodDay.Day.Month), taxPeriodDay.Day.Year.ToString());
            else if (period is TaxPeriodHalfYear taxPeriodHalfYear)
                return ("SEMESTER", GetTaxPeriodNumberValue(taxPeriodHalfYear.Half), String.Empty, taxPeriodHalfYear.Year.ToString());
            else if (period is TaxPeriodMonth taxPeriodMonth)
                return ("MONTH", String.Empty, GetTaxPeriodNumberValue(taxPeriodMonth.Month), taxPeriodMonth.Year.ToString());
            else if (period is TaxPeriodMonthDecade taxPeriodMonthDecade)
                return ("DECADE", GetTaxPeriodNumberValue(taxPeriodMonthDecade.Decade), GetTaxPeriodNumberValue(taxPeriodMonthDecade.Month), taxPeriodMonthDecade.Year.ToString());
            else if (period is TaxPeriodQuarter taxPeriodQuarter)
                return ("QUARTER", GetTaxPeriodNumberValue(taxPeriodQuarter.Quarter), String.Empty, taxPeriodQuarter.Year.ToString());
            else if (period is TaxPeriodYear taxPeriodYear)
                return ("YEAR", String.Empty, String.Empty, taxPeriodYear.Year.ToString());
            else
                throw new ArgumentException();
        }

        private static string GetTaxPeriodNumberValue(int number)
        {
            return number.ToString("D2");
        }

        protected override string CleanFastTransferUrl(string transferId)
        {
            throw new NotImplementedException();
        }

        protected override bool LoginRequestForFastTransfer(string login, string password, List<object> additionalAuthorization, string transferId)
        {
            throw new NotImplementedException();
        }

        protected override string MakeFastTransfer(string transferId)
        {
            throw new NotImplementedException();
        }

        protected override bool MakePrepaidTransferMain(string recipient, string phoneNumber, double amount)
        {
            if (!Authenticate())
                return false;

            (PKOJsonResponsePrepaidInit response, bool requestProcessed) prepaidInitResponse = PerformRequest<PKOJsonResponsePrepaidInit>("ipko3/transactions/recharges/create", HttpMethod.Post,
                JsonConvert.SerializeObject(PKOJsonRequestPrepaidInit.Create()));
            if (!prepaidInitResponse.requestProcessed)
                return false;

            PKOJsonResponsePrepaidInitResponseDataOperator operatorItem = PromptComboBox<PKOJsonResponsePrepaidInitResponseDataOperator>("Operator", prepaidInitResponse.response.response.data.operators_info.Select(o => new SelectComboBoxItem<PKOJsonResponsePrepaidInitResponseDataOperator>(o.name, o)), false).data;
            if (operatorItem == null)
                return false;

            if ((operatorItem.amount_allowed.min_amount != null && amount < operatorItem.amount_allowed.min_amount)
                || (operatorItem.amount_allowed.max_amount != null && amount > operatorItem.amount_allowed.max_amount))
                return CheckFailed($"Kwota powinna znajdować się w zakresie {operatorItem.amount_allowed.min_amount}-{operatorItem.amount_allowed.max_amount}");
            if ((operatorItem.amount_allowed.amounts?.Count ?? 0) != 0 && !operatorItem.amount_allowed.amounts.Contains(amount))
                return CheckFailed($"Kwota powinna być jedną z {String.Join(", ", operatorItem.amount_allowed.amounts.Select(a => a.Display(DecimalSeparator.Dot)))}");

            (PKOJsonResponsePrepaid response, bool requestProcessed) prepaidResponse = PerformRequest<PKOJsonResponsePrepaid>("ipko3/transactions/recharges/create", HttpMethod.Post,
                JsonConvert.SerializeObject(PKOJsonRequestPrepaid.Create(prepaidInitResponse.response.token, prepaidInitResponse.response.flow_id, "fill_form", "submit", phoneNumber, operatorItem.id, amount, SelectedAccountData.Currency, SelectedAccountData.AccountId, false, true, true, true, true, true)));
            if (!prepaidResponse.requestProcessed)
                return false;

            return ConfirmMobileLocal(prepaidResponse.response, "ipko3/transactions/recharges/create", new ConfirmTextPrepaidTransfer(amount, SelectedAccountData.Currency, operatorItem.name, phoneNumber));
        }

        protected override PKOHistoryFilter CreateFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact)
        {
            return new PKOHistoryFilter(direction, title, dateFrom, dateTo, amountExact);
        }

        protected override List<PKOHistoryItem> GetHistoryItems(PKOHistoryFilter filter = null)
        {
            if (!Authenticate())
                return null;

            List<PKOHistoryItem> result = new List<PKOHistoryItem>();

            //TODO use filter.Direction in filter.OperationType if filter.OperationType not set + in other banks

            PKOFilterOperationType operationType = filter.OperationType ?? PKOFilterOperationType.All;
            string title = filter.Title;
            DateTime? dateFrom = filter.DateFrom;
            DateTime? dateTo = filter.DateTo;
            if (dateTo > Today || dateTo == null)
                dateTo = Today;
            if (dateFrom != null && dateTo != null && dateFrom > dateTo)
            {
                dateFrom = filter.DateTo;
                dateTo = filter.DateFrom;
                if (dateTo > Today)
                    dateTo = Today;
            }
            double? amountFrom = filter.AmountFrom;
            double? amountTo = filter.AmountTo;

            if ((dateFrom != null && dateFrom.Value.Date.AddYears(1) < Today)
                && !String.IsNullOrEmpty(title) && filter.SearchType == null)
            {
                Message("Dla wyszukiwania operacji starszych niż rok należy podać Sposób szukania");
                return null;
            }

            if ((dateFrom == null || dateFrom.Value.Date.AddYears(2) < dateTo)
                && (operationType != PKOFilterOperationType.All || !String.IsNullOrEmpty(title) || amountFrom != null || amountTo != null))
            {
                //TODO info on search panel instead message box
                Message("Dla okresu dat dłuższego niż 2 lata usuwany jest filtr na typ operacji, kwotę oraz tekst");
                operationType = PKOFilterOperationType.All;
                title = null;
                amountFrom = null;
                amountTo = null;
            }

            (PKOJsonResponseHistory response, bool requestProcessed) historyResponse = PerformRequest<PKOJsonResponseHistory>("ipko3/accounts/operations/completed/full", HttpMethod.Post,
                //TODO from 0.00 doesn't search 0.01, similar to 0.00 - searches 0.01
                JsonConvert.SerializeObject(PKOJsonRequestHistory.Create("SEARCH", filter.SearchType != null, false, false, SelectedAccountData.AccountId,
                    filter.SearchType != null ? AttributeOperations.GetEnumAttribute(filter.SearchType.Value, (FilterEnumParameterAttribute parameter) => parameter.Parameter, null) : null,
                    AttributeOperations.GetEnumAttribute(operationType, (FilterEnumParameterAttribute parameter) => parameter.Parameter, null),
                    dateFrom,
                    dateTo,
                    amountFrom,
                    amountTo,
                    title)));
            if (!historyResponse.requestProcessed)
                return null;

            result.AddRange(historyResponse.response.response.data.items.Select(i => new PKOHistoryItem(i)));

            string next = historyResponse.response.response.data.next?.type;

            while (next != null && (filter.CounterLimit == 0 || result.Count < filter.CounterLimit))
            {
                historyResponse = PerformRequest<PKOJsonResponseHistory>("ipko3/accounts/operations/completed/full", HttpMethod.Post,
                   JsonConvert.SerializeObject(PKOJsonRequestHistoryNext.Create(next)));
                if (!historyResponse.requestProcessed)
                    return null;

                result.AddRange(historyResponse.response.response.data.next_items.Select(i => new PKOHistoryItem(i)));

                next = historyResponse.response.response.data.next?.type;
            }

            //TODO + odrzucone i anulowane (rejected and cancelled)

            (PKOJsonResponseHistory response, bool requestProcessed) waitingDomesticResponse = PerformRequest<PKOJsonResponseHistory>("ipko3/accounts/operations/waiting/domestic", HttpMethod.Post,
                JsonConvert.SerializeObject(PKOJsonRequestHistoryWaiting.Create("SEARCH", SelectedAccountData.AccountId)));
            if (!waitingDomesticResponse.requestProcessed)
                return null;
            (PKOJsonResponseHistory response, bool requestProcessed) waitingForeignResponse = PerformRequest<PKOJsonResponseHistory>("ipko3/accounts/operations/waiting/foreign", HttpMethod.Post,
                JsonConvert.SerializeObject(PKOJsonRequestHistoryWaiting.Create("SEARCH", SelectedAccountData.AccountId)));
            if (!waitingForeignResponse.requestProcessed)
                return null;

            //TODO apply filter
            result.AddRange(waitingDomesticResponse.response.response.data.items.Select(i => new PKOHistoryItem(i)));
            result.AddRange(waitingForeignResponse.response.response.data.items.Select(i => new PKOHistoryItem(i)));

            return result;
        }

        protected override bool GetDetailsFileMain(PKOHistoryItem item, Func<string, FileStream> file)
        {
            if (item.Id == null)
                return CheckFailed("Operacja dozwolona tylko dla potwierdzonych przelewów");

            (PKOJsonResponseConfirmationInit response, bool requestProcessed) confirmationInitResponse = PerformRequest<PKOJsonResponseConfirmationInit>("ipko3/accounts/operation-confirmation", HttpMethod.Post,
                JsonConvert.SerializeObject(PKOJsonRequestConfirmationInit.Create("account-history", SelectedAccountData.AccountNumber, item.Id, "PL")));
            if (!confirmationInitResponse.requestProcessed)
                return false;

            (PKOJsonResponseConfirmation response, bool requestProcessed) confirmationResponse = PerformRequest<PKOJsonResponseConfirmation>("ipko3/accounts/operation-confirmation", HttpMethod.Post,
                JsonConvert.SerializeObject(PKOJsonRequestConfirmation.Create(confirmationInitResponse.response.token, confirmationInitResponse.response.flow_id, "fill_form", "submit", "pdf")));
            if (!confirmationResponse.requestProcessed)
                return false;

            byte[] data = Convert.FromBase64String(confirmationResponse.response.response.data.file.content.val);

            using (FileStream file2 = file(confirmationResponse.response.response.data.file.name))
            {
                file2.Write(data, 0, data.Length);
            }

            return true;
        }

        private bool ConfirmSMSLocal<T>(PKOJsonResponseFlowAuthBase<T> flowResponse, string url, ConfirmTextBase confirmText) where T : PKOJsonResponseResponseAuthBase
        {
            return SMSConfirm<bool, (PKOJsonResponseSubmit response, bool requestProcessed)>(
                (string SMSCode) =>
                {
                    return PerformRequest<PKOJsonResponseSubmit>(url, HttpMethod.Post,
                        JsonConvert.SerializeObject(PKOJsonRequestAuthVerifySubmit.Create(flowResponse.token, flowResponse.flow_id, "confirmation", "submit", SMSCode)));
                },
                ((PKOJsonResponseSubmit response, bool requestProcessed) smsConfirmResponse) =>
                {
                    if (!smsConfirmResponse.requestProcessed)
                        return false;
                    if (smsConfirmResponse.response.state_id == "confirmation")
                        return null;
                    else
                        //smsConfirmResponse.response.state_id = "END" && smsConfirmResponse.response.finished
                        return true;
                },
                ((PKOJsonResponseSubmit response, bool requestProcessed) smsConfirmResponse) => true,
                ((PKOJsonResponseSubmit response, bool requestProcessed) smsConfirmResponse) => ConfirmSMSLocal(smsConfirmResponse.response, url, confirmText),
                confirmText,
                Int32.Parse(flowResponse.response.auth.tan_index)
                );
        }

        private bool ConfirmMobileLocal<T>(PKOJsonResponseFlowAuthBase<T> flowResponse, string url, ConfirmTextBase confirmText) where T : PKOJsonResponseResponseAuthBase
        {
            return MobileConfirm<bool, PKOJsonResponseMobileStatus>(
                () =>
                {
                    //TODO server responds long. Only after mobile confirm/cancel or after about half a minute. Add cancelling thread
                    (PKOJsonResponseMobileStatus response, bool requestProcessed) mobileStatusResponse = PerformRequestBase<PKOJsonResponseMobileStatus>("api/get_lp_hook_status", HttpMethod.Post,
                        JsonConvert.SerializeObject(PKOJsonRequestMobileStatus.Create(flowResponse.response.auth.session_id, $"auth_id:{flowResponse.response.auth.authorization_id}", "AUTHORIZATION_STATUS", "PENDING")));
                    return mobileStatusResponse.response;
                },
                (PKOJsonResponseMobileStatus mobileStatusResponse) =>
                {
                    if (mobileStatusResponse.StatusValue == PKOMobileStatusStatus.Changed
                        && mobileStatusResponse.ValueValue == PKOMobileStatusValue.Ready)
                    {
                        (PKOJsonResponseSubmit response, bool requestProcessed) mobileSubmitResponse = PerformRequest<PKOJsonResponseSubmit>(url, HttpMethod.Post,
                            JsonConvert.SerializeObject(PKOJsonRequestSubmitMobile.Create(flowResponse.token, flowResponse.flow_id, flowResponse.state_id, "submit")));

                        if (mobileSubmitResponse.response.response.auth?.StateValue == PKOAuthState.Cancelled)
                            return false;
                        return true;
                    }

                    if (mobileStatusResponse.StatusValue == PKOMobileStatusStatus.Error
                        && mobileStatusResponse.ValueValue == PKOMobileStatusValue.Error)
                        return false;

                    if (!(mobileStatusResponse.StatusValue == PKOMobileStatusStatus.NotChanged
                        && mobileStatusResponse.ValueValue == PKOMobileStatusValue.Pending))
                        throw new NotImplementedException();

                    return null;
                },
                (PKOJsonResponseMobileStatus mobileStatusResponse) => true,
                null,
                confirmText
                );
        }


        private (T, bool) GetRequest<T>(HttpRequestMessage request, Func<string, T> responseStrAction) where T : class
        {
            using (HttpResponseMessage response = HttpOperations.GetResponse(Client, request))
            {
                KeyValuePair<string, IEnumerable<string>> sessionIdHeader = response.Headers.FirstOrDefault(h => h.Key == "X-Session-Id");
                if (!sessionIdHeader.Equals(default(KeyValuePair<string, IEnumerable<string>>)))
                    xSessionId = sessionIdHeader.Value.Single();

                using (HttpContent content = response.Content)
                {
                    string responseStr = content.ReadAsStringAsync().Result;
                    return (responseStrAction(responseStr), true);
                }
            }
        }

        private (T, bool) PerformRequest<T>(string requestUri, HttpMethod method, string jsonContent) where T : PKOJsonResponseBaseBase
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, jsonContent))
            {
                (T response, bool requestProcessed) result = GetRequest<T>(request, (responseStr) => JsonConvert.DeserializeObject<T>(responseStr));

                if (result.requestProcessed)
                {
                    if (result.response.HasErrors)
                    {
                        string messageContent = String.Join(Environment.NewLine, result.response.Errors);
                        Message(messageContent);
                        return (null, false);
                    }
                }

                return result;
            }
        }

        private (T, bool) PerformRequestBase<T>(string requestUri, HttpMethod method, string jsonContent) where T : PKOJsonResponseBaseBaseBase
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, jsonContent))
            {
                return GetRequest<T>(request, (responseStr) => JsonConvert.DeserializeObject<T>(responseStr));
            }
        }

        private (string, bool) PerformPlainRequest(string requestUri, HttpMethod method)
        {
            using (HttpRequestMessage request = CreateHttpPlainRequestMessage(requestUri, method))
            {
                return GetRequest<string>(request, (responseStr) => responseStr);
            }
        }

        private HttpRequestMessage CreateHttpPlainRequestMessage(string requestUri, HttpMethod method)
        {
            return HttpOperations.CreateHttpRequestMessage(method, requestUri);
        }

        private HttpRequestMessage CreateHttpRequestMessage(string requestUri, HttpMethod method, string jsonContent)
        {
            List<(string name, string value)> headers = new List<(string name, string value)>();

            if (xSessionId != null)
                headers.Add(("x-session-id", xSessionId));

            return HttpOperations.CreateHttpRequestMessageJson(method, requestUri, jsonContent, headers);
        }
    }
}
