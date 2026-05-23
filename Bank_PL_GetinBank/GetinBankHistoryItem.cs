using BankService.Tax.TaxCreditorIdentifiers;
using BankService.Tax.TaxPeriods;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Tools;
using ToolsNugetExtensionHtmlAgilityPack;

namespace BankService.Bank_PL_GetinBank
{
    public class GetinBankHistoryItem : HistoryItem
    {
        public GetinBankHtmlOperationType Type { get; }
        public string MainTitle { get; }
        public string MainTitleSubtitle { get; }
        public GetinBankHtmlOperationStatus Status { get; }
        public DateTime? PostingDate { get; }
        public string ReferenceNumber { get; }
        public GetinBankHtmlCommissionCosts? CommissionCosts { get; }
        public string PaymentSystem { get; }
        public string PaymentRecommendationAccountNumber { get; }
        public string CommissionsChargedAccountNumber { get; }
        public string ExchangeRate { get; }
        public string CardNumber { get; }
        public string OperationTitle { get; }
        public string FromAccountBankName { get; }
        public string ToAccountBankName { get; }
        public string ToAccountBankSwiftCode { get; }

        public override bool IsTransfer => Type == GetinBankHtmlOperationType.Transfer;
        public override bool IsTaxTransfer => throw new NotImplementedException();
        public override bool IsPaymentOfServices => false;
        public override string TransferTypeName => Type.GetEnumDescription();
        public override bool CompareTitle(string title)
        {
            return Title == title;
        }
        public override bool CompareTax(string taxType, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier)
        {
            throw new NotImplementedException();
        }
        public override bool ComparePaymentOfServicesReferenceNumber(string referenceNumber)
        {
            throw new ArgumentException();
        }

