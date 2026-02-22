using System.Text;

namespace BankService.ConfirmText
{
    public class ConfirmTextPrepaidTransfer : ConfirmTextAmount
    {
        public string OperatorName { get; private set; }
        public string PhoneNumber { get; private set; }

        protected override string OperationName => "Doładowanie telefonu";
        protected override string AdditionalText
        {
            get
            {
                StringBuilder message = new StringBuilder();
                message.Append(base.AdditionalText);
                if (OperatorName != null)
                    message.Append($" - Operator {OperatorName}");
                if (PhoneNumber != null)
                    message.Append($" - Numer telefonu {PhoneNumber}");
                return message.ToString();
            }
        }

        public ConfirmTextPrepaidTransfer(double amount, string currency, string operatorName, string phoneNumber) : base(amount, currency)
        {
            OperatorName = operatorName;
            PhoneNumber = phoneNumber;
        }
    }
}
