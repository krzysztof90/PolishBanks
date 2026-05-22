using BankService.BankCountry;
using BankService.ConfirmText;
using BankService.LocalTools;
using BankService.SMSCodes;
using BankService.Tax.TaxCreditorIdentifiers;
using BankService.Tax.TaxPeriods;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Tools;
using Tools.Enums;
using static BankService.Bank_PL_Pocztowy.PocztowyJsonRequest;
using static BankService.Bank_PL_Pocztowy.PocztowyJsonResponse;

namespace BankService.Bank_PL_Pocztowy
{
    [BankTypeAttribute(BankType.Pocztowy)]
    public class Pocztowy : BankPoland<PocztowyAccountData, PocztowyHistoryItem, PocztowyHistoryFilter, PocztowyJsonResponseAccountTransactions>
    {
        private string publicKey;
        private string sessionId;
        private string xsrfToken;

        protected override int HeartbeatInterval => 270;
        protected override SMSCodeValidator SMSCodeValidator => new SMSCodeValidatorCypher(6);

        public override bool AllowAlternativeLoginMethod => false;

        public override bool TransferMandatoryRecipient => true;
        public override bool TransferMandatoryTitle => true;
        public override bool PrepaidTransferMandatoryRecipient => false;

        protected override string BaseAddress => "https://online.pocztowy.pl";

        public Pocztowy() : base()
        {
            CollectionOperations.PrepareRandom();
        }

        protected override void CleanHttpClient()
        {
            publicKey = null;
            sessionId = null;
            xsrfToken = null;
        }

        protected override bool LoginRequest(string login, string password, List<object> additionalAuthorization)
        {
            int avatarId = (int)additionalAuthorization[0];

            (string response, bool requestProcessed, HttpStatusCode statusCode) loginMainResponse = PerformPlainRequest(
                "login/main", HttpMethod.Get,
                false);
            if (!loginMainResponse.requestProcessed)
                return false;

            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(loginMainResponse.response);
            HtmlAgilityPack.HtmlNode scriptAppNode = document.DocumentNode.Descendants("script").Single(n => n.GetAttributeValue("src", String.Empty).StartsWith("/scripts/app."));
            string scriptUrl = scriptAppNode.GetAttributeValue("src", String.Empty);

            (string response, bool requestProcessed, HttpStatusCode statusCode) loginScriptResponse = PerformPlainRequest(
                scriptUrl, HttpMethod.Get,
                false);
            if (!loginScriptResponse.requestProcessed)
                return false;

            publicKey = loginScriptResponse.response.SubstringFromToEx(".useProdCryptKeys?\"-----BEGIN PUBLIC KEY", "END PUBLIC KEY-----\\n\":", false, false, false, false, true)
                .SubstringFromToEx(".useProdCryptKeys?\"", "\\n\":");

            (PocztowyJsonResponseLogin response, bool requestProcessed) loginResponse = PerformRequest<PocztowyJsonResponseLogin>(
                "smbm-services/api/auth/createSession", HttpMethod.Post,
                JsonConvert.SerializeObject(PocztowyJsonRequestLogin.Create(login)),
                null, null);
            if (!loginResponse.requestProcessed)
                return false;

            sessionId = loginResponse.response.data.sessionId;
            xsrfToken = DomainCookies.GetCookie("XSRF-TOKEN").Value;
            (PocztowyJsonResponseImage response, bool requestProcessed) imageResponse = PerformRequest<PocztowyJsonResponseImage>(
                "smbm-services/api/gallery/sec/image", HttpMethod.Post,
                JsonConvert.SerializeObject(PocztowyJsonRequestImage.Create("BIG_STANDARD")),
                null, null);
            if (!imageResponse.requestProcessed)
                return false;

            if (avatarId.ToString() != Path.GetFileNameWithoutExtension(imageResponse.response.securityImageUrl))
                return CheckFailed("Niepoprawny obrazek bezpieczeństwa");

            string encodedPassword = EncryptWithRSA(loginResponse.response.data.sessionId + password, publicKey);

            (PocztowyJsonResponseLoginPassword response, bool requestProcessed) loginPasswordResponse = PerformRequest<PocztowyJsonResponseLoginPassword>(
                "smbm-services/api/order/auth/prepareOrder", HttpMethod.Post,
                JsonConvert.SerializeObject(PocztowyJsonRequestLoginPassword.Create(encodedPassword, loginResponse.response.data.sessionId, "AUTHORIZE_SESSION")),
                null, null);
            if (!loginPasswordResponse.requestProcessed)
                return false;

            //TODO sometimes asks for SMS confirm

            sessionId = loginPasswordResponse.response.data.result.sessionId;
            xsrfToken = DomainCookies.GetCookie("XSRF-TOKEN").Value;

            return true;
        }

