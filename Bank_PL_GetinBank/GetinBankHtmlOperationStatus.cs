namespace BankService.Bank_PL_GetinBank
{
    public enum GetinBankHtmlOperationStatus
    {
        [HtmlLabel("zrealizowany")]
        Done,
        [HtmlLabel("oczekujący")]
        Pending,
        [HtmlLabel("blokada")]
        Blocked
    }
}
