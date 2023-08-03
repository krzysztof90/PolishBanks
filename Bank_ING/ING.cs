using BankService.LocalTools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using static BankService.Bank_GetinBank.GetinBankJsonResponse;
using static BankService.Bank_ING.INGJsonRequest;
using static BankService.Bank_ING.INGJsonResponse;
using static Tools.HttpOperations;

namespace BankService.Bank_ING
{
    [BankTypeAttribute(BankType.ING)]
    public class ING : BankBase<INGHistoryItem, INGHistoryFilter>
    {
        private const string hexTable = "0123456789abcdef";
        private const int maxRowLength = 35;

        private INGJsonResponseAccounts accountsDetails;
        private INGJsonResponseAccounts AccountsDetails
        {
            get => accountsDetails ?? (accountsDetails = GetAccountsDetails());
            set
            {
                accountsDetails = value;
                if (accountsDetails == null)
                    CallAvailableFundsClear();
            }
        }
        private string Token;
        private string LogoutUrl;

        public override bool FastTransferMandatoryTransferId => true;
        public override bool FastTransferMandatoryBrowserCookies => false;
        public override bool FastTransferMandatoryCookie => false;
        public override bool TransferMandatoryTitle => true;
        public override bool PrepaidTransferMandatoryRecipient => false;

        protected override string BaseAddress => "https://login.ingbank.pl/mojeing/rest/";

        protected override bool LoginRequest(string login, string password)
        {
            return LoginRequest(login, password, null);
        }

        private bool LoginRequest(string login, string password, string transferId)
        {
            (FastTransferType? type, string paData, string pblData) fastTransferData = GetDataFromFastTransfer(transferId);

            Token = null;

            (INGJsonResponseLogin response, bool requestProcessed) loginResponse = PerformRequest<INGJsonResponseLogin>(
                "renchecklogin", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestLogin.Create(login)),
                "Niepoprawny login",
                true, null, null);
            if (!loginResponse.requestProcessed)
                return false;

            string pwdHash = CreatePwdHash(loginResponse.response.data.salt, loginResponse.response.data.mask, loginResponse.response.data.key, password);
            if (pwdHash == null)
                return false;

            (INGJsonResponseLoginPassword response, bool requestProcessed) passwordResponse = PerformRequest<INGJsonResponseLoginPassword>(
                "renlogin", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestLoginPassword.Create(pwdHash, login, fastTransferData.type == FastTransferType.PA ? fastTransferData.paData : null)),
                null, true, null, null);
            if (!passwordResponse.requestProcessed)
                return false;

            bool success = true;

            if (String.IsNullOrEmpty(passwordResponse.response.data.token))
            {
                RemoveSavedCookie(("TBN4VFFiLdynGrcM3aq", "/mojeing", "login.ingbank.pl"));
                success = Confirm(passwordResponse.response.data, null);
            }
            else
            {
                Token = passwordResponse.response.data.token;
            }

