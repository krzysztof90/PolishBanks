using BankService.BankCountry;
using BankService.ConfirmText;
using BankService.LocalTools;
using BankService.SMSCodes;
using BankService.Tax.TaxCreditorIdentifiers;
using BankService.Tax.TaxPeriods;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Tools;
using Tools.Enums;
using ToolsWeb;
using static BankService.Bank_PL_Nest.NestJsonRequest;
using static BankService.Bank_PL_Nest.NestJsonResponse;

namespace BankService.Bank_PL_Nest
{
    [BankTypeAttribute(BankType.Nest)]
    public class Nest : BankPoland<NestAccountData, NestHistoryItem, NestHistoryFilter, NestJsonResponseDashboardConfig>
    {
        private long contextId;
        private string sessionToken;
        private string trustedDeviceId;
        private string trustedDeviceHash;

        //TODO check practically the time (start from 5, increase by 5 (dynamically, not manually)) + in other banks
        protected override int HeartbeatInterval => 270;
        protected override SMSCodeValidator SMSCodeValidator => new SMSCodeValidatorCypher(6);

        public override bool AllowAlternativeLoginMethod => false;

        public override bool TransferMandatoryRecipient => true;
        public override bool TransferMandatoryTitle => true;
        public override bool PrepaidTransferMandatoryRecipient => true;

        protected override string BaseAddress => "https://login.nestbank.pl/rest/v1/";

        protected override void CleanHttpClient()
        {
            contextId = 0;
            sessionToken = null;
            trustedDeviceId = null;
            trustedDeviceHash = null;
        }

        protected override bool LoginRequest(string login, string password, List<object> additionalAuthorization)
        {
            int avatarId = (int)additionalAuthorization[0];

            Cookie trustedDeviceCookie = DomainCookies.GetCookie("^Trusted-device-.", true);
            if (trustedDeviceCookie != null)
                trustedDeviceHash = trustedDeviceCookie.Value.Substring(2);

            (NestJsonResponseLogin response, bool requestProcessed) loginResponse = PerformRequest<NestJsonResponseLogin>(
                "auth/checkLogin", HttpMethod.Post,
                JsonConvert.SerializeObject(NestJsonRequestLogin.Create(login)),
                null, false);
            if (!loginResponse.requestProcessed)
                return false;

            (NestJsonResponsePassword response, bool requestProcessed) passwordResponse;

            switch (loginResponse.response.LoginProcessValue)
            {
                case NestJsonLoginProcess.Full:
                    {
                        passwordResponse = PerformRequest<NestJsonResponsePassword>(
                            "auth/loginByFullPassword", HttpMethod.Post,
                            JsonConvert.SerializeObject(NestJsonRequestFullPassword.Create("WWW", login, password, avatarId)),
                            null, false);
                        if (!passwordResponse.requestProcessed)
                            return false;
                    }
                    break;
                case NestJsonLoginProcess.Masked:
                    {
                        if (loginResponse.response.passwordKeys.Max() > password.Length)
                            return CheckFailed("Niepoprawne hasło");

                        Dictionary<int, string> maskedPassword = new Dictionary<int, string>();
                        foreach (int i in loginResponse.response.passwordKeys)
                            maskedPassword.Add(i, password[i - 1].ToString());

                        passwordResponse = PerformRequest<NestJsonResponsePassword>(
                            "auth/loginByPartialPassword", HttpMethod.Post,
                            JsonConvert.SerializeObject(NestJsonRequestMaskedPassword.Create("WWW", login, maskedPassword, avatarId)),
                            null, false);
                        if (!passwordResponse.requestProcessed)
                            return false;
                    }
                    break;
                case NestJsonLoginProcess.ResetPassword:
                    Message("Wymagana zmiana hasła");
                    return false;
                default: throw new NotImplementedException();
            }

            contextId = passwordResponse.response.userContexts.Single().id;

            (NestJsonResponseTrustedDeviceCheck response, bool requestProcessed) checkTrustedDeviceResponse = PerformRequest<NestJsonResponseTrustedDeviceCheck>(
                $"context/{contextId}/trustedDevices/check", HttpMethod.Get,
                null, null, false);
            if (!checkTrustedDeviceResponse.requestProcessed)
                return false;

            if (!checkTrustedDeviceResponse.response.trustedDevice)
                if (!AuthorizeBrowser(checkTrustedDeviceResponse.response))
                    return false;

            return true;
        }

