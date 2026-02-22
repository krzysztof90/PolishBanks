namespace BankService.Bank_PL_ING
{
    public class INGAccountData : AccountData
    {
        public INGAccountData(string name, string accountNumber, string currency, double availableFunds) : base(name, accountNumber, currency, availableFunds)
        {
        }
    }
}
