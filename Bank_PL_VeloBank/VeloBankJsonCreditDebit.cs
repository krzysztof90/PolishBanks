using Tools;

namespace BankService.Bank_PL_VeloBank
{
    public enum VeloBankJsonCreditDebit
    {
        [JsonValue("DEBIT")]
        Debit,
        [JsonValue("CREDIT")]
        Credit
    }
}