        private bool AuthorizeBrowser(NestJsonResponseTrustedDeviceCheck checkTrustedDeviceResponse)
        {
            string signType = "dashboardSca";

            string prepareSignUrl = WebOperations.BuildUrlWithQuery(BaseAddress, $"context/{contextId}/{signType}/prepareSign",
                new List<(string key, string value)> { ("signedOperation", "confirmation") });
            (NestJsonResponsePrepareSignDashboardSca response, bool requestProcessed) prepareSignResponse = PerformRequest<NestJsonResponsePrepareSignDashboardSca>(
                prepareSignUrl, HttpMethod.Post,
                JsonConvert.SerializeObject(NestJsonRequestPrepareSignLogin.Create(true)),
                null, false);
            //TODO handle info about another confirmation pending - reply?
            if (!prepareSignResponse.requestProcessed)
                return false;

            if (!Confirm(prepareSignResponse.response, signType, "dashboardSca", true, new ConfirmTextAuthorize(null)))
                return false;

            if (!checkTrustedDeviceResponse.canSaveTrustedDevice)
            {
                //TODO before confirming
                Message("Została osiągnięta maksymalna liczba urządzeń zaufanych");
            }
            else if (PromptYesNo("Dodać urządzenie do zarejestrowanych?"))
            {
                (NestJsonResponseTrustedDeviceSave response, bool requestProcessed) saveTrustedDeviceResponse = PerformRequest<NestJsonResponseTrustedDeviceSave>(
                    $"context/{contextId}/trustedDevices/save", HttpMethod.Post,
                    //TODO
                    JsonConvert.SerializeObject(NestJsonRequestTrustedDeviceSave.Create("Netscape", "5.0 (Windows)", "pl", "Mozilla Firefox", true, true, "Firefox 129", 594, 1920, "Win32", "Gecko")),
                    null, false);
                if (!saveTrustedDeviceResponse.requestProcessed)
                    return false;

                trustedDeviceHash = saveTrustedDeviceResponse.response.hash;

                if (!trustedDeviceId.EndsWith("=="))
                    throw new NotImplementedException();

                Cookie trustedDeviceCookie = new Cookie("Trusted-device-" + trustedDeviceId.Substring(0, trustedDeviceId.Length - 2), "==" + trustedDeviceHash, "/", "login.nestbank.pl");
                Cookies.Add(trustedDeviceCookie);
                SaveCookie(trustedDeviceCookie);
            }

            return true;
        }

        protected override bool LogoutRequest()
        {
            (string response, bool requestProcessed) extendSessionResponse = PerformPlainRequest(
                $"auth/logout", HttpMethod.Get);
            if (!extendSessionResponse.requestProcessed)
                return false;

            return true;
        }

        protected override bool TryExtendSession()
        {
            //TODO if messageCode = "tokenSessionMismatch" then change for "Disconnected"
            (string response, bool requestProcessed) extendSessionResponse = PerformPlainRequest(
                $"context/{contextId}/auth/session/extend", HttpMethod.Get);
            if (!extendSessionResponse.requestProcessed)
                return false;

            return true;
        }

        protected override NestJsonResponseDashboardConfig GetAccountsDetails()
        {
            (NestJsonResponseDashboardConfig response, bool requestProcessed) dashboardResponse = PerformRequest<NestJsonResponseDashboardConfig>(
                $"context/{contextId}/dashboard/www/config", HttpMethod.Get,
                null, null, false);

            return dashboardResponse.response;
        }

        protected override List<NestAccountData> GetAccountsDataMainMain(NestJsonResponseDashboardConfig accountsDetails)
        {
            return accountsDetails.accounts.Select(a => new NestAccountData(a.name, a.nrb, a.currency, a.balance) { Id = a.id }).ToList();
        }

