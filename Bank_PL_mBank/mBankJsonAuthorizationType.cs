using Tools;

namespace BankService.Bank_PL_MBank
{
    public enum MBankJsonAuthorizationType
    {
        [JsonValue("SMS")]
        SMS,
        [JsonValue("MA")]
        Mobile,
    }
}
