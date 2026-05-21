namespace BankService.Bank_PL_Pocztowy
{
    public class PocztowyAccountData : AccountData
    {
        public string Id { get; set; }

        public PocztowyAccountData(string name, string accountNumber, string currency, double availableFunds) : base(name, accountNumber, currency, availableFunds)
        {
        }
    }
}
