namespace BankService.Bank_PL_VeloBank
{
    public class VeloBankAccountData : AccountData
    {
        public string Id { get; set; }

        public VeloBankAccountData(string name, string accountNumber, string currency, double availableFunds) : base(name, accountNumber, currency, availableFunds)
        {
        }
    }
}
