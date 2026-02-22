namespace BankService.Bank_PT_Santander
{
    public class SantanderAccountData : AccountData
    {
        public string ShortAccountNumber { get; set; }

        public SantanderAccountData(string name, string accountNumber, string currency, double availableFunds) : base(name, accountNumber, currency, availableFunds)
        {
        }
    }
}
