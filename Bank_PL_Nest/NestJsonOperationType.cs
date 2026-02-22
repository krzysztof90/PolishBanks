using System.ComponentModel;
using Tools;

namespace BankService.Bank_PL_Nest
{
    public enum NestJsonOperationType
    {
        [JsonValue("INCOMING_TRANSFER")]
        [Description("Przelew przychodzący")]
        TransferIncoming,
        [JsonValue("OUTGOING_TRANSFER")]
        [Description("Przelew wychodzący")]
        TransferOutgoing,
    }
}
