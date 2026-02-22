using BankService.BankCountry;
using BankService.ConfirmText;
using BankService.LocalTools;
using BankService.SMSCodes;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Tools;
using Tools.Enums;
using ToolsNugetExtensionHtmlAgilityPack;
using ToolsWeb;
using static BankService.Bank_PT_Santander.SantanderJsonResponse;

namespace BankService.Bank_PT_Santander
{
    [BankTypeAttribute(BankType.Santander)]
    public class Santander : BankPortugal<SantanderAccountData, SantanderHistoryItem, SantanderHistoryFilter, HtmlDocument>
    {
        private Dictionary<string, string> AccountNumberMap;
        private string ogcTOKEN;

        protected override int HeartbeatInterval => 210;
        protected override SMSCodeValidator SMSCodeValidator => new SMSCodeValidatorCypher(6);

        public override bool AllowAlternativeLoginMethod => false;

        public override bool TransferMandatoryRecipient => false;
        public override bool TransferMandatoryTitle => false;
        public override bool PrepaidTransferMandatoryRecipient => false;

        protected override string BaseAddress => "https://www.particulares.santander.pt/";

        protected override void CleanHttpClient()
        {
            AccountNumberMap = null;
            ogcTOKEN = null;
        }

        protected override bool LoginRequest(string login, string password, List<object> additionalAuthorization)
        {
            string ssafe;

            HtmlDocument loginHtmlResponse = PerformHtmlRequest("bepp/sanpt/usuarios/login/0,,,0.shtml", HttpMethod.Get,
                false, false, false,
                null, false);
            ssafe = loginHtmlResponse.Text.SubstringFromToEx("name=\"ssafe\" value=\"", "\"");

            HtmlDocument guardResponse = PerformHtmlRequest("nbp_guard", HttpMethod.Post,
                false, false, true,
                null, false);

            ogcTOKEN = guardResponse.Text.SubstringFromEx("OGC_TOKEN:");

            string loginField;
            string passwordField;

            HtmlDocument loginFormHtmlResponse = PerformHtmlRequest("jsp/sanpt/usuarios/loginForm_novo.jsp", HttpMethod.Get,
                false, false, false,
                null, false);
            passwordField = loginFormHtmlResponse.Text.SubstringFromToEx(".attr(\"for\", '", "')");
            loginField = loginFormHtmlResponse.Text.SubstringFromToEx(".attr(\"for\", '", "')", false, true);

            //TODO "i am not robot" after 3 incorrect password attempts; not required if page refresh

            IEnumerable<KeyValuePair<string, string>> loginParameters = new KeyValuePair<string, string>[] {
                GetTokenParameter(),
                GetActionParameter(SantanderActionType.Login),
                new KeyValuePair<string, string>("ssafe", ssafe),
                new KeyValuePair<string, string>(loginField, login),
                new KeyValuePair<string, string>(passwordField, password),
            };

            HtmlDocument loginResponse = PerformHtmlRequest("bepp/sanpt/usuarios/login", HttpMethod.Post,
                false, false, false,
                loginParameters, false);

            Cookie browserCookie = DomainCookies.GetCookie("^.{40}$", true);
            if (browserCookie != null)
                SaveCookie(browserCookie);

            if (!loginResponse.Text.Contains("window.location.href = '/"))
            {
                HtmlNode codeNode = loginResponse.GetElementbyId("idCodigoAcesso");
                if (codeNode == null)
                    return CheckFailed("Niepoprawny login i hasło");

                //TODO method AuthorizeBrowser, name ConfirmLogin ?

                string code = null;

                if (!SMSConfirm<bool, SantanderJsonResponseConfirmLogin>(
                    (string SMSCode) =>
                    {
                        code = SMSCode;

                        IEnumerable<KeyValuePair<string, string>> confirmLoginParameters1 = new KeyValuePair<string, string>[] {
                            GetActionParameter(SantanderActionType.LoginConfirmAccessCode),
                            new KeyValuePair<string, string>("codigoAcesso", SMSCode),
                        };

                        return PerformRequest<SantanderJsonResponseConfirmLogin>("bepp/sanpt/usuarios/login", HttpMethod.Post,
                            true, true,
                            confirmLoginParameters1, false);
                    },
                    (SantanderJsonResponseConfirmLogin jsonResponseConfirmLogin) =>
                    {
                        //if (!jsonResponseConfirmLogin.sucesso)
                        if (jsonResponseConfirmLogin.SuccessValue != SantanderJsonSuccess.Ok)
                            return false;
                        return true;
                    },
                    (SantanderJsonResponseConfirmLogin jsonResponseConfirmLogin) => true,
                    null,
                    new ConfirmTextAuthorize(null)))
                    return false;

                IEnumerable<KeyValuePair<string, string>> confirmLoginParameters = new KeyValuePair<string, string>[] {
                    GetTokenParameter(),
                    GetActionParameter(SantanderActionType.LoginConfirmAfterAccessCode),
                    new KeyValuePair<string, string>("telSel", String.Empty),
                    new KeyValuePair<string, string>("codigoAcesso", code),
                    new KeyValuePair<string, string>("g-recaptcha-response", String.Empty),
                };
                loginResponse = PerformHtmlRequest("bepp/sanpt/usuarios/login", HttpMethod.Post,
                    false, false, false,
                    confirmLoginParameters, false);
            }

            string redirectUrl = loginResponse.Text.SubstringFromToEx("window.location.href = '/", "';");

            //TODO only headers + in Logout
            PerformHtmlRequest(redirectUrl, HttpMethod.Get,
                false, false, false,
                null, false);

            return true;
        }

        protected override bool LogoutRequest()
        {
            PerformHtmlRequest("bepp/sanpt/usuarios/desconexion/0,,,0.shtml", HttpMethod.Get,
                false, false, false,
                null, false);

            return true;
        }

        protected override bool TryExtendSession()
        {
            string url = WebOperations.BuildUrlWithQuery(BaseAddress, "bepp/sanpt/common/utils",
                new List<(string key, string value)> { ("refreshSession", "yes") });
            SantanderJsonResponseHeartbeat heartbeatResponse = PerformRequest<SantanderJsonResponseHeartbeat>(url, HttpMethod.Get,
                false, false,
                null, false);

            if (heartbeatResponse == null)
                return false;

            return true;
        }

