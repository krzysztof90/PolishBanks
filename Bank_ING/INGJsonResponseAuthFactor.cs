using Tools;

namespace BankService.Bank_ING
{
    public enum INGJsonResponseAuthFactor
    {
        [JsonValue("AUTOCONFIRM")]
        AutoConfirm,
        [JsonValue("NONE")]
        None,
        [JsonValue("SMS")]
        SMS,
        [JsonValue("MOBILE")]
        Mobile,

        [JsonValue("ADDBROWSER")]
        AddBrowser
    }
}
