using System.ComponentModel;
using Tools;

namespace BankService.Bank_PL_Pocztowy
{
    public enum PocztowyJsonTransferType
    {
        [JsonValue("TRANSFER")]
        [Description("Przelew")]
        Transfer,
        [JsonValue("PREPAID_CHARGE")]
        [Description("Doładowanie telefonu")]
        Prepaid,
        [JsonValue("PRZELEW_NA_TELEFON_BLIK")]
        [Description("Blik na telefon")]
        TransferBlikMobile,
    }
}
