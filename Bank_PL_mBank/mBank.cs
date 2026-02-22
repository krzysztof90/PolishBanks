using BankService.BankCountry;
using BankService.ConfirmText;
using BankService.Fingerprints;
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
using static BankService.Bank_PL_mBank.mBankJsonRequest;
using static BankService.Bank_PL_mBank.mBankJsonResponse;

namespace BankService.Bank_PL_mBank
{
    [BankTypeAttribute(BankType.mBank)]
    public class mBank : BankPoland<mBankAccountData, mBankHistoryItem, mBankHistoryFilter, mBankJsonResponseProducts>
    {
        private string Token;

        protected override int HeartbeatInterval => 60;
        protected override SMSCodeValidator SMSCodeValidator => new SMSCodeValidatorCypher(8);

        public override bool AllowAlternativeLoginMethod => false;

        public override bool TransferMandatoryRecipient => true;
        public override bool TransferMandatoryTitle => false;
        public override bool PrepaidTransferMandatoryRecipient => false;

        protected override string BaseAddress => "https://online.mbank.pl";

        protected override void CleanHttpClient()
        {
            Token = null;
        }

        protected override bool LoginRequest(string login, string password, List<object> additionalAuthorization)
        {
            //TODO new api

            string dfp = FingerprintManager.CreateBase64FromFingerprint(new Fingerprint() { Host = new HostInfo() { Platform = " " }, Locale = new LocaleInfo { Lang = " " }, WebGLExtensions = new List<string>() { "   " }, WebGL = new WebGLInfo() });

            mBankJsonRequestLogin jsonRequestLogin = new mBankJsonRequestLogin();
            jsonRequestLogin.UserName = login;
            jsonRequestLogin.Password = password;
            jsonRequestLogin.Scenario = "Default";
            jsonRequestLogin.UWAdditionalParams = new mBankJsonRequestAdditionalOptionsDummy();
            jsonRequestLogin.DfpData = new mBankJsonRequestLoginDfpData()
            {
                dfp = dfp,
            };

            (mBankJsonResponseLogin response, bool requestProcessed) loginResponse = PerformRequest<mBankJsonResponseLogin>(
                "pl/LoginMain/Account/JsonLogin", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestLogin));
            if (!loginResponse.response.successful)
                return CheckFailed($"{loginResponse.response.errorMessageTitle}: {loginResponse.response.errorMessageBody}");

            (mBankJsonResponseSetupData response, bool requestProcessed) setupDataResponse = PerformRequest<mBankJsonResponseSetupData>(
                "pl/setup/data", HttpMethod.Get,
                null);
            if (!setupDataResponse.requestProcessed)
                return false;

            Token = setupDataResponse.response.antiForgeryToken;

            if (loginResponse.response.redirectUrl == "/authorization")
                if (!AuthorizeBrowser(dfp))
                    return false;

            return true;
        }

