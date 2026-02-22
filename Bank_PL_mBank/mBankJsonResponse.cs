using BankService.Bank_PL_Nest;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Tools;

namespace BankService.Bank_PL_mBank
{
    public class mBankJsonResponse
    {
        [DataContract]
        public class mBankJsonResponseBase
        {
        }

        [DataContract]
        public class mBankJsonResponseAmount
        {
            [DataMember] public double value { get; set; }
            [DataMember] public string currency { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseLogin : mBankJsonResponseBase
        {
            [DataMember] public string errorMessageBody { get; set; }
            [DataMember] public string errorMessageTitle { get; set; }
            [DataMember] public bool successful { get; set; }
            [DataMember] public string button { get; set; }
            [DataMember] public string redirectUrl { get; set; }
            [DataMember] public string tabId { get; set; }
            [DataMember] public string sessionKeyForUW { get; set; }
            [DataMember] public bool regulationsApproval { get; set; }
            [DataMember] public bool betaTestingApproval { get; set; }
            [DataMember] public bool implicitTestingApproval { get; set; }
            [DataMember] public bool allAprovalsSaved { get; set; }
            [DataMember] public bool isUserAccountBlocked { get; set; }
            [DataMember] public bool isActivationPackageBlocked { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseSetupData : mBankJsonResponseBase
        {
            [DataMember] public string entity { get; set; }
            [DataMember] public string antiForgeryToken { get; set; }
            [DataMember] public mBankJsonResponseSetupDataCustomer customer { get; set; }
            [DataMember] public mBankJsonResponseSetupDataProfile profile { get; set; }
            [DataMember] public mBankJsonResponseSetupDataFlags flags { get; set; }
            [DataMember] public mBankJsonResponseSetupDataLocalization localization { get; set; }
            [DataMember] public string veneziaCultureUrlPrefix { get; set; }
            [DataMember] public mBankJsonResponseSetupDataTracker tracker { get; set; }
            [DataMember] public mBankJsonResponseSetupDataConfiguration configuration { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseSetupDataCustomer
        {
            [DataMember] public mBankJsonResponseSetupDataCustomerConsents consents { get; set; }
            [DataMember] public string name { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseSetupDataCustomerConsents
        {
            [DataMember] public bool PR1 { get; set; }
            [DataMember] public bool PR2 { get; set; }
            [DataMember] public bool REK { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseSetupDataProfile
        {
            [DataMember] public string name { get; set; }
            [DataMember] public bool isPb { get; set; }
            [DataMember] public int type { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseSetupDataFlags
        {
            [DataMember] public bool AllowDemo { get; set; }
            [DataMember] public bool AllowTranslations { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseSetupDataLocalization
        {
            [DataMember] public List<string> languages { get; set; }
            [DataMember] public string selectedLanguage { get; set; }
            [DataMember] public string defaultLanguage { get; set; }
            [DataMember] public string veneziaCultureUrlPrefix { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseSetupDataTracker
        {
            [DataMember] public string cdnAddress { get; set; }
            [DataMember] public string trackerKey { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseSetupDataConfiguration
        {
            [DataMember] public mBankJsonResponseSetupDataConfigurationSessionTimer sessionTimer { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseSetupDataConfigurationSessionTimer
        {
            [DataMember] public int sessionTimeout { get; set; }
            [DataMember] public int timeLeftToShowAlert { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseAuthorizationData : mBankJsonResponseBase
        {
            [DataMember] public string ScaAuthorizationId { get; set; }
            [DataMember] public bool TrustedDeviceAddingAllowed { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseAuthorizationLocale : mBankJsonResponseBase
        {
            [DataMember(Name = "auth.CancelButtonLabel")] public string CancelButtonLabel { get; set; }
            [DataMember(Name = "auth.ErrorTitleLabel")] public string ErrorTitleLabel { get; set; }
            [DataMember(Name = "auth.GeneralErrorMessage1")] public string GeneralErrorMessage1 { get; set; }
            [DataMember(Name = "auth.GeneralErrorMessage2")] public string GeneralErrorMessage2 { get; set; }
            [DataMember(Name = "auth.LogoutButtonLabel")] public string LogoutButtonLabel { get; set; }
            [DataMember(Name = "auth.NoAuthorizationPushSent")] public string NoAuthorizationPushSent { get; set; }
            [DataMember(Name = "auth.NotificationCancelled")] public string NotificationCancelled { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseAuthorization : mBankJsonResponseBase
        {
            [DataMember] public mBankJsonResponseAuthorizationAuthorizationData authorizationData { get; set; }
            [DataMember] public string data { get; set; }
            [DataMember] public int operationTimeout { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseAuthorizationAuthorizationData
        {
            [DataMember] public string authorizationType { get; set; }
            [DataMember] public string deviceName { get; set; }
            [DataMember] public bool multiDevice { get; set; }
            [DataMember] public string finServerChallenge { get; set; }
            [DataMember] public string activePluginServerChallenge { get; set; }
            [DataMember] public string activePlugin { get; set; }
            [DataMember] public string availablePlugins { get; set; }
            [DataMember] public string activePluginFailCount { get; set; }
            [DataMember] public string activePluginStatus { get; set; }
            [DataMember] public string verifiedPlugins { get; set; }
            [DataMember] public string operationTimeoutSeconds { get; set; }
            [DataMember] public string finalAuthorizationMethod { get; set; }
            [DataMember] public string fullTimeoutSeconds { get; set; }
            [DataMember] public string authorizationId { get; set; }
            [DataMember] public string authorizationDate { get; set; }
            [DataMember] public int authorizationNumber { get; set; }
            //TODO enum
            [DataMember] public string authorizationStatus { get; set; }

            public mBankJsonAuthorizationType? AuthorizationTypeValue
            {
                get { return authorizationType.GetEnumByJsonValue<mBankJsonAuthorizationType>(); }
                set { authorizationType = value.GetEnumJsonValue<mBankJsonAuthorizationType>(); }
            }
        }

        [DataContract]
        public class mBankJsonResponseAuthorizationStatus : mBankJsonResponseBase
        {
            [DataMember] public string authorizationStatus { get; set; }
            [DataMember] public string authorizationId { get; set; }
            [DataMember] public mBankJsonResponseAuthorizePostResult postResult { get; set; }
            //TODO enum
            [DataMember] public string notificationMethod { get; set; }

            public mBankJsonAuthorizationStatus? AuthorizationStatusValue
            {
                get { return authorizationStatus.GetEnumByJsonValue<mBankJsonAuthorizationStatus>(); }
                set { authorizationStatus = value.GetEnumJsonValue<mBankJsonAuthorizationStatus>(); }
            }
        }

        [DataContract]
        public class mBankJsonResponseAuthorizationTransferStatus : mBankJsonResponseBase
        {
            [DataMember] public string Status { get; set; }
            [DataMember] public string Message { get; set; }
            [DataMember] public string ErrorCode { get; set; }

            public mBankJsonAuthorizationTransferStatus? AuthorizationStatusValue
            {
                get { return Status.GetEnumByJsonValue<mBankJsonAuthorizationTransferStatus>(); }
                set { Status = value.GetEnumJsonValue<mBankJsonAuthorizationTransferStatus>(); }
            }
        }

        [DataContract]
        public class mBankJsonResponseFinalizeAuthorization : mBankJsonResponseBase
        {
        }

        [DataContract]
        public class mBankJsonResponseAuthorize : mBankJsonResponseBase
        {
            [DataMember] public string type { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public int status { get; set; }
            [DataMember] public string detail { get; set; }
            [DataMember] public mBankJsonResponseAuthorizeErrors errors { get; set; }
            [DataMember] public string traceId { get; set; }
            [DataMember] public mBankJsonResponseAuthorizePostResult postResult { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseAuthorizeErrors
        {
        }

        [DataContract]
        public class mBankJsonResponseAuthorizePostResult
        {
            [DataMember] public mBankJsonResponseExecutePostResultData data { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseExecutePostResultData
        {
            [DataMember] public string scaAuthorizationId { get; set; }
            [DataMember] public string browserName { get; set; }
            [DataMember] public string browserVersion { get; set; }
            [DataMember] public string osName { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseExecute : mBankJsonResponseBase
        {
            [DataMember] public mBankJsonResponseExecutePostResult PostResult { get; set; }
            [DataMember] public string Message { get; set; }
            [DataMember] public string ErrorCode { get; set; }
            [DataMember] public string error { get; set; }
            [DataMember] public string source { get; set; }
            [DataMember] public string message { get; set; }
            [DataMember] public string type { get; set; }
            [DataMember] public mBankJsonResponseExecuteApiFault apiFault { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseExecuteApiFault
        {
            [DataMember] public string code { get; set; }
            [DataMember] public string message { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseExecutePostResult
        {
            [DataMember] public bool blockResources { get; set; }
            [DataMember] public mBankJsonResponseExecutePostResultOperationBasketStatus operationBasketStatus { get; set; }
            //TODO enum
            [DataMember] public string transferTimeType { get; set; }
            [DataMember] public mBankJsonResponseDomesticAccount receiverAccount { get; set; }
            [DataMember] public mBankJsonResponseDomesticAccount senderAccount { get; set; }
            [DataMember] public mBankJsonResponseAmount transferAmount { get; set; }
            [DataMember] public bool isTransactionWithheld { get; set; }
            [DataMember] public bool showRepeatFromBasketWarning { get; set; }
            [DataMember] public string operationWarningsAndErrors { get; set; }
            [DataMember] public string referenceNumber { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseExecutePostResultOperationBasketStatus
        {
            [DataMember] public bool firstAuthLimitReached { get; set; }
            [DataMember] public bool secondAuthLimitReached { get; set; }
            [DataMember] public bool addToBasket { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTrustedDevicesCheck : mBankJsonResponseBase
        {
            [DataMember] public bool isValid { get; set; }
            [DataMember] public string deviceName { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTrustedDevicesAdd : mBankJsonResponseBase
        {
            [DataMember] public bool isValid { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseUserSettings : mBankJsonResponseBase
        {
            [DataMember] public List<mBankJsonResponseUserSettingsProduct> products { get; set; }
            [DataMember] public bool isWospEnabled { get; set; }
            [DataMember] public bool isSupportUkraineEnabled { get; set; }
            [DataMember] public bool isWospFloodReliefEnabled { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseUserSettingsProduct
        {
            [DataMember] public string id { get; set; }
            [DataMember] public string productType { get; set; }
            [DataMember] public int? order { get; set; }
        }

        //TODO common part with mBankJsonResponseUserSettings
        [DataContract]
        public class mBankJsonResponseProducts : mBankJsonResponseBase
        {
            [DataMember] public List<mBankJsonResponseProductsProduct> products { get; set; }
            [DataMember] public bool isWospEnabled { get; set; }
            [DataMember] public bool isSupportUkraineEnabled { get; set; }
            [DataMember] public bool isWospFloodReliefEnabled { get; set; }
        }

        //TODO covers with mBankJsonResponseUserSettingsProduct
        [DataContract]
        public class mBankJsonResponseProductsProduct : mBankJsonResponseUserSettingsProduct
        {
            [DataMember] public string currentMonthExpenditure { get; set; }
            [DataMember] public string upcomingPayments { get; set; }
            [DataMember] public double ownFunds { get; set; }
            [DataMember] public string number { get; set; }
            [DataMember] public double AvailableBalance { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public string additionalName { get; set; }
            [DataMember] public mBankJsonResponseAmount balance { get; set; }
            [DataMember] public string nameAddition { get; set; }
            [DataMember] public string accountNumberToRedirect { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string shortName { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseExtendSession : mBankJsonResponseBase
        {
            [DataMember] public bool success { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseHistory : mBankJsonResponseBase
        {
            [DataMember] public List<mBankJsonResponseHistoryPfmProduct> pfmProducts { get; set; }
            //TODO datetime?
            [DataMember] public string dateFrom { get; set; }
            [DataMember] public string dateTo { get; set; }
            [DataMember] public List<mBankJsonResponseHistoryPfmTransactionType> pfmTransactionTypes { get; set; }
            [DataMember] public List<mBankJsonResponseHistoryCategory> categories { get; set; }
            [DataMember] public string accountNumber { get; set; }
            [DataMember] public List<string> tags { get; set; }
            [DataMember] public string transactionId { get; set; }
            [DataMember] public string standingOrderId { get; set; }
            [DataMember] public bool showIrrelevantTransactions { get; set; }
            [DataMember] public bool showSavingsAndInvestments { get; set; }
            [DataMember] public int uncategorizedTransactionsCount { get; set; }
            [DataMember] public string debitCardNumber { get; set; }
            [DataMember] public string counterpartyAccountNumbers { get; set; }
            [DataMember] public string navigationDataText { get; set; }
            [DataMember] public bool shouldOverWiteLocalStorage { get; set; }
            [DataMember] public bool showHostModeView { get; set; }
            [DataMember] public bool showPrintoutOption { get; set; }
            [DataMember] public bool showPfm { get; set; }
            [DataMember] public DateTime pfmStartDate { get; set; }
            [DataMember] public string budgetCategory { get; set; }
            [DataMember] public bool showDebitOperationTypes { get; set; }
            [DataMember] public bool showCreditOperationTypes { get; set; }
            [DataMember] public string selectedSuggestion { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseHistoryPfmProduct
        {
            [DataMember] public string id { get; set; }
            [DataMember] public string contractNumber { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string productName { get; set; }
            [DataMember] public string subTitle { get; set; }
            [DataMember] public bool isSelected { get; set; }
            [DataMember] public bool isInPfm { get; set; }
            [DataMember] public bool isInHost { get; set; }
            [DataMember] public string contractCurrency { get; set; }
            [DataMember] public string contractAlias { get; set; }
            [DataMember] public string productType { get; set; }
            [DataMember] public bool isClosedOrRestricted { get; set; }
            [DataMember] public bool isCardContract { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseHistoryPfmTransactionType
        {
            [DataMember] public string value { get; set; }
            [DataMember] public string label { get; set; }
            [DataMember] public bool isSelected { get; set; }
            [DataMember] public List<mBankJsonResponseHistoryPfmTransactionType> subtypes { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseHistoryCategory
        {
            [DataMember] public string id { get; set; }
            [DataMember] public string name { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTransactions : mBankJsonResponseBase
        {
            [DataMember] public List<mBankJsonResponseTransactionsTransaction> transactions { get; set; }
            [DataMember] public string nextPageUrl { get; set; }
            [DataMember] public int totalOperationsCount { get; set; }
            [DataMember] public int pageCount { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTransactionsTransaction
        {
            [DataMember] public long pfmId { get; set; }
            [DataMember] public int operationNumber { get; set; }
            [DataMember] public string operationCode { get; set; }
            [DataMember] public string accountNumber { get; set; }
            [DataMember] public string accountName { get; set; }
            [DataMember] public string operationType { get; set; }
            [DataMember] public string description { get; set; }
            [DataMember] public string subDescription { get; set; }
            [DataMember] public string subAccountDescription { get; set; }
            [DataMember] public string categoryId { get; set; }
            [DataMember] public string comment { get; set; }
            [DataMember] public string category { get; set; }
            [DataMember] public bool isIrrelevant { get; set; }
            [DataMember] public double amount { get; set; }
            [DataMember] public double balance { get; set; }
            [DataMember] public long merchant { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public DateTime transactionDate { get; set; }
            [DataMember] public DateTime originalTransactionDate { get; set; }
            [DataMember] public bool showOkIcon { get; set; }
            [DataMember] public bool isSplit { get; set; }
            [DataMember] public bool showAcceptedIcon { get; set; }
            [DataMember] public List<string> tags { get; set; }
            [DataMember] public string standingOrder { get; set; }

            public mBankJsonOperationCode? OperationCodeValue
            {
                get { return operationCode.GetEnumByJsonValue<mBankJsonOperationCode>(); }
                set { operationCode = value.GetEnumJsonValue<mBankJsonOperationCode>(); }
            }
            public mBankJsonOperationType? OperationTypeValue
            {
                //TODO GetEnumByJsonValueNoEmpty + other places
                get { return operationType.GetEnumByJsonValue<mBankJsonOperationType>(); }
                set { operationType = value.GetEnumJsonValue<mBankJsonOperationType>(); }
            }
            public mBankJsonCategory? CategoryValue
            {
                get { return category.GetEnumByJsonValue<mBankJsonCategory>(); }
                set { category = value.GetEnumJsonValue<mBankJsonCategory>(); }
            }
        }

        [DataContract]
        public class mBankJsonResponseTransaction : mBankJsonResponseBase
        {
            [DataMember] public int id { get; set; }
            [DataMember] public string type { get; set; }
            [DataMember] public Dictionary<string, mBankJsonResponseTransactionDetails> details { get; set; }
            [DataMember] public bool canRetry { get; set; }
            [DataMember] public bool canReply { get; set; }
            [DataMember] public bool canPrint { get; set; }
            [DataMember] public bool canSave { get; set; }
            [DataMember] public bool canRevertPayment { get; set; }
            [DataMember] public bool canConditionsPrint { get; set; }
            [DataMember] public string splitOverview { get; set; }

            public mBankJsonOperationType? OperationTypeValue
            {
                get { return type.GetEnumByJsonValue<mBankJsonOperationType>(); }
                set { type = value.GetEnumJsonValue<mBankJsonOperationType>(); }
            }
        }

        [DataContract]
        public class mBankJsonResponseTransactionDetails
        {
            [DataMember] public string label { get; set; }
            [DataMember] public string key { get; set; }
            [DataMember] public string value { get; set; }
            [DataMember] public bool mapped { get; set; }
        }

        [DataContract]
        public class mBankJsonResponsePhoneCharge : mBankJsonResponseBase
        {
            [DataMember] public List<mBankJsonResponsePhoneChargeAccount> fromAccounts { get; set; }
            [DataMember] public List<mBankJsonResponsePhoneChargeOperator> operators { get; set; }
            [DataMember] public double chargeAmount { get; set; }
            [DataMember] public DateTime operationDate { get; set; }
            [DataMember] public string regulationFileLink { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public bool isFirmAccount { get; set; }
            [DataMember] public mBankJsonResponsePhoneChargeAddressBookNavigationData addressBookNavigationData { get; set; }
            [DataMember] public string fromAccount { get; set; }
        }

        [DataContract]
        public class mBankJsonResponsePhoneChargeAccount
        {
            [DataMember] public double balance { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public string displayName { get; set; }
            [DataMember] public string number { get; set; }
        }

        [DataContract]
        public class mBankJsonResponsePhoneChargeOperator
        {
            [DataMember] public string name { get; set; }
            [DataMember] public string operatorId { get; set; }
            [DataMember] public string minValue { get; set; }
            [DataMember] public string maxValue { get; set; }
            [DataMember] public List<mBankJsonResponsePhoneChargeOperatorAmount> amounts { get; set; }
            [DataMember] public string mTransferId { get; set; }

            public double MinValue => Double.Parse(minValue);
            public double MaxValue => Double.Parse(maxValue);
        }

        [DataContract]
        public class mBankJsonResponsePhoneChargeOperatorAmount
        {
            [DataMember] public string display { get; set; }
            [DataMember] public double value { get; set; }
        }

        [DataContract]
        public class mBankJsonResponsePhoneChargeAddressBookNavigationData
        {
            [DataMember] public string id { get; set; }
            [DataMember] public string number { get; set; }
            [DataMember] public bool isMyContact { get; set; }
            [DataMember] public bool isTrusted { get; set; }
            [DataMember] public string recipientId { get; set; }
            [DataMember] public string recipientName { get; set; }
            [DataMember] public string contactType { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseInitPrepare : mBankJsonResponseBase
        {
            [DataMember] public string AuthMode { get; set; }
            [DataMember] public string ListNumber { get; set; }
            [DataMember] public int TanNumber { get; set; }
            [DataMember] public DateTime OperationDate { get; set; }
            [DataMember] public int OperationNumber { get; set; }
            [DataMember] public string TranId { get; set; }
            [DataMember] public string DeviceName { get; set; }
            [DataMember] public bool MultiDevice { get; set; }
            //TODO enum
            [DataMember] public string Status { get; set; }
            [DataMember] public bool Pending { get; set; }
            [DataMember] public int StatusCheckInterval { get; set; }
            [DataMember] public mBankJsonResponseInitPrepareData Data { get; set; }
            [DataMember] public string AuthData { get; set; }
            [DataMember] public string Message { get; set; }
            [DataMember] public string ErrorCode { get; set; }

            public mBankJsonAuthorizationMode? AuthorizationModeValue
            {
                get { return AuthMode.GetEnumByJsonValue<mBankJsonAuthorizationMode>(); }
                set { AuthMode = value.GetEnumJsonValue<mBankJsonAuthorizationMode>(); }
            }
        }

        [DataContract]
        public class mBankJsonResponseInitPrepareData
        {
            [DataMember] public double chargeAmount { get; set; }
            [DataMember] public string phoneNumber { get; set; }
            [DataMember] public string templateId { get; set; }
            [DataMember] public string operatorId { get; set; }
            [DataMember] public string operatorName { get; set; }
            [DataMember] public double amount { get; set; }
            [DataMember] public DateTime transactionDate { get; set; }
            [DataMember] public string fromAccount { get; set; }
            [DataMember] public bool addToAddressBook { get; set; }
            [DataMember] public string addToAddressBookName { get; set; }
            [DataMember] public bool addToSmsCharge { get; set; }
            [DataMember] public string currency { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseDomestic : mBankJsonResponseBase
        {
            [DataMember] public List<string> eeaCountries { get; set; }
            [DataMember] public List<string> availableCurrencies { get; set; }
            [DataMember] public List<mBankJsonResponseDomesticAccount> availableAccounts { get; set; }
            [DataMember] public List<string> availableCreditCards { get; set; }
            [DataMember] public List<DateTime> unavailableDates { get; set; }
            [DataMember] public List<mBankJsonResponseDomesticTransferMode> transferModes { get; set; }
            [DataMember] public string defaultAccount { get; set; }
            [DataMember] public string defaultEmail { get; set; }
            [DataMember] public string czSkData { get; set; }
            [DataMember] public mBankJsonResponseDomesticTransferData transferData { get; set; }
            [DataMember] public bool isTransferDateFixed { get; set; }
            [DataMember] public bool hasTemplateAccountForOtherProfile { get; set; }
            [DataMember] public bool isBasketModified { get; set; }
            [DataMember] public DateTime timestamp { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseDomesticAccount
        {
            [DataMember] public mBankJsonResponseAmount amount { get; set; }
            [DataMember] public mBankJsonResponseAmount ownFunds { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string fullName { get; set; }
            [DataMember] public string userAccountName { get; set; }
            [DataMember] public string maskedNumber { get; set; }
            [DataMember] public List<mBankJsonResponseDomesticAccountCoowner> coowners { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public bool hasTemplateAccountForOtherProfile { get; set; }
            [DataMember] public bool isCurrencyAccount { get; set; }
            [DataMember] public bool isSavingsAccount { get; set; }
            [DataMember] public string productType { get; set; }
            [DataMember] public string number { get; set; }
            [DataMember] public string cardNumber { get; set; }
            [DataMember] public bool isCard { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseDomesticAccountCoowner
        {
            [DataMember] public string coownerId { get; set; }
            [DataMember] public string displayName { get; set; }
            [DataMember] public string address { get; set; }
            [DataMember] public string city { get; set; }
            [DataMember] public string email { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseDomesticTransferMode
        {
            [DataMember] public string transferModeType { get; set; }
            [DataMember] public string transferType { get; set; }
            [DataMember] public double cost { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public bool isCostApproximate { get; set; }
            [DataMember] public bool shallUsePoints { get; set; }
            [DataMember] public double pointsCurrentTotal { get; set; }
            [DataMember] public double pointsTransactionCost { get; set; }
            [DataMember] public bool isFromEea { get; set; }
            [DataMember] public string commissionsAndFees { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseDomesticTransferData
        {
            [DataMember] public string fromAccount { get; set; }
            [DataMember] public string toAccount { get; set; }
            [DataMember] public string perfToken { get; set; }
            [DataMember] public mBankJsonResponseAmount amount { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public mBankJsonResponseDomesticTransferDataCzSkTransferData czSkTransferData { get; set; }
            [DataMember] public string bankDetails { get; set; }
            [DataMember] public string receiverName { get; set; }
            [DataMember] public string receiverAddress { get; set; }
            [DataMember] public string receiverCity { get; set; }
            [DataMember] public string operationDate { get; set; }
            [DataMember] public bool showRepeatFromBasketWarning { get; set; }
            [DataMember] public string addressBookContactInfo { get; set; }
            [DataMember] public string bicCode { get; set; }
            [DataMember] public string bankInfo { get; set; }
            [DataMember] public string countryCode { get; set; }
            [DataMember] public string source { get; set; }
            [DataMember] public string postActionRedirectUrl { get; set; }
            [DataMember] public string receiverNip { get; set; }
            [DataMember] public string paymentId { get; set; }
            [DataMember] public string proposalId { get; set; }
            [DataMember] public string paymentSource { get; set; }
            [DataMember] public string reference { get; set; }
            [DataMember] public string senderName { get; set; }
            [DataMember] public bool blockFunds { get; set; }
            [DataMember] public string emailAddressFirst { get; set; }
            [DataMember] public string emailAddressSecond { get; set; }
            [DataMember] public string suggestedTransferType { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseDomesticTransferDataCzSkTransferData
        {
            [DataMember] public string reference { get; set; }
            [DataMember] public string constantSymbol { get; set; }
            [DataMember] public string specificSymbol { get; set; }
            [DataMember] public string variableSymbol { get; set; }
            [DataMember] public string incomeSource { get; set; }
            [DataMember] public string paymentPurpose { get; set; }
            [DataMember] public bool isPep { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTransferCheck : mBankJsonResponseBase
        {
            [DataMember] public List<mBankJsonResponseTransferCheckAccount> availableAccounts { get; set; }
            [DataMember] public List<string> availableCreditCards { get; set; }
            [DataMember] public string perfToken { get; set; }
            [DataMember] public mBankJsonResponseTransferCheckBankDetails bankDetails { get; set; }
            [DataMember] public List<string> availableCurrencies { get; set; }
            [DataMember] public DateTime transferDate { get; set; }
            [DataMember] public bool isTransferDateFixed { get; set; }
            [DataMember] public List<mBankJsonResponseDomesticTransferMode> transferModes { get; set; }
            [DataMember] public string selectedTransferType { get; set; }
            //TODO enum
            [DataMember] public string redirectToForm { get; set; }
            [DataMember] public bool isPepTransfer { get; set; }
            [DataMember] public bool isSepaAvailable { get; set; }
            [DataMember] public bool isGlobusOffHours { get; set; }
            [DataMember] public bool isPeriodicTransferAvailable { get; set; }
            [DataMember] public bool isStandingOrderAvailable { get; set; }
            [DataMember] public bool isSeriesAvailable { get; set; }
            [DataMember] public mBankJsonResponseTransferCheckAdditionalOptions additionalOptions { get; set; }
            [DataMember] public bool isInstantPaymentAvailableFromAccount { get; set; }
            [DataMember] public bool isReceiverAccountCzBreGroupIban { get; set; }
            [DataMember] public bool isInstantPaymentAmountExceeded { get; set; }
            [DataMember] public string currencyRateInfo { get; set; }
            [DataMember] public DateTime timestamp { get; set; }
            [DataMember] public string checkDataId { get; set; }
            [DataMember] public string exchangeMargin { get; set; }
        }

        //TODO common part with account
        [DataContract]
        public class mBankJsonResponseTransferCheckAccount
        {
            [DataMember] public string number { get; set; }
            [DataMember] public string cardNumber { get; set; }
            [DataMember] public bool isCard { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTransferCheckBankDetails
        {
            [DataMember] public string bic { get; set; }
            [DataMember] public string bankCode { get; set; }
            [DataMember] public string branch { get; set; }
            [DataMember] public string status { get; set; }
            [DataMember] public string countryCode { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string city { get; set; }
            [DataMember] public string address { get; set; }
            [DataMember] public string vsamKey { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTransferCheckAdditionalOptions
        {
            [DataMember] public bool showBlockFunds { get; set; }
            [DataMember] public bool showSendConfirmations { get; set; }
            [DataMember] public bool showChangeAccountFee { get; set; }
            [DataMember] public bool showAddToAddressBook { get; set; }
            [DataMember] public bool showAddToBasket { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTaxFormType
        {
            [DataMember] public string formName { get; set; }
            [DataMember] public string beDate { get; set; }
            [DataMember] public string enDate { get; set; }
            [DataMember] public string accType { get; set; }
            [DataMember] public bool requiresPP { get; set; }
            [DataMember] public bool isVatCompliant { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTaxTransferPrepare : mBankJsonResponseBase
        {
            [DataMember] public mBankJsonResponseTaxTransferPrepareFormData formData { get; set; }
            [DataMember] public mBankJsonResponseTaxTransferPrepareSidebarData sidebarData { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTaxTransferPrepareFormData
        {
            [DataMember] public string tasksCenterData { get; set; }
            [DataMember] public string templateId { get; set; }
            [DataMember] public string accountParams { get; set; }
            [DataMember] public string perfToken { get; set; }
            [DataMember] public string fromAccount { get; set; }
            [DataMember] public string sender { get; set; }
            [DataMember] public string commitmentId { get; set; }
            [DataMember] public string amount { get; set; }
            [DataMember] public mBankJsonResponseTaxTransferPrepareFormDataIdType idType { get; set; }
            [DataMember] public mBankJsonResponseTaxTransferPrepareFormDataNameAndAddress nameAndAddress { get; set; }
            [DataMember] public mBankJsonResponseTaxTransferPrepareFormDataPeriod period { get; set; }
            [DataMember] public string taxAuthority { get; set; }
            [DataMember] public List<string> symbolsList { get; set; }
            [DataMember] public DateTime date { get; set; }
            [DataMember] public mBankJsonResponseTaxTransferPrepareFormDataAdditionalOptions additionalOptions { get; set; }
            [DataMember] public string formType { get; set; }
            [DataMember] public string paymentName { get; set; }
            [DataMember] public string repeatFor { get; set; }
            [DataMember] public List<string> whenFreeDay { get; set; }
            [DataMember] public string sendSMS { get; set; }
            [DataMember] public string tryAgain { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public string frequency { get; set; }
            [DataMember] public mBankJsonResponseTaxTransferPrepareFormDataIdTypesDict idTypesDict { get; set; }
            [DataMember] public bool requiresPP { get; set; }
            [DataMember] public bool showTaxAuthorityEditControls { get; set; }
            [DataMember] public string emails { get; set; }
            [DataMember] public bool saveAmountInTemplate { get; set; }
            [DataMember] public string templateName { get; set; }
            [DataMember] public mBankJsonResponseTaxTransferPrepareFormDataDefaultData defaultData { get; set; }
            [DataMember] public bool useCache { get; set; }
            [DataMember] public mBankJsonResponseTaxTransferPrepareFormDataLastCheckedValues lastCheckedValues { get; set; }
            [DataMember] public bool canAddToBasket { get; set; }
            [DataMember] public bool addToBasket { get; set; }
            [DataMember] public bool isTrusted { get; set; }
            [DataMember] public bool wasPerformed { get; set; }
            [DataMember] public bool isFromBasket { get; set; }
            [DataMember] public bool showRepeatFromBasketWarning { get; set; }
            [DataMember] public string srcAccountForLogging { get; set; }
            [DataMember] public bool isInvalid { get; set; }
            [DataMember] public string errorMessage { get; set; }
            [DataMember] public int source { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTaxTransferPrepareFormDataIdType
        {
            [DataMember] public string series { get; set; }
            [DataMember] public string type { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTaxTransferPrepareFormDataNameAndAddress
        {
            [DataMember] public string city { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string street { get; set; }
            [DataMember] public string receiverCountryCode { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTaxTransferPrepareFormDataPeriod
        {
            [DataMember] public string currentPeriod { get; set; }
            [DataMember] public int currentValue { get; set; }
            [DataMember] public int currentMonth { get; set; }
            [DataMember] public int currentYear { get; set; }
            [DataMember] public List<mBankJsonResponseTaxTransferPrepareFormDataPeriodValidator> validator { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTaxTransferPrepareFormDataPeriodValidator
        {
            [DataMember] public string period { get; set; }
            [DataMember] public double valueMax { get; set; }
            [DataMember] public double valueMin { get; set; }
            [DataMember] public bool valueVisible { get; set; }
            [DataMember] public bool valueMonth { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTaxTransferPrepareFormDataAdditionalOptions
        {
            [DataMember] public string addReceiver { get; set; }
            [DataMember] public bool blockResources { get; set; }
            [DataMember] public string changeAccountFee { get; set; }
            [DataMember] public bool sendConfirmation { get; set; }
            [DataMember] public List<string> sendConfirmationOptions { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTaxTransferPrepareFormDataIdTypesDict
        {
            [DataMember] public string activeType { get; set; }
            [DataMember] public List<mBankJsonResponseTaxTransferPrepareFormDataIdTypesDictType> types { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTaxTransferPrepareFormDataIdTypesDictType
        {
            [DataMember] public bool decisionNo { get; set; }
            [DataMember] public bool declarationNo { get; set; }
            [DataMember] public bool month { get; set; }
            [DataMember] public string type { get; set; }
            [DataMember] public string label { get; set; }
            [DataMember] public bool year { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTaxTransferPrepareFormDataDefaultData
        {
            [DataMember] public bool isMyContact { get; set; }
            [DataMember] public string recipientId { get; set; }
            [DataMember] public string fromAccount { get; set; }
            [DataMember] public string sender { get; set; }
            [DataMember] public List<string> availableCurrency { get; set; }
            [DataMember] public List<mBankJsonResponseTaxTransferPrepareFormDataDefaultDataAccount> availableFromAccounts { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public bool isDefined { get; set; }
            [DataMember] public bool isAddToAddressDisabled { get; set; }
            [DataMember] public string recipientName { get; set; }
            [DataMember] public DateTime dateNow { get; set; }
            [DataMember] public DateTime date { get; set; }
            [DataMember] public mBankJsonResponseCalendar calendar { get; set; }
            [DataMember] public bool blockResources { get; set; }
            [DataMember] public List<string> deliveryTime { get; set; }
            [DataMember] public List<string> zusAccounts { get; set; }
            [DataMember] public int itemsLimit { get; set; }
            [DataMember] public string userName { get; set; }
            [DataMember] public bool changedFromAcc { get; set; }
            [DataMember] public bool srcAccChangeToTemplate { get; set; }
            [DataMember] public List<string> soPeriodTypes { get; set; }
            [DataMember] public bool executionFromExternalModule { get; set; }
            [DataMember] public bool canAddToBasket { get; set; }
            [DataMember] public bool currentDateMockingEnabled { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTaxTransferPrepareFormDataDefaultDataAccount
        {
            [DataMember] public string accountParams { get; set; }
            [DataMember] public string activeCoownerId { get; set; }
            [DataMember] public string number { get; set; }
            [DataMember] public double balance { get; set; }
            [DataMember] public bool showBalance { get; set; }
            [DataMember] public double ownFunds { get; set; }
            [DataMember] public bool showOwnFunds { get; set; }
            [DataMember] public List<mBankJsonResponseTaxTransferPrepareFormDataDefaultDataAccountCoowner> coowners { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public string displayName { get; set; }
            [DataMember] public bool isCreditCard { get; set; }
            [DataMember] public bool isSavings { get; set; }
            [DataMember] public bool isVatCompliant { get; set; }
            [DataMember] public string relatedAccount { get; set; }
        }

        //TODO common part with mBankJsonResponseDomesticAccountCoowner
        [DataContract]
        public class mBankJsonResponseTaxTransferPrepareFormDataDefaultDataAccountCoowner
        {
            [DataMember] public string coownerId { get; set; }
            [DataMember] public string displayName { get; set; }
            [DataMember] public string street { get; set; }
            [DataMember] public string city { get; set; }
            [DataMember] public string participationType { get; set; }
            [DataMember] public string participationTypeName { get; set; }
            [DataMember] public string email { get; set; }
            [DataMember] public string regon { get; set; }
            [DataMember] public string nip { get; set; }
            [DataMember] public string companyShortName { get; set; }
            [DataMember] public string pesel { get; set; }
            [DataMember] public string personalID { get; set; }
            [DataMember] public string passport { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseCalendar
        {
            [DataMember] public double utcOffsetInMinutes { get; set; }
            [DataMember] public DateTime maxDate { get; set; }
            [DataMember] public DateTime minDate { get; set; }
            [DataMember] public List<DateTime> unavailableDates { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTaxTransferPrepareFormDataLastCheckedValues
        {
        }

        [DataContract]
        public class mBankJsonResponseTaxTransferPrepareSidebarData
        {
            [DataMember] public string activeTemplateId { get; set; }
            [DataMember] public bool isContactShared { get; set; }
            [DataMember] public List<mBankJsonResponseTaxTransferPrepareSidebarDataTemplateType> templateTypes { get; set; }
            [DataMember] public string sourceCategory { get; set; }
            [DataMember] public string receiverName { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTaxTransferPrepareSidebarDataTemplateType
        {
            [DataMember] public string contactId { get; set; }
            [DataMember] public string blankLabel { get; set; }
            [DataMember] public bool disabled { get; set; }
            [DataMember] public List<string> templatesSidebar { get; set; }
            [DataMember] public string type { get; set; }
            [DataMember] public string amountLimit { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTaxCity
        {
            [DataMember] public string taxAuthorityName { get; set; }
            [DataMember] public string taxAccount { get; set; }
        }

        [DataContract]
        public class mBankJsonResponseTaxAccount
        {
            [DataMember] public string taxAuthorityName { get; set; }
            [DataMember] public string taxAccount { get; set; }
        }
    }
}