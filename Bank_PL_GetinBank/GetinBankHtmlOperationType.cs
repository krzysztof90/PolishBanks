using System.ComponentModel;

namespace BankService.Bank_PL_GetinBank
{
    public enum GetinBankHtmlOperationType
    {
        [HtmlLabel("Przelew")]
        [Description("Przelew")]
        Transfer,
        [HtmlLabel("Operacja kartą")]
        [Description("Operacja kartą")]
        Card,
        [HtmlLabel("Express Elixir")]
        [Description("Express Elixir")]
        Elixir,
        [HtmlLabel("PRZELEW ZAGRANICZNY")]
        [Description("Przelew zagraniczny")]
        Foreign,
        [HtmlLabel("us")]
        [Description("Urząd skarbowy")]
        TransferTax,
        [HtmlLabel("Przelew na telefon")]
        [Description("Przelew na telefon")]
        PhoneTransfer,
        [HtmlLabel("Operacja BLIK")]
        [Description("Operacja BLIK")]
        Blik
    }
}
