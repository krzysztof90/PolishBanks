using System;

namespace BankService
{
    public abstract class AccountData
    {
        public string Name { get; set; }
        public string AccountNumber { get; set; }
        public string Currency { get; set; }
        public double AvailableFunds { get; set; }

        public AccountData(string name, string accountNumber, string currency, double availableFunds)
        {
            Name = name;
            AccountNumber = accountNumber;
            Currency = currency;
            AvailableFunds = availableFunds;
        }

        public string Description()
        {
            return $"{Name}, {Currency}";
        }

        public override bool Equals(object obj) => this.Equals(obj as AccountData);

        public bool Equals(AccountData a)
        {
            if (a is null)
                return false;

            if (Object.ReferenceEquals(this, a))
                return true;

            if (this.GetType() != a.GetType())
                return false;

            return (AccountNumber == a.AccountNumber);
        }
    }
}
