using Tools;

namespace BankService.Bank_PL_mBank
{
    public enum mBankJsonAuthorizationType
    {
        [JsonValue("SMS")]
        SMS,
        [JsonValue("MA")]
        Mobile,
    }
}
