using Tools;

namespace BankService.Bank_PL_VeloBank
{
    public enum VeloBankJsonLoginQRStatus
    {
        [JsonValue("PENDING")]
        Pending,
        [JsonValue("PROCESSING")]
        Processing,
        [JsonValue("ACCEPTED")]
        Accepted,
        [JsonValue("REJECTED")]
        Rejected,
        [JsonValue("ERROR_TIMED_OUT")]
        Expired
    }
}
