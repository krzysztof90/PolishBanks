using Tools;

namespace BankService.Bank_VeloBank
{
    public enum VeloBankJsonOperationType
    {
        [JsonValue("TRANSFER_OUT")]
        TransferOut,
        [JsonValue("TRANSFER_IN")]
        TransferIn,
        [JsonValue("CARD_OPERATION")]
        Card,
        [JsonValue("WEB_PURCHASE")]
        WebPurchase,
    }
}
