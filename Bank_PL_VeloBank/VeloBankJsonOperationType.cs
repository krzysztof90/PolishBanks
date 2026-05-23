using Tools;

namespace BankService.Bank_PL_VeloBank
{
    public enum VeloBankJsonOperationType
    {
        [JsonValue("TRANSFER_OUT")]
        TransferOutgoing,
        [JsonValue("TRANSFER_IN")]
        TransferIncoming,
        [JsonValue("CARD_OPERATION")]
        Card,
        [JsonValue("WEB_PURCHASE")]
        WebPurchase
    }
}
