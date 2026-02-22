using Tools;

namespace BankService.Bank_PL_VeloBank
{
    public enum VeloBankJsonCategoryType
    {
        [JsonValue("TRANSFER")]
        Transfer,
        [JsonValue("FOREIGN_TRANSFER")]
        TransferForeign,
        [JsonValue("BLIK_P2P")]
        Blik,
        [JsonValue("CARD_OPERATION")]
        Card,
        [JsonValue("TRANSFER_PREPAID")]
        Prepaid,
        [JsonValue("EXPRESS_ELIXIR")]
        Elixir,
        [JsonValue("WEB_PURCHASE")]
        WebPurchase,
        [JsonValue("TAX_TRANSFER")]
        Tax,
        [JsonValue("FEES_AND_CHARGES")]
        FeesCharges,
        [JsonValue("OWN_TRANSFER")]
        OwnTransfer,
    }
}
