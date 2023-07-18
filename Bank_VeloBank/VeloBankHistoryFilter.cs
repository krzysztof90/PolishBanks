using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Tools;

namespace BankService.Bank_VeloBank
{
    public class VeloBankHistoryFilter : HistoryFilter
    {
        private VeloBankFilterOperation? operationType;
        public VeloBankFilterOperation? OperationType
        {
            get
            {
                if (Direction == OperationDirection.Income)
                    return VeloBankFilterOperation.Incoming;
                if (Direction == OperationDirection.Execute)
                    return VeloBankFilterOperation.Outgoing;
                return operationType;
            }
            set
            {
                operationType = value;

                if (operationType == VeloBankFilterOperation.Incoming)
                    Direction = OperationDirection.Income;
                if (operationType == VeloBankFilterOperation.Outgoing)
                    Direction = OperationDirection.Execute;
            }
        }
        public VeloBankFilterKind? KindType { get; set; }
        public VeloBankFilterStatus? StatusType { get; set; }

        public bool FindAccountNumber => AccountNumber != null;

        public VeloBankHistoryFilter() : base()
        {
        }

        public VeloBankHistoryFilter(string _accountNumber, string _title, DateTime? _dateFrom, DateTime? _dateTo, DateTime? _dateExact, double? _amountFrom, double? _amountTo, double? _amountExact, VeloBankFilterKind? _kindType, VeloBankFilterStatus? _statusType)
            : base(_accountNumber, _title, _dateFrom, _dateTo, _dateExact, _amountFrom, _amountTo, _amountExact)
        {
            KindType = _kindType;
            StatusType = _statusType;
        }

        protected override void Init()
        {
            base.Init();
        }
    }

    [Description("Rodzaj operacji")]
    public enum VeloBankFilterOperation
    {
        [Description("Wpływy")]
        [FilterEnumParameterAttribute("IN")]
        Incoming,
        [Description("Wydatki")]
        [FilterEnumParameterAttribute("OUT")]
        Outgoing,
    }
        [Description("Typ operacji")]
    public enum VeloBankFilterKind
    {
        [Description("Przelewy")]
        [FilterEnumParameterAttribute("TRANSFERS")]
        Transfers,
        [Description("Operacje kartą")]
        [FilterEnumParameterAttribute("CARD_OPERATIONS")]
        Card,
        [Description("Operacje BLIK")]
        [FilterEnumParameterAttribute("BLIK_OPERATIONS")]
        Blik,
        [Description("Wpłaty / Wypłaty")]
        [FilterEnumParameterAttribute("CASH_OPERATIONS")]
        Cash,
        [Description("Opłaty i prowizje")]
        [FilterEnumParameterAttribute("FEES_AND_COMMISSIONS")]
        FeeCommission,
        [Description("Inne")]
        [FilterEnumParameterAttribute("OTHERS")]
        Other,
    }
    [Description("Status operacji")]
    public enum VeloBankFilterStatus
    {
        [Description("Zrealizowane")]
        [FilterEnumParameterAttribute("DONE")]
        Realized,
        [Description("Oczekujące")]
        [FilterEnumParameterAttribute("SUBMITTED")]
        Waiting,
        [Description("Odrzucone")]
        [FilterEnumParameterAttribute("REJECTED")]
        Rejected
    }
}
