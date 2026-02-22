using System.Text;

namespace BankService.ConfirmText
{
    public class ConfirmTextTaxTransfer : ConfirmTextAmount
    {
        public string RecipientName { get; private set; }

        protected override string OperationName => "Przelew podatkowy";
        protected override string AdditionalText
        {
            get
            {
                StringBuilder message = new StringBuilder();
                message.Append(base.AdditionalText);
                if (RecipientName != null)
                    message.Append($" - Odbiorca {RecipientName}");
                return message.ToString();
            }
        }

        public ConfirmTextTaxTransfer(double amount, string currency, string recipientName) : base(amount, currency)
        {
            RecipientName = recipientName;
        }
    }
}
