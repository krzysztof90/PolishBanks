using Tools;

namespace BankService.Bank_VeloBank
{
    public enum VeloBankJsonConfirmType
    {
        [JsonValue("SMS")]
        SMS,
        [JsonValue("MOBILE")]
        Mobile
    }
}
