using Tools;

namespace BankService.Bank_PL_PKO
{
    public enum PKOAuthMethod
    {
        [JsonValue("sms")]
        SMS,
        [JsonValue("mobile_application")]
        Mobile,
    }
}