using Tools;

namespace BankService.Bank_PL_PKO
{
    public enum PKOMobileStatusValue
    {
        [JsonValue("PENDING")]
        Pending,
        [JsonValue("READY")]
        Ready,
        [JsonValue("OBJECT_NOT_FOUND")]
        Error,
    }
}