        public override bool MakeTransfer(string recipient, string address, string accountNumber, string title, double amount)
        {
            //( response, bool requestProcessed) banksResponse = PerformRequest<>(
            //    "dictionary/banks", HttpMethod.Post,
            //    JsonConvert.SerializeObject(),
            //    null);
            //if (!banksResponse.requestProcessed)
            //    return false;

            (NestJsonResponseTransferDate response, bool requestProcessed) transferDateResponse = PerformRequest<NestJsonResponseTransferDate>(
                $"context/{contextId}/order/getDefaultRealizationDate", HttpMethod.Get,
                null, null, false);
            if (!transferDateResponse.requestProcessed)
                return false;

            NestJsonRequestPrepareSignTransfer requestPrepareSignTransfer = new NestJsonRequestPrepareSignTransfer();
            requestPrepareSignTransfer.objectType = "domesticOrder";
            requestPrepareSignTransfer.accountId = SelectedAccountData.Id;
            requestPrepareSignTransfer.amount = amount;
            requestPrepareSignTransfer.cntrAccountNo = accountNumber.SimplifyAccountNumber();
            requestPrepareSignTransfer.cntrFullName = recipient;
            requestPrepareSignTransfer.cntrAddress = address;
            requestPrepareSignTransfer.realizationDate = transferDateResponse.response.date.Display("dd.MM.yyyy");
            requestPrepareSignTransfer.currency = SelectedAccountData.Currency;
            requestPrepareSignTransfer.standingType = "ONCE";
            requestPrepareSignTransfer.title = title;
            requestPrepareSignTransfer.orderType = "ELIXIR";
            requestPrepareSignTransfer.dataChangesLog = "W10=";

            return PrepareSignAndConfirm("order", "creation", requestPrepareSignTransfer,
                (NestJsonResponsePrepareSignOrder prepareSignResponse) =>
                    new ConfirmTextTransfer(amount, SelectedAccountData.Currency, prepareSignResponse.objects[0].cntrBankName, prepareSignResponse.objects[0].cntrAccountNo)
            );
        }

        public override bool MakeTaxTransfer(string taxType, string accountNumber, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier, string creditorName, string obligationId, double amount)
        {
            (string response, bool requestProcessed) taxFormTypesResponse = PerformPlainRequest("dictionary/taxFormCode", HttpMethod.Get);
            if (!taxFormTypesResponse.requestProcessed)
                return false;
            List<NestJsonResponseTaxFormType> responseTaxFormTypes = JsonConvert.DeserializeObject<List<NestJsonResponseTaxFormType>>(taxFormTypesResponse.response);

            NestJsonResponseTaxFormType selectedTax = responseTaxFormTypes.SingleOrDefault(t => t.name == taxType);
            if (selectedTax == null)
                return CheckFailed("Nie znaleziono podanego typu formularza");

            (NestJsonResponseTransferDate response, bool requestProcessed) transferDateResponse = PerformRequest<NestJsonResponseTransferDate>(
                $"context/{contextId}/order/getDefaultRealizationDate", HttpMethod.Get,
                null, null, false);
            if (!transferDateResponse.requestProcessed)
                return false;

            NestJsonRequestPrepareSignTaxTransfer requestPrepareSignTaxTransfer = new NestJsonRequestPrepareSignTaxTransfer();
            requestPrepareSignTaxTransfer.objectType = "usOrder";
            requestPrepareSignTaxTransfer.accountId = SelectedAccountData.Id;
            requestPrepareSignTaxTransfer.amount = amount;
            requestPrepareSignTaxTransfer.realizationDate = transferDateResponse.response.date.Display("dd.MM.yyyy");
            requestPrepareSignTaxTransfer.currency = SelectedAccountData.Currency;
            requestPrepareSignTaxTransfer.standingType = "ONCE";
            requestPrepareSignTaxTransfer.dataChangesLog = "W10=";
            requestPrepareSignTaxTransfer.taxFormCode = taxType;
            requestPrepareSignTaxTransfer.taxPaymentId = obligationId;

            if (selectedTax.irp)
            {
                requestPrepareSignTaxTransfer.cntrAccountNo = accountNumber.SimplifyAccountNumber();
                //TODO recipient to parameters
                requestPrepareSignTaxTransfer.cntrFullName = "Urząd Skarbowy";
                requestPrepareSignTaxTransfer.taxOfficeId = null;
            }
            else
            {
                string taxOfficeUrl = WebOperations.BuildUrlWithQuery(BaseAddress, "dictionary/taxOffice",
                    new List<(string key, string value)> { ("taxFormCode", taxType) });
                (string response, bool requestProcessed) taxOfficeResponse = PerformPlainRequest(taxOfficeUrl, HttpMethod.Get);
                List<NestJsonResponseTaxOffice> responseTaxOffices = JsonConvert.DeserializeObject<List<NestJsonResponseTaxOffice>>(taxOfficeResponse.response);

                NestJsonResponseTaxOffice taxOffice = PromptComboBox<NestJsonResponseTaxOffice>("Urząd", responseTaxOffices.Select(o => new SelectComboBoxItem<NestJsonResponseTaxOffice>(o.name, o)), true).data;
                if (taxOffice == null)
                    return false;

                requestPrepareSignTaxTransfer.cntrAccountNo = taxOffice.nrb;
                requestPrepareSignTaxTransfer.cntrFullName = taxOffice.name;
                requestPrepareSignTaxTransfer.taxOfficeId = taxOffice.id;
            }

            //if (!selectedTax.vatIndicator)

            requestPrepareSignTaxTransfer.taxIdentifierType = GetTaxCreditorIdentifierTypeId(creditorIdentifier);
            requestPrepareSignTaxTransfer.taxIdentifier = creditorIdentifier.GetId();

            if (selectedTax.periodMandatory)
            {
                (string unit, string unitShort, string number, string year) = GetTaxPeriodValue(period);

                requestPrepareSignTaxTransfer.taxPeriodUnit = unit;
                requestPrepareSignTaxTransfer.taxPeriodNo = number;
                requestPrepareSignTaxTransfer.taxPeriodYear = year;
            }

            return PrepareSignAndConfirm("order", "creation", requestPrepareSignTaxTransfer,
                (NestJsonResponsePrepareSignOrder prepareSignResponse) =>
                    new ConfirmTextTaxTransfer(amount, SelectedAccountData.Currency, requestPrepareSignTaxTransfer.taxOfficeId == null ? null : requestPrepareSignTaxTransfer.cntrFullName));
        }

