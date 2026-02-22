using Tools;

namespace BankService.Bank_PL_mBank
{
    public enum mBankJsonAuthorizationTransferStatus
    {
        [JsonValue("Authorized")]
        Authorized,
        [JsonValue("Canceled")]
        Cancel,
    }
}
