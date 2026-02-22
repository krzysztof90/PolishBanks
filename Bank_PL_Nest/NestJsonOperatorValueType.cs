using Tools;

namespace BankService.Bank_PL_Nest
{
    public enum NestJsonOperatorValueType
    {
        [JsonValue("RANGE")]
        Range,
        [JsonValue("CONSTANT")]
        Constant,
    }
}