        public static string GetTaxCreditorIdentifierTypeId(TaxCreditorIdentifier creditorIdentifier)
        {
            if (creditorIdentifier is TaxCreditorIdentifierNIP)
                return "NIP";
            else if (creditorIdentifier is TaxCreditorIdentifierIDCard)
                return "IDENTITY_CARD";
            else if (creditorIdentifier is TaxCreditorIdentifierPESEL)
                return "PESEL";
            else if (creditorIdentifier is TaxCreditorIdentifierREGON)
                return "REGON";
            else if (creditorIdentifier is TaxCreditorIdentifierPassport)
                return "PASSPORT";
            else if (creditorIdentifier is TaxCreditorIdentifierOther)
                return "ANOTHER";
            else
                throw new ArgumentException();
        }

        public static (string unit, string unitShort, string number, string year) GetTaxPeriodValue(TaxPeriod period)
        {
            if (period is TaxPeriodDay taxPeriodDay)
                return ("DAY", "J", $"{GetTaxPeriodNumberValue(taxPeriodDay.Day.Day)}{GetTaxPeriodNumberValue(taxPeriodDay.Day.Month)}", taxPeriodDay.Day.Year.ToString());
            else if (period is TaxPeriodHalfYear taxPeriodHalfYear)
                return ("SEMESTER", "P", GetTaxPeriodNumberValue(taxPeriodHalfYear.Half), taxPeriodHalfYear.Year.ToString());
            else if (period is TaxPeriodMonth taxPeriodMonth)
                return ("MONTH", "M", GetTaxPeriodNumberValue(taxPeriodMonth.Month), taxPeriodMonth.Year.ToString());
            else if (period is TaxPeriodMonthDecade taxPeriodMonthDecade)
                return ("DECADE", "D", $"{GetTaxPeriodNumberValue(taxPeriodMonthDecade.Decade)}{GetTaxPeriodNumberValue(taxPeriodMonthDecade.Month)}", taxPeriodMonthDecade.Year.ToString());
            else if (period is TaxPeriodQuarter taxPeriodQuarter)
                return ("QUARTER", "K", GetTaxPeriodNumberValue(taxPeriodQuarter.Quarter), taxPeriodQuarter.Year.ToString());
            else if (period is TaxPeriodYear taxPeriodYear)
                return ("YEAR", "R", String.Empty, taxPeriodYear.Year.ToString());
            else
                throw new ArgumentException();
        }

