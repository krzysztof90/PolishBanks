using Tools;

namespace BankService.Bank_PL_Nest
{
    public enum NestJsonCreditDebit
    {
        [JsonValue("DEBIT")]
        Debit,
        [JsonValue("CREDIT")]
        Credit
    }
}
