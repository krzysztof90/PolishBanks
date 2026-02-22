using BankService.Tax.TaxCreditorIdentifiers;
using BankService.Tax.TaxPeriods;
using System;
using Tools;
using static BankService.Bank_PL_mBank.mBankJsonResponse;

namespace BankService.Bank_PL_mBank
{
    public class mBankHistoryItem : HistoryItem
    {
        public string FromPersonBankName { get; protected set; }
        public string ToPersonBankName { get; protected set; }

        public mBankJsonOperationCode OperationCode { get; }
        public mBankJsonOperationType OperationType { get; }
        public mBankJsonOperationKind? OperationKind { get; }
        public mBankJsonCategory Category { get; }

        public string AccountNumber { get; protected set; }
        public int OperationNumber { get; protected set; }

        public mBankHistoryItem(mBankJsonResponseTransactionsTransaction transaction)
        {
            Id = transaction.pfmId.ToString();
            Direction = transaction.amount > 0 ? OperationDirection.Income : OperationDirection.Execute;
            Currency = transaction.currency;
            Amount = Math.Abs(transaction.amount);
            Balance = transaction.balance;
            OrderDate = transaction.transactionDate;
            Title = transaction.description;
            //transaction.accountNumber
            //transaction.accountName
            //transaction.subDescription
            //transaction.subAccountDescription
            //transaction.categoryId
            //transaction.comment
            //transaction.originalTransactionDate

            OperationCode = (mBankJsonOperationCode)transaction.OperationCodeValue;
            OperationType = (mBankJsonOperationType)transaction.OperationTypeValue;
            Category = (mBankJsonCategory)transaction.CategoryValue;

            AccountNumber = transaction.accountNumber;
            OperationNumber = transaction.operationNumber;

            OperationKind = mBankJsonOperationKind.Empty;
        }

        public mBankHistoryItem(mBankJsonResponseTransactionsTransaction transaction, mBankJsonResponseTransaction transactionDetails) : this(transaction)
        {
            Title = transactionDetails.details["cTitle1"].value;
            FromPersonAddress = transactionDetails.details["cSenderAddress1"].value;
            string fromPersonCity = transactionDetails.details["cSenderCity"].value;
            if (!String.IsNullOrEmpty(fromPersonCity))
                FromPersonAddress += (!String.IsNullOrEmpty(FromPersonAddress) ? ", " : String.Empty) + fromPersonCity;
            ToPersonAddress = transactionDetails.details["cRecAddress1"].value;
            string toPersonCity = transactionDetails.details["cRecCity"].value;
            if (!String.IsNullOrEmpty(toPersonCity))
                ToPersonAddress += (!String.IsNullOrEmpty(ToPersonAddress) ? ", " : String.Empty) + toPersonCity;
            //transactionDetails.details["mTransAmount"].value; //Kwota operacji
            //transactionDetails.details["mBalance"].value; //Saldo po operacji
            //transactionDetails.details["dValueDate"].value; //Data operacji
            //transactionDetails.details["dTransDate"].value; //Data księgowania
            //transactionDetails.details["iNumber"].value; //Numer operacji
            FromAccountNumber = transactionDetails.details["senderAccountNumber"].value;
            ToAccountNumber = transactionDetails.details["receiverAccountNumber"].value;
            //transactionDetails.details["senderAccountName"].value;
            //transactionDetails.details["receiverAccountName"].value;

            OperationKind = transactionDetails.details["cDescription"].value.GetEnumByJsonValueNoEmpty<mBankJsonOperationKind>();

            if (Direction == OperationDirection.Income)
            {
                FromPersonName = transactionDetails.details["senderName"].value;
                FromPersonBankName = transactionDetails.details["cSenderBank"].value;
                ToPersonName = transactionDetails.details["receiver"].value;
            }
            else
            {
                FromPersonName = transactionDetails.details["sender"].value;
                ToPersonName = transactionDetails.details["receiverName"].value;
                ToPersonBankName = transactionDetails.details["cReceiverBank"].value;
            }
        }

        //TODO change name for IsTransferOutgoing
        public override bool IsTransfer => OperationCode == mBankJsonOperationCode.TransferOutgoing;
        public override bool IsTaxTransfer => OperationCode == mBankJsonOperationCode.TransferTax;
        public override bool IsPaymentOfServices => false;
        public override string TransferTypeName => OperationKind?.GetEnumDescription() ?? OperationType.GetEnumDescription();
        public override bool CompareTitle(string title)
        {
            //TODO only for whole readed (with second constructor)
            return Title == title;
        }
        public override bool CompareTax(string taxType, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier)
        {
            string[] parts = Title.Split(new string[] { " " }, StringSplitOptions.None);

            return parts[2] == taxType
                && parts[1] == mBank.GetTaxPeriodValueShort(period)
                && parts[0] == mBank.GetTaxCreditorIdentifierTypeId(creditorIdentifier) + creditorIdentifier.GetId();
        }
        public override bool ComparePaymentOfServicesReferenceNumber(string referenceNumber)
        {
            throw new ArgumentException();
        }
    }
}
