using Tools;

namespace BankService.Bank_PL_Nest
{
    public enum NestJsonAuthorizationMethod
    {
        [JsonValue("SMS")]
        SMS,
        [JsonValue("FINANTEQ_TOKEN")]
        Mobile,
    }
}
