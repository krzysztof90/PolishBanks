namespace BankService.Bank_PL_Nest
{
    public class NestAccountData : AccountData
    {
        public long Id { get; set; }

        public NestAccountData(string name, string accountNumber, string currency, double availableFunds) : base(name, accountNumber, currency, availableFunds)
        {
        }
    }
}
