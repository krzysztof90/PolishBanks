using Tools;

namespace BankService.Bank_PL_mBank
{
    public enum mBankJsonAuthorizationStatus
    {
        [JsonValue("Authorized")]
        Authorized,
        [JsonValue("AuthCancel")]
        Cancel,
    }
}
