using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Tools;

namespace BankService.Bank_PL_Pocztowy
{
    public class PocztowyJsonRequest
    {
        [DataContract]
        public abstract class PocztowyJsonRequestBase
        {
        }

        [DataContract]
        public abstract class PocztowyJsonRequestTranferBase : PocztowyJsonRequestBase
        {
            [DataMember] public string fromProductId { get; set; }
            [DataMember] public string type { get; set; }

            public PocztowyJsonOutgoingTransferType? TypeValue
            {
                get => type.GetEnumByJsonValue<PocztowyJsonOutgoingTransferType>();
                set => type = value.GetEnumJsonValue<PocztowyJsonOutgoingTransferType>();
            }

            public bool ShouldSerializefromProductId()
            {
                return !String.IsNullOrEmpty(fromProductId);
            }
        }

        [DataContract]
        public class PocztowyJsonRequestAmount
        {
            [DataMember] public double amount { get; set; }
        }

        [DataContract]
        public class PocztowyJsonRequestAmountCurrency : PocztowyJsonRequestAmount
        {
            [DataMember] public string currencyCode { get; set; }
        }

        [DataContract]
        public class PocztowyJsonRequestLogin : PocztowyJsonRequestBase
        {
            [DataMember] public string userId { get; set; }

            public static PocztowyJsonRequestLogin Create(string userId)
            {
                return new PocztowyJsonRequestLogin() { userId = userId };
            }
        }

        [DataContract]
        public class PocztowyJsonRequestImage : PocztowyJsonRequestBase
        {
            //TODO enum
            [DataMember] public string size { get; set; }

            public static PocztowyJsonRequestImage Create(string size)
            {
                return new PocztowyJsonRequestImage() { size = size };
            }
        }

        [DataContract]
        public class PocztowyJsonRequestLoginPassword : PocztowyJsonRequestBase
        {
            [DataMember] public string encodedPasswd { get; set; }
            [DataMember] public string sessionId { get; set; }
            //TODO enum
            [DataMember] public string type { get; set; }

            public static PocztowyJsonRequestLoginPassword Create(string encodedPasswd, string sessionId, string type)
            {
                return new PocztowyJsonRequestLoginPassword() { encodedPasswd = encodedPasswd, sessionId = sessionId, type = type };
            }
        }

        [DataContract]
        public class PocztowyJsonRequestSession : PocztowyJsonRequestBase
        {
            public static PocztowyJsonRequestSession Create()
            {
                return new PocztowyJsonRequestSession() { };
            }
        }

        [DataContract]
        public class PocztowyJsonRequestAccountTransactions : PocztowyJsonRequestBase
        {
            [DataMember] public int numberOfNotifications { get; set; }
            [DataMember] public int numberOfTransactions { get; set; }

            public static PocztowyJsonRequestAccountTransactions Create(int numberOfNotifications, int numberOfTransactions)
            {
                return new PocztowyJsonRequestAccountTransactions() { numberOfNotifications = numberOfNotifications, numberOfTransactions = numberOfTransactions };
            }
        }

        [DataContract]
        public class PocztowyJsonRequestLogout : PocztowyJsonRequestBase
        {
            public static PocztowyJsonRequestLogout Create()
            {
                return new PocztowyJsonRequestLogout() { };
            }
        }

        [DataContract]
        public class PocztowyJsonRequestHistory : PocztowyJsonRequestBase
        {
            [DataMember] public string contains { get; set; }
            [DataMember] public string fromDate { get; set; }
            [DataMember] public string toDate { get; set; }
            [DataMember] public PocztowyJsonRequestAmount minAmount { get; set; }
            [DataMember] public PocztowyJsonRequestAmount maxAmount { get; set; }
            [DataMember] public List<string> types { get; set; }
            [DataMember] public int maxRows { get; set; }
            [DataMember] public List<string> productIds { get; set; }
            [DataMember] public string cursor { get; set; }

            public DateTime? FromDateValue
            {
                get => DateTime.Parse(fromDate);
                set => fromDate = value?.Display("yyyy-MM-dd") ?? String.Empty;
            }

            public DateTime? ToDateValue
            {
                get => DateTime.Parse(toDate);
                set => toDate = value?.Display("yyyy-MM-dd") ?? String.Empty;
            }
        }

        [DataContract]
        public class PocztowyJsonRequestBank : PocztowyJsonRequestBase
        {
            [DataMember] public string nrb { get; set; }

            public static PocztowyJsonRequestBank Create(string nrb)
            {
                return new PocztowyJsonRequestBank() { nrb = nrb };
            }
        }

        [DataContract]
        public class PocztowyJsonRequestTransferParams : PocztowyJsonRequestBase
        {
            [DataMember(Name = "type")] public string typeValue { get; set; }

