using System;

namespace BankService.Bank_PT_Santander
{
    public class SantanderHistoryFilter : HistoryFilter
    {
        public SantanderHistoryFilter() : base()
        {
        }

        public SantanderHistoryFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact) : base(direction, title, dateFrom, dateTo, amountExact)
        {
        }
    }
}
