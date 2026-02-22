using System.Text;

namespace BankService.ConfirmText
{
    public class ConfirmTextTransfer : ConfirmTextAmount
    {
        public string RecipientBankName { get; private set; }
        public string RecipientAccountNumber { get; private set; }

        protected override string OperationName => "Przelew";
        protected override string AdditionalText
        {
            get
            {
                StringBuilder message = new StringBuilder();
                message.Append(base.AdditionalText);
                message.Append($" - Konto {RecipientAccountNumber}");
                if (RecipientBankName != null)
                    message.Append($" - Bank {RecipientBankName}");
                return message.ToString();
            }
        }

        public ConfirmTextTransfer(double amount, string currency, string recipientBankName, string recipientAccountNumber) : base(amount, currency)
        {
            RecipientBankName = recipientBankName;
            RecipientAccountNumber= recipientAccountNumber;
        }
    }
}
