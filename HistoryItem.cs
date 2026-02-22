using BankService.Tax.TaxCreditorIdentifiers;
using BankService.Tax.TaxPeriods;
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
        public string FromAccountNumber { get; set; }
        public string FromPersonName { get; protected set; }
        public string FromPersonAddress { get; protected set; }
        public string ToAccountNumber { get; set; }
        public string ToPersonName { get; protected set; }
        public string ToPersonAddress { get; protected set; }
        public string Title { get; protected set; }
        public string PaymentOfServicesEntityNumber { get; protected set; }
        public string PaymentOfServicesReferenceNumber { get; protected set; }

        public double AmountValue => AmountInCurrency != 0 ? AmountInCurrency : Amount;
        public string AnotherAccountNumber => Direction == OperationDirection.Execute ? ToAccountNumber : FromAccountNumber;
        public string RecipientName => Direction == OperationDirection.Execute ? ToPersonName : FromPersonName;
        public string RecipientAddress => Direction == OperationDirection.Execute ? ToPersonAddress : FromPersonAddress;

        public abstract bool IsTransfer { get; }
        public abstract bool IsTaxTransfer { get; }
        public abstract bool IsPaymentOfServices { get; }
        public abstract string TransferTypeName { get; }
        public abstract bool CompareTitle(string title);
        public abstract bool CompareTax(string taxType, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier);
        public abstract bool ComparePaymentOfServicesReferenceNumber(string referenceNumber);
    }

    public enum OperationDirection
    {
        [Description("Wychodzące")]
        Execute,
        [Description("Przychodzące")]
        Income
    }
}
