﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Tools;
using static BankService.Bank_VeloBank.VeloBankJsonResponse;

namespace BankService.Bank_VeloBank
{
    public class VeloBankHistoryItem : HistoryItem
    {
        public VeloBankJsonCategoryType Type { get; }
        public VeloBankJsonOperationStatusType? Status { get; }
        public VeloBankJsonOperationType OperationType { get; }
        public DateTime? PostingDate { get; }
        public string ReferenceNumber { get; }
        public string PaymentSystem { get; }
        public string PaymentRecommendationAccountNumber { get; }
        public string CommissionsChargedAccountNumber { get; }
        public string ExchangeRate { get; }
        public string CardNumber { get; }
        public string CardProvider { get; }
        public string OperationTitle { get; }
        public string FromAccountBankName { get; }
        public string ToAccountBankName { get; }
        public string ToAccountBankSwiftCode { get; }

        public override bool IsTransfer => Type == VeloBankJsonCategoryType.Transfer;
        public override string TransferTypeName => Type.GetEnumDescription();
        public override bool CompareTitle(string title)
        {
            return Title == title;
        }

        public VeloBankHistoryItem(VeloBankJsonResponseHistoryItem item)
        {
            Id = item.id;
            OrderDate = item.DateValue;
            Type = (VeloBankJsonCategoryType)item.CategoryValue;
            OperationType = (VeloBankJsonOperationType)item.OperationTypeValue;
            Direction = (VeloBankJsonSideType)item.SideValue == VeloBankJsonSideType.Credit ? OperationDirection.Income : OperationDirection.Execute;
            if (item.remitter != null)
            {
                FromAccountNumber = item.remitter.nrb?.account_number;
                FromPersonName = item.remitter.name;
                FromPersonAddress = item.remitter.address;
                FromAccountBankName = item.remitter.bank_name;
            }
            if (item.recipient != null)
            {
                ToAccountNumber = item.recipient.nrb?.account_number;
                ToPersonName = item.recipient.name;
                ToPersonAddress = item.recipient.address;
                ToAccountBankName = item.recipient.bank_name;
                ToAccountBankSwiftCode = item.recipient.bank?.bic_or_swift;
            }
            else if (item.merchant != null)
            {
                ToPersonName = item.merchant.name;
            }
            Amount = item.amount.amount;
            Currency = item.amount.currency;
            Balance = item.balance?.amount ?? 0;
            Title = item.title;
            Status = item.StatusValue;
            if (item.accounting_date != null)
                PostingDate = DateTime.Parse(item.accounting_date);
            ReferenceNumber = item.ref_no;
            AmountInCurrency = item.amount_pln.amount;
            CardNumber = item.card_number;
            CardProvider = item.card_provider;
        }
    }
}
