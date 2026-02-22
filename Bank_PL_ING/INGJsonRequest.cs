using System;
using System.Runtime.Serialization;
using Tools;

namespace BankService.Bank_PL_ING
{
    public class INGJsonRequest
    {
        [DataContract]
        public abstract class INGJsonRequestBase
        {
            [DataMember] public string token { get; set; }
        }

        [DataContract]
        public class INGJsonRequestLogin
        {
            [DataMember] public INGJsonRequestLoginData data { get; set; }

            public static INGJsonRequestLogin Create(string login)
            {
                return new INGJsonRequestLogin() { data = new INGJsonRequestLoginData() { login = login } };
            }
        }

        [DataContract]
        public class INGJsonRequestLoginData
        {
            [DataMember] public string login { get; set; }
        }

        [DataContract]
        public class INGJsonRequestLoginPassword
        {
            [DataMember] public INGJsonRequestLoginPasswordData data { get; set; }

            public static INGJsonRequestLoginPassword Create(string pwdhash, string login, string transferId)
            {
                return new INGJsonRequestLoginPassword() { data = new INGJsonRequestLoginPasswordData() { pwdhash = pwdhash, login = login, polApiLoginSessionId = transferId } };
            }
        }

        [DataContract]
        public class INGJsonRequestLoginPasswordData
        {
            [DataMember] public string pwdhash { get; set; }
            [DataMember] public string login { get; set; }
            [DataMember] public string polApiLoginSessionId { get; set; }
            //[DataMember] public string di { get; set; }
        }

        [DataContract]
        public class INGJsonRequestLogout : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestLogoutData data { get; set; }

            public static INGJsonRequestLogout Create(string token)
            {
                return new INGJsonRequestLogout() { token = token, data = new INGJsonRequestLogoutData() { /*scmode = "D"*/ } };
            }
        }

        [DataContract]
        public class INGJsonRequestLogoutData
        {
            //[DataMember] public string scmode { get; set; }
        }

        [DataContract]
        public class INGJsonRequestPing : INGJsonRequestBase
        {
            public static INGJsonRequestPing Create(string token)
            {
                return new INGJsonRequestPing() { token = token };
            }
        }

        [DataContract]
        public class INGJsonRequestAccounts : INGJsonRequestBase
        {
            public static INGJsonRequestAccounts Create(string token)
            {
                return new INGJsonRequestAccounts() { token = token };
            }
        }

        [DataContract]
        public class INGJsonRequestConfirm : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestConfirmData data { get; set; }

