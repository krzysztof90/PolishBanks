using Tools;

namespace BankService.Bank_ING
{
    public enum INGJsonResponseOrderMode
    {
        [JsonValue("OK")]
        Web,
        [JsonValue("CODE")]
        Code,
        [JsonValue("TOKEN")]
        Mobile
    }
}
