using Tools;

namespace BankService.Bank_PL_VeloBank
{
    public enum VeloBankJsonConfirmationStatusType
    {
        [JsonValue("WAITING")]
        Waiting,
        [JsonValue("ACCEPTED")]
        Accepted,
        [JsonValue("REJECTED")]
        Rejected,
        [JsonValue("OUTDATED")]
        Outdated,
    }
}
