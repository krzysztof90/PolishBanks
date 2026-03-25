using Tools;

namespace BankService.Bank_PL_MBank
{
    public enum MBankJsonAuthorizationTransferStatus
    {
        [JsonValue("Authorized")]
        Authorized,
        [JsonValue("Canceled")]
        Cancel,
    }
}
