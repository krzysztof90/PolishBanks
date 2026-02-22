using Tools;

namespace BankService.Bank_PL_VeloBank
{
    public enum VeloBankJsonConfirmType
    {
        [JsonValue("SMS")]
        SMS,
        [JsonValue("MOBILE")]
        Mobile
    }
}
