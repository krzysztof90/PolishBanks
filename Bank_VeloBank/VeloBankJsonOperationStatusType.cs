using Tools;

namespace BankService.Bank_VeloBank
{
    public enum VeloBankJsonOperationStatusType
    {
        [JsonValue("DONE")]
        Done,
        [JsonValue("PENDING")]
        Pending,
    }
}
