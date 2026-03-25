using Tools;

namespace BankService.Bank_PL_MBank
{
    public enum MBankJsonAuthorizationMode
    {
        [JsonValue("SMS")]
        SMS,
        [JsonValue("NAM")]
        Mobile,
    }
}
