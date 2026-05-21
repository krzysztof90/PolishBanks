using Tools;

namespace BankService.Bank_PL_Pocztowy
{
    public enum PocztowyJsonOperatorAmountType
    {
        [JsonValue("RANGE")]
        Range,
        [JsonValue("CONSTANT")]
        Constant
    }
}
