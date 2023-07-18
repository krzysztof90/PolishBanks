using System;
using Tools;
using static BankService.Bank_ING.INGJsonResponse;

namespace BankService.Bank_ING
{
    public class INGHistoryItem : HistoryItem
    {
        //TODO przelew w innej walucie

        public INGJsonResponseType Type { get; }

        public INGHistoryItem(INGJsonResponseHistoryDataTransactionM transaction)
        {
            Id = transaction.id;
            OrderDate = (DateTime)transaction.DateValue;
            Type = (INGJsonResponseType)transaction.TypeValue;
            Direction = transaction.CreditDebitValue == INGJsonResponseCreditDebit.Credit ? OperationDirection.Income : OperationDirection.Execute;
            FromAccountNumber = transaction.aw;
            //TODO breakline'y
            FromPersonName = $"{transaction.w1} {transaction.w2} {transaction.w3} {transaction.w4}";
            ToAccountNumber = transaction.am;
            ToPersonName = transaction.m1;
            Amount = transaction.amt;
            Currency = transaction.cr;
            Balance = transaction.bal;
            Title = $"{transaction.t1} {transaction.t2} {transaction.t3} {transaction.t4}";
        }

        public override bool IsTransfer => Type == INGJsonResponseType.TransferInternet2;
        public override string TransferTypeName => Type.GetEnumDescription();
        public override bool CompareTitle(string title)
        {
            return Title.TrimEnd() == title.TrimEnd();
        }
    }
}