        private string EncryptWithRSA(string stringDataToEncrypt, string publicKey)
        {
            string keyContent = publicKey.SubstringFromToEx("-----BEGIN PUBLIC KEY-----", "-----END PUBLIC KEY-----").Replace("\\n", String.Empty);
            byte[] key = Convert.FromBase64String(keyContent);

            Asn1Object obj = Asn1Object.FromByteArray(key);

            DerSequence publicKeySequence = (DerSequence)obj;

            DerBitString encodedPublicKey = (DerBitString)publicKeySequence[1];
            DerSequence publicKeyObject = (DerSequence)Asn1Object.FromByteArray(encodedPublicKey.GetBytes());

            DerInteger modulus = (DerInteger)publicKeyObject[0];
            DerInteger exponent = (DerInteger)publicKeyObject[1];

            RsaKeyParameters keyParameters = new RsaKeyParameters(false, modulus.PositiveValue, exponent.PositiveValue);
            RSAParameters parameters = DotNetUtilities.ToRSAParameters(keyParameters);

            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(stringDataToEncrypt);

            string encryptedPassword;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(parameters);

                byte[] encryptedData = rsa.Encrypt(dataToEncrypt, false);
                encryptedPassword = Convert.ToBase64String(encryptedData);
            }

            return Buffer2Hex(Convert.FromBase64String(encryptedPassword));
        }

        private string Buffer2Hex(byte[] buffer)
        {
            return String.Concat(buffer.Select(b => Dec2Hex(b)));
        }

        private string Dec2Hex(int n)
        {
            return n.ToString("x2");
        }

        private string RandomCorrelationId()
        {
            string correlationId = String.Empty;
            for (int i = 0; i < 8; i++)
                correlationId += (char)CollectionOperations.RandomFromRanges(new List<(int, int)> { ('a', 'z'), ('A', 'Z'), ('0', '9') });
            return correlationId;
        }

        protected override bool LogoutRequest()
        {
            (PocztowyJsonResponseLogout response, bool requestProcessed) logoutResponse = PerformRequest<PocztowyJsonResponseLogout>(
                "smbm-services/api/auth/closeSession", HttpMethod.Post,
                JsonConvert.SerializeObject(PocztowyJsonRequestLogout.Create()),
                null, null);
            return logoutResponse.requestProcessed;
        }

        private int GetHeartbeatInterval()
        {
            (PocztowyJsonResponseSession response, bool requestProcessed) sessionResponse = PerformRequest<PocztowyJsonResponseSession>(
                "smbm-services/api/auth/session", HttpMethod.Post,
                JsonConvert.SerializeObject(PocztowyJsonRequestSession.Create()),
                null, null);
            if (!sessionResponse.requestProcessed)
                return 0;

            return sessionResponse.response.data.sessionTimeout;
        }

        protected override bool TryExtendSession()
        {
            //error if called at the same time with another request
            return GetHeartbeatInterval() != 0;
        }

        protected override PocztowyJsonResponseAccountTransactions GetAccountsDetails()
        {
            (PocztowyJsonResponseAccountTransactions response, bool requestProcessed) accountsResponse = PerformRequest<PocztowyJsonResponseAccountTransactions>(
                "smbm-services/api/dashboard/accountTransactionsOthers", HttpMethod.Post,
                JsonConvert.SerializeObject(PocztowyJsonRequestAccountTransactions.Create(20, 10)),
                null, null);

            return accountsResponse.response;
        }

