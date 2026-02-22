using BankService.BankCountry;
using BankService.ConfirmText;
using BankService.LocalTools;
using BankService.SMSCodes;
using BankService.Tax.TaxCreditorIdentifiers;
using BankService.Tax.TaxPeriods;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Tools;
using Tools.Enums;
using ToolsNugetExtensionHtmlAgilityPack;
using static BankService.Bank_PL_GetinBank.GetinBankJsonRequest;
using static BankService.Bank_PL_GetinBank.GetinBankJsonResponse;

namespace BankService.Bank_PL_GetinBank
{
    //[BankTypeAttribute(BankType.GetinBank)]
    public class GetinBank : BankPoland<GetinBankAccountData, GetinBankHistoryItem, GetinBankHistoryFilter, HtmlDocument>
    {
        const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0";
        const string fingerprint = "287f2090fccbd27edddc6ad9f06bac3e";

        private string securityToken;
        private IEnumerable<KeyValuePair<string, string>> heartbeatParameters;

        protected override int HeartbeatInterval => 10;
        protected override SMSCodeValidator SMSCodeValidator => new SMSCodeValidatorCypher(6);

        public override bool AllowAlternativeLoginMethod => false;

        public override bool TransferMandatoryRecipient => true;
        public override bool TransferMandatoryTitle => false;
        public override bool PrepaidTransferMandatoryRecipient => true;

        protected override string BaseAddress => "https://secure.getinbank.pl/";

        protected override void CleanHttpClient()
        {
            securityToken = null;
            heartbeatParameters = null;
        }

        protected override bool LoginRequest(string login, string password, List<object> additionalAuthorization)
        {
            IEnumerable<KeyValuePair<string, string>> loginLoginParameters = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("step", "1"),
                new KeyValuePair<string, string>("send", "true"),
                new KeyValuePair<string, string>("login", login),
            };

            (HtmlDocument response, bool requestProcessed) loginLoginReqest = PerformHtmlRequest(
                "index/index", HttpMethod.Post, loginLoginParameters,
                (HttpStatusCode statusCode) =>
                {
                    if (statusCode == (HttpStatusCode)269)
                    {
                        //TODO handle unblocking here (in fast transfer the same) (blocked when many times incorrect password)
                        return CheckFailed("Odblokuj dostęp na stronie internetowej");
                    }
                    return true;
                });
            if (!loginLoginReqest.requestProcessed)
                return false;

            HtmlNode formNode = loginLoginReqest.response.DocumentNode.Descendants("form").Single(n => n.Id == "signinform");
            bool isPasswordMasked = formNode.HasClass("password-mask");

            HtmlNode loginNode;
            if (isPasswordMasked)
                loginNode = formNode.Descendants("div").Single(n => n.HasClass("show_login")).Descendants("div").Single();
            else
                loginNode = formNode.Descendants("div").Single(n => n.HasClass("show_login"));
            if (loginNode.InnerText.SubstringFromEx("LOGIN: ").TrimEnd() != login)
                throw new NotSupportedException();

            IEnumerable<KeyValuePair<string, string>> setBrowserParamsParameters = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("fingerprint", fingerprint),
                new KeyValuePair<string, string>("fingerprintParams", JsonConvert.SerializeObject(new GetinBankJsonRequestBrowserFingerprint(){ userAgent = userAgent})),
            };

            PerformHtmlRequest("index/setBrowserParams", HttpMethod.Post,
                setBrowserParamsParameters,
                null);

