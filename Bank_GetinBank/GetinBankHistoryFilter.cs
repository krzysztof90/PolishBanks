using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Tools;

namespace BankService.Bank_GetinBank
{
    public class GetinBankHistoryFilter : HistoryFilter
    {
        public GetinBankFilterRange Range { get; set; }
        private int lastOperationCount;
        public int LastOperationCount
        {
            get { return lastOperationCount; }
            set
            {
                lastOperationCount = value;
                if (value != 0)
                    Range = GetinBankFilterRange.LastOperations;
            }
        }
        private GetinBankFilterOperation? operationType;
        public GetinBankFilterOperation? OperationType
        {
            get
            {
                if (Direction == OperationDirection.Income)
                    return GetinBankFilterOperation.Incoming;
                if (Direction == OperationDirection.Execute)
                    return GetinBankFilterOperation.Outgoing;
                return operationType;
                //switch (
            }
            set
            {
                operationType = value;

                if (operationType == GetinBankFilterOperation.Incoming)
                    Direction = OperationDirection.Income;
                if (operationType == GetinBankFilterOperation.Outgoing)
                    Direction = OperationDirection.Execute;
            }
        }
        private GetinBankFilterChannel? channelType;
        public GetinBankFilterChannel? ChannelType
        {
            get { return channelType; }
            set
            {
                channelType = value;
            }
        }
        private GetinBankFilterStatus? statusType;
        public GetinBankFilterStatus? StatusType
        {
            get { return statusType; }
            set
            {
                statusType = value;
            }
        }

        public bool FindAccountNumber => AccountNumber != null;
        public bool SetTitle => Title != null;
        public bool SetDetails => Range != GetinBankFilterRange.None || Amount || OperationType != null || ChannelType != null || StatusType != null;

        public GetinBankHistoryFilter() : base()
        {
        }

        public GetinBankHistoryFilter(string _accountNumber, string _title, DateTime? _dateFrom, DateTime? _dateTo, DateTime? _dateExact, int _lastOperationCount, double? _amountFrom, double? _amountTo, double? _amountExact, GetinBankFilterOperation? _operationType, GetinBankFilterChannel? _channelType, GetinBankFilterStatus? _statusType)
            : base(_accountNumber, _title, _dateFrom, _dateTo, _dateExact, _amountFrom, _amountTo, _amountExact)
        {
            LastOperationCount = _lastOperationCount;
            OperationType = _operationType;
            ChannelType = _channelType;
            StatusType = _statusType;
        }

        protected override void Init()
        {
            base.Init();

            OnDateSet += new Action(() =>
            {
                Range = GetinBankFilterRange.Date;
            });
        }

        public IEnumerable<KeyValuePair<string, string>> CreateTitleParameters()
        {
            return new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("title", Title),
            };
        }

        public IEnumerable<KeyValuePair<string, string>> CreateDetailsParameters()
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

            switch (Range)
            {
                case GetinBankFilterRange.None:
                    break;
                case GetinBankFilterRange.Date:
                    result.Add(new KeyValuePair<string, string>("date", "1"));
                    result.Add(new KeyValuePair<string, string>("dateFrom", (DateFrom ?? DateTime.MinValue).ToString("dd.MM.yyyy", CultureInfo.CreateSpecificCulture("es-ES"))));
                    result.Add(new KeyValuePair<string, string>("dateFrom_submit", DateTime.Now.ToString("dd.MM.yyyy", CultureInfo.CreateSpecificCulture("es-ES"))));
                    result.Add(new KeyValuePair<string, string>("dateTo", (DateTo ?? DateTime.Now).ToString("dd.MM.yyyy", CultureInfo.CreateSpecificCulture("es-ES"))));
                    result.Add(new KeyValuePair<string, string>("dateTo_submit", (DateTo ?? DateTime.Now).ToString("dd.MM.yyyy", CultureInfo.CreateSpecificCulture("es-ES"))));
                    break;
                case GetinBankFilterRange.LastOperations:
                    result.Add(new KeyValuePair<string, string>("date", "2"));
                    result.Add(new KeyValuePair<string, string>("last-operation", LastOperationCount.ToString()));
                    break;
            }
            if (Amount)
            {
                result.Add(new KeyValuePair<string, string>("amount", "1"));
                result.Add(new KeyValuePair<string, string>("amountFrom", (AmountFrom ?? 0).ToString("F", CultureInfo.CreateSpecificCulture("en-CA"))));
                result.Add(new KeyValuePair<string, string>("amountTo", (AmountTo ?? 1000000000).ToString("F", CultureInfo.CreateSpecificCulture("en-CA"))));
            }

