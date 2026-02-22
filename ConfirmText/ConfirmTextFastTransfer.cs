using System.Text;

namespace BankService.ConfirmText
{
    public class ConfirmTextFastTransfer : ConfirmTextAmount
    {
        public string RecipientName { get; private set; }

        protected override string OperationName => "Szybki przelew";
        protected override string AdditionalText
        {
            get
            {
                StringBuilder message = new StringBuilder();
                message.Append(base.AdditionalText);
                if (RecipientText != null)
                    message.Append(RecipientText);
                return message.ToString();
            }
        }

        public ConfirmTextFastTransfer(double amount, string currency, string recipientName) : base(amount, currency)
        {
            RecipientName = recipientName;
        }

        protected virtual string RecipientText => RecipientName != null ? $" - Odbiorca {RecipientName}" : null;
    }
}
