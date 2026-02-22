using BankService.BankCountry;
using BankService.ConfirmText;
using BankService.LocalTools;
using BankService.SMSCodes;
using BankService.Tax.TaxCreditorIdentifiers;
using BankService.Tax.TaxPeriods;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Tools;
using Tools.Enums;
using static BankService.Bank_PL_VeloBank.VeloBankJsonRequest;
using static BankService.Bank_PL_VeloBank.VeloBankJsonResponse;

namespace BankService.Bank_PL_VeloBank
{
    [BankTypeAttribute(BankType.VeloBank)]
    public class VeloBank : BankPoland<VeloBankAccountData, VeloBankHistoryItem, VeloBankHistoryFilter, VeloBankJsonResponseAccounts>
    {
        private string defaultContextHash { get; set; }

        protected override int HeartbeatInterval => 180;
        protected override SMSCodeValidator SMSCodeValidator => new SMSCodeValidatorCypher(6);

        public override bool AllowAlternativeLoginMethod => true;

        public override bool TransferMandatoryRecipient => true;
        public override bool TransferMandatoryTitle => true;
        public override bool PrepaidTransferMandatoryRecipient => true;

        protected override string BaseAddress => "https://secure.velobank.pl/api/v004/";

        protected override void CleanHttpClient()
        {
            Client.DefaultRequestHeaders.Authorization = null;
            defaultContextHash = null;
        }

        protected override bool LoginRequest(string login, string password, List<object> additionalAuthorization)
        {
            return LoginRequest(login, password, null);
        }

        private bool LoginRequest(string login, string password, string transferId)
        {
            VeloJsonRequestLogin jsonRequestLogin = new VeloJsonRequestLogin();
            jsonRequestLogin.login = login;
            VeloJsonResponseLoginLogin loginLoginResponse = PerformRequest<VeloJsonResponseLoginLogin>("Users/passwordType", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestLogin));

            if (loginLoginResponse.CheckErrorExists(10036))
                return CheckFailed("Odblokuj dostęp na stronie internetowej");

            bool isPasswordMasked = !loginLoginResponse.is_password_plain;

            VeloJsonRequestLoginPassword jsonRequestLoginPassword;

            if (transferId == null)
            {
                jsonRequestLoginPassword = new VeloJsonRequestLoginPassword();
                jsonRequestLoginPassword.ModuleValue = VeloBankJsonModuleType.Banking;
            }
            else
            {
                (FastTransferType? type, string pblData, (string key, string hash) paData) fastTransferData = GetDataFromFastTransfer(transferId);
                if (fastTransferData.type == FastTransferType.PA)
                {
                    jsonRequestLoginPassword = new VeloJsonRequestLoginPasswordFastTransferPA();
                    jsonRequestLoginPassword.ModuleValue = VeloBankJsonModuleType.FastTransferPA;
                    ((VeloJsonRequestLoginPasswordFastTransferPA)jsonRequestLoginPassword).consent_request_data = new VeloJsonRequestLoginPasswordConsentRequestData() { authorize_request_key = fastTransferData.paData.key, hash = fastTransferData.paData.hash };
                }
                else if (fastTransferData.type == FastTransferType.PayByLink)
                {
                    jsonRequestLoginPassword = new VeloJsonRequestLoginPassword();
                    jsonRequestLoginPassword.ModuleValue = VeloBankJsonModuleType.FastTransferPBL;
                }
                else
                    throw new NotImplementedException();
            }

            jsonRequestLoginPassword.login = login;
            jsonRequestLoginPassword.method = "PASSWORD";
            if (isPasswordMasked)
            {
                List<int> mask = loginLoginResponse.password_combinations.Split(new string[] { "," }, StringSplitOptions.None).Select(s => Int32.Parse(s)).ToList();

                if (mask.Max() > password.Length - 1)
                    return CheckFailed("Niepoprawne hasło");

                StringBuilder maskedPassword = new StringBuilder();
                foreach (int i in mask)
                    maskedPassword.Append(password[i]);
                jsonRequestLoginPassword.password = maskedPassword.ToString();
            }
            else
                jsonRequestLoginPassword.password = password;

            return CreateSession(jsonRequestLoginPassword);
        }

        public override bool PerformLoginAlternative()
        {
            string code = Sha256(Sha256(Guid.NewGuid().ToString("D")));

            VeloJsonRequestLoginQRGenerate jsonRequestLoginQRGenerate = new VeloJsonRequestLoginQRGenerate();
            jsonRequestLoginQRGenerate.code_challenge = code;
            jsonRequestLoginQRGenerate.type = "LOGIN";
            VeloJsonResponseLoginQRGenerate loginQRGenerateResponse = PerformRequest<VeloJsonResponseLoginQRGenerate>("LoginQR/generate", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestLoginQRGenerate));

            (VeloJsonResponseLoginQRStatus response, bool requestProcessed) confirmResponse = QRCodeConfirm(loginQRGenerateResponse);
            if (!confirmResponse.requestProcessed)
                return false;

            VeloJsonRequestLoginQRCode jsonRequestLoginQRCode = new VeloJsonRequestLoginQRCode();
            jsonRequestLoginQRCode.ModuleValue = VeloBankJsonModuleType.Banking;
            jsonRequestLoginQRCode.method = "QRCODE";
            //TODO
            //jsonRequestLoginPassword.code_verifier = ;
            jsonRequestLoginQRCode.qr_hash = confirmResponse.response.qr_hash;
            jsonRequestLoginQRCode.qr_uuid = loginQRGenerateResponse.qr_uuid;