        private static string GetTaxPeriodNumberValue(int number)
        {
            return number.ToString("D2");
        }

        //TODO return (FastTransferType? type, string pblData)
        protected override string CleanFastTransferUrl(string transferId)
        {
            //https://login.nestbank.pl/login/blueMedia/ac2725e7-22b9-4789-9b8f-75d393ef3326/120

            string newTransferId = transferId
                .Replace("https://", String.Empty)

                .Replace("login.nestbank.pl/login/", String.Empty)
                .Replace("/120", String.Empty);

            (FastTransferType? type, string pblData) fastTransferData = GetDataFromFastTransfer(newTransferId);

            if (fastTransferData.type == null)
                return null;

            return newTransferId;
        }

        protected override bool LoginRequestForFastTransfer(string login, string password, List<object> additionalAuthorization, string transferId)
        {
            return PerformLogin(login, password, additionalAuthorization);
        }

        //TODO if number is "used" and payment not confirmed then return rejected/canceled url + in other banks
        protected override string MakeFastTransfer(string transferId)
        {
            (NestJsonResponseEpayments response, bool requestProcessed) epaymentsResponse = PerformRequest<NestJsonResponseEpayments>(
                $"context/{contextId}/epayments/{transferId}", HttpMethod.Get,
                null, null, false);
            if (!epaymentsResponse.requestProcessed)
                return null;

            NestJsonRequestPrepareSignPbl requestPrepareSignPbl = new NestJsonRequestPrepareSignPbl();
            requestPrepareSignPbl.objectType = "pblOrder";
            requestPrepareSignPbl.accountId = SelectedAccountData.Id;
            requestPrepareSignPbl.amount = epaymentsResponse.response.amount;
            requestPrepareSignPbl.cntrAccountNo = epaymentsResponse.response.receiverAccountNumber;
            requestPrepareSignPbl.cntrFullName = epaymentsResponse.response.receiverName;
            //TODO without it
            requestPrepareSignPbl.confirmationInfo = epaymentsResponse.response.email;
            requestPrepareSignPbl.currency = SelectedAccountData.Currency;
            requestPrepareSignPbl.orderType = "pbl";
            requestPrepareSignPbl.realizationDate = DateTime.Today.Display("dd.MM.yyyy");
            requestPrepareSignPbl.standingType = "ONCE";
            requestPrepareSignPbl.title = epaymentsResponse.response.title;
            requestPrepareSignPbl.transferUID = transferId.SubstringFromEx("/");

            if (!PrepareSignAndConfirm("order", "creation", requestPrepareSignPbl,
                (NestJsonResponsePrepareSignOrder prepareSignResponse) =>
                    new ConfirmTextFastTransfer(epaymentsResponse.response.amount, SelectedAccountData.Currency, epaymentsResponse.response.receiverName)))
                return null;

            return epaymentsResponse.response.backPage;
        }

        protected override bool MakePrepaidTransferMain(string recipient, string phoneNumber, double amount)
        {
            (NestJsonResponseOperator response, bool requestProcessed) operatorResponse = PerformRequest<NestJsonResponseOperator>(
                $"context/{contextId}/topup", HttpMethod.Get,
                null, null, false);
            if (!operatorResponse.requestProcessed)
                return false;

            NestJsonResponseOperatorOperator operatorItem = PromptComboBox<NestJsonResponseOperatorOperator>("Operator", operatorResponse.response.operators.Select(o => new SelectComboBoxItem<NestJsonResponseOperatorOperator>(o.displayName, o)), false).data;
            if (operatorItem == null)
                return false;

            switch (operatorItem.ValueTypeValue)
            {
                case NestJsonOperatorValueType.Range:
                    if (amount < operatorItem.minValue || amount > operatorItem.maxValue)
                        return CheckFailed($"Kwota powinna znajdować się w zakresie {operatorItem.minValue}-{operatorItem.maxValue}");
                    break;
                case NestJsonOperatorValueType.Constant:
                    if (!operatorItem.values.Contains(amount))
                        return CheckFailed($"Kwota powinna być jedną z {String.Join(", ", operatorItem.values.Select(v => v.Display(DecimalSeparator.Dot)))}");
                    break;
                default:
                    throw new NotImplementedException();
            }

            NestJsonRequestPrepareSignPrepaid requestPrepareSignPrepaid = new NestJsonRequestPrepareSignPrepaid();
            requestPrepareSignPrepaid.objectType = "mobileChargeOrder";
            requestPrepareSignPrepaid.accountId = SelectedAccountData.Id;
            requestPrepareSignPrepaid.amount = amount;
            requestPrepareSignPrepaid.clauseAccepted = true;
            requestPrepareSignPrepaid.cntrFullName = recipient;
            requestPrepareSignPrepaid.currency = SelectedAccountData.Currency;
            requestPrepareSignPrepaid.mobilePhone = phoneNumber;
            requestPrepareSignPrepaid.mobileOperator = operatorItem.name;
            requestPrepareSignPrepaid.shouldBlockFunds = true;
            requestPrepareSignPrepaid.standingType = "ONCE";

            return PrepareSignAndConfirm("order", "creation", requestPrepareSignPrepaid,
                (NestJsonResponsePrepareSignOrder prepareSignResponse) =>
                    new ConfirmTextPrepaidTransfer(amount, SelectedAccountData.Currency, operatorItem.displayName, phoneNumber));
        }

