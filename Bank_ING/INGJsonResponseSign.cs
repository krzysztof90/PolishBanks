using Tools;

namespace BankService.Bank_ING
{
    public enum INGJsonResponseSign
    {
        [JsonValue("+")]
        Credit,
        [JsonValue("-")]
        Debit
    }
}
