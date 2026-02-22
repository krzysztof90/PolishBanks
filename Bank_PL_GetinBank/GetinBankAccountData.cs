namespace BankService.Bank_PL_GetinBank
{
    public class GetinBankAccountData : AccountData
    {
        public string AccCode { get; set; }

        public GetinBankAccountData(string name, string accountNumber, string currency, double availableFunds) : base(name, accountNumber, currency, availableFunds)
        {
        }
    }
}
