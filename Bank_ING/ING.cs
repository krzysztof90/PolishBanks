using BankService.LocalTools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using static BankService.Bank_ING.INGJsonRequest;
using static BankService.Bank_ING.INGJsonResponse;

namespace BankService.Bank_ING
{
    [BankTypeAttribute(BankType.ING)]
    public class ING : BankBase<INGHistoryItem, INGHistoryFilter>
    {
        private const string hexTable = "0123456789abcdef";

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

        public override bool FastTransferMandatoryTransferId => false;
        public override bool FastTransferMandatoryBrowserCookies => false;
        public override bool FastTransferMandatoryCookie => true;
        public override bool TransferMandatoryTitle => true;
        public override bool PrepaidTransferMandatoryRecipient => false;

        protected override string BaseAddress => "https://login.ingbank.pl/mojeing/rest/";

        protected override bool LoginRequest(string login, string password)
        {
            Token = null;

            (INGJsonResponseLogin jsonResponseLogin, bool requestProcessed) loginReqest = PerformRequest<INGJsonResponseLogin>(
                "renchecklogin", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestLogin.Create(login)),
                "Niepoprawny login",
                true, null, null);
            if (!loginReqest.requestProcessed)
                return false;

            string pwdHash = CreatePwdHash(loginReqest.jsonResponseLogin.data.salt, loginReqest.jsonResponseLogin.data.mask, loginReqest.jsonResponseLogin.data.key, password);
            if (pwdHash == null)
                return false;

            (INGJsonResponseLoginPassword jsonResponseLoginPassword, bool requestProcessed) passwordReqest = PerformRequest<INGJsonResponseLoginPassword>(
                "renlogin", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestLoginPassword.Create(pwdHash, login)),
                null, true, null, null);
            if (!passwordReqest.requestProcessed)
                return false;

            Token = passwordReqest.jsonResponseLoginPassword.data.token;

            return true;
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
            if (mask.LastIndexOf('*') > password.Length + 1)
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

        protected override bool PostLoginRequest()
        {
            bool result = base.PostLoginRequest();

            if (result)
            {
                AccountsDetails = GetAccountsDetails();
            }

            return result;
        }

        protected override bool LogoutRequest()
        {
            (INGJsonResponseLogout jsonResponseLogout, bool requestProcessed) logoutReqest = PerformRequest<INGJsonResponseLogout>(
                "renlogout", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestLogout.Create()),
                null, true, null, null);
            if (!logoutReqest.requestProcessed)
                return false;

