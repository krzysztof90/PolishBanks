using Tools;

namespace BankService.Bank_PL_mBank
{
    public enum mBankJsonAuthorizationMode
    {
        [JsonValue("SMS")]
        SMS,
        [JsonValue("NAM")]
        Mobile,
    }
}
