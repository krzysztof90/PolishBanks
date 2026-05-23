using System.ComponentModel;
using Tools;

namespace BankService.Bank_PL_Pocztowy
{
    public enum PocztowyJsonTransferType
    {
        [JsonValue("TRANSFER")]
        [Description("Przelew")]
        Transfer,
        [JsonValue("TRANSFER_US")]
        [Description("Przelew podatkowy")]
        TransferTax,
        [JsonValue("PREPAID_CHARGE")]
        [Description("Doładowanie telefonu")]
        Prepaid,
        [JsonValue("PRZELEW_NA_TELEFON_BLIK")]
        [Description("Przelew na telefon")]
        PhoneTransfer
    }
}
