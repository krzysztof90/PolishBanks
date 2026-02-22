using Tools;

namespace BankService.Bank_PL_VeloBank
{
    public enum VeloBankJsonOperationStatusType
    {
        [JsonValue("DONE")]
        Done,
        [JsonValue("PENDING")]
        Pending,
        [JsonValue("SUBMITTED")]
        Submitted,
    }
}
