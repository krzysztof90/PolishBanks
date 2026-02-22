using System;

namespace BankService.Tax.TaxPeriods
{
    public abstract class TaxPeriod
    {
        //public TaxPeriodType PeriodType { get; set; }

        public TaxPeriod()
        {
        }

        public virtual bool Validate(Action<string> message)
        {
            return true;
        }

        protected bool ValidateInt(Action<string> message, int value, string name, int min, int max)
        {
            if (!(value >= min && value <= max))
                return CheckFailed(message, $"Pole okresu '{name}' ma niepoprawną wartość. Prawidłowa wartość: {min} - {max}.");
            return true;
        }

        protected bool ValidateDate(Action<string> message, DateTime value, string name)
        {
            if (value == null || value == DateTime.MinValue)
                return CheckFailed(message, $"Pole okresu '{name}' ma niepoprawną wartość");
            return true;
        }

        protected bool CheckFailed(Action<string> message, string text)
        {
            message(text);
            return false;
        }
    }
}