            public static INGJsonRequestConfirm Create(string token, string docId)
            {
                return new INGJsonRequestConfirm() { token = token, data = new INGJsonRequestConfirmData() { docId = docId } };
            }
        }

        [DataContract]
        public class INGJsonRequestConfirmData
        {
            [DataMember] public string docId { get; set; }
        }

        [DataContract]
        public class INGJsonRequestAuthGetData : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestAuthGetDataData data { get; set; }

            public static INGJsonRequestAuthGetData Create(string token, string refValue)
            {
                return new INGJsonRequestAuthGetData() { token = token ?? String.Empty, data = new INGJsonRequestAuthGetDataData() { refValue = refValue } };
            }
        }

        [DataContract]
        public class INGJsonRequestAuthGetDataData
        {
            [DataMember(Name = "ref")] public string refValue { get; set; }
        }

        [DataContract]
        public class INGJsonRequestAuthConfirm : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestAuthConfirmData data { get; set; }

            public static INGJsonRequestAuthConfirm Create(string token, INGJsonAuthFactor factor, string refValue)
            {
                return new INGJsonRequestAuthConfirm() { token = token ?? String.Empty, data = new INGJsonRequestAuthConfirmData() { FactorValue = factor, refValue = refValue } };
            }
        }

        [DataContract]
        public class INGJsonRequestAuthConfirmData
        {
            [DataMember] public string factor { get; set; }
            [DataMember(Name = "ref")] public string refValue { get; set; }

            public INGJsonAuthFactor? FactorValue
            {
                get { return factor.GetEnumByJsonValue<INGJsonAuthFactor>(); }
                set { factor = value.GetEnumJsonValue<INGJsonAuthFactor>(); }
            }
        }

        [DataContract]
        public class INGJsonRequestFastTransferAuthConfirm : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestFastTransferAuthConfirmData data { get; set; }

            public static INGJsonRequestFastTransferAuthConfirm Create(string token, string docId)
            {
                return new INGJsonRequestFastTransferAuthConfirm() { token = token, data = new INGJsonRequestFastTransferAuthConfirmData() { docId = docId } };
            }
        }

        [DataContract]
        public class INGJsonRequestFastTransferAuthConfirmData
        {
            [DataMember] public string docId { get; set; }
        }

        [DataContract]
        public class INGJsonRequestAuthAutoConfirmConfirm : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestAuthAutoConfirmConfirmData data { get; set; }

            public static INGJsonRequestAuthAutoConfirmConfirm Create(string token, string refValue)
            {
                return new INGJsonRequestAuthAutoConfirmConfirm() { token = token ?? String.Empty, data = new INGJsonRequestAuthAutoConfirmConfirmData() { FactorValue = INGJsonAuthFactor.AutoConfirm, refValue = refValue } };
            }
        }

        [DataContract]
        public class INGJsonRequestAuthAutoConfirmConfirmData : INGJsonRequestAuthConfirmData
        {
        }

        [DataContract]
        public class INGJsonRequestAuthSMSConfirm : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestAuthSMSConfirmData data { get; set; }

            public static INGJsonRequestAuthSMSConfirm Create(string token, INGJsonAuthFactor factor, string refValue, string credentials)
            {
                return new INGJsonRequestAuthSMSConfirm() { token = token ?? String.Empty, data = new INGJsonRequestAuthSMSConfirmData() { FactorValue = factor, refValue = refValue, credentials = credentials } };
            }
        }

        [DataContract]
        public class INGJsonRequestAuthSMSConfirmData : INGJsonRequestAuthConfirmData
        {
            [DataMember] public string credentials { get; set; }
        }

        [DataContract]
        public class INGJsonRequestAuthAddBrowserConfirm : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestAuthAddBrowserConfirmData data { get; set; }

            public static INGJsonRequestAuthAddBrowserConfirm Create(string token, string refValue, string credentials)
            {
                return new INGJsonRequestAuthAddBrowserConfirm() { token = token ?? String.Empty, data = new INGJsonRequestAuthAddBrowserConfirmData() { FactorValue = INGJsonAuthFactor.AddBrowser, refValue = refValue, credentials = credentials ?? String.Empty } };
            }
        }

        [DataContract]
        public class INGJsonRequestAuthAddBrowserConfirmData : INGJsonRequestAuthConfirmData
        {
            [DataMember] public string credentials { get; set; }
        }

        [DataContract]
        public class INGJsonRequestAuthU2FConfirm : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestAuthU2FConfirmData data { get; set; }

            public static INGJsonRequestAuthU2FConfirm Create(string token, string refValue, string credentials)
            {
                return new INGJsonRequestAuthU2FConfirm() { token = token ?? String.Empty, data = new INGJsonRequestAuthU2FConfirmData() { FactorValue = INGJsonAuthFactor.U2F, refValue = refValue, credentials = credentials ?? String.Empty } };
            }
        }

        [DataContract]
        public class INGJsonRequestAuthU2FConfirmData : INGJsonRequestAuthConfirmData
        {
            [DataMember] public string credentials { get; set; }
        }

        [DataContract]
        public class INGJsonRequestAuthFinished : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestAuthFinishedData data { get; set; }

            public static INGJsonRequestAuthFinished Create(string token, string refValue, string dataToken)
            {
                return new INGJsonRequestAuthFinished() { token = token, data = new INGJsonRequestAuthFinishedData() { refValue = refValue, token = dataToken } };
            }
        }

        [DataContract]
        public class INGJsonRequestAuthFinishedData
        {
            [DataMember(Name = "ref")] public string refValue { get; set; }
            [DataMember] public string token { get; set; }
        }

        [DataContract]
        public class INGJsonRequestConfirmCode : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestConfirmCodeData data { get; set; }

            public static INGJsonRequestConfirmCode Create(string token, string code, string docId)
            {
                return new INGJsonRequestConfirmCode() { token = token, data = new INGJsonRequestConfirmCodeData() { code = code, docId = docId } };
            }
        }

        [DataContract]
        public class INGJsonRequestConfirmCodeData
        {
            [DataMember] public string code { get; set; }
            [DataMember] public string docId { get; set; }
        }

        [DataContract]
        public class INGJsonRequestPaymentOrder : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestPaymentOrderData data { get; set; }

            public static INGJsonRequestPaymentOrder Create(string token, double amount, string benefname1, string benefname2, string benefname3, string benefname4, string creacc, string debacc, string details1, string details2, string details3, string details4, string typtrn)
            {
                return new INGJsonRequestPaymentOrder() { token = token, data = new INGJsonRequestPaymentOrderData() { amount = amount, benefname1 = benefname1, benefname2 = benefname2, benefname3 = benefname3, benefname4 = benefname4, creacc = creacc, debacc = debacc, details1 = details1, details2 = details2, details3 = details3, details4 = details4, typtrn = typtrn } };
            }
        }

        [DataContract]
        public class INGJsonRequestPaymentOrderData
        {
            [DataMember] public double amount { get; set; }
            [DataMember] public string benefname1 { get; set; }
            [DataMember] public string benefname2 { get; set; }
            [DataMember] public string benefname3 { get; set; }
            [DataMember] public string benefname4 { get; set; }
            [DataMember] public string creacc { get; set; }
            [DataMember] public string debacc { get; set; }
            [DataMember] public string details1 { get; set; }
            [DataMember] public string details2 { get; set; }
            [DataMember] public string details3 { get; set; }
            [DataMember] public string details4 { get; set; }
            [DataMember] public string typtrn { get; set; }
        }

        [DataContract]
        public class INGJsonRequestFastTransfer : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestFastTransferData data { get; set; }

            public static INGJsonRequestFastTransfer Create(string token, string transferId)
            {
                return new INGJsonRequestFastTransfer() { token = token, data = new INGJsonRequestFastTransferData() { polApiLoginSessionId = transferId } };
            }
        }

        [DataContract]
        public class INGJsonRequestFastTransferData
        {
            [DataMember] public string polApiLoginSessionId { get; set; }
        }

        [DataContract]
        public class INGJsonRequestFastTransferPBL : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestFastTransferPBLData data { get; set; }

            public static INGJsonRequestFastTransferPBL Create(string token, string ctxId)
            {
                return new INGJsonRequestFastTransferPBL() { token = token, data = new INGJsonRequestFastTransferPBLData() { ctxId = ctxId } };
            }
        }

        [DataContract]
        public class INGJsonRequestFastTransferPBLDataConfirm : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestFastTransferPBLDataConfirmData data { get; set; }

            public static INGJsonRequestFastTransferPBLDataConfirm Create(string token, string ctxId, string debacc)
            {
                return new INGJsonRequestFastTransferPBLDataConfirm() { token = token, data = new INGJsonRequestFastTransferPBLDataConfirmData() { ctxId = ctxId, debacc = debacc } };
            }
        }

        [DataContract]
        public class INGJsonRequestFastTransferPBLDataConfirmData
        {
            [DataMember] public string ctxId { get; set; }
            [DataMember] public string debacc { get; set; }
        }

        [DataContract]
        public class INGJsonRequestFastTransferPBLData
        {
            [DataMember] public string ctxId { get; set; }
        }

        [DataContract]
        public class INGJsonRequestFastTransferConfirm : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestFastTransferPolapiauthdataConfirmData data { get; set; }

            public static INGJsonRequestFastTransferConfirm Create(string token, string authType, string debacc)
            {
                return new INGJsonRequestFastTransferConfirm() { token = token, data = new INGJsonRequestFastTransferPolapiauthdataConfirmData() { authType = authType, account = debacc } };
            }
        }
        [DataContract]
        public class INGJsonRequestFastTransferPolapiauthdataConfirmData
        {
            [DataMember] public string account { get; set; }
            //TODO enum
            [DataMember] public string authType { get; set; }
        }

        [DataContract]
        public class INGJsonRequestPaymentDirt : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestPaymentDirtData data { get; set; }

            public static INGJsonRequestPaymentDirt Create(string token, string debacc)
            {
                return new INGJsonRequestPaymentDirt() { token = token, data = new INGJsonRequestPaymentDirtData() { debacc = debacc } };
            }
        }

        [DataContract]
        public class INGJsonRequestPaymentDirtData
        {
            [DataMember] public string debacc { get; set; }
        }

        [DataContract]
        public class INGJsonRequestHistory : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestHistoryData data { get; set; }

            public static INGJsonRequestHistory Create(string token, DateTime? fromDate, DateTime? toDate, string accountId, int maxsug, INGJsonNoYes conx, string search, double minamt, double maxamt, int skipTrn, int maxTrn, INGJsonTransferSign? sign, bool? showIncomingTransfers, bool? showInternalTransfers, bool? showExternalTransfers, bool? showCardTransactionsBlocks, bool? showCardTransactions, bool? showATM, bool? showFees, bool? showSmartSaver, bool? showBlocksAndBlockReleases)
            {
                return new INGJsonRequestHistory() { token = token, data = new INGJsonRequestHistoryData() { FromDateValue = fromDate, ToDateValue = toDate, accountsIds = new string[] { accountId }, maxsug = maxsug, ConxValue = conx, search = search, minamt = minamt, maxamt = maxamt, skipTrn = skipTrn, maxTrn = maxTrn, SignValue = sign, mask = "***************", ShowIncomingTransfers = showIncomingTransfers, ShowInternalTransfers = showInternalTransfers, ShowExternalTransfers = showExternalTransfers, ShowCardTransactionsBlocks = showCardTransactionsBlocks, ShowCardTransactions = showCardTransactions, ShowATM = showATM, ShowFees = showFees, ShowSmartSaver = showSmartSaver, ShowBlocksAndBlockReleases = showBlocksAndBlockReleases } };
            }
        }

        [DataContract]
        public class INGJsonRequestHistoryData
        {
            [DataMember] public string fromDate { get; set; }
            [DataMember] public string toDate { get; set; }
            [DataMember] public string[] accountsIds { get; set; }
            [DataMember] public int maxsug { get; set; }
            [DataMember] public string conx { get; set; }
            [DataMember] public string search { get; set; }
            [DataMember] public double minamt { get; set; }
            [DataMember] public double maxamt { get; set; }
            [DataMember] public int skipTrn { get; set; }
            [DataMember] public int maxTrn { get; set; }
            [DataMember] public string sign { get; set; }
            [DataMember] public string mask { get; set; }

            public bool ShouldSerializesign()
            {
                return sign != null;
            }

            public DateTime? FromDateValue
            {
                get { return DateTime.Parse(fromDate); }
                set { fromDate = value?.Display("yyyy-MM-dd") ?? String.Empty; }
            }

            public DateTime? ToDateValue
            {
                get { return DateTime.Parse(toDate); }
                set { toDate = value?.Display("yyyy-MM-dd") ?? String.Empty; }
            }

            public INGJsonNoYes? ConxValue
            {
                get { return conx.GetEnumByJsonValue<INGJsonNoYes>(); }
                set { conx = value.GetEnumJsonValue<INGJsonNoYes>(); }
            }

            public INGJsonTransferSign? SignValue
            {
                get { return sign.GetEnumByJsonValue<INGJsonTransferSign>(); }
                set { sign = value.GetEnumJsonValue<INGJsonTransferSign>(); }
            }

            public bool? ShowIncomingTransfers
            {
                get { return GetMaskValue(0); }
                set { SetMaskValue(0, value); }
            }

            public bool? ShowInternalTransfers
            {
                get { return GetMaskValue(1); }
                set { SetMaskValue(1, value); }
            }

            public bool? ShowExternalTransfers
            {
                get { return GetMaskValue(2); }
                set { SetMaskValue(2, value); }
            }

            public bool? ShowCardTransactionsBlocks
            {
                get { return GetMaskValue(3); }
                set { SetMaskValue(3, value); }
            }

            public bool? ShowCardTransactions
            {
                get { return GetMaskValue(4); }
                set { SetMaskValue(4, value); }
            }

            public bool? ShowATM
            {
                get { return GetMaskValue(5); }
                set { SetMaskValue(5, value); }
            }

            public bool? ShowFees
            {
                get { return GetMaskValue(6); }
                set { SetMaskValue(6, value); }
            }

            //public bool? ShowTest
            //{
            //    get { return GetMaskValue(7); }
            //    set { SetMaskValue(7, value); }
            //}

            public bool? ShowSmartSaver
            {
                get { return GetMaskValue(8); }
                set { SetMaskValue(8, value); }
            }

            public bool? ShowBlocksAndBlockReleases
            {
                get { return GetMaskValue(9); }
                set { SetMaskValue(9, value); }
            }

            //public bool? ShowTest
            //{
            //    get { return GetMaskValue(10); }
            //    set { SetMaskValue(10, value); }
            //}
            //public bool? ShowTest
            //{
            //    get { return GetMaskValue(11); }
            //    set { SetMaskValue(11, value); }
            //}
            //public bool? ShowTest
            //{
            //    get { return GetMaskValue(12); }
            //    set { SetMaskValue(12, value); }
            //}
            //public bool? ShowTest
            //{
            //    get { return GetMaskValue(13); }
            //    set { SetMaskValue(13, value); }
            //}
            //public bool? ShowTest
            //{
            //    get { return GetMaskValue(14); }
            //    set { SetMaskValue(14, value); }
            //}

            private bool? GetMaskValue(int index)
            {
                switch (mask[index])
                {
                    case 'T':
                        return true;
                    case 'N':
                        return false;
                    case '*':
                        return null;
                    default:
                        throw new NotImplementedException();
                }
            }

            private void SetMaskValue(int index, bool? value)
            {
                char character;
                switch (value)
                {
                    case true:
                        character = 'T';
                        break;
                    case false:
                        character = 'N';
                        break;
                    case null:
                        character = '*';
                        break;
                    default:
                        throw new NotImplementedException();
                }
                mask = mask.SetCharAtEx(index, character);
            }
        }

        [DataContract]
        public class INGJsonRequestGsmOperators : INGJsonRequestBase
        {
            public static INGJsonRequestGsmOperators Create(string token)
            {
                return new INGJsonRequestGsmOperators() { token = token };
            }
        }

        [DataContract]
        public class INGJsonRequestGsmPreload : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestGsmPreloadData data { get; set; }

            public static INGJsonRequestGsmPreload Create(string token, int amt, string oprid, string phone, string rach)
            {
                return new INGJsonRequestGsmPreload() { token = token, data = new INGJsonRequestGsmPreloadData() { amt = amt, oprid = oprid, phone = phone, rach = rach } };
            }
        }

        [DataContract]
        public class INGJsonRequestGsmPreloadData
        {
            [DataMember] public int amt { get; set; }
            [DataMember] public string oprid { get; set; }
            [DataMember] public string phone { get; set; }
            [DataMember] public string rach { get; set; }
        }

        [DataContract]
        public class INGJsonRequestCheckAccount : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestCheckAccountData data { get; set; }

            public static INGJsonRequestCheckAccount Create(string token, string rach)
            {
                return new INGJsonRequestCheckAccount() { token = token, data = new INGJsonRequestCheckAccountData() { rach = rach } };
            }
        }

        [DataContract]
        public class INGJsonRequestCheckAccountData
        {
            [DataMember] public string rach { get; set; }
        }

        [DataContract]
        public class INGJsonRequestTransactionDetails : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestTransactionDetailsData data { get; set; }

            public static INGJsonRequestTransactionDetails Create(string token, string id)
            {
                return new INGJsonRequestTransactionDetails() { token = token, data = new INGJsonRequestTransactionDetailsData() { id = id } };
            }
        }

        [DataContract]
        public class INGJsonRequestTransactionDetailsData
        {
            [DataMember] public string id { get; set; }
        }

        [DataContract]
        public class INGJsonRequestTaxFormTypes : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestTaxFormTypesData data { get; set; }

            public static INGJsonRequestTaxFormTypes Create(string token, string sfp)
            {
                return new INGJsonRequestTaxFormTypes() { token = token, data = new INGJsonRequestTaxFormTypesData() { sfp = sfp } };
            }
        }

        [DataContract]
        public class INGJsonRequestTaxFormTypesData
        {
            [DataMember] public string sfp { get; set; }
        }

        [DataContract]
        public class INGJsonRequestTaxOffice : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestTaxOfficeData data { get; set; }

            public static INGJsonRequestTaxOffice Create(string token, string sfp, string name)
            {
                return new INGJsonRequestTaxOffice() { token = token, data = new INGJsonRequestTaxOfficeData() { sfp = sfp, name = name } };
            }
        }

        [DataContract]
        public class INGJsonRequestTaxOfficeData
        {
            [DataMember] public string name { get; set; }
            [DataMember] public string sfp { get; set; }
        }

        [DataContract]
        public class INGJsonRequestTaxTransfer : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestTaxTransferData data { get; set; }

            public static INGJsonRequestTaxTransfer Create(string token, DateTime DateValue, string debacc, double amount, string benefname1, string benefname2, string benefname3, string benefname4, string typid, string id, string sfp, string okr, string txt, string creditor1, string creditor2, string creditor3, string creditor4)
            {
                return new INGJsonRequestTaxTransfer() { token = token, data = new INGJsonRequestTaxTransferData() { DateValue = DateValue, debacc = debacc, amount = amount, benefname1 = benefname1, benefname2 = benefname2, benefname3 = benefname3, benefname4 = benefname4, typid = typid, id = id, sfp = sfp, okr = okr, txt = txt, creditor1 = creditor1, creditor2 = creditor2, creditor3 = creditor3, creditor4 = creditor4 } };
            }
        }

        [DataContract]
        public class INGJsonRequestTaxTransferData
        {
            [DataMember] public string date { get; set; }
            [DataMember] public string creacc { get; set; }
            [DataMember] public string debacc { get; set; }
            [DataMember] public double amount { get; set; }
            [DataMember] public string benefname1 { get; set; }
            [DataMember] public string benefname2 { get; set; }
            [DataMember] public string benefname3 { get; set; }
            [DataMember] public string benefname4 { get; set; }
            [DataMember] public string typid { get; set; }
            [DataMember] public string id { get; set; }
            [DataMember] public string sfp { get; set; }
            [DataMember] public string okr { get; set; }
            [DataMember] public string txt { get; set; }
            [DataMember] public string creditor1 { get; set; }
            [DataMember] public string creditor2 { get; set; }
            [DataMember] public string creditor3 { get; set; }
            [DataMember] public string creditor4 { get; set; }

            //TODO onlydate
            public DateTime? DateValue
            {
                get { return DateTime.Parse(date); }
                set { date = value?.Display("yyyy-MM-dd") ?? String.Empty; }
            }
        }

        [DataContract]
        public class INGJsonRequestTransactionPDF : INGJsonRequestTransactionDetails
        {
        }

        [DataContract]
        public class INGJsonRequestInitSession : INGJsonRequestBase
        {
            public static INGJsonRequestInitSession Create(string token)
            {
                return new INGJsonRequestInitSession() { token = token };
            }
        }

        [DataContract]
        public class INGJsonRequestGetProperties : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestGetPropertiesData data { get; set; }

            public static INGJsonRequestGetProperties Create(string token, string prefix)
            {
                return new INGJsonRequestGetProperties() { token = token, data = new INGJsonRequestGetPropertiesData() { prefix = prefix } };
            }
        }

        [DataContract]
        public class INGJsonRequestGetPropertiesData
        {
            [DataMember] public string prefix { get; set; }
        }

        [DataContract]
        public class INGJsonRequestOauth2Init
        {
            [DataMember] public INGJsonRequestOauth2InitData data { get; set; }

            public static INGJsonRequestOauth2Init Create(string refValue)
            {
                return new INGJsonRequestOauth2Init() { data = new INGJsonRequestOauth2InitData() { refValue = refValue } };
            }
        }

        [DataContract]
        public class INGJsonRequestOauth2InitData
        {
            [DataMember(Name = "ref")] public string refValue { get; set; }
        }

        [DataContract]
        public class INGJsonRequestOauth2ConfirmLogin : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestOauth2ConfirmData data { get; set; }

            public static INGJsonRequestOauth2ConfirmLogin Create(string token, string credentials, string refValue)
            {
                return new INGJsonRequestOauth2ConfirmLogin() { token = token, data = new INGJsonRequestOauth2ConfirmData() { credentials = credentials, FactorValue = INGJsonAuthFactor.Login, refValue = refValue } };
            }
        }

        [DataContract]
        public class INGJsonRequestOauth2ConfirmData : INGJsonRequestAuthConfirmData
        {
            [DataMember] public string credentials { get; set; }
        }

        [DataContract]
        public class INGJsonRequestOauth2ConfirmPassword : INGJsonRequestBase
        {
            [DataMember] public INGJsonRequestOauth2ConfirmData data { get; set; }

            public static INGJsonRequestOauth2ConfirmPassword Create(string token, string credentials, string refValue)
            {
                return new INGJsonRequestOauth2ConfirmPassword() { token = token, data = new INGJsonRequestOauth2ConfirmData() { credentials = credentials, FactorValue = INGJsonAuthFactor.Password, refValue = refValue } };
            }
        }

        [DataContract]
        public class INGJsonRequestGetToken : INGJsonRequestBase
        {
            public static INGJsonRequestGetToken Create()
            {
                return new INGJsonRequestGetToken() { };
            }
        }
    }
}
