using System;
using System.ComponentModel;

namespace BankService.Bank_PL_Nest
{
    public class NestHistoryFilter : HistoryFilter
    {
        public NestFilterOperationType? OperationType { get; set; }

        public NestHistoryFilter() : base()
        {
        }

        public NestHistoryFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact) : base(direction, title, dateFrom, dateTo, amountExact)
        {
        }
    }

    [Description("Rodzaj operacji")]
    public enum NestFilterOperationType
    {
        [Description("Uznania")]
        [FilterEnumParameterAttribute("CREDIT")]
        Credit,
        [Description("Obciążenia")]
        [FilterEnumParameterAttribute("DEBIT")]
        Debit,
        [Description("Przelewy przychodzące")]
        [FilterEnumParameterAttribute("INCOMING_TRANSFER")]
        Incoming,
        [Description("Przelewy wychodzące")]
        [FilterEnumParameterAttribute("OUTGOING_TRANSFER")]
        Outgoing,
        [Description("Operacje kartą")]
        [FilterEnumParameterAttribute("CARD_PAYMENT")]
        Card,
        [Description("Wypłaty z bankomatów")]
        [FilterEnumParameterAttribute("WITHDRAWAL_FROM_CASH_MACHINE")]
        ATMWithdraw,
        [Description("Wpłaty gotówkowe")]
        [FilterEnumParameterAttribute("CASH_INCOME")]
        CashIn,
        [Description("Wypłaty gotówkowe")]
        [FilterEnumParameterAttribute("CASH_WITHDRAWAL")]
        CashOut,
        [Description("Operacje BLIK")]
        [FilterEnumParameterAttribute("BLIK")]
        Blik,
        [Description("Opłaty i prowizje")]
        [FilterEnumParameterAttribute("FEES_AND_COMMISSIONS")]
        Fee,
        [Description("Inne")]
        [FilterEnumParameterAttribute("OTHER")]
        Other,
        [Description("Transakcje partnerów")]
        [FilterEnumParameterAttribute("TPP_TRANSACTION")]
        TppTransaction
    }
}
