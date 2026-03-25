using System;
using System.ComponentModel;

namespace BankService.Bank_PL_MBank
{
    public class MBankHistoryFilter : HistoryFilter
    {
        public MBankFilterOperationType? OperationType { get; set; }

        public MBankHistoryFilter() : base()
        {
        }

        public MBankHistoryFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact) : base(direction, title, dateFrom, dateTo, amountExact)
        {
        }
    }

    [Description("Rodzaj operacji")]
    public enum MBankFilterOperationType
    {
        [Description("Wszystkie")]
        All,
        [Description("Obciążenia")]
        Outgoing,
        [Description("Uznania")]
        Incoming,
    }
}
