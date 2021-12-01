using Tools;

namespace BankService.Bank_ING
{
    public enum INGJsonResponseCreditDebit
    {
        [JsonValue("-")]
        Debit,
        [JsonValue("+")]
        Credit
    }
}
