using System.ComponentModel;
using Tools;

namespace BankService.Bank_PL_Pocztowy
{
    public enum PocztowyJsonOutgoingTransferType
    {
        [JsonValue("ATTENDANCE")]
        [Description("Autoryzacja")]
        Attendance,
        [JsonValue("TRANSFER_DOMESTIC")]
        [Description("Przelew zwykły")]
        TransferDomestic,
        [JsonValue("TRANSFER_TOP_UP")]
        [Description("Doładowanie telefonu")]
        Prepaid,
        [JsonValue("TRANSFER_PBL")]
        [Description("Szybki przelew")]
        PayByLink,
        [JsonValue("TRANSFER_TAX_IRP")]
        [Description("Przelew podatkowy IRP")]
        TransferTaxIRP,
        [JsonValue("TRANSFER_TAX_TO_US")]
        [Description("Przelew podatkowy US")]
        TransferTaxUS
    }
}