            public static PocztowyJsonRequestTransferParams Create(string typeValue)
            {
                return new PocztowyJsonRequestTransferParams() { typeValue = typeValue };
            }
        }

        [DataContract]
        public class PocztowyJsonRequestTransferPrepare : PocztowyJsonRequestTranferBase
        {
            [DataMember] public PocztowyJsonRequestAmountCurrency amount { get; set; }
            [DataMember] public string date { get; set; }
            [DataMember] public string recipientName { get; set; }
            [DataMember] public string recipientAddress { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string toNrb { get; set; }

            public DateTime? DateValue
            {
                get => DateTime.Parse(date);
                set => date = value?.Display("yyyy-MM-dd") ?? String.Empty;
            }

            public bool ShouldSerializerecipientAddress()
            {
                return !String.IsNullOrEmpty(recipientAddress);
            }
        }

        [DataContract]
        public class PocztowyJsonRequestTransferPrepaidPrepare : PocztowyJsonRequestTranferBase
        {
            [DataMember] public PocztowyJsonRequestAmountCurrency amount { get; set; }
            [DataMember] public string operatorCode { get; set; }
            [DataMember] public string phoneNumber { get; set; }
        }

        [DataContract]
        public class PocztowyJsonRequestTransferPush<T> : PocztowyJsonRequestBase
            where T : PocztowyJsonRequestTranferBase
        {
            [DataMember] public T order { get; set; }
            [DataMember] public string orderId { get; set; }
            [DataMember] public string password { get; set; }
            [DataMember] public string authorizationUsed { get; set; }

            public PocztowyJsonConfirmType? AuthorizationUsedValue
            {
                get => authorizationUsed.GetEnumByJsonValue<PocztowyJsonConfirmType>();
                set => authorizationUsed = value.GetEnumJsonValue<PocztowyJsonConfirmType>();
            }
        }

        [DataContract]
        public class PocztowyJsonRequestOperator : PocztowyJsonRequestBase
        {
            [DataMember] public string phoneNumber { get; set; }

            public static PocztowyJsonRequestOperator Create(string phoneNumber)
            {
                return new PocztowyJsonRequestOperator() { phoneNumber = phoneNumber };
            }
        }

        [DataContract]
        public class PocztowyJsonRequestOperators : PocztowyJsonRequestBase
        {
            public static PocztowyJsonRequestOperators Create()
            {
                return new PocztowyJsonRequestOperators() { };
            }
        }

        [DataContract]
        public class PocztowyJsonRequestDetailsFile : PocztowyJsonRequestBase
        {
            [DataMember] public string operationId { get; set; }

            public static PocztowyJsonRequestDetailsFile Create(string operationId)
            {
                return new PocztowyJsonRequestDetailsFile() { operationId = operationId };
            }
        }

        [DataContract]
        public class PocztowyJsonRequestPayByLink : PocztowyJsonRequestBase
        {
            [DataMember] public string dataHash { get; set; }

            public static PocztowyJsonRequestPayByLink Create(string dataHash)
            {
                return new PocztowyJsonRequestPayByLink() { dataHash = dataHash };
            }
        }

        [DataContract]
        public class PocztowyJsonRequestFastTransferPrepare : PocztowyJsonRequestTranferBase
        {
            [DataMember] public string dataHash { get; set; }
        }

        [DataContract]
        public class PocztowyJsonRequestTaxTransferPrepare : PocztowyJsonRequestTranferBase
        {
            [DataMember] public PocztowyJsonRequestAmountCurrency amount { get; set; }
            [DataMember] public string date { get; set; }
            [DataMember] public string taxpayerIdentity { get; set; }
            [DataMember] public string taxpayerIdentityTypeCode { get; set; }
            [DataMember] public string taxpayerName { get; set; }
            [DataMember] public string period { get; set; }
            [DataMember] public bool sendEmailConfirmation { get; set; }
            [DataMember] public string formUSCode { get; set; }
            [DataMember] public string cityCode { get; set; }
            [DataMember] public string toNrb { get; set; }
            [DataMember] public string day { get; set; }
            [DataMember] public string halfyear { get; set; }
            [DataMember] public int? year { get; set; }
            [DataMember] public string month { get; set; }
            [DataMember] public string decade { get; set; }
            [DataMember] public string quarter { get; set; }

            public DateTime? DateValue
            {
                get => DateTime.Parse(date);
                set => date = value?.Display("yyyy-MM-dd") ?? String.Empty;
            }

            public bool ShouldSerializecityCode()
            {
                return !String.IsNullOrEmpty(cityCode);
            }
        }

        [DataContract]
        public class PocztowyJsonRequestAttendance : PocztowyJsonRequestTranferBase
        {
        }
    }
}
