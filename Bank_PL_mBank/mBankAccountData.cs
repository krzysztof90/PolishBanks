namespace BankService.Bank_PL_MBank
{
    public class MBankAccountData : AccountData
    {
        public MBankAccountData(string name, string accountNumber, string currency, double availableFunds) : base(name, accountNumber, currency, availableFunds)
        {
        }
    }
}
