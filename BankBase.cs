using BankService.ConfirmText;
using BankService.LocalTools;
using BankService.MandatoryTransferDatas;
using BankService.SMSCodes;
using BankService.Tax.TaxCreditorIdentifiers;
using BankService.Tax.TaxPeriods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Tools;
using Tools.Enums;

namespace BankService
{
    public abstract class BankBaseBase : ServiceManager<HttpClient>, IDisposable
    {
        //TODO labels, translator

        private event Action<string> onMessage;
        List<Action<string>> onMessageDelegates = new List<Action<string>>();
        public event Action<string> OnMessage
        {
            add
            {
                onMessage += value;
                onMessageDelegates.Add(value);
            }
            remove
            {
                onMessage -= value;
                onMessageDelegates.Remove(value);
            }
        }
        public void SuppressMessagesEvents()
        {
            foreach (Action<string> onMessageAction in onMessageDelegates)
                onMessage -= onMessageAction;
        }
        public void RestoreMessagesEvents()
        {
            foreach (Action<string> onMessageAction in onMessageDelegates)
                onMessage += onMessageAction;
        }

        private event Action<string> onMessageAdditional;
        public event Action<string> OnMessageAdditional
        {
            add
            {
                onMessageAdditional += value;
            }
            remove
            {
                onMessageAdditional -= value;
            }
        }

        public event Func<List<(string name, string value, string path, string domain)>> OnGetCookies;
        public event Action<(string name, string value, string path, string domain)> OnSetCookie;
        public event Action<(string name, string path, string domain)> OnRemoveCookie;
        public event Func<string, string, bool> OnPromptYesNo;
        public event Func<string, string, System.Drawing.Image, bool> OnPromptOKCancel;
        public event Func<string, string, string> OnPromptString;
        public event Func<string, IEnumerable<SelectComboBoxItemBase>, bool, SelectComboBoxItemBase> OnPromptComboBoxOperator;

        public event Action OnLogging;

        public event Action OnAvailableFundsClear;

        public event Action BeforeRefreshAccountsData;
        public event Action AfterRefreshAccountsData;

        protected WebRequestHandler Handler;
        protected CookieContainer Cookies;
        protected CookieCollection DomainCookies => Cookies.GetCookies(new Uri(BaseAddress));

        private System.Threading.Timer heartbeatTimer;

        public abstract Country Country { get; }
        protected abstract int HeartbeatInterval { get; }
        protected abstract SMSCodeValidator SMSCodeValidator { get; }
        public abstract bool EnabledFastTransfer { get; }
        public abstract bool EnabledPaymentOfServices { get; }
        public abstract bool EnabledPrepaidNIF { get; }
        public abstract bool AllowAlternativeLoginMethod { get; }
        public abstract bool TransferMandatoryRecipient { get; }
        public abstract bool TransferMandatoryTitle { get; }
        //TODO don't display this on form if false
        public abstract bool PrepaidTransferMandatoryRecipient { get; }
        protected abstract string BaseAddress { get; }
        protected abstract void CleanHttpClient();
        protected abstract bool LoginRequest(string login, string password, List<object> additionalAuthorization);
        protected abstract bool LogoutRequest();
        protected abstract bool TryExtendSession();
        protected abstract AccountData SelectAccountData();
        protected abstract List<AccountData> GetAccountsDataMain(bool update);
        protected abstract void ClearAccountsData(bool totalClean);
        public abstract bool EmptyAccountsData { get; }
        //TODO currency transfers
        //TODO phone blik transfers
        public abstract bool MakeTransfer(string recipient, string address, string accountNumber, string title, double amount);
        public abstract bool MakeTaxTransfer(string taxType, string accountNumber, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier, string creditorName, string obligationId, double amount);
        protected abstract string CleanFastTransferUrl(string transferId);
        protected abstract bool LoginRequestForFastTransfer(string login, string password, List<object> additionalAuthorization, string transferId);
        protected abstract string MakeFastTransfer(string transferId);
        public abstract bool MakePaymentOfServicesTransfer(string entity, string reference, double amount);
        protected abstract bool MakePrepaidTransfer(string recipient, string phoneNumber, double amount, string nif);
        public abstract HistoryFilter CreateHistoryFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact);
        public abstract List<HistoryItem> GetHistory(HistoryFilter filter = null);
        public abstract bool GetDetailsFile(HistoryItem item, Func<ContentDispositionHeaderValue, FileStream> file);

