using System;

namespace BankService.Tax.TaxPeriods
{
    public class TaxPeriodQuarter : TaxPeriod
    {
        public int Year { get; set; }
        public int Quarter { get; set; }

        public TaxPeriodQuarter(int year, int quarter ) : base()
        {
            Year = year;
            Quarter = quarter;
        }

        public override bool Validate(Action<string> message)
        {
            bool result = base.Validate(message);
            result = ValidateInt(message, Year, "Rok", 1, Int32.MaxValue) && result;
            result = ValidateInt(message, Quarter, "Kwartał", 1, 4) && result;
            return result;
        }
    }
}
