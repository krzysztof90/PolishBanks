using Tools;

namespace BankService.Bank_PL_Pocztowy
{
    public enum PocztowyJsonCreditDebit
    {
        [JsonValue("DEBIT")]
        Debit,
        [JsonValue("CREDIT")]
        Credit
    }
}
