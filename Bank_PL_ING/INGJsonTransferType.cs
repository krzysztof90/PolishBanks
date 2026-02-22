using System.ComponentModel;
using Tools;

namespace BankService.Bank_PL_ING
{
    public enum INGJsonTransferType
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
        [JsonValueInt(22)]
        [Description("Wypłata Bankomat")]
        ATMWithdraw,
        [JsonValueInt(24)]
        [Description("Zwrot płatności kartą")]
        CardReturn,
        [JsonValueInt(50)]
        [Description("Założenie blokady")]
        BlockEstablish,
        [JsonValueInt(51)]
        [Description("Zdjęcie blokady")]
        BlockRelease,
        [JsonValueInt(56)]
        [Description("Usunięcie blokady ICBS")]
        BlockReleaseICBS,
        [JsonValueInt(100)]
        [Description("Nowy ZUS")]
        NewZUS,
        [JsonValueInt(105)]
        [Description("Przelew podatkowy")]
        TransferTax,
        [JsonValueInt(110)]
        [Description("Przelew własny")]
        OwnTransfer,
        [JsonValueInt(112)]
        [Description("Przelew własny +")]
        OwnTransferPlus,
        [JsonValueInt(117)]
        [Description("Przelew Internet")]
        TransferInternet,
        [JsonValueInt(120)]
        [Description("Przelew przychodzący CIB")]
        TransferReturnCIB,
        [JsonValueInt(121)]
        [Description("Przelew Internet")]
        TransferInternet2,
        [JsonValueInt(130)]
        [Description("Płatność PayU")]
        PayUPayment,
        [JsonValueInt(160)]
        [Description("Przelew walutowy")]
        CurrencyTransfer,
        [JsonValueInt(180)]
        [Description("Prowizje i opłaty")]
        CommissionsAndFees,
        [JsonValueInt(200)]
        [Description("Blokada kartowa")]
        BlockCard,
        [JsonValueInt(222)]
        [Description("Płatość kartą")]
        CardPayment,
        [JsonValueInt(512)]
        [Description("Zakup biletu parkingowego")]
        ParkingTicket,
        [JsonValueInt(890)]
        [Description("Przelew na telefon")]
        PhoneTransfer
    }
}