        protected override List<PocztowyAccountData> GetAccountsDataMainMain(PocztowyJsonResponseAccountTransactions accountsDetails)
        {
            return accountsDetails.data.accounts.Select(a => new PocztowyAccountData(a.name, a.nrb, a.balance.currencyCode, a.balance.amount) { Id = a.accountId }).ToList();
        }

        protected override bool MakeTransfer(string recipient, string address, string accountNumber, string title, double amount)
        {
            string formattedAccountNumber = accountNumber.SimplifyAccountNumber();

            (PocztowyJsonResponseTransferBank response, bool requestProcessed) bankResponse = PerformRequest<PocztowyJsonResponseTransferBank>(
                "smbm-services/api/transfer/bankData", HttpMethod.Post,
                JsonConvert.SerializeObject(PocztowyJsonRequestBank.Create(formattedAccountNumber)),
                null, null);
            if (!bankResponse.requestProcessed)
                return false;

            PocztowyJsonRequestTransferPrepare requestTransfer = new PocztowyJsonRequestTransferPrepare
            {
                TypeValue = PocztowyJsonOutgoingTransferType.TransferDomestic,
                fromProductId = SelectedAccountData.Id,
                amount = new PocztowyJsonRequestAmountCurrency() { amount = amount, currencyCode = SelectedAccountData.Currency },
                DateValue = Today,
                recipientName = recipient,
                recipientAddress = address,
                title = title,
                toNrb = formattedAccountNumber
            };

            //TODO bank name, account number
            return MakeTransferAndConfirm(requestTransfer, new ConfirmTextTransfer(amount, SelectedAccountData.Currency, null, accountNumber)) != null;
        }

        protected override bool MakeTaxTransfer(string taxType, string accountNumber, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier, string creditorName, string obligationId, double amount)
        {
            (PocztowyJsonResponseTransferParams response, bool requestProcessed) transferParamsResponse = PerformRequest<PocztowyJsonResponseTransferParams>(
                "smbm-services/api/v2/transfer/transferParams", HttpMethod.Post,
                JsonConvert.SerializeObject(PocztowyJsonRequestTransferParams.Create("TAX_TRANSFER")),
                null, null);
            if (!transferParamsResponse.requestProcessed)
                return false;

            PocztowyJsonResponseTransferParamsDataTaxFormSymbol selectedTax = transferParamsResponse.response.data.taxFormSymbols.SingleOrDefault(s => s.symbol == taxType);
            if (selectedTax == null)
                return CheckFailed("Nie znaleziono podanego typu formularza");

            (string city, PocztowyJsonResponseTransferPrepareTaxFormOfficeAccountsTaxOfficeAccount account) taxOffice = default;

            PocztowyJsonOutgoingTransferType typeValue;
            string nrb;
            string cityCode = null;
            if (selectedTax.irp)
            {
                typeValue = PocztowyJsonOutgoingTransferType.TaxIRP;
                //TODO use in other banks
                nrb = accountNumber.SimplifyAccountNumber();
            }
            else
            {
                taxOffice = PromptComboBox<(string, PocztowyJsonResponseTransferPrepareTaxFormOfficeAccountsTaxOfficeAccount)>("Urząd", transferParamsResponse.response.data.taxFormOfficeAccounts.SelectMany(o => o.taxOfficeAccounts.Select(a => (o.place, a))).Where(o => o.a.paymentMethod == selectedTax.paymentMethod).Select(o => new SelectComboBoxItem<(string, PocztowyJsonResponseTransferPrepareTaxFormOfficeAccountsTaxOfficeAccount)>($"{o.place}: {o.a.name}", o)), true).data;
                if (taxOffice == default)
                    return false;

                typeValue = PocztowyJsonOutgoingTransferType.TaxUS;
                nrb = taxOffice.account.nrb;
                cityCode = taxOffice.city;
            }

            (string symbol, string day, string decade, string month, string quarter, string halfyear, int? year) = GetTaxPeriodValue(period);

            PocztowyJsonRequestTaxTransferPrepare requestTransfer = new PocztowyJsonRequestTaxTransferPrepare
            {
                TypeValue = typeValue,
                fromProductId = SelectedAccountData.Id,
                amount = new PocztowyJsonRequestAmountCurrency() { amount = amount, currencyCode = SelectedAccountData.Currency },
                DateValue = Today,
                sendEmailConfirmation = false,
                formUSCode = taxType,
                taxpayerName = creditorName,
                cityCode = cityCode,
                toNrb = nrb,
                taxpayerIdentityTypeCode = GetTaxCreditorIdentifierTypeId(creditorIdentifier),
                taxpayerIdentity = creditorIdentifier.GetId(),
                period = symbol,
                day = day,
                halfyear = halfyear,
                year = year,
                month = month,
                decade = decade,
                quarter = quarter
            };

            return MakeTransferAndConfirm(requestTransfer, new ConfirmTextTaxTransfer(amount, SelectedAccountData.Currency, taxOffice == default ? null : $"{taxOffice.account.name} {taxOffice.city}")) != null;
        }

