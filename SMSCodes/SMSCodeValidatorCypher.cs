namespace BankService.SMSCodes
{
    public class SMSCodeValidatorCypher : SMSCodeValidator
    {
        public int Length { get; private set; }

        public SMSCodeValidatorCypher(int length) : base()
        {
            Length = length;
        }

        public override string GetPattern()
        {
            return @"^\d{" + Length.ToString() + "}$";
        }
    }
}
