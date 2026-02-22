using Tools;

namespace BankService.Bank_PL_Nest
{
    public enum NestJsonLoginProcess
    {
        [JsonValue("PARTIAL_PASSWORD")]
        Masked,
        [JsonValue("FULL_PASSWORD")]
        Full,
        [JsonValue("RESET_PASSWORD")]
        ResetPassword,
    }
}