        public static string GetTaxCreditorIdentifierTypeId(TaxCreditorIdentifier creditorIdentifier)
        {
            if (creditorIdentifier is TaxCreditorIdentifierNIP)
                return "NIP";
            else if (creditorIdentifier is TaxCreditorIdentifierIDCard)
                return "ID";
            else if (creditorIdentifier is TaxCreditorIdentifierPESEL)
                return "PESEL";
            else if (creditorIdentifier is TaxCreditorIdentifierREGON)
                return "REGON";
            else if (creditorIdentifier is TaxCreditorIdentifierPassport)
                return "PASSPORT";
            else if (creditorIdentifier is TaxCreditorIdentifierOther)
                return "OTHER";
            else
                throw new ArgumentException();
        }

        public static (string symbol, string day, string decade, string month, string quarter, string halfyear, int? year) GetTaxPeriodValue(TaxPeriod period)
        {
            if (period is TaxPeriodDay taxPeriodDay)
                return ("DAY", taxPeriodDay.Day.Display("yyyy-MM-dd"), null, null, null, null, null);
            else if (period is TaxPeriodHalfYear taxPeriodHalfYear)
                return ("HALFYEAR", null, null, null, null, NumberToNumeral(taxPeriodHalfYear.Half), taxPeriodHalfYear.Year);
            else if (period is TaxPeriodMonth taxPeriodMonth)
                return ("MONTH", null, null, MonthToShortName(taxPeriodMonth.Month), null, null, taxPeriodMonth.Year);
            else if (period is TaxPeriodMonthDecade taxPeriodMonthDecade)
                return ("DECADE", null, NumberToNumeral(taxPeriodMonthDecade.Decade), MonthToShortName(taxPeriodMonthDecade.Month), null, null, taxPeriodMonthDecade.Year);
            else if (period is TaxPeriodQuarter taxPeriodQuarter)
                return ("QUARTER", null, null, null, $"Q{GetTaxPeriodNumberValue(taxPeriodQuarter.Quarter)}", null, taxPeriodQuarter.Year);
            else if (period is TaxPeriodYear taxPeriodYear)
                return ("YEAR", null, null, null, null, null, taxPeriodYear.Year);
            else
                throw new ArgumentException();
        }

        private static string NumberToNumeral(int number)
        {
            switch (number)
            {
                case 1:
                    return "FIRST";
                case 2:
                    return "SECOND";
                case 3:
                    return "THIRD";
                default:
                    throw new ArgumentException();
            }
        }

        private static string MonthToShortName(int month)
        {
            switch (month)
            {
                case 1:
                    return "JAN";
                case 2:
                    return "FEB";
                case 3:
                    return "MAR";
                case 4:
                    return "APR";
                case 5:
                    return "MAY";
                case 6:
                    return "JUN";
                case 7:
                    return "JUL";
                case 8:
                    return "AUG";
                case 9:
                    return "SEP";
                case 10:
                    return "OCT";
                case 11:
                    return "NOV";
                case 12:
                    return "DEC";
                default:
                    throw new ArgumentException();
            }
        }