        protected override HtmlDocument GetAccountsDetails()
        {
            //TODO if more accounts, are they all in one place or uncomment below
            HtmlDocument document = PerformHtmlRequest("bepp/sanpt/homepage/homepage/0,,,0.shtml", HttpMethod.Get,
                false, false, false,
                null, false);
            return document;
            //return PerformHtmlRequest("bepp/sanpt/cuentas/detallecuenta/0,,,0.shtml", HttpMethod.Get, false, false, false, null, false);
        }

        private string GetAccountNumberFromShortNumber(string shortAccountNumber)
        {
            if (AccountNumberMap == null)
                AccountNumberMap = new Dictionary<string, string>();
            if (!AccountNumberMap.ContainsKey(shortAccountNumber))
            {
                string url = WebOperations.BuildUrlWithQuery(BaseAddress, "bepp/sanpt/cuentas/detallecuenta/0,,,0.shtml",
                    new List<(string key, string value)> { ("codigoCuenta", shortAccountNumber) });
                HtmlDocument documentAccountDetails = PerformHtmlRequest(url, HttpMethod.Get,
                    false, false, false,
                    null, false);
                AccountNumberMap[shortAccountNumber] = documentAccountDetails.GetElementbyId("iban").InnerText.Trim()/*.Replace(" ", String.Empty)*/;
            }

            return AccountNumberMap[shortAccountNumber];
        }

        protected override List<SantanderAccountData> GetAccountsDataMainMain(HtmlDocument accountsDetails)
        {
            List<SantanderAccountData> result = new List<SantanderAccountData>();

            //TODO if (update)
            HtmlNode accountsBodyNode = accountsDetails.DocumentNode.Descendants().Single(n => n.Attributes["name"]?.Value == "accountsForm").SingleChildNode("section").SingleChildNode("div", n => n.HasClass("table-block")).SingleChildNode("div").SingleChildNode("div", n => n.HasClass("tbody"));
            List<HtmlNode> accountsNodes = accountsBodyNode.Where("a").ToList();
            foreach (HtmlNode accountNode in accountsNodes)
            {
                List<HtmlNode> columnsNodes = accountNode.Where("span").ToList();
                SantanderAccountData accountData = new SantanderAccountData(columnsNodes[1].InnerText, String.Empty, columnsNodes[2].InnerText.Trim().SubstringFromEx(" "), DoubleOperations.Parse(columnsNodes[2].InnerText.Trim().SubstringToEx(" "), ThousandSeparator.Dot, DecimalSeparator.Comma)) { ShortAccountNumber = columnsNodes[0].InnerText };
                accountData.AccountNumber = GetAccountNumberFromShortNumber(accountData.ShortAccountNumber);
                result.Add(accountData);
            }

            return result;
        }

        protected override SantanderHistoryFilter CreateFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact)
        {
            //TODO GetHistoryItems avoids Direction, AmountExact, Title
            return new SantanderHistoryFilter(direction, title, dateFrom, dateTo, amountExact);
        }

