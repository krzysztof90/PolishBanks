using Tools;

namespace BankService.Bank_PL_VeloBank
{
    public enum VeloBankJsonTransferType
    {
        [JsonValue("TRANSFER")]
        Transfer,
        [JsonValue("TAX_TRANSFER")]
        Tax,
        [JsonValue("PREPAID_TRANSFER")]
        Prepaid,
    }
}