        protected override string CleanFastTransferUrl(string transferId)
        {
            string newTransferId = transferId
                .Replace("https://", String.Empty)

                .Replace("online.pocztowy.pl/login/main?pblData=", String.Empty);

            (FastTransferType? type, string pblData) fastTransferData = GetDataFromFastTransfer(newTransferId);

            if (fastTransferData.type == null)
                return null;

            return newTransferId;
        }

        protected override bool LoginRequestForFastTransfer(string login, string password, List<object> additionalAuthorization, string transferId)
        {
            return PerformLogin(login, password, additionalAuthorization);
        }

        protected override string MakeFastTransfer(string transferId)
        {
            (PocztowyJsonResponsePayByLinkData response, bool requestProcessed) payByLinkDataResponse = PerformRequest<PocztowyJsonResponsePayByLinkData>(
                "smbm-services/api/pbl/getPayByLinkData", HttpMethod.Post,
                JsonConvert.SerializeObject(PocztowyJsonRequestPayByLink.Create(transferId)),
                null, null);
            if (!payByLinkDataResponse.requestProcessed)
                return null;

            PocztowyJsonRequestFastTransferPrepare requestTransfer = new PocztowyJsonRequestFastTransferPrepare
            {
                TypeValue = PocztowyJsonOutgoingTransferType.PayByLink,
                fromProductId = SelectedAccountData.Id,
                dataHash = transferId
            };

            PocztowyJsonResponseTransferPush confirmResponse = MakeTransferAndConfirm(requestTransfer, new ConfirmTextFastTransfer(payByLinkDataResponse.response.data.amount.amount, payByLinkDataResponse.response.data.amount.currencyCode, payByLinkDataResponse.response.data.recipientName));
            if (confirmResponse == null)
                return null;

            //return payByLinkDataResponse.response.data.redirectUrl;
            return confirmResponse.data.redirectUrl;
        }

        protected override bool MakePrepaidTransferMain(string recipient, string phoneNumber, double amount)
        {
            (PocztowyJsonResponseOperator response, bool requestProcessed) operatorResponse = PerformRequest<PocztowyJsonResponseOperator>(
                "smbm-services/api/topup/operatorData", HttpMethod.Post,
                JsonConvert.SerializeObject(PocztowyJsonRequestOperator.Create(phoneNumber)),
                (PocztowyJsonResponseOperator jsonResponseOperator) => jsonResponseOperator.errorCode == "PAY_GET_OPERATOR_DATA_ERROR",
                 null);
            if (!operatorResponse.requestProcessed)
                return false;

            (PocztowyJsonResponseOperators response, bool requestProcessed) operatorsResponse = PerformRequest<PocztowyJsonResponseOperators>(
                "smbm-services/api/topup/operators", HttpMethod.Post,
                JsonConvert.SerializeObject(PocztowyJsonRequestOperators.Create()),
                null, null);

            PocztowyJsonResponseOperatorsDataOperator operatorItem;
            if (operatorResponse.response.errorCode == "PAY_GET_OPERATOR_DATA_ERROR")
            {
                (string name, PocztowyJsonResponseOperatorsDataOperator data) selectedOperatorItem = PromptComboBox<PocztowyJsonResponseOperatorsDataOperator>("Operator", operatorsResponse.response.data.operators.Select(o => new SelectComboBoxItem<PocztowyJsonResponseOperatorsDataOperator>(o.operatorName, o)), false);
                if (selectedOperatorItem.data == null)
                    return false;

                operatorItem = selectedOperatorItem.data;
            }
            else
                operatorItem = operatorsResponse.response.data.operators.Single(o => o.operatorCode == operatorResponse.response.data.operatorCode);

            switch (operatorItem.AmountTypeValue)
            {
                case PocztowyJsonOperatorAmountType.Range:
                    if (amount < operatorItem.amountMin || amount > operatorItem.amountMax)
                        return CheckFailed($"Kwota powinna znajdować się w zakresie {operatorItem.amountMin}-{operatorItem.amountMax}");
                    break;
                case PocztowyJsonOperatorAmountType.Constant:
                    if (!operatorItem.permittedAmounts.Contains(amount))
                        return CheckFailed($"Kwota powinna być jedną z {String.Join(", ", operatorItem.permittedAmounts.Select(a => a.Display(DecimalSeparator.Dot)))}");
                    break;
                default:
                    throw new NotImplementedException();
            }

            PocztowyJsonRequestTransferPrepaidPrepare requestTransfer = new PocztowyJsonRequestTransferPrepaidPrepare
            {
                TypeValue = PocztowyJsonOutgoingTransferType.TopUp,
                fromProductId = SelectedAccountData.Id,
                amount = new PocztowyJsonRequestAmountCurrency() { amount = amount, currencyCode = SelectedAccountData.Currency },
                operatorCode = operatorItem.operatorCode,
                phoneNumber = phoneNumber
            };

            //TODO check in .har where recipient goes; is it even shown in history?
            return MakeTransferAndConfirm(requestTransfer, new ConfirmTextPrepaidTransfer(amount, SelectedAccountData.Currency, operatorItem.operatorName, phoneNumber)) != null;
        }

