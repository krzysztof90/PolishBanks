using System;

namespace BankService.Tax.TaxPeriods
{
    public class TaxPeriodMonth : TaxPeriod
    {
        public int Year { get; set; }
        public int Month { get; set; }

        public TaxPeriodMonth(int year, int month) : base()
        {
            Year = year;
            Month = month;
        }

        public override bool Validate(Action<string> message)
        {
            bool result = base.Validate(message);
            result = ValidateInt(message, Year, "Rok", 1, Int32.MaxValue) && result;
            result = ValidateInt(message, Month, "Miesiąc", 1, 12) && result;
            return result;
        }
    }
}
