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
using static BankService.Bank_PL_MBank.MBankJsonRequest;
using static BankService.Bank_PL_MBank.MBankJsonResponse;

namespace BankService.Bank_PL_MBank
{
    [BankTypeAttribute(BankType.MBank)]
    public class MBank : BankPoland<MBankAccountData, MBankHistoryItem, MBankHistoryFilter, MBankJsonResponseProducts>
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

            MBankJsonRequestLogin jsonRequestLogin = new MBankJsonRequestLogin
            {
                UserName = login,
                Password = password,
                Scenario = "Default",
                UWAdditionalParams = new MBankJsonRequestAdditionalOptionsDummy(),
                DfpData = new MBankJsonRequestLoginDfpData()
                {
                    dfp = dfp,
                }
            };

            (MBankJsonResponseLogin response, bool requestProcessed) loginResponse = PerformRequest<MBankJsonResponseLogin>(
                "pl/LoginMain/Account/JsonLogin", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestLogin));
            if (!loginResponse.response.successful)
                return CheckFailed($"{loginResponse.response.errorMessageTitle}: {loginResponse.response.errorMessageBody}");

            (MBankJsonResponseSetupData response, bool requestProcessed) setupDataResponse = PerformRequest<MBankJsonResponseSetupData>(
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
            (MBankJsonResponseAuthorizationData response, bool requestProcessed) authorizationDataResponse = PerformRequest<MBankJsonResponseAuthorizationData>(
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
                    string trustedDevicesCheckUrl = WebOperations.BuildUrlWithQuery("api/sca/TrustedDevices/IsPossibleToAddNextDeviceAndCheckDeviceName",
                        new List<(string key, string value)> { ("defaultDeviceName", Constants.AppDeviceName) });
                    (MBankJsonResponseTrustedDevicesCheck response, bool requestProcessed) trustedDevicesCheckResponse = PerformRequest<MBankJsonResponseTrustedDevicesCheck>(
                        trustedDevicesCheckUrl, HttpMethod.Get,
                        null);
                    if (!trustedDevicesCheckResponse.requestProcessed)
                        return false;

                    if (!trustedDevicesCheckResponse.response.isValid)
                        //TODO
                        throw new NotImplementedException();

                    string trustedDevicesAddUrl = WebOperations.BuildUrlWithQuery("api/sca/TrustedDevices",
                        new List<(string key, string value)> { ("deviceName", trustedDevicesCheckResponse.response.deviceName) });
                    (MBankJsonResponseTrustedDevicesAdd response, bool requestProcessed) trustedDevicesAddResponse = PerformRequest<MBankJsonResponseTrustedDevicesAdd>(
                        trustedDevicesAddUrl, HttpMethod.Get,
                        null);
                    if (!trustedDevicesAddResponse.requestProcessed)
                        return false;

                    if (!trustedDevicesAddResponse.response.isValid)
                        throw new NotImplementedException();

                    MBankJsonRequestInitializeTrustedDevice jsonRequestInitializeTrustedDevice = new MBankJsonRequestInitializeTrustedDevice
                    {
                        moduleId = "ScaTrustedDevice",
                        moduleData = new MBankJsonRequestInitializeTrustedDeviceModuleData
                        {
                            BrowserName = Constants.AppBrowserName,
                            BrowserVersion = "1",
                            OsName = "Windows",
                            ScaAuthorizationId = authorizationDataResponse.response.ScaAuthorizationId,
                            DeviceName = trustedDevicesCheckResponse.response.deviceName,
                            DfpData = dfp,
                            IsTheOnlyDeviceUser = true
                        }
                    };

                    MBankJsonRequestFinalizeAuthorizationTrustedDevice jsonRequestFinalizeAuthorizationTrustedDevice = new MBankJsonRequestFinalizeAuthorizationTrustedDevice
                    {
                        scaAuthorizationId = authorizationDataResponse.response.ScaAuthorizationId,
                        deviceName = trustedDevicesCheckResponse.response.deviceName,
                        currentDfp = dfp
                    };

                    bool confirmed = ConfirmAuthorization(jsonRequestInitializeTrustedDevice, "pl/Sca/FinalizeTrustedDeviceAuthorization", jsonRequestFinalizeAuthorizationTrustedDevice);

                    if (confirmed)
                        SaveCookie(DomainCookies.GetCookie("mBank8"));

                    return confirmed;
                }
                else
                {
                    MBankJsonRequestInitialize jsonRequestInitialize = new MBankJsonRequestInitialize
                    {
                        moduleId = "ScaHostless",
                        moduleData = new MBankJsonRequestInitializeModuleData
                        {
                            BrowserName = Constants.AppBrowserName,
                            //jsonRequestAuthorization.moduleData.BrowserVersion = "1";
                            //jsonRequestAuthorization.moduleData.OsName = "Windows";
                            ScaAuthorizationId = authorizationDataResponse.response.ScaAuthorizationId
                        }
                    };

                    MBankJsonRequestFinalizeAuthorization jsonRequestFinalizeAuthorization = new MBankJsonRequestFinalizeAuthorization
                    {
                        scaAuthorizationId = authorizationDataResponse.response.ScaAuthorizationId
                    };

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
            (MBankJsonResponseExtendSession response, bool requestProcessed) extendSessionResponse = PerformRequest<MBankJsonResponseExtendSession>(
                "pl/LoginMain/Account/JsonSessionKeepAlive", HttpMethod.Post,
                null);

            return extendSessionResponse.response.success;
        }

        protected override MBankJsonResponseProducts GetAccountsDetails()
        {
            //TODO should be called only once
            (MBankJsonResponseUserSettings response, bool requestProcessed) userSettingsResponse = PerformRequest<MBankJsonResponseUserSettings>(
                "pl/MyDesktop/DynamicDashboard/GetUserSettings", HttpMethod.Post,
                null);

            MBankJsonRequestProducts jsonRequestProducts = new MBankJsonRequestProducts
            {
                productsIds = userSettingsResponse.response.products.Select(p => new MBankJsonRequestProductsProduct()
                { id = p.id, order = (int)p.order, productType = p.productType }).ToList()
            };

            (MBankJsonResponseProducts response, bool requestProcessed) productsResponse = PerformRequest<MBankJsonResponseProducts>(
                "pl/MyDesktop/DynamicDashboard/GetProductsFromUserSettings", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestProducts));

            return productsResponse.response;
        }

        protected override List<MBankAccountData> GetAccountsDataMainMain(MBankJsonResponseProducts accountsDetails)
        {
            //TODO p.balance.value ?
            return accountsDetails.products.Select(p => new MBankAccountData(p.name, p.number, p.currency, p.AvailableBalance)).ToList();
        }

        protected override bool MakeTransfer(string recipient, string address, string accountNumber, string title, double amount)
        {
            if (title?.Length > 140)
                return CheckFailed("Tytuł nie może zawierać więcej niż 140 znaków");
            //TODO checking to AddressTools, inside can be removing spaces
            if (address.Length > 70)
                return CheckFailed("Adres nie może zawierać więcej niż 70 znaków");

            List<string> addresses = AddressTools.SplitAddress(address, 2, 35);

            (MBankJsonResponseDomestic response, bool requestProcessed) domesticResponse = PerformRequest<MBankJsonResponseDomestic>(
                "api/payments/domesticeea", HttpMethod.Get,
                null);

            //TODO almost like in browser but domesticResponse.response.transferData is null. Althought it's ok

            MBankJsonResponseDomesticAccount account = domesticResponse.response.availableAccounts.Single(a => a.name == SelectedAccountData.Name);

            MBankJsonRequestTransferCheck jsonRequestTransferCheck = new MBankJsonRequestTransferCheck
            {
                FromAccount = account.number,
                ToAccount = accountNumber,
                Date = Now,
                Amount = new MBankJsonRequestAmountCapital() { Currency = account.currency, Value = amount },
                RedirectionSource = "none",
                TransferType = "elixir",
                checkDataId = Guid.NewGuid().ToString("D")
            };

            (MBankJsonResponseTransferCheck response, bool requestProcessed) transferCheckResponse = PerformRequest<MBankJsonResponseTransferCheck>(
                "api/payments/domesticeea/checkData", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestTransferCheck));
            if (!transferCheckResponse.requestProcessed)
                return false;

            MBankJsonRequestInitPrepareTransfer jsonRequestInitPrepare = new MBankJsonRequestInitPrepareTransfer
            {
                Data = new MBankJsonRequestInitPrepareTransferData()
                {
                    toAccount = accountNumber,
                    amount = new MBankJsonRequestAmount() { currency = account.currency, value = amount },
                    date = Now,
                    fromAccount = account.number,
                    cardNumber = null,
                    //TODO displayName or coownerId from pl/setup/data, the same in TaxTransfer
                    coowner = account.coowners.Single(/*c=>c.displayName ==*/).coownerId,
                    receiver = new MBankJsonRequestInitPrepareTransferDataReceiver() { name = recipient, street = addresses[0], cityAndPostalCode = addresses[1], nip = null },
                    title = title,
                    transferMode = "elixir",
                    //paymentSource = domesticResponse.response.transferData.paymentSource,
                    additionalOptions = new MBankJsonRequestAdditionalOptionsDummy(),
                    perfToken = transferCheckResponse.response.perfToken,
                },
                Method = "POST",
                TwoFactor = false,
                Url = "payments/domesticEea"
            };

            return ConfirmTransfer(jsonRequestInitPrepare, new ConfirmTextTransfer(amount, account.currency, transferCheckResponse.response.bankDetails.name, accountNumber));
        }

        protected override bool MakeTaxTransfer(string taxType, string accountNumber, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier, string creditorName, string obligationId, double amount)
        {
            (string response, bool requestProcessed) taxFormTypesResponse = PerformPlainRequest("api/paymentsBusiness/getTaxFormTypes", HttpMethod.Post, null);
            if (!taxFormTypesResponse.requestProcessed)
                return false;
            List<MBankJsonResponseTaxFormType> responseTaxFormTypes = JsonConvert.DeserializeObject<List<MBankJsonResponseTaxFormType>>(taxFormTypesResponse.response);

            MBankJsonResponseTaxFormType selectedTax = responseTaxFormTypes.SingleOrDefault(s => s.formName == taxType);
            if (selectedTax == null)
                return CheckFailed("Nie znaleziono podanego typu formularza");

            MBankJsonRequestTaxTransferPrepare jsonRequestTaxTransferPrepare = new MBankJsonRequestTaxTransferPrepare();

            (MBankJsonResponseTaxTransferPrepare response, bool requestProcessed) taxTransferPrepareResponse = PerformRequest<MBankJsonResponseTaxTransferPrepare>(
                "api/paymentsBusiness/prepareOneUs", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestTaxTransferPrepare));

            MBankJsonResponseTaxTransferPrepareFormDataDefaultDataAccount account = taxTransferPrepareResponse.response.formData.defaultData.availableFromAccounts.Single(a => a.displayName == $"{SelectedAccountData.Name} -  {SelectedAccountData.AccountNumber.SimplifyAccountNumber().Substring(0, 4)} ... {SelectedAccountData.AccountNumber.SimplifyAccountNumber().SubstringFromEx(-4)}");

            MBankJsonRequestInitPrepareTaxTransfer jsonRequestInitPrepare = new MBankJsonRequestInitPrepareTaxTransfer
            {
                Data = new MBankJsonRequestInitPrepareTaxTransferData()
                {
                    usform = new MBankJsonRequestInitPrepareTaxTransferDataUsform()
                    {
                        fromAccount = account.number,
                        sender = account.coowners.Single().coownerId,
                        //accountParams = account.accountParams,
                        perfToken = taxTransferPrepareResponse.response.formData.perfToken,
                        date = Today.Display("yyyy-MM-dd"),
                        formType = "us",
                        taxAuthority = new MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthority()
                        {
                            authoritySymbol = new MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthoritySymbol()
                            {
                                symbol = taxType,
                                toAnother = false
                            },
                            authorityCity = new MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityCity(),
                            authorityName = new MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityName(),
                            authorityNameCustom = new MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityNameCustom() { },
                            authorityAccountNumberCustom = new MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityAccountNumberCustom() { },
                        },
                        amount = amount,
                        currency = account.currency,
                        defaultData = null,
                        commitmentId = obligationId,
                        additionalOptions = new MBankJsonRequestAdditionalOptions(),
                    }
                },
                Method = "POST",
                TwoFactor = false,
                Url = "paymentsBusiness/intermediateOneUs"
            };

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

                    MBankJsonRequestTaxAccounts jsonRequestTaxAccounts = new MBankJsonRequestTaxAccounts
                    {
                        accType = selectedTax.accType,
                        cityName = city
                    };

                    (string response, bool requestProcessed) taxAccountsResponse = PerformPlainRequest("api/paymentsBusiness/getTaxAccounts", HttpMethod.Post,
                        JsonConvert.SerializeObject(jsonRequestTaxAccounts));
                    if (!taxAccountsResponse.requestProcessed)
                        return false;

                    List<MBankJsonResponseTaxAccount> responseTaxAccounts = JsonConvert.DeserializeObject<List<MBankJsonResponseTaxAccount>>(taxAccountsResponse.response);
                    if (responseTaxAccounts.Count == 0)
                        return CheckFailed("Brak urzędów w wybranej miejscowości");

                    MBankJsonResponseTaxAccount taxOffice = PromptComboBox<MBankJsonResponseTaxAccount>("Urząd", responseTaxAccounts.Select(o => new SelectComboBoxItem<MBankJsonResponseTaxAccount>(o.taxAuthorityName, o)), true).data;
                    if (taxOffice == null)
                        return false;

                    jsonRequestInitPrepare.Data.usform.taxAuthority.authorityName.name = taxOffice.taxAccount;

                    taxOfficeName = $"{taxOffice.taxAuthorityName} {city}";
                    break;
                default: throw new NotImplementedException();
            }

            jsonRequestInitPrepare.Data.usform.idType = new MBankJsonRequestInitPrepareTaxTransferDataUsformIdType()
            {
                type = GetTaxCreditorIdentifierTypeId(creditorIdentifier),
                series = creditorIdentifier.GetId()
            };

            if (selectedTax.requiresPP)
            {
                (string symbol, string value, string month, string year) = GetTaxPeriodValue(period);

                jsonRequestInitPrepare.Data.usform.period = new MBankJsonRequestInitPrepareTaxTransferDataUsformPeriod()
                {
                    currentValue = value,
                    currentPeriod = symbol,
                    currentMonth = month,
                    currentYear = year
                };
            }
            else
            {
                jsonRequestInitPrepare.Data.usform.period = new MBankJsonRequestInitPrepareTaxTransferDataUsformPeriod()
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
            (MBankJsonResponsePhoneCharge response, bool requestProcessed) phoneChargeResponse = PerformRequest<MBankJsonResponsePhoneCharge>(
                "api/payments/phonecharge", HttpMethod.Get,
                null);

            //TODO find operator https://online.mbank.pl/api/payments/phonecharge/operator?phoneNumber=600000000

            MBankJsonResponsePhoneChargeOperator operatorItem = PromptComboBox<MBankJsonResponsePhoneChargeOperator>("Operator", phoneChargeResponse.response.operators.Select(o => new SelectComboBoxItem<MBankJsonResponsePhoneChargeOperator>(o.name, o)), false).data;
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

            MBankJsonResponsePhoneChargeAccount account = phoneChargeResponse.response.fromAccounts.Single(a => a.displayName == $"{SelectedAccountData.Name} {SelectedAccountData.AccountNumber.SimplifyAccountNumber().Substring(0, 4)} ... {SelectedAccountData.AccountNumber.SimplifyAccountNumber().SubstringFromEx(-4)}");

            MBankJsonRequestInitPreparePhoneCharge jsonRequestInitPrepare = new MBankJsonRequestInitPreparePhoneCharge
            {
                Data = new MBankJsonRequestInitPreparePhoneChargeData()
                {
                    Amount = amount,
                    Currency = SelectedAccountData.Currency,
                    FormType = "PhoneCharge",
                    FromAccount = account.number,
                    MTransferId = operatorItem.mTransferId,
                    OperatorId = operatorItem.operatorId,
                    PhoneNumber = phoneNumber,
                },
                Method = "POST",
                TwoFactor = false,
                Url = "payments/phonecharge"
            };

            return ConfirmTransfer(jsonRequestInitPrepare, new ConfirmTextPrepaidTransfer(amount, SelectedAccountData.Currency, operatorItem.name, phoneNumber));
        }

        protected override MBankHistoryFilter CreateFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact)
        {
            return new MBankHistoryFilter(direction, title, dateFrom, dateTo, amountExact);
        }

        protected override List<MBankHistoryItem> GetHistoryItems(MBankHistoryFilter filter = null)
        {
            List<MBankHistoryItem> result = new List<MBankHistoryItem>();

            //TODO to GetAccountsDetails ?
            string historyUrl = WebOperations.BuildUrlWithQuery("api/pfm/ib/v1.0/history/PfmInitialData",
                new List<(string key, string value)> { ("shouldOverWriteFilters", "true"), ("transactionTypes", String.Empty) });
            (MBankJsonResponseHistory response, bool requestProcessed) historyResponse = PerformRequest<MBankJsonResponseHistory>(
                historyUrl, HttpMethod.Get,
                null);

            DateTime? dateTo = filter.DateTo;
            DateTime? dateFrom = filter.DateFrom;
            if (dateTo != null && dateTo > Today)
                dateTo = Today;
            if (dateFrom != null && dateFrom > Today)
                dateFrom = Today;

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
            if (filter.OperationType == MBankFilterOperationType.All || filter.OperationType == MBankFilterOperationType.Outgoing)
                transactionsParameters.Add(("showDebitTransactionTypes", "true"));
            if (filter.OperationType == MBankFilterOperationType.All || filter.OperationType == MBankFilterOperationType.Incoming)
                transactionsParameters.Add(("showCreditTransactionTypes", "true"));
            string transactionsUrl = WebOperations.BuildUrlWithQuery("api/pfm/ib/v1.0/Transactions/GetOperationsPfm", transactionsParameters);

            (MBankJsonResponseTransactions response, bool requestProcessed) transactionsResponse = PerformRequest<MBankJsonResponseTransactions>(
                transactionsUrl, HttpMethod.Get,
                null);

            if (transactionsResponse.requestProcessed)
            {
                foreach (MBankJsonResponseTransactionsTransaction transaction in transactionsResponse.response.transactions)
                {
                    if (filter.CounterLimit == 0 || result.Count < filter.CounterLimit)
                    {
                        List<(string key, string value)> transactionParameters = new List<(string key, string value)>();
                        transactionParameters.Add(("accountNumber", transaction.accountNumber));
                        //TODO does number depend on paging
                        transactionParameters.Add(("operationNumber", transaction.operationNumber.ToString()));
                        string transactionUrl = WebOperations.BuildUrlWithQuery("api/pfm/ib/v1.0/Transactions/Details", transactionParameters);

                        (MBankJsonResponseTransaction response, bool requestProcessed) transactionResponse = PerformRequest<MBankJsonResponseTransaction>(
                            transactionUrl, HttpMethod.Get,
                            null);

                        result.Add(new MBankHistoryItem(transaction, transactionResponse.response));
                    }
                    else
                    {
                        result.Add(new MBankHistoryItem(transaction));
                    }
                }
            }

            return result;
        }

        protected override bool GetDetailsFileMain(MBankHistoryItem item, Func<string, FileStream> file)
        {
            MBankJsonRequestConfirmation jsonRequestConfirmation = new MBankJsonRequestConfirmation
            {
                Transactions = new List<MBankJsonRequestConfirmationTransaction>() {new MBankJsonRequestConfirmationTransaction(){
                    accountNumber = item.AccountNumber,
                    operationNumber = item.OperationNumber
                }}
            };

            PerformFileRequest(
                "api/pfm/ib/v1.0/Printouts/TransactionsConfirmation", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestConfirmation),
                file);

            return true;
        }

        private bool ConfirmAuthorization(MBankJsonRequestInitializeBase jsonRequestInitialize, string finalizeAuthorizationUrl, MBankJsonRequestFinalizeAuthorizationBase jsonRequestFinalizeAuthorization)
        {
            (MBankJsonResponseAuthorization response, bool requestProcessed) initializeResponse = PerformRequest<MBankJsonResponseAuthorization>(
                "api/AuthorizationMediator/ib/v5/initialize", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestInitialize));
            if (!initializeResponse.requestProcessed)
                return false;

            ConfirmTextBase confirmText = new ConfirmTextAuthorize(null);

            switch (initializeResponse.response.authorizationData.AuthorizationTypeValue)
            {
                case MBankJsonAuthorizationType.SMS:
                    {
                        if (!SMSConfirm<bool, (MBankJsonResponseAuthorize response, bool requestProcessed)>(
                            (string SMSCode) =>
                            {
                                MBankJsonRequestConfirm jsonRequestConfirm = new MBankJsonRequestConfirm
                                {
                                    authorizationCode = SMSCode,
                                    authorizationType = "SMS"
                                };

                                return PerformRequest<MBankJsonResponseAuthorize>(
                                    "api/AuthorizationMediator/ib/v5/authorize", HttpMethod.Post,
                                    JsonConvert.SerializeObject(jsonRequestConfirm));
                            },
                            ((MBankJsonResponseAuthorize response, bool requestProcessed) authorizeResponse) =>
                            {
                                if (!authorizeResponse.requestProcessed)
                                    return false;
                                if (authorizeResponse.response.status == 422)
                                    return null;
                                else
                                    return true;
                            },
                            ((MBankJsonResponseAuthorize response, bool requestProcessed) authorizeResponse) => true,
                            null,
                            confirmText,
                            initializeResponse.response.authorizationData.authorizationNumber))
                            return false;

                        (MBankJsonResponseFinalizeAuthorization response, bool requestProcessed) finalizeAuthorizationStatusResponse = PerformRequest<MBankJsonResponseFinalizeAuthorization>(
                            finalizeAuthorizationUrl, HttpMethod.Post,
                            JsonConvert.SerializeObject(jsonRequestFinalizeAuthorization));

                        return true;
                    }
                case MBankJsonAuthorizationType.Mobile:
                    {
                        if (!MobileConfirm<bool, (MBankJsonResponseAuthorizationStatus response, bool requestProcessed)>(
                            () =>
                            {
                                MBankJsonRequestAuthorizationStatus jsonRequestAuthorizationStatus = new MBankJsonRequestAuthorizationStatus
                                {
                                    authorizationId = initializeResponse.response.authorizationData.authorizationId
                                };

                                return PerformRequest<MBankJsonResponseAuthorizationStatus>(
                                    "api/AuthorizationMediator/ib/v5/status", HttpMethod.Post,
                                    JsonConvert.SerializeObject(jsonRequestAuthorizationStatus));
                            },
                            ((MBankJsonResponseAuthorizationStatus response, bool requestProcessed) authorizationStatusResponse) =>
                            {
                                if (authorizationStatusResponse.response?.AuthorizationStatusValue == MBankJsonAuthorizationStatus.Cancel)
                                    return false;
                                if (authorizationStatusResponse.response?.AuthorizationStatusValue == MBankJsonAuthorizationStatus.Authorized)
                                    return true;

                                return null;
                            },
                            ((MBankJsonResponseAuthorizationStatus response, bool requestProcessed) authorizationStatusResponse) => true,
                            null,
                            confirmText))
                            return false;

                        (MBankJsonResponseFinalizeAuthorization response, bool requestProcessed) finalizeAuthorizationStatusResponse = PerformRequest<MBankJsonResponseFinalizeAuthorization>(
                            finalizeAuthorizationUrl, HttpMethod.Post,
                            JsonConvert.SerializeObject(jsonRequestFinalizeAuthorization));

                        return true;
                    }
                default: throw new NotImplementedException();
            }
        }

        private bool ConfirmTransfer(MBankJsonRequestInitPrepare jsonRequestInitPrepare, ConfirmTextBase confirmText)
        {
            (MBankJsonResponseInitPrepare response, bool requestProcessed) initPrepareResponse = PerformRequest<MBankJsonResponseInitPrepare>(
                "api/auth/initprepare", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestInitPrepare));
            if (!initPrepareResponse.requestProcessed)
                return false;

            switch (initPrepareResponse.response.AuthorizationModeValue)
            {
                case MBankJsonAuthorizationMode.SMS:
                    {
                        return SMSConfirm<bool, (MBankJsonResponseExecute response, bool requestProcessed)>(
                            (string SMSCode) =>
                            {
                                MBankJsonRequestExecute jsonRequestExecute = new MBankJsonRequestExecute
                                {
                                    Auth = SMSCode
                                };

                                return PerformRequest<MBankJsonResponseExecute>(
                                    "api/auth/execute", HttpMethod.Post,
                                    JsonConvert.SerializeObject(jsonRequestExecute));
                            },
                            ((MBankJsonResponseExecute response, bool requestProcessed) executeResponse) =>
                            {
                                if (!executeResponse.requestProcessed)
                                    return false;
                                if (executeResponse.response.error != null)
                                    return null;
                                else
                                    return true;
                            },
                            ((MBankJsonResponseExecute response, bool requestProcessed) executeResponse) => true,
                            null,
                            //TODO use initPrepareResponse.response.Data.amount
                            confirmText,
                            initPrepareResponse.response.OperationNumber);
                    }
                case MBankJsonAuthorizationMode.Mobile:
                    {
                        if (!MobileConfirm<bool, (MBankJsonResponseAuthorizationTransferStatus response, bool requestProcessed)>(
                            () =>
                            {
                                MBankJsonRequestAuthorizationTransferStatus jsonRequestAuthorizationStatus = new MBankJsonRequestAuthorizationTransferStatus
                                {
                                    TranId = initPrepareResponse.response.TranId
                                };

                                return PerformRequest<MBankJsonResponseAuthorizationTransferStatus>(
                                    "api/auth/status", HttpMethod.Post,
                                    JsonConvert.SerializeObject(jsonRequestAuthorizationStatus));
                            },
                            ((MBankJsonResponseAuthorizationTransferStatus response, bool requestProcessed) authorizationStatusResponse) =>
                            {
                                if (authorizationStatusResponse.response?.AuthorizationStatusValue == MBankJsonAuthorizationTransferStatus.Cancel)
                                    return false;
                                if (authorizationStatusResponse.response.AuthorizationStatusValue == MBankJsonAuthorizationTransferStatus.Authorized)
                                    return true;

                                return null;
                            },
                            ((MBankJsonResponseAuthorizationTransferStatus response, bool requestProcessed) authorizationStatusResponse) => true,
                            null,
                            confirmText))
                            return false;

                        MBankJsonRequestExecute jsonRequestExecute = new MBankJsonRequestExecute();

                        (MBankJsonResponseExecute response, bool requestProcessed) executeResponse = PerformRequest<MBankJsonResponseExecute>(
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
            string jsonContent) where T : MBankJsonResponseBase
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
            Func<string, FileStream> fileStream)
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
