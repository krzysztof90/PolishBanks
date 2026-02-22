using BankService.Bank_PL_VeloBank;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Tools;
using Tools.Enums;
using static BankService.Bank_PL_Nest.NestJsonResponse;

namespace BankService.Bank_PL_Nest
{
    public class NestJsonRequest
    {
        [DataContract]
        public abstract class NestJsonRequestBase
        {
        }

        [DataContract]
        public class NestJsonRequestPagination
        {
            [DataMember] public int pageNumber { get; set; }
            [DataMember] public int pageSize { get; set; }
        }

        [DataContract]
        public class NestJsonRequestLogin : NestJsonRequestBase
        {
            [DataMember] public string login { get; set; }

            public static NestJsonRequestLogin Create(string login)
            {
                return new NestJsonRequestLogin() { login = login };
            }
        }

        [DataContract]
        public class NestJsonRequestPassword : NestJsonRequestBase
        {
            [DataMember] public int avatarId { get; set; }
            [DataMember] public string login { get; set; }
            //TODO enum
            [DataMember] public string loginScopeType { get; set; }
        }

        [DataContract]
        public class NestJsonRequestFullPassword : NestJsonRequestPassword
        {
            [DataMember] public string password { get; set; }

            public static NestJsonRequestFullPassword Create(string loginScopeType, string login, string password, int avatarId)
            {
                return new NestJsonRequestFullPassword() { loginScopeType = loginScopeType, login = login, password = password, avatarId = avatarId };
            }
        }

        [DataContract]
        public class NestJsonRequestMaskedPassword : NestJsonRequestPassword
        {
            [DataMember] public Dictionary<int, string> maskedPassword { get; set; }

            public static NestJsonRequestMaskedPassword Create(string loginScopeType, string login, Dictionary<int, string> maskedPassword, int avatarId)
            {
                return new NestJsonRequestMaskedPassword() { loginScopeType = loginScopeType, login = login, maskedPassword = maskedPassword, avatarId = avatarId };
            }
        }

        [DataContract]
        public class NestJsonRequestPrepareSignLogin : NestJsonRequestBase
        {
            [DataMember] public bool disableDashboardSca { get; set; }

            public static NestJsonRequestPrepareSignLogin Create(bool disableDashboardSca)
            {
                return new NestJsonRequestPrepareSignLogin() { disableDashboardSca = disableDashboardSca };
            }
        }

        [DataContract]
        public class NestJsonRequestPrepareSignTransferBase : NestJsonRequestBase
        {
        }

        [DataContract]
        public class NestJsonRequestPrepareSignTransfer : NestJsonRequestPrepareSignTransferBase
        {
            [DataMember(Name = "$objectType")] public string objectType { get; set; }
            [DataMember] public long accountId { get; set; }
            [DataMember] public double amount { get; set; }
            [DataMember] public string cardId { get; set; }
            [DataMember] public string cntrAccountNo { get; set; }
            [DataMember] public string cntrFullName { get; set; }
            [DataMember] public string cntrShortName { get; set; }
            [DataMember] public bool cntrTrusted { get; set; }
            [DataMember] public string cntrAddress { get; set; }
            //TODO date
            [DataMember] public string realizationDate { get; set; }
            [DataMember] public string confirmationInfo { get; set; }
            [DataMember] public string contractorId { get; set; }
            [DataMember] public string currency { get; set; }
            //TODO enum
            [DataMember] public string standingType { get; set; }
            [DataMember] public string id { get; set; }
            [DataMember] public string standingOrderData { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public bool confirmation { get; set; }
            [DataMember] public bool shouldBlockFunds { get; set; }
            //TODO enum
            [DataMember] public string orderType { get; set; }
            [DataMember] public string relatedOrderId { get; set; }
            [DataMember] public bool saveContractor { get; set; }
            [DataMember] public bool sorbnet { get; set; }
            [DataMember] public string dataChangesLog { get; set; }
        }

        [DataContract]
        public class NestJsonRequestPrepareSignTaxTransfer : NestJsonRequestPrepareSignTransferBase
        {
            [DataMember(Name = "$objectType")] public string objectType { get; set; }
            [DataMember] public long accountId { get; set; }
            [DataMember] public double amount { get; set; }
            [DataMember] public string cardId { get; set; }
            [DataMember] public string cntrAccountNo { get; set; }
            [DataMember] public string cntrFullName { get; set; }
            [DataMember] public string cntrShortName { get; set; }
            [DataMember] public bool cntrTrusted { get; set; }
            [DataMember] public string cntrAddress { get; set; }
            //TODO date
            [DataMember] public string realizationDate { get; set; }
            [DataMember] public string confirmationInfo { get; set; }
            [DataMember] public string contractorId { get; set; }
            [DataMember] public string currency { get; set; }
            //TODO enum
            [DataMember] public string standingType { get; set; }
            [DataMember] public string id { get; set; }
            [DataMember] public string standingOrderData { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public bool confirmation { get; set; }
            [DataMember] public bool shouldBlockFunds { get; set; }
            //TODO enum
            [DataMember] public string relatedOrderId { get; set; }
            [DataMember] public bool saveContractor { get; set; }
            [DataMember] public string taxFormCode { get; set; }
            [DataMember] public string taxPeriodNo { get; set; }
            [DataMember] public string taxPeriodUnit { get; set; }
            [DataMember] public string taxPeriodYear { get; set; }
            [DataMember] public long? taxOfficeId { get; set; }
            [DataMember] public string taxIdentifier { get; set; }
            [DataMember] public string taxIdentifierType { get; set; }
            [DataMember] public string taxPaymentId { get; set; }
            [DataMember] public string dataChangesLog { get; set; }
        }

