using System;
using System.ComponentModel;

namespace BankService.Bank_PL_mBank
{
    public class mBankHistoryFilter : HistoryFilter
    {
        public mBankFilterOperationType? OperationType { get; set; }

        public mBankHistoryFilter() : base()
        {
        }

        public mBankHistoryFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact) : base(direction, title, dateFrom, dateTo, amountExact)
        {
        }
    }

    [Description("Rodzaj operacji")]
    public enum mBankFilterOperationType
    {
        [Description("Wszystkie")]
        All,
        [Description("Obciążenia")]
        Outgoing,
        [Description("Uznania")]
        Incoming,
    }
}