        protected override NestHistoryFilter CreateFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact)
        {
            return new NestHistoryFilter(direction, title, dateFrom, dateTo, amountExact) { OperationType = direction == OperationDirection.Execute ? NestFilterOperationType.Outgoing : NestFilterOperationType.Incoming, CounterLimit = 15 };
        }

        protected override List<NestHistoryItem> GetHistoryItems(NestHistoryFilter filter = null)
        {
            List<NestHistoryItem> result = new List<NestHistoryItem>();

            NestJsonRequestHistory jsonRequestHistory = new NestJsonRequestHistory();
            //TODO use from filter.CounterLimit + in other banks. Next pages if limit exceeded. Put default in CreateFilter
            jsonRequestHistory.pagination = new NestJsonRequestPagination() { pageNumber = 1, pageSize = filter.CounterLimit };
            //TODO do operationTypeValue + in other banks
            if (filter.OperationType != null)
                jsonRequestHistory.operationType = AttributeOperations.GetEnumAttribute((NestFilterOperationType)filter.OperationType, (FilterEnumParameterAttribute parameter) => parameter.Parameter, null);
            jsonRequestHistory.textSearch = filter.Title;
            jsonRequestHistory.DateFromValue = filter.DateFrom;
            jsonRequestHistory.DateToValue = filter.DateTo;
            jsonRequestHistory.AmountFromValue = filter.AmountFrom;
            jsonRequestHistory.AmountToValue = filter.AmountTo;

            (NestJsonResponseHistory response, bool requestProcessed) historyResponse = PerformRequest<NestJsonResponseHistory>(
                $"https://api2.nestbank.pl/account-query/api/public/v1/account/{SelectedAccountData.Id}/history", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestHistory),
                null, true);

            if (historyResponse.requestProcessed)
            {
                foreach (NestJsonResponseHistoryItem transaction in historyResponse.response.list)
                {
                    (NestJsonResponseHistoryDetails response, bool requestProcessed) transactionResponse = PerformRequest<NestJsonResponseHistoryDetails>(
                        $"https://api2.nestbank.pl/account-query/api/public/v1/account/{SelectedAccountData.Id}/operation/{transaction.operationNumber}", HttpMethod.Get,
                        null, null, true);

                    result.Add(new NestHistoryItem(transaction, transactionResponse.response));
                }
            }