        protected override List<SantanderHistoryItem> GetHistoryItems(SantanderHistoryFilter filter = null)
        {
            List<SantanderHistoryItem> result = new List<SantanderHistoryItem>();

            PreRequest("bepp/sanpt/cuentas/accountbalancesandtransactions");

            //TODO page number?

            List<KeyValuePair<string, string>> historyParameters1 = new List<KeyValuePair<string, string>>() {
                GetTokenParameter(),
                GetActionParameter(SantanderActionType.TransactionsHistory),
                new KeyValuePair<string, string>("select_account", SelectedAccountData.ShortAccountNumber),
            };
            DateTime dateTo = filter.DateTo ?? DateTime.Today;
            DateTime dateFrom = filter.DateFrom ?? (dateTo.AddDays(-90));
            if (dateTo.Date == DateTime.Today)
                //eg. if transfer added in saturday then in system it is with monday date
                //TODO instead of 7 i -7 do next and previous working day
                dateTo = DateTime.Today.AddDays(7);
            if ((dateFrom.Date - DateTime.Today).Days >= 0)
                //TODO if run in sunday then needs to be set to thursady?
                dateFrom = DateTime.Today.AddDays(-3);
            historyParameters1.Add(new KeyValuePair<string, string>("fechaInicio", dateFrom.Display("dd-MM-yyyy")));
            historyParameters1.Add(new KeyValuePair<string, string>("fechaFin", (dateTo).Display("dd-MM-yyyy")));
            HtmlDocument document = PerformHtmlRequest("bepp/sanpt/cuentas/accountbalancesandtransactions", HttpMethod.Post,
                false, false, false,
                historyParameters1, false);
            if (!CheckError(document))
                return null;

            HtmlNode sectionNode = document.GetElementbyId("idSectionTable");
            if (sectionNode == null)
            {
                if (!SMSConfirm<bool, SantanderJsonResponseAccountBalancesAndTransactions>(
                    (string SMSCode) =>
                    {
                        List<KeyValuePair<string, string>> historyCodeParameters = new List<KeyValuePair<string, string>>() {
                            GetActionParameter(SantanderActionType.TransactionsHistoryAccessCode),
                            new KeyValuePair<string, string>("codigoAcesso", SMSCode),
                        };

                        return PerformRequest<SantanderJsonResponseAccountBalancesAndTransactions>("bepp/sanpt/cuentas/accountbalancesandtransactions", HttpMethod.Post,
                            true, true,
                            historyCodeParameters, false);
                    },
                    (SantanderJsonResponseAccountBalancesAndTransactions jsonResponse) =>
                    {
                        if (!jsonResponse.sucesso)
                            return null;
                        else
                            return true;
                    },
                    (SantanderJsonResponseAccountBalancesAndTransactions jsonResponse) => true,
                    null,
                    new SantanderConfirmTextAuthorizeGetHistory()))
                    return new List<SantanderHistoryItem>();

                List<KeyValuePair<string, string>> historyParameters2 = new List<KeyValuePair<string, string>>() {
                    GetTokenParameter(),
                    GetActionParameter(SantanderActionType.TransactionsHistoryAfterAccessCode),
                };
                document = PerformHtmlRequest("bepp/sanpt/cuentas/accountbalancesandtransactions", HttpMethod.Post,
                    false, false, false,
                    historyParameters2, false);

                sectionNode = document.GetElementbyId("idSectionTable");
            }

            HtmlNode divTableNode = sectionNode.SingleChildNode("div", n => n.HasClass("table-section-content"));
            if (divTableNode != null)
            {
                List<HtmlNode> transactionsNodes = divTableNode.Descendants("a").Where(n => n.HasClass("movimentoIndividual")).ToList();
                //TODO button for load more (without loading already loaded ones)
                foreach (HtmlNode transactionNode in transactionsNodes)
                {
                    if (filter.CounterLimit == 0 || result.Count < filter.CounterLimit)
                    {
                        string transactionIndex = transactionNode.Attributes["onclick"].Value.SubstringFromToEx("javascript:visualizarDetalhes(this,'", "');");
                        //TODO does index work

                        string url = WebOperations.BuildUrlWithQuery(BaseAddress, "bepp/sanpt/cuentas/accountbalancesandtransactions",
                            new List<(string key, string value)> { GetQueryAction(SantanderActionType.TransactionDetails), ("movIndex", transactionIndex) });
                        HtmlDocument documentDetails = PerformHtmlRequest(url, HttpMethod.Get,
                            false, false, false,
                            null, false);

                        SantanderHistoryItem item = new SantanderHistoryItem(transactionNode, documentDetails);

                        switch (item.Type)
                        {
                            case SantanderTransactionType.Transfer:
                                {
                                    PreRequest("bepp/sanpt/cuentas/historicotransferencias");

                                    List<(string id, string personName, string title, double amount)> transfersDetails = GetTransfersDetails(item);
                                    if (transfersDetails != null)
                                    {
                                        foreach ((string id, string personName, string title, double amount) in transfersDetails)
                                        {
                                            if (StringOperations.IndexOfEx(item.Title, title, true) == 0
                                                && id.EndsWith(item.Title.SubstringFromEx(title + "-", false, true))
                                                && StringOperations.Equals(personName, item.RecipientName, true)
                                                && DoubleOperations.Equals(amount, item.Amount))
                                            {
                                                item.TransferRef = id;

                                                List<KeyValuePair<string, string>> historyDeatilsParameters = new List<KeyValuePair<string, string>>() {
                                                    GetTokenParameter(),
                                                    GetActionParameter(SantanderActionType.TransferDetails),
                                                    new KeyValuePair<string, string>("cuentaOrigen", SelectedAccountData.ShortAccountNumber),
                                                    new KeyValuePair<string, string>("REFERENCIA", id),
                                                };
                                                HtmlDocument documentDetailsTransfer = PerformHtmlRequest("bepp/sanpt/cuentas/historicotransferencias", HttpMethod.Post,
                                                    false, false, false,
                                                    historyDeatilsParameters, false);

                                                HtmlNode sectionNodeTransfer = documentDetailsTransfer.DocumentNode.Descendants("section").Single(n => n.HasClass("section-container"));
                                                IEnumerable<HtmlNode> blockNodes = sectionNodeTransfer.Descendants("div").Where(n => n.HasClass("data-block"));

                                                //if old transfer then doesn't have details?
                                                string accountNumber = GetBlockText(blockNodes, (item.Direction == OperationDirection.Income ? "Conta origem" : "Conta destino"))?.TrimEnd()/*.SubstringFromEx(" - ")*/;

                                                if (item.Direction == OperationDirection.Execute)
                                                    item.ToAccountNumber = accountNumber;
                                                else
                                                    item.FromAccountNumber = accountNumber;

                                                break;
                                            }
                                        }
                                    }
                                }
                                break;
                            default:
                                break;
                        }

                        result.Add(item);
                    }
                    else
                    {
                        result.Add(new SantanderHistoryItem(transactionNode));
                    }
                }
            }

            return result;
        }

        public override bool MakeTransfer(string recipient, string address, string accountNumber, string title, double amount)
        {
            //TODO if recipient name is too long then when sending SMS code there is always error about wrong code (ot there is different error than wrong code)?
            //27 without spaces?
            //if (recipient?.Length > 27)
            //    return CheckFailed("Odbiorca nie może zawierać więcej niż 27 znaków");

            PreRequest("bepp/sanpt/cuentas/transferencianacionalf");

            //TODO fetch account owner based on account number, result to Confirm

            IEnumerable<KeyValuePair<string, string>> transferParameters1 = new KeyValuePair<string, string>[] {
                GetTokenParameter(),
                GetActionParameter(SantanderActionType.TransferInitParameters),
                new KeyValuePair<string, string>("nombreBeneficiario", recipient),
                new KeyValuePair<string, string>("ibanBeneficiario", accountNumber),
                new KeyValuePair<string, string>("montante", amount.Display(DecimalSeparator.Comma)),
                new KeyValuePair<string, string>("divisa", SelectedAccountData.Currency),
                new KeyValuePair<string, string>("referenciaOrdenador", title),
                //TODO add to transfer panel dynamically additional info, email, remove address
                new KeyValuePair<string, string>("informacaoAdicional", ""),
                new KeyValuePair<string, string>("email", ""),
                new KeyValuePair<string, string>("tftype", "NORMAL"),
                new KeyValuePair<string, string>("typeOfTranferChosen", "NORMAL"),
                new KeyValuePair<string, string>("fechaInicio", DateTime.Today.Display("dd-MM-yyyy")),
                new KeyValuePair<string, string>("cuentaOrigen", SelectedAccountData.ShortAccountNumber),
            };
            HtmlDocument transferResponse1 = PerformHtmlRequest("bepp/sanpt/cuentas/transferencianacionalf", HttpMethod.Post,
                false, false, false,
                transferParameters1, false);
            if (!CheckError(transferResponse1))
                return false;

            IEnumerable<KeyValuePair<string, string>> transferParameters2 = new KeyValuePair<string, string>[] {
                GetTokenParameter(),
                GetActionParameter(SantanderActionType.TransferInitType),
                new KeyValuePair<string, string>("typeOfTranferChosen", "NORMAL"),
            };
            HtmlDocument document = PerformHtmlRequest("bepp/sanpt/cuentas/transferencianacionalf", HttpMethod.Post,
                false, false, false,
                transferParameters2, false);
            if (!CheckError(document))
                return false;

            HtmlNode formNode = document.GetElementbyId("formulario");

            HtmlNode sectionNode = formNode.SingleOrDefaultChildNode("section");
            if (sectionNode == null)
                throw new NotSupportedException();

            IEnumerable<HtmlNode> blockNodes = sectionNode.Where("div").Where(n => n.HasClass("data-block"));
            string recipientNameToConfirm = GetBlockText(blockNodes, "Nome do destinatário")?.Trim();
            string ibanToConfirm = GetBlockText(blockNodes, "IBAN").Trim();
            double amountToConfirm = DoubleOperations.Parse(GetBlockText(blockNodes, "Montante").Trim().SubstringToEx(" "), ThousandSeparator.Dot, DecimalSeparator.Comma);
            string currencyToConfirm = GetBlockText(blockNodes, "Montante").Trim().SubstringFromEx(" ");
            string titleToConfirm = GetBlockText(blockNodes, "Descrição")?.Trim();

            if (amountToConfirm != amount || currencyToConfirm != SelectedAccountData.Currency)
                throw new NotSupportedException();
            if (!StringOperations.Equals(titleToConfirm, title, true) && !String.IsNullOrEmpty(title))
                Message($"Changed title: {titleToConfirm}");
            if (!AccountNumberTools.CompareAccountNumbers(ibanToConfirm, accountNumber))
                throw new NotSupportedException();
            if (!StringOperations.Equals(recipientNameToConfirm, recipient, true) && !String.IsNullOrEmpty(recipientNameToConfirm))
                Message($"Changed recipient name: {recipientNameToConfirm}");

            IEnumerable<KeyValuePair<string, string>> transferParameters3 = new KeyValuePair<string, string>[] {
                GetTokenParameter(),
                GetActionParameter(SantanderActionType.TransferProcess),
            };
            HtmlDocument transferResponse3 = PerformHtmlRequest("bepp/sanpt/cuentas/transferencianacionalf", HttpMethod.Post,
                false, false, false,
                transferParameters3, false);

            //TODO is TRANSF_INTERBANCARIAS ok if transfer to the same bank?
            return Confirm(transferResponse3, "bepp/sanpt/cuentas/transferencianacionalf", "TRANSF_INTERBANCARIAS", SantanderActionType.TransferConfirm, "A sua transferência foi registada com sucesso.", new ConfirmTextTransferRecipient(amount, currencyToConfirm, null, ibanToConfirm, recipientNameToConfirm));
        }