            LogoutUrl = logoutReqest.jsonResponseLogout.data.url;
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
            (INGJsonResponsePing jsonResponsePing, bool requestProcessed) extendReqest = PerformRequest<INGJsonResponsePing>(
                "renping", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestPing.Create(Token)),
                null, true, null, null);

            return extendReqest.requestProcessed;
        }

        private INGJsonResponseAccounts GetAccountsDetails()
        {
            (INGJsonResponseAccounts jsonResponseLogoutAccounts, bool requestProcessed) accountsReqest = PerformRequest<INGJsonResponseAccounts>(
                "rengetallingprds", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestAccounts.Create(Token)),
                null, true, null, null);
            if (!accountsReqest.requestProcessed)
                throw new NotImplementedException();

            return accountsReqest.jsonResponseLogoutAccounts;
        }

        //TODO kilka rachunków, do wyboru, Suma wszystkich
        public override (string accountNumber, double availableFunds) GetAccountData()
        {
            return (AccountsDetails.data.accts.cur.accts.Single().acct, AccountsDetails.data.accts.cur.accts.Single().plnbal);
        }

        public override bool MakeTransfer(string recipient, string address, string accountNumber, string title, double amount)
        {
            (INGJsonResponsePaymentCheckAccount jsonResponseCheckAccount, bool requestProcessed) transferReqest = PerformRequest<INGJsonResponsePaymentCheckAccount>(
                "rengetacttkirinfo", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestCheckAccount.Create(Token, accountNumber.SimplifyAccountNumber())),
                null, true, null, null);
            if (!transferReqest.requestProcessed)
                return false;

            //TODO długość pól
            (INGJsonResponsePaymentConfirmable jsonResponsePaymentOrder, bool requestProcessed) paymentOrderRequest = PerformRequest<INGJsonResponsePaymentConfirmable>(
                "renpayord", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestPaymentOrder.Create(Token, amount, recipient, address, "", "", accountNumber.SimplifyAccountNumber(), GetAccountData().accountNumber.SimplifyAccountNumber(), title, "", "", "", "S")),
                null, true, null, null);
            if (!paymentOrderRequest.requestProcessed)
                return false;

            return Confirm(paymentOrderRequest.jsonResponsePaymentOrder.data);
        }

        protected override void PostTransfer()
        {
            base.PostTransfer();
            AccountsDetails = null;
        }

        protected override string CleanFastTransferUrl(string transferId)
        {
            return transferId;
        }

        protected override bool LoginRequestForFastTransfer(string login, string password, string transferId, /*Browser browser,*/ string cookie)
        {
            Cookies.Add(new Cookie("JSESSIONID", cookie, "/mojeing", "login.ingbank.pl"));

            return LoginRequest(login, password) && PostLoginRequest();
        }

        protected override string MakeFastTransfer(string transferId, /*Browser browser,*/ string cookie)
        {
            //TODO potwierdzanie danych przelewu
            (INGJsonResponseFastTransfer jsonResponseFastTransfer, bool requestProcessed) fastTransferRequest = PerformRequest<INGJsonResponseFastTransfer>(
                "rengetdirtrndata", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestFastTransfer.Create(Token)),
                null, true, null, null);
            if (!fastTransferRequest.requestProcessed)
                return null;

            (INGJsonResponsePaymentConfirmable jsonResponsePaymentDirt, bool requestProcessed) paymentDirtRequest = PerformRequest<INGJsonResponsePaymentConfirmable>(
                "renpaydirtrn", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestPaymentDirt.Create(Token, GetAccountData().accountNumber.SimplifyAccountNumber())),
                null, true, null, null);
            if (!paymentDirtRequest.requestProcessed)
                return null;

            if (!Confirm(paymentDirtRequest.jsonResponsePaymentDirt.data))
                return null;

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
            //TODO niepoprawny numer telefonu

            if (amount != Math.Truncate(amount))
            {
                return CheckFailed("Kwota nie może zawierać miejsc po przecinku");
            }

            (INGJsonResponseGsmOperators jsonResponseGsmOperators, bool requestProcessed) gsmOperatorsRequest = PerformRequest<INGJsonResponseGsmOperators>(
                "rengetgsmppopr", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestGsmOperators.Create(Token)),
                null, true, null, null);
            if (!gsmOperatorsRequest.requestProcessed)
                return false;

            (string name, INGJsonResponseGsmOperatorsDataOperator data) operatorItem = PromptComboBox<INGJsonResponseGsmOperatorsDataOperator>("Operator", gsmOperatorsRequest.jsonResponseGsmOperators.data.opers.Where(o => o.VisibleValue == INGJsonResponseNoYes.Yes).Select(o => new PrepaidOperatorComboBoxItem<INGJsonResponseGsmOperatorsDataOperator>(o.name, o)));
            if (operatorItem.data == null)
                return false;

            switch (operatorItem.data.RangeValue)
            {
                case INGJsonResponseRange.Borders:
                    if (amount < operatorItem.data.minAmount || amount > operatorItem.data.maxAmount)
                    {
                        return CheckFailed($"Kwota powinna znajdować się w zakresie {operatorItem.data.minAmount}-{operatorItem.data.maxAmount}");
                    }
                    break;
                case INGJsonResponseRange.Enumerator:
                    if (!operatorItem.data.amounts.Contains(amount))
                    {
                        return CheckFailed($"Kwota powinna być jedną z {String.Join(", ", operatorItem.data.amounts)}");
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            (INGJsonResponsePaymentConfirmable jsonResponseGsmPreload, bool requestProcessed) gsmPreloadRequest = PerformRequest<INGJsonResponsePaymentConfirmable>(
                "rengsmppreload", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestGsmPreload.Create(Token, (int)amount, operatorItem.data.id, "+48" + phoneNumber, GetAccountData().accountNumber.SimplifyAccountNumber())),
                null, true, null, null);
            if (!gsmPreloadRequest.requestProcessed)
                return false;

            return Confirm(gsmPreloadRequest.jsonResponseGsmPreload.data);
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
                (INGJsonResponseHistory jsonResponseHistory, bool requestProcessed) historyRequest = PerformRequest<INGJsonResponseHistory>(
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
                if (!historyRequest.requestProcessed)
                    return null;

                pageCounter = (int)(Math.Ceiling(historyRequest.jsonResponseHistory.data.numtrns / (double)maxTransactionsPerPageCount));

                //TODO szczegóły transakcji rengetfurydet
                result.AddRange(historyRequest.jsonResponseHistory.data.trns.Select(t => new INGHistoryItem(t.m)));
            }
            return result;
        }

        public override void GetDetailsFile(HistoryItem item, FileStream file)
        {
            (INGJsonResponseTransactionPDF jsonResponseTransactionPDF, bool requestProcessed) transactionPDFRequest = PerformRequest<INGJsonResponseTransactionPDF>(
                "renprepaccttranspdf", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestTransactionPDF.Create(Token, item.Id)),
                null, true, null, null);
            if (!transactionPDFRequest.requestProcessed)
                return;

            UriBuilder uriBuilder = new UriBuilder(new Uri(new Uri(BaseAddress), "rengetbin"));
            NameValueCollection query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["ref"] = transactionPDFRequest.jsonResponseTransactionPDF.data.refValue;
            query["att"] = "true";
            uriBuilder.Query = query.ToString();
            (INGJsonResponseBase jsonResponse, bool requestProcessed) binRequest = PerformRequest<INGJsonResponseBase>(
                uriBuilder.ToString(), HttpMethod.Get, null,
                null,
                false, null,
                (Stream contentStream) =>
                {
                    contentStream.CopyTo(file);
                });
        }

        private bool Confirm(INGJsonResponsePaymentConfirmableData orderData)
        {
            switch (orderData.ModeValue)
            {
                case INGJsonResponseOrderMode.Web:
                    {
                        (INGJsonResponseConfirm jsonResponseConfirm, bool requestProcessed) confirmRequest = PerformRequest<INGJsonResponseConfirm>(
                            "renconfirm", HttpMethod.Post,
                            JsonConvert.SerializeObject(INGJsonRequestConfirm.Create(Token, orderData.docId)),
                            null, true, null, null);

                        return confirmRequest.requestProcessed;
                    }
                case INGJsonResponseOrderMode.Code:
                    {
                        bool codeProceeded = false;
                        while (!codeProceeded)
                        {
                            string SMSCode = GetSMSCode();
                            if (SMSCode == null)
                                return false;

                            (INGJsonResponseConfirm jsonResponseConfirm, bool requestProcessed) confirmRequest = PerformRequest<INGJsonResponseConfirm>(
                                "renconfirm", HttpMethod.Post,
                                JsonConvert.SerializeObject(INGJsonRequestConfirmCode.Create(Token, SMSCode, orderData.docId)),
                                null,
                                true,
                                (INGJsonResponseConfirm jsonResponseConfirm) => { return jsonResponseConfirm.StatusValue != INGJsonResponseStatus.OK && jsonResponseConfirm.code != "2204"; },
                                null);
                            if (!confirmRequest.requestProcessed)
                                return false;

                            if (confirmRequest.jsonResponseConfirm.StatusValue != INGJsonResponseStatus.OK)
                                Message(confirmRequest.jsonResponseConfirm.msg);
                            else
                                codeProceeded = true;
                        }
                        return true;
                    }
                case INGJsonResponseOrderMode.Mobile:
                    {
                        if (!PromptOKCancel("Potwierdź operację na urządzeniu mobilnym"))
                            return false;

                        (INGJsonResponseConfirmMobile jsonResponseConfirmMobile, bool requestProcessed) confirmRequest = PerformRequest<INGJsonResponseConfirmMobile>(
                            "renconfirm", HttpMethod.Post,
                            JsonConvert.SerializeObject(INGJsonRequestConfirm.Create(Token, orderData.docId)),
                            null, true, null, null);
                        if (!confirmRequest.requestProcessed)
                            return false;

                        if (confirmRequest.jsonResponseConfirmMobile.data == null)
                            return true;

                        return Confirm(confirmRequest.jsonResponseConfirmMobile.data);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private static HttpRequestMessage CreateHttpRequestMessage(string requestUri, HttpMethod method, string jsonContent = null)
        {
            HttpRequestMessage message = new HttpRequestMessage(method, requestUri);
            message.Headers.Add("X-Wolf-Protection", String.Empty);

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

        private  string GetSMSCode()
        {
            return PromptString("Kod SMS", @"^\d{8}$");
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
