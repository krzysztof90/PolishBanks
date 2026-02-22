using System.Text;

namespace BankService.ConfirmText
{
    public abstract class ConfirmTextBase
    {
        protected abstract string OperationName { get; }
        protected abstract string AdditionalText { get; }

        public string Text
        {
            get
            {
                StringBuilder message = new StringBuilder();
                message.Append("Potwierdź wykonanie operacji");
                message.Append($" {OperationName}");
                if (AdditionalText != null)
                    message.Append(AdditionalText);
                return message.ToString();
            }
        }

        public ConfirmTextBase()
        {
        }
    }
}