        protected override bool MakePrepaidTransfer(string recipient, string phoneNumber, double amount, string nif)
        {
            HtmlDocument document1 = PerformHtmlRequest("bepp/sanpt/pagos/topup", HttpMethod.Get,
                false, false, false,
                null, false);

            string operatorsDataResponseStr = document1.Text.SubstringFromToEx("pagamentosInfo : JSON.parse(JSON.stringify(", ")),");
            List<SantanderJsonResponseOperator> operators = JsonConvert.DeserializeObject<List<SantanderJsonResponseOperator>>(operatorsDataResponseStr);
            //data fix. https://www.vodafone.pt/ajuda/artigos/saldos-carregamentos/gestao-carregamentos/como-efetuo-um-carregamento.html
            operators.First(o => StringOperations.Equals(o.descripcion, "Vodafone", true)).minAmount = "7.50 EUR";

            SantanderJsonResponseOperator operatorItem = PromptComboBox<SantanderJsonResponseOperator>("Operator", operators.Select(o => new SelectComboBoxItem<SantanderJsonResponseOperator>(o.descripcion, o)), false).data;
            if (operatorItem == null)
                return false;

            if ((operatorItem.MinAmountAmount != 0 && amount < operatorItem.MinAmountAmount)
            || (operatorItem.MaxAmountAmount != 0 && amount > operatorItem.MaxAmountAmount))
                return CheckFailed($"Kwota powinna znajdować się w zakresie {operatorItem.MinAmountAmount}-{operatorItem.MaxAmountAmount}");
            if (operatorItem.MinAmountAmount == 0 && operatorItem.MaxAmountAmount == 0
                && !operatorItem.montantesPredefinidos.Select(a => a.Amount).Contains(amount))
                return CheckFailed($"Kwota powinna być jedną z {String.Join(", ", operatorItem.montantesPredefinidos.Select(a => a.Amount.Display(DecimalSeparator.Dot)))}");

            HtmlNode cardsNode = document1.GetElementbyId("idComboCards");
            IEnumerable<(string id, string number)> cards = cardsNode.Where("option").Select(c => (c.GetAttributeValue("value", String.Empty), c.InnerText));

            (string number, string id) card = PromptComboBox<string>("Karta", cards.Select(c => new SelectComboBoxItem<string>(c.number, c.id)), true);

            List<KeyValuePair<string, string>> topupParameters2 = new List<KeyValuePair<string, string>>() {
                GetTokenParameter(),
                GetActionParameter(SantanderActionType.TransferTopupInitParameters),
                new KeyValuePair<string, string>("referenciaPago", phoneNumber.SimplifyPhoneNumber()),
                new KeyValuePair<string, string>("codigoEntidad", operatorItem.identificadorHost),
                new KeyValuePair<string, string>("montante", amount.Display(DecimalSeparator.Comma)),
                new KeyValuePair<string, string>("nif", nif),
                new KeyValuePair<string, string>("fechaPago", DateTime.Today.Display("dd/MM/yyyy")),
                new KeyValuePair<string, string>("fechaInicio", DateTime.Today.Display("dd/MM/yyyy")),
                new KeyValuePair<string, string>("radioCuentas", card.id),
                new KeyValuePair<string, string>("codigoTarjeta", card.id),
                new KeyValuePair<string, string>("contaEfetiva", card.id),
                new KeyValuePair<string, string>("cartaoEfetivo", card.number),
            };
            HtmlDocument document = PerformHtmlRequest("bepp/sanpt/pagos/topup", HttpMethod.Post,
                false, false, false,
                topupParameters2, false);
            if (!CheckError(document))
                return false;

            HtmlNode formNode = document.GetElementbyId("formulario");

            HtmlNode sectionNode = formNode.SingleOrDefaultChildNode("section");
            if (sectionNode == null)
                throw new NotSupportedException();

            IEnumerable<HtmlNode> blockNodes = sectionNode.Descendants("div").Where(n => n.HasClass("data-block"));
            string selectedAccount = GetBlockText(blockNodes, "Conta a debitar");
            string entityToConfirm = GetBlockText(blockNodes, "Entidade");
            string referenceToConfirm = GetBlockText(blockNodes, "Número ou referência");
            double amountToConfirm = DoubleOperations.Parse(GetBlockText(blockNodes, "Montante").Trim().SubstringToEx(" "), ThousandSeparator.Dot, DecimalSeparator.Comma);
            string currencyToConfirm = GetBlockText(blockNodes, "Montante").Trim().SubstringFromEx(" ").TrimStart();

            if (selectedAccount != SelectedAccountData.ShortAccountNumber)
                throw new NotSupportedException();
            if (entityToConfirm != operatorItem.descripcion)
                throw new NotSupportedException();
            if (referenceToConfirm != phoneNumber)
                throw new NotSupportedException();
            if (amountToConfirm != amount || currencyToConfirm != SelectedAccountData.Currency)
                throw new NotSupportedException();

            List<KeyValuePair<string, string>> topupParameters3 = new List<KeyValuePair<string, string>>() {
                GetTokenParameter(),
                GetActionParameter(SantanderActionType.TransferTopup),
            };
            HtmlDocument topupResponse3 = PerformHtmlRequest("bepp/sanpt/pagos/topup", HttpMethod.Post,
                false, false, false,
                topupParameters3, false);

            return Confirm(topupResponse3, "bepp/sanpt/pagos/topup", "CARREGAMENTO_TELEMOVEL", SantanderActionType.TransferTopupConfirm, $"O seu carregamento {operatorItem.descripcion} foi efetuado", new ConfirmTextPrepaidTransfer(amount, currencyToConfirm, operatorItem.descripcion, referenceToConfirm));
        }

