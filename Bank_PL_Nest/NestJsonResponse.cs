using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Tools;

namespace BankService.Bank_PL_Nest
{
    public class NestJsonResponse
    {
        [DataContract]
        public class NestJsonResponseBase
        {
            //TODO enum
            [DataMember] public string level { get; set; }
            [DataMember] public List<NestJsonResponseProblem> problems { get; set; }
            [DataMember] public List<NestJsonResponseError> errors { get; set; }
        }

        [DataContract]
        public class NestJsonResponseProblem
        {
            //TODO enum
            [DataMember] public string level { get; set; }
            //[DataMember] public NestJsonResponseProblemObjectId objectId { get; set; }
            //[DataMember] public List<NestJsonResponseProblemObjectId> objectIdsList { get; set; }
            [DataMember] public object objectId { get; set; }
            [DataMember] public List<object> objectIdsList { get; set; }
            [DataMember] public string objectName { get; set; }
            [DataMember] public List<object> messageParams { get; set; }
            [DataMember] public string propertyName { get; set; }
            [DataMember] public string propertyValue { get; set; }
            [DataMember] public string messageCode { get; set; }
            [DataMember] public string locale { get; set; }
            [DataMember] public string description { get; set; }
        }

        [DataContract]
        public class NestJsonResponseProblemObjectId
        {
            [DataMember] public int id { get; set; }
            [DataMember] public string version { get; set; }
        }

        [DataContract]
        public class NestJsonResponseWarning
        {
            //TODO enum
            [DataMember] public string code { get; set; }
            [DataMember] public string message { get; set; }
        }

        [DataContract]
        public class NestJsonResponseError : NestJsonResponseWarning
        {
            [DataMember] public string path { get; set; }
            [DataMember] public string userMessage { get; set; }
        }

        [DataContract]
        public class NestJsonResponseLogin : NestJsonResponseBase
        {
            [DataMember] public List<NestJsonResponseLoginAvatar> avatars { get; set; }
            [DataMember] public string loginProcess { get; set; }
            [DataMember] public int passwordLength { get; set; }
            [DataMember] public List<int> passwordKeys { get; set; }

            public NestJsonLoginProcess? LoginProcessValue
            {
                get { return loginProcess.GetEnumByJsonValue<NestJsonLoginProcess>(); }
                set { loginProcess = value.GetEnumJsonValue<NestJsonLoginProcess>(); }
            }
        }

        [DataContract]
        public class NestJsonResponseLoginAvatar
        {
            [DataMember] public int id { get; set; }
            [DataMember] public string url { get; set; }
            [DataMember] public string name { get; set; }
        }

        [DataContract]
        public class NestJsonResponsePassword : NestJsonResponseBase
        {
            [DataMember] public List<NestJsonResponsePasswordUserContext> userContexts { get; set; }
            [DataMember] public DateTime lastSuccessLoginDate { get; set; }
            [DataMember] public bool authorizationPasswordShouldBeSet { get; set; }
            [DataMember] public string authorizationPasswordEnabledFrom { get; set; }
            [DataMember] public bool userShouldSetAuthorizationToken { get; set; }
            [DataMember] public int sessionTimeoutValue { get; set; }
            [DataMember] public string storyblokToken { get; set; }
        }

        [DataContract]
        public class NestJsonResponsePasswordUserContext
        {
            [DataMember] public long id { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string type { get; set; }
            [DataMember] public string customerPesel { get; set; }
            [DataMember] public string customerRegon { get; set; }
            [DataMember] public string customerNip { get; set; }
            [DataMember] public string address { get; set; }
            [DataMember] public long customerId { get; set; }
            //TODO enum
            [DataMember] public string userRole { get; set; }
            [DataMember] public string salesforceContactKey { get; set; }
            [DataMember] public bool passiveAccess { get; set; }
            [DataMember] public bool selected { get; set; }
            [DataMember] public bool cf { get; set; }
        }

        [DataContract]
        public abstract class NestJsonResponsePrepareSign<T> : NestJsonResponseBase where T : NestJsonResponsePrepareSignObject
        {
            [DataMember] public List<T> objects { get; set; }

