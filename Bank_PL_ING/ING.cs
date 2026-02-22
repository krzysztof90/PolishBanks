using BankService.BankCountry;
using BankService.ConfirmText;
using BankService.LocalTools;
using BankService.SMSCodes;
using BankService.Tax.TaxCreditorIdentifiers;
using BankService.Tax.TaxPeriods;
using Fido2Authenticator;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Tools;
using Tools.Enums;
using ToolsWeb;
using static BankService.Bank_PL_ING.INGJsonRequest;
using static BankService.Bank_PL_ING.INGJsonResponse;

namespace BankService.Bank_PL_ING
{
    [BankTypeAttribute(BankType.ING)]
    public class ING : BankPoland<INGAccountData, INGHistoryItem, INGHistoryFilter, INGAccountsDetails>
    {
        //TODO behavioral tracking must be disabled? + in other banks

        private const string hexTable = "0123456789abcdef";
        private const int maxRowLength = 35;

        private string Token;
        private string LogoutUrl;
        private bool isFastTransfer;

        protected override int HeartbeatInterval => 300;
        protected override SMSCodeValidator SMSCodeValidator => new SMSCodeValidatorCypher(8);

        public override bool AllowAlternativeLoginMethod => false;

        public override bool TransferMandatoryRecipient => true;
        public override bool TransferMandatoryTitle => true;
        public override bool PrepaidTransferMandatoryRecipient => false;

        protected override string BaseAddress => "https://login.ingbank.pl/mojeing/rest/";

        protected override void CleanHttpClient()
        {
            Token = null;
            isFastTransfer = false;
        }

        protected override bool LoginRequest(string login, string password, List<object> additionalAuthorization)
        {
            return LoginRequest(login, password, null);
        }

        private bool LoginRequest(string login, string password, string transferId)
        {
            isFastTransfer = transferId != null;

            (FastTransferType? type, string pblData, string paData) fastTransferData = GetDataFromFastTransfer(transferId);

            //TODO in fast transfers, link from browser is result of location2, so there is old link but it gets redirect; now only solution is to catch old link. PBL and PA works the same way

            bool newLog = true;
            if (newLog)
            {
                string scope;
                string redirectUri;
                string custom;
                switch (fastTransferData.type)
                {
                    case null:
                        scope = "standard";
                        redirectUri = "/app/#authenticated/home/start";
                        custom = null;
                        break;
                    case FastTransferType.PayByLink:
                        scope = "payByLink";
                        redirectUri = "/paybylink/#authenticated/payment";
                        custom = $"payByLink,{transferId}";
                        break;
                    case FastTransferType.PA:
                        scope = "tppPolapi";
                        redirectUri = $"/app/#authenticated/granting/{transferId}/reloadLayout=simple";
                        custom = "dirlink";
                        break;
                    default:
                        throw new ArgumentException();
                }

                List<(string key, string value)> authorizationParameters = new List<(string key, string value)> {
                        ("scope", scope),
                        ("redirectUri", redirectUri) };
                if (custom != null)
                    authorizationParameters.Add(("custom", custom));

                string url = WebOperations.BuildUrlWithQuery(BaseAddress, "oauth2/authorization/nma",
                    authorizationParameters);

                string location = PerformPlainRequest(url, HttpMethod.Get).OriginalString;
                string location2 = PerformPlainRequest(location, HttpMethod.Get).OriginalString;
                string refValue = location2.SubstringFromEx("ref=");
                //PerformPlainRequest(location2, HttpMethod.Get);

                (INGJsonResponseOauth2Init response, bool requestProcessed) initResponse = PerformRequest<INGJsonResponseOauth2Init>(
                    "https://login.ingbank.pl/oauth2/oauth2init", HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestOauth2Init.Create(refValue)),
                    null, null);
                if (!initResponse.requestProcessed)
                    return false;

                if (initResponse.response.data.errorRedirectUrl != null)
                    return CheckFailed("Błąd logowania");

                (INGJsonResponseOauth2ConfirmLogin response, bool requestProcessed) confirmLoginResponse = PerformRequest<INGJsonResponseOauth2ConfirmLogin>(
                    "https://login.ingbank.pl/oauth2/oauth2confirm", HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestOauth2ConfirmLogin.Create(initResponse.response.data.csrfToken, login, initResponse.response.data.authorizationReference)),
                    null, null);
                if (!confirmLoginResponse.requestProcessed)
                    return false;

                string pwdHash = CreatePwdHash(confirmLoginResponse.response.data.challenge.salt, confirmLoginResponse.response.data.challenge.mask, confirmLoginResponse.response.data.challenge.key, password);
                if (pwdHash == null)
                    return false;

                (INGJsonResponseOauth2ConfirmPassword response, bool requestProcessed) confirmPasswordResponse = PerformRequest<INGJsonResponseOauth2ConfirmPassword>(
                    "https://login.ingbank.pl/oauth2/oauth2confirm", HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestOauth2ConfirmPassword.Create(initResponse.response.data.csrfToken, pwdHash, confirmLoginResponse.response.data.refValue)),
                    null, null);
                if (!confirmPasswordResponse.requestProcessed)
                    return false;

                if (confirmPasswordResponse.response.data.messageId == 10061)
                    return CheckFailed("Niepoprawny login lub hasło");