        protected override PocztowyHistoryFilter CreateFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact)
        {
            return new PocztowyHistoryFilter(direction, title, dateFrom, dateTo, amountExact);
        }

        protected override List<PocztowyHistoryItem> GetHistoryItems(PocztowyHistoryFilter filter = null)
        {
            List<PocztowyHistoryItem> result = new List<PocztowyHistoryItem>();

            PocztowyJsonRequestHistory requestHistory = new PocztowyJsonRequestHistory
            {
                maxRows = 30,
                productIds = new List<string>() { SelectedAccountData.Id },
                contains = filter.Title,
                FromDateValue = filter.DateFrom,
                ToDateValue = filter.DateTo
            };
            if (filter.AmountFrom != null)
                requestHistory.minAmount = new PocztowyJsonRequestAmount() { amount = (double)filter.AmountFrom };
            if (filter.AmountTo != null)
                requestHistory.maxAmount = new PocztowyJsonRequestAmount() { amount = (double)filter.AmountTo };
            if (filter.OperationType != null && filter.OperationType != PocztowyFilterOperationType.All)
                requestHistory.types = new List<string>() { AttributeOperations.GetEnumAttribute((PocztowyFilterOperationType)filter.OperationType, (FilterEnumParameterAttribute parameter) => parameter.Parameter, null) };

            (PocztowyJsonResponseHistory response, bool requestProcessed) getHistoryResponse(string cursor)
            {
                requestHistory.cursor = cursor;

                return PerformRequest<PocztowyJsonResponseHistory>(
                    "smbm-services/api/history/executed", HttpMethod.Post,
                    JsonConvert.SerializeObject(requestHistory),
                    null, null);
            }
            (PocztowyJsonResponseHistory response, bool requestProcessed) historyResponse = getHistoryResponse(null);
            if (!historyResponse.requestProcessed)
                return null;

            //TODO old transfers in other banks
            if (historyResponse.response.data.care?.care == "SENSITIVE")
            {
                PocztowyJsonRequestAttendance requestAttendance = new PocztowyJsonRequestAttendance
                {
                    TypeValue = PocztowyJsonOutgoingTransferType.Attendance,
                };

                PocztowyJsonResponseTransferPush attendanceResponse = MakeTransferAndConfirm(requestAttendance, new ConfirmTextAuthorizeGetHistory());
                if (attendanceResponse == null)
                    return null;

                historyResponse = getHistoryResponse(null);
                if (!historyResponse.requestProcessed)
                    return null;
            }

            if (historyResponse.response.data.results != null)
                result.AddRange(historyResponse.response.data.results.Select(t => new PocztowyHistoryItem(t)));

            while (historyResponse.response.data.cursor != null && (filter.CounterLimit == 0 || result.Count < filter.CounterLimit))
            {
                historyResponse = getHistoryResponse(historyResponse.response.data.cursor);
                if (!historyResponse.requestProcessed)
                    return null;

                if (historyResponse.response.data.results != null)
                    result.AddRange(historyResponse.response.data.results.Select(t => new PocztowyHistoryItem(t)));
            }

            return result;
        }

        protected override bool GetDetailsFileMain(PocztowyHistoryItem item, Func<string, FileStream> file)
        {
            PerformFileRequest("smbm-services/api/history/executed/operations/print", HttpMethod.Post, JsonConvert.SerializeObject(PocztowyJsonRequestDetailsFile.Create(item.Id)), file);

            return true;
        }

        private PocztowyJsonResponseTransferPush MakeTransferAndConfirm<Y>(Y requestTransfer, ConfirmTextBase confirmText) where Y : PocztowyJsonRequestTranferBase
        {
            (PocztowyJsonResponseTransferPrepare response, bool requestProcessed) transferResponse = PerformRequest<PocztowyJsonResponseTransferPrepare>(
                "smbm-services/api/order/prepareOrder", HttpMethod.Post,
                JsonConvert.SerializeObject(requestTransfer),
                null,
                (PocztowyJsonResponseTransferPrepare response) => response.data?.violations == null ? null : String.Join(", ", response.data.violations.Select(v => v.message)));
            if (!transferResponse.requestProcessed)
                return null;

            if (transferResponse.response.data.estimatedCharges.amount != 0)
                throw new NotSupportedException();

            switch (transferResponse.response.data.AuthorizationTypeValue)
            {
                case PocztowyJsonConfirmType.SMS:
                    return SMSConfirm<PocztowyJsonResponseTransferPush, (PocztowyJsonResponseTransferPush response, bool requestProcessed)>(
                        (string SMSCode) =>
                        {
                            PocztowyJsonRequestTransferPush<Y> requestConfirm = new PocztowyJsonRequestTransferPush<Y>
                            {
                                order = requestTransfer,
                                orderId = transferResponse.response.data.orderId,
                                password = EncryptWithRSA(sessionId + SMSCode, publicKey)
                            };

                            return PerformRequest<PocztowyJsonResponseTransferPush>(
                                "smbm-services/api/order/pushOrder", HttpMethod.Post,
                                JsonConvert.SerializeObject(requestConfirm),
                                (PocztowyJsonResponseTransferPush jsonResponseConfirm) => jsonResponseConfirm.errorCode == "SMS_CODE_INCORRECT_EXPIRED",
                                null);
                        },
                        ((PocztowyJsonResponseTransferPush response, bool requestProcessed) confirmResponse) =>
                        {
                            if (!confirmResponse.requestProcessed)
                                return false;
                            if (confirmResponse.response.errorCode == "SMS_CODE_INCORRECT_EXPIRED")
                                return null;
                            else
                                return true;
                        },
                        ((PocztowyJsonResponseTransferPush response, bool requestProcessed) confirmResponse) => confirmResponse.response,
                        ((PocztowyJsonResponseTransferPush response, bool requestProcessed) confirmResponse) => MakeTransferAndConfirm(requestTransfer, confirmText),
                        //TODO use transferResponse.response.data.estimatedCharges.amount and transferResponse.response.data.estimatedCharges.currencyCode
                        confirmText,
                        transferResponse.response.data.daySmsNumber);
                case PocztowyJsonConfirmType.Mobile:
                    return MobileConfirm<PocztowyJsonResponseTransferPush, HttpStatusCode>(
                        () =>
                        {
                            (string response, bool requestProcessed, HttpStatusCode statusCode) mobileTokenResponse = PerformPlainRequest($"preweb/authorization/mtoken/authorizations/{transferResponse.response.data.orderId}", HttpMethod.Get, true);
                            return mobileTokenResponse.statusCode;
                        },
                        (HttpStatusCode statusCode) =>
                        {
                            if (statusCode == (HttpStatusCode)422)
                                return false;
                            if (statusCode == HttpStatusCode.OK)
                                return true;

                            //HttpStatusCode.Accepted
                            return null;
                        },
                        (HttpStatusCode statusCode) =>
                        {
                            PocztowyJsonRequestTransferPush<Y> requestConfirm = new PocztowyJsonRequestTransferPush<Y>
                            {
                                order = requestTransfer,
                                orderId = transferResponse.response.data.orderId,
                                AuthorizationUsedValue = PocztowyJsonConfirmType.Mobile
                            };

                            return PerformRequest<PocztowyJsonResponseTransferPush>(
                                "smbm-services/api/order/pushOrder", HttpMethod.Post,
                                JsonConvert.SerializeObject(requestConfirm),
                                null,
                                null)
                            .Item1;
                        },
                        null,
                        confirmText);
                default:
                    throw new NotImplementedException();
            }
        }

        private (T, bool, HttpStatusCode) GetRequest<T>(
            HttpRequestMessage request,
            Func<string, T> responseStrAction) where T : class
        {
            using (HttpResponseMessage response = HttpOperations.GetResponse(Client, request))
            {
                (T, bool) result = ProcessResponse<T>(response,
                    (string responseStr) =>
                    {
                        return true;
                    },
                    responseStrAction);

                return (result.Item1, result.Item2, response.StatusCode);
            }
        }

        private (T, bool) PerformRequest<T>(string requestUri, HttpMethod method,
            string jsonContent,
            Func<T, bool> errorExclude,
            Func<T, string> invalidResponseMessage) where T : PocztowyJsonResponseBase
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, jsonContent, true))
            {
                (T response, bool requestProcessed, HttpStatusCode statusCode) result = GetRequest<T>(request, (responseStr) => JsonConvert.DeserializeObject<T>(responseStr));

                if (result.requestProcessed)
                {
                    if ((errorExclude == null || !errorExclude.Invoke(result.response)) && result.response.errorCode != null)
                    {
                        string messageContent = invalidResponseMessage?.Invoke(result.response);
                        if (messageContent == null)
                            messageContent = result.response.errorCode;
                        Message(messageContent);
                        return (null, false);
                    }
                }

                return (result.response, result.requestProcessed);
            }
        }

        private (string, bool, HttpStatusCode) PerformPlainRequest(string requestUri, HttpMethod method, bool setHeaders)
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, null, setHeaders))
                return GetRequest<string>(request, (responseStr) => responseStr);
        }

        private void PerformFileRequest(string requestUri, HttpMethod method,
            string jsonContent,
            Func<string, FileStream> fileStream)
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, jsonContent, true))
                ProcessFileStream(request, fileStream);
        }

        private HttpRequestMessage CreateHttpRequestMessage(string requestUri, HttpMethod method, string jsonContent, bool setHeaders)
        {
            List<(string name, string value)> headers = new List<(string name, string value)>();

            if (setHeaders)
            {
                headers.Add(("X-BIM-Application-Version", "4.59.23"));
                headers.Add(("X-BIM-OS-Version", $"WWW::{Constants.RealBrowserName}"));
                headers.Add(("X-BIM-CorrelationId", RandomCorrelationId()));
                if (sessionId != null)
                {
                    headers.Add(("X-BIM-SessionId", sessionId));
                    //TODO as in VeloBank.CreateHttpRequestMessage
                    headers.Add(("X-XSRF-TOKEN", xsrfToken));
                }
            }

            return HttpOperations.CreateHttpRequestMessageJson(method, requestUri, jsonContent, headers);
        }

        private static (FastTransferType? type, string pblData) GetDataFromFastTransfer(string transferId)
        {
            //TODO PA
            bool pbl = transferId?.Length == 352 || transferId?.Length == 368;
            FastTransferType? type = null;
            if (pbl)
                type = FastTransferType.PayByLink;
            return (type, transferId);
        }
    }
}
