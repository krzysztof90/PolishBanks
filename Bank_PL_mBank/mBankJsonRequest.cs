using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BankService.Bank_PL_mBank
{
    public class mBankJsonRequest
    {
        [DataContract]
        public abstract class mBankJsonRequestBase
        {
        }

        [DataContract]
        public class mBankJsonRequestAdditionalOptions
        {
        }

        [DataContract]
        public class mBankJsonRequestAdditionalOptionsDummy : mBankJsonRequestAdditionalOptions
        {
            [DataMember] public string a { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestLogin : mBankJsonRequestBase
        {
            [DataMember] public string UserName { get; set; }
            [DataMember] public string Password { get; set; }
            //TODO enum
            [DataMember] public string Scenario { get; set; }
            [DataMember] public mBankJsonRequestAdditionalOptionsDummy UWAdditionalParams { get; set; }
            [DataMember] public mBankJsonRequestLoginDfpData DfpData { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestLoginDfpData
        {
            [DataMember] public string dfp { get; set; }
        }

        [DataContract]
        public abstract class mBankJsonRequestInitializeBase : mBankJsonRequestBase
        {
            //TODO enum
            [DataMember] public string moduleId { get; set; }
        }

        [DataContract]
        public abstract class mBankJsonRequestInitializeModuleDataBase
        {
        }

        [DataContract]
        public class mBankJsonRequestInitialize : mBankJsonRequestInitializeBase
        {
            [DataMember] public mBankJsonRequestInitializeModuleData moduleData { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestInitializeModuleData : mBankJsonRequestInitializeModuleDataBase
        {
            [DataMember] public string ScaAuthorizationId { get; set; }
            [DataMember] public string BrowserName { get; set; }
            [DataMember] public string BrowserVersion { get; set; }
            [DataMember] public string OsName { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestInitializeTrustedDevice : mBankJsonRequestInitializeBase
        {
            [DataMember] public mBankJsonRequestInitializeTrustedDeviceModuleData moduleData { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestInitializeTrustedDeviceModuleData : mBankJsonRequestInitializeModuleData
        {
            [DataMember] public string DeviceName { get; set; }
            [DataMember] public string DfpData { get; set; }
            [DataMember] public bool IsTheOnlyDeviceUser { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestAuthorizationStatus : mBankJsonRequestBase
        {
            [DataMember] public string authorizationId { get; set; }
        }

        [DataContract]
        public abstract class mBankJsonRequestFinalizeAuthorizationBase : mBankJsonRequestBase
        {
            [DataMember] public string scaAuthorizationId { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestFinalizeAuthorization : mBankJsonRequestFinalizeAuthorizationBase
        {
        }

        [DataContract]
        public class mBankJsonRequestFinalizeAuthorizationTrustedDevice : mBankJsonRequestFinalizeAuthorizationBase
        {
            [DataMember] public string deviceName { get; set; }
            [DataMember] public string currentDfp { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestAuthorizationTransferStatus : mBankJsonRequestBase
        {
            [DataMember] public string TranId { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestConfirm : mBankJsonRequestBase
        {
            [DataMember] public string authorizationCode { get; set; }
            //TODO enum
            [DataMember] public string authorizationType { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestProducts : mBankJsonRequestBase
        {
            [DataMember] public List<mBankJsonRequestProductsProduct> productsIds { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestProductsProduct
        {
            [DataMember] public string id { get; set; }
            [DataMember] public int order { get; set; }
            [DataMember] public string productType { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestAmount
        {
            [DataMember] public string currency { get; set; }
            [DataMember] public double value { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestAmountCapital
        {
            //TODO almost the same as above
            [DataMember] public string Currency { get; set; }
            [DataMember] public double Value { get; set; }
        }

        [DataContract]
        public abstract class mBankJsonRequestInitPrepare : mBankJsonRequestBase
        {
            //TODO enum
            [DataMember] public string Method { get; set; }
            [DataMember] public bool TwoFactor { get; set; }
            [DataMember] public string Url { get; set; }
        }

        [DataContract]
        public abstract class mBankJsonRequestInitPrepareData
        {
        }

        [DataContract]
        public class mBankJsonRequestInitPrepareTransfer : mBankJsonRequestInitPrepare
        {
            [DataMember] public mBankJsonRequestInitPrepareTransferData Data { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestInitPrepareTransferData : mBankJsonRequestInitPrepareData
        {
            [DataMember] public string toAccount { get; set; }
            [DataMember] public mBankJsonRequestAmount amount { get; set; }
            [DataMember] public DateTime date { get; set; }
            [DataMember] public string fromAccount { get; set; }
            [DataMember] public string cardNumber { get; set; }
            [DataMember] public string coowner { get; set; }
            [DataMember] public mBankJsonRequestInitPrepareTransferDataReceiver receiver { get; set; }
            [DataMember] public string title { get; set; }
            //TODO enum
            [DataMember] public string transferMode { get; set; }
            [DataMember] public string paymentSource { get; set; }
            [DataMember] public mBankJsonRequestAdditionalOptionsDummy additionalOptions { get; set; }
            [DataMember] public string perfToken { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestInitPrepareTransferDataReceiver
        {
            [DataMember] public string name { get; set; }
            [DataMember] public string street { get; set; }
            [DataMember] public string cityAndPostalCode { get; set; }
            [DataMember] public string nip { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestInitPrepareTaxTransfer : mBankJsonRequestInitPrepare
        {
            [DataMember] public mBankJsonRequestInitPrepareTaxTransferData Data { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestInitPrepareTaxTransferData : mBankJsonRequestInitPrepareData
        {
            [DataMember] public mBankJsonRequestInitPrepareTaxTransferDataUsform usform { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestInitPrepareTaxTransferDataUsform
        {
            [DataMember] public string fromAccount { get; set; }
            [DataMember] public string sender { get; set; }
            [DataMember] public string accountParams { get; set; }
            [DataMember] public string perfToken { get; set; }
            //TODO date
            [DataMember] public string date { get; set; }
            [DataMember] public string formType { get; set; }
            [DataMember] public mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthority taxAuthority { get; set; }
            [DataMember] public double amount { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public mBankJsonRequestInitPrepareTaxTransferDataUsformDefaultData defaultData { get; set; }
            [DataMember] public mBankJsonRequestInitPrepareTaxTransferDataUsformIdType idType { get; set; }
            [DataMember] public mBankJsonRequestInitPrepareTaxTransferDataUsformPeriod period { get; set; }
            [DataMember] public string commitmentId { get; set; }
            [DataMember] public mBankJsonRequestAdditionalOptions additionalOptions { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthority
        {
            [DataMember] public mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthoritySymbol authoritySymbol { get; set; }
            [DataMember] public mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityCity authorityCity { get; set; }
            [DataMember] public mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityName authorityName { get; set; }
            [DataMember] public mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityNameCustom authorityNameCustom { get; set; }
            [DataMember] public mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityAccountNumberCustom authorityAccountNumberCustom { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthoritySymbol
        {
            [DataMember] public string symbol { get; set; }
            [DataMember] public bool toAnother { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityName
        {
            [DataMember] public string name { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityCity
        {
        }

        [DataContract]
        public class mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityNameCustom
        {
        }

        [DataContract]
        public class mBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityAccountNumberCustom
        {
        }

        [DataContract]
        public class mBankJsonRequestInitPrepareTaxTransferDataUsformDefaultData
        {
        }

        [DataContract]
        public class mBankJsonRequestInitPrepareTaxTransferDataUsformIdType
        {
            [DataMember] public string type { get; set; }
            [DataMember] public string series { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestInitPrepareTaxTransferDataUsformPeriod
        {
            [DataMember] public string currentValue { get; set; }
            [DataMember] public string currentPeriod { get; set; }
            [DataMember] public string currentMonth { get; set; }
            [DataMember] public string currentYear { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestInitPrepareTaxTransferDataUsformAdditionalOptionsAddReceiver
        {
            [DataMember] public bool addToAddressBook { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestInitPreparePhoneCharge : mBankJsonRequestInitPrepare
        {
            [DataMember] public mBankJsonRequestInitPreparePhoneChargeData Data { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestInitPreparePhoneChargeData : mBankJsonRequestInitPrepareData
        {
            [DataMember] public double Amount { get; set; }
            [DataMember] public string Currency { get; set; }
            //TODO enum
            [DataMember] public string FormType { get; set; }
            [DataMember] public string FromAccount { get; set; }
            [DataMember] public string MTransferId { get; set; }
            [DataMember] public string OperatorId { get; set; }
            [DataMember] public string PhoneNumber { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestTransferCheck : mBankJsonRequestBase
        {
            [DataMember] public string FromAccount { get; set; }
            [DataMember] public string ToAccount { get; set; }
            [DataMember] public DateTime Date { get; set; }
            [DataMember] public mBankJsonRequestAmountCapital Amount { get; set; }
            //TODO enum
            [DataMember] public string RedirectionSource { get; set; }
            //TODO enum
            [DataMember] public string TransferType { get; set; }
            [DataMember] public string checkDataId { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestExecute : mBankJsonRequestBase
        {
            [DataMember] public string Auth { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestConfirmation : mBankJsonRequestBase
        {
            [DataMember] public List<mBankJsonRequestConfirmationTransaction> Transactions { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestConfirmationTransaction
        {
            [DataMember] public string accountNumber { get; set; }
            [DataMember] public int operationNumber { get; set; }
        }

        [DataContract]
        public class mBankJsonRequestTaxTransferPrepare : mBankJsonRequestBase
        {
        }

        [DataContract]
        public class mBankJsonRequestTaxAccounts : mBankJsonRequestBase
        {
            [DataMember] public string accType { get; set; }
            [DataMember] public string cityName { get; set; }
        }
    }
}
