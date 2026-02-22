using System;

namespace BankService.Tax.TaxPeriods
{
    public class TaxPeriodHalfYear : TaxPeriod
    {
        public int Year { get; set; }
        public int Half { get; set; }

        public TaxPeriodHalfYear(int year, int half) : base()
        {
            Year = year;
            Half = half;
        }

        public override bool Validate(Action<string> message)
        {
            bool result = base.Validate(message);
            result = ValidateInt(message, Year, "Rok", 1, Int32.MaxValue) && result;
            result = ValidateInt(message, Half, "Półrocze", 1, 2) && result;
            return result;
        }
    }
}
