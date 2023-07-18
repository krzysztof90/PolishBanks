using BankService.LocalTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.UI.WebControls;

namespace BankService
{
    public abstract class BankBaseBase : IDisposable
    {
        //TODO bez PLN na sztywno, pokazywanie waluty

        private bool logged;
        public bool Logged
        {
            get { return logged; }
            set
            {
                logged = value;
                if (!logged)
                    DisposeElements();
                OnLogged?.Invoke();
            }
        }

        public event Func<List<(string name, string value, string path, string domain)>> OnGetCookies;
        public event Action<(string name, string value, string path, string domain)> OnSetCookie;
        public event Action<(string name, string path, string domain)> OnRemoveCookie;
        public event Action<string> OnMessage;
        public event Func<string, string, bool> OnPromptYesNo;
        public event Func<string, string, bool> OnPromptOKCancel;
        public event Func<string, string, string> OnPromptString;
        public event Func<string, IEnumerable<PrepaidOperatorComboBoxItemBase>, PrepaidOperatorComboBoxItemBase> OnPromptComboBox;

        public event Action OnLogged;
        public event Action OnLogging;

        public event Action OnAvailableFundsClear;

        protected HttpClient httpClient;
        protected CookieContainer Cookies;

        private System.Threading.Timer heartbeatTimer;

        public abstract bool FastTransferMandatoryTransferId { get; }
        public abstract bool FastTransferMandatoryBrowserCookies { get; }
        public abstract bool FastTransferMandatoryCookie { get; }
        public abstract bool TransferMandatoryTitle { get; }
        public abstract bool PrepaidTransferMandatoryRecipient { get; }
        protected abstract string BaseAddress { get; }
        protected abstract bool LoginRequest(string login, string password);
        protected abstract bool LoginRequestForFastTransfer(string login, string password, string transferId, string cookie);
        protected abstract bool LogoutRequest();
        protected abstract int HeartbeatInterval { get; }
        protected abstract bool TryExtendSession();
        public abstract (string accountNumber, double availableFunds) GetAccountData();
        protected abstract string CleanFastTransferUrl(string transferId);
        public abstract bool MakeTransfer(string recipient, string address, string accountNumber, string title, double amount);
        protected abstract string MakeFastTransfer(string transferId, string cookie);
        protected abstract bool MakePrepaidTransfer(string recipient, string phoneNumber, double amount);
        public abstract List<HistoryItem> GetHistory(HistoryFilter filter = null);
        public abstract HistoryFilter CreateHistoryFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact);
        public abstract void GetDetailsFile(HistoryItem item, FileStream file);

        public BankBaseBase()
        {
            logged = false;
        }

        protected void SaveCookie((string name, string value, string path, string domain) cookie)
        {
            OnSetCookie?.Invoke(cookie);
        }
        protected void RemoveSavedCookie((string name, string path, string domain) cookie)
        {
            OnRemoveCookie?.Invoke(cookie);
        }
        protected void Message(string text)
        {
            OnMessage?.Invoke(text);
        }
        protected bool PromptYesNo(string text, string caption = null)
        {
            return OnPromptYesNo?.Invoke(text, caption ?? String.Empty) ?? false;
        }
        protected bool PromptOKCancel(string text, string caption = null)
        {
            return OnPromptOKCancel?.Invoke(text, caption ?? String.Empty) ?? false;
        }
        protected string PromptString(string text, string caption = null)
        {
            return OnPromptString?.Invoke(text, caption ?? String.Empty) ?? null;
        }
        protected (string name, T data) PromptComboBox<T>(string text, IEnumerable<PrepaidOperatorComboBoxItem<T>> dataSource)
        {
            PrepaidOperatorComboBoxItemBase item = OnPromptComboBox?.Invoke(text, dataSource);
            if (item == null)
                return (null, default(T));
            return (item.Name, ((PrepaidOperatorComboBoxItem<T>)item).Data);
        }

        protected void CallAvailableFundsClear()
        {
            OnAvailableFundsClear?.Invoke();
        }

        private void InitClient(bool includeCookies)
        {
            Cookies = new CookieContainer();
            httpClient = new HttpClient(new WebRequestHandler
            {
                AllowAutoRedirect = false,
                CookieContainer = Cookies
            });
            httpClient.BaseAddress = new Uri(BaseAddress);

            if (includeCookies)
            {
                List<(string name, string value, string path, string domain)> cookies = OnGetCookies?.Invoke() ?? new List<(string name, string value, string path, string domain)>();
                foreach ((string name, string value, string path, string domain) cookie in cookies)
                {
                    Cookies.Add(new Cookie(cookie.name, cookie.value, cookie.path, cookie.domain));
                }
            }
        }

        public bool Login(string login, string password)
        {
            if (String.IsNullOrEmpty(login) || String.IsNullOrEmpty(password))
            {
                return CheckFailed("Niepoprawny login i hasło");
            }

            InitClient(true);

            if (!PerformLogin(login, password))
                return false;

            heartbeatTimer = new System.Threading.Timer((state) =>
            {
                if (!Logged)
                    heartbeatTimer.Dispose();
                else
                {
                    if (!TryExtendSession())
                        //TODO jeżeli brak internetu to bez komunikatu i nie zatrzymywanie timera. Co jeżeli po upływie czasu z heartbeat
                        heartbeatTimer.Dispose();
                }
            }, null, 0, HeartbeatInterval * 1000);

            Logged = true;
            return true;
        }

