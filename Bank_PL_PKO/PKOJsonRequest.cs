using Newtonsoft.Json.Linq;
using ProtoBuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Principal;
using Tools;
using Tools.Enums;
using static BankService.Bank_PL_PKO.PKOJsonRequest;
using static BankService.Bank_PL_PKO.PKOJsonResponse;

namespace BankService.Bank_PL_PKO
{
    public class PKOJsonRequest
    {
        [DataContract]
        public abstract class PKOJsonRequestBaseBase
        {
            [DataMember] public int? version { get; set; }
            [DataMember] public int? seq { get; set; }
            [DataMember] public string location { get; set; }

            public PKOJsonRequestBaseBase()
            {
            }
        }

        [DataContract]
        public abstract class PKOJsonRequestBase<T> : PKOJsonRequestBaseBase where T : PKOJsonRequestDataBase
        {
            [DataMember] public T data { get; set; }
        }

        [DataContract]
        public abstract class PKOJsonRequestFlowBase<T> : PKOJsonRequestBase<T> where T : PKOJsonRequestDataBase
        {
            [DataMember] public string token { get; set; }
            [DataMember] public string flow_id { get; set; }
            //TODO enum or value from previous response (state_id) + seq
            [DataMember] public string state_id { get; set; }
            //TODO enum
            [DataMember] public string action { get; set; }
        }

        [DataContract]
        public abstract class PKOJsonRequestDataBase
        {
        }


        [DataContract]
        public class PKOJsonRequestAmount
        {
            [DataMember] public string amount { get; set; }
            [DataMember] public string currency { get; set; }

            public double Amount
            {
                get => DoubleOperations.Parse(amount, ThousandSeparator.None, DecimalSeparator.Dot);
                set => amount = value.Display(DecimalSeparator.Dot);
            }
        }


        [DataContract]
        public class PKOJsonRequestNudatasecurity
        {
            [DataMember] public string sid { get; set; }

            public static PKOJsonRequestNudatasecurity Create(string sid)
            {
                return new PKOJsonRequestNudatasecurity() { sid = sid };
            }
        }

        //TODO PKOJsonRequestFlowBase?
        [DataContract]
        public class PKOJsonRequestLogin : PKOJsonRequestBase<PKOJsonRequestLoginData>
        {
            //TODO enum
            [DataMember] public string action { get; set; }
            //TODO enum
            [DataMember] public string state_id { get; set; }

            public static PKOJsonRequestLogin Create(string action, string state_id, string login)
            {
                return new PKOJsonRequestLogin() { action = action, state_id = state_id, data = new PKOJsonRequestLoginData() { login = login } };
            }
        }