        //TODO check if proper info is being displayed if broken session after long time + in other banks
        private bool AuthorizeBrowser(string dfp)
        {
            (mBankJsonResponseAuthorizationData response, bool requestProcessed) authorizationDataResponse = PerformRequest<mBankJsonResponseAuthorizationData>(
                "pl/Sca/GetScaAuthorizationData", HttpMethod.Post,
                null);
            if (!authorizationDataResponse.requestProcessed)
                return false;

            if (!authorizationDataResponse.response.TrustedDeviceAddingAllowed)
                //TODO do the same what in else.else
                //TODO show only after the question "Dodać urządzenie do zarejestrowanych?" / order to option
                Message("Została osiągnięta maksymalna liczba urządzeń zaufanych");
            else
            {
                if (PromptYesNo("Dodać urządzenie do zarejestrowanych?"))
                {
                    string trustedDevicesCheckUrl = WebOperations.BuildUrlWithQuery(BaseAddress, "api/sca/TrustedDevices/IsPossibleToAddNextDeviceAndCheckDeviceName",
                        new List<(string key, string value)> { ("defaultDeviceName", Constants.AppDeviceName) });
                    (mBankJsonResponseTrustedDevicesCheck response, bool requestProcessed) trustedDevicesCheckResponse = PerformRequest<mBankJsonResponseTrustedDevicesCheck>(
                        trustedDevicesCheckUrl, HttpMethod.Get,
                        null);
                    if (!trustedDevicesCheckResponse.requestProcessed)
                        return false;

                    if (!trustedDevicesCheckResponse.response.isValid)
                        //TODO
                        throw new NotImplementedException();

                    string trustedDevicesAddUrl = WebOperations.BuildUrlWithQuery(BaseAddress, "api/sca/TrustedDevices",
                        new List<(string key, string value)> { ("deviceName", trustedDevicesCheckResponse.response.deviceName) });
                    (mBankJsonResponseTrustedDevicesAdd response, bool requestProcessed) trustedDevicesAddResponse = PerformRequest<mBankJsonResponseTrustedDevicesAdd>(
                        trustedDevicesAddUrl, HttpMethod.Get,
                        null);
                    if (!trustedDevicesAddResponse.requestProcessed)
                        return false;

                    if (!trustedDevicesAddResponse.response.isValid)
                        throw new NotImplementedException();

                    mBankJsonRequestInitializeTrustedDevice jsonRequestInitializeTrustedDevice = new mBankJsonRequestInitializeTrustedDevice();
                    jsonRequestInitializeTrustedDevice.moduleId = "ScaTrustedDevice";
                    jsonRequestInitializeTrustedDevice.moduleData = new mBankJsonRequestInitializeTrustedDeviceModuleData();
                    jsonRequestInitializeTrustedDevice.moduleData.BrowserName = Constants.AppBrowserName;
                    jsonRequestInitializeTrustedDevice.moduleData.BrowserVersion = "1";
                    jsonRequestInitializeTrustedDevice.moduleData.OsName = "Windows";
                    jsonRequestInitializeTrustedDevice.moduleData.ScaAuthorizationId = authorizationDataResponse.response.ScaAuthorizationId;
                    jsonRequestInitializeTrustedDevice.moduleData.DeviceName = trustedDevicesCheckResponse.response.deviceName;
                    jsonRequestInitializeTrustedDevice.moduleData.DfpData = dfp;
                    jsonRequestInitializeTrustedDevice.moduleData.IsTheOnlyDeviceUser = true;

                    mBankJsonRequestFinalizeAuthorizationTrustedDevice jsonRequestFinalizeAuthorizationTrustedDevice = new mBankJsonRequestFinalizeAuthorizationTrustedDevice();
                    jsonRequestFinalizeAuthorizationTrustedDevice.scaAuthorizationId = authorizationDataResponse.response.ScaAuthorizationId;
                    jsonRequestFinalizeAuthorizationTrustedDevice.deviceName = trustedDevicesCheckResponse.response.deviceName;
                    jsonRequestFinalizeAuthorizationTrustedDevice.currentDfp = dfp;

                    bool confirmed = ConfirmAuthorization(jsonRequestInitializeTrustedDevice, "pl/Sca/FinalizeTrustedDeviceAuthorization", jsonRequestFinalizeAuthorizationTrustedDevice);

                    if (confirmed)
                        SaveCookie(DomainCookies.GetCookie("mBank8"));

                    return confirmed;
                }
                else
                {
                    mBankJsonRequestInitialize jsonRequestInitialize = new mBankJsonRequestInitialize();
                    jsonRequestInitialize.moduleId = "ScaHostless";
                    jsonRequestInitialize.moduleData = new mBankJsonRequestInitializeModuleData();
                    jsonRequestInitialize.moduleData.BrowserName = Constants.AppBrowserName;
                    //jsonRequestAuthorization.moduleData.BrowserVersion = "1";
                    //jsonRequestAuthorization.moduleData.OsName = "Windows";
                    jsonRequestInitialize.moduleData.ScaAuthorizationId = authorizationDataResponse.response.ScaAuthorizationId;

                    mBankJsonRequestFinalizeAuthorization jsonRequestFinalizeAuthorization = new mBankJsonRequestFinalizeAuthorization();
                    jsonRequestFinalizeAuthorization.scaAuthorizationId = authorizationDataResponse.response.ScaAuthorizationId;

                    return ConfirmAuthorization(jsonRequestInitialize, "pl/Sca/FinalizeAuthorization", jsonRequestFinalizeAuthorization);
                }
            }

            return true;
        }

        protected override bool LogoutRequest()
        {
            //TODO check if it is possible (should be not) to get account data after calling this + in other banks
            (string, bool) logoutResponse = PerformPlainRequest("LoginMain/Account/Logout", HttpMethod.Get, null);

            return true;
        }

        protected override bool TryExtendSession()
        {
            (mBankJsonResponseExtendSession response, bool requestProcessed) extendSessionResponse = PerformRequest<mBankJsonResponseExtendSession>(
                "pl/LoginMain/Account/JsonSessionKeepAlive", HttpMethod.Post,
                null);

            return extendSessionResponse.response.success;
        }

        protected override mBankJsonResponseProducts GetAccountsDetails()
        {
            //TODO should be called only once
            (mBankJsonResponseUserSettings response, bool requestProcessed) userSettingsResponse = PerformRequest<mBankJsonResponseUserSettings>(
                "pl/MyDesktop/DynamicDashboard/GetUserSettings", HttpMethod.Post,
                null);

            mBankJsonRequestProducts jsonRequestProducts = new mBankJsonRequestProducts();
            jsonRequestProducts.productsIds = userSettingsResponse.response.products.Select(p => new mBankJsonRequestProductsProduct()
            { id = p.id, order = (int)p.order, productType = p.productType }).ToList();

            (mBankJsonResponseProducts response, bool requestProcessed) productsResponse = PerformRequest<mBankJsonResponseProducts>(
                "pl/MyDesktop/DynamicDashboard/GetProductsFromUserSettings", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestProducts));

