using System.ComponentModel;
using Tools;

namespace BankService.Bank_ING
{
    public enum INGJsonResponseType
    {
        [JsonValueInt(1)]
        [Description("Przelew - obciążenie")]
        TransferCharge,
        [JsonValueInt(5)]
        [Description("Uznanie Elixir")]
        CreditElixir,
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
        CommissionsAndFees
    }
}
