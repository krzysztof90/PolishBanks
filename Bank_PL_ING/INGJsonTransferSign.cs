using Tools;

namespace BankService.Bank_PL_ING
{
    public enum INGJsonTransferSign
    {
        [JsonValue("+")]
        Credit,
        [JsonValue("-")]
        Debit
    }
}