        public override bool MakePaymentOfServicesTransfer(string entity, string reference, double amount)
        {
            //amount must be exact, otherwise response with error

            HtmlDocument document1 = PreRequest("bepp/sanpt/pagos/pagorecibos");

            HtmlNode cardsNode = document1.GetElementbyId("idComboCards");
            IEnumerable<(string id, string number)> cards = cardsNode.Where("option").Select(c => (c.GetAttributeValue("value", String.Empty), c.InnerText));

            (string number, string id) card = PromptComboBox<string>("Karta", cards.Select(c => new SelectComboBoxItem<string>(c.number, c.id)), true);

            List<KeyValuePair<string, string>> paymentParameters1 = new List<KeyValuePair<string, string>>() {
                GetTokenParameter(),
                GetActionParameter(SantanderActionType.TransferInitParameters),
                new KeyValuePair<string, string>("codigoEntidad", entity),
                new KeyValuePair<string, string>("referenciaPago", reference),
                new KeyValuePair<string, string>("montante", amount.Display(DecimalSeparator.Comma)),
                //TODO what if weekend
                new KeyValuePair<string, string>("paymentDay", DateTime.Today.Display("dd-MM-yyyy")),
                new KeyValuePair<string, string>("fechaPago", DateTime.Today.Display("dd/MM/yyyy")),
                new KeyValuePair<string, string>("divisaMontante", SelectedAccountData.Currency),
                new KeyValuePair<string, string>("cuentaEfectivo", SelectedAccountData.ShortAccountNumber),
                new KeyValuePair<string, string>("codigoTarjeta", card.id),
                new KeyValuePair<string, string>("contaEfetiva", card.id),
                new KeyValuePair<string, string>("cartaoEfetivo", card.number),
            };
            HtmlDocument document2 = PerformHtmlRequest("bepp/sanpt/pagos/pagorecibos", HttpMethod.Post,
                false, false, false,
                paymentParameters1, false);
            if (!CheckError(document2))
                return false;

            HtmlNode formNode = document2.GetElementbyId("formulario");

            HtmlNode sectionNode = formNode.SingleOrDefaultChildNode("section");
            if (sectionNode == null)
                throw new NotSupportedException();

            IEnumerable<HtmlNode> blockNodesSpec = document2.GetElementbyId("specv").SingleChildNode("section").SelectMany("div", null, "div", n => n.HasClass("data-block"));
            string selectedAccount = GetBlockText(blockNodesSpec, "Conta a debitar");

            IEnumerable<HtmlNode> blockNodes = sectionNode.Where("div").Where(n => n.HasClass("data-block"));
            string entityToConfirm = GetBlockText(blockNodes, "Entidade");
            string referenceToConfirm = GetBlockText(blockNodes, "Referência");
            double amountToConfirm = DoubleOperations.Parse(GetBlockText(blockNodes, "Montante").Trim().SubstringToEx(" "), ThousandSeparator.Dot, DecimalSeparator.Comma);
            string currencyToConfirm = GetBlockText(blockNodes, "Montante").Trim().SubstringFromEx(" ");

            //string entityNumberToConfirm = entityToConfirm.SubstringToEx(" - ");
            //string entityNameToConfirm = entityToConfirm.SubstringFromEx(" - ");
            string entityNumberToConfirm = entityToConfirm;
            string entityNameToConfirm = GetBlockText(blockNodes, "Destinatário");

            if (selectedAccount != SelectedAccountData.ShortAccountNumber)
                throw new NotSupportedException();
            if (entityNumberToConfirm != entity)
                throw new NotSupportedException();
            if (referenceToConfirm.Replace(" ", String.Empty) != reference.Replace(" ", String.Empty))
                throw new NotSupportedException();
            if (amountToConfirm != amount || currencyToConfirm != SelectedAccountData.Currency)
                throw new NotSupportedException();

            List<KeyValuePair<string, string>> paymentParameters2 = new List<KeyValuePair<string, string>>() {
                GetTokenParameter(),
                GetActionParameter(SantanderActionType.TransferPaymentOfServicesProcess),
            };
            HtmlDocument paymentResponse2 = PerformHtmlRequest("bepp/sanpt/pagos/pagorecibos", HttpMethod.Post,
                false, false, false,
                paymentParameters2, false);

            //TODO create class ConfirmTextPaymentOfServicesTransfer
            return Confirm(paymentResponse2, "bepp/sanpt/pagos/pagorecibos", "PAGAMENTO_SERVICOS", SantanderActionType.TransferPaymentOfServicesConfirm, "Para ver informação detalhada deste pagamento e outros consulte o histórico de pagamentos.", new ConfirmTextPaymentOfServices(amount, currencyToConfirm, entityNameToConfirm, entity, reference));
        }

