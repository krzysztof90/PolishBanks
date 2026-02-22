namespace BankService.ConfirmText
{
    public abstract class ConfirmTextAmount : ConfirmTextBase
    {
        public double Amount { get; private set; }
        public string Currency { get; private set; }

        protected override string AdditionalText => $" na kwotę {DisplayAmount()}";

        public ConfirmTextAmount(double amount, string currency) : base()
        {
            Amount = amount;
            Currency = currency;
        }

        protected string DisplayAmount()
        {
            return $"{Amount} {Currency}";
        }

    }
}
