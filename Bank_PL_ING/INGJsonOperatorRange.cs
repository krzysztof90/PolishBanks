using Tools;

namespace BankService.Bank_PL_ING
{
    public enum INGJsonOperatorRange
    {
        [JsonValue("T")]
        Range,
        [JsonValue("N")]
        Constant
    }
}
