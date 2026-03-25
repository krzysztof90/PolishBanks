using System.ComponentModel;
using Tools;

namespace BankService.Bank_PL_MBank
{
    public enum MBankJsonOperationType
    {
        [JsonValue("Transfer")]
        [Description("Transfer")]
        Transfer
    }
}
