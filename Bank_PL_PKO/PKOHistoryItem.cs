using BankService.Tax.TaxCreditorIdentifiers;
using BankService.Tax.TaxPeriods;
using System;
using Tools;
using static BankService.Bank_PL_PKO.PKOJsonRequest;
using static BankService.Bank_PL_PKO.PKOJsonResponse;

namespace BankService.Bank_PL_PKO
{
    public class PKOHistoryItem : HistoryItem
    {
        public PKOOperationKind Type { get; }
        public PKOJsonResponseTaxTransferResponseDataPayerIdentifier TaxPayerIdentifier { get; }
        public string TaxSymbol { get; }
        public PKOJsonResponseTaxTransferResponseDataPeriod TaxPeriod { get; }

        public PKOHistoryItem(PKOJsonResponseHistoryResponseDataItem item)
        {
            Id = item.operation_id;
            Direction = item.operation_kind.side == "CREDIT" ? OperationDirection.Income : OperationDirection.Execute;
            Currency = item.money.currency;
            Amount = Math.Abs(item.money.Amount);
            Balance = item.ending_balance.Amount;
            OrderDate = item.details.OrderDate;
            FromAccountNumber = Direction == OperationDirection.Execute ? item.ref_operation_completed_confirmation.data.object_id.account : item.details.other_account;
            FromPersonName = Direction == OperationDirection.Execute ? null : item.details.sender_name_and_address;
            FromPersonAddress = Direction == OperationDirection.Execute ? null as string : null;
            ToAccountNumber = Direction == OperationDirection.Execute ? item.details.other_account : item.ref_operation_completed_confirmation.data.object_id.account;
            ToPersonName = Direction == OperationDirection.Execute ? item.details.recipient_name_and_address : null;
            ToPersonAddress = Direction == OperationDirection.Execute ? null as string : null;
            switch (item.operation_kind.CodeValue.Value)
            {
                case PKOOperationKind.TransferOutgoing:
                case PKOOperationKind.TransferIncoming:
                case PKOOperationKind.TransferBlikMobile:
                    Title = item.details.title;
                    break;
                case PKOOperationKind.TaxTransfer:
                    Title = $"{item.details.symbol} {item.details.period.type} {item.details.period.year} {item.details.period.month} {item.details.period.number}";
                    TaxPayerIdentifier = item.details.payer_identifier;
                    TaxSymbol = item.details.symbol;
                    TaxPeriod = item.details.period;
                    break;
                default: throw new NotImplementedException();
            }
            Type = item.operation_kind.CodeValue.Value;
        }

        public override bool IsTransfer => Type == PKOOperationKind.TransferOutgoing;
        public override bool IsTaxTransfer => Type == PKOOperationKind.TaxTransfer;
        public override bool IsPaymentOfServices => false;
        public override string TransferTypeName => Type.GetEnumDescription();
        public override bool CompareTitle(string title)
        {
            return StringOperations.Equals(Title, title, true);
        }
        public override bool CompareTax(string taxType, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier)
        {
            (string type, string number, string month, string year) = PKO.GetTaxPeriodValue(period);
            PKOJsonRequestTaxTransferDataPayerIdentifier payerIdentifier = PKO.GetTaxCreditorIdentifier(creditorIdentifier);

            return TaxPeriod.type == type
                && TaxPeriod.number == number
                && TaxPeriod.month == month
                && TaxPeriod.year == year
                && TaxPayerIdentifier.type == payerIdentifier.type
                && TaxPayerIdentifier.id_card == payerIdentifier.id_card
                && TaxPayerIdentifier.passport == payerIdentifier.passport
                && TaxPayerIdentifier.nip == payerIdentifier.nip
                && TaxPayerIdentifier.pesel == payerIdentifier.pesel
                && TaxPayerIdentifier.regon == payerIdentifier.regon
                && TaxPayerIdentifier.other_document == payerIdentifier.other_document;
        }
        public override bool ComparePaymentOfServicesReferenceNumber(string referenceNumber)
        {
            throw new ArgumentException();
        }
    }
}