            //TODO date
            //TODO DateOnly
            [DataMember] public string authDate { get; set; }
            [DataMember] public int? authCountNumber { get; set; }
            [DataMember] public string authIdentifier { get; set; }
            [DataMember] public bool authorizationPasswordSet { get; set; }
            [DataMember] public bool authorizationPasswordEnabled { get; set; }
            [DataMember] public string authorizationMethod { get; set; }
            [DataMember] public string signatureSets { get; set; }
            [DataMember] public string operationName { get; set; }
            [DataMember] public NestJsonResponsePrepareSignSummaryData summaryData { get; set; }
            [DataMember] public string signPathSuffix { get; set; }
            [DataMember] public string confirmationData { get; set; }
            [DataMember] public string transactionData { get; set; }
            [DataMember] public string chosenSignatureSetName { get; set; }
            [DataMember] public string checkSum { get; set; }
            [DataMember] public bool canSendNotification { get; set; }
            [DataMember] public bool sendNotification { get; set; }
            [DataMember] public bool simpleAuthorization { get; set; }

            public NestJsonAuthorizationMethod? AuthorizationMethodValue
            {
                get { return authorizationMethod.GetEnumByJsonValue<NestJsonAuthorizationMethod>(); }
                set { authorizationMethod = value.GetEnumJsonValue<NestJsonAuthorizationMethod>(); }
            }
        }

        [DataContract]
        public class NestJsonResponsePrepareSignObject
        {
            //TODO different fields depending on value here + in every place where $objectType
            [DataMember(Name = "$objectType")] public string objectType { get; set; }
            [DataMember] public long id { get; set; }
            [DataMember] public string digest { get; set; }
            [DataMember] public string relatedContextId { get; set; }
            [DataMember] public string dataChangesLog { get; set; }
            [DataMember] public bool activeLogin { get; set; }
        }

        [DataContract]
        public class NestJsonResponsePrepareSignDashboardSca : NestJsonResponsePrepareSign<NestJsonResponsePrepareSignObjectDashboardSca>
        {
            //[DataMember] public List<NestJsonResponsePrepareSignObjectDashboardSca> objects { get; set; }
        }

        [DataContract]
        public class NestJsonResponsePrepareSignObjectDashboardSca : NestJsonResponsePrepareSignObject
        {
            [DataMember] public bool disableDashboardSca { get; set; }
            [DataMember] public List<NestJsonResponsePrepareSignObjectSignedProperty> signedProperties { get; set; }
        }

        [DataContract]
        public class NestJsonResponsePrepareSignOrder : NestJsonResponsePrepareSign<NestJsonResponsePrepareSignObjectOrder>
        {
            //[DataMember] public List<NestJsonResponsePrepareSignObjectOrder> objects { get; set; }
        }

        [DataContract]
        public class NestJsonResponsePrepareSignObjectOrder : NestJsonResponsePrepareSignObject
        {
            [DataMember] public long accountId { get; set; }
            [DataMember] public string cardId { get; set; }
            [DataMember] public string cntrFullName { get; set; }
            [DataMember] public string cntrAddress { get; set; }
            [DataMember] public string cntrAccountNo { get; set; }
            [DataMember] public bool? cntrTrusted { get; set; }
            [DataMember] public string cntrShortName { get; set; }
            [DataMember] public string cntrBankName { get; set; }
            [DataMember] public string senderFullName { get; set; }
            [DataMember] public string senderAddress { get; set; }
            [DataMember] public double amount { get; set; }
            [DataMember] public string currency { get; set; }
            //TODO date
            [DataMember] public string realizationDateByUser { get; set; }
            //TODO date
            [DataMember] public string entryDate { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string contractorId { get; set; }
            //TODO enum
            [DataMember] public string standingType { get; set; }
            [DataMember] public string standingOrderData { get; set; }
            [DataMember] public bool confirmation { get; set; }
            [DataMember] public string confirmationInfo { get; set; }
            [DataMember] public string geoCoordinates { get; set; }
            [DataMember] public bool clauseAccepted { get; set; }
            [DataMember] public string relatedOrderId { get; set; }
            [DataMember] public string standingOrderOperation { get; set; }
            [DataMember] public bool saveContractor { get; set; }
            [DataMember] public bool shouldBlockFunds { get; set; }
            [DataMember] public string chosenOrderId { get; set; }
            [DataMember] public string sourceAccountNo { get; set; }
            [DataMember] public string destinationAccountSubType { get; set; }
            [DataMember] public double fee { get; set; }
            [DataMember] public bool quickPayment { get; set; }
            [DataMember] public bool holdProcessed { get; set; }
            [DataMember] public bool psd2 { get; set; }
            [DataMember] public string tppTransactionId { get; set; }
            [DataMember] public string tppName { get; set; }
            [DataMember] public string psd2StandingType { get; set; }
            [DataMember] public string psd2FallbackPaymentId { get; set; }
            [DataMember] public bool currencyOrder { get; set; }
            [DataMember] public string tppApiKey { get; set; }
            [DataMember] public bool sendNotification { get; set; }
            [DataMember] public bool inDeleteProcess { get; set; }
            [DataMember] public string entryChannel { get; set; }
            [DataMember] public bool sorbnet { get; set; }
            [DataMember] public string vatAccountNrb { get; set; }
            [DataMember] public bool package { get; set; }
            //TODO date
            [DataMember] public string realizationDate { get; set; }
            //TODO enum
            [DataMember] public string orderType { get; set; }
        }

