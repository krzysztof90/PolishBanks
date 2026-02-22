using System;

namespace BankService.Tax.TaxPeriods
{
    public class TaxPeriodDay : TaxPeriod
    {
        //TODO date only
        public DateTime Day { get; set; }

        public TaxPeriodDay(DateTime day) : base()
        {
            Day = day;
        }

        public override bool Validate(Action<string> message)
        {
            bool result = base.Validate(message);
            result = ValidateDate(message, Day, "Dzień") && result;
            return result;
        }
    }
}
