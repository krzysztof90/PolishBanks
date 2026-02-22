using Tools;

namespace BankService.Bank_PL_ING
{
    public enum INGJsonCreditDebit
    {
        [JsonValue("-")]
        Debit,
        [JsonValue("+")]
        Credit
    }
}
