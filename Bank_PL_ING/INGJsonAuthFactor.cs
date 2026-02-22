using Tools;

namespace BankService.Bank_PL_ING
{
    public enum INGJsonAuthFactor
    {
        [JsonValue("AUTOCONFIRM")]
        AutoConfirm,
        [JsonValue("NONE")]
        None,
        [JsonValue("SMS")]
        SMS,
        [JsonValue("REDSMS")]
        RedSMS,
        [JsonValue("MOBILE")]
        Mobile,
        [JsonValue("U2F")]
        U2F,

        [JsonValue("ADDBROWSER")]
        AddBrowser,

        [JsonValue("LOGIN")]
        Login,
        [JsonValue("PASSWORD")]
        Password
    }
}
