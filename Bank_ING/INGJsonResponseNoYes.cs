using Tools;

namespace BankService.Bank_ING
{
    public enum INGJsonResponseNoYes
    {
        [JsonValue("T")]
        Yes,
        [JsonValue("N")]
        No
    }
}
