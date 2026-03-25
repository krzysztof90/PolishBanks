using BankService.Bank_PL_Nest;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Tools;

namespace BankService.Bank_PL_MBank
{
    public class MBankJsonResponse
    {
        [DataContract]
        public class MBankJsonResponseBase
        {
        }

        [DataContract]
        public class MBankJsonResponseAmount
        {
            [DataMember] public double value { get; set; }
            [DataMember] public string currency { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseLogin : MBankJsonResponseBase
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
        public class MBankJsonResponseSetupData : MBankJsonResponseBase
        {
            [DataMember] public string entity { get; set; }
            [DataMember] public string antiForgeryToken { get; set; }
            [DataMember] public MBankJsonResponseSetupDataCustomer customer { get; set; }
            [DataMember] public MBankJsonResponseSetupDataProfile profile { get; set; }
            [DataMember] public MBankJsonResponseSetupDataFlags flags { get; set; }
            [DataMember] public MBankJsonResponseSetupDataLocalization localization { get; set; }
            [DataMember] public string veneziaCultureUrlPrefix { get; set; }
            [DataMember] public MBankJsonResponseSetupDataTracker tracker { get; set; }
            [DataMember] public MBankJsonResponseSetupDataConfiguration configuration { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseSetupDataCustomer
        {
            [DataMember] public MBankJsonResponseSetupDataCustomerConsents consents { get; set; }
            [DataMember] public string name { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseSetupDataCustomerConsents
        {
            [DataMember] public bool PR1 { get; set; }
            [DataMember] public bool PR2 { get; set; }
            [DataMember] public bool REK { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseSetupDataProfile
        {
            [DataMember] public string name { get; set; }
            [DataMember] public bool isPb { get; set; }
            [DataMember] public int type { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseSetupDataFlags
        {
            [DataMember] public bool AllowDemo { get; set; }
            [DataMember] public bool AllowTranslations { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseSetupDataLocalization
        {
            [DataMember] public List<string> languages { get; set; }
            [DataMember] public string selectedLanguage { get; set; }
            [DataMember] public string defaultLanguage { get; set; }
            [DataMember] public string veneziaCultureUrlPrefix { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseSetupDataTracker
        {
            [DataMember] public string cdnAddress { get; set; }
            [DataMember] public string trackerKey { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseSetupDataConfiguration
        {
            [DataMember] public MBankJsonResponseSetupDataConfigurationSessionTimer sessionTimer { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseSetupDataConfigurationSessionTimer
        {
            [DataMember] public int sessionTimeout { get; set; }
            [DataMember] public int timeLeftToShowAlert { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseAuthorizationData : MBankJsonResponseBase
        {
            [DataMember] public string ScaAuthorizationId { get; set; }
            [DataMember] public bool TrustedDeviceAddingAllowed { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseAuthorizationLocale : MBankJsonResponseBase
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
        public class MBankJsonResponseAuthorization : MBankJsonResponseBase
        {
            [DataMember] public MBankJsonResponseAuthorizationAuthorizationData authorizationData { get; set; }
            [DataMember] public string data { get; set; }
            [DataMember] public int operationTimeout { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseAuthorizationAuthorizationData
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

            public MBankJsonAuthorizationType? AuthorizationTypeValue
            {
                get => authorizationType.GetEnumByJsonValue<MBankJsonAuthorizationType>();
                set => authorizationType = value.GetEnumJsonValue<MBankJsonAuthorizationType>();
            }
        }

        [DataContract]
        public class MBankJsonResponseAuthorizationStatus : MBankJsonResponseBase
        {
            [DataMember] public string authorizationStatus { get; set; }
            [DataMember] public string authorizationId { get; set; }
            [DataMember] public MBankJsonResponseAuthorizePostResult postResult { get; set; }
            //TODO enum
            [DataMember] public string notificationMethod { get; set; }

            public MBankJsonAuthorizationStatus? AuthorizationStatusValue
            {
                get => authorizationStatus.GetEnumByJsonValue<MBankJsonAuthorizationStatus>();
                set => authorizationStatus = value.GetEnumJsonValue<MBankJsonAuthorizationStatus>();
            }
        }

        [DataContract]
        public class MBankJsonResponseAuthorizationTransferStatus : MBankJsonResponseBase
        {
            [DataMember] public string Status { get; set; }
            [DataMember] public string Message { get; set; }
            [DataMember] public string ErrorCode { get; set; }

            public MBankJsonAuthorizationTransferStatus? AuthorizationStatusValue
            {
                get => Status.GetEnumByJsonValue<MBankJsonAuthorizationTransferStatus>();
                set => Status = value.GetEnumJsonValue<MBankJsonAuthorizationTransferStatus>();
            }
        }

        [DataContract]
        public class MBankJsonResponseFinalizeAuthorization : MBankJsonResponseBase
        {
        }

        [DataContract]
        public class MBankJsonResponseAuthorize : MBankJsonResponseBase
        {
            [DataMember] public string type { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public int status { get; set; }
            [DataMember] public string detail { get; set; }
            [DataMember] public MBankJsonResponseAuthorizeErrors errors { get; set; }
            [DataMember] public string traceId { get; set; }
            [DataMember] public MBankJsonResponseAuthorizePostResult postResult { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseAuthorizeErrors
        {
        }

        [DataContract]
        public class MBankJsonResponseAuthorizePostResult
        {
            [DataMember] public MBankJsonResponseExecutePostResultData data { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseExecutePostResultData
        {
            [DataMember] public string scaAuthorizationId { get; set; }
            [DataMember] public string browserName { get; set; }
            [DataMember] public string browserVersion { get; set; }
            [DataMember] public string osName { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseExecute : MBankJsonResponseBase
        {
            [DataMember] public MBankJsonResponseExecutePostResult PostResult { get; set; }
            [DataMember] public string Message { get; set; }
            [DataMember] public string ErrorCode { get; set; }
            [DataMember] public string error { get; set; }
            [DataMember] public string source { get; set; }
            [DataMember] public string message { get; set; }
            [DataMember] public string type { get; set; }
            [DataMember] public MBankJsonResponseExecuteApiFault apiFault { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseExecuteApiFault
        {
            [DataMember] public string code { get; set; }
            [DataMember] public string message { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseExecutePostResult
        {
            [DataMember] public bool blockResources { get; set; }
            [DataMember] public MBankJsonResponseExecutePostResultOperationBasketStatus operationBasketStatus { get; set; }
            //TODO enum
            [DataMember] public string transferTimeType { get; set; }
            [DataMember] public MBankJsonResponseDomesticAccount receiverAccount { get; set; }
            [DataMember] public MBankJsonResponseDomesticAccount senderAccount { get; set; }
            [DataMember] public MBankJsonResponseAmount transferAmount { get; set; }
            [DataMember] public bool isTransactionWithheld { get; set; }
            [DataMember] public bool showRepeatFromBasketWarning { get; set; }
            [DataMember] public string operationWarningsAndErrors { get; set; }
            [DataMember] public string referenceNumber { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseExecutePostResultOperationBasketStatus
        {
            [DataMember] public bool firstAuthLimitReached { get; set; }
            [DataMember] public bool secondAuthLimitReached { get; set; }
            [DataMember] public bool addToBasket { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTrustedDevicesCheck : MBankJsonResponseBase
        {
            [DataMember] public bool isValid { get; set; }
            [DataMember] public string deviceName { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTrustedDevicesAdd : MBankJsonResponseBase
        {
            [DataMember] public bool isValid { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseUserSettings : MBankJsonResponseBase
        {
            [DataMember] public List<MBankJsonResponseUserSettingsProduct> products { get; set; }
            [DataMember] public bool isWospEnabled { get; set; }
            [DataMember] public bool isSupportUkraineEnabled { get; set; }
            [DataMember] public bool isWospFloodReliefEnabled { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseUserSettingsProduct
        {
            [DataMember] public string id { get; set; }
            [DataMember] public string productType { get; set; }
            [DataMember] public int? order { get; set; }
        }

        //TODO common part with MBankJsonResponseUserSettings
        [DataContract]
        public class MBankJsonResponseProducts : MBankJsonResponseBase
        {
            [DataMember] public List<MBankJsonResponseProductsProduct> products { get; set; }
            [DataMember] public bool isWospEnabled { get; set; }
            [DataMember] public bool isSupportUkraineEnabled { get; set; }
            [DataMember] public bool isWospFloodReliefEnabled { get; set; }
        }

        //TODO covers with MBankJsonResponseUserSettingsProduct
        [DataContract]
        public class MBankJsonResponseProductsProduct : MBankJsonResponseUserSettingsProduct
        {
            [DataMember] public string currentMonthExpenditure { get; set; }
            [DataMember] public string upcomingPayments { get; set; }
            [DataMember] public double ownFunds { get; set; }
            [DataMember] public string number { get; set; }
            [DataMember] public double AvailableBalance { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public string additionalName { get; set; }
            [DataMember] public MBankJsonResponseAmount balance { get; set; }
            [DataMember] public string nameAddition { get; set; }
            [DataMember] public string accountNumberToRedirect { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string shortName { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseExtendSession : MBankJsonResponseBase
        {
            [DataMember] public bool success { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseHistory : MBankJsonResponseBase
        {
            [DataMember] public List<MBankJsonResponseHistoryPfmProduct> pfmProducts { get; set; }
            //TODO datetime?
            [DataMember] public string dateFrom { get; set; }
            [DataMember] public string dateTo { get; set; }
            [DataMember] public List<MBankJsonResponseHistoryPfmTransactionType> pfmTransactionTypes { get; set; }
            [DataMember] public List<MBankJsonResponseHistoryCategory> categories { get; set; }
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
        public class MBankJsonResponseHistoryPfmProduct
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
        public class MBankJsonResponseHistoryPfmTransactionType
        {
            [DataMember] public string value { get; set; }
            [DataMember] public string label { get; set; }
            [DataMember] public bool isSelected { get; set; }
            [DataMember] public List<MBankJsonResponseHistoryPfmTransactionType> subtypes { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseHistoryCategory
        {
            [DataMember] public string id { get; set; }
            [DataMember] public string name { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTransactions : MBankJsonResponseBase
        {
            [DataMember] public List<MBankJsonResponseTransactionsTransaction> transactions { get; set; }
            [DataMember] public string nextPageUrl { get; set; }
            [DataMember] public int totalOperationsCount { get; set; }
            [DataMember] public int pageCount { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTransactionsTransaction
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

            public MBankJsonOperationCode? OperationCodeValue
            {
                get => operationCode.GetEnumByJsonValue<MBankJsonOperationCode>();
                set => operationCode = value.GetEnumJsonValue<MBankJsonOperationCode>();
            }
            public MBankJsonOperationType? OperationTypeValue
            {
                //TODO GetEnumByJsonValueNoEmpty + other places
                get => operationType.GetEnumByJsonValue<MBankJsonOperationType>();
                set => operationType = value.GetEnumJsonValue<MBankJsonOperationType>();
            }
            public MBankJsonCategory? CategoryValue
            {
                get => category.GetEnumByJsonValue<MBankJsonCategory>();
                set => category = value.GetEnumJsonValue<MBankJsonCategory>();
            }
        }

        [DataContract]
        public class MBankJsonResponseTransaction : MBankJsonResponseBase
        {
            [DataMember] public int id { get; set; }
            [DataMember] public string type { get; set; }
            [DataMember] public Dictionary<string, MBankJsonResponseTransactionDetails> details { get; set; }
            [DataMember] public bool canRetry { get; set; }
            [DataMember] public bool canReply { get; set; }
            [DataMember] public bool canPrint { get; set; }
            [DataMember] public bool canSave { get; set; }
            [DataMember] public bool canRevertPayment { get; set; }
            [DataMember] public bool canConditionsPrint { get; set; }
            [DataMember] public string splitOverview { get; set; }

            public MBankJsonOperationType? OperationTypeValue
            {
                get => type.GetEnumByJsonValue<MBankJsonOperationType>();
                set => type = value.GetEnumJsonValue<MBankJsonOperationType>();
            }
        }

        [DataContract]
        public class MBankJsonResponseTransactionDetails
        {
            [DataMember] public string label { get; set; }
            [DataMember] public string key { get; set; }
            [DataMember] public string value { get; set; }
            [DataMember] public bool mapped { get; set; }
        }

        [DataContract]
        public class MBankJsonResponsePhoneCharge : MBankJsonResponseBase
        {
            [DataMember] public List<MBankJsonResponsePhoneChargeAccount> fromAccounts { get; set; }
            [DataMember] public List<MBankJsonResponsePhoneChargeOperator> operators { get; set; }
            [DataMember] public double chargeAmount { get; set; }
            [DataMember] public DateTime operationDate { get; set; }
            [DataMember] public string regulationFileLink { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public bool isFirmAccount { get; set; }
            [DataMember] public MBankJsonResponsePhoneChargeAddressBookNavigationData addressBookNavigationData { get; set; }
            [DataMember] public string fromAccount { get; set; }
        }

        [DataContract]
        public class MBankJsonResponsePhoneChargeAccount
        {
            [DataMember] public double balance { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public string displayName { get; set; }
            [DataMember] public string number { get; set; }
        }

        [DataContract]
        public class MBankJsonResponsePhoneChargeOperator
        {
            [DataMember] public string name { get; set; }
            [DataMember] public string operatorId { get; set; }
            [DataMember] public string minValue { get; set; }
            [DataMember] public string maxValue { get; set; }
            [DataMember] public List<MBankJsonResponsePhoneChargeOperatorAmount> amounts { get; set; }
            [DataMember] public string mTransferId { get; set; }

            public double MinValue => Double.Parse(minValue);
            public double MaxValue => Double.Parse(maxValue);
        }

        [DataContract]
        public class MBankJsonResponsePhoneChargeOperatorAmount
        {
            [DataMember] public string display { get; set; }
            [DataMember] public double value { get; set; }
        }

        [DataContract]
        public class MBankJsonResponsePhoneChargeAddressBookNavigationData
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
        public class MBankJsonResponseInitPrepare : MBankJsonResponseBase
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
            [DataMember] public MBankJsonResponseInitPrepareData Data { get; set; }
            [DataMember] public string AuthData { get; set; }
            [DataMember] public string Message { get; set; }
            [DataMember] public string ErrorCode { get; set; }

            public MBankJsonAuthorizationMode? AuthorizationModeValue
            {
                get => AuthMode.GetEnumByJsonValue<MBankJsonAuthorizationMode>();
                set => AuthMode = value.GetEnumJsonValue<MBankJsonAuthorizationMode>();
            }
        }

        [DataContract]
        public class MBankJsonResponseInitPrepareData
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
        public class MBankJsonResponseDomestic : MBankJsonResponseBase
        {
            [DataMember] public List<string> eeaCountries { get; set; }
            [DataMember] public List<string> availableCurrencies { get; set; }
            [DataMember] public List<MBankJsonResponseDomesticAccount> availableAccounts { get; set; }
            [DataMember] public List<string> availableCreditCards { get; set; }
            [DataMember] public List<DateTime> unavailableDates { get; set; }
            [DataMember] public List<MBankJsonResponseDomesticTransferMode> transferModes { get; set; }
            [DataMember] public string defaultAccount { get; set; }
            [DataMember] public string defaultEmail { get; set; }
            [DataMember] public string czSkData { get; set; }
            [DataMember] public MBankJsonResponseDomesticTransferData transferData { get; set; }
            [DataMember] public bool isTransferDateFixed { get; set; }
            [DataMember] public bool hasTemplateAccountForOtherProfile { get; set; }
            [DataMember] public bool isBasketModified { get; set; }
            [DataMember] public DateTime timestamp { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseDomesticAccount
        {
            [DataMember] public MBankJsonResponseAmount amount { get; set; }
            [DataMember] public MBankJsonResponseAmount ownFunds { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string fullName { get; set; }
            [DataMember] public string userAccountName { get; set; }
            [DataMember] public string maskedNumber { get; set; }
            [DataMember] public List<MBankJsonResponseDomesticAccountCoowner> coowners { get; set; }
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
        public class MBankJsonResponseDomesticAccountCoowner
        {
            [DataMember] public string coownerId { get; set; }
            [DataMember] public string displayName { get; set; }
            [DataMember] public string address { get; set; }
            [DataMember] public string city { get; set; }
            [DataMember] public string email { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseDomesticTransferMode
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
        public class MBankJsonResponseDomesticTransferData
        {
            [DataMember] public string fromAccount { get; set; }
            [DataMember] public string toAccount { get; set; }
            [DataMember] public string perfToken { get; set; }
            [DataMember] public MBankJsonResponseAmount amount { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public MBankJsonResponseDomesticTransferDataCzSkTransferData czSkTransferData { get; set; }
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
        public class MBankJsonResponseDomesticTransferDataCzSkTransferData
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
        public class MBankJsonResponseTransferCheck : MBankJsonResponseBase
        {
            [DataMember] public List<MBankJsonResponseTransferCheckAccount> availableAccounts { get; set; }
            [DataMember] public List<string> availableCreditCards { get; set; }
            [DataMember] public string perfToken { get; set; }
            [DataMember] public MBankJsonResponseTransferCheckBankDetails bankDetails { get; set; }
            [DataMember] public List<string> availableCurrencies { get; set; }
            [DataMember] public DateTime transferDate { get; set; }
            [DataMember] public bool isTransferDateFixed { get; set; }
            [DataMember] public List<MBankJsonResponseDomesticTransferMode> transferModes { get; set; }
            [DataMember] public string selectedTransferType { get; set; }
            //TODO enum
            [DataMember] public string redirectToForm { get; set; }
            [DataMember] public bool isPepTransfer { get; set; }
            [DataMember] public bool isSepaAvailable { get; set; }
            [DataMember] public bool isGlobusOffHours { get; set; }
            [DataMember] public bool isPeriodicTransferAvailable { get; set; }
            [DataMember] public bool isStandingOrderAvailable { get; set; }
            [DataMember] public bool isSeriesAvailable { get; set; }
            [DataMember] public MBankJsonResponseTransferCheckAdditionalOptions additionalOptions { get; set; }
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
        public class MBankJsonResponseTransferCheckAccount
        {
            [DataMember] public string number { get; set; }
            [DataMember] public string cardNumber { get; set; }
            [DataMember] public bool isCard { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTransferCheckBankDetails
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
        public class MBankJsonResponseTransferCheckAdditionalOptions
        {
            [DataMember] public bool showBlockFunds { get; set; }
            [DataMember] public bool showSendConfirmations { get; set; }
            [DataMember] public bool showChangeAccountFee { get; set; }
            [DataMember] public bool showAddToAddressBook { get; set; }
            [DataMember] public bool showAddToBasket { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTaxFormType
        {
            [DataMember] public string formName { get; set; }
            [DataMember] public string beDate { get; set; }
            [DataMember] public string enDate { get; set; }
            [DataMember] public string accType { get; set; }
            [DataMember] public bool requiresPP { get; set; }
            [DataMember] public bool isVatCompliant { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTaxTransferPrepare : MBankJsonResponseBase
        {
            [DataMember] public MBankJsonResponseTaxTransferPrepareFormData formData { get; set; }
            [DataMember] public MBankJsonResponseTaxTransferPrepareSidebarData sidebarData { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTaxTransferPrepareFormData
        {
            [DataMember] public string tasksCenterData { get; set; }
            [DataMember] public string templateId { get; set; }
            [DataMember] public string accountParams { get; set; }
            [DataMember] public string perfToken { get; set; }
            [DataMember] public string fromAccount { get; set; }
            [DataMember] public string sender { get; set; }
            [DataMember] public string commitmentId { get; set; }
            [DataMember] public string amount { get; set; }
            [DataMember] public MBankJsonResponseTaxTransferPrepareFormDataIdType idType { get; set; }
            [DataMember] public MBankJsonResponseTaxTransferPrepareFormDataNameAndAddress nameAndAddress { get; set; }
            [DataMember] public MBankJsonResponseTaxTransferPrepareFormDataPeriod period { get; set; }
            [DataMember] public string taxAuthority { get; set; }
            [DataMember] public List<string> symbolsList { get; set; }
            [DataMember] public DateTime date { get; set; }
            [DataMember] public MBankJsonResponseTaxTransferPrepareFormDataAdditionalOptions additionalOptions { get; set; }
            [DataMember] public string formType { get; set; }
            [DataMember] public string paymentName { get; set; }
            [DataMember] public string repeatFor { get; set; }
            [DataMember] public List<string> whenFreeDay { get; set; }
            [DataMember] public string sendSMS { get; set; }
            [DataMember] public string tryAgain { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public string frequency { get; set; }
            [DataMember] public MBankJsonResponseTaxTransferPrepareFormDataIdTypesDict idTypesDict { get; set; }
            [DataMember] public bool requiresPP { get; set; }
            [DataMember] public bool showTaxAuthorityEditControls { get; set; }
            [DataMember] public string emails { get; set; }
            [DataMember] public bool saveAmountInTemplate { get; set; }
            [DataMember] public string templateName { get; set; }
            [DataMember] public MBankJsonResponseTaxTransferPrepareFormDataDefaultData defaultData { get; set; }
            [DataMember] public bool useCache { get; set; }
            [DataMember] public MBankJsonResponseTaxTransferPrepareFormDataLastCheckedValues lastCheckedValues { get; set; }
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
        public class MBankJsonResponseTaxTransferPrepareFormDataIdType
        {
            [DataMember] public string series { get; set; }
            [DataMember] public string type { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTaxTransferPrepareFormDataNameAndAddress
        {
            [DataMember] public string city { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string street { get; set; }
            [DataMember] public string receiverCountryCode { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTaxTransferPrepareFormDataPeriod
        {
            [DataMember] public string currentPeriod { get; set; }
            [DataMember] public int currentValue { get; set; }
            [DataMember] public int currentMonth { get; set; }
            [DataMember] public int currentYear { get; set; }
            [DataMember] public List<MBankJsonResponseTaxTransferPrepareFormDataPeriodValidator> validator { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTaxTransferPrepareFormDataPeriodValidator
        {
            [DataMember] public string period { get; set; }
            [DataMember] public double valueMax { get; set; }
            [DataMember] public double valueMin { get; set; }
            [DataMember] public bool valueVisible { get; set; }
            [DataMember] public bool valueMonth { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTaxTransferPrepareFormDataAdditionalOptions
        {
            [DataMember] public string addReceiver { get; set; }
            [DataMember] public bool blockResources { get; set; }
            [DataMember] public string changeAccountFee { get; set; }
            [DataMember] public bool sendConfirmation { get; set; }
            [DataMember] public List<string> sendConfirmationOptions { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTaxTransferPrepareFormDataIdTypesDict
        {
            [DataMember] public string activeType { get; set; }
            [DataMember] public List<MBankJsonResponseTaxTransferPrepareFormDataIdTypesDictType> types { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTaxTransferPrepareFormDataIdTypesDictType
        {
            [DataMember] public bool decisionNo { get; set; }
            [DataMember] public bool declarationNo { get; set; }
            [DataMember] public bool month { get; set; }
            [DataMember] public string type { get; set; }
            [DataMember] public string label { get; set; }
            [DataMember] public bool year { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTaxTransferPrepareFormDataDefaultData
        {
            [DataMember] public bool isMyContact { get; set; }
            [DataMember] public string recipientId { get; set; }
            [DataMember] public string fromAccount { get; set; }
            [DataMember] public string sender { get; set; }
            [DataMember] public List<string> availableCurrency { get; set; }
            [DataMember] public List<MBankJsonResponseTaxTransferPrepareFormDataDefaultDataAccount> availableFromAccounts { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public bool isDefined { get; set; }
            [DataMember] public bool isAddToAddressDisabled { get; set; }
            [DataMember] public string recipientName { get; set; }
            [DataMember] public DateTime dateNow { get; set; }
            [DataMember] public DateTime date { get; set; }
            [DataMember] public MBankJsonResponseCalendar calendar { get; set; }
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
        public class MBankJsonResponseTaxTransferPrepareFormDataDefaultDataAccount
        {
            [DataMember] public string accountParams { get; set; }
            [DataMember] public string activeCoownerId { get; set; }
            [DataMember] public string number { get; set; }
            [DataMember] public double balance { get; set; }
            [DataMember] public bool showBalance { get; set; }
            [DataMember] public double ownFunds { get; set; }
            [DataMember] public bool showOwnFunds { get; set; }
            [DataMember] public List<MBankJsonResponseTaxTransferPrepareFormDataDefaultDataAccountCoowner> coowners { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public string displayName { get; set; }
            [DataMember] public bool isCreditCard { get; set; }
            [DataMember] public bool isSavings { get; set; }
            [DataMember] public bool isVatCompliant { get; set; }
            [DataMember] public string relatedAccount { get; set; }
        }

        //TODO common part with MBankJsonResponseDomesticAccountCoowner
        [DataContract]
        public class MBankJsonResponseTaxTransferPrepareFormDataDefaultDataAccountCoowner
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
        public class MBankJsonResponseCalendar
        {
            [DataMember] public double utcOffsetInMinutes { get; set; }
            [DataMember] public DateTime maxDate { get; set; }
            [DataMember] public DateTime minDate { get; set; }
            [DataMember] public List<DateTime> unavailableDates { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTaxTransferPrepareFormDataLastCheckedValues
        {
        }

        [DataContract]
        public class MBankJsonResponseTaxTransferPrepareSidebarData
        {
            [DataMember] public string activeTemplateId { get; set; }
            [DataMember] public bool isContactShared { get; set; }
            [DataMember] public List<MBankJsonResponseTaxTransferPrepareSidebarDataTemplateType> templateTypes { get; set; }
            [DataMember] public string sourceCategory { get; set; }
            [DataMember] public string receiverName { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTaxTransferPrepareSidebarDataTemplateType
        {
            [DataMember] public string contactId { get; set; }
            [DataMember] public string blankLabel { get; set; }
            [DataMember] public bool disabled { get; set; }
            [DataMember] public List<string> templatesSidebar { get; set; }
            [DataMember] public string type { get; set; }
            [DataMember] public string amountLimit { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTaxCity
        {
            [DataMember] public string taxAuthorityName { get; set; }
            [DataMember] public string taxAccount { get; set; }
        }

        [DataContract]
        public class MBankJsonResponseTaxAccount
        {
            [DataMember] public string taxAuthorityName { get; set; }
            [DataMember] public string taxAccount { get; set; }
        }
    }
}