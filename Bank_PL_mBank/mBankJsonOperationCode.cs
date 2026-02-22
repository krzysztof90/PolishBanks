using System.ComponentModel;
using Tools;

namespace BankService.Bank_PL_mBank
{
    public enum mBankJsonOperationCode
    {
        [JsonValue("TRI")]
        [Description("Transfer przychodzący")]
        TransferIncoming,
        [JsonValue("TRO")]
        [Description("Transfer wychodzący")]
        TransferOutgoing,
        [JsonValue("TUS")]
        [Description("Transfer podatkowy")]
        TransferTax
    }
}
