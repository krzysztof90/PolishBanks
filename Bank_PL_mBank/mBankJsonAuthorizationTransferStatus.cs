using Tools;

namespace BankService.Bank_PL_MBank
{
    public enum MBankJsonAuthorizationTransferStatus
    {
        [JsonValue("Authorized")]
        Accepted,
        [JsonValue("Canceled")]
        Canceled
    }
}