        //TODO make user interface
        public bool MakePayToTheStateTransfer(string reference, double amount, string nif)
        {
            HtmlDocument document1 = PreRequest("bepp/sanpt/pagos/pagoestado");

            HtmlNode formNode = document1.GetElementbyId("formulario");
            string entityCode = formNode.SingleChildNode("input", n => n.Attributes["name"].Value == "codigoEntidad").GetAttributeValue("value", String.Empty);

            HtmlNode cardsNode = document1.GetElementbyId("idComboCards");
            IEnumerable<(string id, string number)> cards = cardsNode.Where("option").Select(c => (c.GetAttributeValue("value", String.Empty), c.InnerText));

            (string number, string id) card = PromptComboBox<string>("Karta", cards.Select(c => new SelectComboBoxItem<string>(c.number, c.id)), true);

            List<KeyValuePair<string, string>> paymentParameters1 = new List<KeyValuePair<string, string>>() {
                GetTokenParameter(),
                GetActionParameter(SantanderActionType.TransferPayToTheStateInitParameters),
                new KeyValuePair<string, string>("codigoReciboEstado", reference),
                new KeyValuePair<string, string>("importe", amount.Display(DecimalSeparator.Comma)),
                new KeyValuePair<string, string>("repartoFinanzas", nif),
                new KeyValuePair<string, string>("fechaPagoDia", DateTime.Today.Display("dd-MM-yyyy")),
                new KeyValuePair<string, string>("nuevo", String.Empty),
                new KeyValuePair<string, string>("codigoTarjeta", card.id),
                new KeyValuePair<string, string>("contaSelecionada", String.Empty),
                new KeyValuePair<string, string>("codigoEntidad", entityCode),
                new KeyValuePair<string, string>("tipoDocumento", "1"),
                new KeyValuePair<string, string>("isAgendamento", "false"),
                new KeyValuePair<string, string>("fechaPago", DateTime.Today.Display("dd/MM/yyyy")),
                new KeyValuePair<string, string>("bancoOrigem", String.Empty),
                new KeyValuePair<string, string>("contaEfetiva", card.id),
                new KeyValuePair<string, string>("cartaoEfetivo", card.number),
                new KeyValuePair<string, string>("cartaoEfetivoAlternativo", String.Empty),
            };
            HtmlDocument document2 = PerformHtmlRequest("bepp/sanpt/pagos/pagoestado", HttpMethod.Post,
                false, false, false,
                paymentParameters1, false);
            if (!CheckError(document2))
                return false;

            return true;
        }

        protected override bool GetDetailsFileMain(SantanderHistoryItem item, Func<ContentDispositionHeaderValue, FileStream> file)
        {
            switch (item.Type)
            {
                case SantanderTransactionType.Transfer:
                    {
                        //transfers - not transactions
                        PreRequest("bepp/sanpt/cuentas/historicotransferencias");

                        List<(string id, string personName, string title, double amount)> transfersDetails = GetTransfersDetails(item);
                        if (transfersDetails != null)
                        {
                            foreach ((string id, string personName, string title, double amount) in transfersDetails)
                            {
                                if (id == item.TransferRef)
                                {
                                    List<KeyValuePair<string, string>> historyPdfParameters = new List<KeyValuePair<string, string>>() {
                                        GetTokenParameter(),
                                        GetActionParameter(SantanderActionType.TransferDocument),
                                        new KeyValuePair<string, string>("formatoDescarga", "pdf"),
                                    };
                                    string url = WebOperations.BuildUrlWithQuery(BaseAddress, "bepp/sanpt/cuentas/historicotransferencias/0,,,00.pdf",
                                        new List<(string key, string value)> { ("appInterceptor", "pdf"), ("pdfmethod", "get") });
                                    PerformFileRequest(url, HttpMethod.Post,
                                        historyPdfParameters,
                                        file);
                                    return true;
                                }
                            }
                        }
                        return CheckFailed("Brak przelewu na liście przelewów");
                    }
                    break;
                case SantanderTransactionType.PaymentOfServices when item.Card == null:
                    {
                        //TODO does it work if more than one row

                        PreRequest("bepp/sanpt/pagos/consultaordenespago");

                        //TODO page number
                        string url1 = WebOperations.BuildUrlWithQuery(BaseAddress, "bepp/sanpt/pagos/consultaordenespago",
                            new List<(string key, string value)> {
                                GetQueryAction(SantanderActionType.TransfersPaymentOfServicesHistory),
                                ("account", SelectedAccountData.ShortAccountNumber), 
                                //("montante", String.Empty), 
                                ("montante", item.Amount.Display(DecimalSeparator.Comma)),
                                ("dateInterval", "pt_consultaordenespago_option_periodo_intervalo"),
                                ("fechaInicio", (item.OrderDate.AddDays(-7)).Display("dd/MM/yyyy")),
                                ("fechaFim", (item.OrderDate.AddDays(7)).Display("dd/MM/yyyy")) });

                        HtmlDocument document = PerformHtmlRequest(url1, HttpMethod.Post,
                            true, true, false,
                            null, true);

                        List<HtmlNode> transfersNodes = document.DocumentNode.Where("a").ToList();
                        foreach (HtmlNode transferNode in transfersNodes)
                        {
                            List<HtmlNode> columnsNodes = transferNode.Where("span").ToList();
                            string entityName = columnsNodes[1].InnerText.TrimEnd();
                            double amount = Math.Abs(DoubleOperations.Parse(columnsNodes[3].InnerText.Trim().SubstringToEx(" "), ThousandSeparator.Dot, DecimalSeparator.Comma));
                            string currency = columnsNodes[3].InnerText.Trim().SubstringFromEx(" ");
                            string id = transferNode.Attributes["href"].Value.SubstringFromToEx("javascript:detail('", "');").SubstringToEx("_");

                            if (/*StringOperations.Equals(entityName, item.ToPersonName, true) &&*/ DoubleOperations.Equals(amount, item.Amount) && currency == item.Currency)
                            {
                                List<KeyValuePair<string, string>> historyDeatilsParameters = new List<KeyValuePair<string, string>>() {
                                    GetTokenParameter(),
                                    GetActionParameter(SantanderActionType.TransferPaymentOfServicesDetails),
                                    new KeyValuePair<string, string>("cuentaCargo", SelectedAccountData.ShortAccountNumber),
                                    new KeyValuePair<string, string>("identificadorOrdenPago", id),
                                    new KeyValuePair<string, string>("idoper", id),
                                    new KeyValuePair<string, string>("hasComprovativo", "true"),
                                };
                                HtmlDocument documentDetails = PerformHtmlRequest("bepp/sanpt/pagos/consultaordenespago", HttpMethod.Post,
                                    false, false, false,
                                    historyDeatilsParameters, false);

                                HtmlNode sectionNode = documentDetails.DocumentNode.Descendants("section").Single(n => n.HasClass("section-container"));
                                IEnumerable<HtmlNode> blockNodes = sectionNode.Descendants("div").Where(n => n.HasClass("data-block"));

                                string entityNumber = GetBlockText(blockNodes, "Entidade");
                                string referenceNumber = GetBlockText(blockNodes, "Ref./Nº Beneficiário");

                                if (entityNumber == item.PaymentOfServicesEntityNumber && referenceNumber.Replace(" ", String.Empty) == item.PaymentOfServicesReferenceNumber)
                                {
                                    List<KeyValuePair<string, string>> historyPdfParameters = new List<KeyValuePair<string, string>>() {
                                        GetTokenParameter(),
                                        GetActionParameter(SantanderActionType.TransferPaymentOfServicesDocument),
                                        new KeyValuePair<string, string>("formatoDescarga", "pdf"),
                                        new KeyValuePair<string, string>("idoper", id),
                                        new KeyValuePair<string, string>("hasComprovativo", "true"),
                                        new KeyValuePair<string, string>("cuentaCargo", SelectedAccountData.ShortAccountNumber),
                                    };
                                    string url2 = WebOperations.BuildUrlWithQuery(BaseAddress, "bepp/sanpt/pagos/consultaordenespago/0,,,00.pdf",
                                        new List<(string key, string value)> { ("appInterceptor", "pdf"), ("pdfmethod", "get") });
                                    PerformFileRequest(url2, HttpMethod.Post,
                                        historyPdfParameters,
                                        file);
                                    return true;
                                }
                            }
                        }
                    }
                    break;
                default:
                    return CheckFailed("Operacja dozwolona tylko dla przelewów");
            }

            return false;
        }

