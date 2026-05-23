using System.ComponentModel;
using Tools;

namespace BankService.Bank_PL_Nest
{
    public enum NestJsonOperationType
    {
        [JsonValue("OUTGOING_TRANSFER")]
        [Description("Przelew wychodzący")]
        TransferOutgoing,
        [JsonValue("INCOMING_TRANSFER")]
        [Description("Przelew przychodzący")]
        TransferIncoming
    }
}
