using Tools;

namespace BankService.Bank_VeloBank
{
    public enum VeloBankJsonTransferType
    {
        [JsonValue("TRANSFER")]
        Transfer,
        [JsonValue("PREPAID_TRANSFER")]
        Prepaid,
    }
}
