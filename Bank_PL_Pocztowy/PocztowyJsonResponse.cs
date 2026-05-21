using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Tools;

namespace BankService.Bank_PL_Pocztowy
{
    public class PocztowyJsonResponse
    {
        [DataContract]
        public class PocztowyJsonResponseBase
        {
            //TODO enum
            [DataMember] public string errorCode { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseTransferBase
        {
            [DataMember] public string orderId { get; set; }
            [DataMember] public string authorizationType { get; set; }
            [DataMember] public int daySmsNumber { get; set; }

            public PocztowyJsonConfirmType? AuthorizationTypeValue
            {
                get => authorizationType.GetEnumByJsonValue<PocztowyJsonConfirmType>();
                set => authorizationType = value.GetEnumJsonValue<PocztowyJsonConfirmType>();
            }
        }

        [DataContract]
        public class PocztowyJsonResponseViolation
        {
            [DataMember] public string property { get; set; }
            //TODO enum
            [DataMember] public string message { get; set; }
            [DataMember] public List<object> invalidValue { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseAmount
        {
            [DataMember] public string currencyCode { get; set; }
            [DataMember] public double amount { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseLogin : PocztowyJsonResponseBase
        {
            [DataMember] public PocztowyJsonResponseLoginData data { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseLoginData
        {
            [DataMember] public string sessionId { get; set; }
            [DataMember] public int authorizeTimeout { get; set; }
            [DataMember] public int sessionTimeout { get; set; }
            [DataMember] public bool isMaskedLogin { get; set; }
            [DataMember] public List<int> mask { get; set; }
            [DataMember] public bool isP24 { get; set; }
            [DataMember] public bool isFirstLogIn { get; set; }
            [DataMember] public bool isPasswordFromSms { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseImage : PocztowyJsonResponseBase
        {
            [DataMember] public string securityImageUrl { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseLoginPassword : PocztowyJsonResponseBase
        {
            [DataMember] public PocztowyJsonResponseLoginPasswordData data { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseLoginPasswordData
        {
            [DataMember] public PocztowyJsonResponseLoginPasswordDataResult result { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseLoginPasswordDataResult
        {
            [DataMember] public bool isBlockadeWarn { get; set; }
            [DataMember] public string sessionId { get; set; }
            [DataMember] public bool isPasswdChangeRequired { get; set; }
            [DataMember] public bool isTermsOfUseAccepted { get; set; }
            [DataMember] public DateTime lastFailedLoginDate { get; set; }
            [DataMember] public DateTime lastSuccessLoginDate { get; set; }
            [DataMember] public string lastFailedLoginIp { get; set; }
            [DataMember] public string lastSuccessLoginIp { get; set; }
            [DataMember] public string userFirstName { get; set; }
            [DataMember] public string userLastName { get; set; }
            //TODO enum
            [DataMember] public string factor { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseLogout : PocztowyJsonResponseBase
        {
        }

        [DataContract]
        public class PocztowyJsonResponseSession : PocztowyJsonResponseBase
        {
            [DataMember] public PocztowyJsonResponseSessionData data { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseSessionData
        {
            [DataMember] public int sessionTimeout { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseAccountTransactions : PocztowyJsonResponseBase
        {
            [DataMember] public PocztowyJsonResponseAccountTransactionsData data { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseAccountTransactionsData
        {
            [DataMember] public List<PocztowyJsonResponseAccountTransactionsDataAccount> accounts { get; set; }
            //TODO enum
            [DataMember] public string loyaltyProgramVisibility { get; set; }
            [DataMember] public List<string> shortcuts { get; set; }
            [DataMember] public List<PocztowyJsonResponseAccountTransactionsDataTransaction> firstAccountTransactions { get; set; }
            [DataMember] public List<PocztowyJsonResponseAccountTransactionsDataNotification> notifications { get; set; }
            [DataMember] public PocztowyJsonResponseAccountTransactionsDataEmptyStatesMarketingOffer emptyStatesMarketingOffer { get; set; }
            [DataMember] public List<string> errorWidgets { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseAccountTransactionsDataAccount
        {
            [DataMember] public string accountId { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string nrb { get; set; }
            [DataMember] public PocztowyJsonResponseAmount balance { get; set; }
            //TODO enum
            [DataMember] public string accountType { get; set; }
            [DataMember] public string userRole { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseAccountTransactionsDataTransaction
        {
            [DataMember] public string operationId { get; set; }
            //TODO enum
            [DataMember] public string side { get; set; }
            //TODO enum
            [DataMember] public string type { get; set; }
            [DataMember] public string bookingDate { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string text { get; set; }
            [DataMember] public PocztowyJsonResponseAmount transactionValue { get; set; }
            [DataMember] public string iconUrl { get; set; }

            public DateTime? BookingDateValue
            {
                get => DateTime.Parse(bookingDate);
                set => bookingDate = value?.Display("yyyy-MM-dd") ?? String.Empty;
            }
        }

        [DataContract]
        public class PocztowyJsonResponseAccountTransactionsDataNotification
        {
            [DataMember] public PocztowyJsonResponseAccountTransactionsDataNotificationMessage message { get; set; }
            [DataMember] public PocztowyJsonResponseAccountTransactionsDataNotificationCalendarEvent calendarEvent { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseAccountTransactionsDataNotificationMessage
        {
            [DataMember] public string messageId { get; set; }
            [DataMember] public string messageDate { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string content { get; set; }
            [DataMember] public bool read { get; set; }
            [DataMember] public string type { get; set; }
            [DataMember] public bool overdue { get; set; }

            public DateTime? MessageDateValue
            {
                get => DateTime.Parse(messageDate);
                set => messageDate = value?.Display("yyyy-MM-dd") ?? String.Empty;
            }
        }

        [DataContract]
        public class PocztowyJsonResponseAccountTransactionsDataNotificationCalendarEvent
        {
            [DataMember] public string calendarEventId { get; set; }
            [DataMember] public string eventDate { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string content { get; set; }
            [DataMember] public string eventType { get; set; }
            [DataMember] public bool overdue { get; set; }

            public DateTime? EventDateValue
            {
                get => DateTime.Parse(eventDate);
                set => eventDate = value?.Display("yyyy-MM-dd") ?? String.Empty;
            }
        }

        [DataContract]
        public class PocztowyJsonResponseAccountTransactionsDataEmptyStatesMarketingOffer
        {
        }

        [DataContract]
        public class PocztowyJsonResponseHistory : PocztowyJsonResponseBase
        {
            [DataMember] public PocztowyJsonResponseHistoryData data { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseHistoryData
        {
            [DataMember] public PocztowyJsonResponseHistoryDataCare care { get; set; }
            [DataMember] public string cursor { get; set; }
            [DataMember] public List<PocztowyJsonResponseHistoryDataResult> results { get; set; }
            [DataMember] public int cnt { get; set; }
            [DataMember] public List<PocztowyJsonResponseHistoryDataSummary> summaries { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseHistoryDataCare
        {
            //TODO enum
            [DataMember] public string care { get; set; }
            [DataMember] public string below { get; set; }

            public DateTime? BelowDateValue
            {
                get => DateTime.Parse(below);
                set => below = value?.Display("yyyy-MM-dd") ?? String.Empty;
            }
        }

        [DataContract]
        public class PocztowyJsonResponseHistoryDataResult
        {
            [DataMember] public string operationId { get; set; }
            //TODO enum
            [DataMember] public string type { get; set; }
            [DataMember] public string iconUrl { get; set; }
            [DataMember] public string bookingDate { get; set; }
            //TODO enum
            [DataMember] public string side { get; set; }
            [DataMember] public string transactionDate { get; set; }
            [DataMember] public PocztowyJsonResponseAmount transactionValue { get; set; }
            [DataMember] public PocztowyJsonResponseAmount balanceAfter { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string counterpartyName { get; set; }
            [DataMember] public string remitterNRB { get; set; }
            [DataMember] public string remitterName { get; set; }
            [DataMember] public string remitterAddress { get; set; }
            [DataMember] public string remitterBank { get; set; }
            [DataMember] public string beneficiaryNRB { get; set; }
            [DataMember] public string beneficiaryName { get; set; }
            [DataMember] public string beneficiaryAddress { get; set; }
            [DataMember] public string beneficiaryBank { get; set; }
            [DataMember(Name = "operator")] public string operatorValue { get; set; }
            [DataMember] public string phoneNumber { get; set; }
            [DataMember] public string userName { get; set; }
            [DataMember] public bool modifiable { get; set; }
            [DataMember] public bool deletable { get; set; }
            [DataMember] public string subtype { get; set; }

            public PocztowyJsonTransferType? TypeValue
            {
                get => type.GetEnumByJsonValue<PocztowyJsonTransferType>();
                set => type = value.GetEnumJsonValue<PocztowyJsonTransferType>();
            }
            public PocztowyJsonCreditDebit? CreditDebitValue
            {
                get => side.GetEnumByJsonValue<PocztowyJsonCreditDebit>();
                set => side = value.GetEnumJsonValue<PocztowyJsonCreditDebit>();
            }
            public DateTime? BookingDateValue
            {
                get => DateTime.Parse(bookingDate);
                set => bookingDate = value?.Display("yyyy-MM-dd") ?? String.Empty;
            }
            //TODO bez null
            public DateTime? TransactionDateValue
            {
                get => DateTime.Parse(transactionDate);
                set => transactionDate = value?.Display("yyyy-MM-dd") ?? String.Empty;
            }
        }

        [DataContract]
        public class PocztowyJsonResponseHistoryDataSummary
        {
            [DataMember] public PocztowyJsonResponseAmount benefit { get; set; }
            [DataMember] public PocztowyJsonResponseAmount remit { get; set; }
            [DataMember] public PocztowyJsonResponseAmount balance { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseTransferBank : PocztowyJsonResponseBase
        {
            [DataMember] public PocztowyJsonResponseTransferBankData data { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseTransferBankData
        {
            [DataMember] public string bankName { get; set; }
            [DataMember] public bool nbp { get; set; }
            [DataMember] public bool bp { get; set; }
            [DataMember] public bool zus { get; set; }
            [DataMember] public bool krus { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseTransferParams : PocztowyJsonResponseBase
        {
            [DataMember] public PocztowyJsonResponseTransferParamsData data { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseTransferParamsData
        {
            [DataMember] public List<PocztowyJsonResponseTransferParamsDataTaxFormSymbol> taxFormSymbols { get; set; }
            [DataMember] public List<PocztowyJsonResponseTransferParamsDataTaxFormOfficeAccounts> taxFormOfficeAccounts { get; set; }
            [DataMember] public int yearsMaxBackInPeriodTypeYear { get; set; }
            [DataMember] public List<string> messages { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseTransferParamsDataTaxFormSymbol
        {
            [DataMember] public string paymentMethod { get; set; }
            [DataMember] public string symbol { get; set; }
            [DataMember] public bool splitPaymentForm { get; set; }
            [DataMember] public bool irp { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseTransferParamsDataTaxFormOfficeAccounts
        {
            [DataMember] public string place { get; set; }
            [DataMember] public List<PocztowyJsonResponseTransferPrepareTaxFormOfficeAccountsTaxOfficeAccount> taxOfficeAccounts { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseTransferPrepareTaxFormOfficeAccountsTaxOfficeAccount
        {
            [DataMember] public string paymentMethod { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string nrb { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseTransferPrepare : PocztowyJsonResponseBase
        {
            [DataMember] public PocztowyJsonResponseTransferPrepareData data { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseTransferPrepareData : PocztowyJsonResponseTransferBase
        {
            //TODO to PocztowyJsonResponseDataBase
            [DataMember] public List<PocztowyJsonResponseViolation> violations { get; set; }
            [DataMember] public string estimatedRealizationDate { get; set; }
            [DataMember] public PocztowyJsonResponseAmount estimatedCharges { get; set; }
            [DataMember] public string createdDate { get; set; }
            [DataMember] public PocztowyJsonResponseTransferPrepareDataTransferDetails transferDetails { get; set; }
            [DataMember] public string chargeDate { get; set; }

            public DateTime? EstimatedRealizationDateValue
            {
                get => DateTime.Parse(estimatedRealizationDate);
                set => estimatedRealizationDate = value?.Display("yyyy-MM-dd") ?? String.Empty;
            }
            public DateTime? CreatedDateValue
            {
                get => DateTime.Parse(createdDate);
                set => createdDate = value?.Display("yyyy-MM-dd") ?? String.Empty;
            }
            public DateTime? ChargeDateValue
            {
                get => DateTime.Parse(chargeDate);
                set => chargeDate = value?.Display("yyyy-MM-dd") ?? String.Empty;
            }
        }

        [DataContract]
        public class PocztowyJsonResponseTransferPrepareDataTransferDetails
        {
            [DataMember] public PocztowyJsonResponseTransferPrepareDataTransferDetailsFrom from { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseTransferPrepareDataTransferDetailsFrom
        {
            [DataMember] public string name { get; set; }
            [DataMember] public string address { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseTransferPush : PocztowyJsonResponseBase
        {
            [DataMember] public PocztowyJsonResponseTransferPushData data { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseTransferPushData
        {
            //TODO enum
            [DataMember] public string resultStatus { get; set; }
            [DataMember] public string id { get; set; }
            [DataMember] public string orderId { get; set; }
            [DataMember] public PocztowyJsonResponseAmount balance { get; set; }
            [DataMember] public PocztowyJsonResponseAmount amount { get; set; }
            [DataMember] public bool saveCounterpartyEnable { get; set; }
            [DataMember] public string redirectUrl { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseOperator : PocztowyJsonResponseBase
        {
            [DataMember] public PocztowyJsonResponseOperatorData data { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseOperatorData
        {
            [DataMember] public string prepaidStatus { get; set; }
            [DataMember] public string operatorCode { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseOperators : PocztowyJsonResponseBase
        {
            [DataMember] public PocztowyJsonResponseOperatorsData data { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseOperatorsData
        {
            [DataMember] public List<PocztowyJsonResponseOperatorsDataOperator> operators { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponseOperatorsDataOperator
        {
            [DataMember] public int id { get; set; }
            [DataMember] public string operatorCode { get; set; }
            [DataMember] public string operatorName { get; set; }
            [DataMember] public double amountMax { get; set; }
            [DataMember] public double amountMin { get; set; }
            [DataMember] public List<double> permittedAmounts { get; set; }
            [DataMember] public string amountType { get; set; }

            public PocztowyJsonOperatorAmountType? AmountTypeValue
            {
                get => amountType.GetEnumByJsonValue<PocztowyJsonOperatorAmountType>();
                set => amountType = value.GetEnumJsonValue<PocztowyJsonOperatorAmountType>();
            }
        }

        [DataContract]
        public class PocztowyJsonResponsePayByLinkData : PocztowyJsonResponseBase
        {
            [DataMember] public PocztowyJsonResponsePayByLinkDataData data { get; set; }
        }

        [DataContract]
        public class PocztowyJsonResponsePayByLinkDataData
        {
            [DataMember] public string recipientName { get; set; }
            [DataMember] public string recipientAddress { get; set; }
            [DataMember] public string toNrb { get; set; }
            [DataMember] public PocztowyJsonResponseAmount amount { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string redirectUrl { get; set; }
        }
    }
}