        [DataContract]
        public class NestJsonRequestPrepareSignPrepaid : NestJsonRequestPrepareSignTransferBase
        {
            [DataMember(Name = "$objectType")] public string objectType { get; set; }
            [DataMember] public long accountId { get; set; }
            [DataMember] public double amount { get; set; }
            [DataMember] public bool clauseAccepted { get; set; }
            [DataMember] public string cntrFullName { get; set; }
            [DataMember] public string cntrShortName { get; set; }
            [DataMember] public bool cntrTrusted { get; set; }
            [DataMember] public string confirmationInfo { get; set; }
            [DataMember] public string contractorId { get; set; }
            [DataMember] public string currency { get; set; }
            //TODO enum
            [DataMember] public string standingType { get; set; }
            [DataMember] public string id { get; set; }
            [DataMember] public string standingOrderData { get; set; }
            [DataMember] public bool confirmation { get; set; }
            [DataMember] public bool shouldBlockFunds { get; set; }
            //TODO enum
            [DataMember] public string orderType { get; set; }
            [DataMember] public bool saveContractor { get; set; }
            [DataMember] public string mobilePhone { get; set; }
            [DataMember] public string mobileOperator { get; set; }
        }

        //TODO common part with other
        [DataContract]
        public class NestJsonRequestPrepareSignPbl : NestJsonRequestPrepareSignTransferBase
        {
            [DataMember(Name = "$objectType")] public string objectType { get; set; }
            [DataMember] public long accountId { get; set; }
            [DataMember] public double amount { get; set; }
            [DataMember] public string cntrAccountNo { get; set; }
            [DataMember] public string cntrFullName { get; set; }
            [DataMember] public string confirmationInfo { get; set; }
            [DataMember] public string currency { get; set; }
            //TODO date
            [DataMember] public string realizationDate { get; set; }
            //TODO enum
            [DataMember] public string standingType { get; set; }
            [DataMember] public string standingOrderData { get; set; }
            [DataMember] public bool confirmation { get; set; }
            [DataMember] public bool shouldBlockFunds { get; set; }
            //TODO enum
            [DataMember] public string orderType { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string transferUID { get; set; }
        }

        [DataContract]
        public class NestJsonRequestTrustedDeviceSave : NestJsonRequestBase
        {
            [DataMember] public string appName { get; set; }
            [DataMember] public string appVersion { get; set; }
            [DataMember] public string browserLanguage { get; set; }
            [DataMember] public string browserName { get; set; }
            [DataMember] public bool browserOnline { get; set; }
            [DataMember] public bool cookieEnabled { get; set; }
            [DataMember] public string deviceName { get; set; }
            [DataMember] public int height { get; set; }
            [DataMember] public int width { get; set; }
            [DataMember] public string platform { get; set; }
            [DataMember] public string product { get; set; }

            public static NestJsonRequestTrustedDeviceSave Create(string appName, string appVersion, string browserLanguage, string browserName, bool browserOnline, bool cookieEnabled, string deviceName, int height, int width, string platform, string product)
            {
                return new NestJsonRequestTrustedDeviceSave() { appName = appName, appVersion = appVersion, browserLanguage = browserLanguage, browserName = browserName, browserOnline = browserOnline, cookieEnabled = cookieEnabled, deviceName = deviceName, height = height, width = width, platform = platform, product = product };
            }
        }

        [DataContract]
        public class NestJsonRequestHistory : NestJsonRequestBase
        {
            [DataMember] public NestJsonRequestPagination pagination { get; set; }
            [DataMember] public string operationType { get; set; }
            [DataMember] public string textSearch { get; set; }
            [DataMember] public string dateFrom { get; set; }
            [DataMember] public string dateTo { get; set; }
            [DataMember] public string amountFrom { get; set; }
            [DataMember] public string amountTo { get; set; }

            public DateTime? DateFromValue
            {
                get { return DateTime.Parse(dateFrom); }
                set { dateFrom = value?.Display("dd.MM.yyyy"); }
            }
            public DateTime? DateToValue
            {
                get { return DateTime.Parse(dateTo); }
                set { dateTo = value?.Display("dd.MM.yyyy"); }
            }
            public double? AmountFromValue
            {
                get { return amountFrom != null ? (double?)DoubleOperations.Parse(amountFrom) : null; }
                set { amountFrom = value != null ? ((double)(value)).Display(DecimalSeparator.Comma) : null; }
            }
            public double? AmountToValue
            {
                get { return amountTo != null ? (double?)DoubleOperations.Parse(amountTo) : null; }
                set { amountTo = value != null ? ((double)(value)).Display(DecimalSeparator.Comma) : null; }
            }
        }

        [DataContract]
        public class NestJsonRequestSign<T> : NestJsonRequestBase where T : NestJsonResponsePrepareSignObject
        {
            [DataMember] public string authorizationPassword { get; set; }
            [DataMember] public string credential { get; set; }
            [DataMember] public List<T> signableObjects { get; set; }
            [DataMember] public T signedObject { get; set; }

            public static NestJsonRequestSign<T> Create(string authorizationPassword, string credential, List<T> signableObjects, T signedObject, bool oneObject)
            {
                return new NestJsonRequestSign<T>() { authorizationPassword = authorizationPassword, credential = credential, signableObjects = signableObjects, signedObject = signedObject, oneObject = oneObject };
            }

            private bool oneObject;
            public bool ShouldSerializesignableObjects()
            {
                return !oneObject;
            }
            public bool ShouldSerializesignedObject()
            {
                return oneObject;
            }
        }
    }
}
