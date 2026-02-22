using System;

namespace BankService.Tax.TaxPeriods
{
    public class TaxPeriodMonthDecade : TaxPeriod
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Decade { get; set; }

        public TaxPeriodMonthDecade(int year, int month, int decade) : base()
        {
            Year = year;
            Month = month;
            Decade = decade;
        }

        public override bool Validate(Action<string> message)
        {
            bool result = base.Validate(message);
            result = ValidateInt(message, Year, "Rok", 1, Int32.MaxValue) && result;
            result = ValidateInt(message, Month, "Miesiąc", 1, 12) && result;
            result = ValidateInt(message, Decade, "Dekada", 1, 3) && result;
            return result;
        }
    }
}
