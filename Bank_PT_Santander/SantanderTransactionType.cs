using System.ComponentModel;

namespace BankService.Bank_PT_Santander
{
    public enum SantanderTransactionType
    {
        [HtmlLabel(null)]
        [Description("Niepobrane szczegóły")]
        Empty,

        [HtmlLabel("Transferência")]
        [Description("Transfer")]
        Transfer,
        [HtmlLabel("Pagamento de Serviços")]
        [Description("Pagamento de Serviços")]
        PaymentOfServices,
        [HtmlLabel("Compra")]
        [Description("Zakupy")]
        Purchase,
        [HtmlLabel("Levantamento")]
        [Description("Wypłata z bankomatu")]
        ATMWithdraw,
        [HtmlLabel("Compra no estrangeiro")]
        [Description("Zakupy za granicą")]
        PurchaseAbroad,
        [HtmlLabel("Débito direto")]
        [Description("Polecenie zapłaty")]
        DirectDebit,
        [HtmlLabel("Encargo e imposto")]
        [Description("Opłaty i prowizje")]
        FeeCommission,
        [HtmlLabel("Outros movimentos")]
        [Description("Pozostałe")]
        Other,
    }
}
