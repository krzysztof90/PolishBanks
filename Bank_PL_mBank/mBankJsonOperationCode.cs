using System.ComponentModel;
using Tools;

namespace BankService.Bank_PL_MBank
{
    public enum MBankJsonOperationCode
    {
        [JsonValue("TRO")]
        [Description("Przelew wychodzący")]
        TransferOutgoing,
        [JsonValue("TRI")]
        [Description("Przelew przychodzący")]
        TransferIncoming,
        [JsonValue("TUS")]
        [Description("Przelew podatkowy")]
        TransferTax
    }
}
