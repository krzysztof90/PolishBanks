using HtmlAgilityPack;
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
using static BankService.Bank_GetinBank.GetinBankBrowserJsonRequest;
using BankService.LocalTools;

namespace BankService.Bank_GetinBank
{
    [BankTypeAttribute(BankType.GetinBank)]
    public class GetinBank : BankBase<GetinBankHistoryItem, GetinBankHistoryFilter>
    {
        const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0";
        const string fingerprint = "287f2090fccbd27edddc6ad9f06bac3e";

        private string homePage;
        private string HomePage
        {
            get => homePage ?? (homePage = GetHomePage());
            set
            {
                homePage = value;
                if (homePage == null)
                    CallAvailableFundsClear();
            }
        }

        private IEnumerable<KeyValuePair<string, string>> heartbeatParameters;

        protected override int HeartbeatInterval => 10;

        private string securityToken;

        public override bool FastTransferMandatoryTransferId => true;
        public override bool FastTransferMandatoryBrowserCookies => false;
        public override bool FastTransferMandatoryCookie => false;
        public override bool TransferMandatoryTitle => false;
        public override bool PrepaidTransferMandatoryRecipient => true;

        protected override string BaseAddress => "https://secure.getinbank.pl/";

        protected override bool LoginRequest(string login, string password)
        {
            IEnumerable<KeyValuePair<string, string>> loginLoginParameters = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("step", "1"),
                new KeyValuePair<string, string>("send", "true"),
                new KeyValuePair<string, string>("login", login),
            };

            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) loginLoginReqest = PerformRequest(
                "index/index", HttpMethod.Post, loginLoginParameters,
                (HttpStatusCode statusCode) =>
                {
                    if (statusCode == (HttpStatusCode)269)
                    {
                        //TODO obsłużyć odblokowanie tutaj (w fast transfer to samo) (zablokowane kiedy kilka razy niepoprawne hasło)
                        return CheckFailed("Odblokuj dostęp na stronie internetowej");
                    }
                    return true;
                },
                true, null);

