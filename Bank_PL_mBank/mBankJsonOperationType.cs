using System.ComponentModel;
using Tools;

namespace BankService.Bank_PL_mBank
{
    public enum mBankJsonOperationType
    {
        [JsonValue("Transfer")]
        [Description("Transfer")]
        Transfer
    }
}
