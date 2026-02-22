using Tools;

namespace BankService.Bank_PL_Nest
{
    public enum NestJsonAuthorizationStatus
    {
        [JsonValue("NEW")]
        New,
        [JsonValue("SUCCESS")]
        Success,
        [JsonValue("VERIFIED")]
        Verified,
        [JsonValue("CANCEL")]
        Cancel,
        [JsonValue("EXPIRED")]
        Expired,
    }
}
