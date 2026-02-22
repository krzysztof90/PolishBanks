namespace BankService.SMSCodes
{
    public abstract class SMSCodeValidator
    {
        public abstract string GetPattern();

        public SMSCodeValidator()
        {
        }
    }
}