            return CreateSession(jsonRequestLoginQRCode);
        }

        private string Sha256(string input)
        {
            SHA256Managed crypt = new SHA256Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(input));
            foreach (byte theByte in crypto)
                hash += theByte.ToString("x2");
            return hash;
        }

        private (VeloJsonResponseLoginQRStatus response, bool requestProcessed) QRCodeConfirm(VeloJsonResponseLoginQRGenerate loginQRGenerateResponse)
        {
            bool aborted = false;
            VeloJsonResponseLoginQRStatus acceptedResponse = null;
            //in mobile application there is error after inputting/scanning code
            //Right after scanning server checks if within a few seconds before were called status (or beginning method)
            System.Threading.Timer statusTimer = null;
            statusTimer = new System.Threading.Timer((state) =>
            {
                (VeloJsonResponseLoginQRStatus, bool) status2 = GetLoginQRStatus(loginQRGenerateResponse.qr_uuid);

                if (status2 == (null, false))
                {
                    aborted = true;
                    statusTimer.Dispose();
                }
                else if (status2.Item1 != null)
                {
                    acceptedResponse = status2.Item1;
                    statusTimer.Dispose();
                }
                //else if (status2 == (null, true))
                //{
                //}
                //else
                //    throw new NotImplementedException();
            }, null, 5000, 5000);

            byte[] bytes = Convert.FromBase64String(loginQRGenerateResponse.qr_code.SubstringFromEx("data:image/png;base64,"));
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (System.Drawing.Image image = System.Drawing.Image.FromStream(ms))
                {
                    bool dialogResult = PromptOKCancel(loginQRGenerateResponse.short_code, "Logowanie przez kod QR", image);

                    if (aborted)
                        return (null, false);
                    if (acceptedResponse != null)
                        return (acceptedResponse, true);

                    statusTimer?.Dispose();

                    if (!dialogResult)
                        return (null, false);
                }
            }

            (VeloJsonResponseLoginQRStatus, bool) status = GetLoginQRStatus(loginQRGenerateResponse.qr_uuid);

            if (status == (null, false))
                return (null, false);
            else if (status.Item1 != null)
                return (status.Item1, true);
            //else if (status == (null, true))
            return QRCodeConfirm(loginQRGenerateResponse);
            //else
            //    throw new NotImplementedException();
        }

        private (VeloJsonResponseLoginQRStatus, bool) GetLoginQRStatus(string qr_uuid)
        {
            VeloJsonRequestLoginQRStatus jsonRequestLoginQRStatus = new VeloJsonRequestLoginQRStatus();
            jsonRequestLoginQRStatus.uuid = qr_uuid;
            VeloJsonResponseLoginQRStatus loginQRStatusResponse = PerformRequest<VeloJsonResponseLoginQRStatus>("LoginQR/status", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestLoginQRStatus));

            if (loginQRStatusResponse.StatusValue == VeloBankJsonLoginQRStatus.Rejected)
                return (null, false);
            if (loginQRStatusResponse.StatusValue == VeloBankJsonLoginQRStatus.Accepted)
                return (loginQRStatusResponse, true);

            return (null, true);
        }

        private bool CreateSession(VeloJsonRequestLoginSessionCreate jsonRequestLoginSessionCreate)
        {
            VeloJsonResponseLoginSessionCreate loginSessionCreateResponse = PerformRequest<VeloJsonResponseLoginSessionCreate>("Session/create", HttpMethod.Post,
               JsonConvert.SerializeObject(jsonRequestLoginSessionCreate));

            if (loginSessionCreateResponse.CheckErrorExists(10096))
                return CheckFailed("Niepoprawne hasło");
            else if (loginSessionCreateResponse.CheckErrorExists(10036))
                return CheckFailed("Niepoprawne hasło. Odblokuj dostęp na stronie internetowej");
            if (loginSessionCreateResponse.CheckErrorExists(10351)) //fast transfer
                return CheckFailed("Niepoprawny kod");

            if (loginSessionCreateResponse.access_token != null)
            {
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginSessionCreateResponse.access_token);
                defaultContextHash = loginSessionCreateResponse.default_context_hash;
            }
            else
            {
                if (!AuthorizeBrowser(loginSessionCreateResponse))
                    return false;
            }

            return true;
        }

        private bool AuthorizeBrowser(VeloJsonResponseLoginSessionCreate loginSessionCreateResponse)
        {
            (bool, VeloJsonResponseConfirm) confirm = Confirm(loginSessionCreateResponse, new ConfirmTextAuthorize(null));
            if (!confirm.Item1)
                return false;

            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", confirm.Item2.response.session_create.access_token);
            defaultContextHash = confirm.Item2.response.session_create.default_context_hash;

            if (PromptYesNo("Dodać urządzenie do zarejestrowanych?"))
            {
                VeloJsonRequestRememberDevice jsonRequestRememberDevice = new VeloJsonRequestRememberDevice();
                jsonRequestRememberDevice.option = "PERMANENT";

                VeloJsonResponseRememberDevice rememberDeviceResponse = PerformRequest<VeloJsonResponseRememberDevice>(
                   "Banking/rememberDevice", HttpMethod.Post,
                   JsonConvert.SerializeObject(jsonRequestRememberDevice));

                if (!Confirm(rememberDeviceResponse, new ConfirmTextAddDevice(null)).Item1)
                    return false;
            }

            return true;
        }

        protected override bool LogoutRequest()
        {
            PerformRequest<VeloJsonResponseLogout>("Session/delete", HttpMethod.Delete,
                 null);

            return true;
        }

        protected override bool TryExtendSession()
        {
            VeloJsonResponseHeartbeat heartbeatResponse = PerformRequest<VeloJsonResponseHeartbeat>("Session/extend", HttpMethod.Post,
                 null);

            //TODO?
            if (heartbeatResponse == null)
                return true;

            return true;
        }

        protected override VeloBankJsonResponseAccounts GetAccountsDetails()
        {
            VeloBankJsonResponseAccounts accountsResponse = PerformRequest<VeloBankJsonResponseAccounts>(
                //"https://secure.velobank.pl/api/v006/Users/finances?type=DASHBOARD"
                "Users/finances?type=DASHBOARD", HttpMethod.Get,
                null);

            return accountsResponse;
        }

        protected override List<VeloBankAccountData> GetAccountsDataMainMain(VeloBankJsonResponseAccounts accountsDetails)
        {
            return accountsDetails.accounts.summary.SelectMany(a => a.products.Select(p => new VeloBankAccountData(p.display_name, p.account_number.account_number, p.balance.currency, p.available_funds.amount) { Id = p.id })).ToList();
        }

        public override bool MakeTransfer(string recipient, string address, string accountNumber, string title, double amount)
        {
            string formattedAccountNumber = accountNumber.SimplifyAccountNumber(true);

            VeloJsonRequestTransferInfo jsonRequestTransferInfo = new VeloJsonRequestTransferInfo();
            jsonRequestTransferInfo.amount = new VeloJsonRequestAmount() { amount = amount.Display(DecimalSeparator.Dot), currency = SelectedAccountData.Currency };
            jsonRequestTransferInfo.sender_account_number = new VeloJsonRequestAccountNumber() { account_number = SelectedAccountData.AccountNumber, country_code = null };
            //TODO PL + below
            jsonRequestTransferInfo.recipient_account_number = new VeloJsonRequestAccountNumber() { account_number = formattedAccountNumber, country_code = "PL" };
            jsonRequestTransferInfo.transfer_mode = "ELIXIR";
            jsonRequestTransferInfo.transfer_type = "TRANSFER";
            jsonRequestTransferInfo.transfer_date = DateTime.Today.Display("yyyy-MM-dd");

            VeloBankJsonResponseTransferInfo transferInfoResponse = PerformRequest<VeloBankJsonResponseTransferInfo>(
                "Transfers/info", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestTransferInfo));

            if (transferInfoResponse.recipient_bank == null)
                return CheckFailed("Niepoprawny numer konta");

            VeloJsonRequestTransferCheck jsonRequestTransferCheck = new VeloJsonRequestTransferCheck();
            jsonRequestTransferCheck.amount = new VeloJsonRequestAmount() { amount = amount.Display(DecimalSeparator.Dot), currency = SelectedAccountData.Currency };
            jsonRequestTransferCheck.recipient_account_number = new VeloJsonRequestAccountNumber() { account_number = formattedAccountNumber, country_code = "PL" };
            jsonRequestTransferCheck.source_product = SelectedAccountData.Id;
            jsonRequestTransferCheck.title = title;
            jsonRequestTransferCheck.transfer_mode = "ELIXIR";
            jsonRequestTransferCheck.transfer_type = "TRANSFER";
            jsonRequestTransferCheck.payment_date = DateTime.Today.Display("yyyy-MM-dd");

            //TODO is this neccessary + in other transfers
            VeloBankJsonResponseTransferCheck transferCheckResponse = PerformRequest<VeloBankJsonResponseTransferCheck>(
                "Transfers/check", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestTransferCheck));

            VeloJsonRequestTransferDomestic jsonRequestTransferDomestic = new VeloJsonRequestTransferDomestic();
            jsonRequestTransferDomestic.amount = new VeloJsonRequestAmount() { amount = amount.Display(DecimalSeparator.Dot), currency = SelectedAccountData.Currency };
            jsonRequestTransferDomestic.recipient_account_number = new VeloJsonRequestAccountNumber() { account_number = formattedAccountNumber, country_code = "PL" };
            jsonRequestTransferDomestic.recipient_address = address;
            jsonRequestTransferDomestic.recipient_id = null;
            jsonRequestTransferDomestic.recipient_name = recipient;
            jsonRequestTransferDomestic.retry_if_lack_of_funds = false;
            jsonRequestTransferDomestic.save_recipient = false;
            jsonRequestTransferDomestic.send_notification_to_email = false;
            jsonRequestTransferDomestic.source_product = SelectedAccountData.Id;
            jsonRequestTransferDomestic.title = title;
            jsonRequestTransferDomestic.type = "ELIXIR";
            jsonRequestTransferDomestic.payment_date = DateTime.Today.Display("yyyy-MM-dd");

            VeloBankJsonResponseTransferDomestic transferDomesticResponse = PerformRequest<VeloBankJsonResponseTransferDomestic>(
                "Transfers/domestic", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestTransferDomestic));

            //TODO 10031 also to MakePrepaidTransfer ?
            if (transferDomesticResponse.CheckErrorExists(10031) || transferDomesticResponse.CheckErrorExists(20808) || transferDomesticResponse.CheckErrorExists(22006))
                return CheckFailed("Błędne dane");

            return Confirm(transferDomesticResponse, new ConfirmTextTransfer(amount, SelectedAccountData.Currency, transferInfoResponse.recipient_bank.name, accountNumber.SimplifyAccountNumber())).Item1;
        }

        public override bool MakeTaxTransfer(string taxType, string accountNumber, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier, string creditorName, string obligationId, double amount)
        {
            VeloBankJsonResponseTaxFormTypes taxFormTypesResponse = PerformRequest<VeloBankJsonResponseTaxFormTypes>(
                "Dictionaries/taxFormTypes", HttpMethod.Get,
                null);

            VeloBankJsonResponseTaxFormTypesItem selectedTax = taxFormTypesResponse.items.SingleOrDefault(i => i.form_type == taxType);
            if (selectedTax == null)
                return CheckFailed("Nie znaleziono podanego typu formularza");

            VeloJsonRequestTransferCheck jsonRequestTransferCheck = new VeloJsonRequestTransferCheck();
            jsonRequestTransferCheck.amount = new VeloJsonRequestAmount() { amount = amount.Display(DecimalSeparator.Dot), currency = SelectedAccountData.Currency };
            jsonRequestTransferCheck.payment_date = DateTime.Today.Display("yyyy-MM-dd");
            jsonRequestTransferCheck.source_product = SelectedAccountData.Id;
            jsonRequestTransferCheck.transfer_mode = "ELIXIR";
            jsonRequestTransferCheck.transfer_type = "TAX_TRANSFER";

            VeloJsonRequestTransferTax jsonRequestTaxTransfer = new VeloJsonRequestTransferTax();
            jsonRequestTaxTransfer.amount = new VeloJsonRequestAmount() { amount = amount.Display(DecimalSeparator.Dot), currency = SelectedAccountData.Currency };
            jsonRequestTaxTransfer.source_product = SelectedAccountData.Id;
            jsonRequestTaxTransfer.payment_date = DateTime.Today.Display("yyyy-MM-dd");
            jsonRequestTaxTransfer.tax_declaration_data = new VeloJsonRequestTransferTaxTaxDeclarationData();
            jsonRequestTaxTransfer.tax_declaration_data.form_type = taxType;
            jsonRequestTaxTransfer.tax_declaration_data.obligation_identifier = obligationId ?? String.Empty;
            jsonRequestTaxTransfer.tax_declaration_data.payer_address = String.Empty;
            jsonRequestTaxTransfer.tax_declaration_data.payer_name = creditorName;
            //additional_data

            VeloBankJsonResponseTaxOfficeItem taxOffice = null;

            if (selectedTax.is_irp)
            {
                jsonRequestTransferCheck.recipient_account_number = new VeloJsonRequestAccountNumber() { account_number = accountNumber, country_code = "PL" };
                jsonRequestTaxTransfer.recipient_account_number = new VeloJsonRequestAccountNumberDomestic() { account_number = accountNumber };

                //TODO check if accountNumber matches template from z Dictionaries/taxOffices/form_type/{taxType}
            }
            else
            {
                VeloBankJsonResponseTaxOffice taxOfficeResponse = PerformRequest<VeloBankJsonResponseTaxOffice>(
                    $"Dictionaries/taxOffices/form_type/{taxType}", HttpMethod.Get,
                    null);

                taxOffice = PromptComboBox<VeloBankJsonResponseTaxOfficeItem>("Urząd", taxOfficeResponse.items.Select(o => new SelectComboBoxItem<VeloBankJsonResponseTaxOfficeItem>(o.description, o)), true).data;
                if (taxOffice == null)
                    return false;

                jsonRequestTransferCheck.recipient_account_number = new VeloJsonRequestAccountNumber() { account_number = taxOffice.account_number.account_number, country_code = "PL" };
                jsonRequestTaxTransfer.recipient_account_number = new VeloJsonRequestAccountNumberDomestic() { account_number = taxOffice.account_number.account_number };
                jsonRequestTaxTransfer.tax_declaration_data.tax_office = taxOffice.id;
            }

            //if (selectedTax.is_vat_indicator)

            jsonRequestTaxTransfer.tax_declaration_data.identifier_type = GetTaxCreditorIdentifierTypeId(creditorIdentifier);
            jsonRequestTaxTransfer.tax_declaration_data.identifier_value = creditorIdentifier.GetId();

            //period sent always, tax code doesn't have period markation
            (string settlementType, string number, string year) = GetTaxPeriodValue(period);

            jsonRequestTaxTransfer.tax_declaration_data.settlement_type = settlementType;
            jsonRequestTaxTransfer.tax_declaration_data.settlement_value = number;
            jsonRequestTaxTransfer.tax_declaration_data.year = year;

            VeloBankJsonResponseTransferCheck transferCheckResponse = PerformRequest<VeloBankJsonResponseTransferCheck>(
                "Transfers/check", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestTransferCheck));

            VeloBankJsonResponseTransferTax taxTransferResponse = PerformRequest<VeloBankJsonResponseTransferTax>(
                "Transfers/tax", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestTaxTransfer));

            if (taxTransferResponse.CheckErrorExists(10031) || taxTransferResponse.CheckErrorExists(20808) || taxTransferResponse.CheckErrorExists(22006))
                return CheckFailed("Błędne dane");

            return Confirm(taxTransferResponse, new ConfirmTextTaxTransfer(amount, SelectedAccountData.Currency, taxOffice == null ? null : taxOffice.description)).Item1;
        }

        public static string GetTaxCreditorIdentifierTypeId(TaxCreditorIdentifier creditorIdentifier)
        {
            if (creditorIdentifier is TaxCreditorIdentifierNIP)
                return "NIP";
            else if (creditorIdentifier is TaxCreditorIdentifierIDCard)
                return "ID_CARD";
            else if (creditorIdentifier is TaxCreditorIdentifierPESEL)
                return "PESEL";
            else if (creditorIdentifier is TaxCreditorIdentifierREGON)
                return "REGON";
            else if (creditorIdentifier is TaxCreditorIdentifierPassport)
                return "PASSPORT";
            else if (creditorIdentifier is TaxCreditorIdentifierOther)
                throw new ArgumentException("Nieobsługiwany typ identyfikatora");
            else
                throw new ArgumentException();
        }

        public static (string unit, string number, string year) GetTaxPeriodValue(TaxPeriod period)
        {
            if (period is TaxPeriodDay taxPeriodDay)
                return ("DAY", $"{GetTaxPeriodNumberValue(taxPeriodDay.Day.Day)}{GetTaxPeriodNumberValue(taxPeriodDay.Day.Month)}", taxPeriodDay.Day.Year.ToString());
            else if (period is TaxPeriodHalfYear taxPeriodHalfYear)
                return ("HALF_YEAR", GetTaxPeriodNumberValue(taxPeriodHalfYear.Half), taxPeriodHalfYear.Year.ToString());
            else if (period is TaxPeriodMonth taxPeriodMonth)
                return ("MONTH", GetTaxPeriodNumberValue(taxPeriodMonth.Month), taxPeriodMonth.Year.ToString());
            else if (period is TaxPeriodMonthDecade taxPeriodMonthDecade)
                return ("DECADE", $"{GetTaxPeriodNumberValue(taxPeriodMonthDecade.Decade)}{GetTaxPeriodNumberValue(taxPeriodMonthDecade.Month)}", taxPeriodMonthDecade.Year.ToString());
            else if (period is TaxPeriodQuarter taxPeriodQuarter)
                return ("QUARTER", GetTaxPeriodNumberValue(taxPeriodQuarter.Quarter), taxPeriodQuarter.Year.ToString());
            else if (period is TaxPeriodYear taxPeriodYear)
                return ("YEAR", null, taxPeriodYear.Year.ToString());
            else
                throw new ArgumentException();
        }

        private static string GetTaxPeriodNumberValue(int number)
        {
            return number.ToString("D2");
        }

        protected override string CleanFastTransferUrl(string transferId)
        {
            string newTransferId = transferId
                .Replace("https://", String.Empty)

                .Replace("secure.velobank.pl/login/pa/", String.Empty)

                .Replace("secure.velobank.pl/login/pbl/", String.Empty)

                .Replace("/mobile", String.Empty);

            (FastTransferType? type, string pblData, (string key, string hash) paData) fastTransferData = GetDataFromFastTransfer(newTransferId);

            if (fastTransferData.type == null)
                return null;

            return newTransferId;
        }

        protected override bool LoginRequestForFastTransfer(string login, string password, List<object> additionalAuthorization, string transferId)
        {
            return LoginRequest(login, password, transferId) && PostLoginRequest();
        }

        protected override string MakeFastTransfer(string transferId)
        {
            (FastTransferType? type, string pblData, (string key, string hash) paData) fastTransferData = GetDataFromFastTransfer(transferId);

            if (fastTransferData.type == FastTransferType.PayByLink)
            {
                VeloBankJsonResponseFastTransferPBL fastTransferPBLResponse = PerformRequest<VeloBankJsonResponseFastTransferPBL>(
                    $"PayByLink/details/hash/{fastTransferData.pblData}", HttpMethod.Get,
                    null);

                VeloJsonRequestFastTransferAcceptPBL jsonRequestFastTransferAcceptPBL = new VeloJsonRequestFastTransferAcceptPBL();
                jsonRequestFastTransferAcceptPBL.id_product = fastTransferPBLResponse.products.First().id;

                VeloBankJsonResponseFastTransferAcceptPBL fastTransferAcceptPBLResponse = PerformRequest<VeloBankJsonResponseFastTransferAcceptPBL>(
                    $"PayByLink/accept/hash/{fastTransferData.paData.key}", HttpMethod.Put,
                    JsonConvert.SerializeObject(jsonRequestFastTransferAcceptPBL));

                if (!Confirm(fastTransferAcceptPBLResponse, new ConfirmTextFastTransfer(fastTransferPBLResponse.payment.amount.amount, fastTransferPBLResponse.payment.amount.currency, fastTransferPBLResponse.payment.recipient_name)).Item1)
                    return null;

                return fastTransferPBLResponse.redirect_uri;
            }
            else if (fastTransferData.type == FastTransferType.PA)
            {
                VeloBankJsonResponseFastTransferAuthorize fastTransferAuthorizeResponse = PerformRequest<VeloBankJsonResponseFastTransferAuthorize>(
                    $"Consent/get?authorize_request_key={fastTransferData.paData.key}", HttpMethod.Get,
                    null);

                if (fastTransferAuthorizeResponse.CheckErrorExists(502))
                {
                    CheckFailed("Kod nieważny");
                    return null;
                }

                VeloJsonRequestFastTransferAcceptAuthorize jsonRequestFastTransferAuthorize = new VeloJsonRequestFastTransferAcceptAuthorize();
                jsonRequestFastTransferAuthorize.privilege_details = fastTransferAuthorizeResponse.agreements.Select(a => new VeloJsonRequestFastTransferAcceptAuthorizePrivilegeDetail() { id = a.id_agreement, products = a.products.Select(p => new VeloJsonRequestAccountNumber() { account_number = p.account_number.account_number, country_code = p.account_number.country_code }).ToList() }).ToList();

                VeloBankJsonResponseFastTransferAcceptAuthorize fastTransferAcceptAuthorizeResponse = PerformRequest<VeloBankJsonResponseFastTransferAcceptAuthorize>(
                    $"Consent/accept/authorize_request_key/{fastTransferData.paData.key}", HttpMethod.Put,
                    JsonConvert.SerializeObject(jsonRequestFastTransferAuthorize));

                double amount = fastTransferAuthorizeResponse.agreements.Single().details.Single().transfers_total_amount.amount;
                string recipientName = fastTransferAuthorizeResponse.agreements.Single().details.Single().payments.Single().recipient.name;
                (bool, VeloJsonResponseConfirm) confirm = Confirm(fastTransferAcceptAuthorizeResponse, new ConfirmTextFastTransfer(amount, SelectedAccountData.Currency, recipientName));
                if (!confirm.Item1)
                    return null;

                //TODO where come go-api.przelewy24.pl from
                //TODO the same value as url
                return confirm.Item2.response.consent_accept.redirect_url;
            }
            else
                throw new NotImplementedException();
        }

        protected override bool MakePrepaidTransferMain(string recipient, string phoneNumber, double amount)
        {
            VeloBankJsonResponseTransferPrepaidInfo transferPrepaidInfoResponse = PerformRequest<VeloBankJsonResponseTransferPrepaidInfo>(
                "Transfers/prepaidInfo", HttpMethod.Get,
                null);

            VeloBankJsonResponseTransferPrepaidInfoOperator operatorItem = PromptComboBox<VeloBankJsonResponseTransferPrepaidInfoOperator>("Operator", transferPrepaidInfoResponse.operators.Select(o => new SelectComboBoxItem<VeloBankJsonResponseTransferPrepaidInfoOperator>(o.display_name, o)), false).data;
            if (operatorItem == null)
                return false;

            if ((operatorItem.amount_min != null && amount < operatorItem.amount_min.amount)
                || (operatorItem.amount_max != null && amount > operatorItem.amount_max.amount))
                return CheckFailed($"Kwota powinna znajdować się w zakresie {operatorItem.amount_min?.amount}-{operatorItem.amount_max?.amount}");
            if (operatorItem.amounts.Count != 0 && !operatorItem.amounts.Select(a => a.amount).Contains(amount))
                return CheckFailed($"Kwota powinna być jedną z {String.Join(", ", operatorItem.amounts.Select(a => a.amount.Display(DecimalSeparator.Dot)))}");

            VeloJsonRequestTransferCheck jsonRequestTransferCheck = new VeloJsonRequestTransferCheck();
            jsonRequestTransferCheck.amount = new VeloJsonRequestAmount() { amount = amount.Display(DecimalSeparator.Dot), currency = SelectedAccountData.Currency };
            jsonRequestTransferCheck.recipient_phone = new VeloJsonRequestPhone() { id_operator = operatorItem.id, prefix = "+48", phone_number = phoneNumber };
            jsonRequestTransferCheck.source_product = SelectedAccountData.Id;
            jsonRequestTransferCheck.transfer_mode = "ELIXIR";
            jsonRequestTransferCheck.transfer_type = "PREPAID_TRANSFER";
            jsonRequestTransferCheck.payment_date = DateTime.Today.Display("yyyy-MM-dd");

            VeloBankJsonResponseTransferCheck transferCheckResponse = PerformRequest<VeloBankJsonResponseTransferCheck>(
                "Transfers/check", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestTransferCheck));

            //TODO also to MakeTransfer and MakeTaxTransfer ?
            if (transferCheckResponse.CheckErrorExists(22006))
                return CheckFailed("Błędne dane");

            VeloJsonRequestTransferPrepaid jsonRequestTransferPrepaid = new VeloJsonRequestTransferPrepaid();
            jsonRequestTransferPrepaid.amount = new VeloJsonRequestAmount() { amount = amount.Display(DecimalSeparator.Dot), currency = SelectedAccountData.Currency };
            jsonRequestTransferPrepaid.phone_number = new VeloJsonRequestPhone() { id_operator = operatorItem.id, prefix = "+48", phone_number = phoneNumber };
            jsonRequestTransferPrepaid.source_product = SelectedAccountData.Id;
            jsonRequestTransferPrepaid.recipient_id = null;
            jsonRequestTransferPrepaid.recipient_name = recipient;
            jsonRequestTransferPrepaid.save_recipient = false;
            jsonRequestTransferPrepaid.payment_date = DateTime.Today.Display("yyyy-MM-dd");

            VeloBankJsonResponseTransferPrepaid transferPrepaidResponse = PerformRequest<VeloBankJsonResponseTransferPrepaid>(
                "Transfers/prepaid", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestTransferPrepaid));

            if (transferPrepaidResponse.CheckErrorExists(20808) || transferPrepaidResponse.CheckErrorExists(22006))
                return CheckFailed("Błędne dane");

            return Confirm(transferPrepaidResponse, new ConfirmTextPrepaidTransfer(amount, SelectedAccountData.Currency, operatorItem.display_name, phoneNumber)).Item1;
        }

        protected override VeloBankHistoryFilter CreateFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact)
        {
            return new VeloBankHistoryFilter(direction, title, dateFrom, dateTo, amountExact);
        }

        protected override List<VeloBankHistoryItem> GetHistoryItems(VeloBankHistoryFilter filter = null)
        {
            List<VeloBankHistoryItem> result = new List<VeloBankHistoryItem>();

            if (filter != null)
            {
                if (filter.FindAccountNumber)
                {
                    //TODO the same what in getinbank
                    VeloBankAcountNumbersHistory.Download(this);

                    Dictionary<(DateTime dateFrom, DateTime dateTo), List<string>> ranges = new Dictionary<(DateTime dateFrom, DateTime dateTo), List<string>>();
                    foreach (DictionaryEntry entry in Properties.Settings.Default.VeloBankAcountNumbers)
                    {
                        if (AccountNumberTools.CompareAccountNumbers((string)entry.Value, filter.AccountNumber))
                        {
                            DateTime transferDate = GetDateFromReferenceNumber((string)entry.Key);
                            if ((filter.DateFrom == null || filter.DateFrom <= transferDate) && (filter.DateTo == null || transferDate <= filter.DateTo))
                            {
                                List<string> refNumbers;
                                DateTime rangeDateFrom;
                                DateTime rangeDateTo;
                                KeyValuePair<(DateTime dateFrom, DateTime dateTo), List<string>> range = ranges.SingleOrDefault(r => r.Key.dateFrom <= transferDate && transferDate <= r.Key.dateTo);
                                if (!range.Equals(default(KeyValuePair<(DateTime dateFrom, DateTime dateTo), List<string>>)))
                                {
                                    refNumbers = range.Value;
                                    rangeDateFrom = range.Key.dateFrom;
                                    rangeDateTo = range.Key.dateTo;
                                }
                                else
                                {
                                    refNumbers = new List<string>();
                                    rangeDateFrom = transferDate;
                                    rangeDateTo = transferDate;
                                }
                                refNumbers.Add((string)entry.Key);
                                ranges[(rangeDateFrom, rangeDateTo)] = refNumbers;

                                KeyValuePair<(DateTime dateFrom, DateTime dateTo), List<string>> bottomRange = ranges.SingleOrDefault(r => r.Key.dateTo == transferDate.AddDays(-1));
                                if (!bottomRange.Equals(default(KeyValuePair<(DateTime dateFrom, DateTime dateTo), List<string>>)))
                                {
                                    List<string> refNumbersMerge = bottomRange.Value;
                                    refNumbers.AddRange(refNumbersMerge);
                                    ranges.Remove((rangeDateFrom, rangeDateTo));
                                    ranges.Remove(bottomRange.Key);
                                    rangeDateFrom = bottomRange.Key.dateFrom;
                                    ranges[(rangeDateFrom, rangeDateTo)] = refNumbers;
                                }
                                KeyValuePair<(DateTime dateFrom, DateTime dateTo), List<string>> topRange = ranges.SingleOrDefault(r => r.Key.dateFrom == transferDate.AddDays(1));
                                if (!topRange.Equals(default(KeyValuePair<(DateTime dateFrom, DateTime dateTo), List<string>>)))
                                {
                                    List<string> refNumbersMerge = topRange.Value;
                                    refNumbers.AddRange(refNumbersMerge);
                                    ranges.Remove((rangeDateFrom, rangeDateTo));
                                    ranges.Remove(topRange.Key);
                                    rangeDateTo = topRange.Key.dateTo;
                                    ranges[(rangeDateFrom, rangeDateTo)] = refNumbers;
                                }
                            }
                        }
                    }

                    foreach (KeyValuePair<(DateTime dateFrom, DateTime dateTo), List<string>> range in ranges.OrderByDescending(r => r.Key.dateFrom))
                    {
                        List<VeloBankHistoryItem> items = GetHistoryItems(new VeloBankHistoryFilter()
                        {
                            DateFrom = range.Key.dateFrom,
                            DateTo = range.Key.dateTo,
                            AmountExact = filter.AmountExact,
                            AmountFrom = filter.AmountFrom,
                            AmountTo = filter.AmountTo,
                            OperationType = filter.OperationType,
                            Title = filter.Title
                        });
                        if (items != null)
                            result.AddRange(items.Where(i => range.Value.Contains(i.ReferenceNumber)));
                    }
                }
                else
                {
                    bool fetchedAll = false;
                    for (int page = 1; !fetchedAll && (filter.CounterLimit == 0 || result.Count < filter.CounterLimit); page++)
                    {
                        VeloJsonRequestHistory jsonRequestHistory = new VeloJsonRequestHistory();
                        jsonRequestHistory.show_blockades = true;
                        jsonRequestHistory.search = filter.Title ?? String.Empty;
                        jsonRequestHistory.paginator = new VeloJsonRequestHistoryPaginator() { limit = 15, page = page };
                        jsonRequestHistory.filters = new VeloJsonRequestHistoryFilters() { products = new List<string>() { SelectedAccountData.Id }, cards = new List<string>() };
                        if (filter.DateFrom != null)
                            jsonRequestHistory.filters.date_from = ((DateTime)filter.DateFrom).Display("yyyy-MM-dd");
                        if (filter.DateTo != null)
                        {
                            DateTime dateTo = ((DateTime)filter.DateTo);
                            //TODO timezone + in santander
                            if (dateTo > DateTime.Today)
                                dateTo = DateTime.Today;
                            jsonRequestHistory.filters.date_to = dateTo.Display("yyyy-MM-dd");
                        }
                        if (filter.AmountFrom != null)
                            jsonRequestHistory.filters.min_amount = ((double)filter.AmountFrom).Display(DecimalSeparator.Dot);
                        if (filter.AmountTo != null)
                            jsonRequestHistory.filters.max_amount = ((double)filter.AmountTo).Display(DecimalSeparator.Dot);
                        jsonRequestHistory.filters.type = filter.OperationType == null ? String.Empty : AttributeOperations.GetEnumAttribute((VeloBankFilterOperation)filter.OperationType, (FilterEnumParameterAttribute parameter) => parameter.Parameter, String.Empty);
                        jsonRequestHistory.filters.kind = filter.KindType == null ? String.Empty : AttributeOperations.GetEnumAttribute((VeloBankFilterKind)filter.KindType, (FilterEnumParameterAttribute parameter) => parameter.Parameter, String.Empty);
                        jsonRequestHistory.filters.status = filter.StatusType == null ? String.Empty : AttributeOperations.GetEnumAttribute((VeloBankFilterStatus)filter.StatusType, (FilterEnumParameterAttribute parameter) => parameter.Parameter, String.Empty);

                        VeloBankJsonResponseHistory historyResponse = PerformRequest<VeloBankJsonResponseHistory>(
                            "Transfers/history", HttpMethod.Post,
                            JsonConvert.SerializeObject(jsonRequestHistory));

                        fetchedAll = !historyResponse.pagination.has_next_page;

                        result.AddRange(historyResponse.list.Select(i => new VeloBankHistoryItem(i)));
                    }
                }
            }

            return result;
        }

        protected override bool GetDetailsFileMain(VeloBankHistoryItem item, Func<ContentDispositionHeaderValue, FileStream> file)
        {
            VeloJsonRequestDetailsFile jsonRequestDetailsFile = new VeloJsonRequestDetailsFile();
            jsonRequestDetailsFile.id = new List<string>() { item.Id };
            jsonRequestDetailsFile.product = SelectedAccountData.Id;
            jsonRequestDetailsFile.type = "TRANSFER";

            VeloBankJsonResponseDetailsFile detailsFileResponse = PerformRequest<VeloBankJsonResponseDetailsFile>(
                "File/get", HttpMethod.Post,
                JsonConvert.SerializeObject(jsonRequestDetailsFile));

            PerformFileRequest("https://secure.velobank.pl" + detailsFileResponse.path, HttpMethod.Get, file);

            return true;
        }

        private (bool, VeloJsonResponseConfirm) Confirm(VeloJsonResponseConfirmable response, ConfirmTextBase confirmText)
        {
            switch (response.TypeValue)
            {
                case VeloBankJsonConfirmType.SMS:
                    {
                        return SMSConfirm<(bool, VeloJsonResponseConfirm), VeloJsonResponseConfirm>(
                            (string SMSCode) =>
                            {
                                VeloJsonRequestConfirm jsonRequestConfirm = new VeloJsonRequestConfirm();
                                jsonRequestConfirm.token = SMSCode;

                                return PerformRequest<VeloJsonResponseConfirm>(
                                    $"Confirmations/confirm/uuid/{response.uuid}", HttpMethod.Put,
                                    JsonConvert.SerializeObject(jsonRequestConfirm));
                            },
                            (VeloJsonResponseConfirm confirmResponse) =>
                            {
                                if (confirmResponse.CheckErrorExists(10012))
                                    return CheckFailed(confirmResponse.errors.First(e => e.error == 10012).error_description);
                                if (confirmResponse.CheckErrorExists(10000))
                                    return null;
                                else
                                    return true;
                            },
                            (VeloJsonResponseConfirm confirmResponse) => (true, confirmResponse),
                            null,
                            //TODO use confirmResponse.response.session_create.device_name
                            confirmText,
                            response.sms_no);
                    }
                case VeloBankJsonConfirmType.Mobile:
                    {
                        //TODO change channel for SMS, like in ING
                        return MobileConfirm<(bool, VeloJsonResponseConfirm), VeloJsonResponseConfirm>(
                            () =>
                            {
                                return PerformRequest<VeloJsonResponseConfirm>(
                                    $"Confirmations/info/uuid/{response.uuid}", HttpMethod.Get,
                                    null);
                            },
                            (VeloJsonResponseConfirm confirmResponse) =>
                            {
                                if (confirmResponse.StatusValue == VeloBankJsonConfirmationStatusType.Rejected)
                                    return false;
                                if (confirmResponse.StatusValue == VeloBankJsonConfirmationStatusType.Accepted)
                                    return true;

                                return null;
                            },
                            (VeloJsonResponseConfirm confirmResponse) => (true, confirmResponse),
                            null,
                            confirmText);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private T PerformRequest<T>(string requestUri, HttpMethod method,
            string jsonContent) where T : VeloJsonResponseBase
        {
            try
            {
                using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, jsonContent))
                using (HttpResponseMessage response = HttpOperations.GetResponse(Client, request))
                    return ProcessResponse<T>(response,
                        (string responseStr) =>
                        {
                            if (!CheckNotExpired(responseStr))
                            {
                                NoteExpiredSession();
                                return false;
                            }
                            return true;
                        },
                        (string responseStr) => JsonConvert.DeserializeObject<T>(responseStr)).Item1;
            }
            //TODO + in other methods + in other banks
            catch (AggregateException aggregateException)
            {
                if (aggregateException.InnerExceptions.Count == 1)
                {
                    if (aggregateException.InnerExceptions[0] is HttpRequestException httpRequestException)
                    {
                        if (httpRequestException.InnerException != null)
                        {
                            if (httpRequestException.InnerException is WebException)
                            {
                                CheckFailed("Brak internetu");
                                return default;
                            }
                        }
                    }
                }

                throw;
            }
        }

        private void PerformFileRequest(string requestUri, HttpMethod method,
            Func<ContentDispositionHeaderValue, FileStream> fileStream)
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, null))
                ProcessFileStream(request, fileStream);
        }

        private HttpRequestMessage CreateHttpRequestMessage(string requestUri, HttpMethod method, string jsonContent)
        {
            List<(string name, string value)> headers = new List<(string name, string value)>();
            headers.Add(("X-Client-Hash", "b2-hash"));
            if (defaultContextHash != null)
                headers.Add(("X-CONTEXT", defaultContextHash));
            //TODO adsress automatically from httpClient
            Cookie hostCsrfCookie = DomainCookies.GetCookie("__Host-csrf");
            if (hostCsrfCookie != null)
                headers.Add(("X-CSRF-TOKEN", hostCsrfCookie.Value));

            return HttpOperations.CreateHttpRequestMessageJson(method, requestUri, jsonContent, headers);
        }

        private bool CheckNotExpired(string responseStr)
        {
            VeloJsonResponseBase response = JsonConvert.DeserializeObject<VeloJsonResponseBase>(responseStr);

            if (response?.CheckErrorExists(10011) ?? false)
                return CheckFailed("Utracono połączenie");

            return true;
        }

        public static DateTime GetDateFromReferenceNumber(string referenceNumber)
        {
            string dateString = referenceNumber.SubstringToEx("/");
            return new DateTime(Int32.Parse(dateString.Substring(0, 4)), Int32.Parse(dateString.Substring(4, 2)), Int32.Parse(dateString.Substring(6, 2)));
        }

        private static (FastTransferType? type, string pblData, (string key, string hash) paData) GetDataFromFastTransfer(string transferId)
        {
            string newTransferId = transferId.SubstringToEx("?");

            bool pbl = !newTransferId.Contains("/") && transferId.Length == 64;
            bool pa = newTransferId.Contains("/") && newTransferId.SubstringToEx("/").Length == 32 && newTransferId.SubstringFromEx("/").Length == 28;
            FastTransferType? type = null;
            if (pbl)
                type = FastTransferType.PayByLink;
            if (pa)
                type = FastTransferType.PA;
            return (type, newTransferId, (newTransferId.SubstringToEx("/"), newTransferId.SubstringFromEx("/")));
        }
    }
}
