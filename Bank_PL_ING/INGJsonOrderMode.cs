using Tools;

namespace BankService.Bank_PL_ING
{
    public enum INGJsonOrderMode
    {
        [JsonValue("OK")]
        Web,
        [JsonValue("CODE")]
        SMS,
        [JsonValue("TOKEN")]
        Mobile
    }
}
