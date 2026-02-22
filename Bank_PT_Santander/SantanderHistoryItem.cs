using BankService.Tax.TaxCreditorIdentifiers;
using BankService.Tax.TaxPeriods;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Tools.Enums;
using ToolsNugetExtensionHtmlAgilityPack;

namespace BankService.Bank_PT_Santander
{
    public class SantanderHistoryItem : HistoryItem
    {
        public SantanderTransactionType Type { get; protected set; }
        public string DescriptionId { get; protected set; }
        public string Card { get; protected set; }
        public string TransferRef { get; set; }

        public SantanderHistoryItem(HtmlNode transferNode)
        {
            HtmlNode currencyNode = transferNode.Descendants("span").Single(n => n.HasClass("chart-currency"));
            Currency = currencyNode.InnerText.Trim();
            double amount = DoubleOperations.Parse(currencyNode.ParentNode.InnerText.Replace(Currency, String.Empty).Trim(), ThousandSeparator.Dot, DecimalSeparator.Comma);
            Amount = Math.Abs(amount);
            Direction = amount > 0 ? OperationDirection.Income : OperationDirection.Execute;

            Title = Santander.GetBlockText(transferNode.Descendants("div").Where(n => n.HasClass("data-block")), null);

            Type = SantanderTransactionType.Empty;
        }

        public SantanderHistoryItem(HtmlNode transferNode, HtmlAgilityPack.HtmlDocument documentDetails) : this(transferNode)
        {
            IEnumerable<HtmlNode> blockNodes = documentDetails.DocumentNode.Descendants("div").Where(n => n.HasClass("data-block"));
            string transactionType = Santander.GetBlockText(blockNodes, "Tipo de movimento");
            string operationDate = Santander.GetBlockText(blockNodes, "Data de operação");
            string valueDate = Santander.GetBlockText(blockNodes, "Data valor");
            string purchaseDate = Santander.GetBlockText(blockNodes, "Data de compra");
            string balance = Santander.GetBlockText(blockNodes, "Saldo contabilístico");
            string description = Santander.GetBlockText(blockNodes, "Descritivo do movimento");
            string recipientIban = Santander.GetBlockText(blockNodes, "IBAN de destinatário");
            string recipient = Santander.GetBlockText(blockNodes, "Destinatário");
            string title = Santander.GetBlockText(blockNodes, "Descritivo de transferência");
            string additionalInfo = Santander.GetBlockText(blockNodes, "Informação adicional");
            string transferType = Santander.GetBlockText(blockNodes, "Tipo de transferência");
            string senderIban = Santander.GetBlockText(blockNodes, "IBAN de ordenante");
            string sender = Santander.GetBlockText(blockNodes, "Ordenante");
            string transferOrigin = Santander.GetBlockText(blockNodes, "Origem de transferência");
            string transferRef = Santander.GetBlockText(blockNodes, "Referência de transferência");
            Card = Santander.GetBlockText(blockNodes, "Cartão");
            string storeLocation = Santander.GetBlockText(blockNodes, "Localidade de comerciante");
            string purchaseType = Santander.GetBlockText(blockNodes, "Tipo de compra");

            //Id=
            //AmountInCurrency=
            Balance = DoubleOperations.Parse(balance.Trim().SubstringToEx(" "), ThousandSeparator.Dot, DecimalSeparator.Comma);
            OrderDate = DateTime.Parse(operationDate);
            FromAccountNumber = senderIban;
            FromPersonName = sender;
            //FromPersonAddress=
            ToAccountNumber = recipientIban;
            ToPersonName = recipient;
            //ToPersonAddress=
            if (title != null || description != null)
                Title = title ?? description;

            if (description != null && title != null)
                DescriptionId = description.SubstringFromEx(title + "-");
            TransferRef = transferRef;

            Type = AttributeOperations.GetEnumByAttributeNoEmpty<SantanderTransactionType, HtmlLabel, string>(transactionType, (HtmlLabel label) => label.Value);

            if (Type == SantanderTransactionType.PaymentOfServices)
            {
                ToPersonName = transferNode.Descendants("p").Single().InnerText.SubstringFromEx("Pagamento ");
                PaymentOfServicesEntityNumber = Type != SantanderTransactionType.PaymentOfServices ? null : Title.SubstringFromToEx("Pag Servicos ", "-");
                if (PaymentOfServicesEntityNumber.Contains(" "))
                    PaymentOfServicesEntityNumber = PaymentOfServicesEntityNumber.SubstringFromEx(" ");
                PaymentOfServicesReferenceNumber = Type != SantanderTransactionType.PaymentOfServices ? null : Title.SubstringFromEx("-").SubstringToEx(" ");
            }
        }

        public override bool IsTransfer => Type == SantanderTransactionType.Transfer;
        //TODO certainly there are no tax transfers?
        public override bool IsTaxTransfer => false;
        public override bool IsPaymentOfServices => Type == SantanderTransactionType.PaymentOfServices;
        public override string TransferTypeName => Type.GetEnumDescription();
        public override bool CompareTitle(string title)
        {
            return Title == title;
        }
        public override bool CompareTax(string taxType, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier)
        {
            throw new ArgumentException();
        }
        public override bool ComparePaymentOfServicesReferenceNumber(string referenceNumber)
        {
            return PaymentOfServicesReferenceNumber == referenceNumber.Replace(" ", String.Empty);
        }
    }
}
