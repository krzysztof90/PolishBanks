using Tools;

namespace BankService.Bank_VeloBank
{
    public enum VeloBankJsonModuleType
    {
        [JsonValue("BANKING")]
        Banking,
        [JsonValue("PA")]
        FastTransferPA,
        [JsonValue("PBL")]
        FastTransferPBL,
    }
}
