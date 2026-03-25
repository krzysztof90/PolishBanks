using Tools;

namespace BankService.Bank_PL_PKO
{
    public enum PKOMobileStatusStatus
    {
        [JsonValue("LP_NOT_CHANGED")]
        NotChanged,
        [JsonValue("LP_CHANGED")]
        Changed,
        [JsonValue("LP_ERROR")]
        Error,
    }
}
