using System.ComponentModel;
using Tools;

namespace BankService.Bank_PL_mBank
{
    public enum mBankJsonOperationKind
    {
        [JsonValue(null)]
        [Description("Niepobrane szczegóły")]
        Empty,

        [JsonValue("PRZELEW ZEWNĘTRZNY PRZYCHODZĄCY")]
        [Description("Przelew zewnętrzny przychodzący")]
        TransferExternalIncoming,
        [JsonValue("PRZELEW ZEWNĘTRZNY WYCHODZĄCY")]
        [Description("Przelew zewnętrzny wychodzący")]
        TransferExternalOutgoing,
        [JsonValue("PRZELEW PODATKOWY")]
        [Description("Przelew podatkowy")]
        TransferTax,
        [JsonValue("PRZELEW MTRANSFER WYCHODZACY")]
        [Description("mTransfer wychodzący")]
        MTransferOutgoing,
        [JsonValue("BLIK P2P-WYCHODZĄCY")]
        [Description("Przelew na telefon wychodzący")]
        BlikPhoneOutgoing,
    }
}
