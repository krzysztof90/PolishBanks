using System.Text;

namespace BankService.ConfirmText
{
    public class ConfirmTextPaymentOfServices : ConfirmTextFastTransfer
    {
        public string EntityNumber { get; private set; }
        public string Reference { get; private set; }

        protected override string AdditionalText
        {
            get
            {
                StringBuilder message = new StringBuilder();
                message.Append(base.AdditionalText);
                if (Reference != null)
                    message.Append($" - Numer referencyjny {Reference}");
                return message.ToString();
            }
        }

        protected override string RecipientText => base.RecipientText != null ? $"{base.RecipientText} ({EntityNumber})" : null;

        public ConfirmTextPaymentOfServices(double amount, string currency, string recipientName, string entityNumber, string reference) : base(amount, currency, recipientName)
        {
            EntityNumber = entityNumber;
            Reference = reference;
        }
    }
}
