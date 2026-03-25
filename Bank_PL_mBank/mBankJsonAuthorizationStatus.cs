using Tools;

namespace BankService.Bank_PL_MBank
{
    public enum MBankJsonAuthorizationStatus
    {
        [JsonValue("Authorized")]
        Authorized,
        [JsonValue("AuthCancel")]
        Cancel,
    }
}
