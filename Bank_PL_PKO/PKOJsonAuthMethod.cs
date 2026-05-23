using Tools;

namespace BankService.Bank_PL_PKO
{
    public enum PKOJsonAuthMethod
    {
        [JsonValue("sms")]
        SMS,
        [JsonValue("mobile_application")]
        Mobile
    }
}