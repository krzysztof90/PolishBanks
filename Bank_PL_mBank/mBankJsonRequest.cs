using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BankService.Bank_PL_MBank
{
    public class MBankJsonRequest
    {
        [DataContract]
        public abstract class MBankJsonRequestBase
        {
        }

        [DataContract]
        public class MBankJsonRequestAdditionalOptions
        {
        }

        [DataContract]
        public class MBankJsonRequestAdditionalOptionsDummy : MBankJsonRequestAdditionalOptions
        {
            [DataMember] public string a { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestLogin : MBankJsonRequestBase
        {
            [DataMember] public string UserName { get; set; }
            [DataMember] public string Password { get; set; }
            //TODO enum
            [DataMember] public string Scenario { get; set; }
            [DataMember] public MBankJsonRequestAdditionalOptionsDummy UWAdditionalParams { get; set; }
            [DataMember] public MBankJsonRequestLoginDfpData DfpData { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestLoginDfpData
        {
            [DataMember] public string dfp { get; set; }
        }

        [DataContract]
        public abstract class MBankJsonRequestInitializeBase : MBankJsonRequestBase
        {
            //TODO enum
            [DataMember] public string moduleId { get; set; }
        }

        [DataContract]
        public abstract class BankJsonRequestInitializeModuleDataBase
        {
        }

        [DataContract]
        public class MBankJsonRequestInitialize : MBankJsonRequestInitializeBase
        {
            [DataMember] public MBankJsonRequestInitializeModuleData moduleData { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestInitializeModuleData : BankJsonRequestInitializeModuleDataBase
        {
            [DataMember] public string ScaAuthorizationId { get; set; }
            [DataMember] public string BrowserName { get; set; }
            [DataMember] public string BrowserVersion { get; set; }
            [DataMember] public string OsName { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestInitializeTrustedDevice : MBankJsonRequestInitializeBase
        {
            [DataMember] public MBankJsonRequestInitializeTrustedDeviceModuleData moduleData { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestInitializeTrustedDeviceModuleData : MBankJsonRequestInitializeModuleData
        {
            [DataMember] public string DeviceName { get; set; }
            [DataMember] public string DfpData { get; set; }
            [DataMember] public bool IsTheOnlyDeviceUser { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestAuthorizationStatus : MBankJsonRequestBase
        {
            [DataMember] public string authorizationId { get; set; }
        }

        [DataContract]
        public abstract class MBankJsonRequestFinalizeAuthorizationBase : MBankJsonRequestBase
        {
            [DataMember] public string scaAuthorizationId { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestFinalizeAuthorization : MBankJsonRequestFinalizeAuthorizationBase
        {
        }

        [DataContract]
        public class MBankJsonRequestFinalizeAuthorizationTrustedDevice : MBankJsonRequestFinalizeAuthorizationBase
        {
            [DataMember] public string deviceName { get; set; }
            [DataMember] public string currentDfp { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestAuthorizationTransferStatus : MBankJsonRequestBase
        {
            [DataMember] public string TranId { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestConfirm : MBankJsonRequestBase
        {
            [DataMember] public string authorizationCode { get; set; }
            //TODO enum
            [DataMember] public string authorizationType { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestProducts : MBankJsonRequestBase
        {
            [DataMember] public List<MBankJsonRequestProductsProduct> productsIds { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestProductsProduct
        {
            [DataMember] public string id { get; set; }
            [DataMember] public int order { get; set; }
            [DataMember] public string productType { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestAmount
        {
            [DataMember] public string currency { get; set; }
            [DataMember] public double value { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestAmountCapital
        {
            //TODO almost the same as above
            [DataMember] public string Currency { get; set; }
            [DataMember] public double Value { get; set; }
        }

        [DataContract]
        public abstract class MBankJsonRequestInitPrepare : MBankJsonRequestBase
        {
            //TODO enum
            [DataMember] public string Method { get; set; }
            [DataMember] public bool TwoFactor { get; set; }
            [DataMember] public string Url { get; set; }
        }

        [DataContract]
        public abstract class MBankJsonRequestInitPrepareData
        {
        }

        [DataContract]
        public class MBankJsonRequestInitPrepareTransfer : MBankJsonRequestInitPrepare
        {
            [DataMember] public MBankJsonRequestInitPrepareTransferData Data { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestInitPrepareTransferData : MBankJsonRequestInitPrepareData
        {
            [DataMember] public string toAccount { get; set; }
            [DataMember] public MBankJsonRequestAmount amount { get; set; }
            [DataMember] public DateTime date { get; set; }
            [DataMember] public string fromAccount { get; set; }
            [DataMember] public string cardNumber { get; set; }
            [DataMember] public string coowner { get; set; }
            [DataMember] public MBankJsonRequestInitPrepareTransferDataReceiver receiver { get; set; }
            [DataMember] public string title { get; set; }
            //TODO enum
            [DataMember] public string transferMode { get; set; }
            [DataMember] public string paymentSource { get; set; }
            [DataMember] public MBankJsonRequestAdditionalOptionsDummy additionalOptions { get; set; }
            [DataMember] public string perfToken { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestInitPrepareTransferDataReceiver
        {
            [DataMember] public string name { get; set; }
            [DataMember] public string street { get; set; }
            [DataMember] public string cityAndPostalCode { get; set; }
            [DataMember] public string nip { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestInitPrepareTaxTransfer : MBankJsonRequestInitPrepare
        {
            [DataMember] public MBankJsonRequestInitPrepareTaxTransferData Data { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestInitPrepareTaxTransferData : MBankJsonRequestInitPrepareData
        {
            [DataMember] public MBankJsonRequestInitPrepareTaxTransferDataUsform usform { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestInitPrepareTaxTransferDataUsform
        {
            [DataMember] public string fromAccount { get; set; }
            [DataMember] public string sender { get; set; }
            [DataMember] public string accountParams { get; set; }
            [DataMember] public string perfToken { get; set; }
            //TODO date
            [DataMember] public string date { get; set; }
            [DataMember] public string formType { get; set; }
            [DataMember] public MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthority taxAuthority { get; set; }
            [DataMember] public double amount { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public MBankJsonRequestInitPrepareTaxTransferDataUsformDefaultData defaultData { get; set; }
            [DataMember] public MBankJsonRequestInitPrepareTaxTransferDataUsformIdType idType { get; set; }
            [DataMember] public MBankJsonRequestInitPrepareTaxTransferDataUsformPeriod period { get; set; }
            [DataMember] public string commitmentId { get; set; }
            [DataMember] public MBankJsonRequestAdditionalOptions additionalOptions { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthority
        {
            [DataMember] public MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthoritySymbol authoritySymbol { get; set; }
            [DataMember] public MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityCity authorityCity { get; set; }
            [DataMember] public MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityName authorityName { get; set; }
            [DataMember] public MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityNameCustom authorityNameCustom { get; set; }
            [DataMember] public MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityAccountNumberCustom authorityAccountNumberCustom { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthoritySymbol
        {
            [DataMember] public string symbol { get; set; }
            [DataMember] public bool toAnother { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityName
        {
            [DataMember] public string name { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityCity
        {
        }

        [DataContract]
        public class MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityNameCustom
        {
        }

        [DataContract]
        public class MBankJsonRequestInitPrepareTaxTransferDataUsformTaxAuthorityAuthorityAccountNumberCustom
        {
        }

        [DataContract]
        public class MBankJsonRequestInitPrepareTaxTransferDataUsformDefaultData
        {
        }

        [DataContract]
        public class MBankJsonRequestInitPrepareTaxTransferDataUsformIdType
        {
            [DataMember] public string type { get; set; }
            [DataMember] public string series { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestInitPrepareTaxTransferDataUsformPeriod
        {
            [DataMember] public string currentValue { get; set; }
            [DataMember] public string currentPeriod { get; set; }
            [DataMember] public string currentMonth { get; set; }
            [DataMember] public string currentYear { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestInitPrepareTaxTransferDataUsformAdditionalOptionsAddReceiver
        {
            [DataMember] public bool addToAddressBook { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestInitPreparePhoneCharge : MBankJsonRequestInitPrepare
        {
            [DataMember] public MBankJsonRequestInitPreparePhoneChargeData Data { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestInitPreparePhoneChargeData : MBankJsonRequestInitPrepareData
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
        public class MBankJsonRequestTransferCheck : MBankJsonRequestBase
        {
            [DataMember] public string FromAccount { get; set; }
            [DataMember] public string ToAccount { get; set; }
            [DataMember] public DateTime Date { get; set; }
            [DataMember] public MBankJsonRequestAmountCapital Amount { get; set; }
            //TODO enum
            [DataMember] public string RedirectionSource { get; set; }
            //TODO enum
            [DataMember] public string TransferType { get; set; }
            [DataMember] public string checkDataId { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestExecute : MBankJsonRequestBase
        {
            [DataMember] public string Auth { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestConfirmation : MBankJsonRequestBase
        {
            [DataMember] public List<MBankJsonRequestConfirmationTransaction> Transactions { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestConfirmationTransaction
        {
            [DataMember] public string accountNumber { get; set; }
            [DataMember] public int operationNumber { get; set; }
        }

        [DataContract]
        public class MBankJsonRequestTaxTransferPrepare : MBankJsonRequestBase
        {
        }

        [DataContract]
        public class MBankJsonRequestTaxAccounts : MBankJsonRequestBase
        {
            [DataMember] public string accType { get; set; }
            [DataMember] public string cityName { get; set; }
        }
    }
}