            if (OperationType != null)
            {
                result.Add(new KeyValuePair<string, string>("operation", "1"));
                result.Add(new KeyValuePair<string, string>("operation_select", AttributeOperations.GetEnumAttribute((GetinBankFilterOperation)OperationType, (FilterEnumParameterAttribute parameter) => parameter.Parameter, null)));
            }

            if (ChannelType != null)
            {
                result.Add(new KeyValuePair<string, string>("channel", "1"));
                result.Add(new KeyValuePair<string, string>("channel_select", AttributeOperations.GetEnumAttribute((GetinBankFilterChannel)ChannelType, (FilterEnumParameterAttribute parameter) => parameter.Parameter, null)));
            }

            if (StatusType != null)
            {
                result.Add(new KeyValuePair<string, string>("status", "1"));
                result.Add(new KeyValuePair<string, string>("status_select", AttributeOperations.GetEnumAttribute((GetinBankFilterStatus)StatusType, (FilterEnumParameterAttribute parameter) => parameter.Parameter, null)));
            }

            //result.Add(new KeyValuePair<string, string>("last-operation", ""));
            //result.Add(new KeyValuePair<string, string>("amountFrom", ""));
            //result.Add(new KeyValuePair<string, string>("amountTo", ""));
            //result.Add(new KeyValuePair<string, string>("operation_select", ""));
            //result.Add(new KeyValuePair<string, string>("channel_select", ""));
            //result.Add(new KeyValuePair<string, string>("status_select", ""));

            return result;
        }
    }

    public enum GetinBankFilterRange
    {
        None,
        Date,
        LastOperations
    }
    [Description("Rodzaj operacji")]
    public enum GetinBankFilterOperation
    {
        [Description("Wszystkie uznania")]
        [FilterEnumParameterAttribute("CREDIT")]
        Incoming,
        [Description("Wszystkie obciążenia")]
        [FilterEnumParameterAttribute("DEBIT")]
        Outgoing,
        [Description("Przelewy")]
        [FilterEnumParameterAttribute("TRANSFER")]
        Transfers,
        [Description("Przelewy express elixir")]
        [FilterEnumParameterAttribute("EXPRESS_ELIKSIR")]
        Elixir,
        [Description("Operacje kartą")]
        [FilterEnumParameterAttribute("CARD_TRANSACTION")]
        Card,
        [Description("Doładowania")]
        [FilterEnumParameterAttribute("PREPAID")]
        Prepaid,
        [Description("Transakcje iKasa")]
        [FilterEnumParameterAttribute("IKASA_TRANSACTION")]
        IKasa,
        [Description("Wpłata kasowa")]
        [FilterEnumParameterAttribute("CASH_TRANSACTION_IN")]
        CashIn,
        [Description("Wypłata kasowa")]
        [FilterEnumParameterAttribute("CASH_TRANSACTION_OUT")]
        CashOut,
        [Description("Operacje BLIK")]
        [FilterEnumParameterAttribute("BLIK")]
        Blik
    }
    [Description("Kanał operacji")]
    public enum GetinBankFilterChannel
    {
        [Description("Bankowość/placówka")]
        [FilterEnumParameterAttribute("ONLINE_BANKING")]
        Bank,
        [Description("Mobile")]
        [FilterEnumParameterAttribute("MOBILE_BANKING")]
        Mobile,
        [Description("Infolinia")]
        [FilterEnumParameterAttribute("CALL_CENTER")]
        CallCenter,
        [Description("Kanały alternatywne")]
        [FilterEnumParameterAttribute("ALTERNATE")]
        Alternate,
        [Description("Podmioty zewnętrzne")]
        [FilterEnumParameterAttribute("TPP")]
        External
    }
    [Description("Status operacji")]
    public enum GetinBankFilterStatus
    {
        [Description("Zrealizowane")]
        [FilterEnumParameterAttribute("REALIZED")]
        Realized,
        [Description("Oczekujące")]
        [FilterEnumParameterAttribute("WAITING")]
        Waiting,
        [Description("Blokady kartowe")]
        [FilterEnumParameterAttribute("is_authorizations_only")]
        AuthorizationsOnly,
        [Description("Niezrealizowane")]
        [FilterEnumParameterAttribute("REJECTED")]
        Rejected
    }
}
