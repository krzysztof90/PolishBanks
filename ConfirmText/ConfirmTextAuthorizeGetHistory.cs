namespace BankService.ConfirmText
{
    public class ConfirmTextAuthorizeGetHistory : ConfirmTextBase
    {
        protected override string OperationName => "Podgląd historii transakcji";
        protected override string AdditionalText => null;

        public ConfirmTextAuthorizeGetHistory()
        {
        }
    }
}
