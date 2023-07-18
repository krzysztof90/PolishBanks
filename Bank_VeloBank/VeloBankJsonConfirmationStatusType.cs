using Tools;

namespace BankService.Bank_VeloBank
{
    public enum VeloBankJsonConfirmationStatusType
    {
        [JsonValue("WAITING")]
        Waiting,
        [JsonValue("ACCEPTED")]
        Accepted,
        [JsonValue("OUTDATED")]
        Outdated,
    }
}