                return ConfirmNew(confirmPasswordResponse.response.data.refValue, initResponse.response.data.csrfToken, new ConfirmTextAddDevice(Constants.AppBrowserName));
            }
            else
            {
                (INGJsonResponseLogin response, bool requestProcessed) loginResponse = PerformRequest<INGJsonResponseLogin>(
                    "renchecklogin", HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestLogin.Create(login)),
                    "Niepoprawny login",
                     null);
                if (!loginResponse.requestProcessed)
                    return false;

                string pwdHash = CreatePwdHash(loginResponse.response.data.salt, loginResponse.response.data.mask, loginResponse.response.data.key, password);
                if (pwdHash == null)
                    return false;

                (INGJsonResponseLoginPassword response, bool requestProcessed) passwordResponse = PerformRequest<INGJsonResponseLoginPassword>(
                    "renlogin", HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestLoginPassword.Create(pwdHash, login, fastTransferData.type == FastTransferType.PA ? fastTransferData.paData : null)),
                    null, null);
                if (!passwordResponse.requestProcessed)
                    return false;

                bool success;
                if (!String.IsNullOrEmpty(passwordResponse.response.data.token))
                {
                    Token = passwordResponse.response.data.token;
                    success = true;
                }
                else
                    success = Confirm(passwordResponse.response.data.refValue, new ConfirmTextAddDevice(Constants.AppBrowserName));

                return success;
            }
        }

        #region Password salt
        private string CreatePwdHash(string salt, string mask, string key, string password)
        {
            string saltMask = MixSaltAndMaskData(salt, mask, password);
            if (saltMask == null)
                return null;
            return Hmac(key, saltMask);
        }

        private string Hmac(string key, string text)
        {
            int[] n = Str2BinL(key);
            if (n.Length > 16)
                n = CoreMd5(n, 8 * key.Length);
            else if (n.Length < 16)
                Array.Resize<int>(ref n, 16);
            int[] iPad = new int[16];
            int[] oPad = new int[16];
            for (int i = 0; i < 16; i++)
            {
                iPad[i] = 909522486 ^ n[i];
                oPad[i] = 1549556828 ^ n[i];
            }
            int[] hash = CoreMd5(iPad.Concat(Str2BinL(text)).ToArray(), 512 + 8 * text.Length);
            int[] l2 = CoreMd5(oPad.Concat(hash).ToArray(), 672);

            return BinL2Hex(l2);
        }

        private int[] Str2BinL(string text)
        {
            Dictionary<int, int> binDict = new Dictionary<int, int>();
            for (int i = 0; i < 8 * text.Length; i += 8)
            {
                int binValue = 0;
                if (binDict.ContainsKey(i >> 5))
                    binValue = binDict[i >> 5];
                binDict[i >> 5] = binValue | (255 & (int)text[i / 8]) << 24 - i % 32;
            }

            int[] bin = new int[binDict.Max(k => k.Key) + 1];
            for (int i = 0; i < bin.Length; i++)
                bin[i] = binDict[i];
            return bin;
        }

        private int[] CoreMd5(int[] x, int length)
        {
            int newIndex = Math.Max(length >> 5, 15 + (length + 64 >> 9 << 4));
            if (x.Length <= newIndex)
                Array.Resize<int>(ref x, newIndex + 1);
            x[length >> 5] |= 128 << 24 - length % 32;
            x[15 + (length + 64 >> 9 << 4)] = length;
            int[] t = new int[80];
            int a = 1732584193;
            int b = -271733879;
            int c = -1732584194;
            int d = 271733878;
            int e = -1009589776;
            for (int i = 0; i < x.Length; i += 16)
            {
                int oldA = a;
                int oldB = b;
                int oldC = c;
                int oldD = d;
                int oldE = e;
                for (int j = 0; j < 80; j++)
                {
                    t[j] = j < 16 ? x[i + j] : BitRol(t[j - 3] ^ t[j - 8] ^ t[j - 14] ^ t[j - 16], 1);
                    int h = SafeAdd(SafeAdd(BitRol(a, 5), HmacR(j, b, c, d)), SafeAdd(SafeAdd(e, t[j]), HmacO(j)));
                    e = d;
                    d = c;
                    c = BitRol(b, 30);
                    b = a;
                    a = h;
                }
                a = SafeAdd(a, oldA);
                b = SafeAdd(b, oldB);
                c = SafeAdd(c, oldC);
                d = SafeAdd(d, oldD);
                e = SafeAdd(e, oldE);
            }
            return new int[] { a, b, c, d, e };
        }

        private int BitRol(int num, int cnt)
        {
            return (int)((uint)(num << cnt) | ((uint)num) >> 32 - cnt);
        }

        private int HmacR(int e, int t, int n, int i)
        {
            return e < 20 ? t & n | ~t & i : e < 40 ? t ^ n ^ i : e < 60 ? t & n | t & i | n & i : t ^ n ^ i;
        }

        private int SafeAdd(int x, int y)
        {
            int lsw = (65535 & x) + (65535 & y);
            return ((x >> 16) + (y >> 16) + (lsw >> 16)) << 16 | 65535 & lsw;
        }

        private int HmacO(int e)
        {
            return e < 20 ? 1518500249 : e < 40 ? 1859775393 : e < 60 ? -1894007588 : -899497514;
        }

        private string BinL2Hex(int[] binArray)
        {
            string t = String.Empty;
            for (int i = 0; i < 4 * binArray.Length; i++)
            {
                t += hexTable[(binArray[i >> 2]) >> 8 * (3 - i % 4) + 4 & 15];
                t += hexTable[(binArray[i >> 2]) >> 8 * (3 - i % 4) & 15];
            }
            return t;
        }

        private string MixSaltAndMaskData(string salt, string mask, string password)
        {
            if (mask.LastIndexOf('*') > password.Length - 1)
            {
                CheckFailed("Niepoprawne hasło");
                return null;
            }
            StringBuilder result = new StringBuilder(salt);
            for (int i = 0; i < mask.Length; i++)
                if (mask[i] == '*')
                    result[i] = password[i];
            return result.ToString();
        }
        #endregion

        private string GetAuthorizationData(string challengeData)
        {
            return Fido2Manager.GetAuthorizationData<Fido2Options, Fido2AuthorizationData>(challengeData, "https://login.ingbank.pl",
                (Fido2Options options, string authenticatorData, string clientDataJSON, string signature, string id) =>
                {
                    Fido2AuthorizationData authorization = new Fido2AuthorizationData();
                    authorization.type = "public-key";
                    authorization.id = id;
                    authorization.rawId = id;
                    authorization.response = new AuthorizationDataResponse()
                    {
                        authenticatorData = authenticatorData,
                        clientDataJSON = clientDataJSON,
                        signature = signature,
                        userHandle = null,
                    };
                    authorization.clientExtensionResults = new AuthorizationDataClientExtensionResults()
                    {
                        appid = null,
                        appidExclude = null,
                        credProps = null,
                    };
                    return authorization;
                });
        }

        //TODO similar to AddressTools.SplitAddress ?
        private string[] SplitDescription(params string[] descriptions)
        {
            int maxNumberOfRows = 4;
            List<string> lines = new List<string>();
            foreach (string description in descriptions)
                FillLinesFromDescription(description, lines);

            if (lines.Count > maxNumberOfRows)
            {
                string description = String.Join(" ", descriptions);
                lines = new List<string>();
                FillLinesFromDescription(description, lines);
            }

            string[] result = new string[maxNumberOfRows];
            for (int i = 0; i < maxNumberOfRows; i++)
            {
                if (i < lines.Count)
                    result[i] = lines[i];
                else
                    result[i] = String.Empty;
            }

            return result;
        }

        private void FillLinesFromDescription(string description, List<string> lines)
        {
            string[] words = description.Split(' ');
            List<string> lineWords = new List<string>();
            foreach (string word in words)
            {
                if (lineWords.Sum(w => w.Length) + lineWords.Count + word.Length > maxRowLength)
                {
                    lines.Add(String.Join(" ", lineWords));
                    lineWords = new List<string>();
                }
                lineWords.Add(word);
            }
            if (lineWords.Count != 0)
                lines.Add(String.Join(" ", lineWords));
        }

        protected override bool LogoutRequest()
        {
            (INGJsonResponseLogout response, bool requestProcessed) logoutResponse = PerformRequest<INGJsonResponseLogout>(
                "renlogout", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestLogout.Create(Token)),
                null, null);
            if (!logoutResponse.requestProcessed)
                return false;

            LogoutUrl = logoutResponse.response.data.url;
            return true;
        }

        protected override bool TryExtendSession()
        {
            (INGJsonResponsePing response, bool requestProcessed) extendResponse = PerformRequest<INGJsonResponsePing>(
                "renping", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestPing.Create(Token)),
                null, null);

            return extendResponse.requestProcessed;
        }

        protected override INGAccountsDetails GetAccountsDetails()
        {
            return GetAccountsDetails(false);
        }

        private INGAccountsDetails GetAccountsDetails(bool pbl)
        {
            if (!pbl)
            {
                //TODO fastTransfer to parameter during fetching SelectedAccountData?
                //TODO unnecessary
                if (!isFastTransfer)
                {
                    (INGJsonResponseAccounts response, bool requestProcessed) accountsResponse = PerformRequest<INGJsonResponseAccounts>(
                        "rengetallingprds", HttpMethod.Post,
                        JsonConvert.SerializeObject(INGJsonRequestAccounts.Create(Token)),
                        null, null);
                    if (!accountsResponse.requestProcessed)
                        throw new NotImplementedException();

                    return new INGAccountsDetails()
                    {
                        data = new INGAccountsDetailsData()
                        {
                            accts = new INGAccountsDetailsDataAcct()
                            {
                                cur = new INGAccountsDetailsDataAcctCur()
                                {
                                    accts = accountsResponse.response.data.accts.cur.accts,
                                    total = accountsResponse.response.data.accts.cur.total
                                },
                                sav = new INGAccountsDetailsDataAcctSav()
                                {
                                    accts = accountsResponse.response.data.accts.sav.accts,
                                },
                                loan = new INGAccountsDetailsDataAcctLoan()
                                {
                                    accts = accountsResponse.response.data.accts.loan.accts,
                                    total = accountsResponse.response.data.accts.loan.total
                                },
                                vat = new INGAccountsDetailsDataAcctVat()
                                {
                                    accts = accountsResponse.response.data.accts.vat.accts,
                                },
                                total = accountsResponse.response.data.accts.total,
                            },
                            insurances = accountsResponse.response.data.insurances,
                            blik = accountsResponse.response.data.blik,
                            retirement = accountsResponse.response.data.retirement,
                            balvisible = accountsResponse.response.data.balvisible,
                            hidezeros = accountsResponse.response.data.hidezeros,
                        }
                    };
                }
                else
                {
                    //(INGJsonResponseAccounts response, bool requestProcessed) accountsResponse = PerformRequest<INGJsonResponseAccounts>(
                    //    "rengetallingprds", HttpMethod.Post,
                    //    JsonConvert.SerializeObject(INGJsonRequestAccounts.Create(Token)),
                    //    null, null, null, null);
                    //if (!accountsResponse.requestProcessed)
                    //    throw new NotImplementedException();

                    //return new INGAccountsDetails()
                    //{
                    //    data = new INGAccountsDetailsData()
                    //    {
                    //        accts = new INGAccountsDetailsDataAcct()
                    //        {
                    //            cur = new INGAccountsDetailsDataAcctCur()
                    //            {
                    //                accts = accountsResponse.response.data.accts.cur.accts,
                    //                total = accountsResponse.response.data.accts.cur.total
                    //            },
                    //            sav = new INGAccountsDetailsDataAcctSav()
                    //            {
                    //                accts = accountsResponse.response.data.accts.sav.accts,
                    //            },
                    //            loan = new INGAccountsDetailsDataAcctLoan()
                    //            {
                    //                accts = accountsResponse.response.data.accts.loan.accts,
                    //                total = accountsResponse.response.data.accts.loan.total
                    //            },
                    //            vat = new INGAccountsDetailsDataAcctVat()
                    //            {
                    //                accts = accountsResponse.response.data.accts.vat.accts,
                    //            },
                    //            total = accountsResponse.response.data.accts.total,
                    //        },
                    //        insurances = accountsResponse.response.data.insurances,
                    //        blik = accountsResponse.response.data.blik,
                    //        retirement = accountsResponse.response.data.retirement,
                    //        balvisible = accountsResponse.response.data.balvisible,
                    //        hidezeros = accountsResponse.response.data.hidezeros,
                    //    }
                    //};

                    (INGJsonResponseAccountsPBL response, bool requestProcessed) accountsResponse = PerformRequest<INGJsonResponseAccountsPBL>(
                        "rengetallaccounts", HttpMethod.Post,
                        JsonConvert.SerializeObject(INGJsonRequestAccounts.Create(Token)),
                        null, null);
                    if (!accountsResponse.requestProcessed)
                        throw new NotImplementedException();

                    return new INGAccountsDetails()
                    {
                        data = new INGAccountsDetailsData()
                        {
                            accts = new INGAccountsDetailsDataAcct()
                            {
                                cur = new INGAccountsDetailsDataAcctCur()
                                {
                                    accts = accountsResponse.response.data.cur.ToArray(),
                                    //total = 
                                },
                                sav = new INGAccountsDetailsDataAcctSav()
                                {
                                    accts = accountsResponse.response.data.sav.ToArray(),
                                },
                                loan = new INGAccountsDetailsDataAcctLoan()
                                {
                                    //accts = accountsResponse.response.data.loan.ToArray(),
                                    //total = 
                                },
                                vat = new INGAccountsDetailsDataAcctVat()
                                {
                                    //accts = accountsResponse.response.data.vat.ToArray(),
                                },
                                //total= a
                            },
                        }
                    };
                }
            }
            else
            {
                (INGJsonResponseAccountsPBL response, bool requestProcessed) accountsResponse = PerformRequest<INGJsonResponseAccountsPBL>(
                    "rengetallaccounts", HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestAccounts.Create(Token)),
                    null, null);
                if (!accountsResponse.requestProcessed)
                    throw new NotImplementedException();

                return new INGAccountsDetails()
                {
                    data = new INGAccountsDetailsData()
                    {
                        accts = new INGAccountsDetailsDataAcct()
                        {
                            cur = new INGAccountsDetailsDataAcctCur()
                            {
                                accts = accountsResponse.response.data.cur.ToArray(),
                                //total = 
                            },
                            sav = new INGAccountsDetailsDataAcctSav()
                            {
                                accts = accountsResponse.response.data.sav.ToArray(),
                            },
                            loan = new INGAccountsDetailsDataAcctLoan()
                            {
                                //accts = accountsResponse.response.data.loan.ToArray(),
                                //total = 
                            },
                            vat = new INGAccountsDetailsDataAcctVat()
                            {
                                //accts = accountsResponse.response.data.vat.ToArray(),
                            },
                            //total= a
                        },
                    }
                };
            }
        }

        protected override List<INGAccountData> GetAccountsDataMainMain(INGAccountsDetails accountsDetails)
        {
            return accountsDetails.data.accts.cur.accts.Select(a => new INGAccountData(a.name, a.acct, a.curr, a.avbal)).ToList();
        }

        public override bool MakeTransfer(string recipient, string address, string accountNumber, string title, double amount)
        {
            (INGJsonResponsePaymentCheckAccount response, bool requestProcessed) transferResponse = PerformRequest<INGJsonResponsePaymentCheckAccount>(
                "rengetacttkirinfo", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestCheckAccount.Create(Token, accountNumber.SimplifyAccountNumber())),
                null, null);
            if (!transferResponse.requestProcessed)
                return false;

            string[] benefname = SplitDescription(recipient, address);
            string[] details = SplitDescription(title);

            (INGJsonResponsePaymentConfirmable response, bool requestProcessed) paymentOrderResponse = PerformRequest<INGJsonResponsePaymentConfirmable>(
                "renpayord", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestPaymentOrder.Create(Token, amount, benefname[0], benefname[1], benefname[2], benefname[3], accountNumber.SimplifyAccountNumber(), SelectedAccountData.AccountNumber.SimplifyAccountNumber(), details[0], details[1], details[2], details[3], "S")),
                null, null);
            if (!paymentOrderResponse.requestProcessed)
                return false;

            return Confirm(paymentOrderResponse.response.data.refValue, new ConfirmTextTransfer(amount, SelectedAccountData.Currency, transferResponse.response.data.name, accountNumber.SimplifyAccountNumber()));
        }

        public override bool MakeTaxTransfer(string taxType, string accountNumber, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier, string creditorName, string obligationId, double amount)
        {
            (INGJsonResponseTaxFormTypes response, bool requestProcessed) taxFormTypesResponse = PerformRequest<INGJsonResponseTaxFormTypes>(
                "renfissfp", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestTaxFormTypes.Create(Token, "")),
                null, null);
            if (!taxFormTypesResponse.requestProcessed)
                return false;

            INGJsonResponseTaxFormTypesDataSfp selectedTax = taxFormTypesResponse.response.data.sfps.SingleOrDefault(s => s.sfp == taxType);
            if (selectedTax == null)
                return CheckFailed("Nie znaleziono podanego typu formularza");

            string[] benefname = SplitDescription();
            string[] creditor = SplitDescription(creditorName);

            INGJsonRequestTaxTransfer requestTaxTransfer = new INGJsonRequestTaxTransfer();
            requestTaxTransfer.token = Token;
            requestTaxTransfer.data = new INGJsonRequestTaxTransferData();
            requestTaxTransfer.data.DateValue = DateTime.Today;
            requestTaxTransfer.data.debacc = SelectedAccountData.AccountNumber.SimplifyAccountNumber();
            requestTaxTransfer.data.amount = amount;
            requestTaxTransfer.data.benefname1 = benefname[0];
            requestTaxTransfer.data.benefname2 = benefname[1];
            requestTaxTransfer.data.benefname3 = benefname[2];
            requestTaxTransfer.data.benefname4 = benefname[3];
            requestTaxTransfer.data.sfp = taxType;
            requestTaxTransfer.data.txt = obligationId;
            requestTaxTransfer.data.creditor1 = creditor[0];
            requestTaxTransfer.data.creditor2 = creditor[1];
            requestTaxTransfer.data.creditor3 = creditor[2];
            requestTaxTransfer.data.creditor4 = creditor[3];

            INGJsonResponseTaxAccount taxOffice = null;

            if (selectedTax.irp)
                requestTaxTransfer.data.creacc = accountNumber.SimplifyAccountNumber();
            else
            {
                if (selectedTax.fisacct != null)
                    taxOffice = selectedTax.fisacct;
                else
                {
                    (INGJsonResponseTaxOffice response, bool requestProcessed) taxOfficeResponse = PerformRequest<INGJsonResponseTaxOffice>(
                        "renfisfindacct", HttpMethod.Post,
                        JsonConvert.SerializeObject(INGJsonRequestTaxOffice.Create(Token, selectedTax.sfp, "")),
                        null, null);
                    if (!taxOfficeResponse.requestProcessed)
                        return false;

                    taxOffice = PromptComboBox<INGJsonResponseTaxAccount>("Urząd", taxOfficeResponse.response.data.accts.Select(o => new SelectComboBoxItem<INGJsonResponseTaxAccount>($"{o.name1}{o.name2}{o.city}", o)), true).data;
                }

                if (taxOffice == null)
                    return false;

                requestTaxTransfer.data.creacc = taxOffice.acct;
            }

            //if (!sfp.vat)

            requestTaxTransfer.data.typid = GetTaxCreditorIdentifierTypeId(creditorIdentifier);
            requestTaxTransfer.data.id = creditorIdentifier.GetId();

            if (selectedTax.PeriodValue == INGJsonTaxNoYes.No)
                requestTaxTransfer.data.okr = String.Empty;
            else
                requestTaxTransfer.data.okr = GetTaxPeriodValue(period);

            //if (selectedTax.OthValue != INGJsonTaxOth.N)

            (INGJsonResponseTaxTransfer response, bool requestProcessed) taxTransferResponse = PerformRequest<INGJsonResponseTaxTransfer>(
                "renfispayord", HttpMethod.Post,
                JsonConvert.SerializeObject(requestTaxTransfer),
                null, null);
            if (!taxTransferResponse.requestProcessed)
                return false;

            return Confirm(taxTransferResponse.response.data.refValue, new ConfirmTextTaxTransfer(amount, SelectedAccountData.Currency, taxOffice == null ? null : $"{taxOffice.name1} {taxOffice.name2} {taxOffice.city}"));
        }

        //TODO common parts with other banks / universal notations (uniwersal different options) ?
        public static string GetTaxCreditorIdentifierTypeId(TaxCreditorIdentifier creditorIdentifier)
        {
            if (creditorIdentifier is TaxCreditorIdentifierNIP)
                return "N";
            else if (creditorIdentifier is TaxCreditorIdentifierIDCard)
                return "1";
            else if (creditorIdentifier is TaxCreditorIdentifierPESEL)
                return "P";
            else if (creditorIdentifier is TaxCreditorIdentifierREGON)
                return "R";
            else if (creditorIdentifier is TaxCreditorIdentifierPassport)
                return "2";
            else if (creditorIdentifier is TaxCreditorIdentifierOther)
                return "3";
            else
                throw new ArgumentException();
        }

        public static string GetTaxPeriodValue(TaxPeriod period)
        {
            if (period is TaxPeriodDay taxPeriodDay)
                return $"{GetTaxPeriodYearValue(taxPeriodDay.Day.Year)}J{GetTaxPeriodNumberValue(taxPeriodDay.Day.Day)}{GetTaxPeriodNumberValue(taxPeriodDay.Day.Month)}";
            else if (period is TaxPeriodHalfYear taxPeriodHalfYear)
                return $"{GetTaxPeriodYearValue(taxPeriodHalfYear.Year)}P{GetTaxPeriodNumberValue(taxPeriodHalfYear.Half)}";
            else if (period is TaxPeriodMonth taxPeriodMonth)
                return $"{GetTaxPeriodYearValue(taxPeriodMonth.Year)}M{GetTaxPeriodNumberValue(taxPeriodMonth.Month)}";
            else if (period is TaxPeriodMonthDecade taxPeriodMonthDecade)
                return $"{GetTaxPeriodYearValue(taxPeriodMonthDecade.Year)}D{GetTaxPeriodNumberValue(taxPeriodMonthDecade.Decade)}{GetTaxPeriodNumberValue(taxPeriodMonthDecade.Month)}";
            else if (period is TaxPeriodQuarter taxPeriodQuarter)
                return $"{GetTaxPeriodYearValue(taxPeriodQuarter.Year)}K{GetTaxPeriodNumberValue(taxPeriodQuarter.Quarter)}";
            else if (period is TaxPeriodYear taxPeriodYear)
                return $"{GetTaxPeriodYearValue(taxPeriodYear.Year)}R";
            else
                throw new ArgumentException();
        }

        private static string GetTaxPeriodNumberValue(int number)
        {
            return number.ToString("D2");
        }

        private static string GetTaxPeriodYearValue(int year)
        {
            return year.ToString().SubstringFromEx(-2);
        }

        protected override string CleanFastTransferUrl(string transferId)
        {
            //TODO get from url parameter's value
            string newTransferId = transferId
                .Replace("https://", String.Empty)

                .Replace("login.ingbank.pl/mojeing/app/?#select/", String.Empty)
                .Replace("login.ingbank.pl/mojeing/app/#select/", String.Empty)

                .Replace("login.ingbank.pl/mojeing/paybylink/#login/ctxid=", String.Empty)
                .Replace("login.ingbank.pl/mojeing/paybylink/#login-new/ctxid=", String.Empty);

            (FastTransferType? type, string pblData, string paData) fastTransferData = GetDataFromFastTransfer(newTransferId);

            if (fastTransferData.type == null)
                return null;

            return newTransferId;
        }

        protected override bool LoginRequestForFastTransfer(string login, string password, List<object> additionalAuthorization, string transferId)
        {
            (FastTransferType? type, string pblData, string paData) fastTransferData = GetDataFromFastTransfer(transferId);

            return LoginRequest(login, password, transferId) && PostLoginRequest();
        }

        protected override string MakeFastTransfer(string transferId)
        {
            (FastTransferType? type, string pblData, string paData) fastTransferData = GetDataFromFastTransfer(transferId);

            if (fastTransferData.type == FastTransferType.PayByLink)
            {
                //TODO in every bank
                //TODO is it possible to chose account
                //TODO what if there is currency, which doesn't exist in any account
                //TODO available account depends on a.atrs, not on a.def?

                INGAccountsDetails accountsDetailsLocal = GetAccountsDetails(true);
                string currency = accountsDetailsLocal.data.accts.cur.accts.Cast<INGJsonResponseAccountsPBLDataAccount>().FirstOrDefault(a => a.VisibleValue == INGJsonNoYes.Yes && !a.hidden && a.def).curr;

                if (!GetAccountsDataMain(false).Any(a => a.Equals(SelectedAccountData))
                    || currency != SelectedAccountData.Currency)
                    return null;

                (INGJsonResponseFastTransferPBL response, bool requestProcessed) fastTransferPBLResponse = PerformRequest<INGJsonResponseFastTransferPBL>(
                    "rengetdirtrndata", HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestFastTransferPBL.Create(Token, fastTransferData.pblData)),
                    null, null);
                if (!fastTransferPBLResponse.requestProcessed)
                    return null;

                (INGJsonResponseFastTransferPBLDataConfirm response, bool requestProcessed) fastTransferPBLDataConfirmResponse = PerformRequest<INGJsonResponseFastTransferPBLDataConfirm>(
                    "renpaydirtrn", HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestFastTransferPBLDataConfirm.Create(Token, fastTransferData.pblData, SelectedAccountData.AccountNumber)),
                    null, null);
                if (!fastTransferPBLDataConfirmResponse.requestProcessed)
                    return null;

                if (!Confirm(fastTransferPBLDataConfirmResponse.response.data.refValue, new ConfirmTextFastTransfer(fastTransferPBLResponse.response.data.amount, currency, fastTransferPBLResponse.response.data.shopname)))
                    return null;
            }
            else if (fastTransferData.type == FastTransferType.PA)
            {
                (INGJsonResponseFastTransferPolapiauthdata response, bool requestProcessed) fastTransferPolapiauthdataResponse = PerformRequest<INGJsonResponseFastTransferPolapiauthdata>(
                    "rengetpolapiauthdata", HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestFastTransfer.Create(Token, transferId)),
                    null,
                    (INGJsonResponseFastTransferPolapiauthdata r) =>
                    {
                        switch (r.code)
                        {
                            case "6269":
                                return "Nie udało się pobrać danych. Prawdopodobnie link się przedawnił";
                            case "-2147483648":
                                return "Nie udało się pobrać danych. Prawdopodobnie zrealizowano wcześniej";
                            default:
                                return null;
                        }
                    });
                //TODO return error redirectUrl
                if (!fastTransferPolapiauthdataResponse.requestProcessed)
                    return null;

                INGJsonResponseFastTransferPolapiauthdataDataTransferAccount account = fastTransferPolapiauthdataResponse.response.data.transfer.accounts.SingleOrDefault(a => a.accountNumber == SelectedAccountData.AccountNumber);
                if (account == null)
                    //TODO + message + below
                    return null;

                if (!fastTransferPolapiauthdataResponse.response.data.transfer.accounts.Any(a => new INGAccountData(a.name, a.accountNumber, a.currency, a.availableBal).Equals(SelectedAccountData))
                        || fastTransferPolapiauthdataResponse.response.data.transfer.detail.currency != SelectedAccountData.Currency)
                    return null;

                (INGJsonResponseFastTransferPADataConfirm response, bool requestProcessed) fastTransferPolapiauthdataConfirmResponse = PerformRequest<INGJsonResponseFastTransferPADataConfirm>(
                    "renpolapiauthconfirm", HttpMethod.Post,
                    //JsonConvert.SerializeObject(INGJsonRequestFastTransferConfirm.Create(Token, "TRANSFER", SelectedAccountData.AccountNumber.SimplifyAccountNumber())),
                    JsonConvert.SerializeObject(INGJsonRequestFastTransferConfirm.Create(Token, account.authType, SelectedAccountData.AccountNumber.SimplifyAccountNumber())),
                    null, null);
                if (!fastTransferPolapiauthdataConfirmResponse.requestProcessed)
                    return null;

                if (!Confirm(fastTransferPolapiauthdataConfirmResponse.response.data.refValue, new ConfirmTextFastTransfer(fastTransferPolapiauthdataResponse.response.data.transfer.detail.amount, fastTransferPolapiauthdataResponse.response.data.transfer.detail.currency, $"{fastTransferPolapiauthdataResponse.response.data.transfer.recipient.nameAddress1} {fastTransferPolapiauthdataResponse.response.data.transfer.recipient.nameAddress2} {fastTransferPolapiauthdataResponse.response.data.transfer.recipient.nameAddress3}")))
                    return null;
            }
            else
                throw new NotImplementedException();

            Logout();
            return LogoutUrl;
        }

        protected override bool MakePrepaidTransferMain(string recipient, string phoneNumber, double amount)
        {
            //TODO + in other banks
            if (amount != Math.Truncate(amount))
                return CheckFailed("Kwota nie może zawierać miejsc po przecinku");

            (INGJsonResponseGsmOperators response, bool requestProcessed) gsmOperatorsResponse = PerformRequest<INGJsonResponseGsmOperators>(
                "rengetgsmppopr", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestGsmOperators.Create(Token)),
                null, null);
            if (!gsmOperatorsResponse.requestProcessed)
                return false;

            //TODO to INGJsonResponseGsmOperatorsDataOperator + everywhere interface with field Name, alternatively Id
            INGJsonResponseGsmOperatorsDataOperator operatorItem = PromptComboBox<INGJsonResponseGsmOperatorsDataOperator>("Operator", gsmOperatorsResponse.response.data.opers.Where(o => o.VisibleValue == INGJsonNoYes.Yes).Select(o => new SelectComboBoxItem<INGJsonResponseGsmOperatorsDataOperator>(o.name, o)), false).data;
            if (operatorItem == null)
                return false;

            switch (operatorItem.RangeValue)
            {
                case INGJsonOperatorRange.Borders:
                    if (amount < operatorItem.minAmount || amount > operatorItem.maxAmount)
                        return CheckFailed($"Kwota powinna znajdować się w zakresie {operatorItem.minAmount}-{operatorItem.maxAmount}");
                    break;
                case INGJsonOperatorRange.Enumerator:
                    if (!operatorItem.amounts.Contains(amount))
                        return CheckFailed($"Kwota powinna być jedną z {String.Join(", ", operatorItem.amounts.Select(a => a.Display(DecimalSeparator.Dot)))}");
                    break;
                default:
                    throw new NotImplementedException();
            }

            //TODO wrong phone number
            (INGJsonResponsePaymentConfirmable response, bool requestProcessed) gsmPreloadResponse = PerformRequest<INGJsonResponsePaymentConfirmable>(
                "rengsmppreload", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestGsmPreload.Create(Token, (int)amount, operatorItem.id, "+48" + phoneNumber, SelectedAccountData.AccountNumber.SimplifyAccountNumber())),
                null, null);
            if (!gsmPreloadResponse.requestProcessed)
                return false;

            return Confirm(gsmPreloadResponse.response.data.refValue, new ConfirmTextPrepaidTransfer(amount, SelectedAccountData.Currency, operatorItem.name, phoneNumber));
        }

        protected override INGHistoryFilter CreateFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact)
        {
            return new INGHistoryFilter(direction, title, dateFrom, dateTo, amountExact);
        }

        protected override List<INGHistoryItem> GetHistoryItems(INGHistoryFilter filter = null)
        {
            //TODO there can be account number in title

            //TODO displaying 15 for getinbank, for ing editable
            int maxTransactionsPerPageCount = 50;

            List<INGHistoryItem> result = new List<INGHistoryItem>();

            int? pageCounter = null;
            //TODO to method
            for (int page = 1; (pageCounter == null || page <= pageCounter) && (filter.CounterLimit == 0 || result.Count < filter.CounterLimit); page++)
            {
                INGJsonTransferSign? sign = null;
                if (filter.Direction != null)
                    sign = filter.Direction == OperationDirection.Execute ? INGJsonTransferSign.Debit : INGJsonTransferSign.Credit;
                (INGJsonResponseHistory response, bool requestProcessed) historyResponse = PerformRequest<INGJsonResponseHistory>(
                    "rengetfury", HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestHistory.Create(Token,
                        filter.DateFrom,
                        filter.DateTo,
                        SelectedAccountData.AccountNumber.SimplifyAccountNumber(),
                        5, //?
                        INGJsonNoYes.Yes, //?
                        filter.Title,
                        filter.AmountFrom ?? 0,
                        filter.AmountTo ?? 9999999999999,
                        (page - 1) * maxTransactionsPerPageCount,
                        maxTransactionsPerPageCount,
                        sign,
                        filter.ShowIncomingTransfers,
                        filter.ShowInternalTransfers,
                        filter.ShowExternalTransfers,
                        filter.ShowCardTransactionsBlocks,
                        filter.ShowCardTransactions,
                        filter.ShowATM,
                        filter.ShowFees,
                        filter.ShowSmartSaver,
                        filter.ShowBlocksAndBlockReleases
                    )),
                    null, null);
                if (!historyResponse.requestProcessed)
                    return null;

                pageCounter = (int)(Math.Ceiling(historyResponse.response.data.numtrns / (double)maxTransactionsPerPageCount));

                //TODO transaction details rengetfurydet
                result.AddRange(historyResponse.response.data.trns.Select(t => new INGHistoryItem(t.m)));
            }
            return result;
        }

        protected override bool GetDetailsFileMain(INGHistoryItem item, Func<ContentDispositionHeaderValue, FileStream> file)
        {
            (INGJsonResponseTransactionPDF response, bool requestProcessed) transactionPDFResponse = PerformRequest<INGJsonResponseTransactionPDF>(
                "renprepaccttranspdf", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestTransactionPDF.Create(Token, item.Id)),
                null, null);
            if (!transactionPDFResponse.requestProcessed)
                return false;

            string url = WebOperations.BuildUrlWithQuery(BaseAddress, "rengetbin",
                new List<(string key, string value)> { ("ref", transactionPDFResponse.response.data.refValue), ("att", "true") });
            PerformFileRequest(url, HttpMethod.Get,
                file);

            return true;
        }

        private bool Confirm(string refValue, ConfirmTextBase confirmText)
        {
            (INGJsonResponseAuthGetData response, bool requestProcessed) authGetDataResponse = PerformRequest<INGJsonResponseAuthGetData>(
                "renauthgetdata", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestAuthGetData.Create(Token, refValue)),
                null, null);
            if (!authGetDataResponse.requestProcessed)
                return false;
            if (authGetDataResponse.response.data.messageId != 0)
            {
                Message(authGetDataResponse.response.data.message);
                return false;
            }

            switch (authGetDataResponse.response.data.FactorValue)
            {
                case INGJsonAuthFactor.None:
                    {
                        if (!ConfirmWithoutFactor(confirmText))
                            return false;

                        (INGJsonResponseAuthConfirm response, bool requestProcessed) confirmResponse = PerformRequest<INGJsonResponseAuthConfirm>(
                            "renauthconfirm", HttpMethod.Post,
                            JsonConvert.SerializeObject(INGJsonRequestAuthConfirm.Create(Token, INGJsonAuthFactor.None, refValue)),
                            null, null);
                        if (!confirmResponse.requestProcessed)
                            return false;

                        return ConfirmAuthorizationFinish(refValue, confirmResponse.response.data.token, authGetDataResponse.response.data.confirmURN);
                    }
                case INGJsonAuthFactor.AutoConfirm:
                    {
                        (INGJsonResponseAuthAutoConfirmConfirm response, bool requestProcessed) confirmResponse = PerformRequest<INGJsonResponseAuthAutoConfirmConfirm>(
                            "renauthconfirm", HttpMethod.Post,
                            JsonConvert.SerializeObject(INGJsonRequestAuthAutoConfirmConfirm.Create(Token, refValue)),
                            null, null);

                        //TODO every time or only after login autenthication?
                        //TODO restrict pattern. Example values: TBN4VFFiLdynGrcM3aq, TBNCVt5miqK184De3HV
                        Cookie browserCookie = DomainCookies.GetCookie("^.{19}$", true);
                        if (browserCookie != null)
                            SaveCookie(browserCookie);

                        return ConfirmAuthorizationFinish(refValue, authGetDataResponse.response.data.token, authGetDataResponse.response.data.confirmURN);
                    }
                case INGJsonAuthFactor.SMS:
                case INGJsonAuthFactor.RedSMS:
                    {
                        (INGJsonResponseAuthSMSConfirm response, bool requestProcessed) confirmResponse = GetSMSConfirm((INGJsonAuthFactor)authGetDataResponse.response.data.FactorValue, refValue, confirmText);
                        if (confirmResponse == default)
                            return false;

                        return ConfirmAuthorizationFinish(refValue, confirmResponse.response.data.token, authGetDataResponse.response.data.confirmURN);
                    }
                case INGJsonAuthFactor.Mobile:
                    {
                        if (!ConfirmMobile(confirmText))
                        {
                            if (!authGetDataResponse.response.data.alternativeFactors.Contains("SMS") || !PromptYesNo("Czy chcesz zmienić metodę potwierdzenia na SMS?"))
                                return false;

                            (INGJsonResponseAuthChangeFactor response, bool requestProcessed) changeFactorResponse = PerformRequest<INGJsonResponseAuthChangeFactor>(
                                "renauthchangefactor", HttpMethod.Post,
                                JsonConvert.SerializeObject(INGJsonRequestAuthConfirm.Create(Token, INGJsonAuthFactor.SMS, refValue)),
                                null, null);
                            if (!changeFactorResponse.requestProcessed)
                                return false;

                            (INGJsonResponseAuthSMSConfirm response, bool requestProcessed) confirmResponse = GetSMSConfirm((INGJsonAuthFactor)changeFactorResponse.response.data.FactorValue, changeFactorResponse.response.data.refValue, confirmText);
                            if (confirmResponse == default)
                                return false;

                            return ConfirmAuthorizationFinish(changeFactorResponse.response.data.refValue, confirmResponse.response.data.token, changeFactorResponse.response.data.confirmURN);
                        }

                        return Confirm(refValue, confirmText);
                    }
                case INGJsonAuthFactor.AddBrowser:
                    {
                        //TODO dynamic name, like in Confirm
                        RemoveSavedCookie(("TBN4VFFiLdynGrcM3aq", "/mojeing", "login.ingbank.pl"));

                        string browserName = null;
                        if (PromptYesNo("Dodać przeglądarkę do zaufanych?"))
                        {
                            if (!(authGetDataResponse.response.data.challenge.acceptNewBrowsers ?? true))
                                Message("Przekroczono limit zaufanych przeglądarek");
                            else
                                browserName = Constants.AppBrowserName;
                        }

                        //bool saveBrowserCookie = browserName != null;

                        (INGJsonResponseAuthAddBrowserConfirm response, bool requestProcessed) confirmResponse = PerformRequest<INGJsonResponseAuthAddBrowserConfirm>(
                            "renauthconfirm", HttpMethod.Post,
                            JsonConvert.SerializeObject(INGJsonRequestAuthAddBrowserConfirm.Create(Token, refValue, browserName)),
                            null, null);

                        return Confirm(refValue, confirmText);
                    }
                case INGJsonAuthFactor.U2F:
                    {
                        string authorizationData = GetAuthorizationData(authGetDataResponse.response.data.challenge.webAuthToken);
                        if (authorizationData == null)
                            return false;

                        (INGJsonResponseAuthU2FConfirm response, bool requestProcessed) confirmResponse = PerformRequest<INGJsonResponseAuthU2FConfirm>(
                            "renauthconfirm", HttpMethod.Post,
                            JsonConvert.SerializeObject(INGJsonRequestAuthU2FConfirm.Create(Token, refValue, authorizationData)),
                            null, null);
                        if (!confirmResponse.requestProcessed)
                            return false;

                        return ConfirmAuthorizationFinish(refValue, confirmResponse.response.data.token, authGetDataResponse.response.data.confirmURN);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        //TODO common part with Confirm
        private bool ConfirmNew(string refValue, string token, ConfirmTextBase confirmText)
        {
            (INGJsonResponseAuthGetData response, bool requestProcessed) authGetDataResponse = PerformRequest<INGJsonResponseAuthGetData>(
                "https://login.ingbank.pl/oauth2/oauth2getauthdata", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestAuthGetData.Create(token, refValue)),
                null, null);
            if (!authGetDataResponse.requestProcessed)
                return false;
            if (authGetDataResponse.response.data.messageId != 0)
            {
                Message(authGetDataResponse.response.data.message);
                return false;
            }

            if (authGetDataResponse.response.data.finished)
                return ConfirmAuthorizationFinishNew(refValue, authGetDataResponse.response.data.token, authGetDataResponse.response.data.confirmURN);

            //TODO missing factors (None, SMS, RedSMS) only during transfers, therefore not called here?
            switch (authGetDataResponse.response.data.FactorValue)
            {
                case INGJsonAuthFactor.AutoConfirm:
                    {
                        (INGJsonResponseAuthAutoConfirmConfirm response, bool requestProcessed) confirmResponse = PerformRequest<INGJsonResponseAuthAutoConfirmConfirm>(
                            "https://login.ingbank.pl/oauth2/oauth2confirm", HttpMethod.Post,
                            JsonConvert.SerializeObject(INGJsonRequestAuthAutoConfirmConfirm.Create(token, refValue)),
                            null, null);

                        //TODO every time or only after login autenthication?
                        //TODO restrict pattern. Example values: TBN4VFFiLdynGrcM3aq, TBNCVt5miqK184De3HV
                        Cookie browserCookie = DomainCookies.GetCookie("^.{19}$", true);
                        if (browserCookie != null)
                            SaveCookie(browserCookie);

                        return ConfirmAuthorizationFinishNew(refValue, authGetDataResponse.response.data.token, authGetDataResponse.response.data.confirmURN);
                    }
                case INGJsonAuthFactor.Mobile:
                    {
                        if (!ConfirmMobile(confirmText))
                        {
                            if (!authGetDataResponse.response.data.alternativeFactors.Contains("SMS") || !PromptYesNo("Czy chcesz zmienić metodę potwierdzenia na SMS?"))
                                return false;

                            (INGJsonResponseAuthChangeFactor response, bool requestProcessed) changeFactorResponse = PerformRequest<INGJsonResponseAuthChangeFactor>(
                                "https://login.ingbank.pl/oauth2/oauth2changefactor", HttpMethod.Post,
                                JsonConvert.SerializeObject(INGJsonRequestAuthConfirm.Create(token, INGJsonAuthFactor.SMS, refValue)),
                                null, null);
                            if (!changeFactorResponse.requestProcessed)
                                return false;

                            (INGJsonResponseAuthSMSConfirm response, bool requestProcessed) confirmResponse = GetSMSConfirmNew((INGJsonAuthFactor)changeFactorResponse.response.data.FactorValue, changeFactorResponse.response.data.refValue, token, confirmText);
                            if (confirmResponse == default)
                                return false;

                            return ConfirmAuthorizationFinishNew(refValue, authGetDataResponse.response.data.token, authGetDataResponse.response.data.confirmURN);
                        }

                        return ConfirmNew(refValue, token, confirmText);
                    }
                case INGJsonAuthFactor.AddBrowser:
                    {
                        //TODO dynamic name, like in Confirm
                        RemoveSavedCookie(("TBN4VFFiLdynGrcM3aq", "/", "login.ingbank.pl"));

                        string browserName = null;
                        if (PromptYesNo("Dodać przeglądarkę do zaufanych?"))
                        {
                            if (!(authGetDataResponse.response.data.challenge.acceptNewBrowsers ?? true))
                                Message("Przekroczono limit zaufanych przeglądarek");
                            else
                                browserName = Constants.AppBrowserName;
                        }

                        (INGJsonResponseAuthAddBrowserConfirm response, bool requestProcessed) confirmResponse = PerformRequest<INGJsonResponseAuthAddBrowserConfirm>(
                            "https://login.ingbank.pl/oauth2/oauth2confirm", HttpMethod.Post,
                            JsonConvert.SerializeObject(INGJsonRequestAuthAddBrowserConfirm.Create(token, refValue, browserName)),
                            null, null);

                        return ConfirmNew(refValue, token, confirmText);
                    }
                case INGJsonAuthFactor.U2F:
                    {
                        string authorizationData = GetAuthorizationData(authGetDataResponse.response.data.challenge.webAuthToken);
                        if (authorizationData == null)
                            return false;

                        (INGJsonResponseAuthU2FConfirm response, bool requestProcessed) confirmResponse = PerformRequest<INGJsonResponseAuthU2FConfirm>(
                            "https://login.ingbank.pl/oauth2/oauth2confirm", HttpMethod.Post,
                            JsonConvert.SerializeObject(INGJsonRequestAuthU2FConfirm.Create(token, refValue, authorizationData)),
                            null, null);
                        if (!confirmResponse.requestProcessed)
                            return false;

                        return ConfirmAuthorizationFinishNew(refValue, authGetDataResponse.response.data.token, authGetDataResponse.response.data.confirmURN);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private (INGJsonResponseAuthSMSConfirm response, bool requestProcessed) GetSMSConfirm(INGJsonAuthFactor factorValue, string refValue, ConfirmTextBase confirmText)
        {
            return GetSMSConfirmBase("renauthconfirm", Token, factorValue, refValue, confirmText);
        }

        private (INGJsonResponseAuthSMSConfirm response, bool requestProcessed) GetSMSConfirmNew(INGJsonAuthFactor factorValue, string refValue, string token, ConfirmTextBase confirmText)
        {
            return GetSMSConfirmBase("https://login.ingbank.pl/oauth2/oauth2confirm", token, factorValue, refValue, confirmText);
        }

        private (INGJsonResponseAuthSMSConfirm response, bool requestProcessed) GetSMSConfirmBase(string url, string token, INGJsonAuthFactor factorValue, string refValue, ConfirmTextBase confirmText)
        {
            return SMSConfirm<(INGJsonResponseAuthSMSConfirm response, bool requestProcessed), (INGJsonResponseAuthSMSConfirm response, bool requestProcessed)>(
                (string SMSCode) =>
                {
                    return PerformRequest<INGJsonResponseAuthSMSConfirm>(
                    url, HttpMethod.Post,
                    JsonConvert.SerializeObject(INGJsonRequestAuthSMSConfirm.Create(token, factorValue, refValue, SMSCode)),
                    null, null);
                },
                ((INGJsonResponseAuthSMSConfirm response, bool requestProcessed) confirmResponse) =>
                {
                    if (!confirmResponse.requestProcessed)
                        return false;
                    if (confirmResponse.response.data.messageId != 0)
                        return null;
                    else
                        return true;
                },
                ((INGJsonResponseAuthSMSConfirm response, bool requestProcessed) confirmResponse) => confirmResponse,
                null,
                confirmText);
        }

        private bool ConfirmAuthorizationFinish(string refValue, string dataToken, string confirmUrl)
        {
            (INGJsonResponseAuthFinished response, bool requestProcessed) finishedResponse = PerformRequest<INGJsonResponseAuthFinished>(
                confirmUrl, HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestAuthFinished.Create(Token, refValue, dataToken)),
                null, null);

            if (Token == null && finishedResponse.requestProcessed)
                Token = finishedResponse.response.data.token;

            return finishedResponse.requestProcessed;
        }

        private bool ConfirmAuthorizationFinishNew(string refValue, string dataToken, string confirmUrl)
        {
            IEnumerable<KeyValuePair<string, string>> finishedParameters = new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("token", dataToken),
                new KeyValuePair<string, string>("ref", refValue),
            };

            //"https://login.ingbank.pl/oauth2/" + confirmUrl
            string location = PerformPlainRequest("https://login.ingbank.pl/oauth2/oauth2finished", HttpMethod.Post, finishedParameters).OriginalString;
            string location2 = PerformPlainRequest(location, HttpMethod.Get).OriginalString;
            //PerformPlainRequest(location2, HttpMethod.Get);

            (INGJsonResponseGetToken response, bool requestProcessed) getTokenResponse = PerformRequest<INGJsonResponseGetToken>(
                "getcsrftoken", HttpMethod.Post,
                JsonConvert.SerializeObject(INGJsonRequestGetToken.Create()),
                null, null);

            //TODO common part with ConfirmAuthorizationFinish
            if (Token == null && getTokenResponse.requestProcessed)
                Token = getTokenResponse.response.data.token;

            return getTokenResponse.requestProcessed;
        }

        private (T, bool) PerformRequest<T>(string requestUri, HttpMethod method,
            string jsonContent,
            string errorMessage,
            Func<T, string> invalidResponseMessage) where T : INGJsonResponseBase
        {
            using (HttpRequestMessage request = CreateHttpRequestMessage(requestUri, method, jsonContent))
            using (HttpResponseMessage response = HttpOperations.GetResponse(Client, request))
                return ProcessResponse<T>(response,
                        (string responseStr) =>
                        {
                            T jsonResponse = JsonConvert.DeserializeObject<T>(responseStr);

                            if (jsonResponse.StatusValue != INGJsonTransferStatus.OK)
                            {
                                string messageContent = errorMessage;
                                if (messageContent == null)
                                    messageContent = invalidResponseMessage?.Invoke(jsonResponse);
                                if (messageContent == null)
                                    messageContent = jsonResponse.msg;
                                Message(messageContent);
                                return false;
                            }
                            return true;
                        },
                        (string responseStr) => JsonConvert.DeserializeObject<T>(responseStr));
        }

        private Uri PerformPlainRequest(string requestUri, HttpMethod method, IEnumerable<KeyValuePair<string, string>> parameters = null)
        {
            using (HttpRequestMessage request = HttpOperations.CreateHttpRequestMessageForm(method, requestUri, parameters))
            using (HttpResponseMessage response = HttpOperations.GetResponse(Client, request))
            {
                return response.Headers.Location;
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
            headers.Add(("x-wolf-protection", Token ?? "0"));

            return HttpOperations.CreateHttpRequestMessageJson(method, requestUri, jsonContent, headers);
        }

        //TODO common part with Velo
        private static (FastTransferType? type, string pblData, string paData) GetDataFromFastTransfer(string transferId)
        {
            bool pbl = transferId?.Length == 32;
            bool pa = transferId?.Length == 36;
            FastTransferType? type = null;
            if (pbl)
                type = FastTransferType.PayByLink;
            if (pa)
                type = FastTransferType.PA;
            return (type, transferId, transferId);
        }
    }
}