        [DataContract]
        public class NestJsonResponsePrepareSignObjectSignedProperty
        {
        }

        [DataContract]
        public class NestJsonResponsePrepareSignSummaryData
        {
            [DataMember] public string signOperation { get; set; }
            [DataMember] public List<NestJsonResponseWarning> realizationDate { get; set; }
            [DataMember] public List<NestJsonResponseWarning> smsWarning { get; set; }
            [DataMember] public NestJsonResponsePrepareSignSummaryDataCommission commission { get; set; }
            [DataMember] public string expectedDeliveryTime { get; set; }
        }

        [DataContract]
        public class NestJsonResponsePrepareSignSummaryDataCommission : NestJsonResponseWarning
        {
            [DataMember] public double commissionAmount { get; set; }
            [DataMember] public string commissionCurrency { get; set; }
        }

        [DataContract]
        public class NestJsonResponseSign : NestJsonResponseBase
        {
            [DataMember] public NestJsonResponseSignSummaryData summaryData { get; set; }
            [DataMember] public NestJsonResponseSignErrorData errorData { get; set; }
        }

        [DataContract]
        public class NestJsonResponseTrustedDeviceCheck : NestJsonResponseBase
        {
            [DataMember] public bool trustedDevice { get; set; }
            [DataMember] public bool canSaveTrustedDevice { get; set; }
            [DataMember] public bool dashboardSca { get; set; }
            //TODO enum
            [DataMember] public string userAuthorizationMethod { get; set; }
            [DataMember] public bool userShouldSetAuthorizationToken { get; set; }
        }

        [DataContract]
        public class NestJsonResponseTrustedDeviceSave : NestJsonResponseBase
        {
            [DataMember] public string hash { get; set; }
            [DataMember] public DateTime createDate { get; set; }
        }

        [DataContract]
        public class NestJsonResponseAuthorization : NestJsonResponseBase
        {
            [DataMember] public string status { get; set; }
            [DataMember] public string pushType { get; set; }
            [DataMember] public NestJsonResponseSign signResponse { get; set; }

            public NestJsonAuthorizationStatus? StatusValue
            {
                get { return status.GetEnumByJsonValue<NestJsonAuthorizationStatus>(); }
                set { status = value.GetEnumJsonValue<NestJsonAuthorizationStatus>(); }
            }

            public NestJsonAuthorizationPushType? PushTypeValue
            {
                get { return pushType.GetEnumByJsonValue<NestJsonAuthorizationPushType>(); }
                set { pushType = value.GetEnumJsonValue<NestJsonAuthorizationPushType>(); }
            }
        }

        [DataContract]
        public class NestJsonResponseSignSummaryData
        {
        }

        [DataContract]
        public class NestJsonResponseSignErrorData
        {
        }

        [DataContract]
        public class NestJsonResponseDashboardConfig : NestJsonResponseBase
        {
            [DataMember] public List<NestJsonResponseDashboardConfigAccount> accounts { get; set; }
            [DataMember] public NestJsonResponseDashboardConfigCardsLoans cards { get; set; }
            [DataMember] public bool userShouldSetAuthorizationToken { get; set; }
            [DataMember] public NestJsonResponseDashboardConfigCardsLoans loans { get; set; }
            [DataMember] public List<NestJsonResponseDashboardConfigDashboardItem> dashboardItems { get; set; }
            [DataMember] public bool dashboardSca { get; set; }
            [DataMember] public List<string> passedTerms { get; set; }
            [DataMember] public string userAuthorizationMethod { get; set; }
            [DataMember] public bool vindicated { get; set; }
        }