            List<KeyValuePair<string, string>> loginPasswordParameters = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("step", "2"),
                new KeyValuePair<string, string>("send", "true"),
            };

            if (isPasswordMasked)
            {
                HtmlNode passwordNode = formNode.Descendants("div").Single(n => n.HasClass("masked-pass"));
                foreach (HtmlNode cellNode in passwordNode.Descendants("div").Where(n => n.HasClass("cell") && !n.HasClass("disabled")))
                {
                    int cellNumber = Int32.Parse(cellNode.Descendants("div").Single().InnerText);
                    string inputName = cellNode.Descendants("input").Single().GetAttributeValue("name", null);

                    if (cellNumber > password.Length)
                    {
                        return CheckFailed("Za krótkie hasło");
                    }

                    loginPasswordParameters.Add(new KeyValuePair<string, string>(inputName, password[cellNumber - 1].ToString()));
                }
            }
            else
            {
                loginPasswordParameters.Add(new KeyValuePair<string, string>("password", password));
            }

            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) loginPasswordRequest = PerformPlainRequest(
               "index/index", HttpMethod.Post,
               loginPasswordParameters);
            if (!loginPasswordRequest.requestProcessed)
                return false;

            bool success;
            if (loginPasswordRequest.responseTypeHeader.MediaType.EqualsMediaType(HttpContentMediaType.ApplicationJson))
            {
                GetinJsonResponseLoginPassword jsonResponseLoginPassword = JsonConvert.DeserializeObject<GetinJsonResponseLoginPassword>(loginPasswordRequest.responseStr);
                success = jsonResponseLoginPassword.type != 7;
            }
            else if (loginPasswordRequest.responseTypeHeader.MediaType.EqualsMediaType(HttpContentMediaType.TextHtml))
            {
                HtmlDocument confirmDocument = new HtmlDocument();
                confirmDocument.LoadHtml(loginPasswordRequest.responseStr);

                if (!AuthorizeBrowser(confirmDocument))
                    return false;

                success = true;
            }
            else
                throw new NotImplementedException();

            if (!success)
                return CheckFailed("Niepoprawny login i hasło");

            string heartbeatId = GetHeartbeatId();
            if (heartbeatId == null)
                return false;
            heartbeatParameters = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("hash", heartbeatId)
            };

            return true;
        }

        private bool AuthorizeBrowser(HtmlDocument confirmDocument)
        {
            HtmlNode confirmFormNode = confirmDocument.DocumentNode.Descendants("form").Single(n => n.Id == "signinform");
            string confirmationInformation = confirmFormNode.SingleChildNode("div", n => n.HasClass("loginStep3")).SingleChildNode("div", n => n.HasClass("jsConfirmationContainer")).SingleChildNode("div").SingleChildNode("div", n => n.HasClass("jsDeviceConfirmationPopover")).SingleChildNode("div", n => n.HasClass("information-content")).InnerText;

            bool mobileAuthorization = confirmationInformation == "Potwierdź logowanie na urządzeniu z aktywną mobilną autoryzacją";

            //TODO DeviceName
            ConfirmTextBase confirmTextAuthorization = new ConfirmTextAuthorize(null);

            if (!mobileAuthorization)
            {
                if (!SMSConfirm<bool, (GetinJsonResponseLoginConfirmation response, bool requestProcessed)>(
                    (string SMSCode) =>
                    {
                        IEnumerable<KeyValuePair<string, string>> loginConfirmationParameters = new KeyValuePair<string, string>[] {
                                new KeyValuePair<string, string>("step", "3"),
                                new KeyValuePair<string, string>("send", "true"),
                                new KeyValuePair<string, string>("token", SMSCode),
                        };
                        return PerformRequest<GetinJsonResponseLoginConfirmation>("index/index", HttpMethod.Post,
                            loginConfirmationParameters);
                    },
                    ((GetinJsonResponseLoginConfirmation response, bool requestProcessed) loginConfirmationRequest) =>
                    {
                        if (loginConfirmationRequest.response.validationMessages != null)
                            return null;
                        else
                            return true;
                    },
                    ((GetinJsonResponseLoginConfirmation response, bool requestProcessed) loginConfirmationRequest) => true,
                    null,
                    confirmTextAuthorization))
                    return false;
            }
            else
            {
                if (!ConfirmMobile(confirmTextAuthorization))
                    return false;
            }

            if (PromptYesNo("Dodać urządzenie do zarejestrowanych?"))
            {
                //TODO DeviceName
                ConfirmTextBase confirmTextAddDevice = new ConfirmTextAddDevice(null);

                (HtmlDocument response, bool requestProcessed) untrustedDeviceGetRequest = PerformHtmlRequest(
                   "announcements/untrustedDevice", HttpMethod.Get,
                   null,
                   null);
                if (!untrustedDeviceGetRequest.requestProcessed)
                    return false;

                string deviceName = untrustedDeviceGetRequest.response.Text.SubstringFromToEx("Dodaj urządzenie <b>", "</b>");
                if (String.IsNullOrEmpty(deviceName))
                {
                    Message("Niepoprawne urządzenie");
                }
                else
                {
                    IEnumerable<KeyValuePair<string, string>> untrustedDeviceAddParameters = new KeyValuePair<string, string>[] {
                        new KeyValuePair<string, string>("step", "add_device"),
                    };

                    (HtmlDocument response, bool requestProcessed) untrustedDeviceAddRequest = PerformHtmlRequest(
                       "announcements/untrustedDevice", HttpMethod.Post,
                       untrustedDeviceAddParameters,
                       null);
                    if (!untrustedDeviceAddRequest.requestProcessed)
                        return false;

                    IEnumerable<KeyValuePair<string, string>> untrustedDeviceMakeParameters = new KeyValuePair<string, string>[] {
                        new KeyValuePair<string, string>("step", "make"),
                        new KeyValuePair<string, string>("add_device_chck", "1"),
                    };

                    (GetinJsonResponseUntrustedDeviceMake response, bool requestProcessed) untrustedDeviceMakeRequest = PerformRequest<GetinJsonResponseUntrustedDeviceMake>(
                       "announcements/untrustedDevice", HttpMethod.Post,
                       untrustedDeviceMakeParameters);
                    if (!untrustedDeviceMakeRequest.requestProcessed)
                        return false;

                    mobileAuthorization = IsConfirmationMobileAuthorization(untrustedDeviceMakeRequest.response.confirmation);

                    if (!mobileAuthorization)
                    {
                        int SMSCodeNumber = Int32.Parse(untrustedDeviceMakeRequest.response.confirmation.SubstringFromToEx("SMS nr <b class=\"jsSmsNo sms-no\">", "</b>"));

                        if (!SMSConfirm<bool, (GetinJsonResponseUntrustedDeviceConfirmation response, bool requestProcessed)>(
                            (string SMSCode) =>
                            {
                                IEnumerable<KeyValuePair<string, string>> untrustedDeviceConfirmParameters = new KeyValuePair<string, string>[] {
                                        new KeyValuePair<string, string>("step", "confirm"),
                                        new KeyValuePair<string, string>("confirmation_code", untrustedDeviceMakeRequest.response.confirmation_code),
                                        new KeyValuePair<string, string>("make_step", "make"),
                                        new KeyValuePair<string, string>("token", SMSCode),
                                };

                                return PerformRequest<GetinJsonResponseUntrustedDeviceConfirmation>("announcements/untrustedDevice", HttpMethod.Post,
                                    untrustedDeviceConfirmParameters);
                            },
                            ((GetinJsonResponseUntrustedDeviceConfirmation response, bool requestProcessed) untrustedDeviceConfirmRequest) =>
                            {
                                if (!untrustedDeviceConfirmRequest.requestProcessed)
                                    return false;

                                if (untrustedDeviceConfirmRequest.response.url == "announcements/untrustedDevice/error")
                                {
                                    (HtmlDocument response, bool requestProcessed) untrustedDeviceErrorRequest = PerformHtmlRequest(
                                        "announcements/untrustedDevice/error", HttpMethod.Get,
                                        null,
                                        null);
                                    if (!untrustedDeviceErrorRequest.requestProcessed)
                                        return false;

                                    return CheckFailed(untrustedDeviceErrorRequest.response.Text.SubstringFromToEx("<span>", "</span>"));
                                }

                                if (untrustedDeviceConfirmRequest.response.validationMessages == null
                                    || !PromptYesNo("Dodać urządzenie do zarejestrowanych?", untrustedDeviceConfirmRequest.response.validationMessages.token))
                                    return true;
                                else
                                    return null;
                            },
                            ((GetinJsonResponseUntrustedDeviceConfirmation response, bool requestProcessed) untrustedDeviceConfirmRequest) => true,
                            null,
                            confirmTextAddDevice,
                            SMSCodeNumber))
                            return false;
                    }
                    else
                    {
                        if (!ConfirmMobile(confirmTextAddDevice))
                            return false;
                    }
                }
            }

            return true;
        }

        protected override bool LogoutRequest()
        {
            (HtmlDocument response, bool requestProcessed) logoutRequest = PerformHtmlRequest(
                "index/logout", HttpMethod.Get,
                null,
                null);
            return logoutRequest.requestProcessed;
        }

        protected override bool TryExtendSession()
        {
            (GetinJsonResponseHeartbeat response, bool requestProcessed) heartbeatRequest = PerformRequest<GetinJsonResponseHeartbeat>(
                "index/heartbeat", HttpMethod.Post,
                heartbeatParameters);
            if (!heartbeatRequest.requestProcessed)
                return false;
            else
            {
                if (heartbeatRequest.response.sessionExpire)
                    ExtendSession();

                return true;
            }
        }

        protected override HtmlDocument GetAccountsDetails()
        {
            (HtmlDocument response, bool requestProcessed) getHomePageRequest = PerformHtmlRequest(
                "wallet/index", HttpMethod.Post,
                null,
                null);

            return getHomePageRequest.response;
        }

        protected override List<GetinBankAccountData> GetAccountsDataMainMain(HtmlDocument accountsDetails)
        {
            //new GetinBankAccountData (){ AccCode = accountsDetails.Text.SubstringFromToEx("href=\"#transfers/index/", "/")};
            throw new NotImplementedException();
        }

        //public override AccountData GetAccountData(bool update)
        //{
        //    if (update)
        //        HomePage = GetHomePage();

        //    HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
        //    document.LoadHtml(HomePage);

        //    HtmlNode accountSection = document.DocumentNode.Descendants("div").Single(n => n.HasClass("w109") && n.Descendants("div").Single(d => d.HasClass("account-title")).Descendants("span").Single().Descendants("strong").Single().InnerText == "Konto Swobodne").ParentNode;

        //    HtmlNode accountNumberSection = accountSection.Descendants("div").Single(n => n.HasClass("w109")).Descendants("div").Single(n => n.HasClass("account-number"));
        //    string accountNumber = accountNumberSection.Descendants("div").Single().Descendants("input").Single().GetAttributeValue("value", String.Empty);

        //    HtmlNode availableFundsSection = accountSection.Descendants("p").Single(n => n.InnerText == "dostępne środki").NextSibling.NextSibling;
        //    string availableFunds = availableFundsSection.FirstChild.InnerText;

        //    return new AccountData("?", accountNumber, "?", Double.Parse(availableFunds));
        //}

        public override bool MakeTransfer(string recipient, string address, string accountNumber, string title, double amount)
        {
            string formattedAccountNumber = accountNumber.SimplifyAccountNumber();

            IEnumerable<KeyValuePair<string, string>> bankNameParameters = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("nrb", formattedAccountNumber)
            };

            (GetinJsonResponseBankName response, bool requestProcessed) getBankNameRequest = PerformRequest<GetinJsonResponseBankName>(
                "transfers/getBankName", HttpMethod.Post,
                bankNameParameters);
            if (!getBankNameRequest.requestProcessed)
                return false;

            if (getBankNameRequest.response.bank_own == 0 && String.IsNullOrEmpty(getBankNameRequest.response.bank_name))
                return CheckFailed("Niepoprawny numer konta");

            string amountToConfirm;
            string titleToConfirm;
            string transferPayment;
            string recipientAccountNumberToConfirm;
            string recipientBankNameToConfirm;
            string recipientNameToConfirm;
            string recipientAddressToConfirm;

            IEnumerable<KeyValuePair<string, string>> transferBeginParameters = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("step", "2"),
                new KeyValuePair<string, string>("accCode", SelectedAccountData.AccCode),
                new KeyValuePair<string, string>("object_name", recipient),
                new KeyValuePair<string, string>("address1", address),
                new KeyValuePair<string, string>("nrb", formattedAccountNumber),
                new KeyValuePair<string, string>("title", title),
                new KeyValuePair<string, string>("amount", amount.Display(DecimalSeparator.Dot)),
                new KeyValuePair<string, string>("currency", SelectedAccountData.Currency),
            };

            (HtmlDocument response, bool requestProcessed) transferBeginRequest = PerformHtmlRequest(
                "transfers/standard/", HttpMethod.Post,
                transferBeginParameters,
                null);
            if (!transferBeginRequest.requestProcessed)
                return false;

            int stepNumber = Int32.Parse(transferBeginRequest.response.DocumentNode.SingleChildNode("section").SingleChildNode("input").GetAttributeValue("value", String.Empty));

            if (stepNumber == 1)
                return CheckFailed("Niepoprawne dane");

            amountToConfirm = transferBeginRequest.response.DocumentNode.Descendants("div").Single(n1 => n1.HasClass("col-md-3") && n1.Descendants("div").Single(n11 => n11.HasClass("title")).InnerText == "na kwotę").Descendants("div").Single(n12 => n12.ContainsClasses(new string[] { "value", "green" })).InnerText;
            titleToConfirm = transferBeginRequest.response.DocumentNode.Descendants("div").Single(n1 => n1.HasClass("col-md-3") && n1.Descendants("div").Single(n11 => n11.HasClass("title")).InnerText == "tytułem").Descendants("div").Single(n12 => n12.ContainsClasses(new string[] { "value", "green" })).InnerText;
            transferPayment = transferBeginRequest.response.DocumentNode.Descendants("div").Single(n1 => n1.HasClass("col-md-3") && n1.Descendants("div").Single(n11 => n11.HasClass("title")).InnerText == "opłata za przelew").Descendants("div").Single(n12 => n12.ContainsClasses(new string[] { "value", "green" })).InnerText;

            HtmlNode recipientSection = transferBeginRequest.response.DocumentNode.Descendants("div").Single(n => n.HasClass("title-wide") && n.InnerText == "odbiorca").ParentNode;
            HtmlNode recipientRowSection = recipientSection.Descendants("div").Single(n => n.HasClass("row"));
            HtmlNode recipientAccountSection = recipientRowSection.Descendants("div").First();
            HtmlNode recipientNameSection = recipientAccountSection.NextSibling.NextSibling;

            recipientAccountNumberToConfirm = recipientAccountSection.Descendants("div").Single(n => n.HasClass("value")).InnerText;
            recipientBankNameToConfirm = recipientAccountSection.Descendants("div").Single(n => n.HasClass("subtext")).InnerText;
            recipientNameToConfirm = recipientNameSection.Descendants("div").Single(n => n.HasClass("value")).InnerText;
            recipientAddressToConfirm = recipientNameSection.Descendants("div").Single(n => n.HasClass("subtext")).InnerText;

            if (amountToConfirm.Replace(" ", String.Empty) != $"{amount.Display(DecimalSeparator.Dot)}{SelectedAccountData.Currency}")
                throw new NotSupportedException();
            if (titleToConfirm != title && !String.IsNullOrEmpty(title))
                Message($"Changed title: {titleToConfirm}");
            if (DoubleOperations.Parse(transferPayment.SubstringToEx($" {SelectedAccountData.Currency}")) != 0)
                Message($"Transfer payment: {transferPayment}");
            if (!AccountNumberTools.CompareAccountNumbers(recipientAccountNumberToConfirm, formattedAccountNumber))
                throw new NotSupportedException();
            if (recipientNameToConfirm != recipient)
                Message($"Changed recipient name: {recipientNameToConfirm}");
            if (recipientAddressToConfirm != address)
                Message($"Changed address: {recipientAddressToConfirm}");

            IEnumerable<KeyValuePair<string, string>> transferMakeParameters = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("step", "make"),
            };

            (GetinJsonResponseTransferMake response, bool requestProcessed) transferMakeRequest = PerformRequest<GetinJsonResponseTransferMake>(
                "transfers/standard/", HttpMethod.Post,
                transferMakeParameters);
            if (!transferMakeRequest.requestProcessed)
                return false;

            if (!transferMakeRequest.response.allow)
                throw new NotSupportedException();

            bool mobileAuthorization = IsConfirmationMobileAuthorization(transferMakeRequest.response.confirmation);

            ConfirmTextBase confirmText = new ConfirmTextTransfer(amount, SelectedAccountData.Currency, recipientBankNameToConfirm, recipientAccountNumberToConfirm);

            if (!mobileAuthorization)
            {
                int SMSCodeNumber = Int32.Parse(transferMakeRequest.response.confirmation.SubstringFromToEx("SMS nr <b class=\"jsSmsNo sms-no\">", "</b>"));

                return SMSConfirm<bool, (GetinJsonResponseTransferConfirm response, bool requestProcessed)>(
                    (string SMSCode) =>
                    {
                        IEnumerable<KeyValuePair<string, string>> transferConfirmParameters = new KeyValuePair<string, string>[] {
                            new KeyValuePair<string, string>("step", "confirm"),
                            new KeyValuePair<string, string>("token", SMSCode),
                        };
                        return PerformRequest<GetinJsonResponseTransferConfirm>("transfers/standard/", HttpMethod.Post,
                            transferConfirmParameters);
                    },
                    ((GetinJsonResponseTransferConfirm response, bool requestProcessed) transferConfirmRequest) =>
                    {
                        if (!transferConfirmRequest.requestProcessed)
                            return false;

                        if (!transferConfirmRequest.response.confirmed)
                            return null;
                        else
                            return true;
                    },
                    ((GetinJsonResponseTransferConfirm response, bool requestProcessed) transferConfirmRequest) => true,
                    null,
                    confirmText,
                    SMSCodeNumber);
            }
            else
                return ConfirmMobile(confirmText);
        }

        public override bool MakeTaxTransfer(string taxType, string accountNumber, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier, string creditorName, string obligationId, double amount)
        {
            throw new NotImplementedException();
        }

        protected override string CleanFastTransferUrl(string transferId)
        {
            string newTransferId = transferId
                .Replace("https://", String.Empty)

                .Replace("secure.getinbank.pl/pbl/payment/new/", String.Empty)
                .Replace("secure.getinbank.pl/pbl/#index/index/", String.Empty);
            //.Replace("secure.getinbank.pl/pa/#index/index/", String.Empty);

            if (newTransferId.Length != 64)
            {
                return null;
            }

            return newTransferId;
        }

        protected override bool LoginRequestForFastTransfer(string login, string password, List<object> additionalAuthorization, string transferId)
        {
            IEnumerable<KeyValuePair<string, string>> loginLoginParameters = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("step", "1"),
                new KeyValuePair<string, string>("send", "true"),
                new KeyValuePair<string, string>("login", login),
            };

            (HtmlDocument response, bool requestProcessed) loginLoginRequest = PerformHtmlRequest(
                $"pbl/index/index/{transferId}/0", HttpMethod.Post, loginLoginParameters,
                (HttpStatusCode statusCode) =>
                {
                    if (statusCode == (HttpStatusCode)269)
                    {
                        return CheckFailed("Odblokuj dostęp na stronie internetowej");
                    }
                    return true;
                });
            if (!loginLoginRequest.requestProcessed)
                return false;

            if (loginLoginRequest.response.Text.SubstringFromToEx(new string[] { "LOGIN: " }, new string[] { " ", "<" }) != login)
                throw new NotSupportedException();

            IEnumerable<KeyValuePair<string, string>> loginPasswordParameters = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("step", "2"),
                new KeyValuePair<string, string>("send", "true"),
                new KeyValuePair<string, string>("password", password),
            };

            (GetinJsonResponseLoginPassword response, bool requestProcessed) loginPasswordRequest = PerformRequest<GetinJsonResponseLoginPassword>(
                $"pbl/index/index/{transferId}/0", HttpMethod.Post,
                loginPasswordParameters);
            if (!loginPasswordRequest.requestProcessed)
                return false;

            bool success = loginPasswordRequest.response.type != 7;

            if (!success)
            {
                return CheckFailed("Niepoprawny login i hasło");
            }

            return true;
        }

        protected override string MakeFastTransfer(string transferId)
        {
            (HtmlDocument response, bool requestProcessed) getTransferRequest = PerformHtmlRequest(
                $"pbl/transfers/index/{transferId}", HttpMethod.Get,
                null,
                null);
            if (!getTransferRequest.requestProcessed)
                return null;

            string codeProduct;
            string recipientNameToConfirm;
            try
            {
                codeProduct = getTransferRequest.response.DocumentNode.Descendants().First(n => n.GetAttributeValue("data-code", null) != null).GetAttributeValue("data-code", null);
                recipientNameToConfirm = getTransferRequest.response.DocumentNode.Descendants("div").Single(n => n.HasClass("transfer-details-recipient")).FirstChildNode("div").NextSibling.NextSibling.SingleChildNode("dl").ChildNodes.Single(n => n.Name == "dt").InnerText;
            }
            catch (InvalidOperationException)
            {
                string message = getTransferRequest.response.DocumentNode.Descendants().Single(n => n.HasClass("popUp-content")).SingleChildNode("p").InnerText;

                CheckFailed(message);
                return null;
            }

            IEnumerable<KeyValuePair<string, string>> transferMakeParameters = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("step", "make"),
                new KeyValuePair<string, string>("code_product", codeProduct),
            };

            (GetinJsonResponseTransferMake response, bool requestProcessed) transferMakeRequest = PerformRequest<GetinJsonResponseTransferMake>(
                $"pbl/transfers/standard", HttpMethod.Post,
                transferMakeParameters);
            if (!transferMakeRequest.requestProcessed)
                return null;

            if (!transferMakeRequest.response.allow)
                throw new NotSupportedException();

            bool mobileAuthorization = IsConfirmationMobileAuthorization(transferMakeRequest.response.confirmation);

            //TODO amount, currency
            ConfirmTextBase confirmText = new ConfirmTextFastTransfer(0, SelectedAccountData.Currency, recipientNameToConfirm);

            if (!mobileAuthorization)
            {
                int SMSCodeNumber = Int32.Parse(transferMakeRequest.response.confirmation.SubstringFromToEx("SMS nr <b class=\"jsSmsNo sms-no\">", "</b>"));

                if (!SMSConfirm<bool, (GetinJsonResponseTransferConfirm response, bool requestProcessed)>(
                    (string SMSCode) =>
                    {
                        IEnumerable<KeyValuePair<string, string>> transferConfirmParameters = new KeyValuePair<string, string>[] {
                            new KeyValuePair<string, string>("step", "confirm"),
                            new KeyValuePair<string, string>("code_product", codeProduct),
                            new KeyValuePair<string, string>("token", SMSCode),
                        };
                        return PerformRequest<GetinJsonResponseTransferConfirm>($"pbl/transfers/standard", HttpMethod.Post,
                            transferConfirmParameters);
                    },
                    ((GetinJsonResponseTransferConfirm response, bool requestProcessed) transferConfirmRequest) =>
                    {
                        if (!transferConfirmRequest.requestProcessed)
                            return false;

                        if (!transferConfirmRequest.response.confirmed)
                            return null;
                        else
                            return true;
                    },
                    ((GetinJsonResponseTransferConfirm response, bool requestProcessed) transferConfirmRequest) => true,
                    null,
                    confirmText,
                    SMSCodeNumber))
                    return null;
            }
            else
            {
                //TODO does mobile authorization work
                if (!ConfirmMobile(confirmText))
                    return null;
            }

            (HtmlDocument response, bool requestProcessed) transferSuccessRequest = PerformHtmlRequest(
                $"pbl/transfers/standard/success", HttpMethod.Get,
                null,
                null);

            string redirectAddress = transferSuccessRequest.response.DocumentNode.SingleChildNode("form").GetAttributeValue("action", null);
            redirectAddress = redirectAddress.NormalizeHtmlCharactersEx();

            return redirectAddress;
        }

        protected override bool MakePrepaidTransferMain(string recipient, string phoneNumber, double amount)
        {
            (HtmlDocument response, bool requestProcessed) prepaidRequest = PerformHtmlRequest(
                "transfers/prepaid", HttpMethod.Get,
                null,
                null);
            if (!prepaidRequest.requestProcessed)
                return false;

            HtmlNode operatorsNode = prepaidRequest.response.DocumentNode.Descendants("div").Single(n => n.Id == "operators_list_content").Descendants("ul").Single();
            IEnumerable<(string name, string code)> operators = operatorsNode.Descendants("li").Select(n => (n.InnerText, n.GetAttributeValue("data-value", String.Empty)));

            (string name, string data) operatorItem = PromptComboBox<string>("Operator", operators.Select(o => new SelectComboBoxItem<string>(o.name, o.code)), false);
            if (operatorItem.data == null)
                return false;

            string operatorCode = operatorItem.data;

            string amountToConfirm;
            string phoneNumberToConfirm;
            string operatorNameToConfirm;
            string recipientNameToConfirm;

            IEnumerable<KeyValuePair<string, string>> transferBeginParameters = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("step", "2"),
                new KeyValuePair<string, string>("accCode", SelectedAccountData.AccCode),
                new KeyValuePair<string, string>("object_name", recipient),
                new KeyValuePair<string, string>("phone_number", phoneNumber.SimplifyPhoneNumber()),
                new KeyValuePair<string, string>("operator", operatorCode),
                new KeyValuePair<string, string>("amount", amount.Display(DecimalSeparator.Dot)),
                new KeyValuePair<string, string>("accept-term", "on"),
            };

            (HtmlDocument response, bool requestProcessed) transferBeginRequest = PerformHtmlRequest(
                "transfers/prepaid/", HttpMethod.Post,
                transferBeginParameters,
                null);
            if (!transferBeginRequest.requestProcessed)
                return false;

            int stepNumber = Int32.Parse(transferBeginRequest.response.DocumentNode.SingleChildNode("section").SingleChildNode("input").GetAttributeValue("value", String.Empty));

            if (stepNumber == 1)
            {
                StringBuilder errorMessage = new StringBuilder();

                foreach (HtmlNode errorNode in transferBeginRequest.response.DocumentNode.Descendants("div").Where(n => n.HasClass("cloud")))
                {
                    if (errorMessage == null)
                        errorMessage = new StringBuilder();
                    errorMessage.AppendLine(errorNode.InnerText);
                }
                //https://secure.getinbank.pl/transfers/prepaidAmounts

                return CheckFailed(errorMessage != null ? errorMessage.ToString() : "Niepoprawne dane");
            }

            HtmlNode transferDetailsInfoSection = transferBeginRequest.response.DocumentNode.Descendants("div").Single(n => n.HasClass("transfer-details-info"));
            HtmlNode transferDetailsSection = transferBeginRequest.response.DocumentNode.Descendants("div").Single(n => n.HasClass("transfer-details") && !n.HasClass("from"));

            amountToConfirm = transferDetailsInfoSection.Descendants("dl").Single(n1 => n1.Descendants("dd").Single().InnerText == "na kwotę").Descendants("dt").Single().InnerText;
            phoneNumberToConfirm = transferDetailsSection.Descendants("dl").First().Descendants("dt").Single().InnerText;
            operatorNameToConfirm = transferDetailsSection.Descendants("dl").First().Descendants("dd").Single().InnerText;
            recipientNameToConfirm = transferDetailsSection.Descendants("dl").First().NextSibling.NextSibling.Descendants("dt").Single().InnerText;

            if (amountToConfirm.Replace(" ", String.Empty) != $"{amount.Display(DecimalSeparator.Comma)}{SelectedAccountData.Currency}")
                throw new NotSupportedException();
            if (phoneNumberToConfirm != phoneNumber)
                Message($"Changed phone number: {phoneNumberToConfirm}");
            if (recipientNameToConfirm != recipient)
                Message($"Changed recipient name: {recipientNameToConfirm}");
            if (operatorNameToConfirm != operatorItem.name)
                Message($"Changed operator: {operatorNameToConfirm}");

            int SMSCodeNumber;

            IEnumerable<KeyValuePair<string, string>> transferMakeParameters = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("step", "make"),
            };

            (GetinJsonResponseTransferMake response, bool requestProcessed) transferMakeRequest = PerformRequest<GetinJsonResponseTransferMake>(
                "transfers/prepaid/", HttpMethod.Post,
                transferMakeParameters);
            if (!transferMakeRequest.requestProcessed)
                return false;

            if (!transferMakeRequest.response.allow)
                throw new NotSupportedException();

            bool mobileAuthorization = IsConfirmationMobileAuthorization(transferMakeRequest.response.confirmation);

            ConfirmTextBase confirmText = new ConfirmTextPrepaidTransfer(amount, SelectedAccountData.Currency, operatorItem.name, phoneNumberToConfirm);

            if (!mobileAuthorization)
            {
                SMSCodeNumber = Int32.Parse(transferMakeRequest.response.confirmation.SubstringFromToEx("SMS nr <b class=\"jsSmsNo sms-no\">", "</b>"));

                return SMSConfirm<bool, (GetinJsonResponseTransferConfirm response, bool requestProcessed)>(
                    (string SMSCode) =>
                    {
                        IEnumerable<KeyValuePair<string, string>> transferConfirmParameters = new KeyValuePair<string, string>[] {
                            new KeyValuePair<string, string>("step", "confirm"),
                            new KeyValuePair<string, string>("token", SMSCode),
                        };
                        return PerformRequest<GetinJsonResponseTransferConfirm>("transfers/prepaid/", HttpMethod.Post,
                            transferConfirmParameters);
                    },
                    ((GetinJsonResponseTransferConfirm response, bool requestProcessed) transferConfirmRequest) =>
                    {
                        if (!transferConfirmRequest.requestProcessed)
                            return false;

                        if (!transferConfirmRequest.response.confirmed)
                            return null;
                        else
                            return true;
                    },
                    ((GetinJsonResponseTransferConfirm response, bool requestProcessed) transferConfirmRequest) => true,
                    null,
                    confirmText,
                    SMSCodeNumber);
            }
            else
                return ConfirmMobile(confirmText);
        }

        protected override GetinBankHistoryFilter CreateFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact)
        {
            return new GetinBankHistoryFilter(direction, title, dateFrom, dateTo, amountExact);
        }

        protected override List<GetinBankHistoryItem> GetHistoryItems(GetinBankHistoryFilter filter = null)
        {
            (HtmlDocument response, bool requestProcessed) clearTitleFilterRequest = PerformHtmlRequest(
                "history/setTitleFilter", HttpMethod.Post,
                HttpOperations.EmptyParameters,
                null);
            if (!clearTitleFilterRequest.requestProcessed)
                return null;

            (HtmlDocument response, bool requestProcessed) clearFiltersRequest = PerformHtmlRequest(
                "history/setFilters", HttpMethod.Post,
                HttpOperations.EmptyParameters,
                null);
            if (!clearFiltersRequest.requestProcessed)
                return null;

            List<GetinBankHistoryItem> result = new List<GetinBankHistoryItem>();

            (HtmlDocument response, bool requestProcessed) getHistoryRequest = PerformHtmlRequest(
                "history/index", HttpMethod.Get,
                null,
                null);
            if (!getHistoryRequest.requestProcessed)
                return null;

            if (filter != null)
            {
                if (filter.FindAccountNumber)
                {
                    List<GetinBankHistoryItem> emptyOperations = GetinBankAcountNumbersHistory.Download(this);

                    Dictionary<(DateTime dateFrom, DateTime dateTo), List<string>> ranges = new Dictionary<(DateTime dateFrom, DateTime dateTo), List<string>>();
                    foreach (DictionaryEntry entry in Properties.Settings.Default.GetinBankAcountNumbers)
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

                    //removed because operations from today were duplicated
                    //List<HistoryItem> notSavedItems = GetHistory(new GetinFilter()
                    //{
                    //    DateFrom = Properties.Settings.Default.AcountNumbersDownloadDate,
                    //    ChannelType = FilterChannel.Bank,
                    //    AmountExact = filter.AmountExact,
                    //    AmountFrom = filter.AmountFrom,
                    //    AmountTo = filter.AmountTo,
                    //    OperationType = filter.OperationType,
                    //    //StatusType=
                    //    Title = filter.Title
                    //});
                    //if (notSavedItems != null)
                    //    result.AddRange(notSavedItems.Where(i => AccountNumberTools.CompareAccountNumbers(i.direction == OperationDirection.Execute ? i.toAccountNumber : i.fromAccountNumber, filter.AccountNumber)));
                    if (emptyOperations != null)
                        result.AddRange(emptyOperations.Where(i => AccountNumberTools.CompareAccountNumbers(i.Direction == OperationDirection.Execute ? i.ToAccountNumber : i.FromAccountNumber, filter.AccountNumber)));

                    foreach (KeyValuePair<(DateTime dateFrom, DateTime dateTo), List<string>> range in ranges.OrderByDescending(r => r.Key.dateFrom))
                    {
                        //result.AddRange(new List<HistoryItem>().Where(i => range.Value.Contains(i.referenceNumber)));
                        List<GetinBankHistoryItem> items = GetHistoryItems(new GetinBankHistoryFilter()
                        {
                            DateFrom = range.Key.dateFrom,
                            DateTo = range.Key.dateTo,
                            ChannelType = GetinBankFilterChannel.Bank,
                            AmountExact = filter.AmountExact,
                            AmountFrom = filter.AmountFrom,
                            AmountTo = filter.AmountTo,
                            OperationType = filter.OperationType,
                            //StatusType=
                            Title = filter.Title
                        });
                        if (items != null)
                            result.AddRange(items.Where(i => range.Value.Contains(i.ReferenceNumber)));
                    }
                }
                else
                {
                    if (filter.SetTitle)
                    {
                        (HtmlDocument response, bool requestProcessed) setTitleFilterRequest = PerformHtmlRequest(
                            "history/setTitleFilter", HttpMethod.Post,
                            filter.CreateTitleParameters(),
                            null);
                        if (!setTitleFilterRequest.requestProcessed)
                            return null;
                    }

                    if (filter.SetDetails)
                    {
                        (HtmlDocument response, bool requestProcessed) setFiltersRequest = PerformHtmlRequest(
                            "history/setFilters", HttpMethod.Post,
                            filter.CreateDetailsParameters(),
                            null);
                        if (!setFiltersRequest.requestProcessed)
                            return null;
                    }

                    //TODO page count fetch from document
                    int? pageCounter = null;
                    for (int page = 1; (pageCounter == null || page <= pageCounter) && (filter.CounterLimit == 0 || result.Count < filter.CounterLimit); page++)
                    {
                        bool success = false;

                        while (!success)
                        {
                            (HtmlDocument response, bool requestProcessed) loadHistoryRequest = PerformHtmlRequest(
                                $"history/loadHistory/{page}", HttpMethod.Get,
                                null,
                                null);
                            if (!loadHistoryRequest.requestProcessed)
                                return null;

                            if (loadHistoryRequest.response.DocumentNode.Descendants("section").SingleOrDefault(n => n.HasClass("empty-section")) != null)
                            {
                                pageCounter = 0;
                                break;
                            }

                            pageCounter = Int32.Parse(loadHistoryRequest.response.DocumentNode.Descendants("input").Single(n => n.Id == "pages").GetAttributeValue("value", String.Empty));

                            result.AddRange(loadHistoryRequest.response.DocumentNode.Descendants("li").Select(n => new GetinBankHistoryItem(n)));
                            success = true;
                        }
                    }
                }
            }

            return result;
        }

        protected override bool GetDetailsFileMain(GetinBankHistoryItem item, Func<ContentDispositionHeaderValue, FileStream> file)
        {
            PerformFileRequest($"history/printDetails/{item.Id}", HttpMethod.Get, file);

            return true;
        }

        private bool ConfirmMobile(ConfirmTextBase confirmText)
        {
            return MobileConfirm<bool, GetinJsonResponseMobileConfirmation>(
                () =>
                {
                    (GetinJsonResponseMobileConfirmation response, bool requestProcessed) confirmationCheckRequest = PerformRequest<GetinJsonResponseMobileConfirmation>(
                        "index/confirmationCheck", HttpMethod.Post,
                        null);
                    return confirmationCheckRequest.response;
                },
                (GetinJsonResponseMobileConfirmation jsonResponseMobileConfirmation) =>
                {
                    if (jsonResponseMobileConfirmation.confirmed)
                        return true;

                    return null;
                },
                (GetinJsonResponseMobileConfirmation jsonResponseMobileConfirmation) => true,
                null,
                confirmText);
        }

        public string GetHeartbeatId()
        {
            (GetinJsonResponseHash response, bool requestProcessed) getHeartbeatIdRequest = PerformRequest<GetinJsonResponseHash>(
                "layout/index/do", HttpMethod.Get,
                null);
            if (!getHeartbeatIdRequest.requestProcessed)
                return null;

            return getHeartbeatIdRequest.response.hash;
        }

        public void ExtendSession()
        {
            GetAccountsDetails();
        }

        private (string, MediaTypeHeaderValue, bool) PerformPlainRequest(string requestUri, HttpMethod method,
            IEnumerable<KeyValuePair<string, string>> parameters)
        {
            try
            {
                using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, parameters))
                using (HttpResponseMessage response = HttpOperations.GetResponse(Client, request))
                {
                    ((string responseStrValue, MediaTypeHeaderValue contentType), bool) responseResult = ProcessResponse<(string, MediaTypeHeaderValue)>(response,
                        (string responseStr) =>
                        {
                            if (!CheckNotExpired(responseStr))
                            {
                                NoteExpiredSession();
                                return false;
                            }
                            return true;
                        },
                        (string responseStr) => ((responseStr, response.Content.Headers.ContentType)));

                    return (responseResult.Item1.responseStrValue, responseResult.Item1.contentType, responseResult.Item2);
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
                                return default;
                            }
                        }
                    }
                }

                throw;
            }
        }

        private (HtmlDocument, bool) PerformHtmlRequest(string requestUri, HttpMethod method,
            IEnumerable<KeyValuePair<string, string>> parameters,
            Func<HttpStatusCode, bool> statusCodeAction)
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, parameters))
            using (HttpResponseMessage response = HttpOperations.GetResponse(Client, request))
            {
                return ProcessResponse<HtmlDocument>(response,
                    (string responseStr) =>
                    {
                        AssertMediaType(response.Content.Headers.ContentType, HttpContentMediaType.TextHtml);

                        if (statusCodeAction != null)
                        {
                            if (!statusCodeAction.Invoke(response.StatusCode))
                                return false;
                        }
                        if (!CheckNotExpired(responseStr))
                        {
                            NoteExpiredSession();
                            return false;
                        }
                        return true;
                    },
                    (string responseStr) =>
                    {
                        HtmlDocument document = new HtmlDocument();
                        document.LoadHtml(responseStr);
                        return (document);
                    });
            }
        }

        private (T, bool) PerformRequest<T>(string requestUri, HttpMethod method,
            IEnumerable<KeyValuePair<string, string>> parameters) where T : GetinJsonResponseBase
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, parameters))
            using (HttpResponseMessage response = HttpOperations.GetResponse(Client, request))
            {
                return ProcessResponse<T>(response,
                    (string responseStr) =>
                    {
                        AssertMediaType(response.Content.Headers.ContentType, HttpContentMediaType.ApplicationJson);

                        if (!CheckNotExpired(responseStr))
                        {
                            NoteExpiredSession();
                            return false;
                        }
                        return true;
                    },
                    (string responseStr) => (JsonConvert.DeserializeObject<T>(responseStr)));
            }
        }

        private void PerformFileRequest(string requestUri, HttpMethod method,
            Func<ContentDispositionHeaderValue, FileStream> fileStream)
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, null))
                ProcessFileStream(request, fileStream);
        }

        private HttpRequestMessage CreateHttpRequestMessage(string requestUri, HttpMethod method, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            List<(string name, string value)> headers = new List<(string name, string value)>();
            headers.Add(("User-Agent", userAgent));
            headers.Add(("X-Requested-With", "XMLHttpRequest"));

            return HttpOperations.CreateHttpRequestMessageForm(method, requestUri, parameters, headers);
        }

        private void RefreshCsrfToken()
        {
            Int64 appStartTime = Convert.ToInt64(Math.Truncate(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).Subtract(new TimeSpan(2, 0, 0)).TotalMilliseconds));
            securityToken = StringEncryptor.CreateMD5(appStartTime.ToString()).ToLower();
        }

        private static bool IsConfirmationMobileAuthorization(string confirmationContent)
        {
            HtmlAgilityPack.HtmlDocument untrustedDeviceMakeDocument = new HtmlAgilityPack.HtmlDocument();
            untrustedDeviceMakeDocument.LoadHtml(confirmationContent);
            HtmlNode untrustedDeviceMakeMainNode = untrustedDeviceMakeDocument.DocumentNode.SingleChildNode("div");
            return untrustedDeviceMakeMainNode.HasClass("confirmation-mobile");
        }

        private bool CheckNotExpired(string responseStr)
        {
            try
            {
                GetinJsonResponseExpired jsonResponseExpired = JsonConvert.DeserializeObject<GetinJsonResponseExpired>(responseStr);
                if (jsonResponseExpired != null && (jsonResponseExpired.clear && jsonResponseExpired.type == 5))
                {
                    return CheckFailed("Sesja wygasła");
                }
            }
            catch (JsonReaderException)
            {
            }
            return true;
        }

        public static DateTime GetDateFromReferenceNumber(string referenceNumber)
        {
            string dateString = referenceNumber.SubstringToEx("/");
            return new DateTime(Int32.Parse(dateString.Substring(0, 4)), Int32.Parse(dateString.Substring(4, 2)), Int32.Parse(dateString.Substring(6, 2)));
        }
    }
}
