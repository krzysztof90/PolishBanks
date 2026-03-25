namespace BankService.Bank_PL_PKO
{
    public class PKOAccountData : AccountData
    {
        public string AccountId { get; set; }

        public PKOAccountData(string name, string accountNumber, string currency, double availableFunds, string accountId) : base(name, accountNumber, currency, availableFunds)
        {
            AccountId = accountId;
        }
    }
}
