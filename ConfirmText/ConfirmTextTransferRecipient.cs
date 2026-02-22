using System.Text;

namespace BankService.ConfirmText
{
    public class ConfirmTextTransferRecipient: ConfirmTextTransfer
    {
        public string RecipientName { get; private set; }

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

        public ConfirmTextTransferRecipient(double amount, string currency, string recipientBankName, string recipientAccountNumber, string recipientName) : base(amount, currency, recipientBankName, recipientAccountNumber)
        {
            RecipientName = recipientName;
        }
    }
}
