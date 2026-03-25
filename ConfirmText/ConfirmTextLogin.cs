using System.Text;

namespace BankService.ConfirmText
{
    public class ConfirmTextLogin : ConfirmTextBase
    {
        protected override string OperationName => "Autoryzacja logowania";

        protected override string AdditionalText => null;

        public ConfirmTextLogin() : base()
        {
        }
    }
}