        public GetinBankHistoryItem(HtmlNode node)
        {
            //direction = node.ContainsClass("executed") ? TransferDirection.Execute : TransferDirection.Income;
            switch (GetAttribute(node, "data-transfertype"))
            {
                case "out":
                    Direction = OperationDirection.Execute;
                    break;
                case "in":
                    Direction = OperationDirection.Income;
                    break;
                default:
                    throw new NotImplementedException();
            }
            //type = node.ContainsClass("card") ? OperationType.Card : OperationType.Transfer;
            Currency = GetAttribute(node, "data-currency");
            Amount = Double.Parse(GetAttribute(node, "data-amount"), CultureInfo.InvariantCulture);
            HtmlNode nodeMain = node.Descendants("div").Single(n => n.HasClass("item-main"));
            HtmlNode selectItemNode = nodeMain.Descendants("div").Single(n => n.HasClass("chbox--select-item"));
            Id = GetAttribute(selectItemNode.Descendants("input").Single(), "value");
            HtmlNode mainTitleNode = nodeMain.Descendants("dl").Single(n => n.HasClass("main-title"));
            MainTitle = mainTitleNode.Descendants("dd").Single().InnerText;
            MainTitleSubtitle = mainTitleNode.Descendants("dt").Single().InnerText.Trim();
            //balanceAfter = Double.Parse(nodeMain.Descendants("div").Single(n => n.ContainsClass("main-amount")).Descendants("span").Single(n => n.ContainsClass("balance")).InnerText.SubstringFromToEx("saldo po operacji: ", "PLN").TrimEnd());
            HtmlNode nodeDetails = node.Descendants("div").Single(n => n.HasClass("item-details"));
            HtmlNode nodeDetailsInfo = nodeDetails.Descendants("div").Single(n => n.HasClass("details-info"));
            HtmlNode nodeTransferStatus = nodeDetailsInfo.Descendants("div").SingleOrDefault(n => n.HasClass("transfer-status"));
            if (nodeTransferStatus != null)
                Status = AttributeOperations.GetEnumByAttributeNoEmpty<GetinBankHtmlOperationStatus, HtmlLabel, string>(nodeTransferStatus.Descendants("dt").Single().InnerText, (HtmlLabel label) => label.Value);
            OrderDate = DateTime.ParseExact(GetTextFromNodeWithTitle(GetNodeByTitle(nodeDetailsInfo, "data zlecenia")), "dd.MM.yyyy", null);
            string postingDateString = GetTextFromNodeWithTitle(GetNodeByTitle(nodeDetailsInfo, "data księgowania"));
            if (postingDateString != "-")
                PostingDate = DateTime.ParseExact(postingDateString, "dd.MM.yyyy", null);
            string balanceAfterString = GetTextFromNodeWithTitle(GetNodeByTitle(nodeDetailsInfo, "saldo po operacji")).TrimEnd();
            if (balanceAfterString != "-")
                Balance = DoubleOperations.Parse(balanceAfterString.SubstringToEx(Currency).TrimEnd());
            ReferenceNumber = GetTextFromNodeWithTitle(GetNodeByTitle(nodeDetailsInfo, "ref"));
            HtmlNode typeNode = GetNodeByTitle(nodeDetailsInfo, "typ operacji");
            if (typeNode != null)
                Type = AttributeOperations.GetEnumByAttributeNoEmpty<GetinBankHtmlOperationType, HtmlLabel, string>(GetTextFromNodeWithTitle(typeNode), (HtmlLabel label) => label.Value);
            HtmlNode commissionCostsNode = GetNodeByTitle(nodeDetailsInfo, "koszty prowizyjne");
            if (commissionCostsNode != null)
                CommissionCosts = AttributeOperations.GetEnumByAttributeNoEmpty<GetinBankHtmlCommissionCosts, HtmlLabel, string>(GetTextFromNodeWithTitle(commissionCostsNode, true), (HtmlLabel label) => label.Value);
            PaymentSystem = GetTextFromNodeWithTitle(GetNodeByTitle(nodeDetailsInfo, "system płatniczy"));
            PaymentRecommendationAccountNumber = GetTextFromNodeWithTitle(GetNodeByTitle(nodeDetailsInfo, "NALEŻNOŚĆ Z TYTUŁU POLECENIA WYPŁATY"));
            CommissionsChargedAccountNumber = GetTextFromNodeWithTitle(GetNodeByTitle(nodeDetailsInfo, "koszty i prowizje naliczone przez bank"));
            ExchangeRate = GetTextFromNodeWithTitle(GetNodeByTitle(nodeDetailsInfo, "kurs"));
            HtmlNode amountInCurrencyNode = GetNodeByTitle(nodeDetailsInfo, "kwota w walucie rachunku");
            if (amountInCurrencyNode != null)
                AmountInCurrency = DoubleOperations.Parse(GetTextFromNodeWithTitle(amountInCurrencyNode).SubstringToEx(Currency).TrimEnd());
            CardNumber = GetTextFromNodeWithTitle(GetNodeByTitle(nodeDetailsInfo, "numer karty"));
            OperationTitle = GetTextFromNodeWithTitle(GetNodeByTitle(nodeDetailsInfo, "tytuł operacji"));
            HtmlNode nodeDetailsTransfer = nodeDetails.Descendants("div").SingleOrDefault(n => n.HasClass("details-transfer"));
            if (nodeDetailsTransfer != null)
            {
                //TODO toolsNuget.SingleChild
                HtmlNode nodeFromAccount = nodeDetailsTransfer.SingleChildNode("div", n => n.Descendants("span").Single().InnerText == "z rachunku");
                HtmlNode nodeFromAccountSub1 = nodeFromAccount.Descendants("dl").First();
                FromAccountNumber = nodeFromAccountSub1.Descendants("dt").Single().InnerText;
                FromAccountBankName = nodeFromAccountSub1.Descendants("dd").Single().InnerText;
                HtmlNode nodeFromAccountSub2 = nodeFromAccountSub1.NextSibling.NextSibling;
                FromPersonName = nodeFromAccountSub2.Descendants("dt").Single().InnerText;
                FromPersonAddress = nodeFromAccountSub2.Descendants("dd").Single().InnerText;
                HtmlNode nodeToAccount = nodeDetailsTransfer.SingleChildNode("div", n => n.Descendants("span").Single().InnerText == "na rachunek");
                HtmlNode nodeToAccountSub1 = nodeToAccount.Descendants("dl").First();
                ToAccountNumber = nodeToAccountSub1.Descendants("dt").Single().InnerText;
                IEnumerable<HtmlNode> bankNodes = nodeToAccountSub1.Descendants("dd");
                ToAccountBankName = bankNodes.First().InnerText;
                if (bankNodes.Count() > 1)
                    ToAccountBankSwiftCode = bankNodes.ElementAt(1).InnerText;
                HtmlNode nodeToAccountSub2 = nodeToAccountSub1.NextSibling.NextSibling;
                ToPersonName = nodeToAccountSub2.Descendants("dt").Single().InnerText;
                ToPersonAddress = nodeToAccountSub2.Descendants("dd").Single().InnerText;
            }

            Title = OperationTitle ?? MainTitleSubtitle.SubstringFromEx(" - ");
        }

        private static string GetAttribute(HtmlNode node, string attribute)
        {
            return node.GetAttributeValue(attribute, String.Empty);
        }

        private static HtmlNode GetNodeByTitle(HtmlNode parentNode, string title)
        {
            return parentNode.Descendants("dl").SingleOrDefault(n => n.Descendants("dd").Single().InnerText == title);
        }

        private static string GetTextFromNodeWithTitle(HtmlNode node, bool onlyMainText = false)
        {
            if (node == null)
                return null;
            HtmlNode textNode = node.Descendants("dt").Single();
            return !onlyMainText ? textNode.InnerText : textNode.SelectNodes("./text()").Single().InnerText.Trim();
        }

        public bool IsCommisionForTransfer()
        {
            return IsTransfer
                && ToAccountBankName == null && ToAccountBankSwiftCode == null && ToAccountNumber == null && ToPersonAddress == null && ToPersonName == null
                && FromAccountBankName == null && FromAccountNumber == null && FromPersonAddress == null && FromPersonName == null;
        }
    }
}
