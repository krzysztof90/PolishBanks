namespace BankService.MandatoryTransferDatas
{
    public class MandatoryTransferDataAmount : MandatoryTransferData
    {
        public double Amount { get; set; }

        public MandatoryTransferDataAmount(double amount, bool mandatoryCondition = true) : base(mandatoryCondition)
        {
            Amount = amount;
        }

        public override bool ValidateData()
        {
            return Amount > 0;
        }
    }
}
