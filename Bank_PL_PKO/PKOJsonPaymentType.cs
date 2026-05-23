using Tools;

namespace BankService.Bank_PL_PKO
{
    public enum PKOJsonPaymentType
    {
        [JsonValue("ELIXIR")]
        Elixir,
        [JsonValue("SORBNET")]
        Sorbnet,
        [JsonValue("EXPRESS-ELIXIR")]
        ExpressElixir
    }
}
