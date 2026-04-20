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
            get => accountNumber?.SimplifyAccountNumber();
            set => accountNumber = value;
        }

        public int CounterLimit { get; set; }

        public OperationDirection? Direction { get; set; }

        private DateTime? dateFrom;
        public DateTime? DateFrom
        {
            get => dateFrom;
            set
            {
                dateFrom = value;
                if (value != null)
                    OnDateSet?.Invoke();
            }
        }
        private DateTime? dateTo;
        public DateTime? DateTo
        {
            get => dateTo;
            set
            {
                dateTo = value;
                if (value != null)
                    OnDateSet?.Invoke();
            }
        }
        public DateTime? DateExact
        {
            get => dateFrom == dateTo ? dateFrom : null;
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
            get => amountFrom;
            set
            {
                amountFrom = value;
                if (value != null)
                    Amount = true;
            }
        }
        private double? amountTo;
        public double? AmountTo
        {
            get => amountTo;
            set
            {
                amountTo = value;
                if (value != null)
                    Amount = true;
            }
        }
        public double? AmountExact
        {
            get => amountFrom == amountTo ? amountFrom : null;
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

        public HistoryFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact) : this()
        {
            Direction = direction;
            Title = title;
            DateFrom = dateFrom;
            DateTo = dateTo;
            AmountExact = amountExact;
        }

        protected virtual void Init()
        {
        }
    }
}