            return productsResponse.response;
        }

        protected override List<mBankAccountData> GetAccountsDataMainMain(mBankJsonResponseProducts accountsDetails)
        {
            //TODO p.balance.value ?
            return accountsDetails.products.Select(p => new mBankAccountData(p.name, p.number, p.currency, p.AvailableBalance)).ToList();
        }

        public override bool MakeTransfer(string recipient, string address, string accountNumber, string title, double amount)
        {
            if (title?.Length > 140)
                return CheckFailed("Tytuł nie może zawierać więcej niż 140 znaków");
            //TODO checking to AddressTools, inside can be removing spaces
            if (address.Length > 70)
                return CheckFailed("Adres nie może zawierać więcej niż 70 znaków");

            List<string> addresses = AddressTools.SplitAddress(address, 2, 35);

            (mBankJsonResponseDomestic response, bool requestProcessed) domesticResponse = PerformRequest<mBankJsonResponseDomestic>(
                "api/payments/domesticeea", HttpMethod.Get,
                null);

            //TODO almost like in browser but domesticResponse.response.transferData is null. Althought it's ok

            mBankJsonResponseDomesticAccount account = domesticResponse.response.availableAccounts.Single(a => a.name == SelectedAccountData.Name);

            mBankJsonRequestTransferCheck jsonRequestTransferCheck = new mBankJsonRequestTransferCheck();
            jsonRequestTransferCheck.FromAccount = account.number;
            jsonRequestTransferCheck.ToAccount = accountNumber;
            jsonRequestTransferCheck.Date = DateTime.Now;
            jsonRequestTransferCheck.Amount = new mBankJsonRequestAmountCapital() { Currency = account.currency, Value = amount };
            jsonRequestTransferCheck.RedirectionSource = "none";
            jsonRequestTransferCheck.TransferType = "elixir";
            jsonRequestTransferCheck.checkDataId = Guid.NewGuid().ToString("D");

            (mBankJsonResponseTransferCheck response, bool requestProcessed) transferCheckResponse = PerformRequest<mBankJsonResponseTransferCheck>(
                "api/payments/domesticeea/checkData", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestTransferCheck));
            if (!transferCheckResponse.requestProcessed)
                return false;

            mBankJsonRequestInitPrepareTransfer jsonRequestInitPrepare = new mBankJsonRequestInitPrepareTransfer();
            jsonRequestInitPrepare.Data = new mBankJsonRequestInitPrepareTransferData()
            {
                toAccount = accountNumber,
                amount = new mBankJsonRequestAmount() { currency = account.currency, value = amount },
                date = DateTime.Now,
                fromAccount = account.number,
                cardNumber = null,
                //TODO displayName or coownerId from pl/setup/data, the same in TaxTransfer
                coowner = account.coowners.Single(/*c=>c.displayName ==*/).coownerId,
                receiver = new mBankJsonRequestInitPrepareTransferDataReceiver() { name = recipient, street = addresses[0], cityAndPostalCode = addresses[1], nip = null },
                title = title,
                transferMode = "elixir",
                //paymentSource = domesticResponse.response.transferData.paymentSource,
                additionalOptions = new mBankJsonRequestAdditionalOptionsDummy(),
                perfToken = transferCheckResponse.response.perfToken,
            };
            jsonRequestInitPrepare.Method = "POST";
            jsonRequestInitPrepare.TwoFactor = false;
            jsonRequestInitPrepare.Url = "payments/domesticEea";

            return ConfirmTransfer(jsonRequestInitPrepare, new ConfirmTextTransfer(amount, account.currency, transferCheckResponse.response.bankDetails.name, accountNumber));
        }

        public override bool MakeTaxTransfer(string taxType, string accountNumber, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier, string creditorName, string obligationId, double amount)
        {
            (string response, bool requestProcessed) taxFormTypesResponse = PerformPlainRequest("api/paymentsBusiness/getTaxFormTypes", HttpMethod.Post, null);
            if (!taxFormTypesResponse.requestProcessed)
                return false;
            List<mBankJsonResponseTaxFormType> responseTaxFormTypes = JsonConvert.DeserializeObject<List<mBankJsonResponseTaxFormType>>(taxFormTypesResponse.response);

            mBankJsonResponseTaxFormType selectedTax = responseTaxFormTypes.SingleOrDefault(s => s.formName == taxType);
            if (selectedTax == null)
                return CheckFailed("Nie znaleziono podanego typu formularza");

            mBankJsonRequestTaxTransferPrepare jsonRequestTaxTransferPrepare = new mBankJsonRequestTaxTransferPrepare();

            (mBankJsonResponseTaxTransferPrepare response, bool requestProcessed) taxTransferPrepareResponse = PerformRequest<mBankJsonResponseTaxTransferPrepare>(
                "api/paymentsBusiness/prepareOneUs", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestTaxTransferPrepare));

            mBankJsonResponseTaxTransferPrepareFormDataDefaultDataAccount account = taxTransferPrepareResponse.response.formData.defaultData.availableFromAccounts.Single(a => a.displayName == $"{SelectedAccountData.Name} -  {SelectedAccountData.AccountNumber.SimplifyAccountNumber().Substring(0, 4)} ... {SelectedAccountData.AccountNumber.SimplifyAccountNumber().SubstringFromEx(-4)}");

            mBankJsonRequestInitPrepareTaxTransfer jsonRequestInitPrepare = new mBankJsonRequestInitPrepareTaxTransfer();
            jsonRequestInitPrepare.Data = new mBankJsonRequestInitPrepareTaxTransferData()
            {
                usform = new mBankJsonRequestInitPrepareTaxTransferDataUsform()
                {
                    fromAccount = account.number,
                    sender = account.coowners.Single().coownerId,
                    //accountParams = account.accountParams,
                    perfToken = taxTransferPrepareResponse.response.formData.perfToken,
                    date = DateTime.Today.Display("yyyy-MM-dd"),
                    formType = "us",
                    taxAuthority = new mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthority()
                    {
                        authoritySymbol = new mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthoritySymbol()
                        {
                            symbol = taxType,
                            toAnother = false
                        },
                        authorityCity = new mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityCity(),
                        authorityName = new mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityName(),
                        authorityNameCustom = new mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityNameCustom() { },
                        authorityAccountNumberCustom = new mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityAccountNumberCustom() { },
                    },
                    amount = amount,
                    currency = account.currency,
                    defaultData = null,
                    commitmentId = obligationId,
                    additionalOptions = new mBankJsonRequestAdditionalOptions(),
                }
            };
            jsonRequestInitPrepare.Method = "POST";
            jsonRequestInitPrepare.TwoFactor = false;
            jsonRequestInitPrepare.Url = "paymentsBusiness/intermediateOneUs";

            string taxOfficeName = null;

            switch (selectedTax.accType)
            {
                case "13":
                    jsonRequestInitPrepare.Data.usform.taxAuthority.authorityName.name = accountNumber;
                    break;
                case "7":
                case "8":
                case "9":
                case "10":
                case "11":
                case "12":
                case "14":
                    //TODO type AKC in other banks automaticaly choses office and city, here not
                    (string response, bool requestProcessed) taxCitiesResponse = PerformPlainRequest("api/paymentsBusiness/getTaxCities", HttpMethod.Post, null);
                    if (!taxCitiesResponse.requestProcessed)
                        return false;

                    List<string> responseTaxCities = JsonConvert.DeserializeObject<List<string>>(taxCitiesResponse.response);

                    string city = PromptComboBox<string>("Miejscowość urzędu", responseTaxCities.Select(c => new SelectComboBoxItem<string>(c, c)), true).data;
                    if (city == null)
                        return false;

                    mBankJsonRequestTaxAccounts jsonRequestTaxAccounts = new mBankJsonRequestTaxAccounts();
                    jsonRequestTaxAccounts.accType = selectedTax.accType;
                    jsonRequestTaxAccounts.cityName = city;

                    (string response, bool requestProcessed) taxAccountsResponse = PerformPlainRequest("api/paymentsBusiness/getTaxAccounts", HttpMethod.Post,
                        JsonConvert.SerializeObject(jsonRequestTaxAccounts));
                    if (!taxAccountsResponse.requestProcessed)
                        return false;

                    List<mBankJsonResponseTaxAccount> responseTaxAccounts = JsonConvert.DeserializeObject<List<mBankJsonResponseTaxAccount>>(taxAccountsResponse.response);
                    if (responseTaxAccounts.Count == 0)
                        return CheckFailed("Brak urzędów w wybranej miejscowości");

                    mBankJsonResponseTaxAccount taxOffice = PromptComboBox<mBankJsonResponseTaxAccount>("Urząd", responseTaxAccounts.Select(o => new SelectComboBoxItem<mBankJsonResponseTaxAccount>(o.taxAuthorityName, o)), true).data;
                    if (taxOffice == null)
                        return false;

                    jsonRequestInitPrepare.Data.usform.taxAuthority.authorityName.name = taxOffice.taxAccount;

                    taxOfficeName = $"{taxOffice.taxAuthorityName} {city}";
                    break;
                default: throw new NotImplementedException();
            }

            jsonRequestInitPrepare.Data.usform.idType = new mBankJsonRequestInitPrepareTaxTransferDataUsformIdType()
            {
                type = GetTaxCreditorIdentifierTypeId(creditorIdentifier),
                series = creditorIdentifier.GetId()
            };

            if (selectedTax.requiresPP)
            {
                (string symbol, string value, string month, string year) = GetTaxPeriodValue(period);

                jsonRequestInitPrepare.Data.usform.period = new mBankJsonRequestInitPrepareTaxTransferDataUsformPeriod()
                {
                    currentValue = value,
                    currentPeriod = symbol,
                    currentMonth = month,
                    currentYear = year
                };
            }
            else
            {
                jsonRequestInitPrepare.Data.usform.period = new mBankJsonRequestInitPrepareTaxTransferDataUsformPeriod()
                {
                    currentValue = "0",
                    currentPeriod = "0",
                    currentMonth = "0",
                    currentYear = "0"
                };
            }

            //if (selectedTax.isVatCompliant)

            return ConfirmTransfer(jsonRequestInitPrepare, new ConfirmTextTaxTransfer(amount, account.currency, taxOfficeName));
        }

        public static string GetTaxCreditorIdentifierTypeId(TaxCreditorIdentifier creditorIdentifier)
        {
            if (creditorIdentifier is TaxCreditorIdentifierNIP)
                return "N";
            else if (creditorIdentifier is TaxCreditorIdentifierIDCard)
                return "1";
            else if (creditorIdentifier is TaxCreditorIdentifierPESEL)
                return "P";
            else if (creditorIdentifier is TaxCreditorIdentifierREGON)
                return "R";
            else if (creditorIdentifier is TaxCreditorIdentifierPassport)
                return "2";
            else if (creditorIdentifier is TaxCreditorIdentifierOther)
                return "3";
            else
                throw new ArgumentException();
        }

        public static (string symbol, string value, string month, string year) GetTaxPeriodValue(TaxPeriod period)
        {
            if (period is TaxPeriodDay taxPeriodDay)
                return ("J", taxPeriodDay.Day.Day.ToString(), taxPeriodDay.Day.Month.ToString(), taxPeriodDay.Day.Year.ToString());
            else if (period is TaxPeriodHalfYear taxPeriodHalfYear)
                return ("P", taxPeriodHalfYear.Half.ToString(), "0", taxPeriodHalfYear.Year.ToString());
            else if (period is TaxPeriodMonth taxPeriodMonth)
                return ("M", taxPeriodMonth.Month.ToString(), taxPeriodMonth.Month.ToString(), taxPeriodMonth.Year.ToString());
            else if (period is TaxPeriodMonthDecade taxPeriodMonthDecade)
                return ("D", taxPeriodMonthDecade.Decade.ToString(), taxPeriodMonthDecade.Month.ToString(), taxPeriodMonthDecade.Year.ToString());
            else if (period is TaxPeriodQuarter taxPeriodQuarter)
                return ("K", taxPeriodQuarter.Quarter.ToString(), "0", taxPeriodQuarter.Year.ToString());
            else if (period is TaxPeriodYear taxPeriodYear)
                return ("R", "0", "0", taxPeriodYear.Year.ToString());
            else
                throw new ArgumentException();
        }

        public static string GetTaxPeriodValueShort(TaxPeriod period)
        {
            if (period is TaxPeriodDay taxPeriodDay)
                return $"{GetTaxPeriodYearValue(taxPeriodDay.Day.Year)}J{GetTaxPeriodNumberValue(taxPeriodDay.Day.Day)}{GetTaxPeriodNumberValue(taxPeriodDay.Day.Month)}";
            else if (period is TaxPeriodHalfYear taxPeriodHalfYear)
                return $"{GetTaxPeriodYearValue(taxPeriodHalfYear.Year)}P{GetTaxPeriodNumberValue(taxPeriodHalfYear.Half)}";
            else if (period is TaxPeriodMonth taxPeriodMonth)
                return $"{GetTaxPeriodYearValue(taxPeriodMonth.Year)}M{GetTaxPeriodNumberValue(taxPeriodMonth.Month)}";
            else if (period is TaxPeriodMonthDecade taxPeriodMonthDecade)
                return $"{GetTaxPeriodYearValue(taxPeriodMonthDecade.Year)}D{GetTaxPeriodNumberValue(taxPeriodMonthDecade.Decade)}{GetTaxPeriodNumberValue(taxPeriodMonthDecade.Month)}";
            else if (period is TaxPeriodQuarter taxPeriodQuarter)
                return $"{GetTaxPeriodYearValue(taxPeriodQuarter.Year)}K{GetTaxPeriodNumberValue(taxPeriodQuarter.Quarter)}";
            else if (period is TaxPeriodYear taxPeriodYear)
                return $"{GetTaxPeriodYearValue(taxPeriodYear.Year)}R";
            else
                throw new ArgumentException();
        }

        private static string GetTaxPeriodNumberValue(int number)
        {
            return number.ToString("D2");
        }

        private static string GetTaxPeriodYearValue(int year)
        {
            return year.ToString().SubstringFromEx(-2);
        }

        protected override string CleanFastTransferUrl(string transferId)
        {
            //TODO
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
            (mBankJsonResponsePhoneCharge response, bool requestProcessed) phoneChargeResponse = PerformRequest<mBankJsonResponsePhoneCharge>(
                "api/payments/phonecharge", HttpMethod.Get,
                null);

            //TODO find operator https://online.mbank.pl/api/payments/phonecharge/operator?phoneNumber=600000000

            mBankJsonResponsePhoneChargeOperator operatorItem = PromptComboBox<mBankJsonResponsePhoneChargeOperator>("Operator", phoneChargeResponse.response.operators.Select(o => new SelectComboBoxItem<mBankJsonResponsePhoneChargeOperator>(o.name, o)), false).data;
            if (operatorItem == null)
                return false;

            if (operatorItem.amounts.Count == 0)
            {
                if (amount < operatorItem.MinValue || amount > operatorItem.MaxValue)
                    return CheckFailed($"Kwota powinna znajdować się w zakresie {operatorItem.MinValue}-{operatorItem.MaxValue}");
            }
            else
            {
                if (!operatorItem.amounts.Select(a => a.value).Contains(amount))
                    return CheckFailed($"Kwota powinna być jedną z {String.Join(", ", operatorItem.amounts.Select(a => a.value.Display(DecimalSeparator.Dot)))}");
            }

            mBankJsonResponsePhoneChargeAccount account = phoneChargeResponse.response.fromAccounts.Single(a => a.displayName == $"{SelectedAccountData.Name} {SelectedAccountData.AccountNumber.SimplifyAccountNumber().Substring(0, 4)} ... {SelectedAccountData.AccountNumber.SimplifyAccountNumber().SubstringFromEx(-4)}");

            mBankJsonRequestInitPreparePhoneCharge jsonRequestInitPrepare = new mBankJsonRequestInitPreparePhoneCharge();
            jsonRequestInitPrepare.Data = new mBankJsonRequestInitPreparePhoneChargeData()
            {
                Amount = amount,
                Currency = SelectedAccountData.Currency,
                FormType = "PhoneCharge",
                FromAccount = account.number,
                MTransferId = operatorItem.mTransferId,
                OperatorId = operatorItem.operatorId,
                PhoneNumber = phoneNumber,
            };
            jsonRequestInitPrepare.Method = "POST";
            jsonRequestInitPrepare.TwoFactor = false;
            jsonRequestInitPrepare.Url = "payments/phonecharge";

            return ConfirmTransfer(jsonRequestInitPrepare, new ConfirmTextPrepaidTransfer(amount, SelectedAccountData.Currency, operatorItem.name, phoneNumber));
        }

        protected override mBankHistoryFilter CreateFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact)
        {
            return new mBankHistoryFilter(direction, title, dateFrom, dateTo, amountExact);
        }

        protected override List<mBankHistoryItem> GetHistoryItems(mBankHistoryFilter filter = null)
        {
            List<mBankHistoryItem> result = new List<mBankHistoryItem>();

            //TODO to GetAccountsDetails ?
            string historyUrl = WebOperations.BuildUrlWithQuery(BaseAddress, "api/pfm/ib/v1.0/history/PfmInitialData",
                new List<(string key, string value)> { ("shouldOverWriteFilters", "true"), ("transactionTypes", String.Empty) });
            (mBankJsonResponseHistory response, bool requestProcessed) historyResponse = PerformRequest<mBankJsonResponseHistory>(
                historyUrl, HttpMethod.Get,
                null);

            DateTime? dateTo = filter.DateTo;
            DateTime? dateFrom = filter.DateFrom;
            if (dateTo != null && dateTo > DateTime.Today)
                dateTo = DateTime.Today;
            if (dateFrom != null && dateFrom > DateTime.Today)
                dateFrom = DateTime.Today;

            List<(string key, string value)> transactionsParameters = new List<(string key, string value)>();
            transactionsParameters.Add(("productIds", historyResponse.response.pfmProducts.Single(p => p.name == SelectedAccountData.Name).id));
            if (filter.AmountFrom != null)
                transactionsParameters.Add(("amountFrom", ((double)filter.AmountFrom).Display(DecimalSeparator.Dot)));
            if (filter.AmountTo != null)
                transactionsParameters.Add(("amountTo", ((double)filter.AmountTo).Display(DecimalSeparator.Dot)));
            transactionsParameters.Add(("searchText", filter.Title));
            if (filter.DateFrom != null)
                transactionsParameters.Add(("dateFrom", ((DateTime)dateFrom).Display("yyyy-MM-dd")));
            if (filter.DateTo != null)
                transactionsParameters.Add(("dateTo", ((DateTime)dateTo).Display("yyyy-MM-dd")));
            if (filter.OperationType == mBankFilterOperationType.All || filter.OperationType == mBankFilterOperationType.Outgoing)
                transactionsParameters.Add(("showDebitTransactionTypes", "true"));
            if (filter.OperationType == mBankFilterOperationType.All || filter.OperationType == mBankFilterOperationType.Incoming)
                transactionsParameters.Add(("showCreditTransactionTypes", "true"));
            string transactionsUrl = WebOperations.BuildUrlWithQuery(BaseAddress, "api/pfm/ib/v1.0/Transactions/GetOperationsPfm", transactionsParameters);

            (mBankJsonResponseTransactions response, bool requestProcessed) transactionsResponse = PerformRequest<mBankJsonResponseTransactions>(
                transactionsUrl, HttpMethod.Get,
                null);

            if (transactionsResponse.requestProcessed)
            {
                foreach (mBankJsonResponseTransactionsTransaction transaction in transactionsResponse.response.transactions)
                {
                    if (filter.CounterLimit == 0 || result.Count < filter.CounterLimit)
                    {
                        List<(string key, string value)> transactionParameters = new List<(string key, string value)>();
                        transactionParameters.Add(("accountNumber", transaction.accountNumber));
                        //TODO does number depend on paging
                        transactionParameters.Add(("operationNumber", transaction.operationNumber.ToString()));
                        string transactionUrl = WebOperations.BuildUrlWithQuery(BaseAddress, "api/pfm/ib/v1.0/Transactions/Details", transactionParameters);

                        (mBankJsonResponseTransaction response, bool requestProcessed) transactionResponse = PerformRequest<mBankJsonResponseTransaction>(
                            transactionUrl, HttpMethod.Get,
                            null);

                        result.Add(new mBankHistoryItem(transaction, transactionResponse.response));
                    }
                    else
                    {
                        result.Add(new mBankHistoryItem(transaction));
                    }
                }
            }

            return result;
        }

        protected override bool GetDetailsFileMain(mBankHistoryItem item, Func<ContentDispositionHeaderValue, FileStream> file)
        {
            mBankJsonRequestConfirmation jsonRequestConfirmation = new mBankJsonRequestConfirmation();
            jsonRequestConfirmation.Transactions = new List<mBankJsonRequestConfirmationTransaction>() {new mBankJsonRequestConfirmationTransaction(){
                accountNumber = item.AccountNumber,
                operationNumber = item.OperationNumber
            }};

            PerformFileRequest(
                "api/pfm/ib/v1.0/Printouts/TransactionsConfirmation", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestConfirmation),
                file);

            return true;
        }

        private bool ConfirmAuthorization(mBankJsonRequestInitializeBase jsonRequestInitialize, string finalizeAuthorizationUrl, mBankJsonRequestFinalizeAuthorizationBase jsonRequestFinalizeAuthorization)
        {
            (mBankJsonResponseAuthorization response, bool requestProcessed) initializeResponse = PerformRequest<mBankJsonResponseAuthorization>(
                "api/AuthorizationMediator/ib/v5/initialize", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestInitialize));
            if (!initializeResponse.requestProcessed)
                return false;

            ConfirmTextBase confirmText = new ConfirmTextAuthorize(null);

            switch (initializeResponse.response.authorizationData.AuthorizationTypeValue)
            {
                case mBankJsonAuthorizationType.SMS:
                    {
                        if (!SMSConfirm<bool, (mBankJsonResponseAuthorize response, bool requestProcessed)>(
                            (string SMSCode) =>
                            {
                                mBankJsonRequestConfirm jsonRequestConfirm = new mBankJsonRequestConfirm();
                                jsonRequestConfirm.authorizationCode = SMSCode;
                                jsonRequestConfirm.authorizationType = "SMS";

                                return PerformRequest<mBankJsonResponseAuthorize>(
                                    "api/AuthorizationMediator/ib/v5/authorize", HttpMethod.Post,
                                    JsonConvert.SerializeObject(jsonRequestConfirm));
                            },
                            ((mBankJsonResponseAuthorize response, bool requestProcessed) authorizeResponse) =>
                            {
                                if (!authorizeResponse.requestProcessed)
                                    return false;
                                if (authorizeResponse.response.status == 422)
                                    return null;
                                else
                                    return true;
                            },
                            ((mBankJsonResponseAuthorize response, bool requestProcessed) authorizeResponse) => true,
                            null,
                            confirmText,
                            initializeResponse.response.authorizationData.authorizationNumber))
                            return false;

                        (mBankJsonResponseFinalizeAuthorization response, bool requestProcessed) finalizeAuthorizationStatusResponse = PerformRequest<mBankJsonResponseFinalizeAuthorization>(
                            finalizeAuthorizationUrl, HttpMethod.Post,
                            JsonConvert.SerializeObject(jsonRequestFinalizeAuthorization));

                        return true;
                    }
                case mBankJsonAuthorizationType.Mobile:
                    {
                        if (!MobileConfirm<bool, (mBankJsonResponseAuthorizationStatus response, bool requestProcessed)>(
                            () =>
                            {
                                mBankJsonRequestAuthorizationStatus jsonRequestAuthorizationStatus = new mBankJsonRequestAuthorizationStatus();
                                jsonRequestAuthorizationStatus.authorizationId = initializeResponse.response.authorizationData.authorizationId;

                                return PerformRequest<mBankJsonResponseAuthorizationStatus>(
                                    "api/AuthorizationMediator/ib/v5/status", HttpMethod.Post,
                                    JsonConvert.SerializeObject(jsonRequestAuthorizationStatus));
                            },
                            ((mBankJsonResponseAuthorizationStatus response, bool requestProcessed) authorizationStatusResponse) =>
                            {
                                if (authorizationStatusResponse.response?.AuthorizationStatusValue == mBankJsonAuthorizationStatus.Cancel)
                                    return false;
                                if (authorizationStatusResponse.response?.AuthorizationStatusValue == mBankJsonAuthorizationStatus.Authorized)
                                    return true;

                                return null;
                            },
                            ((mBankJsonResponseAuthorizationStatus response, bool requestProcessed) authorizationStatusResponse) => true,
                            null,
                            confirmText))
                            return false;

                        (mBankJsonResponseFinalizeAuthorization response, bool requestProcessed) finalizeAuthorizationStatusResponse = PerformRequest<mBankJsonResponseFinalizeAuthorization>(
                            finalizeAuthorizationUrl, HttpMethod.Post,
                            JsonConvert.SerializeObject(jsonRequestFinalizeAuthorization));

                        return true;
                    }
                default: throw new NotImplementedException();
            }
        }

        private bool ConfirmTransfer(mBankJsonRequestInitPrepare jsonRequestInitPrepare, ConfirmTextBase confirmText)
        {
            (mBankJsonResponseInitPrepare response, bool requestProcessed) initPrepareResponse = PerformRequest<mBankJsonResponseInitPrepare>(
                "api/auth/initprepare", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestInitPrepare));
            if (!initPrepareResponse.requestProcessed)
                return false;

            switch (initPrepareResponse.response.AuthorizationModeValue)
            {
                case mBankJsonAuthorizationMode.SMS:
                    {
                        return SMSConfirm<bool, (mBankJsonResponseExecute response, bool requestProcessed)>(
                            (string SMSCode) =>
                            {
                                mBankJsonRequestExecute jsonRequestExecute = new mBankJsonRequestExecute();
                                jsonRequestExecute.Auth = SMSCode;

                                return PerformRequest<mBankJsonResponseExecute>(
                                    "api/auth/execute", HttpMethod.Post,
                                    JsonConvert.SerializeObject(jsonRequestExecute));
                            },
                            ((mBankJsonResponseExecute response, bool requestProcessed) executeResponse) =>
                            {
                                if (!executeResponse.requestProcessed)
                                    return false;
                                if (executeResponse.response.error != null)
                                    return null;
                                else
                                    return true;
                            },
                            ((mBankJsonResponseExecute response, bool requestProcessed) executeResponse) => true,
                            null,
                            //TODO use initPrepareResponse.response.Data.amount
                            confirmText,
                            initPrepareResponse.response.OperationNumber);
                    }
                case mBankJsonAuthorizationMode.Mobile:
                    {
                        if (!MobileConfirm<bool, (mBankJsonResponseAuthorizationTransferStatus response, bool requestProcessed)>(
                            () =>
                            {
                                mBankJsonRequestAuthorizationTransferStatus jsonRequestAuthorizationStatus = new mBankJsonRequestAuthorizationTransferStatus();
                                jsonRequestAuthorizationStatus.TranId = initPrepareResponse.response.TranId;

                                return PerformRequest<mBankJsonResponseAuthorizationTransferStatus>(
                                    "api/auth/status", HttpMethod.Post,
                                    JsonConvert.SerializeObject(jsonRequestAuthorizationStatus));
                            },
                            ((mBankJsonResponseAuthorizationTransferStatus response, bool requestProcessed) authorizationStatusResponse) =>
                            {
                                if (authorizationStatusResponse.response?.AuthorizationStatusValue == mBankJsonAuthorizationTransferStatus.Cancel)
                                    return false;
                                if (authorizationStatusResponse.response.AuthorizationStatusValue == mBankJsonAuthorizationTransferStatus.Authorized)
                                    return true;

                                return null;
                            },
                            ((mBankJsonResponseAuthorizationTransferStatus response, bool requestProcessed) authorizationStatusResponse) => true,
                            null,
                            confirmText))
                            return false;

                        mBankJsonRequestExecute jsonRequestExecute = new mBankJsonRequestExecute();

                        (mBankJsonResponseExecute response, bool requestProcessed) executeResponse = PerformRequest<mBankJsonResponseExecute>(
                            "api/auth/execute", HttpMethod.Post,
                            JsonConvert.SerializeObject(jsonRequestExecute));

                        return true;
                    }
                default: throw new NotImplementedException();
            }
        }

        private (T, bool) GetRequest<T>(
            HttpRequestMessage request,
            Func<string, T> responseStrAction) where T : class
        {
            using (HttpResponseMessage response = HttpOperations.GetResponse(Client, request))
                return ProcessResponse<T>(response,
                    (string responseStr) =>
                    {
                        if (response.StatusCode == HttpStatusCode.BadRequest)
                            return false;
                        return true;
                    },
                    responseStrAction);
        }

        private (T, bool) PerformRequest<T>(string requestUri, HttpMethod method,
            string jsonContent) where T : mBankJsonResponseBase
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, jsonContent))
            {
                (T response, bool requestProcessed) result = GetRequest<T>(request, (responseStr) => JsonConvert.DeserializeObject<T>(responseStr));

                return result;
            }
        }

        private (string, bool) PerformPlainRequest(string requestUri, HttpMethod method, string jsonContent)
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, jsonContent))
                return GetRequest<string>(request, (responseStr) => responseStr);
        }

        private void PerformFileRequest(string requestUri, HttpMethod method,
            string jsonContent,
            Func<ContentDispositionHeaderValue, FileStream> fileStream)
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, jsonContent))
                ProcessFileStream(request, fileStream);
        }

        private HttpRequestMessage CreateHttpRequestMessage(string requestUri, HttpMethod method, string jsonContent)
        {
            List<(string name, string value)> headers = new List<(string name, string value)>();
            if (Token != null)
                headers.Add(("x-request-verification-token", Token));

            return HttpOperations.CreateHttpRequestMessageJson(method, requestUri, jsonContent, headers);
        }
    }
}