        protected bool PerformLogin(string login, string password)
        {
            if (!LoginRequest(login, password))
                return false;

            if (!PostLoginRequest())
                return false;

            return true;
        }

        protected virtual bool PostLoginRequest()
        {
            return true;
        }

        public bool Logout()
        {
            bool logout = LogoutRequest();

            Logged = !logout;
            PostLogoutRequest();
            return logout;
        }

        protected virtual void PostLogoutRequest()
        {
        }

        protected void NoteExpiredSession()
        {
            Logged = false;
        }

        public bool PerformTransfer(string recipient, string address, string accountNumber, string title, double amount)
        {
            if (String.IsNullOrEmpty(recipient) || String.IsNullOrEmpty(accountNumber) || (TransferMandatoryTitle && String.IsNullOrEmpty(title)) || amount <= 0)
            {
                return CheckFailed("Dane nie mogą być puste");
            }
            if (amount > GetAccountData().availableFunds)
            {
                return CheckFailed("Niewystarczające środki na koncie");
            }

            bool performed = MakeTransfer(recipient, address, accountNumber, title, amount);

            if (performed)
            {
                Message("Wykonano");

                PostTransfer();
            }
            return performed;
        }

        protected virtual void PostTransfer()
        {
        }

        public bool PerformFastTransfer(string login, string password, string transferId, string cookie)
        {
            if (FastTransferMandatoryTransferId)
            {
                if (transferId == null)
                    return CheckFailed("Wymagany numer przelewu");
                transferId = CleanFastTransferUrl(transferId);
                if (transferId == null)
                {
                    return CheckFailed("Niepoprawny numer");
                }
            }
            if (FastTransferMandatoryCookie && cookie == null)
                return CheckFailed("Wymagana wartość ciasteczka");

            if (String.IsNullOrEmpty(login) || String.IsNullOrEmpty(password))
            {
                return CheckFailed("Niepoprawny login i hasło");
            }

            InitClient(true);

            OnLogging?.Invoke();

            if (!LoginRequestForFastTransfer(login, password, transferId, cookie))
                return false;

            string redirectAddress = MakeFastTransfer(transferId, cookie);
            if (redirectAddress == null)
                return false;

            Message("Wykonano");

            Process.Start(redirectAddress);

            PostFastTransfer();

            return true;
        }

        protected virtual void PostFastTransfer()
        {
        }

        public bool PerformPrepaidTransfer(string recipient, string phoneNumber, double amount)
        {
            //TODO numer telefonu z +48

            if ((PrepaidTransferMandatoryRecipient && recipient == null) || phoneNumber == null)
            {
                return CheckFailed("Dane nie mogą być puste");
            }

            bool performed = MakePrepaidTransfer(recipient, phoneNumber, amount);

            if (performed)
            {
                Message("Wykonano");

                PostTransfer();
            }

            return true;
        }

        public void OpenDetailsFile(HistoryItem item)
        {
            string fileName = Path.GetTempFileName().Replace(".tmp", ".pdf");
            using (FileStream file = File.Create(fileName))
            {
                GetDetailsFile(item, file);
            }

            Process.Start(fileName);
        }

        //TODO title do filter
        public bool CheckTransferMade(string accountNumber, HistoryFilter filter = null, string title = null)
        {
            List<HistoryItem> items = GetHistory(filter);

            if (items == null)
                return false;

            HistoryItem operation = items.Where(o => o.IsTransfer && AccountNumberTools.CompareAccountNumbers(o.ToAccountNumber, accountNumber) && (title == null || o.CompareTitle(title))).FirstOrDefault();
            if (operation != null)
            {
                return CheckFailed($"Wykonano wcześniej ({operation.OrderDate.ToString("dd.MM.yyyy", CultureInfo.CreateSpecificCulture("es-ES"))})");
            }

            return true;
        }

        protected bool CheckFailed(string text)
        {
            Message(text);
            return false;
        }

        //TODO co jeżeli brak internetu (zatrzymanie timera), potem przywrócenie internetu, próba wykonania operacji po upływie czasu z heartbeat
        protected void DisposeElements()
        {
            httpClient?.Dispose();
            heartbeatTimer?.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            DisposeElements();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public abstract class BankBase<H, F> : BankBaseBase where H : HistoryItem where F : HistoryFilter
    {
        protected abstract List<H> GetHistoryItems(F filter = null);
        protected abstract F CreateFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact);

        public override List<HistoryItem> GetHistory(HistoryFilter filter = null)
        {
            return GetHistoryItems((F)filter).Cast<HistoryItem>().ToList();
        }

        public override HistoryFilter CreateHistoryFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact)
        {
            return CreateFilter(direction, title, dateFrom, dateTo, amountExact);
        }
    }

}
