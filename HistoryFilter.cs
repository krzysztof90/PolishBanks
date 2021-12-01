using BankService.LocalTools;
using System;

namespace BankService
{
    public abstract class HistoryFilter
    {
        protected event Action OnDateSet;

        private string accountNumber;
        public string AccountNumber
        {
            get { return accountNumber?.SimplifyAccountNumber(); }
            set { accountNumber = value; }
        }

        public int CounterLimit { get; set; }

        public OperationDirection? Direction { get; set; }

        private DateTime? dateFrom;
        public DateTime? DateFrom
        {
            get { return dateFrom; }
            set
            {
                dateFrom = value;
                if (value != null)
                {
                    OnDateSet?.Invoke();
                }
            }
        }
        private DateTime? dateTo;
        public DateTime? DateTo
        {
            get { return dateTo; }
            set
            {
                dateTo = value;
                if (value != null)
                {
                    OnDateSet?.Invoke();
                }
            }
        }
        public DateTime? DateExact
        {
            get { return dateFrom == dateTo ? dateFrom : null; }
            set
            {
                if (value != null)
                {
                    OnDateSet?.Invoke();
                    dateTo = value;
                    dateFrom = value;
                }
            }
        }

        public bool Amount { get; set; }
        private double? amountFrom;
        public double? AmountFrom
        {
            get { return amountFrom; }
            set
            {
                amountFrom = value;
                if (value != null)
                {
                    Amount = true;
                }
            }
        }
        private double? amountTo;
        public double? AmountTo
        {
            get { return amountTo; }
            set
            {
                amountTo = value;
                if (value != null)
                {
                    Amount = true;
                }
            }
        }
        public double? AmountExact
        {
            get { return amountFrom == amountTo ? amountFrom : null; }
            set
            {
                if (value != null)
                {
                    Amount = true;
                    amountFrom = value;
                    amountTo = value;
                }
            }
        }

        public string Title { get; set; }

        public HistoryFilter()
        {
            Init();
        }

        public HistoryFilter(string _accountNumber, string _title, DateTime? _dateFrom, DateTime? _dateTo, DateTime? _dateExact, double? _amountFrom, double? _amountTo, double? _amountExact) : this()
        {
            AccountNumber = _accountNumber;
            Title = _title;
            if (_dateFrom != null)
                DateFrom = _dateFrom;
            if (_dateTo != null)
                DateTo = _dateTo;
            DateExact = _dateExact;
            if (_amountFrom != null)
                AmountFrom = _amountFrom;
            if (_amountTo != null)
                AmountTo = _amountTo;
            AmountExact = _amountExact;
        }

        protected virtual void Init()
        {
        }
    }
}
