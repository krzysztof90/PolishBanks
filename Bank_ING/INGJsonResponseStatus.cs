using Tools;

namespace BankService.Bank_ING
{
    public enum INGJsonResponseStatus
    {
        [JsonValue("OK")]
        OK,
        [JsonValue("ERROR")]
        Error
    }
}
