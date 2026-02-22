using Tools;

namespace BankService.Bank_PL_ING
{
    public enum INGJsonTransferStatus
    {
        [JsonValue("OK")]
        OK,
        [JsonValue("ERROR")]
        Error
    }
}
