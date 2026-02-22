using System;

namespace BankService.Tax.TaxPeriods
{
    public class TaxPeriodYear : TaxPeriod
    {
        public int Year { get; set; }

        public TaxPeriodYear(int year) : base()
        {
            Year = year;
        }

        public override bool Validate(Action<string> message)
        {
            bool result = base.Validate(message);
            result = ValidateInt(message, Year, "Rok", 1, Int32.MaxValue) && result;
            return result;
        }
    }
}
