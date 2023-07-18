using System.ComponentModel;
using Tools;

namespace BankService.Bank_ING
{
    public enum INGJsonResponseType
    {
        [JsonValueInt(1)]
        [Description("Przelew - obciążenie")]
        TransferCharge,
        [JsonValueInt(4)]
        [Description("Przelew - uznanie")]
        TransferCredit,
        [JsonValueInt(5)]
        [Description("Uznanie Elixir")]
        CreditElixir,
        [JsonValueInt(20)]
        [Description("BLIK TR ZAKUPU INTERNETOWA")]
        BlikInternet,
        [JsonValueInt(24)]
        [Description("Zwrot płatności kartą")]
        CardReturn,
        [JsonValueInt(51)]
        [Description("Zdjęcie Blokady")]
        BlockRelease,
        [JsonValueInt(110)]
        [Description("Przelew własny")]
        OwnTransfer,
        [JsonValueInt(112)]
        [Description("Przelew własny +")]
        OwnTransferPlus,
        [JsonValueInt(117)]
        [Description("Przelew Internet")]
        TransferInternet,
        [JsonValueInt(121)]
        [Description("Przelew Internet")]
        TransferInternet2,
        [JsonValueInt(130)]
        [Description("Płatność PayU")]
        PayUPayment,
        [JsonValueInt(180)]
        [Description("Prowizje i opłaty")]
        CommissionsAndFees,
        [JsonValueInt(200)]
        [Description("Blokada kartowa")]
        Block,
        [JsonValueInt(222)]
        [Description("Płatość kartą")]
        CardPayment
    }
}
