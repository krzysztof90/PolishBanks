using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Tools;

namespace BankService.Bank_PL_ING
{
    public class INGJsonResponse
    {
        [DataContract]
        public class INGJsonResponseBase
        {
            [DataMember] public string status { get; set; }
            [DataMember] public string code { get; set; }
            [DataMember] public string msg { get; set; }

            public INGJsonTransferStatus? StatusValue
            {
                get { return status.GetEnumByJsonValue<INGJsonTransferStatus>(); }
                set { status = value.GetEnumJsonValue<INGJsonTransferStatus>(); }
            }
        }

        [DataContract]
        public class INGJsonResponseLogin : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseLoginData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginData
        {
            [DataMember] public string salt { get; set; }
            [DataMember] public string mask { get; set; }
            [DataMember] public string mobimask { get; set; }
            [DataMember] public string key { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginPassword : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseLoginPasswordData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginPasswordData : INGJsonResponsePaymentConfirmableData
        {
            [DataMember] public string token { get; set; }
            [DataMember] public INGJsonResponseLoginPasswordDataData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginPasswordDataData
        {
            [DataMember] public string name { get; set; }
            [DataMember] public string sex { get; set; }
            [DataMember] public string rights { get; set; }
            [DataMember] public INGJsonResponseLoginPasswordDataDataFmcat[] pfmcats { get; set; }
            [DataMember] public INGJsonResponseLoginPasswordDataDataFmcat[] bfmcats { get; set; }
            [DataMember] public DateTime now { get; set; }
            [DataMember] public string maskedLogin { get; set; }
            [DataMember] public DateTime lastLogged { get; set; }
            [DataMember] public INGJsonResponseLoginPasswordDataDataMak mak { get; set; }
            [DataMember] public INGJsonResponseLoginPasswordDataDataAdobe adobe { get; set; }
            [DataMember] public INGJsonResponseLoginPasswordDataDataMenuEntry[] menu { get; set; }
            [DataMember] public INGJsonResponseLoginPasswordDataDataBadge[] badges { get; set; }
            [DataMember] public string assistCode { get; set; }
            [DataMember] public bool isSoftwareToken { get; set; }
            [DataMember] public string[] pendingActions { get; set; }
            [DataMember] public int nmsg { get; set; }
            [DataMember] public int nnotif { get; set; }
            [DataMember] public string chgpwd { get; set; }
            [DataMember] public string chgpin { get; set; }
            [DataMember] public INGJsonResponseLoginPasswordDataDataCtxinfo ctxinfo { get; set; }
            [DataMember] public string smartset { get; set; }
            [DataMember] public string kofana { get; set; }
            [DataMember] public string kjd { get; set; }
            [DataMember] public string policies { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginPasswordDataDataFmcat
        {
            [DataMember] public string catid { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string act { get; set; }
            [DataMember] public string grp { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginPasswordDataDataMak
        {
            [DataMember] public string ws { get; set; }
            [DataMember] public string onboarding { get; set; }
            [DataMember] public string access { get; set; }
            [DataMember] public string ngavailable { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginPasswordDataDataAdobe
        {
            [DataMember] public string at { get; set; }
            [DataMember(Name = "as")] public string asValue { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginPasswordDataDataMenuEntry
        {
            [DataMember] public string id { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string url { get; set; }
            [DataMember] public string itemClass { get; set; }
            [DataMember] public string iconClass { get; set; }
            [DataMember] public string dataTag { get; set; }
            [DataMember] public string[] subapps { get; set; }
            [DataMember] public string description { get; set; }
            [DataMember] public int badge { get; set; }
            [DataMember] public string picture { get; set; }
            [DataMember] public string visible { get; set; }
            [DataMember] public INGJsonResponseLoginPasswordDataDataMenuEntry[] entries { get; set; }

            public INGJsonNoYes? VisibleValue
            {
                get { return visible.GetEnumByJsonValue<INGJsonNoYes>(); }
                set { visible = value.GetEnumJsonValue<INGJsonNoYes>(); }
            }
        }

        [DataContract]
        public class INGJsonResponseLoginPasswordDataDataBadge
        {
            [DataMember] public string menuId { get; set; }
            [DataMember(Name = "new")] public bool newValue { get; set; }
            [DataMember] public int count { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLoginPasswordDataDataCtxinfo
        {
            [DataMember] public string curctx { get; set; }
            [DataMember] public string defctx { get; set; }
            [DataMember] public string alwctx { get; set; }
            [DataMember] public string broc { get; set; }
            [DataMember] public string setctx { get; set; }
            [DataMember] public string owner { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLogout : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseLogoutData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseLogoutData
        {
            [DataMember] public string url { get; set; }
        }

        [DataContract]
        public class INGJsonResponseStart : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseStartData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponsePing : INGJsonResponseBase
        {
        }

        [DataContract]
        public class INGJsonResponseStartData
        {
            [DataMember] public INGJsonResponseStartDataItem[] cur { get; set; }
            [DataMember] public INGJsonResponseStartDataItem[] sav { get; set; }
            [DataMember] public INGJsonResponseStartDataItem[] loan { get; set; }
            [DataMember] public INGJsonResponseStartDataItem[] cards { get; set; }
            [DataMember] public INGJsonResponseStartDataItem[] inv { get; set; }
            [DataMember] public INGJsonResponseStartDataItem[] retirement { get; set; }
            [DataMember] public INGJsonResponseStartDataItem[] broc { get; set; }
            [DataMember] public INGJsonResponseStartDataItem[] insurance { get; set; }
        }

        [DataContract]
        public class INGJsonResponseStartDataItem
        {
            [DataMember] public string icon { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public INGJsonResponseStartDataItemProduct[] products { get; set; }
        }

        [DataContract]
        public class INGJsonResponseStartDataItemProduct
        {
            [DataMember] public string name { get; set; }
            [DataMember(Name = "adobe-tag")] public string adobeTag { get; set; }
            [DataMember(Name = "internal-tag")] public string internalTag { get; set; }
            [DataMember] public INGJsonResponseStartDataItemProductItem[] items { get; set; }
        }

        [DataContract]
        public class INGJsonResponseStartDataItemProductItem
        {
            [DataMember] public string title { get; set; }
            [DataMember] public INGJsonResponseStartDataItemProductItemAction[] actions { get; set; }
        }

        [DataContract]
        public class INGJsonResponseStartDataItemProductItemAction
        {
            [DataMember] public int order { get; set; }
            [DataMember] public string type { get; set; }
            [DataMember] public string action { get; set; }
            [DataMember] public string label { get; set; }
            [DataMember(Name = "class")] public string classValue { get; set; }
            [DataMember(Name = "params")] public INGJsonResponseStartDataItemProductItemActionParams paramsValue { get; set; }
        }

        [DataContract]
        public class INGJsonResponseStartDataItemProductItemActionParams
        {
            [DataMember] public INGJsonResponseStartDataItemProductItemActionParamsProcessParams processParams { get; set; }
        }

        [DataContract]
        public class INGJsonResponseStartDataItemProductItemActionParamsProcessParams
        {
            [DataMember] public string p_rortype { get; set; }
            [DataMember] public string p_curr { get; set; }
            [DataMember] public string p_href { get; set; }
            [DataMember] public string p_speid { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccounts : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseAccountsData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsData
        {
            [DataMember] public INGJsonResponseAccountsDataAcct accts { get; set; }
            [DataMember] public INGJsonResponseAccountsDataInsurances insurances { get; set; }
            [DataMember] public string blik { get; set; }
            [DataMember] public INGJsonResponseAccountsDataRetirement retirement { get; set; }
            [DataMember] public string balvisible { get; set; }
            [DataMember] public string hidezeros { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsPBL : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseAccountsPBLData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsPBLData
        {
            [DataMember] public List<INGJsonResponseAccountsPBLDataAccount> cur { get; set; }
            [DataMember] public List<INGJsonResponseAccountsPBLDataAccount> sav { get; set; }
            [DataMember] public List<INGJsonResponseAccountsPBLDataAccount> loan { get; set; }
            [DataMember] public List<INGJsonResponseAccountsPBLDataAccount> vat { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsPBLDataAccount : INGJsonResponseDataAcctAcct
        {
            [DataMember] public string opndate { get; set; }
            [DataMember] public bool def { get; set; }
            [DataMember] public int priority { get; set; }
            [DataMember] public bool hidden { get; set; }
            [DataMember] public string ctx { get; set; }
            [DataMember] public INGJsonResponseAccountsPBLDataAccountGInfo ginfo { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsPBLDataAccountGInfo
        {
            [DataMember] public string allowed { get; set; }
            [DataMember] public string exists { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataAcct
        {
            [DataMember] public INGJsonResponseAccountsDataTotal[] total { get; set; }
            [DataMember] public INGJsonResponseAccountsDataAcctCur cur { get; set; }
            [DataMember] public INGJsonResponseAccountsDataAcctSav sav { get; set; }
            [DataMember] public INGJsonResponseAccountsDataAcctLoan loan { get; set; }
            [DataMember] public INGJsonResponseAccountsDataAcctVat vat { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataTotal
        {
            [DataMember] public double amt { get; set; }
            [DataMember] public string curr { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataAcctCur
        {
            [DataMember] public INGJsonResponseAccountsDataAcctAcct[] accts { get; set; }
            [DataMember] public INGJsonResponseAccountsDataTotal[] total { get; set; }
        }

        [DataContract]
        public class INGJsonResponseDataAcctAcct
        {
            [DataMember] public string type { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public double avbal { get; set; }
            [DataMember] public string curr { get; set; }
            [DataMember] public string acct { get; set; }
            [DataMember] public string atrs { get; set; }
            [DataMember] public string visible { get; set; }

            public INGJsonNoYes? VisibleValue
            {
                get { return visible.GetEnumByJsonValue<INGJsonNoYes>(); }
                set { visible = value.GetEnumJsonValue<INGJsonNoYes>(); }
            }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataAcctAcct : INGJsonResponseDataAcctAcct
        {
            [DataMember] public double plnbal { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataAcctSav
        {
            [DataMember] public INGJsonResponseAccountsDataAcctAcct[] accts { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataAcctLoan
        {
            [DataMember] public INGJsonResponseAccountsDataAcctLoanAcct accts { get; set; }
            [DataMember] public INGJsonResponseAccountsDataAcctLoanLeases leases { get; set; }
            [DataMember] public INGJsonResponseAccountsDataTotal[] total { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataAcctLoanAcct
        {
            [DataMember] public string[] items { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataAcctLoanLeases
        {
            [DataMember] public string[] items { get; set; }
            [DataMember] public INGJsonResponseAccountsDataTotal[] total { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataAcctVat
        {
            [DataMember] public string[] accts { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataInsurances
        {
            [DataMember] public INGJsonResponseAccountsDataInsurancesInsurance[] insurances { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataInsurancesInsurance
        {
            [DataMember] public long number { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string prdName { get; set; }
            [DataMember] public bool emuInsurance { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataRetirement
        {
            [DataMember] public INGJsonResponseAccountsDataTotal[] total { get; set; }
            [DataMember] public INGJsonResponseAccountsDataTotalIKZE IKZE { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAccountsDataTotalIKZE
        {
            [DataMember] public string[] accts { get; set; }
        }

        [DataContract]
        public class INGJsonResponsePaymentConfirmable : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponsePaymentConfirmableData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponsePaymentConfirmableData
        {
            [DataMember] public string docId { get; set; }
            [DataMember] public string mode { get; set; }
            [DataMember(Name = "ref")] public string refValue { get; set; }
            [DataMember] public string authorizationReference { get; set; }

            public INGJsonOrderMode? ModeValue
            {
                get { return mode.GetEnumByJsonValue<INGJsonOrderMode>(); }
                set { mode = value.GetEnumJsonValue<INGJsonOrderMode>(); }
            }
        }

        [DataContract]
        public class INGJsonResponseAuthGetData : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseAuthConfirmData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseConfirm : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseConfirmData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseConfirmData
        {
            //[DataMember] public string trnsts { get; set; }
            //[DataMember] public string trnmsg { get; set; }
        }

        [DataContract]
        public class INGJsonResponseFastTransferAuthConfirm : INGJsonResponseBase
        {
        }

        [DataContract]
        public class INGJsonResponseAuthConfirm : INGJsonResponseAuthGetData
        {
        }

        [DataContract]
        public class INGJsonResponseAuthChangeFactor : INGJsonResponseAuthGetData
        {
        }

        [DataContract]
        public class INGJsonResponseAuthConfirmData
        {
            [DataMember(Name = "ref")] public string refValue { get; set; }
            [DataMember] public bool finished { get; set; }
            [DataMember] public string confirmURN { get; set; }
            [DataMember] public string factor { get; set; }
            [DataMember] public INGJsonResponseAuthConfirmDataChallenge challenge { get; set; }
            [DataMember] public List<string> alternativeFactors { get; set; }
            [DataMember] public string token { get; set; }
            [DataMember] public string pdfSignRefs { get; set; }
            [DataMember] public int expirySeconds { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public int messageId { get; set; }
            [DataMember] public string message { get; set; }
            [DataMember] public int? regenerateAfterSeconds { get; set; }

            public INGJsonAuthFactor? FactorValue
            {
                get { return factor.GetEnumByJsonValue<INGJsonAuthFactor>(); }
                set { factor = value.GetEnumJsonValue<INGJsonAuthFactor>(); }
            }
        }

        [DataContract]
        public class INGJsonResponseAuthConfirmDataChallenge
        {
            [DataMember] public string salt { get; set; }
            [DataMember] public string key { get; set; }
            [DataMember] public string mask { get; set; }
            [DataMember] public string pdfPrint { get; set; }
            [DataMember] public string pdfSign { get; set; }
            [DataMember] public string questions { get; set; }
            [DataMember] public bool? acceptNewBrowsers { get; set; }
            [DataMember] public string tripwires { get; set; }
            [DataMember] public string webAuthToken { get; set; }
            [DataMember] public string nfc { get; set; }
            [DataMember] public string template { get; set; }
            [DataMember] public string mobile { get; set; }
            [DataMember] public string mobywatel { get; set; }
        }

        [DataContract]
        public class INGJsonResponseAuthAutoConfirmConfirm : INGJsonResponseAuthGetData
        {
        }

        [DataContract]
        public class INGJsonResponseAuthSMSConfirm : INGJsonResponseAuthGetData
        {
        }

        [DataContract]
        public class INGJsonResponseAuthAddBrowserConfirm : INGJsonResponseAuthGetData
        {
        }

        [DataContract]
        public class INGJsonResponseAuthU2FConfirm : INGJsonResponseAuthGetData
        {
        }

        [DataContract]
        public class INGJsonResponseAuthFinished : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseLoginPasswordData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseConfirmMobile : INGJsonResponsePaymentConfirmable
        {
        }

        [DataContract]
        public class INGJsonResponseFastTransfer : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseFastTransferData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseFastTransferData
        {
            [DataMember] public string shopid { get; set; }
            [DataMember] public string trnref { get; set; }
            [DataMember] public double amount { get; set; }
            [DataMember] public string trntm { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string merchant { get; set; }
            [DataMember] public string shopname { get; set; }
            [DataMember] public string shopacct { get; set; }
            [DataMember] public string shoployal { get; set; }
        }

        [DataContract]
        public class INGJsonResponseFastTransferPolapiauthdata : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseFastTransferPolapiauthdataData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseFastTransferPBL : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseFastTransferPBLData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseFastTransferPBLData
        {
            [DataMember] public string shopid { get; set; }
            [DataMember] public string trnref { get; set; }
            [DataMember] public double amount { get; set; }
            [DataMember] public string trntm { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string merchant { get; set; }
            [DataMember] public string shopname { get; set; }
            [DataMember] public string shopacct { get; set; }
            [DataMember] public string shoployal { get; set; }
        }

        [DataContract]
        public class INGJsonResponseFastTransferPolapiauthdataData
        {
            [DataMember] public string tppName { get; set; }
            [DataMember] public string authType { get; set; }
            [DataMember] public string scopeTimeLimit { get; set; }
            [DataMember] public INGJsonResponseFastTransferPolapiauthdataDataTransfer transfer { get; set; }
        }

        [DataContract]
        public class INGJsonResponseFastTransferPADataConfirm : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseFastTransferPAConfirmableData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseFastTransferPBLDataConfirm : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseFastTransferPBLConfirmableData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseFastTransferPAConfirmableData : INGJsonResponsePaymentConfirmableData
        {
        }

        [DataContract]
        public class INGJsonResponseFastTransferPBLConfirmableData : INGJsonResponsePaymentConfirmableData
        {
        }

        [DataContract]
        public class INGJsonResponseFastTransferPolapiauthdataDataTransfer
        {
            [DataMember] public INGJsonResponseFastTransferPolapiauthdataDataTransferRecipient recipient { get; set; }
            [DataMember] public INGJsonResponseFastTransferPolapiauthdataDataTransferDetail detail { get; set; }
            [DataMember] public INGJsonResponseFastTransferPolapiauthdataDataTransferMoreDetail moredetail { get; set; }
            [DataMember] public INGJsonResponseFastTransferPolapiauthdataDataTransferUSInfo usInfo { get; set; }
            [DataMember] public List<INGJsonResponseFastTransferPolapiauthdataDataTransferAccount> accounts { get; set; }
        }

        [DataContract]
        public class INGJsonResponseFastTransferPolapiauthdataDataTransferRecipient
        {
            [DataMember] public string accountNumber { get; set; }
            [DataMember] public string creditAccountBankInfo { get; set; }
            [DataMember] public string nameAddress1 { get; set; }
            [DataMember] public string nameAddress2 { get; set; }
            [DataMember] public string nameAddress3 { get; set; }
        }

        [DataContract]
        public class INGJsonResponseFastTransferPolapiauthdataDataTransferDetail
        {
            [DataMember] public double amount { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public string description1 { get; set; }
            [DataMember] public string description2 { get; set; }
            [DataMember] public string executionDate { get; set; }
        }

        [DataContract]
        public class INGJsonResponseFastTransferPolapiauthdataDataTransferMoreDetail
        {
            [DataMember] public string deliveryMode { get; set; }
            [DataMember] public string system { get; set; }
            [DataMember] public string executionMode { get; set; }
        }

        [DataContract]
        public class INGJsonResponseFastTransferPolapiauthdataDataTransferUSInfo
        {
            [DataMember] public string payorId { get; set; }
            [DataMember] public string payorIdType { get; set; }
            [DataMember] public string formCode { get; set; }
            [DataMember] public string periodId { get; set; }
            [DataMember] public string periodType { get; set; }
            [DataMember] public int year { get; set; }
            [DataMember] public string obligationId { get; set; }
        }

        [DataContract]
        public class INGJsonResponseFastTransferPolapiauthdataDataTransferAccount
        {
            [DataMember] public string authType { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string accountNumber { get; set; }
            [DataMember] public string type { get; set; }
            [DataMember] public double availableBal { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public string context { get; set; }
        }

        [DataContract]
        public class INGJsonResponseGsmOperators : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseGsmOperatorsData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseGsmOperatorsData
        {
            [DataMember] public INGJsonResponseGsmOperatorsDataOperator[] opers { get; set; }
        }

        [DataContract]
        public class INGJsonResponseGsmOperatorsDataOperator
        {
            [DataMember] public string id { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string visible { get; set; }
            [DataMember] public string range { get; set; }
            [DataMember] public double minAmount { get; set; }
            [DataMember] public double maxAmount { get; set; }
            [DataMember] public double[] amounts { get; set; }

            public INGJsonNoYes? VisibleValue
            {
                get { return visible.GetEnumByJsonValue<INGJsonNoYes>(); }
                set { visible = value.GetEnumJsonValue<INGJsonNoYes>(); }
            }

            public INGJsonOperatorRange? RangeValue
            {
                get { return range.GetEnumByJsonValue<INGJsonOperatorRange>(); }
                set { range = value.GetEnumJsonValue<INGJsonOperatorRange>(); }
            }
        }

        [DataContract]
        public class INGJsonResponseHistory : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseHistoryData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseHistoryData
        {
            [DataMember] public int numtrns { get; set; }
            [DataMember] public double minAmt { get; set; }
            [DataMember] public double maxAmt { get; set; }
            [DataMember] public INGJsonResponseHistoryDataTransaction[] trns { get; set; }
            [DataMember] public string attrs { get; set; }
            [DataMember] public INGJsonResponseHistoryDataSum sum { get; set; }
        }

        [DataContract]
        public class INGJsonResponseHistoryDataTransaction
        {
            [DataMember] public INGJsonResponseHistoryDataTransactionM m { get; set; }
        }

        [DataContract]
        public class INGJsonResponseHistoryDataTransactionM
        {
            [DataMember] public string keraaja { get; set; }
            [DataMember] public string id { get; set; }
            [DataMember] public string date { get; set; }
            [DataMember] public int typ { get; set; }
            [DataMember] public string tpn { get; set; }
            [DataMember] public string uo { get; set; }
            [DataMember] public string aw { get; set; }
            [DataMember] public string awn { get; set; }
            [DataMember] public string w1 { get; set; }
            [DataMember] public string w2 { get; set; }
            [DataMember] public string w3 { get; set; }
            [DataMember] public string w4 { get; set; }
            [DataMember] public string am { get; set; }
            [DataMember] public string amn { get; set; }
            [DataMember] public string m1 { get; set; }
            [DataMember] public string m3 { get; set; }
            [DataMember] public double amt { get; set; }
            [DataMember] public string cr { get; set; }
            [DataMember] public double bal { get; set; }
            [DataMember] public string t1 { get; set; }
            [DataMember] public string t2 { get; set; }
            [DataMember] public string t3 { get; set; }
            [DataMember] public string t4 { get; set; }
            [DataMember] public string fl { get; set; }
            [DataMember] public string rfl { get; set; }
            [DataMember] public int pfi { get; set; }
            [DataMember] public int pfs { get; set; }
            [DataMember] public string st { get; set; }
            [DataMember] public string itt { get; set; }
            [DataMember] public string jtr { get; set; }
            [DataMember] public bool allowSum { get; set; }

            public INGJsonNoYes? KeraajaValue
            {
                get { return keraaja.GetEnumByJsonValue<INGJsonNoYes>(); }
                set { keraaja = value.GetEnumJsonValue<INGJsonNoYes>(); }
            }

            public DateTime? DateValue
            {
                get { return DateTime.Parse(date); }
                set { date = value?.Display("yyyy-MM-dd") ?? String.Empty; }
            }

            public INGJsonTransferType? TypeValue
            {
                get { return typ.GetEnumByJsonValue<INGJsonTransferType>(); }
                set { typ = value.GetEnumJsonValueInt<INGJsonTransferType>(); }
            }

            public INGJsonCreditDebit? CreditDebitValue
            {
                get { return uo.GetEnumByJsonValue<INGJsonCreditDebit>(); }
                set { uo = value.GetEnumJsonValue<INGJsonCreditDebit>(); }
            }

            //public INGJsonResponseCategory? CategoryValue
            //{
            //    get { return pfi.GetEnumByJsonValue<INGJsonResponseCategory>(); }
            //    set { pfi = value.GetEnumJsonValueInt<INGJsonResponseCategory>(); }
            //}

            public INGJsonNoYes? StValue
            {
                get { return st.GetEnumByJsonValue<INGJsonNoYes>(); }
                set { st = value.GetEnumJsonValue<INGJsonNoYes>(); }
            }

            public INGJsonNoYes? IttValue
            {
                get { return itt.GetEnumByJsonValue<INGJsonNoYes>(); }
                set { itt = value.GetEnumJsonValue<INGJsonNoYes>(); }
            }

            public INGJsonNoYes? JtrValue
            {
                get { return jtr.GetEnumByJsonValue<INGJsonNoYes>(); }
                set { jtr = value.GetEnumJsonValue<INGJsonNoYes>(); }
            }
        }

        [DataContract]
        public class INGJsonResponseHistoryDataSum
        {
            [DataMember] public INGJsonResponseHistoryDataSumTot[] tot { get; set; }
            [DataMember] public int numcred { get; set; }
            [DataMember] public int numdebs { get; set; }
        }

        [DataContract]
        public class INGJsonResponseHistoryDataSumTot
        {
            [DataMember] public string curr { get; set; }
            [DataMember] public int numdebs { get; set; }
            [DataMember] public double totdebs { get; set; }
            [DataMember] public int numcred { get; set; }
            [DataMember] public double totcred { get; set; }
        }

        [DataContract]
        public class INGJsonResponsePaymentCheckAccount : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponsePaymentCheckAccountData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponsePaymentCheckAccountData : INGJsonResponseBase
        {
            [DataMember] public string name { get; set; }
            [DataMember] public string addr { get; set; }
            [DataMember] public string accttype { get; set; }
        }

        [DataContract]
        public class INGJsonResponseTransactionPDF : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseTransactionPDFData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseTransactionPDFData
        {
            [DataMember(Name = "ref")] public string refValue { get; set; }
        }

        [DataContract]
        public class INGJsonResponseTaxFormTypes : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseTaxFormTypesData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseTaxFormTypesData
        {
            [DataMember] public List<INGJsonResponseTaxFormTypesDataSfp> sfps { get; set; }
        }

        [DataContract]
        public class INGJsonResponseTaxFormTypesDataSfp
        {
            [DataMember] public string sfp { get; set; }
            [DataMember] public string okr { get; set; }
            //TODO enum
            [DataMember] public string oth { get; set; }
            [DataMember] public bool vat { get; set; }
            [DataMember] public bool irp { get; set; }
            [DataMember] public INGJsonResponseTaxAccount fisacct { get; set; }

            public INGJsonTaxNoYes? PeriodValue
            {
                get { return okr.GetEnumByJsonValue<INGJsonTaxNoYes>(); }
                set { okr = value.GetEnumJsonValue<INGJsonTaxNoYes>(); }
            }

            public INGJsonTaxOth? OthValue
            {
                get { return oth.GetEnumByJsonValue<INGJsonTaxOth>(); }
                set { oth = value.GetEnumJsonValue<INGJsonTaxOth>(); }
            }
        }

        [DataContract]
        public class INGJsonResponseTaxAccount
        {
            [DataMember] public string acct { get; set; }
            [DataMember] public string name1 { get; set; }
            [DataMember] public string name2 { get; set; }
            [DataMember] public string city { get; set; }
        }

        [DataContract]
        public class INGJsonResponseTaxOffice : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseTaxOfficeData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseTaxOfficeData
        {
            [DataMember] public List<INGJsonResponseTaxAccount> accts { get; set; }
        }

        [DataContract]
        public class INGJsonResponseTaxTransfer : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseTaxTransferData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseTaxTransferData : INGJsonResponsePaymentConfirmableData
        {
        }

        [DataContract]
        public class INGJsonResponseOauth2Init : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseOauth2InitData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseOauth2InitData
        {
            [DataMember] public string authorizationReference { get; set; }
            [DataMember] public string csrfToken { get; set; }
            [DataMember] public string clientId { get; set; }
            [DataMember] public string scopes { get; set; }
            [DataMember] public string custom { get; set; }
            [DataMember] public string errorRedirectUrl { get; set; }
        }

        [DataContract]
        public class INGJsonResponseOauth2ConfirmLogin : INGJsonResponseAuthGetData
        {
        }

        [DataContract]
        public class INGJsonResponseOauth2ConfirmPassword : INGJsonResponseAuthGetData
        {
        }

        [DataContract]
        public class INGJsonResponseGetToken : INGJsonResponseBase
        {
            [DataMember] public INGJsonResponseGetTokenData data { get; set; }
        }

        [DataContract]
        public class INGJsonResponseGetTokenData
        {
            [DataMember] public string token { get; set; }
        }
    }
}
