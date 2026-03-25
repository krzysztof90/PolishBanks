using System.ComponentModel;
using Tools;

namespace BankService.Bank_PL_MBank
{
    public enum MBankJsonOperationCode
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
