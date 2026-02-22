using BankService.Tax.TaxCreditorIdentifiers;
using BankService.Tax.TaxPeriods;
using System;
using Tools;
using static BankService.Bank_PL_Nest.NestJsonResponse;

namespace BankService.Bank_PL_Nest
{
    public class NestHistoryItem : HistoryItem
    {
        public NestJsonOperationType Type { get; }

        public NestJsonResponseHistoryDetailsTaxData TaxData { get; }

        public NestHistoryItem(NestJsonResponseHistoryItem transaction, NestJsonResponseHistoryDetails transactionDetails)
        {
            Id = transaction.operationNumber.ToString();
            Direction = transaction.CreditDebitValue == NestJsonCreditDebit.Credit ? OperationDirection.Income : OperationDirection.Execute;
            Currency = transaction.currencyCode;
            Amount = transaction.amount;
            Balance = transaction.balanceAfterOperation;
            OrderDate = transaction.TransactionDateValue;
            FromAccountNumber = transactionDetails.debtor.accountNumber;
            FromPersonName = transactionDetails.debtor.name.Replace("\n", " ");
            FromPersonAddress = transactionDetails.debtor.address;
            ToAccountNumber = transactionDetails.creditor.accountNumber;
            ToPersonName = transactionDetails.creditor.name;
            ToPersonAddress = transactionDetails.creditor.address;
            Title = transactionDetails.title;
            TaxData = transactionDetails.taxData;
            Type = (NestJsonOperationType)transaction.OperationTypeValue;
        }

        public override bool IsTransfer => Type == NestJsonOperationType.TransferIncoming || Type == NestJsonOperationType.TransferOutgoing;
        public override bool IsTaxTransfer => TaxData != null;
        public override bool IsPaymentOfServices => false;
        public override string TransferTypeName => Type.GetEnumDescription();
        public override bool CompareTitle(string title)
        {
            return Title == title;
        }
        public override bool CompareTax(string taxType, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier)
        {
            (string unit, string unitShort, string number, string year) = Nest.GetTaxPeriodValue(period);

            return TaxData.formCode == taxType
                && TaxData.periodUnit == unitShort && TaxData.periodNumber == number && TaxData.periodYear == year
                && TaxData.identifierType == Nest.GetTaxCreditorIdentifierTypeId(creditorIdentifier) && TaxData.identifier == creditorIdentifier.GetId();
        }
        public override bool ComparePaymentOfServicesReferenceNumber(string referenceNumber)
        {
            throw new ArgumentException();
        }
    }
}
