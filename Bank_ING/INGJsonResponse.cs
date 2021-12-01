using System;
using System.Globalization;
using System.Runtime.Serialization;
using Tools;

namespace BankService.Bank_ING
{
    public class INGJsonResponse
    {
        [DataContract]
        public class INGJsonResponseBase
        {
            [DataMember]
            public string status { get; set; }
            [DataMember]
            public string code { get; set; }
            [DataMember]
            public string msg { get; set; }

            public INGJsonResponseStatus? StatusValue
            {
                get { return status.GetEnumByJsonValue<INGJsonResponseStatus>(); }
                set { status = value.GetEnumJsonValue<INGJsonResponseStatus>(); }
            }
        }

        [DataContract]
        public class INGJsonResponseLogin : INGJsonResponseBase
        {
            [DataMember]
            public INGJsonResponseLoginData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginData
        {
            [DataMember]
            public string salt { get; set; }
            [DataMember]
            public string mask { get; set; }
            [DataMember]
            public string mobimask { get; set; }
            [DataMember]
            public string key { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginPassword : INGJsonResponseBase
        {
            [DataMember]
            public INGJsonResponseLoginPasswordData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginPasswordData
        {
            [DataMember]
            public string token { get; set; }
            [DataMember]
            public INGJsonResponseLoginPasswordDataData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginPasswordDataData
        {
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string sex { get; set; }
            [DataMember]
            public string rights { get; set; }
            [DataMember]
            public INGJsonResponseLoginPasswordDataDataFmcat[] pfmcats { get; set; }
            [DataMember]
            public INGJsonResponseLoginPasswordDataDataFmcat[] bfmcats { get; set; }
            [DataMember]
            public DateTime now { get; set; }
            [DataMember]
            public string maskedLogin { get; set; }
            [DataMember]
            public DateTime lastLogged { get; set; }
            [DataMember]
            public INGJsonResponseLoginPasswordDataDataMak mak { get; set; }
            [DataMember]
            public INGJsonResponseLoginPasswordDataDataAdobe adobe { get; set; }
            [DataMember]
            public INGJsonResponseLoginPasswordDataDataMenuEntry[] menu { get; set; }
            [DataMember]
            public INGJsonResponseLoginPasswordDataDataBadge[] badges { get; set; }
            [DataMember]
            public string assistCode { get; set; }
            [DataMember]
            public bool isSoftwareToken { get; set; }
            [DataMember]
            public string[] pendingActions { get; set; }
            [DataMember]
            public int nmsg { get; set; }
            [DataMember]
            public int nnotif { get; set; }
            [DataMember]
            public string chgpwd { get; set; }
            [DataMember]
            public string chgpin { get; set; }
            [DataMember]
            public INGJsonResponseLoginPasswordDataDataCtxinfo ctxinfo { get; set; }
            [DataMember]
            public string smartset { get; set; }
            [DataMember]
            public string kofana { get; set; }
            [DataMember]
            public string kjd { get; set; }
            [DataMember]
            public string policies { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginPasswordDataDataFmcat
        {
            [DataMember]
            public string catid { get; set; }
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string act { get; set; }
            [DataMember]
            public string grp { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginPasswordDataDataMak
        {
            [DataMember]
            public string ws { get; set; }
            [DataMember]
            public string onboarding { get; set; }
            [DataMember]
            public string access { get; set; }
            [DataMember]
            public string ngavailable { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginPasswordDataDataAdobe
        {
            [DataMember]
            public string at { get; set; }
            [DataMember(Name = "as")]
            public string As { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginPasswordDataDataMenuEntry
        {
            [DataMember]
            public string id { get; set; }
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string url { get; set; }
            [DataMember]
            public string itemClass { get; set; }
            [DataMember]
            public string iconClass { get; set; }
            [DataMember]
            public string dataTag { get; set; }
            [DataMember]
            public string[] subapps { get; set; }
            [DataMember]
            public string description { get; set; }
            [DataMember]
            public int badge { get; set; }
            [DataMember]
            public string picture { get; set; }
            [DataMember]
            public string visible { get; set; }
            [DataMember]
            public INGJsonResponseLoginPasswordDataDataMenuEntry[] entries { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginPasswordDataDataBadge
        {
            [DataMember]
            public string menuId { get; set; }
            [DataMember(Name = "new")]
            public bool New { get; set; }
            [DataMember]
            public int count { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginPasswordDataDataCtxinfo
        {
            [DataMember]
            public string curctx { get; set; }
            [DataMember]
            public string defctx { get; set; }
            [DataMember]
            public string alwctx { get; set; }
            [DataMember]
            public string broc { get; set; }
            [DataMember]
            public string setctx { get; set; }
            [DataMember]
            public string owner { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLogout : INGJsonResponseBase
        {
            [DataMember]
            public INGJsonResponseLogoutData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLogoutData
        {
            [DataMember]
            public string url { get; set; }
        }

        [DataContract]
        public class INGJsonResponseStart : INGJsonResponseBase
        {
            [DataMember]
            public INGJsonResponseStartData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponsePing : INGJsonResponseBase
        {
        }

        [DataContract]
        public class INGJsonResponseStartData
        {
            [DataMember]
            public INGJsonResponseStartDataItem[] cur { get; set; }
            [DataMember]
            public INGJsonResponseStartDataItem[] sav { get; set; }
            [DataMember]
            public INGJsonResponseStartDataItem[] loan { get; set; }
            [DataMember]
            public INGJsonResponseStartDataItem[] cards { get; set; }
            [DataMember]
            public INGJsonResponseStartDataItem[] inv { get; set; }
            [DataMember]
            public INGJsonResponseStartDataItem[] retirement { get; set; }
            [DataMember]
            public INGJsonResponseStartDataItem[] broc { get; set; }
            [DataMember]
            public INGJsonResponseStartDataItem[] insurance { get; set; }
        }

        [DataContract]
        public class INGJsonResponseStartDataItem
        {
            [DataMember]
            public string icon { get; set; }
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public INGJsonResponseStartDataItemProduct[] products { get; set; }
        }

        [DataContract]
        public class INGJsonResponseStartDataItemProduct
        {
            [DataMember]
            public string name { get; set; }
            [DataMember(Name = "adobe-tag")]
            public string adobeTag { get; set; }
            [DataMember(Name = "internal-tag")]
            public string internalTag { get; set; }
            [DataMember]
            public INGJsonResponseStartDataItemProductItem[] items { get; set; }
        }

        [DataContract]
        public class INGJsonResponseStartDataItemProductItem
        {
            [DataMember]
            public string title { get; set; }
            [DataMember]
            public INGJsonResponseStartDataItemProductItemAction[] actions { get; set; }
        }

        [DataContract]
        public class INGJsonResponseStartDataItemProductItemAction
        {
            [DataMember]
            public int order { get; set; }
            [DataMember]
            public string type { get; set; }
            [DataMember]
            public string action { get; set; }
            [DataMember]
            public string label { get; set; }
            [DataMember(Name = "class")]
            public string Class { get; set; }
            [DataMember(Name = "params")]
            public INGJsonResponseStartDataItemProductItemActionParams Params { get; set; }
        }

        [DataContract]
        public class INGJsonResponseStartDataItemProductItemActionParams
        {
            [DataMember]
            public INGJsonResponseStartDataItemProductItemActionParamsProcessParams processParams { get; set; }
        }

        [DataContract]
        public class INGJsonResponseStartDataItemProductItemActionParamsProcessParams
        {
            [DataMember]
            public string p_rortype { get; set; }
            [DataMember]
            public string p_curr { get; set; }
            [DataMember]
            public string p_href { get; set; }
            [DataMember]
            public string p_speid { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccounts : INGJsonResponseBase
        {
            [DataMember]
            public INGJsonResponseAccountsData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsData
        {
            [DataMember]
            public INGJsonResponseAccountsDataAcct accts { get; set; }
            [DataMember]
            public INGJsonResponseAccountsDataInsurances insurances { get; set; }
            [DataMember]
            public string blik { get; set; }
            [DataMember]
            public INGJsonResponseAccountsDataRetirement retirement { get; set; }
            [DataMember]
            public string balvisible { get; set; }
            [DataMember]
            public string hidezeros { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataAcct
        {
            [DataMember]
            public INGJsonResponseAccountsDataTotal[] total { get; set; }
            [DataMember]
            public INGJsonResponseAccountsDataAcctCur cur { get; set; }
            [DataMember]
            public INGJsonResponseAccountsDataAcctSav sav { get; set; }
            [DataMember]
            public INGJsonResponseAccountsDataAcctLoan loan { get; set; }
            [DataMember]
            public INGJsonResponseAccountsDataAcctVat vat { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataTotal
        {
            [DataMember]
            public double amt { get; set; }
            [DataMember]
            public string curr { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataAcctCur
        {
            [DataMember]
            public INGJsonResponseAccountsDataAcctCurAcct[] accts { get; set; }
            [DataMember]
            public INGJsonResponseAccountsDataTotal[] total { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataAcctCurAcct
        {
            [DataMember]
            public string type { get; set; }
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public double avbal { get; set; }
            [DataMember]
            public string curr { get; set; }
            [DataMember]
            public double plnbal { get; set; }
            [DataMember]
            public string acct { get; set; }
            [DataMember]
            public string atrs { get; set; }
            [DataMember]
            public string visible { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataAcctSav
        {
            [DataMember]
            public string[] accts { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataAcctLoan
        {
            [DataMember]
            public INGJsonResponseAccountsDataAcctLoanAcct accts { get; set; }
            [DataMember]
            public INGJsonResponseAccountsDataAcctLoanLeases leases { get; set; }
            [DataMember]
            public INGJsonResponseAccountsDataTotal[] total { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataAcctLoanAcct
        {
            [DataMember]
            public string[] items { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataAcctLoanLeases
        {
            [DataMember]
            public string[] items { get; set; }
            [DataMember]
            public INGJsonResponseAccountsDataTotal[] total { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataAcctVat
        {
            [DataMember]
            public string[] accts { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataInsurances
        {
            [DataMember]
            public string[] insurances { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataRetirement
        {
            [DataMember]
            public INGJsonResponseAccountsDataTotal[] total { get; set; }
            [DataMember]
            public INGJsonResponseAccountsDataTotalIKZE IKZE { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataTotalIKZE
        {
            [DataMember]
            public string[] accts { get; set; }
        }

        [DataContract]
        public class INGJsonResponsePaymentConfirmable : INGJsonResponseBase
        {
            [DataMember]
            public INGJsonResponsePaymentConfirmableData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponsePaymentConfirmableData
        {
            [DataMember]
            public string docId { get; set; }
            [DataMember]
            public string mode { get; set; }

            public INGJsonResponseOrderMode? ModeValue
            {
                get { return mode.GetEnumByJsonValue<INGJsonResponseOrderMode>(); }
                set { mode = value.GetEnumJsonValue<INGJsonResponseOrderMode>(); }
            }
        }

        [DataContract]
        public class INGJsonResponseConfirm : INGJsonResponseBase
        {
            [DataMember]
            public INGJsonResponseConfirmData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseConfirmData
        {
            [DataMember]
            public string trnsts { get; set; }
            [DataMember]
            public string trnmsg { get; set; }
        }

        [DataContract]
        public class INGJsonResponseConfirmMobile : INGJsonResponsePaymentConfirmable
        {
        }

        [DataContract]
        public class INGJsonResponseFastTransfer : INGJsonResponseBase
        {
            [DataMember]
            public INGJsonResponseFastTransferData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseFastTransferData
        {
            [DataMember]
            public string shopid { get; set; }
            [DataMember]
            public string trnref { get; set; }
            [DataMember]
            public double amount { get; set; }
            [DataMember]
            public string trntm { get; set; }
            [DataMember]
            public string title { get; set; }
            [DataMember]
            public string merchant { get; set; }
            [DataMember]
            public string shopname { get; set; }
            [DataMember]
            public string shopacct { get; set; }
            [DataMember]
            public string shoployal { get; set; }
        }

        [DataContract]
        public class INGJsonResponseGsmOperators : INGJsonResponseBase
        {
            [DataMember]
            public INGJsonResponseGsmOperatorsData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseGsmOperatorsData
        {
            [DataMember]
            public INGJsonResponseGsmOperatorsDataOperator[] opers { get; set; }
        }

        [DataContract]
        public class INGJsonResponseGsmOperatorsDataOperator
        {
            [DataMember]
            public string id { get; set; }
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string visible { get; set; }
            [DataMember]
            public string range { get; set; }
            [DataMember]
            public double minAmount { get; set; }
            [DataMember]
            public double maxAmount { get; set; }
            [DataMember]
            public double[] amounts { get; set; }

            public INGJsonResponseNoYes? VisibleValue
            {
                get { return visible.GetEnumByJsonValue<INGJsonResponseNoYes>(); }
                set { visible = value.GetEnumJsonValue<INGJsonResponseNoYes>(); }
            }

            public INGJsonResponseRange? RangeValue
            {
                get { return range.GetEnumByJsonValue<INGJsonResponseRange>(); }
                set { range = value.GetEnumJsonValue<INGJsonResponseRange>(); }
            }
        }

        [DataContract]
        public class INGJsonResponseHistory : INGJsonResponseBase
        {
            [DataMember]
            public INGJsonResponseHistoryData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseHistoryData
        {
            [DataMember]
            public int numtrns { get; set; }
            [DataMember]
            public double minAmt { get; set; }
            [DataMember]
            public double maxAmt { get; set; }
            [DataMember]
            public INGJsonResponseHistoryDataTransaction[] trns { get; set; }
            [DataMember]
            public string attrs { get; set; }
            [DataMember]
            public INGJsonResponseHistoryDataSum sum { get; set; }
        }

        [DataContract]
        public class INGJsonResponseHistoryDataTransaction
        {
            [DataMember]
            public INGJsonResponseHistoryDataTransactionM m { get; set; }
        }

        [DataContract]
        public class INGJsonResponseHistoryDataTransactionM
        {
            [DataMember]
            public string keraaja { get; set; }
            [DataMember]
            public string id { get; set; }
            [DataMember]
            public string date { get; set; }
            [DataMember]
            public int typ { get; set; }
            [DataMember]
            public string tpn { get; set; }
            [DataMember]
            public string uo { get; set; }
            [DataMember]
            public string aw { get; set; }
            [DataMember]
            public string awn { get; set; }
            [DataMember]
            public string w1 { get; set; }
            [DataMember]
            public string w2 { get; set; }
            [DataMember]
            public string w3 { get; set; }
            [DataMember]
            public string w4 { get; set; }
            [DataMember]
            public string am { get; set; }
            [DataMember]
            public string amn { get; set; }
            [DataMember]
            public string m1 { get; set; }
            [DataMember]
            public double amt { get; set; }
            [DataMember]
            public string cr { get; set; }
            [DataMember]
            public double bal { get; set; }
            [DataMember]
            public string t1 { get; set; }
            [DataMember]
            public string t2 { get; set; }
            [DataMember]
            public string t3 { get; set; }
            [DataMember]
            public string t4 { get; set; }
            [DataMember]
            public string fl { get; set; }
            [DataMember]
            public string rfl { get; set; }
            [DataMember]
            public int pfi { get; set; }
            [DataMember]
            public int pfs { get; set; }
            [DataMember]
            public string st { get; set; }
            [DataMember]
            public string itt { get; set; }
            [DataMember]
            public string jtr { get; set; }

            public INGJsonResponseNoYes? KeraajaValue
            {
                get { return keraaja.GetEnumByJsonValue<INGJsonResponseNoYes>(); }
                set { keraaja = value.GetEnumJsonValue<INGJsonResponseNoYes>(); }
            }

            public DateTime? DateValue
            {
                get { return DateTime.Parse(date); }
                set { date = value?.ToString("yyyy-MM-dd", CultureInfo.CreateSpecificCulture("es-ES")) ?? String.Empty; }
            }

            public INGJsonResponseType? TypeValue
            {
                get { return typ.GetEnumByJsonValue<INGJsonResponseType>(); }
                set { typ = value.GetEnumJsonValueInt<INGJsonResponseType>(); }
            }

            public INGJsonResponseCreditDebit? CreditDebitValue
            {
                get { return uo.GetEnumByJsonValue<INGJsonResponseCreditDebit>(); }
                set { uo = value.GetEnumJsonValue<INGJsonResponseCreditDebit>(); }
            }

            //public INGJsonResponseCategory? CategoryValue
            //{
            //    get { return pfi.GetEnumByJsonValue<INGJsonResponseCategory>(); }
            //    set { pfi = value.GetEnumJsonValueInt<INGJsonResponseCategory>(); }
            //}

            public INGJsonResponseNoYes? StValue
            {
                get { return st.GetEnumByJsonValue<INGJsonResponseNoYes>(); }
                set { st = value.GetEnumJsonValue<INGJsonResponseNoYes>(); }
            }

            public INGJsonResponseNoYes? IttValue
            {
                get { return itt.GetEnumByJsonValue<INGJsonResponseNoYes>(); }
                set { itt = value.GetEnumJsonValue<INGJsonResponseNoYes>(); }
            }

            public INGJsonResponseNoYes? JtrValue
            {
                get { return jtr.GetEnumByJsonValue<INGJsonResponseNoYes>(); }
                set { jtr = value.GetEnumJsonValue<INGJsonResponseNoYes>(); }
            }
        }

        [DataContract]
        public class INGJsonResponseHistoryDataSum
        {
            [DataMember]
            public INGJsonResponseHistoryDataSumTot[] tot { get; set; }
            [DataMember]
            public int numcred { get; set; }
            [DataMember]
            public int numdebs { get; set; }
        }

        [DataContract]
        public class INGJsonResponseHistoryDataSumTot
        {
            [DataMember]
            public string curr { get; set; }
            [DataMember]
            public int numdebs { get; set; }
            [DataMember]
            public double totdebs { get; set; }
            [DataMember]
            public int numcred { get; set; }
            [DataMember]
            public double totcred { get; set; }
        }

        [DataContract]
        public class INGJsonResponsePaymentCheckAccount : INGJsonResponseBase
        {
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string addr { get; set; }
            [DataMember]
            public string accttype { get; set; }
        }

        [DataContract]
        public class INGJsonResponseTransactionPDF : INGJsonResponseBase
        {
            [DataMember]
            public INGJsonResponseTransactionPDFData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseTransactionPDFData
        {
            [DataMember(Name = "ref")]
            public string refValue { get; set; }
        }
    }
}
