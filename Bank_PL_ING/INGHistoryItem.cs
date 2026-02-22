using BankService.Tax.TaxCreditorIdentifiers;
using BankService.Tax.TaxPeriods;
using System;
using System.Linq;
using System.Text;
using Tools;
using static BankService.Bank_PL_ING.INGJsonResponse;

namespace BankService.Bank_PL_ING
{
    public class INGHistoryItem : HistoryItem
    {
        public INGJsonTransferType Type { get; }

        public INGHistoryItem(INGJsonResponseHistoryDataTransactionM transaction)
        {
            Id = transaction.id;
            OrderDate = (DateTime)transaction.DateValue;
            Type = (INGJsonTransferType)transaction.TypeValue;
            Direction = transaction.CreditDebitValue == INGJsonCreditDebit.Credit ? OperationDirection.Income : OperationDirection.Execute;
            FromAccountNumber = transaction.aw;
            FromPersonName = JoinDescription(transaction.w1, transaction.w2, transaction.w3, transaction.w4);
            ToAccountNumber = transaction.am;
            ToPersonName = JoinDescription(transaction.m1, transaction.m3);
            Amount = transaction.amt;
            Currency = transaction.cr;
            Balance = transaction.bal;
            Title = JoinDescription(transaction.t1, transaction.t2, transaction.t3, transaction.t4);
        }

        private string JoinDescription(params string[] descriptions)
        {
            //TODO breaklines
            return string.Join(" ", descriptions.Where(d => d != null));
        }

        //TODO transfer confirmations cannot be download for Blokada kartowa, Zdjęcie blokady, Usunięcie blokady icbs

        public override bool IsTransfer => Type == INGJsonTransferType.TransferInternet2 || Type == INGJsonTransferType.NewZUS;
        public override bool IsTaxTransfer => Type == INGJsonTransferType.TransferTax;
        public override bool IsPaymentOfServices => false;
        public override string TransferTypeName => Type.GetEnumDescription();
        public override bool CompareTitle(string title)
        {
            return Title.TrimEnd() == title.TrimEnd();
        }
        public override bool CompareTax(string taxType, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier)
        {
            string[] parts = Title.Split(new string[] { "/" }, StringSplitOptions.None);

            return parts[6] == taxType
                && parts[4] == ING.GetTaxPeriodValue(period)
                && parts[2] == ING.GetTaxCreditorIdentifierTypeId(creditorIdentifier) + creditorIdentifier.GetId();
        }
        public override bool ComparePaymentOfServicesReferenceNumber(string referenceNumber)
        {
            throw new ArgumentException();
        }
    }
}
