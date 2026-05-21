using BankService.Tax.TaxCreditorIdentifiers;
using BankService.Tax.TaxPeriods;
using System;
using Tools;
using static BankService.Bank_PL_Pocztowy.PocztowyJsonResponse;

namespace BankService.Bank_PL_Pocztowy
{
    public class PocztowyHistoryItem : HistoryItem
    {
        public PocztowyJsonTransferType Type { get; }
        public string FromAccountBankName { get; }
        public string ToAccountBankName { get; }

        public PocztowyHistoryItem(PocztowyJsonResponseHistoryDataResult transaction)
        {
            Id = transaction.operationId;
            Direction = transaction.CreditDebitValue == PocztowyJsonCreditDebit.Credit ? OperationDirection.Income : OperationDirection.Execute;
            Currency = transaction.transactionValue.currencyCode;
            Amount = transaction.transactionValue.amount;
            Balance = transaction.balanceAfter.amount;
            OrderDate = (DateTime)transaction.TransactionDateValue;
            FromAccountNumber = transaction.remitterNRB;
            FromPersonName = transaction.remitterName;
            FromPersonAddress = transaction.remitterAddress;
            FromAccountBankName = transaction.remitterBank;
            ToAccountNumber = transaction.beneficiaryNRB;
            ToPersonName = transaction.beneficiaryName;
            ToAccountBankName = transaction.beneficiaryBank;
            ToPersonAddress = transaction.beneficiaryAddress;
            Title = transaction.title;
            Type = (PocztowyJsonTransferType)transaction.TypeValue;
        }

        public override bool IsTransfer => Type == PocztowyJsonTransferType.Transfer;
        //TODO
        public override bool IsTaxTransfer => throw new NotImplementedException();
        public override bool IsPaymentOfServices => false;
        public override string TransferTypeName => Type.GetEnumDescription();
        public override bool CompareTitle(string title)
        {
            return Title == title;
        }
        public override bool CompareTax(string taxType, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier)
        {
            //TODO
            throw new NotImplementedException();
        }
        public override bool ComparePaymentOfServicesReferenceNumber(string referenceNumber)
        {
            throw new ArgumentException();
        }
    }
}