        [DataContract]
        public class NestJsonResponseDashboardConfigAccount
        {
            [DataMember] public long id { get; set; }
            [DataMember] public string nrb { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public double openingBalance { get; set; }
            [DataMember] public DateTime openingBalanceDate { get; set; }
            [DataMember] public double balance { get; set; }
            [DataMember] public DateTime balanceDate { get; set; }
            [DataMember] public double availableFunds { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public int version { get; set; }
            //TODO enum
            [DataMember] public string status { get; set; }
            //TODO enum
            [DataMember] public string customerType { get; set; }
            [DataMember] public bool unchargeable { get; set; }
            [DataMember] public bool purposefulAccount { get; set; }
            [DataMember] public bool goalAccount { get; set; }
            //TODO enum
            [DataMember] public string accountClassification { get; set; }
            [DataMember] public bool ownTransfersOnly { get; set; }
            [DataMember] public string accountType { get; set; }
            [DataMember] public DateTime accountOpeningDate { get; set; }
            [DataMember] public string balanceDiff { get; set; }
            [DataMember] public bool canCreatePocket { get; set; }
            [DataMember] public List<string> pockets { get; set; }
            [DataMember] public string prevLoginBalance { get; set; }
            [DataMember] public string vatAccountNrb { get; set; }
            [DataMember] public string freeAmountSum { get; set; }
            [DataMember] public double goalsCount { get; set; }
            [DataMember] public string goals { get; set; }
            [DataMember] public string goalsSummary { get; set; }
            [DataMember(Name = "default")] public bool defaultValue { get; set; }
            [DataMember] public bool vat { get; set; }
            [DataMember] public bool pad { get; set; }
            [DataMember] public bool ekid { get; set; }
            [DataMember] public bool savingAccount { get; set; }
        }

        [DataContract]
        public class NestJsonResponseDashboardConfigCardsLoans
        {
            [DataMember] public int pageCount { get; set; }
            [DataMember] public int count { get; set; }
            [DataMember] public long? pageSize { get; set; }
            [DataMember] public List<string> list { get; set; }
        }

        [DataContract]
        public class NestJsonResponseDashboardConfigDashboardItem
        {
            [DataMember(Name = "$objectType")] public string objectType { get; set; }
            [DataMember] public NestJsonResponseDashboardConfigDashboardItemData data { get; set; }
        }

        [DataContract]
        public class NestJsonResponseDashboardConfigDashboardItemData
        {
            [DataMember(Name = "$objectType")] public string objectType { get; set; }
            [DataMember] public List<NestJsonResponseDashboardConfigDashboardItemDataProduct> products { get; set; }
            [DataMember] public List<string> rejectedOperationSummaries { get; set; }
            [DataMember] public List<string> forApprovalOperationSummaries { get; set; }
            [DataMember] public int totalRejectedOperationCount { get; set; }
            [DataMember] public int totalForApprovalOperationCount { get; set; }
            [DataMember] public NestJsonResponseDashboardConfigDashboardItemDataApplicationsAndDispositionCounters applicationsAndDispositionCounters { get; set; }
            [DataMember] public List<string> creditCards { get; set; }
            [DataMember] public int totalCreditCardsDebit { get; set; }
            [DataMember] public List<string> loans { get; set; }
            [DataMember] public int totalLoanDebit { get; set; }
            [DataMember] public List<string> futureOrdersWithFailedBlockadePerAccount { get; set; }
            [DataMember] public int totalFutureOrdersWithFailedBlockadePerAccount { get; set; }
            [DataMember] public List<string> myFinancesTransferFunds { get; set; }
            [DataMember] public int? totalMyFinancesTransferFundsCount { get; set; }
            [DataMember] public NestJsonResponseDashboardConfigDashboardItemDataGoals goals { get; set; }
        }

        [DataContract]
        public class NestJsonResponseDashboardConfigDashboardItemDataProduct
        {
            [DataMember(Name = "$objectType")] public string objectType { get; set; }
            [DataMember] public long id { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public double availableAmount { get; set; }
            [DataMember] public string currency { get; set; }
            //TODO Parse DateTime
            [DataMember] public string balanceDate { get; set; }
            [DataMember] public double balance { get; set; }
            [DataMember] public List<NestJsonResponseDashboardConfigDashboardItemDataProductWaitingPayment> waitingPayments { get; set; }
            [DataMember] public string prevLoginBalance { get; set; }
            //TODO enum
            [DataMember] public string accountClassification { get; set; }
            [DataMember] public string lastOperations { get; set; }
        }

        [DataContract]
        public class NestJsonResponseDashboardConfigDashboardItemDataProductWaitingPayment
        {
            [DataMember] public double sumAmount { get; set; }
            [DataMember] public int count { get; set; }
            [DataMember] public string currency { get; set; }
        }

        [DataContract]
        public class NestJsonResponseDashboardConfigDashboardItemDataApplicationsAndDispositionCounters
        {
            [DataMember] public int submittedCnt { get; set; }
            [DataMember] public int savedCnt { get; set; }
            [DataMember] public int toSignCnt { get; set; }
        }

        [DataContract]
        public class NestJsonResponseDashboardConfigDashboardItemDataGoals
        {
            [DataMember] public List<string> underpaidGoals { get; set; }
            [DataMember] public List<string> almostFinishedGoals { get; set; }
            [DataMember] public List<string> finishedGoals { get; set; }
        }

        [DataContract]
        public class NestJsonResponseHistory : NestJsonResponseBase
        {
            [DataMember] public int pageCount { get; set; }
            [DataMember] public int count { get; set; }
            [DataMember] public double creditSum { get; set; }
            [DataMember] public double debitSum { get; set; }
            [DataMember] public List<NestJsonResponseHistoryItem> list { get; set; }
        }

        [DataContract]
        public class NestJsonResponseHistoryItem
        {
            [DataMember] public long operationNumber { get; set; }
            [DataMember] public string description { get; set; }
            [DataMember] public string transactionDate { get; set; }
            [DataMember] public string operationDate { get; set; }
            [DataMember] public string operationSide { get; set; }
            [DataMember] public double amount { get; set; }
            [DataMember] public string currencyCode { get; set; }
            [DataMember] public double balanceAfterOperation { get; set; }
            [DataMember] public string operationType { get; set; }
            [DataMember] public bool isRepayable { get; set; }
            [DataMember] public bool isRenewable { get; set; }
            [DataMember] public string orderId { get; set; }
            [DataMember] public bool isTppTransaction { get; set; }
            [DataMember] public string cardMcc { get; set; }

            public NestJsonOperationType? OperationTypeValue
            {
                get { return operationType.GetEnumByJsonValue<NestJsonOperationType>(); }
                set { operationType = value.GetEnumJsonValue<NestJsonOperationType>(); }
            }
            public NestJsonCreditDebit? CreditDebitValue
            {
                get { return operationSide.GetEnumByJsonValue<NestJsonCreditDebit>(); }
                set { operationSide = value.GetEnumJsonValue<NestJsonCreditDebit>(); }
            }
            public DateTime TransactionDateValue
            {
                get { return DateTime.Parse(transactionDate); }
                set { transactionDate = value.Display("dd.MM.yyyy"); }
            }
        }

        [DataContract]
        public class NestJsonResponseHistoryDetails : NestJsonResponseBase
        {
            [DataMember] public NestJsonResponseHistoryDetailsCreditor creditor { get; set; }
            [DataMember] public NestJsonResponseHistoryDetailsDebtor debtor { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string description { get; set; }
            //TODO date
            [DataMember] public string transactionDate { get; set; }
            //TODO date
            [DataMember] public string operationDate { get; set; }
            //TODO enum
            [DataMember] public string operationSide { get; set; }
            [DataMember] public double amount { get; set; }
            [DataMember] public string currencyCode { get; set; }
            [DataMember] public double operationAmount { get; set; }
            [DataMember] public string operationCurrencyCode { get; set; }
            [DataMember] public double exchangeRate { get; set; }
            [DataMember] public string spData { get; set; }
            [DataMember] public NestJsonResponseHistoryDetailsTaxData taxData { get; set; }
            [DataMember] public string zusData { get; set; }
            [DataMember] public bool isOneSide { get; set; }
            //TODO enum
            [DataMember] public string operationType { get; set; }
            //TODO enum
            [DataMember] public string transferKind { get; set; }
            //TODO enum
            [DataMember] public string transferType { get; set; }
            [DataMember] public bool isRepayable { get; set; }
            [DataMember] public bool isRenewable { get; set; }
            [DataMember] public string orderId { get; set; }
            [DataMember] public bool isTppTransaction { get; set; }
        }

        [DataContract]
        public class NestJsonResponseHistoryDetailsCreditorDebtor
        {
            [DataMember] public string accountNumber { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string address { get; set; }
        }

        [DataContract]
        public class NestJsonResponseHistoryDetailsCreditor : NestJsonResponseHistoryDetailsCreditorDebtor
        {
        }

        [DataContract]
        public class NestJsonResponseHistoryDetailsDebtor : NestJsonResponseHistoryDetailsCreditorDebtor
        {
            [DataMember] public string bankName { get; set; }
        }

        [DataContract]
        public class NestJsonResponseHistoryDetailsTaxData
        {
            [DataMember] public string formCode { get; set; }
            [DataMember] public string periodNumber { get; set; }
            [DataMember] public string periodUnit { get; set; }
            [DataMember] public string periodYear { get; set; }
            //TODO based on NestJsonRequestPrepareSignTaxTransfer, not sure if ok
            //[DataMember] public long? officeId { get; set; }
            [DataMember] public string identifier { get; set; }
            [DataMember] public string identifierType { get; set; }
            [DataMember] public string paymentIdentifier { get; set; }
        }

        [DataContract]
        public class NestJsonResponseTransferDate : NestJsonResponseBase
        {
            [DataMember] public DateTime date { get; set; }
            [DataMember] public bool afterEOD { get; set; }
        }

        [DataContract]
        public class NestJsonResponseOperator : NestJsonResponseBase
        {
            [DataMember] public List<NestJsonResponseOperatorOperator> operators { get; set; }
            [DataMember] public List<NestJsonResponseOperatorClause> clauses { get; set; }
        }

        [DataContract]
        public class NestJsonResponseOperatorOperator
        {
            [DataMember] public string name { get; set; }
            [DataMember] public string displayName { get; set; }
            [DataMember] public string valueType { get; set; }
            [DataMember] public double? minValue { get; set; }
            [DataMember] public double? maxValue { get; set; }
            [DataMember] public List<double> values { get; set; }

            public NestJsonOperatorValueType? ValueTypeValue
            {
                get { return valueType.GetEnumByJsonValue<NestJsonOperatorValueType>(); }
                set { valueType = value.GetEnumJsonValue<NestJsonOperatorValueType>(); }
            }
        }

        [DataContract]
        public class NestJsonResponseOperatorClause
        {
            [DataMember] public string label { get; set; }
            [DataMember] public List<NestJsonResponseOperatorClauseLink> links { get; set; }
        }

        [DataContract]
        public class NestJsonResponseOperatorClauseLink
        {
            [DataMember] public string id { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string link { get; set; }
        }

        [DataContract]
        public class NestJsonResponseEpayments : NestJsonResponseBase
        {
            [DataMember] public string receiverAccountNumber { get; set; }
            [DataMember] public string receiverName { get; set; }
            [DataMember] public string receiverAddress { get; set; }
            [DataMember] public double amount { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public DateTime date { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public string email { get; set; }
            [DataMember] public string providerCommission { get; set; }
            [DataMember] public string bankCommission { get; set; }
            [DataMember] public string backPage { get; set; }
            [DataMember] public string backPageRejected { get; set; }
            [DataMember] public string backPageCancelled { get; set; }
            [DataMember] public string backPageData { get; set; }
            [DataMember] public string backPageRejectedData { get; set; }
            [DataMember] public int sessionTimeout { get; set; }
            [DataMember] public int successTimeout { get; set; }
        }

        [DataContract]
        public class NestJsonResponseTaxFormType
        {
            [DataMember] public string name { get; set; }
            [DataMember] public bool periodMandatory { get; set; }
            [DataMember] public bool vatIndicator { get; set; }
            [DataMember] public bool irp { get; set; }
        }

        [DataContract]
        public class NestJsonResponseTaxOffice
        {
            [DataMember] public long id { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string nrb { get; set; }
            [DataMember] public string bankName { get; set; }
            [DataMember] public string town { get; set; }
        }
    }
}
