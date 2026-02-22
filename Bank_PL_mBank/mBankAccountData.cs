namespace BankService.Bank_PL_mBank
{
    public class mBankAccountData : AccountData
    {
        public mBankAccountData(string name, string accountNumber, string currency, double availableFunds) : base(name, accountNumber, currency, availableFunds)
        {
        }
    }
}
