using Tools;

namespace BankService.Bank_PL_VeloBank
{
    public enum VeloBankJsonLoginQRStatus
    {
        [JsonValue("PENDING")]
        Pending,
        [JsonValue("PROCESSING")]
        Processing,
        [JsonValue("ERROR_TIMED_OUT")]
        TimedOut,
        [JsonValue("REJECTED")]
        Rejected,
        [JsonValue("ACCEPTED")]
        Accepted,
    }
}