            if (!loginLoginReqest.requestProcessed)
                //TODO wyświetlić wiadomość
                return false;
            AssertMediaType(loginLoginReqest.responseTypeHeader, "text/html");

            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(loginLoginReqest.responseStr);
            HtmlNode formNode = document.DocumentNode.Descendants("form").Single(n => n.Id == "signinform");
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
                new KeyValuePair<string, string>("fingerprintParams", JsonConvert.SerializeObject(new GetinBankBrowserJsonRequestFingerprint(){userAgent = userAgent})),
            };

            PerformRequest("index/setBrowserParams", HttpMethod.Post, setBrowserParamsParameters, null, true, null);

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

            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) loginPasswordRequest = PerformRequest(
               "index/index", HttpMethod.Post, loginPasswordParameters, null, true, null);

            if (!loginPasswordRequest.requestProcessed)
                return false;

            bool success;
            if (loginPasswordRequest.responseTypeHeader.MediaType == "application/json")
            {
                GetinJsonLoginPassword jsonResponseLoginPassword = JsonConvert.DeserializeObject<GetinJsonLoginPassword>(loginPasswordRequest.responseStr);
                success = jsonResponseLoginPassword.type != 7;
            }
            else if (loginPasswordRequest.responseTypeHeader.MediaType == "text/html")
            {
                HtmlAgilityPack.HtmlDocument confirmDocument = new HtmlAgilityPack.HtmlDocument();
                confirmDocument.LoadHtml(loginPasswordRequest.responseStr);
                HtmlNode confirmFormNode = confirmDocument.DocumentNode.Descendants("form").Single(n => n.Id == "signinform");
                string confirmationInformation = confirmFormNode.SingleChildNode("div", n => n.HasClass("loginStep3")).SingleChildNode("div", n => n.HasClass("jsConfirmationContainer")).SingleChildNode("div").SingleChildNode("div", n => n.HasClass("jsDeviceConfirmationPopover")).SingleChildNode("div", n => n.HasClass("information-content")).InnerText;

                bool mobileAuthorization = confirmationInformation == "Potwierdź logowanie na urządzeniu z aktywną mobilną autoryzacją";

                bool loginConfirmed = false;
                if (!mobileAuthorization)
                {
                    while (!loginConfirmed)
                    {
                        string SMSCodeLoginConfirm = GetSMSCode("Kod SMS");
                        if (SMSCodeLoginConfirm == null)
                            return false;

                        IEnumerable<KeyValuePair<string, string>> loginConfirmationParameters = new KeyValuePair<string, string>[] {
                            new KeyValuePair<string, string>("step", "3"),
                            new KeyValuePair<string, string>("send", "true"),
                            new KeyValuePair<string, string>("token", SMSCodeLoginConfirm),
                        };

                        (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) loginConfirmationRequest = PerformRequest(
                           "index/index", HttpMethod.Post, loginConfirmationParameters, null, true, null);

                        AssertMediaType(loginConfirmationRequest.responseTypeHeader, "application/json");

                        GetinJsonLoginConfirmation jsonResponseLoginConfirmation = JsonConvert.DeserializeObject<GetinJsonLoginConfirmation>(loginConfirmationRequest.responseStr);
                        if (jsonResponseLoginConfirmation.validationMessages != null)
                            Message(jsonResponseLoginConfirmation.validationMessages.token);
                        else
                            loginConfirmed = true;
                    }
                }
                else
                {
                    if (!ConfirmMobile())
                        return false;

                    loginConfirmed = true;
                }

                if (PromptYesNo("Dodać urządzenie do zarejestrowanych?"))
                {
                    (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) untrustedDeviceGetRequest = PerformRequest(
                       "announcements/untrustedDevice", HttpMethod.Get, null, null, true, null);

                    if (!untrustedDeviceGetRequest.requestProcessed)
                        return false;
                    AssertMediaType(untrustedDeviceGetRequest.responseTypeHeader, "text/html");

                    string deviceName = untrustedDeviceGetRequest.responseStr.SubstringFromToEx("Dodaj urządzenie <b>", "</b>");
                    if (String.IsNullOrEmpty(deviceName))
                    {
                        Message("Niepoprawne urządzenie");
                    }
                    else
                    {
                        IEnumerable<KeyValuePair<string, string>> untrustedDeviceAddParameters = new KeyValuePair<string, string>[] {
                                new KeyValuePair<string, string>("step", "add_device"),
                            };

                        (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) untrustedDeviceAddRequest = PerformRequest(
                           "announcements/untrustedDevice", HttpMethod.Post, untrustedDeviceAddParameters, null, true, null);

                        if (!untrustedDeviceAddRequest.requestProcessed)
                            return false;

                        IEnumerable<KeyValuePair<string, string>> untrustedDeviceMakeParameters = new KeyValuePair<string, string>[] {
                                new KeyValuePair<string, string>("step", "make"),
                                new KeyValuePair<string, string>("add_device_chck", "1"),
                            };

                        (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) untrustedDeviceMakeRequest = PerformRequest(
                           "announcements/untrustedDevice", HttpMethod.Post, untrustedDeviceMakeParameters, null, true, null);

                        if (!untrustedDeviceMakeRequest.requestProcessed)
                            return false;
                        AssertMediaType(untrustedDeviceMakeRequest.responseTypeHeader, "application/json");

                        GetinJsonUntrustedDeviceMake jsonResponseUntrustedDeviceMake = JsonConvert.DeserializeObject<GetinJsonUntrustedDeviceMake>(untrustedDeviceMakeRequest.responseStr);

                        mobileAuthorization = IsConfirmationMobileAuthorization(jsonResponseUntrustedDeviceMake.confirmation);

                        if (!mobileAuthorization)
                        {
                            int SMSCodeNumber = Int32.Parse(jsonResponseUntrustedDeviceMake.confirmation.SubstringFromToEx("SMS nr <b class=\"jsSmsNo sms-no\">", "</b>"));

                            bool deviceConfirmed = false;
                            while (!deviceConfirmed)
                            {
                                string SMSCodeDeviceConfirm = GetSMSCode($"Kod SMS nr {SMSCodeNumber}");
                                if (SMSCodeDeviceConfirm != null)
                                {
                                    IEnumerable<KeyValuePair<string, string>> untrustedDeviceConfirmParameters = new KeyValuePair<string, string>[] {
                                            new KeyValuePair<string, string>("step", "confirm"),
                                            new KeyValuePair<string, string>("confirmation_code", jsonResponseUntrustedDeviceMake.confirmation_code),
                                            new KeyValuePair<string, string>("make_step", "make"),
                                            new KeyValuePair<string, string>("token", SMSCodeDeviceConfirm),
                                        };

                                    (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) untrustedDeviceConfirmRequest = PerformRequest(
                                        "announcements/untrustedDevice", HttpMethod.Post, untrustedDeviceConfirmParameters, null, true, null);

                                    if (!untrustedDeviceConfirmRequest.requestProcessed)
                                        return false;
                                    AssertMediaType(untrustedDeviceConfirmRequest.responseTypeHeader, "application/json");

                                    GetinJsonUntrustedDeviceConfirmation jsonResponseUntrustedDeviceConfirmation = JsonConvert.DeserializeObject<GetinJsonUntrustedDeviceConfirmation>(untrustedDeviceConfirmRequest.responseStr);

                                    if (jsonResponseUntrustedDeviceConfirmation.url == "announcements/untrustedDevice/error")
                                    {
                                        (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) untrustedDeviceErrorRequest = PerformRequest(
                                            "announcements/untrustedDevice/error", HttpMethod.Get, null, null, true, null);

                                        if (!untrustedDeviceErrorRequest.requestProcessed)
                                            return false;
                                        AssertMediaType(untrustedDeviceErrorRequest.responseTypeHeader, "text/html");

                                        Message(untrustedDeviceErrorRequest.responseStr.SubstringFromToEx("<span>", "</span>"));
                                        break;
                                    }

                                    if (jsonResponseUntrustedDeviceConfirmation.validationMessages == null
                                        || !PromptYesNo("Dodać urządzenie do zarejestrowanych?", jsonResponseUntrustedDeviceConfirmation.validationMessages.token))
                                        deviceConfirmed = true;
                                }
                            }
                        }
                        else
                        {
                            if (!ConfirmMobile())
                                return false;
                        }
                    }
                }

                success = true;
            }
            else
                throw new NotImplementedException();

            if (!success)
            {
                return CheckFailed("Niepoprawny login i hasło");
            }

            return true;
        }

        protected override bool PostLoginRequest()
        {
            bool result = base.PostLoginRequest();

            if (result)
            {
                HomePage = GetHomePage();

                string heartbeatId = GetHeartbeatId();
                if (heartbeatId == null)
                    return false;
                heartbeatParameters = new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("hash", heartbeatId)
                };
            }

            return result;
        }

        protected override bool LogoutRequest()
        {
            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) logoutRequest = PerformRequest(
                "index/logout", HttpMethod.Get, null, null, false, null);
            return logoutRequest.requestProcessed;
        }

        protected override void PostLogoutRequest()
        {
            base.PostLogoutRequest();
            HomePage = null;
        }

        protected override bool TryExtendSession()
        {
            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) heartbeatRequest = PerformRequest(
                "index/heartbeat", HttpMethod.Post, heartbeatParameters, null, true, null);

            if (!heartbeatRequest.requestProcessed)
            {
                return false;
            }
            else
            {
                AssertMediaType(heartbeatRequest.responseTypeHeader, "application/json");

                GetinJsonHeartbeat jsonResponseHeartbeat = JsonConvert.DeserializeObject<GetinJsonHeartbeat>(heartbeatRequest.responseStr);
                if (jsonResponseHeartbeat.sessionExpire)
                {
                    ExtendSession();
                }

                return true;
            }
        }

        //pbl: pay-by-link
        protected override string CleanFastTransferUrl(string transferId)
        {
            string newTransferId = transferId
                .Replace("pbl/payment/new/", String.Empty)
                .Replace("pbl/#index/index/", String.Empty);

            if (newTransferId.Length != 64)
            {
                return null;
            }

            return newTransferId;
        }

        protected override bool LoginRequestForFastTransfer(string login, string password, string transferId, /*Browser browser,*/ string cookie)
        {
            IEnumerable<KeyValuePair<string, string>> loginLoginParameters = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("step", "1"),
                new KeyValuePair<string, string>("send", "true"),
                new KeyValuePair<string, string>("login", login),
            };

            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) loginLoginRequest = PerformRequest(
                $"pbl/index/index/{transferId}/0", HttpMethod.Post, loginLoginParameters,
                (HttpStatusCode statusCode) =>
                {
                    if (statusCode == (HttpStatusCode)269)
                    {
                        return CheckFailed("Odblokuj dostęp na stronie internetowej");
                    }
                    return true;
                },
                true, null);

            if (!loginLoginRequest.requestProcessed)
                return false;
            AssertMediaType(loginLoginRequest.responseTypeHeader, "text/html");

            if (loginLoginRequest.responseStr.SubstringFromToEx(new string[] { "LOGIN: " }, new string[] { " ", "<" }) != login)
                throw new NotSupportedException();

            IEnumerable<KeyValuePair<string, string>> loginPasswordParameters = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("step", "2"),
                new KeyValuePair<string, string>("send", "true"),
                new KeyValuePair<string, string>("password", password),
            };

            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) loginPasswordRequest = PerformRequest(
                $"pbl/index/index/{transferId}/0", HttpMethod.Post, loginPasswordParameters, null, true, null);

            if (!loginPasswordRequest.requestProcessed)
                return false;
            AssertMediaType(loginPasswordRequest.responseTypeHeader, "application/json");

            GetinJsonLoginPassword jsonResponseLoginPassword = JsonConvert.DeserializeObject<GetinJsonLoginPassword>(loginPasswordRequest.responseStr);
            bool success = jsonResponseLoginPassword.type != 7;

            if (!success)
            {
                return CheckFailed("Niepoprawny login i hasło");
            }

            return true;
        }

        protected override string MakeFastTransfer(string transferId, /*Browser browser,*/ string cookie)
        {
            //TODO przed wykonaniem informacja o danych przelewu

            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) getTransferRequest = PerformRequest(
                $"pbl/transfers/index/{transferId}", HttpMethod.Get, null, null, true, null);

            if (!getTransferRequest.requestProcessed)
                return null;
            AssertMediaType(getTransferRequest.responseTypeHeader, "text/html");

            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(getTransferRequest.responseStr);

            string codeProduct;
            string recipientNameToConfirm;
            try
            {
                codeProduct = document.DocumentNode.Descendants().First(n => n.GetAttributeValue("data-code", null) != null).GetAttributeValue("data-code", null);
                recipientNameToConfirm = document.DocumentNode.Descendants("div").Single(n => n.HasClass("transfer-details-recipient")).FirstChildNode("div").NextSibling.NextSibling.SingleChildNode("dl").ChildNodes.Single(n => n.Name == "dt").InnerText;
            }
            catch (InvalidOperationException)
            {
                string message = document.DocumentNode.Descendants().Single(n => n.HasClass("popUp-content")).SingleChildNode("p").InnerText;

                CheckFailed(message);
                return null;
            }

            IEnumerable<KeyValuePair<string, string>> transferMakeParameters = new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("step", "make"),
                    new KeyValuePair<string, string>("code_product", codeProduct),
                };

            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) transferMakeRequest = PerformRequest(
                $"pbl/transfers/standard", HttpMethod.Post, transferMakeParameters, null, true, null);

            if (!transferMakeRequest.requestProcessed)
                return null;
            AssertMediaType(transferMakeRequest.responseTypeHeader, "application/json");

            GetinJsonTransferMake jsonResponseTransferMake = JsonConvert.DeserializeObject<GetinJsonTransferMake>(transferMakeRequest.responseStr);
            if (!jsonResponseTransferMake.allow)
            {
                throw new NotSupportedException();
            }

            bool mobileAuthorization = IsConfirmationMobileAuthorization(jsonResponseTransferMake.confirmation);

            if (!mobileAuthorization)
            {
                int SMSCodeNumber = Int32.Parse(jsonResponseTransferMake.confirmation.SubstringFromToEx("SMS nr <b class=\"jsSmsNo sms-no\">", "</b>"));

                bool codeProceeded = false;
                while (!codeProceeded)
                {
                    string SMSCode = GetSMSCode($"Odbiorca {recipientNameToConfirm}. Kod SMS nr {SMSCodeNumber}");
                    if (SMSCode == null)
                        return null;

                    IEnumerable<KeyValuePair<string, string>> transferConfirmParameters = new KeyValuePair<string, string>[] {
                            new KeyValuePair<string, string>("step", "confirm"),
                            new KeyValuePair<string, string>("code_product", codeProduct),
                            new KeyValuePair<string, string>("token", SMSCode),
                        };

                    (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) transferConfirmRequest = PerformRequest(
                        $"pbl/transfers/standard", HttpMethod.Post, transferConfirmParameters, null, true, null);

                    if (!transferConfirmRequest.requestProcessed)
                        return null;
                    AssertMediaType(transferConfirmRequest.responseTypeHeader, "application/json");

                    GetinJsonTransferConfirm jsonResponseTransferConfirm = JsonConvert.DeserializeObject<GetinJsonTransferConfirm>(transferConfirmRequest.responseStr);
                    if (!jsonResponseTransferConfirm.confirmed)
                        Message(jsonResponseTransferConfirm.validationMessages.token);
                    else
                        codeProceeded = true;
                }
            }
            else
            {
                //TODO czy działa autoryzacja mobilna
                if (!ConfirmMobile())
                    return null;
            }

            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) transferSuccessRequest = PerformRequest(
                $"pbl/transfers/standard/success", HttpMethod.Get, null, null, true, null);

            AssertMediaType(transferSuccessRequest.responseTypeHeader, "text/html");

            document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(transferSuccessRequest.responseStr);
            string redirectAddress = document.DocumentNode.SingleChildNode("form").GetAttributeValue("action", null);
            redirectAddress = redirectAddress.NormalizeHtmlCharactersEx();

            return redirectAddress;
        }

        protected override void PostFastTransfer()
        {
            base.PostFastTransfer();
            HomePage = null;
        }

        private string GetHomePage()
        {
            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) getHomePageRequest = PerformRequest(
                "wallet/index", HttpMethod.Post, null, null, true, null);

            AssertMediaType(getHomePageRequest.responseTypeHeader, "text/html");

            return getHomePageRequest.responseStr;
        }

        public override (string accountNumber, double availableFunds) GetAccountData()
        {
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(HomePage);

            HtmlNode accountSection = document.DocumentNode.Descendants("div").Single(n => n.HasClass("w109") && n.Descendants("div").Single(d => d.HasClass("account-title")).Descendants("span").Single().Descendants("strong").Single().InnerText == "Konto Swobodne").ParentNode;

            HtmlNode accountNumberSection = accountSection.Descendants("div").Single(n => n.HasClass("w109")).Descendants("div").Single(n => n.HasClass("account-number"));
            string accountNumber = accountNumberSection.Descendants("div").Single().Descendants("input").Single().GetAttributeValue("value", String.Empty);

            HtmlNode availableFundsSection = accountSection.Descendants("p").Single(n => n.InnerText == "dostępne środki").NextSibling.NextSibling;
            string availableFunds = availableFundsSection.FirstChild.InnerText;

            return (accountNumber, Double.Parse(availableFunds));
        }

        //TODO
        //public void lokata()
        //{
        //    //IEnumerable<KeyValuePair<string, string>> bankNameParameters = new KeyValuePair<string, string>[] {
        //    //    new KeyValuePair<string, string>("nrb", formattedAccountNumber)
        //    //};

        //    (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) checkTransferRequest = PerformRequest(
        //        "https://secure.getinbank.pl/wallet/checkTransfer", HttpMethod.Get, null,
        //        null,
        //        false,
        //        null);

        //    (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) getDepositsRequest = PerformRequest(
        //        "https://secure.getinbank.pl/deposits/index", HttpMethod.Get, null,
        //        null,
        //        true,
        //        null);


        //    (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) getDepositsOfferRequest = PerformRequest(
        //        "https://secure.getinbank.pl/deposits/offer", HttpMethod.Get, null,
        //        null,
        //        true,
        //        null);

        //    HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
        //    document.LoadHtml(getDepositsOfferRequest.responseStr);

        //    string offerCode = document.GetElementbyId("depo-offer-container").SingleChildNode("div").SingleChildNode("div").SingleChildNode("div").SingleChildNode("div").SingleChildNode("ul",
        //        n => n.Any("li", n2 => n2.SingleChildNode("div", n3 => n3.HasClass("title")).SingleChildNode("dl").SingleChildNode("dt").InnerText == "Lokata Tradycyjna"))
        //        .SingleChildNode("li", n2 => n2.SingleChildNode("div", n3 => n3.HasClass("title")).SingleChildNode("dl").SingleChildNode("dt").InnerText == "Lokata Tradycyjna")
        //        .SingleChildNode("div", n => n.HasClass("deposit-button")).SingleChildNode("a").GetAttributeValue("data-code", null);

        //    IEnumerable<KeyValuePair<string, string>> parameters1 = new KeyValuePair<string, string>[] {
        //        new KeyValuePair<string, string>("code", offerCode)
        //    };

        //    (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) depositDetailsRequest = PerformRequest(
        //        "https://secure.getinbank.pl/deposits/depositDetails", HttpMethod.Post, parameters1,
        //        null,
        //        true,
        //        null);

        //    GetinJsonDepositDetails jsonResponseDepositDetails = JsonConvert.DeserializeObject<GetinJsonDepositDetails>(depositDetailsRequest.responseStr);

        //    string offerIntervalCode = jsonResponseDepositDetails.products.Single().intervals.Single(i => i.time == 12).code;

        //    (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) newDepositRequest = PerformRequest(
        //        $"https://secure.getinbank.pl/deposits/newDeposit/{offerCode}/", HttpMethod.Get, null,
        //        null,
        //        true,
        //        null);

        //    document = new HtmlAgilityPack.HtmlDocument();
        //    document.LoadHtml(newDepositRequest.responseStr);

        //    string parameter1Name = document.GetElementbyId("newDepositOffer").SingleChildNode("input",
        //        n => n.GetAttributeValue("value", null) != "1" && n.GetAttributeValue("value", null) != String.Empty && n.GetAttributeValue("value", null) != offerCode)
        //        .GetAttributeValue("name", null);

        //    HtmlNode step1SectionNode = document.GetElementbyId("newDepositOffer").SingleChildNode("div").SingleChildNode("div", n => n.HasClass("transferTab")).SingleChildNode("div", n => n.HasClass("cutWindow")).SingleChildNode("div").SingleChildNode("section");
        //    string parameter2Name = step1SectionNode.SingleChildNode("div", n => n.HasClass("deposit-checkbox-required") && n.SingleChildNode("span").InnerText == "OŚWIADCZAM, ŻE ZAPOZNAŁEM SIĘ I AKCEPTUJĘ TREŚĆ: REGULAMINU RACHUNKÓW BANKOWYCH, KART DEBETOWYCH ORAZ USŁUGI BANKOWOŚCI ELEKTRONICZNEJ I USŁUGI BANKOWOŚCI TELEFONICZNEJ W GETIN NOBLE BANK S.A. DLA OSÓB FIZYCZNYCH NIEPROWADZĄCYCH DZIAŁALNOŚCI GOSPODARCZEJ.").SingleChildNode("input").GetAttributeValue("name", null);
        //    string parameter3Name = step1SectionNode.SingleChildNode("div", n => n.HasClass("deposit-checkbox-required") && n.SingleChildNode("span").InnerText == "OŚWIADCZAM, ŻE PRZED ZAWARCIEM UMOWY ZAPOZNAŁEM SIĘ Z WARUNKAMI UMOWNYMI LOKATY W  TABELI OPROCENTOWANIA LOKAT TERMINOWYCH DLA OSÓB FIZYCZNYCH  I WSZYSTKIE AKCEPTUJĘ ORAZ ZOSTAŁEM POINFORMOWANY PRZEZ BANK O PRAWIE ODSTĄPIENIA I OTRZYMAŁEM WZÓR OŚWIADCZENIA O ODSTĄPIENIU.").SingleChildNode("input").GetAttributeValue("name", null);
        //    string parameter4Name = step1SectionNode.SingleChildNode("div", n => n.HasClass("deposit-checkbox-required") && n.SingleChildNode("span").InnerText == "Potwierdzam otrzymanie Arkusza informacyjnego dla deponentów.").SingleChildNode("input").GetAttributeValue("name", null);
        //    string parameter5Name = step1SectionNode.SingleChildNode("div", n => n.HasClass("deposit-checkbox-required") && n.SingleChildNode("span").InnerText == "OŚWIADCZAM, ŻE ZAPOZNAŁEM SIĘ I AKCEPTUJĘ TREŚĆ TABELI OPŁAT I PROWIZJI GETIN NOBLE BANKU S.A. DLA KLIENTÓW INDYWIDUALNYCH - RACHUNKI PŁATNICZE W ZŁ I INNYCH WALUTACH, LOKATY I KREDYT W RACHUNKU PŁATNICZYM.").SingleChildNode("input").GetAttributeValue("name", null);


        //    (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) depositIntervalsRequest = PerformRequest(
        //        $"https://secure.getinbank.pl/deposits/getIntervalsInfo/{offerCode}/{offerIntervalCode}", HttpMethod.Get, null,
        //        null,
        //        true,
        //        null);

        //    GetinJsonDepositDetailsProductInterval jsonResponseDepositDetailsProductInterval = JsonConvert.DeserializeObject<GetinJsonDepositDetailsProductInterval>(depositIntervalsRequest.responseStr);

        //    IEnumerable<KeyValuePair<string, string>> parameters2 = new KeyValuePair<string, string>[] {
        //        //new KeyValuePair<string, string>(parameter1Name, "0c3501341ed01d3330dbbf23e198ad37"),
        //        new KeyValuePair<string, string>(parameter1Name, "1699320666680"),
        //        new KeyValuePair<string, string>("selected_offer", offerCode),
        //        new KeyValuePair<string, string>("code_acrm_slider", ""),
        //        new KeyValuePair<string, string>("code_acrm_offer", ""),
        //        new KeyValuePair<string, string>("send", "1"),
        //        new KeyValuePair<string, string>("step", "1"),
        //        new KeyValuePair<string, string>("skipAML", ""),
        //        //TODO
        //        new KeyValuePair<string, string>("nrb_out", "78146011812025517961010002"),
        //        //TODO
        //        new KeyValuePair<string, string>("amount", "1000"),
        //        new KeyValuePair<string, string>("interval", offerIntervalCode),
        //        //TODO "+"
        //        new KeyValuePair<string, string>("disponent", "KRZYSZTOF SZULC"),
        //        //TODO
        //        new KeyValuePair<string, string>("nrb_back", "78146011812025517961010002"),
        //        new KeyValuePair<string, string>("nrb_other", ""),
        //        new KeyValuePair<string, string>(parameter2Name, "1"),
        //        new KeyValuePair<string, string>(parameter3Name, "1"),
        //        new KeyValuePair<string, string>(parameter4Name, "1"),
        //        new KeyValuePair<string, string>(parameter5Name, "1"),
        //    };

        //    (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) newDepositRequest2 = PerformRequest(
        //       "https://secure.getinbank.pl/deposits/newDeposit", HttpMethod.Post, parameters2,
        //       null,
        //       true,
        //       null);
        //}

        public override bool MakeTransfer(string recipient, string address, string accountNumber, string title, double amount)
        {
            string currency = "PLN";
            string formattedAccountNumber = accountNumber.SimplifyAccountNumber();

            IEnumerable<KeyValuePair<string, string>> bankNameParameters = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("nrb", formattedAccountNumber)
            };

            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) getBankNameRequest = PerformRequest(
                "transfers/getBankName", HttpMethod.Post, bankNameParameters, null, true, null);

            if (!getBankNameRequest.requestProcessed)
                return false;
            AssertMediaType(getBankNameRequest.responseTypeHeader, "application/json");

            GetinJsonBankName jsonResponseBankName = JsonConvert.DeserializeObject<GetinJsonBankName>(getBankNameRequest.responseStr);
            if (jsonResponseBankName.bank_own == 0 && String.IsNullOrEmpty(jsonResponseBankName.bank_name))
            {
                return CheckFailed("numer konta nie jest poprawny");
            }

            string accCode;
            accCode = HomePage.SubstringFromToEx("href=\"#accounts/details/", "/");

            string amountToConfirm;
            string titleToConfirm;
            string transferPayment;
            string recipientAccountNumberToConfirm;
            string recipientBankNameToConfirm;
            string recipientNameToConfirm;
            string recipientAddressToConfirm;

            IEnumerable<KeyValuePair<string, string>> transferBeginParameters = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("step", "2"),
                new KeyValuePair<string, string>("accCode", accCode),
                new KeyValuePair<string, string>("object_name", recipient),
                new KeyValuePair<string, string>("address1", address),
                new KeyValuePair<string, string>("nrb", formattedAccountNumber),
                new KeyValuePair<string, string>("title", title),
                new KeyValuePair<string, string>("amount", amount.ToString("F", CultureInfo.CreateSpecificCulture("en-CA"))),
                new KeyValuePair<string, string>("currency", currency),
            };

            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) transferBeginRequest = PerformRequest(
                "transfers/standard/", HttpMethod.Post, transferBeginParameters, null, true, null);

            if (!transferBeginRequest.requestProcessed)
                return false;
            AssertMediaType(transferBeginRequest.responseTypeHeader, "text/html");

            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(transferBeginRequest.responseStr);

            int stepNumber = Int32.Parse(document.DocumentNode.SingleChildNode("section").SingleChildNode("input").GetAttributeValue("value", String.Empty));

            if (stepNumber == 1)
            {
                return CheckFailed("Niepoprawne dane");
            }

            amountToConfirm = document.DocumentNode.Descendants("div").Single(n1 => n1.HasClass("col-md-3") && n1.Descendants("div").Single(n11 => n11.HasClass("title")).InnerText == "na kwotę").Descendants("div").Single(n12 => n12.ContainsClasses(new string[] { "value", "green" })).InnerText;
            titleToConfirm = document.DocumentNode.Descendants("div").Single(n1 => n1.HasClass("col-md-3") && n1.Descendants("div").Single(n11 => n11.HasClass("title")).InnerText == "tytułem").Descendants("div").Single(n12 => n12.ContainsClasses(new string[] { "value", "green" })).InnerText;
            transferPayment = document.DocumentNode.Descendants("div").Single(n1 => n1.HasClass("col-md-3") && n1.Descendants("div").Single(n11 => n11.HasClass("title")).InnerText == "opłata za przelew").Descendants("div").Single(n12 => n12.ContainsClasses(new string[] { "value", "green" })).InnerText;

            HtmlNode recipientSection = document.DocumentNode.Descendants("div").Single(n => n.HasClass("title-wide") && n.InnerText == "odbiorca").ParentNode;
            HtmlNode recipientRowSection = recipientSection.Descendants("div").Single(n => n.HasClass("row"));
            HtmlNode recipientAccountSection = recipientRowSection.Descendants("div").First();
            HtmlNode recipientNameSection = recipientAccountSection.NextSibling.NextSibling;

            recipientAccountNumberToConfirm = recipientAccountSection.Descendants("div").Single(n => n.HasClass("value")).InnerText;
            recipientBankNameToConfirm = recipientAccountSection.Descendants("div").Single(n => n.HasClass("subtext")).InnerText;
            recipientNameToConfirm = recipientNameSection.Descendants("div").Single(n => n.HasClass("value")).InnerText;
            recipientAddressToConfirm = recipientNameSection.Descendants("div").Single(n => n.HasClass("subtext")).InnerText;

            if (amountToConfirm.Replace(" ", String.Empty) != $"{amount.ToString("F", CultureInfo.CreateSpecificCulture("fr-FR"))}{currency}")
            {
                throw new NotSupportedException();
            }
            if (titleToConfirm != title && !String.IsNullOrEmpty(title))
                Message($"Changed title: {titleToConfirm}");
            if (Double.Parse(transferPayment.SubstringToEx(" PLN")) != 0)
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

            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) transferMakeRequest = PerformRequest(
                "transfers/standard/", HttpMethod.Post, transferMakeParameters, null, true, null);

            if (!transferMakeRequest.requestProcessed)
                return false;
            AssertMediaType(transferMakeRequest.responseTypeHeader, "application/json");

            GetinJsonTransferMake jsonResponseTransferMake = JsonConvert.DeserializeObject<GetinJsonTransferMake>(transferMakeRequest.responseStr);
            if (!jsonResponseTransferMake.allow)
            {
                throw new NotSupportedException();
            }

            bool mobileAuthorization = IsConfirmationMobileAuthorization(jsonResponseTransferMake.confirmation);

            if (!mobileAuthorization)
            {
                int SMSCodeNumber = Int32.Parse(jsonResponseTransferMake.confirmation.SubstringFromToEx("SMS nr <b class=\"jsSmsNo sms-no\">", "</b>"));

                //TODO podpiąć telefon. Pobrać kod. Sprawdzić czy dane się zgadzają (nr konta, kwota, nr kodu)

                bool codeProceeded = false;
                while (!codeProceeded)
                {
                    string SMSCode = GetSMSCode($"Bank {recipientBankNameToConfirm}. Kod SMS nr {SMSCodeNumber}");
                    if (SMSCode == null)
                        return false;
                    IEnumerable<KeyValuePair<string, string>> transferConfirmParameters = new KeyValuePair<string, string>[] {
                        new KeyValuePair<string, string>("step", "confirm"),
                        new KeyValuePair<string, string>("token", SMSCode),
                    };

                    (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) transferConfirmRequest = PerformRequest(
                        "transfers/standard/", HttpMethod.Post, transferConfirmParameters, null, true, null);

                    if (!transferConfirmRequest.requestProcessed)
                        return false;
                    AssertMediaType(transferConfirmRequest.responseTypeHeader, "application/json");

                    GetinJsonTransferConfirm jsonResponseTransferConfirm = JsonConvert.DeserializeObject<GetinJsonTransferConfirm>(transferConfirmRequest.responseStr);
                    if (!jsonResponseTransferConfirm.confirmed)
                        Message(jsonResponseTransferConfirm.validationMessages.token);
                    else
                        codeProceeded = true;
                }
            }
            else
            {
                if (!ConfirmMobile())
                    return false;
            }

            return true;
        }

        protected override void PostTransfer()
        {
            base.PostTransfer();
            HomePage = null;
        }

        protected override bool MakePrepaidTransfer(string recipient, string phoneNumber, double amount)
        {
            string accCode = HomePage.SubstringFromToEx("href=\"#accounts/details/", "/");

            IEnumerable<(string name, string code)> operators;

            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) prepaidRequest = PerformRequest(
                "transfers/prepaid", HttpMethod.Get, null, null, true, null);

            if (!prepaidRequest.requestProcessed)
                return false;
            AssertMediaType(prepaidRequest.responseTypeHeader, "text/html");

            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(prepaidRequest.responseStr);

            HtmlNode operatorsNode = document.DocumentNode.Descendants("div").Single(n => n.Id == "operators_list_content").Descendants("ul").Single();
            operators = operatorsNode.Descendants("li").Select(n => (n.InnerText, n.GetAttributeValue("data-value", String.Empty)));

            (string name, string data) operatorItem = PromptComboBox<string>("Operator", operators.Select(o => new PrepaidOperatorComboBoxItem<string>(o.name, o.code)));
            if (operatorItem.data == null)
                return false;

            string operatorCode = operatorItem.data;

            string amountToConfirm;
            string phoneNumberToConfirm;
            string operatorNameToConfirm;
            string recipientNameToConfirm;

            IEnumerable<KeyValuePair<string, string>> transferBeginParameters = new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("step", "2"),
                    new KeyValuePair<string, string>("accCode", accCode),
                    new KeyValuePair<string, string>("object_name", recipient),
                    new KeyValuePair<string, string>("phone_number", phoneNumber.SimplifyPhoneNumber()),
                    new KeyValuePair<string, string>("operator", operatorCode),
                    new KeyValuePair<string, string>("amount", amount.ToString("F", CultureInfo.CreateSpecificCulture("en-CA"))),
                    new KeyValuePair<string, string>("accept-term", "on"),
                };

            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) transferBeginRequest = PerformRequest(
                "transfers/prepaid/", HttpMethod.Post, transferBeginParameters, null, true, null);

            if (!transferBeginRequest.requestProcessed)
                return false;
            AssertMediaType(transferBeginRequest.responseTypeHeader, "text/html");

            document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(transferBeginRequest.responseStr);

            int stepNumber = Int32.Parse(document.DocumentNode.SingleChildNode("section").SingleChildNode("input").GetAttributeValue("value", String.Empty));

            if (stepNumber == 1)
            {
                StringBuilder errorMessage = new StringBuilder();

                foreach (HtmlNode errorNode in document.DocumentNode.Descendants("div").Where(n => n.HasClass("cloud")))
                {
                    if (errorMessage == null)
                        errorMessage = new StringBuilder();
                    errorMessage.AppendLine(errorNode.InnerText);
                }
                //TODO https://secure.getinbank.pl/transfers/prepaidAmounts

                return CheckFailed(errorMessage != null ? errorMessage.ToString() : "Niepoprawne dane");
            }

            HtmlNode transferDetailsInfoSection = document.DocumentNode.Descendants("div").Single(n => n.HasClass("transfer-details-info"));
            HtmlNode transferDetailsSection = document.DocumentNode.Descendants("div").Single(n => n.HasClass("transfer-details") && !n.HasClass("from"));

            amountToConfirm = transferDetailsInfoSection.Descendants("dl").Single(n1 => n1.Descendants("dd").Single().InnerText == "na kwotę").Descendants("dt").Single().InnerText;
            phoneNumberToConfirm = transferDetailsSection.Descendants("dl").First().Descendants("dt").Single().InnerText;
            operatorNameToConfirm = transferDetailsSection.Descendants("dl").First().Descendants("dd").Single().InnerText;
            recipientNameToConfirm = transferDetailsSection.Descendants("dl").First().NextSibling.NextSibling.Descendants("dt").Single().InnerText;

            if (amountToConfirm.Replace(" ", String.Empty) != $"{amount.ToString("F", CultureInfo.CreateSpecificCulture("fr-FR"))}{"PLN"}")
            {
                throw new NotSupportedException();
            }
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

            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) transferMakeRequest = PerformRequest(
                "transfers/prepaid/", HttpMethod.Post, transferMakeParameters, null, true, null);

            if (!transferMakeRequest.requestProcessed)
                return false;
            AssertMediaType(transferMakeRequest.responseTypeHeader, "application/json");

            GetinJsonTransferMake jsonResponseTransferMake = JsonConvert.DeserializeObject<GetinJsonTransferMake>(transferMakeRequest.responseStr);
            if (!jsonResponseTransferMake.allow)
            {
                throw new NotSupportedException();
            }

            bool mobileAuthorization = IsConfirmationMobileAuthorization(jsonResponseTransferMake.confirmation);

            if (!mobileAuthorization)
            {
                SMSCodeNumber = Int32.Parse(jsonResponseTransferMake.confirmation.SubstringFromToEx("SMS nr <b class=\"jsSmsNo sms-no\">", "</b>"));

                bool codeProceeded = false;
                while (!codeProceeded)
                {
                    string SMSCode = GetSMSCode($"Operator {operatorItem.name}. Kod SMS nr {SMSCodeNumber}");
                    if (SMSCode == null)
                        return false;
                    IEnumerable<KeyValuePair<string, string>> transferConfirmParameters = new KeyValuePair<string, string>[] {
                                new KeyValuePair<string, string>("step", "confirm"),
                                new KeyValuePair<string, string>("token", SMSCode),
                            };

                    (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) transferConfirmRequest = PerformRequest(
                        "transfers/prepaid/", HttpMethod.Post, transferConfirmParameters, null, true, null);

                    if (!transferConfirmRequest.requestProcessed)
                        return false;
                    AssertMediaType(transferConfirmRequest.responseTypeHeader, "application/json");

                    GetinJsonTransferConfirm jsonResponseTransferConfirm = JsonConvert.DeserializeObject<GetinJsonTransferConfirm>(transferConfirmRequest.responseStr);
                    if (!jsonResponseTransferConfirm.confirmed)
                        Message(jsonResponseTransferConfirm.validationMessages.token);
                    else
                        codeProceeded = true;
                }
            }
            else
            {
                if (!ConfirmMobile())
                    return false;
            }

            return true;
        }

        public override void GetDetailsFile(HistoryItem item, FileStream file)
        {
            PerformRequest($"history/printDetails/{item.Id}", HttpMethod.Get, null, null, false, (Stream contentStream) =>
                {
                    contentStream.CopyTo(file);
                }
            );
        }

        protected override GetinBankHistoryFilter CreateFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact)
        {
            return new GetinBankHistoryFilter() { DateFrom = dateFrom, DateTo = dateTo, Direction = direction, AmountExact = amountExact, Title = title };
        }

        protected override List<GetinBankHistoryItem> GetHistoryItems(GetinBankHistoryFilter filter = null)
        {
            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) clearTitleFilterRequest = PerformRequest(
                "history/setTitleFilter", HttpMethod.Post, HttpOperations.EmptyParameters, null, false, null);

            if (!clearTitleFilterRequest.requestProcessed)
                return null;

            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) clearFiltersRequest = PerformRequest(
                "history/setFilters", HttpMethod.Post, HttpOperations.EmptyParameters, null, false, null);

            if (!clearFiltersRequest.requestProcessed)
                return null;

            List<GetinBankHistoryItem> result = new List<GetinBankHistoryItem>();

            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) getHistoryRequest = PerformRequest(
                "history/index", HttpMethod.Get, null, null, false, null);

            if (!getHistoryRequest.requestProcessed)
                return null;

            if (filter != null && filter.FindAccountNumber)
            {
                List<GetinBankHistoryItem> emptyOperations = GetinBankAcountNumbersHistory.Download(this);

                Dictionary<(DateTime dateFrom, DateTime dateTo), List<string>> ranges = new Dictionary<(DateTime dateFrom, DateTime dateTo), List<string>>();
                foreach (DictionaryEntry entry in Properties.Settings.Default.AcountNumbers)
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

                //usunięte, bo dublowało operacje z dzisiaj
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
                if (filter != null)
                {
                    if (filter.SetTitle)
                    {
                        (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) setTitleFilterRequest = PerformRequest(
                            "history/setTitleFilter", HttpMethod.Post, filter.CreateTitleParameters(), null, false, null);

                        if (!setTitleFilterRequest.requestProcessed)
                            return null;

                        //TODO tutaj pierwsza strona wyników. Użyć CheckNotExpired
                        //if (responseTypeHeader.MediaType == )
                    }

                    if (filter.SetDetails)
                    {
                        (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) setFiltersRequest = PerformRequest(
                            "history/setFilters", HttpMethod.Post, filter.CreateDetailsParameters(), null, false, null);

                        if (!setFiltersRequest.requestProcessed)
                            return null;
                    }
                }

                //TODO liczbę stron pobrać z document
                int? pageCounter = null;
                for (int page = 1; (pageCounter == null || page <= pageCounter) && (filter.CounterLimit == 0 || result.Count < filter.CounterLimit); page++)
                {
                    bool success = false;

                    while (!success)
                    {
                        (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) loadHistoryRequest = PerformRequest(
                            $"history/loadHistory/{page}", HttpMethod.Get, null, null, true, null);

                        if (!loadHistoryRequest.requestProcessed)
                            return null;
                        AssertMediaType(loadHistoryRequest.responseTypeHeader, "text/html");

                        HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                        document.LoadHtml(loadHistoryRequest.responseStr);

                        if (document.DocumentNode.Descendants("section").SingleOrDefault(n => n.HasClass("empty-section")) != null)
                        {
                            pageCounter = 0;
                            break;
                        }

                        pageCounter = Int32.Parse(document.DocumentNode.Descendants("input").Single(n => n.Id == "pages").GetAttributeValue("value", String.Empty));

                        result.AddRange(document.DocumentNode.Descendants("li").Select(n => new GetinBankHistoryItem(n)));
                        success = true;
                    }
                }
            }
            return result;
        }

        private bool ConfirmMobile()
        {
            while (true)
            {
                if (!PromptOKCancel("Potwierdź operację na urządzeniu mobilnym"))
                    return false;

                (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) confirmationCheckRequest = PerformRequest(
                    "index/confirmationCheck", HttpMethod.Post, null, null, true, null);
                AssertMediaType(confirmationCheckRequest.responseTypeHeader, "application/json");
                GetinJsonMobileConfirmation jsonResponseMobileConfirmation = JsonConvert.DeserializeObject<GetinJsonMobileConfirmation>(confirmationCheckRequest.responseStr);
                if (jsonResponseMobileConfirmation.confirmed)
                    return true;
            }
        }

        public string GetHeartbeatId()
        {
            (string responseStr, MediaTypeHeaderValue responseTypeHeader, bool requestProcessed) getHeartbeatIdRequest = PerformRequest(
                "layout/index/do", HttpMethod.Get, null, null, true, null);

            if (!getHeartbeatIdRequest.requestProcessed)
                return null;
            AssertMediaType(getHeartbeatIdRequest.responseTypeHeader, "application/json");

            GetinJsonHash jsonResponseHash = JsonConvert.DeserializeObject<GetinJsonHash>(getHeartbeatIdRequest.responseStr);
            return jsonResponseHash.hash;
        }

        public void ExtendSession()
        {
            GetHomePage();
        }

        //TODO zwracać klasę
        private (string, MediaTypeHeaderValue, bool) PerformRequest(string requestUri, HttpMethod method, IEnumerable<KeyValuePair<string, string>> parameters,
            Func<HttpStatusCode, bool> statusCodeAction,
            bool readContent,
            Action<Stream> useStream)
        {
            try
            {
                using (HttpRequestMessage message = CreateHttpRequestMessage(requestUri, method, parameters))
                {
                    using (HttpResponseMessage response = httpClient.SendAsync(message).Result)
                    {
                        if (statusCodeAction != null)
                        {
                            if (!statusCodeAction.Invoke(response.StatusCode))
                                return (null, null, false);
                        }
                        using (HttpContent content = response.Content)
                        {
                            string responseStr = null;

                            if (readContent)
                            {
                                responseStr = content.ReadAsStringAsync().Result;

                                if (!CheckNotExpired(responseStr))
                                {
                                    NoteExpiredSession();
                                    return (null, null, false);
                                }
                            }

                            useStream?.Invoke(content.ReadAsStreamAsync().Result);

                            return (responseStr, response.Content.Headers.ContentType, true);
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
                                return (null, null, false);
                            }
                        }
                    }
                }

                throw;
            }
        }

        private static HttpRequestMessage CreateHttpRequestMessage(string requestUri, HttpMethod method, IEnumerable<KeyValuePair<string, string>> parameters = null)
        {
            HttpRequestMessage message = new HttpRequestMessage(method, requestUri);
            message.Headers.Add("User-Agent", userAgent);
            //if (dataType != null)
            //    message.Headers.Add("X-DataType", dataType);
            //if (requestedWithXMLHttpRequest)
            message.Headers.Add("X-Requested-With", "XMLHttpRequest");
            //message.Headers.Add("Connection", "keep-alive");

            //if (reload != null)
            //    message.Headers.Add("X-Reload", reload.Value ? "1" : "0");
            //if (customDestination)
            //    message.Headers.Add("X-Custom-Destination", "true");
            //message.Headers.Add("Accept-Language", "pl,en-US;q=0.7,en;q=0.3");
            //message.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            //message.Headers.Add("Accept", "text/html, */*; q=0.01");

            //message.Headers.Add("Referer", "https://secure.getinbank.pl/");

            //if (refreshCsrfToken)
            //    RefreshCsrfToken();
            //message.Headers.Add("X-CSRFToken", securityToken);

            if (parameters != null)
                message.Content = new FormUrlEncodedContent(parameters);

            return message;
        }

        private void RefreshCsrfToken()
        {
            //TODO zamiast -2h, to dopasowanie do strefy czasowej z response
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

        private string GetSMSCode(string text)
        {
            return PromptString(text, @"^\d{6}$");
        }

        private bool CheckNotExpired(string responseStr)
        {
            try
            {
                GetinJsonExpired jsonResponseExpired = JsonConvert.DeserializeObject<GetinJsonExpired>(responseStr);
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

        //TODO mediaType jako enum
        private static void AssertMediaType(MediaTypeHeaderValue responseTypeHeader, string mediaType)
        {
            if (responseTypeHeader.MediaType != mediaType)
                throw new NotImplementedException();
        }
    }
}
