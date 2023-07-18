using Tools;

namespace BankService.Bank_VeloBank
{
    public enum VeloBankJsonSideType
    {
        [JsonValue("DEBIT")]
        Debit,
        [JsonValue("CREDIT")]
        Credit,
    }
}