        [DataContract]
        public class PKOJsonRequestLoginData : PKOJsonRequestDataBase
        {
            [DataMember] public string login { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestPassword : PKOJsonRequestFlowBase<PKOJsonRequestPasswordData>
        {
            public static PKOJsonRequestPassword Create(string token, string flow_id, string action, string state_id, string password, string session_uuid)
            {
                return new PKOJsonRequestPassword() { token = token, flow_id = flow_id, state_id = state_id, action = action, data = new PKOJsonRequestPasswordData() { password = password, session_uuid = session_uuid } };
            }
        }

        [DataContract]
        public class PKOJsonRequestPasswordData : PKOJsonRequestDataBase
        {
            [DataMember] public string password { get; set; }
            [DataMember] public string session_uuid { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestAuthorizeKey : PKOJsonRequestFlowBase<PKOJsonRequestDeviceConfirmData>
        {
            public static PKOJsonRequestAuthorizeKey Create(string token, string flow_id, string action, string state_id, string authorization_data)
            {
                return new PKOJsonRequestAuthorizeKey() { token = token, flow_id = flow_id, state_id = state_id, action = action, data = new PKOJsonRequestDeviceConfirmData() { authorization_data = authorization_data } };
            }
        }

        [DataContract]
        public class PKOJsonRequestDeviceConfirmFields
        {
            [DataMember] public PKOJsonResponseField authorization_data { get; set; }
            [DataMember] public List<PKOJsonResponseError> errors { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestDeviceConfirmData : PKOJsonRequestDataBase
        {
            [DataMember] public string authorization_data { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestLoginInit : PKOJsonRequestBase<PKOJsonRequestLoginInitData>
        {
            public static PKOJsonRequestLoginInit Create()
            {
                return new PKOJsonRequestLoginInit() { data = new PKOJsonRequestLoginInitData() { after_login_actions = new PKOJsonRequestInitDataAfterLoginActions(), client_info = new PKOJsonRequestInitDataClientInfo(), context_info = new PKOJsonRequestInitDataContextInfo(), is_new_investment_account_view = new PKOJsonRequestInitDataIsNewInvestmentAccountView(), is_personal_banking = new PKOJsonRequestInitDataIsPersonalBanking(), oauth = new PKOJsonRequestInitDataOauth(), sso = new PKOJsonRequestInitDataSso() } };
            }
        }

        [DataContract]
        public class PKOJsonRequestLoginInitData : PKOJsonRequestDataBase
        {
            [DataMember] public PKOJsonRequestInitDataAfterLoginActions after_login_actions { get; set; }
            [DataMember] public PKOJsonRequestInitDataClientInfo client_info { get; set; }
            [DataMember] public PKOJsonRequestInitDataContextInfo context_info { get; set; }
            [DataMember] public PKOJsonRequestInitDataIsNewInvestmentAccountView is_new_investment_account_view { get; set; }
            [DataMember] public PKOJsonRequestInitDataIsPersonalBanking is_personal_banking { get; set; }
            [DataMember] public PKOJsonRequestInitDataOauth oauth { get; set; }
            [DataMember] public PKOJsonRequestInitDataSso sso { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestInitDataAfterLoginActions
        {
        }

        [DataContract]
        public class PKOJsonRequestInitDataClientInfo
        {
        }

        [DataContract]
        public class PKOJsonRequestInitDataContextInfo
        {
        }

        [DataContract]
        public class PKOJsonRequestInitDataIsNewInvestmentAccountView
        {
        }

        [DataContract]
        public class PKOJsonRequestInitDataIsPersonalBanking
        {
        }

        [DataContract]
        public class PKOJsonRequestInitDataIsSrvTransferNormal
        {
        }

        [DataContract]
        public class PKOJsonRequestInitDataIsSrvTransferNormalRepeat
        {
        }

        [DataContract]
        public class PKOJsonRequestInitDataOauth
        {
        }

        [DataContract]
        public class PKOJsonRequestInitDataSso
        {
        }

        [DataContract]
        public class PKOJsonRequestAddTrustedDevice : PKOJsonRequestBase<PKOJsonRequestAddTrustedDeviceData>
        {
            public static PKOJsonRequestAddTrustedDevice Create(string type, string os, string browser, string label)
            {
                return new PKOJsonRequestAddTrustedDevice() { data = new PKOJsonRequestAddTrustedDeviceData() { type = type, os = os, browser = browser, label = label } };
            }
        }

        [DataContract]
        public class PKOJsonRequestAddTrustedDeviceData : PKOJsonRequestDataBase
        {
            //TODO enum
            [DataMember] public string type { get; set; }
            [DataMember] public string os { get; set; }
            [DataMember] public string browser { get; set; }
            [DataMember] public string label { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestAuthVerify : PKOJsonRequestBase<PKOJsonRequestAuthVerifyData>
        {
            public static PKOJsonRequestAuthVerify Create(string access_limiter_id, string operation_type)
            {
                return new PKOJsonRequestAuthVerify() { data = new PKOJsonRequestAuthVerifyData() { access_limiter_id = access_limiter_id, operation_type = operation_type } };
            }
        }

        [DataContract]
        public class PKOJsonRequestAuthVerifyData : PKOJsonRequestDataBase
        {
            [DataMember] public string access_limiter_id { get; set; }
            [DataMember] public string operation_type { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestAuthVerifySubmit : PKOJsonRequestFlowBase<PKOJsonRequestAuthVerifySubmitData>
        {
            public static PKOJsonRequestAuthVerifySubmit Create(string token, string flow_id, string state_id, string action, string tan_code)
            {
                return new PKOJsonRequestAuthVerifySubmit() { token = token, flow_id = flow_id, state_id = state_id, action = action, data = new PKOJsonRequestAuthVerifySubmitData() { tan_code = tan_code } };
            }
        }

        [DataContract]
        public class PKOJsonRequestAuthVerifySubmitData : PKOJsonRequestDataBase
        {
            [DataMember] public string tan_code { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestSubmitMobile : PKOJsonRequestFlowBase<PKOJsonRequestAuthVerifySubmitMobileData>
        {
            public static PKOJsonRequestSubmitMobile Create(string token, string flow_id, string state_id, string action)
            {
                return new PKOJsonRequestSubmitMobile() { token = token, flow_id = flow_id, state_id = state_id, action = action, data = new PKOJsonRequestAuthVerifySubmitMobileData() { } };
            }
        }

        [DataContract]
        public class PKOJsonRequestAuthVerifySubmitMobileData : PKOJsonRequestDataBase
        {
        }

        [DataContract]
        public class PKOJsonRequestMobileStatus
        {
            [DataMember] public string lp_id { get; set; }
            [DataMember] public string lp_key { get; set; }
            //TODO enum
            [DataMember] public string lp_type { get; set; }
            //TODO enum
            [DataMember] public string lp_value { get; set; }

            public static PKOJsonRequestMobileStatus Create(string lp_id, string lp_key, string lp_type, string lp_value)
            {
                return new PKOJsonRequestMobileStatus() { lp_id = lp_id, lp_key = lp_key, lp_type = lp_type, lp_value = lp_value };
            }
        }

        [DataContract]
        public class PKOJsonRequestAddedSecurityAuth : PKOJsonRequestBase<PKOJsonRequestAddedSecurityAuthData>
        {
            public static PKOJsonRequestAddedSecurityAuth Create()
            {
                return new PKOJsonRequestAddedSecurityAuth() { };
            }
        }

        [DataContract]
        public class PKOJsonRequestAddedSecurityAuthData : PKOJsonRequestDataBase
        {
        }

        [DataContract]
        public class PKOJsonRequestAddedSecurity : PKOJsonRequestBase<PKOJsonRequestAddedSecurityData>
        {
            public static PKOJsonRequestAddedSecurity Create()
            {
                return new PKOJsonRequestAddedSecurity() { data = new PKOJsonRequestAddedSecurityData() { } };
            }
        }

        [DataContract]
        public class PKOJsonRequestAddedSecurityData : PKOJsonRequestDataBase
        {
        }

        [DataContract]
        public class PKOJsonRequestAddTrustedDeviceSubmit : PKOJsonRequestFlowBase<PKOJsonRequestAddTrustedDeviceSubmitData>
        {
            public static PKOJsonRequestAddTrustedDeviceSubmit Create(string token, string flow_id, string state_id, string action, string name)
            {
                return new PKOJsonRequestAddTrustedDeviceSubmit() { token = token, flow_id = flow_id, state_id = state_id, action = action, data = new PKOJsonRequestAddTrustedDeviceSubmitData() { name = name } };
            }
        }

        [DataContract]
        public class PKOJsonRequestAddTrustedDeviceSubmitData : PKOJsonRequestDataBase
        {
            [DataMember] public string name { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestAddTrustedDeviceConfirm : PKOJsonRequestFlowBase<PKOJsonRequestAddTrustedDeviceConfirmData>
        {
            public static PKOJsonRequestAddTrustedDeviceConfirm Create(string token, string flow_id, string state_id, string action, string code)
            {
                return new PKOJsonRequestAddTrustedDeviceConfirm() { token = token, flow_id = flow_id, state_id = state_id, action = action, data = new PKOJsonRequestAddTrustedDeviceConfirmData() { tan_code = code } };
            }
        }

        [DataContract]
        public class PKOJsonRequestAddTrustedDeviceConfirmData : PKOJsonRequestDataBase
        {
            [DataMember] public string tan_code { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestLogout : PKOJsonRequestBase<PKOJsonRequestLogoutData>
        {
            public static PKOJsonRequestLogout Create(string reason)
            {
                return new PKOJsonRequestLogout() { data = new PKOJsonRequestLogoutData() { reason = reason } };
            }
        }

        [DataContract]
        public class PKOJsonRequestLogoutData : PKOJsonRequestDataBase
        {
            //TODO enum
            [DataMember] public string reason { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestAccountsInit : PKOJsonRequestBase<PKOJsonRequestAccountsInitData>
        {
            public static PKOJsonRequestAccountsInit Create()
            {
                return new PKOJsonRequestAccountsInit() { data = new PKOJsonRequestAccountsInitData() { accounts = new Dictionary<string, PKOJsonRequestAccountsInitDataAccount>() } };
            }
        }

        [DataContract]
        public class PKOJsonRequestAccountsInitData : PKOJsonRequestDataBase
        {
            [DataMember] public Dictionary<string, PKOJsonRequestAccountsInitDataAccount> accounts { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestAccountsInitDataAccount
        {
        }

        [DataContract]
        public class PKOJsonRequestRefresh : PKOJsonRequestBase<PKOJsonRequestRefreshData>
        {
            public static PKOJsonRequestRefresh Create()
            {
                return new PKOJsonRequestRefresh() { data = new PKOJsonRequestRefreshData() };
            }
        }

        [DataContract]
        public class PKOJsonRequestRefreshData : PKOJsonRequestDataBase
        {
        }

        [DataContract]
        public class PKOJsonRequestBhxToken : PKOJsonRequestBase<PKOJsonRequestBhxTokenData>
        {
            public static PKOJsonRequestBhxToken Create()
            {
                return new PKOJsonRequestBhxToken() { };
            }
        }

        [DataContract]
        public class PKOJsonRequestBhxTokenData : PKOJsonRequestDataBase
        {
        }

        [DataContract]
        public class PKOJsonRequestBhx : PKOJsonRequestBase<PKOJsonRequestBhxData>
        {
            public static PKOJsonRequestBhx Create(string channel, string screen_size, string token, string element_id1, string lang1, DateTime time_stamp1, string sequence1, string element_id2, string lang2, DateTime time_stamp2, string sequence2)
            {
                return new PKOJsonRequestBhx() { data = new PKOJsonRequestBhxData() { channel = channel, screen_size = screen_size, token = token, events = new List<PKOJsonRequestBhxDataEvent>() { new PKOJsonRequestBhxDataEvent() { element_id = element_id1, lang = lang1, TimeStampValue = time_stamp1, sequence = sequence1 }, new PKOJsonRequestBhxDataEvent() { element_id = element_id2, lang = lang2, TimeStampValue = time_stamp2, sequence = sequence2 } } } };
            }
        }

        [DataContract]
        public class PKOJsonRequestBhxData : PKOJsonRequestDataBase
        {
            [DataMember] public string channel { get; set; }
            [DataMember] public List<PKOJsonRequestBhxDataEvent> events { get; set; }
            [DataMember] public string screen_size { get; set; }
            [DataMember] public string token { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestBhxDataEvent
        {
            [DataMember] public string element_id { get; set; }
            [DataMember] public string lang { get; set; }
            [DataMember] public string time_stamp { get; set; }
            [DataMember] public string sequence { get; set; }

            public DateTime? TimeStampValue
            {
                get => DateTime.Parse(time_stamp);
                set => time_stamp = value?.Display("yyyy-MM-dd H:mm:ss.fff") ?? null;
            }
        }

        [DataContract]
        public class PKOJsonRequestHistory : PKOJsonRequestBase<PKOJsonRequestHistoryData>
        {
            public static PKOJsonRequestHistory Create(string type, bool choose_filters, bool only_filters, bool should_initialize, string source_account, string search_type, string operation_type, DateTime? dateFrom, DateTime? dateTo, double? amountFrom, double? amountTo, string search_phrase)
            {
                return new PKOJsonRequestHistory() { data = new PKOJsonRequestHistoryData() { action = new PKOJsonRequestHistoryDataAction() { type = type }, choose_filters = choose_filters, only_filters = only_filters, should_initialize = should_initialize, filter_form = new PKOJsonRequestHistoryDataFilterForm() { source_account = source_account, search_type = search_type, operation_type = operation_type, AmountFromValue = amountFrom, AmountToValue = amountTo, DateFromValue = dateFrom, DateToValue = dateTo, search_phrase = search_phrase } } };
            }
        }

        [DataContract]
        public class PKOJsonRequestHistoryData : PKOJsonRequestDataBase
        {
            [DataMember] public PKOJsonRequestHistoryDataAction action { get; set; }
            [DataMember] public bool choose_filters { get; set; }
            [DataMember] public bool only_filters { get; set; }
            [DataMember] public bool should_initialize { get; set; }
            [DataMember] public PKOJsonRequestHistoryDataFilterForm filter_form { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestHistoryDataAction
        {
            //TODO enum
            [DataMember] public string type { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestHistoryDataFilterFormBase
        {
            [DataMember] public string source_account { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestHistoryDataFilterForm : PKOJsonRequestHistoryDataFilterFormBase
        {
            [DataMember] public string search_type { get; set; }
            //TODO enum
            [DataMember] public string operation_type { get; set; }
            [DataMember] public string amount_from { get; set; }
            [DataMember] public string amount_to { get; set; }
            [DataMember] public string date_from { get; set; }
            [DataMember] public string date_to { get; set; }
            [DataMember] public string search_phrase { get; set; }

            public double? AmountFromValue
            {
                get => amount_from != null ? (double?)DoubleOperations.Parse(amount_from) : null;
                set => amount_from = value != null ? ((double)(value)).Display(DecimalSeparator.Dot) : null;
            }
            public double? AmountToValue
            {
                get => amount_to != null ? (double?)DoubleOperations.Parse(amount_to) : null;
                set => amount_to = value != null ? ((double)(value)).Display(DecimalSeparator.Dot) : null;
            }
            public DateTime? DateFromValue
            {
                get => DateTime.Parse(date_from);
                set => date_from = (value ?? new DateTime(1900, 1, 1)).Display("yyyy-MM-dd");
            }
            public DateTime? DateToValue
            {
                get => DateTime.Parse(date_to);
                set => date_to = value?.Display("yyyy-MM-dd") ?? null;
            }
        }

        [DataContract]
        public class PKOJsonRequestHistoryNext : PKOJsonRequestBase<PKOJsonRequestHistoryNextData>
        {
            public static PKOJsonRequestHistoryNext Create(string type)
            {
                return new PKOJsonRequestHistoryNext() { data = new PKOJsonRequestHistoryNextData() { action = new PKOJsonRequestHistoryDataAction() { type = type } } };
            }
        }

        [DataContract]
        public class PKOJsonRequestHistoryNextData : PKOJsonRequestDataBase
        {
            [DataMember] public PKOJsonRequestHistoryDataAction action { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestHistoryWaiting : PKOJsonRequestBase<PKOJsonRequestHistoryWaitingData>
        {
            public static PKOJsonRequestHistoryWaiting Create(string type, string source_account)
            {
                return new PKOJsonRequestHistoryWaiting() { data = new PKOJsonRequestHistoryWaitingData() { action = new PKOJsonRequestHistoryDataAction() { type = type }, filter_form = new PKOJsonRequestHistoryDataFilterForm() { source_account = source_account } } };
            }
        }

        [DataContract]
        public class PKOJsonRequestHistoryWaitingData : PKOJsonRequestDataBase
        {
            [DataMember] public PKOJsonRequestHistoryDataAction action { get; set; }
            [DataMember] public PKOJsonRequestHistoryDataFilterFormBase filter_form { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestConfirmationInit : PKOJsonRequestBase<PKOJsonRequestConfirmationInitData>
        {
            public static PKOJsonRequestConfirmationInit Create(string source, string account, string id, string language)
            {
                return new PKOJsonRequestConfirmationInit() { data = new PKOJsonRequestConfirmationInitData() { object_id = new PKOJsonRequestConfirmationInitObjectId() { source = source, account = account, id = id }, data = new PKOJsonRequestConfirmationInitDataData() { language = language }, } };
            }
        }

        [DataContract]
        public class PKOJsonRequestConfirmationInitData : PKOJsonRequestDataBase
        {
            [DataMember] public PKOJsonRequestConfirmationInitObjectId object_id { get; set; }
            [DataMember] public PKOJsonRequestConfirmationInitDataData data { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestConfirmationInitDataData
        {
            [DataMember] public string language { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestConfirmationInitObjectId
        {
            [DataMember] public string source { get; set; }
            [DataMember] public string account { get; set; }
            [DataMember] public string id { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestConfirmation : PKOJsonRequestFlowBase<PKOJsonRequestConfirmationData>
        {
            public static PKOJsonRequestConfirmation Create(string token, string flow_id, string state_id, string action, string media_type)
            {
                return new PKOJsonRequestConfirmation() { token = token, flow_id = flow_id, state_id = state_id, action = action, data = new PKOJsonRequestConfirmationData() { media_type = media_type } };
            }
        }

        [DataContract]
        public class PKOJsonRequestConfirmationData : PKOJsonRequestDataBase
        {
            [DataMember] public string media_type { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestTransferInit : PKOJsonRequestBase<PKOJsonRequestTransferInitData>
        {
            public static PKOJsonRequestTransferInit Create(string account)
            {
                return new PKOJsonRequestTransferInit() { data = new PKOJsonRequestTransferInitData() { account = account }, };
            }
        }

        [DataContract]
        public class PKOJsonRequestTransferInitData : PKOJsonRequestDataBase
        {
            [DataMember] public string account { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestTransfer : PKOJsonRequestFlowBase<PKOJsonRequestTransferData>
        {
            public static PKOJsonRequestTransfer Create(string token, string flow_id, string state_id, string action, double amount, string currency, DateTime paymentDate, PKOPaymentType paymentType, string recipient_account, string recipient_name, string recipient_address, string title, string source_account)
            {
                return new PKOJsonRequestTransfer() { token = token, flow_id = flow_id, state_id = state_id, action = action, data = new PKOJsonRequestTransferData() { money = new PKOJsonRequestAmount() { Amount = amount, currency = currency }, PaymentDateValue = paymentDate, PaymentTypeValue = paymentType, recipient_account = recipient_account, recipient_name = recipient_name, recipient_address = recipient_address, title = title, source_account = source_account }, };
            }
        }

        [DataContract]
        public class PKOJsonRequestTransferData : PKOJsonRequestDataBase
        {
            [DataMember] public PKOJsonRequestAmount money { get; set; }
            [DataMember] public string payment_date { get; set; }
            [DataMember] public string payment_type { get; set; }
            [DataMember] public string recipient_account { get; set; }
            [DataMember] public string recipient_name { get; set; }
            [DataMember] public string recipient_address { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string source_account { get; set; }

            public DateTime? PaymentDateValue
            {
                get => DateTime.Parse(payment_date);
                set => payment_date = value?.Display("yyyy-MM-dd") ?? null;
            }
            public PKOPaymentType? PaymentTypeValue
            {
                get => payment_type.GetEnumByJsonValue<PKOPaymentType>();
                set => payment_type = value.GetEnumJsonValue<PKOPaymentType>();
            }
        }

        [DataContract]
        public class PKOJsonRequestTaxTransferInit : PKOJsonRequestBase<PKOJsonRequestTaxTransferInitData>
        {
            public static PKOJsonRequestTaxTransferInit Create()
            {
                return new PKOJsonRequestTaxTransferInit() { data = new PKOJsonRequestTaxTransferInitData() { }, };
            }
        }

        [DataContract]
        public class PKOJsonRequestTaxTransferInitData : PKOJsonRequestDataBase
        {
        }

        [DataContract]
        public class PKOJsonRequestTaxTransferType : PKOJsonRequestFlowBase<PKOJsonRequestTaxTransferTypeData>
        {
            public static PKOJsonRequestTaxTransferType Create(string token, string flow_id, string state_id, string action, string tax_type_group)
            {
                return new PKOJsonRequestTaxTransferType() { token = token, flow_id = flow_id, state_id = state_id, action = action, data = new PKOJsonRequestTaxTransferTypeData() { tax_type_group = tax_type_group } };
            }
        }

        [DataContract]
        public class PKOJsonRequestTaxTransferTypeData : PKOJsonRequestDataBase
        {
            //TODO enum
            [DataMember] public string tax_type_group { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestTaxTransferAccount : PKOJsonRequestFlowBase<PKOJsonRequestTaxTransferAccountData>
        {
            public static PKOJsonRequestTaxTransferAccount Create(string token, string flow_id, string state_id, string action, string symbol, string tax_type_group, string individual_tax_account, string recipient_account_type)
            {
                return new PKOJsonRequestTaxTransferAccount() { token = token, flow_id = flow_id, state_id = state_id, action = action, data = new PKOJsonRequestTaxTransferAccountData() { symbol = symbol, tax_type_group = tax_type_group, individual_tax_account = individual_tax_account, recipient_account_type = recipient_account_type } };
            }
        }

        [DataContract]
        public class PKOJsonRequestTaxTransferAccountData : PKOJsonRequestDataBase
        {
            [DataMember] public string symbol { get; set; }
            //TODO enum
            [DataMember] public string tax_type_group { get; set; }
            [DataMember] public string individual_tax_account { get; set; }
            //TODO enum
            [DataMember] public string recipient_account_type { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestTaxTransfer : PKOJsonRequestFlowBase<PKOJsonRequestTaxTransferData>
        {
            public static PKOJsonRequestTaxTransfer Create(string token, string flow_id, string state_id, string action, string symbol, string tax_type_group, double amount, PKOJsonRequestTaxTransferDataPayerIdentifier payerIdentifier, DateTime payment_date, string payment_type, string obligation_id, PKOJsonRequestTaxTransferDataPeriod period, string individual_tax_account, string recipient_account_type, string recipient_bank, string tax_office_account, string source_account)
            {
                return new PKOJsonRequestTaxTransfer() { token = token, flow_id = flow_id, state_id = state_id, action = action, data = new PKOJsonRequestTaxTransferData() { symbol = symbol, tax_type_group = tax_type_group, Amount = amount, payer_identifier = payerIdentifier, PaymentDateValue = payment_date, payment_type = payment_type, obligation_id = obligation_id, period = period, individual_tax_account = individual_tax_account, recipient_account_type = recipient_account_type, recipient_bank = recipient_bank, tax_office_account = tax_office_account, source_account = source_account } };
            }
        }

        [DataContract]
        public class PKOJsonRequestTaxTransferData : PKOJsonRequestDataBase
        {
            [DataMember] public string symbol { get; set; }
            [DataMember] public string amount { get; set; }
            [DataMember] public PKOJsonRequestTaxTransferDataPayerIdentifier payer_identifier { get; set; }
            [DataMember] public string source_account { get; set; }
            [DataMember] public string payment_date { get; set; }
            //TODO enum
            [DataMember] public string payment_type { get; set; }
            [DataMember] public string individual_tax_account { get; set; }
            //TODO enum
            [DataMember] public string recipient_account_type { get; set; }
            //TODO enum
            [DataMember] public string tax_type_group { get; set; }
            [DataMember] public PKOJsonRequestTaxTransferDataPeriod period { get; set; }
            [DataMember] public string obligation_id { get; set; }
            [DataMember] public string recipient_name { get; set; }
            [DataMember] public string recipient_bank { get; set; }
            [DataMember] public string tax_office_account { get; set; }

            public double Amount
            {
                get => DoubleOperations.Parse(amount, ThousandSeparator.None, DecimalSeparator.Dot);
                set => amount = value.Display(DecimalSeparator.Dot);
            }
            public DateTime? PaymentDateValue
            {
                get => DateTime.Parse(payment_date);
                set => payment_date = value?.Display("yyyy-MM-dd") ?? null;
            }
        }

        [DataContract]
        public class PKOJsonRequestTaxTransferDataPayerIdentifier
        {
            [DataMember] public string type { get; set; }
            [DataMember] public string id_card { get; set; }
            [DataMember] public string passport { get; set; }
            [DataMember] public string nip { get; set; }
            [DataMember] public string pesel { get; set; }
            [DataMember] public string regon { get; set; }
            [DataMember] public string other_document { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestTaxTransferDataPeriod
        {
            //TODO enum
            [DataMember] public string type { get; set; }
            [DataMember] public string number { get; set; }
            [DataMember] public string month { get; set; }
            [DataMember] public string year { get; set; }
        }

        [DataContract]
        public class PKOJsonRequestTaxOfficeSearch : PKOJsonRequestBase<PKOJsonRequestTaxOfficeSearchData>
        {
            [DataMember] public string symbol { get; set; }
            [DataMember] public string city { get; set; }

            public static PKOJsonRequestTaxOfficeSearch Create(string symbol, string city)
            {
                return new PKOJsonRequestTaxOfficeSearch() { symbol = symbol, city = city };
            }
        }

        [DataContract]
        public class PKOJsonRequestTaxOfficeSearchData : PKOJsonRequestDataBase
        {
        }

        [DataContract]
        public class PKOJsonRequestPrepaidInit : PKOJsonRequestBase<PKOJsonRequestPrepaidInitData>
        {
            public static PKOJsonRequestPrepaidInit Create()
            {
                return new PKOJsonRequestPrepaidInit() { };
            }
        }

        [DataContract]
        public class PKOJsonRequestPrepaidInitData : PKOJsonRequestDataBase
        {
        }

        [DataContract]
        public class PKOJsonRequestPrepaid : PKOJsonRequestFlowBase<PKOJsonRequestPrepaidData>
        {
            public static PKOJsonRequestPrepaid Create(string token, string flow_id, string state_id, string action, string mobile_phone, string operatorValue, double amount, string currency, string source_account, bool invoice, bool ado_agreement, bool confirm_polish_residence, bool processing_regulations, bool recharge_regulations, bool waive_right_to_renounce)
            {
                return new PKOJsonRequestPrepaid() { token = token, flow_id = flow_id, state_id = state_id, action = action, data = new PKOJsonRequestPrepaidData() { mobile_phone = mobile_phone, operatorValue = operatorValue, money = new PKOJsonRequestAmount() { Amount = amount, currency = currency }, source_account = source_account, invoice = invoice, ado_agreement = ado_agreement, confirm_polish_residence = confirm_polish_residence, processing_regulations = processing_regulations, recharge_regulations = recharge_regulations, waive_right_to_renounce = waive_right_to_renounce } };
            }
        }

        [DataContract]
        public class PKOJsonRequestPrepaidData : PKOJsonRequestDataBase
        {
            [DataMember] public string mobile_phone { get; set; }
            [DataMember(Name = "operator")] public string operatorValue { get; set; }
            [DataMember] public PKOJsonRequestAmount money { get; set; }
            [DataMember] public string source_account { get; set; }
            [DataMember] public bool invoice { get; set; }
            [DataMember] public bool ado_agreement { get; set; }
            [DataMember] public bool confirm_polish_residence { get; set; }
            [DataMember] public bool processing_regulations { get; set; }
            [DataMember] public bool recharge_regulations { get; set; }
            [DataMember] public bool waive_right_to_renounce { get; set; }
        }
    }
}