            return success;
        }

        private string CreatePwdHash(string salt, string mask, string key, string password)
        {
            string saltMask = MixSaltAndMaskData(salt, mask, password);
            if (saltMask == null)
                return null;
            return Hmac(key, saltMask);
        }

        private string MixSaltAndMaskData(string salt, string mask, string password)
        {
            if (mask.LastIndexOf('*') > password.Length - 1)
            {
                CheckFailed("Niepoprawne hasło");
                return null;
            }
            StringBuilder result = new StringBuilder(salt);
            for (int i = 0; i < mask.Length; i++)
                if (mask[i] == '*')
                    result[i] = password[i];
            return result.ToString();
        }

        private string[] SplitDescription(params string[] descriptions)
        {
            int maxNumberOfRows = 4;
            List<string> lines = new List<string>();
            foreach (string description in descriptions)
            {
                FillLinesFromDescription(description, lines);
            }

            if (lines.Count > maxNumberOfRows)
            {
                string description = String.Join(" ", descriptions);
                lines = new List<string>();
                FillLinesFromDescription(description, lines);
            }

            string[] result = new string[maxNumberOfRows];
            for (int i = 0; i < maxNumberOfRows; i++)
            {
                if (i < lines.Count)
                    result[i] = lines[i];
                else
                    result[i] = String.Empty;
            }

            return result;
        }

        private void FillLinesFromDescription(string description, List<string> lines)
        {
            string[] words = description.Split(' ');
            List<string> lineWords = new List<string>();
            foreach (string word in words)
            {
                if (lineWords.Sum(w => w.Length) + lineWords.Count + word.Length > maxRowLength)
                {
                    lines.Add(String.Join(" ", lineWords));
                    lineWords = new List<string>();
                }
                lineWords.Add(word);
            }
            if (lineWords.Count != 0)
                lines.Add(String.Join(" ", lineWords));
        }

        protected override bool PostLoginRequest()
        {
            bool result = base.PostLoginRequest();

            if (result)
            {
                AccountsDetails = GetAccountsDetails();
            }

            return result;
        }

        protected bool PostPBLLoginRequest()
        {
            AccountsDetails = GetAccountsDetails(true);

            return true;
        }

        protected override bool LogoutRequest()
        {
            (INGJsonResponseLogout response, bool requestProcessed) logoutResponse = PerformRequest<INGJsonResponseLogout>(
                "renlogout", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestLogout.Create()),
                null, true, null, null);
            if (!logoutResponse.requestProcessed)
                return false;

            LogoutUrl = logoutResponse.response.data.url;
            return true;
        }

        protected override void PostLogoutRequest()
        {
            base.PostLogoutRequest();
            AccountsDetails = null;
        }

        protected override int HeartbeatInterval => 300;

        protected override bool TryExtendSession()
        {
            (INGJsonResponsePing response, bool requestProcessed) extendResponse = PerformRequest<INGJsonResponsePing>(
                "renping", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestPing.Create(Token)),
                null, true, null, null);

            return extendResponse.requestProcessed;
        }

        private INGJsonResponseAccounts GetAccountsDetails(bool pbl = false)
        {
            if (!pbl)
            {
                (INGJsonResponseAccounts response, bool requestProcessed) accountsResponse = PerformRequest<INGJsonResponseAccounts>(
                    "rengetallingprds", HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestAccounts.Create(Token)),
                    null, true, null, null);
                if (!accountsResponse.requestProcessed)
                    throw new NotImplementedException();

                return accountsResponse.response;
            }
            else
            {
                (INGJsonResponseAccountsPBL response, bool requestProcessed) accountsResponse = PerformRequest<INGJsonResponseAccountsPBL>(
                    "rengetallaccounts", HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestAccounts.Create(Token)),
                    null, true, null, null);
                if (!accountsResponse.requestProcessed)
                    throw new NotImplementedException();

                //TODO inaczej
                return new INGJsonResponseAccounts() { data = new INGJsonResponseAccountsData() { accts = new INGJsonResponseAccountsDataAcct() { cur = new INGJsonResponseAccountsDataAcctCur() { accts = accountsResponse.response.data.cur.Select(c => c.CreateAccountsDataAcctAcct()).ToArray() }, sav = new INGJsonResponseAccountsDataAcctSav() { accts = accountsResponse.response.data.sav.Select(c => c.CreateAccountsDataAcctAcct()).ToArray() } } } };
            }
        }

        //TODO kilka rachunków, do wyboru, Suma wszystkich. To samo w VeloBank
        public override (string accountNumber, double availableFunds) GetAccountData()
        {
            INGJsonResponseAccountsDataAcctAcct account = AccountsDetails.data.accts.cur.accts.First();
            return (account.acct, account.plnbal);
        }

        public override bool MakeTransfer(string recipient, string address, string accountNumber, string title, double amount)
        {
            (INGJsonResponsePaymentCheckAccount response, bool requestProcessed) transferResponse = PerformRequest<INGJsonResponsePaymentCheckAccount>(
                "rengetacttkirinfo", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestCheckAccount.Create(Token, accountNumber.SimplifyAccountNumber())),
                null, true, null, null);
            if (!transferResponse.requestProcessed)
                return false;

            string[] benefname = SplitDescription(recipient, address);
            string[] details = SplitDescription(title);

            (INGJsonResponsePaymentConfirmable response, bool requestProcessed) paymentOrderResponse = PerformRequest<INGJsonResponsePaymentConfirmable>(
                "renpayord", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestPaymentOrder.Create(Token, amount, benefname[0], benefname[1], benefname[2], benefname[3], accountNumber.SimplifyAccountNumber(), GetAccountData().accountNumber.SimplifyAccountNumber(), details[0], details[1], details[2], details[3], "S")),
                null, true, null, null);
            if (!paymentOrderResponse.requestProcessed)
                return false;

            return Confirm(paymentOrderResponse.response.data, amount);
        }

        protected override void PostTransfer()
        {
            base.PostTransfer();
            AccountsDetails = null;
        }

        protected override string CleanFastTransferUrl(string transferId)
        {
            string newTransferId = transferId
                .Replace("https://login.ingbank.pl/mojeing/app/?#select/", String.Empty)
                .Replace("https://login.ingbank.pl/mojeing/app/#select/", String.Empty)

                .Replace("https://login.ingbank.pl/mojeing/paybylink/#login/ctxid=", String.Empty);

            (FastTransferType? type, string paData, string pblData) fastTransferData = GetDataFromFastTransfer(newTransferId);

            if (fastTransferData.type == null)
                return null;

            return newTransferId;
        }

        protected override bool LoginRequestForFastTransfer(string login, string password, string transferId, string cookie)
        {
            (FastTransferType? type, string paData, string pblData) fastTransferData = GetDataFromFastTransfer(transferId);

            return LoginRequest(login, password, transferId) && (fastTransferData.type == FastTransferType.PA ? PostLoginRequest() : PostPBLLoginRequest());
        }

        protected override string MakeFastTransfer(string transferId, string cookie)
        {
            (FastTransferType? type, string paData, string pblData) fastTransferData = GetDataFromFastTransfer(transferId);

            if (fastTransferData.type == FastTransferType.PA)
            {
                (INGJsonResponseFastTransferPolapiauthdata response, bool requestProcessed) fastTransferPolapiauthdataResponse = PerformRequest<INGJsonResponseFastTransferPolapiauthdata>(
                    "rengetpolapiauthdata", HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestFastTransfer.Create(Token)),
                    null, true, null, null);

                if (!fastTransferPolapiauthdataResponse.requestProcessed)
                    return null;

                (INGJsonResponseFastTransferDataConfirm response, bool requestProcessed) fastTransferPolapiauthdataConfirmResponse = PerformRequest<INGJsonResponseFastTransferDataConfirm>(
                    "renpolapiauthconfirm", HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestFastTransferConfirm.Create(Token, GetAccountData().accountNumber.SimplifyAccountNumber())),
                    null, true, null, null);

                if (!ConfirmFastTransfer(fastTransferPolapiauthdataConfirmResponse.response.data, fastTransferPolapiauthdataResponse.response.data.transfer.detail.amount))
                    return null;
            }
            else if (fastTransferData.type == FastTransferType.PayByLink)
            {
                (INGJsonResponseFastTransferPBL response, bool requestProcessed) fastTransferPBLResponse = PerformRequest<INGJsonResponseFastTransferPBL>(
                    "rengetdirtrndata", HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestFastTransferPBL.Create(Token, fastTransferData.pblData)),
                    null, true, null, null);

                if (!fastTransferPBLResponse.requestProcessed)
                    return null;

                (INGJsonResponseFastTransferDataConfirm response, bool requestProcessed) fastTransferPBLDataConfirmResponse = PerformRequest<INGJsonResponseFastTransferDataConfirm>(
                    "renpaydirtrn", HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestFastTransferPBLDataConfirm.Create(Token, fastTransferData.pblData, GetAccountData().accountNumber)),
                    null, true, null, null);

                if (!ConfirmFastTransfer(fastTransferPBLDataConfirmResponse.response.data, fastTransferPBLResponse.response.data.amount))
                    return null;
            }
            else
                throw new NotImplementedException();

            Logout();
            return LogoutUrl;
        }

        protected override void PostFastTransfer()
        {
            base.PostFastTransfer();
            AccountsDetails = null;
        }

        protected override bool MakePrepaidTransfer(string recipient, string phoneNumber, double amount)
        {
            if (amount != Math.Truncate(amount))
            {
                return CheckFailed("Kwota nie może zawierać miejsc po przecinku");
            }

            (INGJsonResponseGsmOperators response, bool requestProcessed) gsmOperatorsResponse = PerformRequest<INGJsonResponseGsmOperators>(
                "rengetgsmppopr", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestGsmOperators.Create(Token)),
                null, true, null, null);
            if (!gsmOperatorsResponse.requestProcessed)
                return false;

            (string name, INGJsonResponseGsmOperatorsDataOperator data) operatorItem = PromptComboBox<INGJsonResponseGsmOperatorsDataOperator>("Operator", gsmOperatorsResponse.response.data.opers.Where(o => o.VisibleValue == INGJsonResponseNoYes.Yes).Select(o => new PrepaidOperatorComboBoxItem<INGJsonResponseGsmOperatorsDataOperator>(o.name, o)));
            if (operatorItem.data == null)
                return false;

            switch (operatorItem.data.RangeValue)
            {
                case INGJsonResponseRange.Borders:
                    if (amount < operatorItem.data.minAmount || amount > operatorItem.data.maxAmount)
                        return CheckFailed($"Kwota powinna znajdować się w zakresie {operatorItem.data.minAmount}-{operatorItem.data.maxAmount}");
                    break;
                case INGJsonResponseRange.Enumerator:
                    if (!operatorItem.data.amounts.Contains(amount))
                        return CheckFailed($"Kwota powinna być jedną z {String.Join(", ", operatorItem.data.amounts)}");
                    break;
                default:
                    throw new NotImplementedException();
            }

            //TODO niepoprawny numer telefonu
            (INGJsonResponsePaymentConfirmable response, bool requestProcessed) gsmPreloadResponse = PerformRequest<INGJsonResponsePaymentConfirmable>(
                "rengsmppreload", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestGsmPreload.Create(Token, (int)amount, operatorItem.data.id, "+48" + phoneNumber, GetAccountData().accountNumber.SimplifyAccountNumber())),
                null, true, null, null);
            if (!gsmPreloadResponse.requestProcessed)
                return false;

            return Confirm(gsmPreloadResponse.response.data, amount);
        }

        protected override INGHistoryFilter CreateFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact)
        {
            return new INGHistoryFilter() { DateFrom = dateFrom, DateTo = dateTo, Direction = direction, AmountExact = amountExact, Title = title };
        }

        protected override List<INGHistoryItem> GetHistoryItems(INGHistoryFilter filter = null)
        {
            //TODO może być numer konta w tytule

            //TODO wyświetlanie 15 dla getinbanku, dla ing do edycji
            int maxTransactionsPerPageCount = 50;

            List<INGHistoryItem> result = new List<INGHistoryItem>();

            int? pageCounter = null;
            for (int page = 1; (pageCounter == null || page <= pageCounter) && (filter.CounterLimit == 0 || result.Count < filter.CounterLimit); page++)
            {
                INGJsonResponseSign? sign = null;
                if (filter.Direction != null)
                    sign = filter.Direction == OperationDirection.Execute ? INGJsonResponseSign.Debit : INGJsonResponseSign.Credit;
                (INGJsonResponseHistory response, bool requestProcessed) historyResponse = PerformRequest<INGJsonResponseHistory>(
                    "rengetfury", HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestHistory.Create(Token,
                        filter.DateFrom,
                        filter.DateTo,
                        GetAccountData().accountNumber.SimplifyAccountNumber(),
                        5, //?
                        INGJsonResponseNoYes.Yes, //?
                        filter.Title,
                        filter.AmountFrom ?? 0,
                        filter.AmountTo ?? 9999999999999,
                        (page - 1) * maxTransactionsPerPageCount,
                        maxTransactionsPerPageCount,
                        sign,
                        filter.ShowIncomingTransfers,
                        filter.ShowInternalTransfers,
                        filter.ShowExternalTransfers,
                        filter.ShowCardTransactionsBlocks,
                        filter.ShowCardTransactions,
                        filter.ShowATM,
                        filter.ShowFees,
                        filter.ShowSmartSaver,
                        filter.ShowBlocksAndBlockReleases
                    )),
                    null, true, null, null);
                if (!historyResponse.requestProcessed)
                    return null;

                pageCounter = (int)(Math.Ceiling(historyResponse.response.data.numtrns / (double)maxTransactionsPerPageCount));

                //TODO szczegóły transakcji rengetfurydet
                result.AddRange(historyResponse.response.data.trns.Select(t => new INGHistoryItem(t.m)));
            }
            return result;
        }

        public override void GetDetailsFile(HistoryItem item, FileStream file)
        {
            (INGJsonResponseTransactionPDF response, bool requestProcessed) transactionPDFResponse = PerformRequest<INGJsonResponseTransactionPDF>(
                "renprepaccttranspdf", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestTransactionPDF.Create(Token, item.Id)),
                null, true, null, null);
            if (!transactionPDFResponse.requestProcessed)
                return;

            UriBuilder uriBuilder = new UriBuilder(new Uri(new Uri(BaseAddress), "rengetbin"));
            NameValueCollection query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["ref"] = transactionPDFResponse.response.data.refValue;
            query["att"] = "true";
            uriBuilder.Query = query.ToString();
            (INGJsonResponseBase response, bool requestProcessed) binResponse = PerformRequest<INGJsonResponseBase>(
                uriBuilder.ToString(), HttpMethod.Get, null,
                null,
                false, null,
                (Stream contentStream) =>
                {
                    contentStream.CopyTo(file);
                });
        }

        private bool Confirm(INGJsonResponsePaymentConfirmableData orderData, double? amount)
        {
            (INGJsonResponseAuthGetData response, bool requestProcessed) authGetDataResponse = PerformRequest<INGJsonResponseAuthGetData>(
                "renauthgetdata", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestAuthGetData.Create(Token, orderData.refValue)),
                null, true, null, null);
            if (!authGetDataResponse.requestProcessed)
                return false;
            if (authGetDataResponse.response.data.messageId != 0)
            {
                Message(authGetDataResponse.response.data.message);
                return false;
            }

            switch (authGetDataResponse.response.data.FactorValue)
            {
                case INGJsonResponseAuthFactor.None:
                    {
                        if (!PromptOKCancel($"Potwierdź wykonanie operacji na kwotę {amount}"))
                            return false;

                        (INGJsonResponseAuthConfirm response, bool requestProcessed) confirmResponse = PerformRequest<INGJsonResponseAuthConfirm>(
                            "renauthconfirm", HttpMethod.Post,
                            JsonConvert.SerializeObject(INGJsonRequestAuthConfirm.Create(Token, orderData.refValue)),
                            null, true, null, null);

                        if (!confirmResponse.requestProcessed)
                            return false;

                        return ConfirmAuthorizationFinish(orderData.refValue, confirmResponse.response.data.token, authGetDataResponse.response.data.confirmURN);
                    }
                case INGJsonResponseAuthFactor.AutoConfirm:
                    {
                        (INGJsonResponseAuthAutoConfirmConfirm response, bool requestProcessed) confirmResponse = PerformRequest<INGJsonResponseAuthAutoConfirmConfirm>(
                            "renauthconfirm", HttpMethod.Post,
                            JsonConvert.SerializeObject(INGJsonRequestAuthAutoConfirmConfirm.Create(Token, orderData.refValue)),
                            null, true, null, null);

                        Cookie browserCookie = Cookies.GetCookie("login.ingbank.pl", "TBN4VFFiLdynGrcM3aq");
                        if (browserCookie != null)
                            SaveCookie(("TBN4VFFiLdynGrcM3aq", browserCookie.Value, "/mojeing", "login.ingbank.pl"));

                        return ConfirmAuthorizationFinish(orderData.refValue, authGetDataResponse.response.data.token, authGetDataResponse.response.data.confirmURN);
                    }
                case INGJsonResponseAuthFactor.SMS:
                    {
                        (INGJsonResponseAuthSMSConfirm response, bool requestProcessed) confirmResponse = (null, false);

                        bool codeProceeded = false;
                        while (!codeProceeded)
                        {
                            string SMSCode = GetSMSCode();
                            if (SMSCode == null)
                                return false;

                            confirmResponse = PerformRequest<INGJsonResponseAuthSMSConfirm>(
                                "renauthconfirm", HttpMethod.Post,
                                JsonConvert.SerializeObject(INGJsonRequestAuthSMSConfirm.Create(Token, orderData.refValue, SMSCode)),
                                null, true, null, null);

                            if (!confirmResponse.requestProcessed)
                                return false;

                            if (confirmResponse.response.data.messageId != 0)
                                Message(confirmResponse.response.data.message);
                            else
                                codeProceeded = true;
                        }

                        return ConfirmAuthorizationFinish(orderData.refValue, confirmResponse.response.data.token, authGetDataResponse.response.data.confirmURN);
                    }
                case INGJsonResponseAuthFactor.Mobile:
                    {
                        if (!PromptOKCancel("Potwierdź operację na urządzeniu mobilnym"))
                            return false;

                        return Confirm(orderData, amount);
                    }
                case INGJsonResponseAuthFactor.AddBrowser:
                    {
                        string browserName = null;
                        if (PromptYesNo("Dodać przeglądarkę do zaufanych?"))
                        {
                            if (!authGetDataResponse.response.data.challenge.acceptNewBrowsers)
                            {
                                Message("Przekroczono limit zaufanych przeglądarek");
                            }
                            else
                            {
                                browserName = "C# client";
                            }
                        }

                        bool saveBrowserCookie = browserName != null;

                        (INGJsonResponseAuthAddBrowserConfirm response, bool requestProcessed) confirmResponse = PerformRequest<INGJsonResponseAuthAddBrowserConfirm>(
                            "renauthconfirm", HttpMethod.Post,
                            JsonConvert.SerializeObject(INGJsonRequestAuthAddBrowserConfirm.Create(Token, orderData.refValue, browserName)),
                            null, true, null, null);
                        return Confirm(orderData, null);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private bool ConfirmFastTransfer(INGJsonResponseFastTransferConfirmableData data, double? amount)
        {
            if (!PromptOKCancel($"Potwierdź wykonanie operacji na kwotę {amount}"))
                return false;

            (INGJsonResponseFastTransferAuthConfirm response, bool requestProcessed) confirmResponse = PerformRequest<INGJsonResponseFastTransferAuthConfirm>(
                "renconfirm", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestFastTransferAuthConfirm.Create(Token, data.docId)),
                null, true, null, null);

            if (!confirmResponse.requestProcessed)
                return false;

            return confirmResponse.response.StatusValue == INGJsonResponseStatus.OK;
        }

        private bool ConfirmAuthorizationFinish(string refValue, string dataToken, string confirmUrl)
        {
            (INGJsonResponseAuthFinished response, bool requestProcessed) finishedResponse = PerformRequest<INGJsonResponseAuthFinished>(
                confirmUrl, HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestAuthFinished.Create(Token, refValue, dataToken)),
                null, true, null, null);

            if (Token == null && finishedResponse.requestProcessed)
                Token = finishedResponse.response.data.token;

            return finishedResponse.requestProcessed;
        }

        private static HttpRequestMessage CreateHttpRequestMessage(string requestUri, HttpMethod method, string jsonContent = null)
        {
            HttpRequestMessage message = new HttpRequestMessage(method, requestUri);
            message.Headers.Add("X-Wolf-Protection", "0");

            if (jsonContent != null)
                message.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            return message;
        }

        private (T, bool) PerformRequest<T>(string requestUri, HttpMethod method, string jsonContent,
            string errorMessage,
            bool readContent,
            Func<T, bool> invalidResponse,
            Action<Stream> useStream)
            where T : INGJsonResponseBase
        {
            using (HttpRequestMessage message = CreateHttpRequestMessage(requestUri, method, jsonContent))
            {
                using (HttpResponseMessage response = httpClient.SendAsync(message).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        string responseStr = null;

                        useStream?.Invoke(content.ReadAsStreamAsync().Result);

                        if (!readContent)
                            return (null, false);

                        responseStr = content.ReadAsStringAsync().Result;
                        T jsonResponse = JsonConvert.DeserializeObject<T>(responseStr);
                        if (invalidResponse == null ? jsonResponse.StatusValue != INGJsonResponseStatus.OK : invalidResponse.Invoke(jsonResponse))
                        {
                            Message(errorMessage ?? jsonResponse.msg);
                            return (null, false);
                        }
                        return (jsonResponse, true);
                    }
                }
            }
        }

        private string GetSMSCode()
        {
            return PromptString("Kod SMS", @"^\d{8}$");
        }

        //TODO to samo co w Velo
        private static (FastTransferType? type, string paData, string pblData) GetDataFromFastTransfer(string transferId)
        {
            bool pbl = transferId?.Length == 32;
            bool pa = transferId?.Length == 36;
            FastTransferType? type = null;
            if (pbl)
                type = FastTransferType.PayByLink;
            if (pa)
                type = FastTransferType.PA;
            return (type, transferId, transferId);
        }

        private string Hmac(string key, string text)
        {
            int[] n = Str2BinL(key);
            if (n.Length > 16)
                n = CoreMd5(n, 8 * key.Length);
            else if (n.Length < 16)
                Array.Resize<int>(ref n, 16);
            int[] iPad = new int[16];
            int[] oPad = new int[16];
            for (int i = 0; i < 16; i++)
            {
                iPad[i] = 909522486 ^ n[i];
                oPad[i] = 1549556828 ^ n[i];
            }
            int[] hash = CoreMd5(iPad.Concat(Str2BinL(text)).ToArray(), 512 + 8 * text.Length);
            int[] l2 = CoreMd5(oPad.Concat(hash).ToArray(), 672);

            return BinL2Hex(l2);
        }

        private int[] Str2BinL(string text)
        {
            Dictionary<int, int> binDict = new Dictionary<int, int>();
            for (int i = 0; i < 8 * text.Length; i += 8)
            {
                int binValue = 0;
                if (binDict.ContainsKey(i >> 5))
                    binValue = binDict[i >> 5];
                binDict[i >> 5] = binValue | (255 & (int)text[i / 8]) << 24 - i % 32;
            }

            int[] bin = new int[binDict.Max(k => k.Key) + 1];
            for (int i = 0; i < bin.Length; i++)
                bin[i] = binDict[i];
            return bin;
        }

        private int[] CoreMd5(int[] x, int length)
        {
            int newIndex = Math.Max(length >> 5, 15 + (length + 64 >> 9 << 4));
            if (x.Length <= newIndex)
                Array.Resize<int>(ref x, newIndex + 1);
            x[length >> 5] |= 128 << 24 - length % 32;
            x[15 + (length + 64 >> 9 << 4)] = length;
            int[] t = new int[80];
            int a = 1732584193;
            int b = -271733879;
            int c = -1732584194;
            int d = 271733878;
            int e = -1009589776;
            for (int i = 0; i < x.Length; i += 16)
            {
                int oldA = a;
                int oldB = b;
                int oldC = c;
                int oldD = d;
                int oldE = e;
                for (int j = 0; j < 80; j++)
                {
                    t[j] = j < 16 ? x[i + j] : BitRol(t[j - 3] ^ t[j - 8] ^ t[j - 14] ^ t[j - 16], 1);
                    int h = SafeAdd(SafeAdd(BitRol(a, 5), HmacR(j, b, c, d)), SafeAdd(SafeAdd(e, t[j]), HmacO(j)));
                    e = d;
                    d = c;
                    c = BitRol(b, 30);
                    b = a;
                    a = h;
                }
                a = SafeAdd(a, oldA);
                b = SafeAdd(b, oldB);
                c = SafeAdd(c, oldC);
                d = SafeAdd(d, oldD);
                e = SafeAdd(e, oldE);
            }
            return new int[] { a, b, c, d, e };
        }

        private int BitRol(int num, int cnt)
        {
            return (int)((uint)(num << cnt) | ((uint)num) >> 32 - cnt);
        }

        private int HmacR(int e, int t, int n, int i)
        {
            return e < 20 ? t & n | ~t & i : e < 40 ? t ^ n ^ i : e < 60 ? t & n | t & i | n & i : t ^ n ^ i;
        }

        private int SafeAdd(int x, int y)
        {
            int lsw = (65535 & x) + (65535 & y);
            return ((x >> 16) + (y >> 16) + (lsw >> 16)) << 16 | 65535 & lsw;
        }

        private int HmacO(int e)
        {
            return e < 20 ? 1518500249 : e < 40 ? 1859775393 : e < 60 ? -1894007588 : -899497514;
        }

        private string BinL2Hex(int[] binArray)
        {
            string t = String.Empty;
            for (int i = 0; i < 4 * binArray.Length; i++)
            {
                t += hexTable[(binArray[i >> 2]) >> 8 * (3 - i % 4) + 4 & 15];
                t += hexTable[(binArray[i >> 2]) >> 8 * (3 - i % 4) & 15];
            }
            return t;
        }
    }
}