        private List<(string id, string personName, string title, double amount)> GetTransfersDetails(SantanderHistoryItem item)
        {
            //TODO page number

            List<KeyValuePair<string, string>> historyParameters = new List<KeyValuePair<string, string>>() {
                GetTokenParameter(),
                GetActionParameter(SantanderActionType.TransfersHistory),
                new KeyValuePair<string, string>("cuentaOrigen", SelectedAccountData.ShortAccountNumber),
                new KeyValuePair<string, string>("dateInterval", "pt_historicotransferencias_option_periodo_intervalo"),
                new KeyValuePair<string, string>("fechaInicio", (item.OrderDate.AddDays(-7)).Display("dd-MM-yyyy")),
                new KeyValuePair<string, string>("fechaFim", (item.OrderDate.AddDays(7)).Display("dd-MM-yyyy")),
                new KeyValuePair<string, string>("numeroPagina", "1"),
                new KeyValuePair<string, string>("search", "si"),
                new KeyValuePair<string, string>("TIPO_TRANSFERENCIA", (item.Direction == OperationDirection.Income ? "R" : "E")),
            };

            HtmlDocument document = PerformHtmlRequest("bepp/sanpt/cuentas/historicotransferencias", HttpMethod.Post,
                false, false, false,
                historyParameters, false);

            HtmlNode divTableNodeTransfer = document.GetElementbyId("resultadosTable");
            if (divTableNodeTransfer == null)
                return null;

            List<(string id, string personName, string title, double amount)> result = new List<(string id, string personName, string title, double amount)>();

            List<HtmlNode> transfersNodes = divTableNodeTransfer.Where("a").ToList();
            foreach (HtmlNode transferNode in transfersNodes)
            {
                List<HtmlNode> columnsNodes = transferNode.Where("span").ToList();
                string personName = columnsNodes[1].InnerText;
                string title = columnsNodes[2].InnerText;
                double amount = Math.Abs(DoubleOperations.Parse(String.Join(String.Empty, columnsNodes[4].InnerText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).Where(t => t != String.Empty)), ThousandSeparator.Dot, DecimalSeparator.Comma));
                string id = transferNode.Attributes["href"].Value.SubstringFromToEx("javascript:detail('", "');");

                result.Add((id, personName, title, amount));
            }

