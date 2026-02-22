using BankService.ConfirmText;

namespace BankService.Bank_PT_Santander
{
    public class SantanderConfirmTextAuthorizeGetHistory : ConfirmTextBase
    {
        protected override string OperationName => "Podgląd historii transakcji";
        protected override string AdditionalText => null;

        public SantanderConfirmTextAuthorizeGetHistory()
        {
        }
    }
}