            return result;
        }

        protected override bool GetDetailsFileMain(NestHistoryItem item, Func<ContentDispositionHeaderValue, FileStream> file)
        {
            string exportUrl = WebOperations.BuildUrlWithQuery(BaseAddress, $"context/{contextId}/account/{SelectedAccountData.Id}/history/{item.Id}/export",
                new List<(string key, string value)> { ("fileName", "Potwierdzenie zlecenia platniczego"), ("idType", "OPERATION_NUMBER") });
            PerformFileRequest(
                exportUrl, HttpMethod.Get,
                file);

            return true;
        }

        private bool PrepareSignAndConfirm(string signType, string signOperation, NestJsonRequestPrepareSignTransferBase requestPrepareSignTransferBase, Func<NestJsonResponsePrepareSignOrder, ConfirmTextBase> confirmTextMethod)
        {
            string prepareSignUrl = WebOperations.BuildUrlWithQuery(BaseAddress, $"context/{contextId}/{signType}/prepareSign",
                new List<(string key, string value)> { ("signedOperation", signOperation) });
            (NestJsonResponsePrepareSignOrder response, bool requestProcessed) prepareSignResponse = PerformRequest<NestJsonResponsePrepareSignOrder>(
                prepareSignUrl, HttpMethod.Post,
                JsonConvert.SerializeObject(new List<NestJsonRequestPrepareSignTransferBase>() { requestPrepareSignTransferBase }),
                null, false);
            if (!prepareSignResponse.requestProcessed)
                return false;

            return Confirm(prepareSignResponse.response, signType, signOperation, false, confirmTextMethod(prepareSignResponse.response));
        }

        //TODO add title to confirm text
        private bool Confirm<T>(NestJsonResponsePrepareSign<T> prepareSignResponse, string signType, string signOperation, bool oneObject, ConfirmTextBase confirmText) where T : NestJsonResponsePrepareSignObject
        {
            switch (prepareSignResponse.AuthorizationMethodValue)
            {
                case NestJsonAuthorizationMethod.Mobile:
                    {
                        (NestJsonResponseAuthorization response, bool requestProcessed) authorizationResponse = PerformRequest<NestJsonResponseAuthorization>(
                            $"context/{contextId}/authorization/getCrossChannelAuthorizationStatus", HttpMethod.Get,
                            null, null, false);
                        if (!authorizationResponse.requestProcessed)
                            return false;

                        switch (authorizationResponse.response.StatusValue)
                        {
                            case NestJsonAuthorizationStatus.New:
                                {
                                    switch (authorizationResponse.response.PushTypeValue)
                                    {
                                        case NestJsonAuthorizationPushType.Mobile:
                                            {
                                                if (!ConfirmMobile(confirmText))
                                                    return false;

                                                return Confirm(prepareSignResponse, signType, signOperation, oneObject, confirmText);
                                            }
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                            case NestJsonAuthorizationStatus.Success:
                            case NestJsonAuthorizationStatus.Verified:
                                return true;
                            case NestJsonAuthorizationStatus.Cancel:
                                {
                                    Message("Anulowano");
                                    return false;
                                }
                            case NestJsonAuthorizationStatus.Expired:
                                {
                                    //TODO + in other banks
                                    Message("Minął czas na autoryzację");
                                    return false;
                                }
                            default:
                                throw new NotImplementedException();
                        }
                    }
                case NestJsonAuthorizationMethod.SMS:
                    {
                        return SMSConfirm<bool, (NestJsonResponseSign response, bool requestProcessed)>(
                            (string SMSCode) =>
                            {
                                string signUrl = WebOperations.BuildUrlWithQuery(BaseAddress, $"context/{contextId}/{signType}/sign",
                                    new List<(string key, string value)> { ("signedOperation", signOperation) });
                                return PerformRequest<NestJsonResponseSign>(
                                    signUrl, HttpMethod.Post,
                                    JsonConvert.SerializeObject(NestJsonRequestSign<T>.Create("", SMSCode, prepareSignResponse.objects, prepareSignResponse.objects[0], oneObject)),
                                    //All because if additional other error then wrong
                                    (NestJsonResponseSign jsonResponseSign) => { return !(jsonResponseSign.level == "ERROR" && jsonResponseSign.problems.All(p => p.messageCode == "modules.authorization.sms.wrong.only")) && !CheckRequest(jsonResponseSign); },
                                    false);
                            },
                            ((NestJsonResponseSign response, bool requestProcessed) signResponse) =>
                            {
                                if (!signResponse.requestProcessed)
                                    return false;
                                if (signResponse.response.level == "ERROR" && signResponse.response.problems.All(p => p.messageCode == "modules.authorization.sms.wrong.only"))
                                    return null;
                                else
                                    return true;
                            },
                            ((NestJsonResponseSign response, bool requestProcessed) signResponse) => true,
                            null,
                            confirmText,
                            prepareSignResponse.authCountNumber);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private (T, bool) GetRequest<T>(
            HttpRequestMessage request,
            Func<string, T> responseStrAction) where T : class
        {
            using (HttpResponseMessage response = HttpOperations.GetResponse(Client, request))
            {
                KeyValuePair<string, IEnumerable<string>> sessionTokenHeader = response.Headers.FirstOrDefault(h => h.Key == "Session-Token");
                if (!sessionTokenHeader.Equals(default(KeyValuePair<string, IEnumerable<string>>)))
                    sessionToken = sessionTokenHeader.Value.Single();

                KeyValuePair<string, IEnumerable<string>> trustedDeviceIdHeader = response.Headers.FirstOrDefault(h => h.Key == "Trusted-Device-Id");
                if (!trustedDeviceIdHeader.Equals(default(KeyValuePair<string, IEnumerable<string>>)))
                {
                    trustedDeviceId = trustedDeviceIdHeader.Value.Single();
                    Cookies.Add(new Cookie("Trusted-device-id", trustedDeviceId, "/", "login.nestbank.pl"));
                }

                return ProcessResponse<T>(response,
                    (string responseStr) =>
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            NestJsonResponseBase jsonResponse = JsonConvert.DeserializeObject<NestJsonResponseBase>(responseStr);

                            if (!CheckRequest(jsonResponse))
                                return false;
                        }
                        if (response.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            Message(responseStr);
                            return false;
                        }
                        return true;
                    },
                    responseStrAction);
            }
        }

        //TODO check everywhere requestProcessed + in other banks
        private (T, bool) PerformRequest<T>(string requestUri, HttpMethod method,
            string jsonContent,
            Func<T, bool> invalidResponse,
            bool addContextId) where T : NestJsonResponseBase
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, jsonContent, addContextId))
            {
                (T response, bool requestProcessed) result = GetRequest<T>(request, (responseStr) => JsonConvert.DeserializeObject<T>(responseStr));

                if (result.requestProcessed)
                {
                    if (invalidResponse == null ? !CheckRequest(result.response) : invalidResponse.Invoke(result.response))
                        return (null, false);
                }

                return result;
            }
        }

        private (string, bool) PerformPlainRequest(string requestUri, HttpMethod method)
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, null, false))
                return GetRequest<string>(request, (responseStr) => responseStr);
        }

        private void PerformFileRequest(string requestUri, HttpMethod method,
            Func<ContentDispositionHeaderValue, FileStream> fileStream)
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, null, false))
                ProcessFileStream(request, fileStream);
        }

        private HttpRequestMessage CreateHttpRequestMessage(string requestUri, HttpMethod method, string jsonContent, bool addContextId)
        {
            List<(string name, string value)> headers = new List<(string name, string value)>();
            if (sessionToken != null)
                headers.Add(("Session-Token", sessionToken));
            if (trustedDeviceHash != null)
                headers.Add(("Trusted-device", trustedDeviceHash));
            if (addContextId)
                headers.Add(("Context-Id", contextId.ToString()));

            return HttpOperations.CreateHttpRequestMessageJson(method, requestUri, jsonContent, headers);
        }

        private bool CheckRequest(NestJsonResponseBase jsonResponse)
        {
            //TODO logout when error from other session active + in other banks
            if (jsonResponse == null)
            {
                Message("Błąd. Pusta odpowiedź");
                return false;
            }
            if (jsonResponse.level == "ERROR")
            {
                string messageContent = String.Join(Environment.NewLine, jsonResponse.problems.Where(p => p.level == "ERROR").Select(p => (!String.IsNullOrEmpty(p.description) ? p.description : p.messageCode)));
                Message(messageContent);
                return false;
            }
            if (jsonResponse.errors != null && jsonResponse.errors.Count != 0)
            {
                string messageContent = String.Join(Environment.NewLine, jsonResponse.errors.Select(e => e.userMessage ?? e.message));
                Message(messageContent);
                return false;
            }

            return true;
        }

        private static (FastTransferType? type, string pblData) GetDataFromFastTransfer(string transferId)
        {
            //TODO PA
            bool pbl = transferId?.SubstringFromEx("/").Length == 36;
            FastTransferType? type = null;
            if (pbl)
                type = FastTransferType.PayByLink;
            return (type, transferId);
        }
    }
}
