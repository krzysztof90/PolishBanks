using System;
using System.ComponentModel;

namespace BankService.Bank_PL_Pocztowy
{
    public class PocztowyHistoryFilter : HistoryFilter
    {
        public PocztowyFilterOperationType? OperationType { get; set; }

        public PocztowyHistoryFilter() : base()
        {
        }

        public PocztowyHistoryFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact) : base(direction, title, dateFrom, dateTo, amountExact)
        {
        }
    }

    [Description("Rodzaj operacji")]
    public enum PocztowyFilterOperationType
    {
        //TODO not needed
        [Description("Wszystkie")]
        [FilterEnumParameterAttribute("ALL")]
        All,
        [Description("Obciążenia")]
        [FilterEnumParameterAttribute("REMIT")]
        Remit,
        [Description("Uznania")]
        [FilterEnumParameterAttribute("BENEFIT")]
        Benefit,
        [Description("Opłaty i prowizje")]
        [FilterEnumParameterAttribute("FEE")]
        Fee,
        [Description("Transakcje kartą")]
        [FilterEnumParameterAttribute("CARD")]
        Card,
    }
}