            return result;
        }

        private bool Confirm(HtmlDocument document, string url, string funcionType, SantanderActionType actionType, string successMessage, ConfirmTextBase confirmText)
        {
            HtmlNode codeNode = document.GetElementbyId("claveOperativa");
            string givenCode = codeNode.Attributes["value"]?.Value;

            if (givenCode != null)
            {
                if (!ConfirmWithoutFactor(confirmText))
                    return false;

                (bool success, string transferMessage, string errorMessage) = ConfirmTransfer(givenCode, url, actionType, successMessage);

                if (!success)
                    return CheckFailed(transferMessage ?? errorMessage);
                return true;
            }
            else
            {
                string url1 = WebOperations.BuildUrlWithQuery(BaseAddress, "bepp/sanpt/usuarios/autenticacaofortefunctions",
                    new List<(string key, string value)> { GetQueryAction(SantanderActionType.TransferAuthenticateSMS), ("funcionalidade", funcionType) });
                SantanderJsonResponseAutenticacaOforteFunctions jsonResponse = PerformRequest<SantanderJsonResponseAutenticacaOforteFunctions>(url1, HttpMethod.Post,
                    true, true,
                    null, true);
                if (jsonResponse.MessageValue != SantanderJsonResponseStatus.Success)
                    return CheckFailed(jsonResponse.msg);

                return SMSConfirm<bool, (bool success, string transferMessage, string errorMessage)>(
                    (string SMSCode) =>
                    {
                        return ConfirmTransfer(SMSCode, url, actionType, successMessage);
                    },
                    ((bool success, string transferMessage, string errorMessage) confirmation) =>
                    {
                        if (confirmation.transferMessage == null && confirmation.errorMessage == null)
                            return false;
                        if (confirmation.transferMessage != null && !confirmation.success)
                            return CheckFailed(confirmation.transferMessage);
                        if (confirmation.transferMessage == null)
                            return null;
                        else
                            return true;
                    },
                    ((bool success, string transferMessage, string errorMessage) confirmation) => true,
                    null,
                    confirmText);
            }
        }

        private (bool success, string transferMessage, string errorMessage) ConfirmTransfer(string code, string url, SantanderActionType actionType, string successMessage)
        {
            IEnumerable<KeyValuePair<string, string>> transferParameters = new KeyValuePair<string, string>[] {
                GetTokenParameter(),
                GetActionParameter(actionType),
                new KeyValuePair<string, string>("claveOperativa", code)
            };
            HtmlDocument document = PerformHtmlRequest(url, HttpMethod.Post,
                false, false, false,
                transferParameters, false);

            HtmlNode formNode = document.GetElementbyId("formulario");

            HtmlNode messageNode = formNode.SingleChildNode("section").SingleOrDefaultChildNode("div", n => n.HasClass("form-message"));
            string transferMessage = messageNode?.SingleChildNode("p").InnerText.Trim();

            //TODO show error, eg. account blocked, not only "błędny SMS"

            return (transferMessage == successMessage, transferMessage, GetErrorMessage(document));
        }

        private T GetRequest<T>(
            HttpRequestMessage request,
            Func<string, T> responseStrAction) where T : class
        {
            using (HttpResponseMessage response = HttpOperations.GetResponse(Client, request))
            {
                //needed if cookies from domain .santander.pt
                Handler.SetCookies(response);

                return ProcessResponse<T>(response,
                    (string responseStr) =>
                    {
                        return true;
                    },
                    responseStrAction).Item1;
            }
        }

        private T PerformRequest<T>(string requestUri, HttpMethod method,
            bool setXRequestedWith, bool setHeaderToken,
            IEnumerable<KeyValuePair<string, string>> parameters, bool forceEmptyContent) where T : SantanderJsonResponseBase
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, setXRequestedWith, setHeaderToken, false, parameters, forceEmptyContent))
            {
                T response = GetRequest<T>(request, (responseStr) => JsonConvert.DeserializeObject<T>(responseStr));

                return response;
            }
        }

        private HtmlDocument PerformHtmlRequest(string requestUri, HttpMethod method,
            bool setXRequestedWith, bool setHeaderToken, bool setFetchCsrfToken,
            IEnumerable<KeyValuePair<string, string>> parameters, bool forceEmptyContent)
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, setXRequestedWith, setHeaderToken, setFetchCsrfToken, parameters, forceEmptyContent))
                return GetRequest<HtmlDocument>(request, (responseStr) =>
                {
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(responseStr);
                    return document;
                });
        }

        private HtmlDocument PreRequest(string requestUri)
        {
            IEnumerable<KeyValuePair<string, string>> parameters = new KeyValuePair<string, string>[] {
                GetTokenParameter(),
            };
            return PerformHtmlRequest(requestUri, HttpMethod.Post,
                false, false, false,
                parameters, false);
        }

        private void PerformFileRequest(string requestUri, HttpMethod method,
            IEnumerable<KeyValuePair<string, string>> parameters,
            Func<ContentDispositionHeaderValue, FileStream> fileStream)
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, false, false, false, parameters, false))
                ProcessFileStream(request, fileStream);
        }

        private HttpRequestMessage CreateHttpRequestMessage(string requestUri, HttpMethod method, bool setXRequestedWith, bool setHeaderToken, bool setFetchCsrfToken, IEnumerable<KeyValuePair<string, string>> parameters, bool forceEmptyContent)
        {
            List<(string name, string value)> headers = new List<(string name, string value)>();
            headers.Add(("User-Agent", Constants.AppBrowserName));
            headers.Add(("Accept", "*/*"));
            if (setXRequestedWith)
                headers.Add(("X-Requested-With", "XMLHttpRequest"));
            if (setHeaderToken)
                headers.Add(("OGC_TOKEN", ogcTOKEN));
            if (setFetchCsrfToken)
                headers.Add(("FETCH-CSRF-TOKEN", "1"));

            return forceEmptyContent ? HttpOperations.CreateHttpRequestMessageEmpty(method, requestUri, headers) : HttpOperations.CreateHttpRequestMessageForm(method, requestUri, parameters, headers);
        }

        //TODO check it everywhere
        private bool CheckError(HtmlDocument document)
        {
            HtmlNode errorNode = document.DocumentNode.Descendants("div").SingleOrDefault(n => n.HasClass("global-message-error"));
            if (errorNode != null && errorNode.Any("p"))
            {
                string errorMessage = String.Join(Environment.NewLine, errorNode.Where("p").Select(n => n.InnerText));
                if (errorMessage != null)
                {
                    Message(errorMessage);
                    return false;
                }
            }
            return true;
        }

        private string GetErrorMessage(HtmlDocument document)
        {
            string errorMessage = null;
            HtmlNode errorNode = document.DocumentNode.Descendants("div").SingleOrDefault(n => n.HasClass("global-message-error"));
            if (errorNode != null && errorNode.Any("p"))
                errorMessage = String.Join(Environment.NewLine, errorNode.Where("p").Select(n => n.InnerText));
            return errorMessage;
        }

        private string GetActionNumber(SantanderActionType action)
        {
            return AttributeOperations.GetEnumAttribute(action, (SantanderActionNumberAttribute actionNumber) => actionNumber.Number.ToString());
        }

        private (string key, string value) GetQueryAction(SantanderActionType action)
        {
            return ("accion", GetActionNumber(action));
        }

        private KeyValuePair<string, string> GetActionParameter(SantanderActionType action)
        {
            return new KeyValuePair<string, string>("accion", GetActionNumber(action));
        }

        private KeyValuePair<string, string> GetTokenParameter()
        {
            return new KeyValuePair<string, string>("OGC_TOKEN", ogcTOKEN);
        }

        public static string GetBlockText(IEnumerable<HtmlNode> blockNodes, string label)
        {
            HtmlNode blockNode = blockNodes.SingleOrDefault(n =>
                label == null
                || n.SingleOrDefaultChildNode("label")?.InnerText == label
                || n.SingleOrDefaultChildNode("p", p => p.HasClass("data-label"))?.InnerText == label);
            return blockNode != null ? blockNode.SingleChildNode("p", p => p.HasClass("data-value") || p.HasClass("field-data-value")).InnerText : null;
        }
    }
}
