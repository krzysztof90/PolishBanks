using System.ComponentModel;
using Tools;

namespace BankService.Bank_PL_PKO
{
    public enum PKOJsonOperationKind
    {
        [JsonValue("TRANSFER")]
        [Description("Przelew wychodzący")]
        TransferOutgoing,
        [JsonValue("TRANSFER_IN")]
        [Description("Przelew przychodzący")]
        TransferIncoming,
        [JsonValue("MOBILE_PAYMENT_C2C_EXTERNAL")]
        [Description("Przelew na telefon")]
        PhoneTransfer,
        [JsonValue("US_TRANSFER")]
        [Description("Przelew podatkowy")]
        TransferTax
    }
}