        public AccountData selectedAccountData;
        public AccountData SelectedAccountData
        {
            get
            {
                if (selectedAccountData == null)
                {
                    SelectedAccountData = SelectAccountData();
                }
                return selectedAccountData;
            }
            set
            {
                selectedAccountData = value;
                if (selectedAccountData != null && !GetAccountsData(false).Contains(selectedAccountData))
                    throw new ArgumentException();
            }
        }
        public AccountData RefreshedSelectedAccountData
        {
            get
            {
                AccountData currentAccountData = SelectedAccountData;
                GetAccountsData(true);
                if (!currentAccountData.Equals(SelectedAccountData))
                    throw new ArgumentException();
                return SelectedAccountData;
            }
        }

        public BankBaseBase() : base()
        {
        }

        protected override HttpClient CreateClient()
        {
            Cookies = new CookieContainer();
            Handler = new WebRequestHandler
            {
                AllowAutoRedirect = false,
                //AllowAutoRedirect = true,
                CookieContainer = Cookies
            };
            HttpClient httpClient = new HttpClient(Handler);
            httpClient.BaseAddress = new Uri(BaseAddress);
            //httpClient.DefaultRequestVersion = new Version(2, 0);

            return httpClient;
        }

        protected void SaveCookie((string name, string value, string path, string domain) cookie)
        {
            OnSetCookie?.Invoke(cookie);
        }
        protected void SaveCookie(Cookie cookie)
        {
            SaveCookie((cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
        }
        protected void RemoveSavedCookie((string name, string path, string domain) cookie)
        {
            OnRemoveCookie?.Invoke(cookie);
        }
        protected void Message(string text)
        {
            onMessage?.Invoke(text);
        }
        protected void MessageAdditional(string text)
        {
            onMessageAdditional?.Invoke(text);
        }
        protected bool PromptYesNo(string text, string caption = null)
        {
            return OnPromptYesNo?.Invoke(text, caption ?? String.Empty) ?? false;
        }
        protected bool PromptOKCancel(string text, string caption = null, System.Drawing.Image image = null)
        {
            return OnPromptOKCancel?.Invoke(text, caption ?? String.Empty, image) ?? false;
        }
        protected string PromptString(string text, string pattern = null)
        {
            return OnPromptString?.Invoke(text, pattern ?? String.Empty) ?? null;
        }
        protected (string name, T data) PromptComboBox<T>(string text, IEnumerable<SelectComboBoxItem<T>> dataSource, bool selectOnlyOne)
        {
            SelectComboBoxItemBase item = OnPromptComboBoxOperator?.Invoke(text, dataSource, selectOnlyOne);
            if (item == null)
                return (null, default(T));
            return (item.Name, ((SelectComboBoxItem<T>)item).Data);
        }

        protected void CallAvailableFundsClear()
        {
            OnAvailableFundsClear?.Invoke();
        }

        public List<AccountData> GetAccountsData(bool update)
        {
            if (update)
                BeforeRefreshAccountsData?.Invoke();
            List<AccountData> accountsData = GetAccountsDataMain(update);
            if (update)
                AfterRefreshAccountsData?.Invoke();
            return accountsData;
        }

        private void InitClient(bool includeCookies)
        {
            base.InitClient();

            if (includeCookies)
            {
                List<(string name, string value, string path, string domain)> cookies = OnGetCookies?.Invoke() ?? new List<(string name, string value, string path, string domain)>();
                foreach ((string name, string value, string path, string domain) cookie in cookies)
                    Cookies.Add(new Cookie(cookie.name, cookie.value, cookie.path, cookie.domain));
            }
        }

        protected bool CheckSelectedAccount()
        {
            return SelectedAccountData == null ? CheckFailed("Nie wybrano konta") : true;
        }

        public bool LoginAlternative()
        {
            return Login(() => PerformLoginAlternative());
        }

        public virtual bool PerformLoginAlternative()
        {
            return true;
        }

        public bool Login(string login, string password, List<object> additionalAuthorization)
        {
            if (String.IsNullOrEmpty(login) || String.IsNullOrEmpty(password))
                return CheckFailed("Niepoprawny login i hasło");

            return Login(() => PerformLogin(login, password, additionalAuthorization));
        }

        private bool Login(Func<bool> loginAction)
        {
            InitClient(true);

            if (!loginAction())
                return false;

            heartbeatTimer = new System.Threading.Timer((state) =>
            {
                if (!Logged)
                    heartbeatTimer.Dispose();
                else
                {
                    if (!TryExtendSession())
                        heartbeatTimer.Dispose();
                }
            }, null, 0, HeartbeatInterval * 1000);

            return SetLogged(true);
        }

        protected bool PerformLogin(string login, string password, List<object> additionalAuthorization)
        {
            //TODO handle unblocking account after giving wrong password, or just open page with recovery
            if (!LoginRequest(login, password, additionalAuthorization))
                return false;

            if (!PostLoginRequest())
                return false;

            return true;
        }

        protected virtual bool PostLoginRequest()
        {
            return true;
        }

        protected override bool LogoutMain()
        {
            bool logout = LogoutRequest();
            ClearAccountsData(true);
            return logout;
        }

        //TODO use in TryExtendSession
        protected void NoteExpiredSession()
        {
            Logged = false;
        }

        private bool PreValidateTransfer()
        {
            if (!CheckSelectedAccount())
                return false;

            return true;
        }

        private bool ValidateTransferData(List<MandatoryTransferData> transferDatas)
        {
            foreach (MandatoryTransferData transferData in transferDatas)
                if (!transferData.Validate())
                    return CheckFailed("Dane nie mogą być puste");
            return true;
        }

        private bool ValidateTransferAmount(double amount)
        {
            if (amount > RefreshedSelectedAccountData.AvailableFunds)
                return CheckFailed("Niewystarczające środki na koncie");
            return true;
        }

        private bool PerformAndFinishTransfer(Func<bool> transferAction)
        {
            bool performed = transferAction();

            if (performed)
            {
                //TODO in some banks there should be: click OK for end/finalize confirming
                Message("Wykonano");

                ClearAccountsData(false);
            }
            else
                MessageAdditional("Anulowano");

            return performed;
        }

        public bool PerformTransfer(string recipient, string address, string accountNumber, string title, double amount)
        {
            if (!PreValidateTransfer())
                return false;

            if (!ValidateTransferData(new List<MandatoryTransferData>()
            {
                new MandatoryTransferDataString(recipient, TransferMandatoryRecipient),
                new MandatoryTransferDataString(accountNumber),
                new MandatoryTransferDataString(title, TransferMandatoryTitle),
                new MandatoryTransferDataAmount(amount)
            }))
                return false;

            if (!ValidateTransferAmount(amount))
                return false;

            return PerformAndFinishTransfer(() => MakeTransfer(recipient, address, accountNumber, title, amount));
        }

        public bool PerformTaxTransfer(string taxType, string accountNumber, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier, string creditorName, string obligationId, double amount)
        {
            if (!PreValidateTransfer())
                return false;

            if (!ValidateTransferData(new List<MandatoryTransferData>()
            {
                new MandatoryTransferDataString(accountNumber),
                new MandatoryTransferDataAmount(amount)
            }))
                return false;

            if (!ValidateTransferAmount(amount))
                return false;

            if (!period.Validate((text) => Message(text)))
                return false;

            return PerformAndFinishTransfer(() => MakeTaxTransfer(taxType, accountNumber, period, creditorIdentifier, creditorName, obligationId, amount));
        }

        public bool PerformPrepaidTransfer(string recipient, string phoneNumber, double amount, string nif)
        {
            //TODO remove country prefix

            if (!PreValidateTransfer())
                return false;

            if (!ValidateTransferData(new List<MandatoryTransferData>()
            {
                new MandatoryTransferDataString(recipient, PrepaidTransferMandatoryRecipient),
                new MandatoryTransferDataString(phoneNumber),
                new MandatoryTransferDataAmount(amount)
            }))
                return false;

            if (!ValidateTransferAmount(amount))
                return false;

            //TODO phone number validation

            return PerformAndFinishTransfer(() => MakePrepaidTransfer(recipient, phoneNumber, amount, nif));
        }

        public bool PerformPaymentOfServicesTransfer(string entity, string reference, double amount)
        {
            if (!PreValidateTransfer())
                return false;

            if (!ValidateTransferData(new List<MandatoryTransferData>()
            {
                new MandatoryTransferDataString(entity),
                new MandatoryTransferDataString(reference),
                new MandatoryTransferDataAmount(amount)
            }))
                return false;

            if (!ValidateTransferAmount(amount))
                return false;

            return PerformAndFinishTransfer(() => MakePaymentOfServicesTransfer(entity, reference, amount));
        }

        public bool PerformFastTransfer(string login, string password, List<object> additionalAuthorization, string transferId)
        {
            if (transferId == null)
                return CheckFailed("Wymagany numer przelewu");
            string transferIdMain = CleanFastTransferUrl(transferId);
            if (transferIdMain == null)
                return CheckFailed("Niepoprawny numer");

            if (String.IsNullOrEmpty(login) || String.IsNullOrEmpty(password))
                return CheckFailed("Niepoprawny login i hasło");

            InitClient(true);

            OnLogging?.Invoke();

            if (!LoginRequestForFastTransfer(login, password, additionalAuthorization, transferIdMain))
                return false;

            if (!CheckSelectedAccount())
                return false;

            string redirectAddress = MakeFastTransfer(transferIdMain);
            if (redirectAddress == null)
                return false;

            Message("Wykonano");

            Process.Start(redirectAddress);

            ClearAccountsData(true);

            return true;
        }

        private (bool success, string fileName, FileStream file) GetDetailsFile(HistoryItem item)
        {
            string fileName = null;
            FileStream file = null;
            bool success = GetDetailsFile(item, (ContentDispositionHeaderValue headerContentDisposition) =>
            {
                string rawFileName = headerContentDisposition.FileName ?? headerContentDisposition.FileNameStar;
                fileName = FilesOperations.GetUniqueFileName(Path.Combine(System.IO.Path.GetTempPath(), rawFileName.Replace("\"", String.Empty)));
                file = File.Create(fileName);
                return file;
            });
            return (success, fileName, file);
        }

        public void OpenDetailsFile(HistoryItem item)
        {
            (bool success, string fileName, FileStream file) = GetDetailsFile(item);

            if (success && fileName != null)
                //TODO dialog with checkboxes "Open" and "Show in folder"
                Process.Start(fileName);
        }

        private (IEnumerable<HistoryItem> operations, bool amountReach) GetTransfersMade(HistoryFilter filter, double amount, Func<HistoryItem, bool> compareOperationFunction)
        {
            List<HistoryItem> items = GetHistory(filter);

            if (items == null)
                return (null, false);

            IEnumerable<HistoryItem> operations = items.Where(o => compareOperationFunction(o)).ToList();

            double currentAmount = operations.Any() ? operations.Sum(o => o.Amount) : 0;

            return (operations, currentAmount >= amount);
        }

        private List<FileStream> GetConfirmationFiles(double amount, HistoryFilter filter, Func<HistoryItem, bool> compareOperationFunction)
        {
            (IEnumerable<HistoryItem> operations, bool amountReach) = GetTransfersMade(filter, amount, compareOperationFunction);
            if (!amountReach)
                return new List<FileStream>();
            //TODO fileName instead of file because disposed
            return operations.Select(o => GetDetailsFile(o).file).ToList();
        }

        private (bool toPerform, double newAmount) CheckTransferMade(HistoryFilter filter, double amount, Func<HistoryItem, bool> compareOperationFunction)
        {
            (IEnumerable<HistoryItem> operations, bool amountReach) = GetTransfersMade(filter, amount, compareOperationFunction);

            if (operations == null)
                return (false, 0);

            if (!operations.Any())
                return (true, amount);

            if (amountReach)
                return (CheckFailed($"Wykonano wcześniej ({String.Join(", ", operations.Select(o => o.OrderDate.Display("dd.MM.yyyy")))})"), 0);
            else
            {
                double currentAmount = operations.Any() ? operations.Sum(o => o.Amount) : 0;
                Message($"Wykonano wcześniej ({String.Join(", ", operations.Select(o => o.OrderDate.Display("dd.MM.yyyy")))}), ale kwota jest mniejsza o {amount - currentAmount}");
                return (true, amount - currentAmount);
            }
        }

        private Func<HistoryItem, bool> CompareOperationFunctionTransfers(string accountNumber, string title)
        {
            return (HistoryItem o) => o.IsTransfer && AccountNumberTools.CompareAccountNumbers(o.ToAccountNumber, accountNumber) && (title == null || o.CompareTitle(title));
        }

        private Func<HistoryItem, bool> CompareOperationFunctionTaxTransfers(string taxType, string accountNumber, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier, string creditorName, string obligationId)
        {
            return (HistoryItem o) => o.IsTaxTransfer && AccountNumberTools.CompareAccountNumbers(o.ToAccountNumber, accountNumber) && o.CompareTax(taxType, period, creditorIdentifier);
        }

        private Func<HistoryItem, bool> CompareOperationFunctionPaymentOfServicesTransfers(string entity, string reference)
        {
            return (HistoryItem o) => o.IsPaymentOfServices && entity == o.PaymentOfServicesEntityNumber && o.ComparePaymentOfServicesReferenceNumber(reference);
        }

        public IEnumerable<FileStream> GetConfirmationFilesTransfers(string accountNumber, string title, double amount, HistoryFilter filter)
        {
            return GetConfirmationFiles(amount, filter, CompareOperationFunctionTransfers(accountNumber, title));
        }

        public IEnumerable<FileStream> GetConfirmationFilesTaxTransfers(string taxType, string accountNumber, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier, string creditorName, string obligationId, double amount, HistoryFilter filter)
        {
            return GetConfirmationFiles(amount, filter, CompareOperationFunctionTaxTransfers(taxType, accountNumber, period, creditorIdentifier, creditorName, obligationId));
        }

        public IEnumerable<FileStream> GetConfirmationFilesPaymentOfServicesTransfers(string entity, string reference, double amount, HistoryFilter filter)
        {
            return GetConfirmationFiles(amount, filter, CompareOperationFunctionPaymentOfServicesTransfers(entity, reference));
        }

        //TODO title to filter
        public (bool toPerform, double newAmount) CheckTransferMade(string accountNumber, string title, double amount, HistoryFilter filter)
        {
            return CheckTransferMade(filter, amount, CompareOperationFunctionTransfers(accountNumber, title));
        }

        public (bool toPerform, double newAmount) CheckTaxTransferMade(string taxType, string accountNumber, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier, string creditorName, string obligationId, double amount, HistoryFilter filter)
        {
            return CheckTransferMade(filter, amount, CompareOperationFunctionTaxTransfers(taxType, accountNumber, period, creditorIdentifier, creditorName, obligationId));
        }

        public (bool toPerform, double newAmount) CheckPaymentOfServicesTransferMade(string entity, string reference, double amount, HistoryFilter filter)
        {
            return CheckTransferMade(filter, amount, CompareOperationFunctionPaymentOfServicesTransfers(entity, reference));
        }

        //TODO return class
        protected (T, bool) ProcessResponse<T>(HttpResponseMessage response, Func<string, bool> checkResponseAction, Func<string, T> responseStrAction)
        {
            using (HttpContent content = response.Content)
            {
                string responseStr = content.ReadAsStringAsync().Result;

                if (!checkResponseAction(responseStr))
                    return default;

                return (responseStrAction(responseStr), true);
            }
        }

        protected void ProcessFileStream(HttpRequestMessage request, Func<ContentDispositionHeaderValue, FileStream> fileStream)
        {
            HttpOperations.ProcessResponseContentStream(Client, request, (Stream contentStream, ContentDispositionHeaderValue contentDisposition) =>
            {
                using (FileStream file = fileStream(contentDisposition))
                    contentStream.CopyTo(file);
            });
        }

        protected bool ConfirmWithoutFactor(ConfirmTextBase confirmText)
        {
            return PromptOKCancel(confirmText.Text);
        }

        protected bool ConfirmMobile(ConfirmTextBase confirmText)
        {
            return PromptOKCancel($"{confirmText.Text}", "Potwierdź na urządzeniu mobilnym");
        }

        protected T MobileConfirm<T, Y>(Func<Y> mobileRequestAction, Func<Y, bool?> responseValidateAction, Func<Y, T> okResultAction, Func<T> replayAction, ConfirmTextBase confirmText)
        {
            if (!ConfirmMobile(confirmText))
                return default;

            Y mobileResponse = mobileRequestAction();

            bool? validation = responseValidateAction(mobileResponse);

            switch (validation)
            {
                //TODO what if confirmation terminated (already done in nest, in ing as error in response
                case false:
                    return default;
                case null:
                    return replayAction != null ? replayAction() : MobileConfirm(mobileRequestAction, responseValidateAction, okResultAction, replayAction, confirmText);
                case true:
                    return okResultAction(mobileResponse);
                default: throw new ArgumentException();
            }
        }

        protected T SMSConfirm<T, Y>(Func<string, Y> smsRequestAction, Func<Y, bool?> responseValidateAction, Func<Y, T> okResultAction, Func<T> replayAction, ConfirmTextBase confirmText, int? codeNumber = null)
        {
            //TODO connect to phone, fetch code, check if data matches (account number, amount, code number)
            string SMSCode = GetSMSCode(codeNumber, confirmText);
            if (SMSCode == null)
                return default;

            Y smsResponse = smsRequestAction(SMSCode);

            bool? validation = responseValidateAction(smsResponse);

            switch (validation)
            {
                case false:
                    return default;
                case null:
                    {
                        Message("Nieprawidłowy kod SMS");
                        return replayAction != null ? replayAction() : SMSConfirm(smsRequestAction, responseValidateAction, okResultAction, replayAction, confirmText, codeNumber);
                    }
                case true:
                    return okResultAction(smsResponse);
                default: throw new ArgumentException();
            }
        }

        //TODO use bank name in additionalText as in GetinBank + in mobile confirmations
        protected string GetSMSCode(int? codeNumber, ConfirmTextBase confirmText)
        {
            StringBuilder message = new StringBuilder();
            message.Append("Kod SMS");
            if (codeNumber != null)
                message.Append($" nr {codeNumber}");
            message.Append($". {confirmText.Text}");
            return PromptString(message.ToString(), SMSCodeValidator.GetPattern());
        }

        protected bool CheckFailed(string text)
        {
            Message(text);
            return false;
        }

        protected void AssertMediaType(MediaTypeHeaderValue responseTypeHeader, HttpContentMediaType mediaType)
        {
            if (!responseTypeHeader.MediaType.EqualsMediaType(mediaType))
                throw new NotImplementedException();
        }

        //TODO what if no internet (timer stop), then internet restore; try make operation after time elapse from heartbeat
        protected override void DisposeElements()
        {
            base.DisposeElements();
            heartbeatTimer?.Dispose();
        }
    }

    public abstract class BankBase<A, H, F, AccDetResp> : BankBaseBase where A : AccountData where H : HistoryItem where F : HistoryFilter where AccDetResp : class
    {
        private AccDetResp accountsDetails;
        private AccDetResp AccountsDetails
        {
            get => accountsDetails ?? (accountsDetails = GetAccountsDetails());
            set
            {
                accountsDetails = value;
                if (accountsDetails == null)
                    CallAvailableFundsClear();
            }
        }
        public override bool EmptyAccountsData => accountsDetails == null;

        protected abstract AccDetResp GetAccountsDetails();
        protected abstract List<A> GetAccountsDataMainMain(AccDetResp accountsDetails);
        protected abstract F CreateFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact);
        protected abstract List<H> GetHistoryItems(F filter = null);
        protected abstract bool GetDetailsFileMain(H item, Func<ContentDispositionHeaderValue, FileStream> file);

        public new A SelectedAccountData
        {
            get => (A)base.SelectedAccountData;
            set => base.SelectedAccountData = value;
        }

        protected override List<AccountData> GetAccountsDataMain(bool update)
        {
            return GetAccountsDataMainMain(update)?.Cast<AccountData>().ToList();
        }

        private List<A> GetAccountsDataMainMain(bool update)
        {
            if (update)
                AccountsDetails = GetAccountsDetails();
            return GetAccountsDataMainMain(AccountsDetails);
        }

        protected override AccountData SelectAccountData()
        {
            List<A> accounts = GetAccountsDataMainMain(false);
            if (accounts == null || accounts.Count == 0)
                return null;
            (string name, A data) item = PromptComboBox<A>("Konto", accounts.Select(o => new SelectComboBoxItem<A>(o.Description(), o)), true);
            return item.data;
        }

        protected override void ClearAccountsData(bool totalClean)
        {
            AccountsDetails = null;
            if (totalClean)
                CleanHttpClient();
        }

        public override HistoryFilter CreateHistoryFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact)
        {
            return CreateFilter(direction, title, dateFrom, dateTo, amountExact);
        }

        public override List<HistoryItem> GetHistory(HistoryFilter filter = null)
        {
            if (!CheckSelectedAccount())
                return new List<HistoryItem>();
            return GetHistoryItems((F)filter)?.Cast<HistoryItem>().ToList();
        }

        public override bool GetDetailsFile(HistoryItem item, Func<ContentDispositionHeaderValue, FileStream> file)
        {
            return GetDetailsFileMain(item as H, file);
        }
    }

}
