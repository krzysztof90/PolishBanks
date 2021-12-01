using System;
using System.ComponentModel;

namespace BankService
{
    public abstract class HistoryItem
    {
        public string Id { get; protected set; }
        public OperationDirection Direction { get; protected set; }
        public string Currency { get; protected set; }
        public double Amount { get; protected set; }
        public double AmountInCurrency { get; protected set; }
        public double Balance { get; protected set; }
        public DateTime OrderDate { get; protected set; }
        public string FromAccountNumber { get; protected set; }
        public string FromPersonName { get; protected set; }
        public string FromPersonAddress { get; protected set; }
        public string ToAccountNumber { get; protected set; }
        public string ToPersonName { get; protected set; }
        public string ToPersonAddress { get; protected set; }
        public string Title { get; protected set; }

        public double AmountValue => (Currency == "PLN" ? Amount : AmountInCurrency);
        public string AnotherAccountNumber => Direction == OperationDirection.Execute ? ToAccountNumber : FromAccountNumber;
        public string RecipientName => Direction == OperationDirection.Execute ? ToPersonName : FromPersonName;
        public string RecipientAddress => Direction == OperationDirection.Execute ? ToPersonAddress : FromPersonAddress;

        public abstract bool IsTransfer { get; }
        public abstract string TransferTypeName { get; }
    }

    public enum OperationDirection
    {
        [Description("Wychodzące")]
        Execute,
        [Description("Przychodzące")]
        Income
    }
}
