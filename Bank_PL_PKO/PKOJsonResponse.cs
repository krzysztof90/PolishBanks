using BankService.Bank_PL_GetinBank;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using Tools;
using Tools.Enums;
using static BankService.Bank_PL_PKO.PKOJsonRequest;
using static BankService.Bank_PL_PKO.PKOJsonResponse;

namespace BankService.Bank_PL_PKO
{
    public class PKOJsonResponse
    {
        [DataContract]
        public class PKOJsonResponseBaseBaseBase
        {
        }

        [DataContract]
        public class PKOJsonResponseBaseBase : PKOJsonResponseBaseBaseBase
        {
            //TODO enum
            [DataMember] public string state_id { get; set; }
            [DataMember] public int httpStatus { get; set; }
            [DataMember] public List<PKOJsonResponseError> errors { get; set; }

            protected virtual PKOJsonResponseFieldsBase FieldsBase => null;

            public virtual bool HasErrors => state_id == "ERROR" || (httpStatus != 200 && httpStatus != 0) || (FieldsBase?.Errors.Any() ?? false);
            public virtual IEnumerable<string> Errors
            {
                get
                {
                    List<PKOJsonResponseError> errorsLocal = new List<PKOJsonResponseError>();
                    if (errors != null)
                        errorsLocal.AddRange(errors);
                    if (FieldsBase != null)
                        errorsLocal.AddRange(FieldsBase.Errors);
                    return errorsLocal.Select(e => e.Message);
                }
            }
        }

        [DataContract]
        public class PKOJsonResponseBase<T> : PKOJsonResponseBaseBase where T : PKOJsonResponseResponseBase
        {
            [DataMember] public T response { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseFlowBase<T> : PKOJsonResponseBase<T> where T : PKOJsonResponseResponseBase
        {
            [DataMember] public string token { get; set; }
            [DataMember] public string flow_id { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseFlowAuthBase<T> : PKOJsonResponseFlowBase<T> where T : PKOJsonResponseResponseAuthBase
        {
        }

        [DataContract]
        public class PKOJsonResponseResponseBase
        {
        }

        [DataContract]
        public class PKOJsonResponseResponseAuthBase : PKOJsonResponseResponseBase
        {
            [DataMember] public PKOJsonResponseResponseAuth auth { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseError
        {
            [DataMember] public string hint { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string message { get; set; }
            [DataMember] public string description { get; set; }
            [DataMember] public string mobile_description { get; set; }

            public string Message => String.Join("; ", new List<string>() { title, message, description, hint }.Where(m => !String.IsNullOrEmpty(m)));
        }

        [DataContract]
        public class PKOJsonResponseAmount
        {
            [DataMember] public string amount { get; set; }
            [DataMember] public string currency { get; set; }

            public double Amount
            {
                get => DoubleOperations.Parse(amount, ThousandSeparator.None, DecimalSeparator.Dot);
                set => value.Display(DecimalSeparator.Dot);
            }
        }

        [DataContract]
        public class PKOJsonResponseCoordinates
        {
            [DataMember] public string latitude { get; set; }
            [DataMember] public string longitude { get; set; }
        }

        [DataContract]
        public class PKOJsonResponsePhoneNumber
        {
            [DataMember] public string country_code { get; set; }
            [DataMember] public string number { get; set; }
        }

        [DataContract]
        public abstract class PKOJsonResponseFieldsBase
        {
            [DataMember] public PKOJsonResponseError errors { get; set; }

            protected abstract List<PKOJsonResponseFieldBaseBaseBase> Fields { get; }

            public List<PKOJsonResponseError> Errors
            {
                get
                {
                    List<PKOJsonResponseFieldBaseBaseBase> fields = new List<PKOJsonResponseFieldBaseBaseBase>();
                    fields.AddRange(Fields);
                    fields.AddRange(Fields.SelectMany(d => d?.Fields ?? new List<PKOJsonResponseFieldBaseBaseBase>()));

                    List<PKOJsonResponseError> result = new List<PKOJsonResponseError>();
                    result.Add(errors);
                    result.AddRange(fields.Select(d => d?.errors));
                    return result.Where(e => e != null).ToList();
                }
            }
        }

        [DataContract]
        public class PKOJsonResponseFieldBaseBaseBase
        {
            [DataMember] public PKOJsonResponseError errors { get; set; }
            [JsonConverter(typeof(PKOJsonFieldValueConverter))]
            [DataMember] public object value { get; set; }

            public List<PKOJsonResponseFieldBaseBaseBase> Fields
            {
                get
                {
                    List<PKOJsonResponseFieldBaseBaseBase> result = new List<PKOJsonResponseFieldBaseBaseBase>();
                    if (value is Dictionary<string, PKOJsonResponseFieldBaseBaseBase> fields)
                    {
                        result.AddRange(fields.Select(f => f.Value));
                        result.AddRange(fields.SelectMany(f => f.Value?.Fields ?? new List<PKOJsonResponseFieldBaseBaseBase>()));
                    }
                    return result;
                }
            }
        }

        [DataContract]
        public class PKOJsonResponseFieldBaseBase<T> : PKOJsonResponseFieldBaseBaseBase where T : PKOJsonResponseFieldWidgetBaseBase
        {
            [DataMember] public T widget { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseFieldBase : PKOJsonResponseFieldBaseBase<PKOJsonResponseFieldWidgetBaseBase>
        {
        }

        [DataContract]
        public class PKOJsonResponseField : PKOJsonResponseFieldBaseBase<PKOJsonResponseFieldWidget>
        {
        }

        [DataContract]
        public class PKOJsonResponseFieldSchema : PKOJsonResponseFieldBaseBase<PKOJsonResponseFieldWidgetSchemaWidget>
        {
        }

        [DataContract]
        public class PKOJsonResponseFieldWidgetBaseBase
        {
            [DataMember] public bool required { get; set; }
            //TODO enum
            [DataMember] public string field_type { get; set; }
            [DataMember] public bool trim { get; set; }
            //TODO enum
            [DataMember] public string field_format { get; set; }
            [DataMember(Name = "default")] public string defaultValue { get; set; }
            [DataMember] public int min_len { get; set; }
            [DataMember] public int max_len { get; set; }
            [DataMember] public string valid_characters { get; set; }
            [DataMember] public string regex { get; set; }
            [DataMember] public string min_amount { get; set; }
            [DataMember] public string max_amount { get; set; }
            [DataMember] public bool master_node { get; set; }
            [DataMember] public PKOJsonResponseFieldWidgetSchema schema { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseFieldWidgetBase<T> : PKOJsonResponseFieldWidgetBaseBase where T : PKOJsonResponseFieldWidgetItemBase
        {
            [DataMember] public List<T> items { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseFieldWidgetItemBase
        {
            [DataMember] public string id { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseFieldWidgetBase : PKOJsonResponseFieldWidgetBaseBase
        {
        }

        [DataContract]
        public class PKOJsonResponseFieldWidget : PKOJsonResponseFieldWidgetBase<PKOJsonResponseFieldWidgetItem>
        {
        }

        [DataContract]
        public class PKOJsonResponseFieldWidgetSchemaWidget : PKOJsonResponseFieldWidgetBase<PKOJsonResponseFieldWidgetSchemaWidgetItem>
        {
        }

        [DataContract]
        public class PKOJsonResponseFieldWidgetItem : PKOJsonResponseFieldWidgetItemBase
        {
            [DataMember] public string text { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseFieldWidgetSchemaWidgetItem : PKOJsonResponseFieldWidgetItemBase
        {
            [DataMember] public PKOJsonResponseWidgetItemData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseWidgetItemData
        {
            [DataMember] public string text { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseFieldWidgetSchema
        {
            [DataMember] public bool required { get; set; }
            //TODO enum
            [DataMember] public string field_type { get; set; }

            //TODO dynamically
            [DataMember] public PKOJsonResponseFieldWidgetSchemaWidget amount { get; set; }
            [DataMember] public PKOJsonResponseFieldWidgetSchemaWidget currency { get; set; }

            [DataMember] public PKOJsonResponseFieldWidgetSchemaWidget invoice_business { get; set; }
            [DataMember] public PKOJsonResponseFieldWidgetSchemaWidget name { get; set; }
            [DataMember] public PKOJsonResponseFieldWidgetSchemaWidget address { get; set; }
            [DataMember] public PKOJsonResponseFieldWidgetSchemaWidget nip { get; set; }

            [DataMember] public PKOJsonResponseFieldWidgetSchemaWidget number { get; set; }
            [DataMember] public PKOJsonResponseFieldWidgetSchemaWidget type { get; set; }
            [DataMember] public PKOJsonResponseFieldWidgetSchemaWidget month { get; set; }
            [DataMember] public PKOJsonResponseFieldWidgetSchemaWidget year { get; set; }

            [DataMember] public PKOJsonResponseFieldWidgetSchemaWidget id_card { get; set; }
            [DataMember] public PKOJsonResponseFieldWidgetSchemaWidget passport { get; set; }
            //[DataMember] public PKOJsonResponseFieldWidgetSchemaWidget nip { get; set; }
            [DataMember] public PKOJsonResponseFieldWidgetSchemaWidget pesel { get; set; }
            [DataMember] public PKOJsonResponseFieldWidgetSchemaWidget regon { get; set; }
            [DataMember] public PKOJsonResponseFieldWidgetSchemaWidget other_document { get; set; }
            //[DataMember] public PKOJsonResponseFieldWidgetSchemaWidget type { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseDataRefZxc
        {
            [DataMember] public PKOJsonResponseDataRefZxcData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseDataRefZxcData
        {
            //TODO enum
            [DataMember] public string place_name { get; set; }
        }


        [DataContract]
        public class PKOJsonResponseLogin : PKOJsonResponseFlowBase<PKOJsonResponseLoginResponse>
        {
            protected override PKOJsonResponseFieldsBase FieldsBase => response.fields;
        }

        [DataContract]
        public class PKOJsonResponseLoginResponse : PKOJsonResponseResponseBase
        {
            [DataMember] public PKOJsonResponseLoginResponseFields fields { get; set; }
            [DataMember] public PKOJsonResponseLoginResponseData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseLoginResponseFields : PKOJsonResponseFieldsBase
        {
            [DataMember] public PKOJsonResponseFieldBase password { get; set; }
            [DataMember] public PKOJsonResponseFieldBase behavioral_tracking_id { get; set; }
            [DataMember] public PKOJsonResponseFieldBase placement { get; set; }
            [DataMember] public PKOJsonResponseFieldBase widget_data { get; set; }
            [DataMember] public PKOJsonResponseFieldBase placement_page_no { get; set; }
            [DataMember] public PKOJsonResponseFieldBase login { get; set; }
            [DataMember] public PKOJsonResponseFieldBase fingerprint { get; set; }

            protected override List<PKOJsonResponseFieldBaseBaseBase> Fields => new List<PKOJsonResponseFieldBaseBaseBase>() { password, behavioral_tracking_id, placement, widget_data, placement_page_no, login, fingerprint };
        }

        [DataContract]
        public class PKOJsonResponseLoginResponseData
        {
            [DataMember] public string tracking_pixel { get; set; }
            [DataMember] public PKOJsonResponseImage image { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseImage
        {
            [DataMember] public string src { get; set; }
        }

        [DataContract]
        public class PKOJsonResponsePassword : PKOJsonResponseFlowBase<PKOJsonResponsePasswordResponse>
        {
            [DataMember] public bool finished { get; set; }

            protected override PKOJsonResponseFieldsBase FieldsBase => response.fields;
        }

        [DataContract]
        public class PKOJsonResponsePasswordResponse : PKOJsonResponseResponseBase
        {
            [DataMember] public PKOJsonResponsePasswordResponseFields fields { get; set; }
            [DataMember] public PKOJsonResponsePasswordResponseData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponsePasswordResponseFields : PKOJsonResponseFieldsBase
        {
            //[DataMember] public PKOJsonResponseFieldBase { get; set; }

            protected override List<PKOJsonResponseFieldBaseBaseBase> Fields => new List<PKOJsonResponseFieldBaseBaseBase>() { };
        }

        [DataContract]
        public class PKOJsonResponsePasswordResponseData
        {
            //TODO enum
            [DataMember] public string login_type { get; set; }
            [DataMember] public string method { get; set; }
            [DataMember] public string number { get; set; }
            [DataMember] public int warning_time { get; set; }
            [DataMember] public int approval_period { get; set; }
            [DataMember] public string challenge_data { get; set; }
            [DataMember] public PKOJsonResponseImage image { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseAuthorizeKey : PKOJsonResponseFlowBase<PKOJsonResponseAuthorizeKeyResponse>
        {
            [DataMember] public bool finished { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseAuthorizeKeyResponse : PKOJsonResponseResponseBase
        {
            [DataMember] public string login_type { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseLanguageResponse : PKOJsonResponseResponseBase
        {
            [DataMember] public PKOJsonResponseLanguageResponseData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseLanguageResponseData
        {
            [DataMember] public string language { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseLoginInit : PKOJsonResponseBase<PKOJsonResponseLoginInitResponse>
        {
        }

        [DataContract]
        public class PKOJsonResponseLoginInitResponse : PKOJsonResponseResponseBase
        {
            [DataMember] public PKOJsonResponseLoginInitResponseData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseLoginInitResponseData
        {
            [DataMember] public bool is_personal_banking { get; set; }
            [DataMember] public bool is_new_investment_account_view { get; set; }
            [DataMember] public PKOJsonResponseLoginInitResponseDataOauth oauth { get; set; }
            [DataMember] public PKOJsonResponseLoginInitResponseDataClientInfo client_info { get; set; }
            [DataMember] public List<PKOJsonResponseLoginInitResponseDataAfterLoginAction> after_login_actions { get; set; }
            [DataMember] public PKOJsonResponseLoginInitResponseDataSso sso { get; set; }
            [DataMember] public PKOJsonResponseLoginInitResponseDataContextInfo context_info { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseLoginInitResponseDataOauth
        {
            [DataMember] public string oauth_action { get; set; }
            [DataMember] public string oauth_go_to { get; set; }
            [DataMember] public string oauth_oper_type { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseLoginInitResponseDataClientInfo
        {
            [DataMember] public bool has_expired_id { get; set; }
            [DataMember] public bool epg_hints { get; set; }
            [DataMember] public bool is_restricted_ppk { get; set; }
            [DataMember] public bool embedded_complaint_access { get; set; }
            [DataMember] public bool is_adult { get; set; }
            [DataMember] public bool is_dkk_client { get; set; }
            [DataMember] public bool is_restricted_expired_id { get; set; }
            [DataMember] public bool ppk_hints { get; set; }
            [DataMember] public bool client_need_verification { get; set; }
            [DataMember] public string behavioral_tracking_id { get; set; }
            [DataMember] public string citizenship { get; set; }
            [DataMember] public bool is_restricted_epg { get; set; }
            [DataMember] public string full_name { get; set; }
            [DataMember] public string client_id { get; set; }
            [DataMember] public bool is_restricted_refugee { get; set; }
            [DataMember] public bool is_dkk_adult_client { get; set; }
            [DataMember] public bool is_young_adult { get; set; }
            [DataMember] public bool need_verification_in_branch { get; set; }
            [DataMember] public bool has_added_security { get; set; }
            [DataMember] public bool is_passive_mode { get; set; }
            [DataMember] public bool ipko3_agreement_access { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseLoginInitResponseDataAfterLoginAction
        {
            //TODO enum
            [DataMember] public string id { get; set; }
            [DataMember] public PKOJsonResponseLoginInitResponseDataAfterLoginActionData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseLoginInitResponseDataAfterLoginActionData
        {
            [DataMember] public string operation_type { get; set; }
            [DataMember] public string access_limiter_id { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseLoginInitResponseDataSso
        {
        }

        [DataContract]
        public class PKOJsonResponseLoginInitResponseDataContextInfo
        {
            //TODO enum
            [DataMember] public string current_context_id { get; set; }
            [DataMember] public bool show_direct_debit_agreement_implied { get; set; }
            [DataMember] public List<PKOJsonResponseLoginInitResponseDataContextInfoContext> context_list { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseLoginInitResponseDataContextInfoContext
        {
            //TODO enum
            [DataMember] public string id { get; set; }
            [DataMember] public PKOJsonResponseLoginInitResponseDataContextInfoContextRef ref_create_account { get; set; }
            [DataMember] public PKOJsonResponseLoginInitResponseDataContextInfoContextRef ref_create_company_account { get; set; }
            [DataMember] public PKOJsonResponseLoginInitResponseDataContextInfoContextRef ref_create_junior_account { get; set; }
            [DataMember] public PKOJsonResponseLoginInitResponseDataContextInfoContextRefInit ref_init { get; set; }
            [DataMember] public PKOJsonResponseLoginInitResponseDataContextInfoContextRefObsoleteInit ref_obsolete_init { get; set; }
            [DataMember] public PKOJsonResponseLoginInitResponseDataContextInfoContextRefCeidgRegister ref_ceidg_register { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseLoginInitResponseDataContextInfoContextRef
        {
            [DataMember] public PKOJsonResponseLoginInitResponseDataContextInfoContextRefData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseLoginInitResponseDataContextInfoContextRefData
        {
            [DataMember] public string application_ctx { get; set; }
            [DataMember] public PKOJsonResponseLoginInitResponseDataContextInfoContextRefDataEntryParams entry_params { get; set; }
            //TODO enum
            [DataMember] public string application_id { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseLoginInitResponseDataContextInfoContextRefDataEntryParams
        {
        }

        [DataContract]
        public class PKOJsonResponseLoginInitResponseDataContextInfoContextRefInit
        {
            //TODO enum
            [DataMember] public string context_id { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseLoginInitResponseDataContextInfoContextRefObsoleteInit
        {
        }

        [DataContract]
        public class PKOJsonResponseLoginInitResponseDataContextInfoContextRefCeidgRegister
        {
        }

        [DataContract]
        public class PKOJsonResponseAddTrustedDevice : PKOJsonResponseFlowBase<PKOJsonResponseAddTrustedDeviceResponse>
        {
            protected override PKOJsonResponseFieldsBase FieldsBase => response.fields;
        }

        [DataContract]
        public class PKOJsonResponseAddTrustedDeviceResponse : PKOJsonResponseResponseBase
        {
            [DataMember] public PKOJsonResponseAddTrustedDeviceResponseFields fields { get; set; }
            [DataMember] public PKOJsonResponseAddTrustedDeviceResponseData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseAddTrustedDeviceResponseFields : PKOJsonResponseFieldsBase
        {
            //[DataMember] public PKOJsonResponseFieldBase  { get; set; }
            [DataMember] public PKOJsonResponseFieldBase name { get; set; }

            protected override List<PKOJsonResponseFieldBaseBaseBase> Fields => new List<PKOJsonResponseFieldBaseBaseBase>() { name };
        }

        [DataContract]
        public class PKOJsonResponseAddTrustedDeviceResponseData
        {
            [DataMember] public string ref_skip { get; set; }
            [DataMember] public PKOJsonResponseResponseDataRefSkipWithAuthVerify ref_skip_with_auth_verify { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseResponseDataRefSkipWithAuthVerify
        {
            [DataMember] public PKOJsonResponseAddTrustedDeviceResponseDataRefSkipWithAuthVerifyData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseAddTrustedDeviceResponseDataRefSkipWithAuthVerifyData
        {
            [DataMember] public string access_limiter_id { get; set; }
            [DataMember] public string operation_type { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseAddTrustedDeviceSubmit : PKOJsonResponseFlowAuthBase<PKOJsonResponseAddTrustedDeviceSubmitResponse>
        {
            protected override PKOJsonResponseFieldsBase FieldsBase => response.fields;
        }

        [DataContract]
        public class PKOJsonResponseAddTrustedDeviceSubmitResponse : PKOJsonResponseResponseAuthBase
        {
            [DataMember] public PKOJsonResponseAddTrustedDeviceSubmitResponseFields fields { get; set; }
            [DataMember] public PKOJsonResponseAddTrustedDeviceSubmitResponseData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseAddTrustedDeviceSubmitResponseFields : PKOJsonResponseFieldsBase
        {
            //[DataMember] public  PKOJsonResponseFieldBase { get; set; }

            protected override List<PKOJsonResponseFieldBaseBaseBase> Fields => new List<PKOJsonResponseFieldBaseBaseBase>() { };
        }

        [DataContract]
        public class PKOJsonResponseAddTrustedDeviceSubmitResponseData
        {
            [DataMember] public string name { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseResponseAuth
        {
            [DataMember] public string auth_method { get; set; }
            [DataMember] public string state { get; set; }

            [DataMember] public string tan_index { get; set; }

            [DataMember] public int approval_period { get; set; }
            [DataMember] public string authorization_id { get; set; }
            [DataMember] public string device_name { get; set; }
            [DataMember] public PKOJsonResponsePhoneNumber phone_number { get; set; }
            [DataMember] public string session_id { get; set; }
            [DataMember] public int sync_request_period { get; set; }
            [DataMember] public int warning_time { get; set; }

            public PKOAuthMethod? AuthMethodValue
            {
                get => auth_method.GetEnumByJsonValue<PKOAuthMethod>();
                set => auth_method = value.GetEnumJsonValue<PKOAuthMethod>();
            }
            public PKOAuthState? StateValue
            {
                get => state.GetEnumByJsonValue<PKOAuthState>();
                set => state = value.GetEnumJsonValue<PKOAuthState>();
            }
        }

        [DataContract]
        public class PKOJsonResponseAddTrustedDeviceConfirm : PKOJsonResponseFlowBase<PKOJsonResponseAddTrustedDeviceSubmitResponse>
        {
            //TODO FieldsBase?
            [DataMember] public bool finished { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseAddedSecurity : PKOJsonResponseBase<PKOJsonResponseAddedSecurityResponse>
        {
        }

        [DataContract]
        public class PKOJsonResponseAddedSecurityResponse : PKOJsonResponseResponseBase
        {
            [DataMember] public PKOJsonResponseAddedSecurityResponseData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseAddedSecurityResponseData
        {
            //[DataMember] public PKOJsonResponseAddedSecurityDataRefAdd ref_add { get; set; }
            //[DataMember] public string ref_list { get; set; }
            //[DataMember] public string ref_list_with_auth_verify { get; set; }
            //[DataMember] public string ref_skip { get; set; }
            [DataMember] public PKOJsonResponseResponseDataRefSkipWithAuthVerify ref_skip_with_auth_verify { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseAddedSecurityDataRefAdd
        {
        }

        [DataContract]
        public class PKOJsonResponseAuthVerify : PKOJsonResponseFlowAuthBase<PKOJsonResponseAuthVerifyResponse>
        {
        }

        [DataContract]
        public class PKOJsonResponseAuthVerifyResponse : PKOJsonResponseResponseAuthBase
        {
        }

        [DataContract]
        public class PKOJsonResponseSubmit : PKOJsonResponseFlowAuthBase<PKOJsonResponseSubmitResponse>
        {
            [DataMember] public bool finished { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseSubmitResponse : PKOJsonResponseResponseAuthBase
        {
            //TODO there can be different data for another request
            //[DataMember] public PKOJsonResponseAuthVerifySubmitResponseTransferData data{ get; set; }
        }

        [DataContract]
        public class PKOJsonResponseSubmitResponseTransferData
        {
            [DataMember] public PKOJsonResponseDataRefZxc ref_zxc { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public bool show_mobile_authorization_promotion { get; set; }
            //[DataMember] public ref_recipient_normal_create_after_transfer { get; set; }
            //[DataMember] public ref_standing_order_normal_create_after_transfer { get; set; }
            //[DataMember] public ref_variable_order_create { get; set; }
            //[DataMember] public ref_recipients_normal { get; set; }
            [DataMember] public bool show_express_payment_notice { get; set; }
            [DataMember] public string recipient_name { get; set; }
            [DataMember] public PKOJsonResponseAmount money { get; set; }
            [DataMember] public string exec_date { get; set; }
            [DataMember] public string ref_external_app_redirect { get; set; }
            [DataMember] public bool is_fraud_detection { get; set; }
            [DataMember] public string approval_deadline { get; set; }
            [DataMember] public string ref_additional_verification { get; set; }

            public DateTime ExecDate => DateTime.Parse(exec_date);
        }

        [DataContract]
        public class PKOJsonResponseMobileStatus : PKOJsonResponseBaseBaseBase
        {
            //TODO enum
            [DataMember] public string lp_status { get; set; }
            //TODO enum
            [DataMember] public string lp_value { get; set; }

            public PKOMobileStatusStatus? StatusValue
            {
                get => lp_status.GetEnumByJsonValue<PKOMobileStatusStatus>();
                set => lp_status = value.GetEnumJsonValue<PKOMobileStatusStatus>();
            }
            public PKOMobileStatusValue? ValueValue
            {
                get => lp_value.GetEnumByJsonValue<PKOMobileStatusValue>();
                set => lp_value = value.GetEnumJsonValue<PKOMobileStatusValue>();
            }
        }

        [DataContract]
        public class PKOJsonResponseLogout : PKOJsonResponseBase<PKOJsonResponseLogoutResponse>
        {
        }

        [DataContract]
        public class PKOJsonResponseLogoutResponse : PKOJsonResponseResponseBase
        {
            [DataMember] public PKOJsonResponseLogoutResponseSession session { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseLogoutResponseSession
        {
            //TODO enum
            [DataMember] public string deactivation_reason { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseAccountInit : PKOJsonResponseBase<PKOJsonResponseAccountInitResponse>
        {
        }

        [DataContract]
        public class PKOJsonResponseAccountInitResponse : PKOJsonResponseResponseBase
        {
            [DataMember] public PKOJsonResponseAccountInitResponseData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseAccountInitResponseData
        {
            [DataMember] public Dictionary<string, PKOJsonResponseAccountInitResponseDataAccount> accounts { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseAccountInitResponseDataAccount
        {
            [DataMember] public string balance { get; set; }
            [DataMember] public string bank_name { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public string holds { get; set; }
            [DataMember] public bool is_business { get; set; }
            [DataMember] public bool is_pkobp { get; set; }
            [DataMember] public string ledger { get; set; }
            [DataMember] public string nickname { get; set; }
            [DataMember] public PKOJsonResponseAccountInitResponseDataAccountNumber number { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string business_credit_limit { get; set; }
            [DataMember] public string is_inactive { get; set; }
            [DataMember] public string komfort_limit { get; set; }
            [DataMember] public string overdraft_limit { get; set; }
            [DataMember] public string revolving_credit_limit { get; set; }
            [DataMember] public string synchronization_time { get; set; }
            [DataMember] public string synchronization_status { get; set; }
            //[DataMember] public ref_change_account_type { get; set; }
            //[DataMember] public ref_disposition_choose { get; set; }
            //[DataMember] public ref_set_account_nickname { get; set; }
            //[DataMember] public ref_account_details { get; set; }
            //[DataMember] public ref_account_manage { get; set; }
            //[DataMember] public ref_dispositions { get; set; }
            //[DataMember] public ref_modify_user_accounts_order { get; set; }
            //[DataMember] public ref_transfer_data{ get; set; }
            //[DataMember] public ref_account_balance { get; set; }
            //[DataMember] public ref_home_operations_completed { get; set; }
            //[DataMember] public ref_home_operations_rejected { get; set; }
            //[DataMember] public ref_home_operations_waiting { get; set; }
            //[DataMember] public ref_account_operations_completed { get; set; }
            //[DataMember] public ref_account_operations_rejected { get; set; }
            //[DataMember] public ref_account_operations_unsettled { get; set; }
            //[DataMember] public ref_account_operations_waiting { get; set; }
            //[DataMember] public ref_transfer_foreign_create { get; set; }
            //[DataMember] public ref_transfer_normal_create { get; set; }
            //[DataMember] public ref_account_owner_confidential_details { get; set; }

            public double Balance => DoubleOperations.Parse(balance, ThousandSeparator.None, DecimalSeparator.Dot);
        }

        [DataContract]
        public class PKOJsonResponseAccountInitResponseDataAccountNumber
        {
            [DataMember] public string value { get; set; }
            [DataMember] public string format { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseRefresh : PKOJsonResponseBase<PKOJsonResponseRefreshResponse>
        {
        }

        [DataContract]
        public class PKOJsonResponseRefreshResponse : PKOJsonResponseResponseBase
        {
        }

        [DataContract]
        public class PKOJsonResponseHistory : PKOJsonResponseBase<PKOJsonResponseHistoryResponse>
        {
            protected override PKOJsonResponseFieldsBase FieldsBase => response.data.filter_form;
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponse : PKOJsonResponseResponseBase
        {
            //TODO enum
            [DataMember] public string case_id { get; set; }
            [DataMember] public PKOJsonResponseHistoryResponseData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponseData
        {
            [DataMember] public List<PKOJsonResponseHistoryResponseDataItem> items { get; set; }
            [DataMember] public List<PKOJsonResponseHistoryResponseDataItem> next_items { get; set; }
            [DataMember] public PKOJsonResponseDataRefZxc ref_zxc { get; set; }
            [DataMember] public PKOJsonResponseHistoryResponseDataNext next { get; set; }
            [DataMember] public PKOJsonResponseHistoryResponseDataFields filter_form { get; set; }
            [DataMember] public PKOJsonResponseHistoryResponseDataSearch search { get; set; }
            [DataMember] public PKOJsonResponseHistoryResponseDataSummary summary { get; set; }
            [DataMember] public string ref_sca_authorization { get; set; }
            [DataMember] public string ref_merchant_data_dialog { get; set; }
            [DataMember] public int sca_days { get; set; }
            [DataMember] public PKOJsonResponseHistoryResponseDataPeriodDates period_dates { get; set; }
            [DataMember] public string google_map_api_key { get; set; }
            [DataMember] public List<string> modified_filter_list { get; set; }
            [DataMember] public PKOJsonResponseHistoryResponseDataRefOperationsDownloadPreferences ref_operations_download_preferences { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponseDataNext
        {
            [DataMember] public string type { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponseDataFields : PKOJsonResponseFieldsBase
        {
            [DataMember] public PKOJsonResponseFieldSchema source_account { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema search_phrase { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema operation_type { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema date_from { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema date_to { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema search_type { get; set; }

            [DataMember] public PKOJsonResponseFieldSchema city { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema symbol { get; set; }

            protected override List<PKOJsonResponseFieldBaseBaseBase> Fields => new List<PKOJsonResponseFieldBaseBaseBase>() { source_account, search_phrase, operation_type, date_from, date_to, search_type, city, symbol };
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponseDataItem
        {
            [DataMember] public string operation_id { get; set; }
            [DataMember] public string date { get; set; }
            [DataMember] public PKOJsonResponseAmount money { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public PKOJsonResponseHistoryResponseDataItemOperationKind operation_kind { get; set; }
            [DataMember] public PKOJsonResponseAmount ending_balance { get; set; }
            //TODO enum
            [DataMember] public string operation_type { get; set; }
            [DataMember] public PKOJsonResponseHistoryResponseDataItemDetails details { get; set; }
            [DataMember] public string avatar_url { get; set; }
            [DataMember] public string google_place_id { get; set; }
            [DataMember] public PKOJsonResponseCoordinates coordinates { get; set; }
            [DataMember] public string other_side { get; set; }
            [DataMember] public string recharge_operator_id { get; set; }
            [DataMember] public bool is_hold { get; set; }
            [DataMember] public bool? is_other_side_recipient { get; set; }
            [DataMember] public string vat_account_number { get; set; }
            [DataMember] public string ebc_margin { get; set; }
            [DataMember] public string avatar { get; set; }
            //[DataMember] public ref_fee_hint { get; set; }
            //[DataMember] public ref_map { get; set; }
            [DataMember] public PKOJsonResponseHistoryResponseDataItemRefOperationCompletedConfirmation ref_operation_completed_confirmation { get; set; }
            //[DataMember] public ref_operation_completed_confirmation_pl { get; set; }
            //[DataMember] public ref_operation_completed_confirmation_en { get; set; }
            //[DataMember] public ref_operation_completed_recipient_create_normal { get; set; }
            //[DataMember] public ref_operation_completed_recipient_create_tax { get; set; }
            //[DataMember] public ref_operation_completed_recipient_create_split { get; set; }
            //[DataMember] public ref_operation_completed_repeat_foreign { get; set; }
            //[DataMember] public ref_operation_completed_repeat_normal { get; set; }
            //[DataMember] public ref_operation_completed_repeat_split { get; set; }
            //[DataMember] public ref_operation_completed_repeat_tax { get; set; }
            //[DataMember] public ref_operation_completed_exec_foreign { get; set; }
            //[DataMember] public ref_operation_completed_exec_normal { get; set; }
            //[DataMember] public ref_operation_completed_exec_split { get; set; }
            //[DataMember] public ref_operation_completed_exec_tax { get; set; }
            //[DataMember] public ref_operation_completed_cancel_foreign { get; set; }
            //[DataMember] public ref_recipient_history { get; set; }
            [DataMember] public PKOJsonResponseHistoryResponseDataItemRefOperationWaitingDetails ref_operation_waiting_details { get; set; }
            [DataMember] public string recipient_name { get; set; }
            [DataMember] public string description { get; set; }
            [DataMember] public string operation_description_kind { get; set; }

            public DateTime Date => DateTime.Parse(date);
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponseDataItemOperationKind
        {
            [DataMember] public string code { get; set; }
            //TODO enum
            [DataMember] public string side { get; set; }

            public PKOOperationKind? CodeValue
            {
                get => code.GetEnumByJsonValue<PKOOperationKind>();
                set => code = value.GetEnumJsonValue<PKOOperationKind>();
            }
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponseDataItemDetails
        {
            [DataMember] public string order_date { get; set; }
            [DataMember] public string exec_date { get; set; }
            [DataMember] public PKOJsonResponseHistoryResponseDataItemOperationKind operation_kind { get; set; }
            [DataMember] public PKOJsonResponseAmount money { get; set; }
            [DataMember] public string other_account { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string recipient_name_and_address { get; set; }
            [DataMember] public string sender_name_and_address { get; set; }
            [DataMember] public PKOJsonResponseAmount ending_balance { get; set; }
            [DataMember] public PKOJsonResponseTaxTransferResponseDataPayerIdentifier payer_identifier { get; set; }
            [DataMember] public string symbol { get; set; }
            [DataMember] public PKOJsonResponseTaxTransferResponseDataPeriod period { get; set; }

            public DateTime OrderDate => DateTime.Parse(order_date);
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponseDataSearch
        {
            //TODO enum
            [DataMember] public string type { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponseDataSummary
        {
            [DataMember] public PKOJsonResponseAmount credit { get; set; }
            [DataMember] public PKOJsonResponseAmount debit { get; set; }
            [DataMember] public List<PKOJsonResponseAmount> total { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponseDataPeriodDates
        {
            [DataMember] public string today { get; set; }
            [DataMember] public string week { get; set; }
            [DataMember] public string month { get; set; }
            [DataMember] public string quarter { get; set; }
            [DataMember] public string year { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponseDataRefOperationsDownloadPreferences
        {
            [DataMember] public PKOJsonResponseHistoryResponseDataRefOperationsDownloadPreferencesData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponseDataRefOperationsDownloadPreferencesData
        {
            [DataMember] public string source_account { get; set; }
            [DataMember] public string search_phrase { get; set; }
            [DataMember] public string amount_from { get; set; }
            [DataMember] public string amount_to { get; set; }
            //TODO enum
            [DataMember] public string operation_type { get; set; }
            [DataMember] public string date_from { get; set; }
            [DataMember] public string date_to { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponseDataItemRefOperationCompletedConfirmation
        {
            [DataMember] public PKOJsonResponseHistoryResponseDataItemRefOperationCompletedConfirmationData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponseDataItemRefOperationCompletedConfirmationData
        {
            [DataMember] public PKOJsonResponseHistoryResponseDataItemRefOperationCompletedConfirmationDataObjectId object_id { get; set; }
            [DataMember] public PKOJsonResponseHistoryResponseDataItemRefOperationCompletedConfirmationDataData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponseDataItemRefOperationWaitingDetails
        {
            [DataMember] public PKOJsonResponseHistoryResponseDataItemRefOperationWaitingDetailsData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponseDataItemRefOperationWaitingDetailsData
        {
            [DataMember] public string source { get; set; }
            [DataMember] public string account { get; set; }
            [DataMember] public string id { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponseDataItemRefOperationCompletedConfirmationDataObjectId
        {
            [DataMember] public string source { get; set; }
            [DataMember] public string account { get; set; }
            [DataMember] public string id { get; set; }
            [DataMember] public string seq { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseHistoryResponseDataItemRefOperationCompletedConfirmationDataData
        {
            [DataMember] public string language { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseConfirmationInit : PKOJsonResponseFlowBase<PKOJsonResponseConfirmationInitResponse>
        {
            protected override PKOJsonResponseFieldsBase FieldsBase => response.fields;
        }

        [DataContract]
        public class PKOJsonResponseConfirmationInitResponse : PKOJsonResponseResponseBase
        {
            [DataMember] public PKOJsonResponseConfirmationResponseFields fields { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseConfirmationResponseFields : PKOJsonResponseFieldsBase
        {
            [DataMember] public PKOJsonResponseField media_type { get; set; }
            [DataMember] public PKOJsonResponseField email { get; set; }

            protected override List<PKOJsonResponseFieldBaseBaseBase> Fields => new List<PKOJsonResponseFieldBaseBaseBase>() { media_type, email };
        }

        [DataContract]
        public class PKOJsonResponseConfirmation : PKOJsonResponseFlowBase<PKOJsonResponseConfirmationResponse>
        {
            [DataMember] public bool finished { get; set; }

            protected override PKOJsonResponseFieldsBase FieldsBase => response.fields;
        }

        [DataContract]
        public class PKOJsonResponseConfirmationResponse : PKOJsonResponseResponseBase
        {
            [DataMember] public PKOJsonResponseConfirmationResponseData data { get; set; }
            [DataMember] public PKOJsonResponseConfirmationResponseFields fields { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseConfirmationResponseData
        {
            [DataMember] public PKOJsonResponseConfirmationResponseDataFile file { get; set; }
            [DataMember] public string media_type { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseConfirmationResponseDataFile
        {
            [DataMember] public string type { get; set; }
            [DataMember] public PKOJsonResponseConfirmationResponseDataFileContent content { get; set; }
            [DataMember] public string name { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseConfirmationResponseDataFileContent
        {
            [DataMember] public string val { get; set; }
            [DataMember] public string fmt { get; set; }
            [DataMember] public string enc { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseTransferInit : PKOJsonResponseFlowBase<PKOJsonResponseTransferInitResponse>
        {
            protected override PKOJsonResponseFieldsBase FieldsBase => response.fields;
        }

        [DataContract]
        public class PKOJsonResponseTransferInitResponse : PKOJsonResponseResponseBase
        {
            [DataMember] public PKOJsonResponseTransferResponseFields fields { get; set; }
            [DataMember] public PKOJsonResponseTransferInitResponseData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseTransferResponseFields : PKOJsonResponseFieldsBase
        {
            [DataMember] public PKOJsonResponseField recipient_address { get; set; }
            [DataMember] public PKOJsonResponseField recipient_name { get; set; }
            [DataMember] public PKOJsonResponseField recipient_account { get; set; }
            [DataMember] public PKOJsonResponseField source_account { get; set; }
            [DataMember] public PKOJsonResponseField recipient_id { get; set; }
            [DataMember] public PKOJsonResponseField money { get; set; }
            [DataMember] public PKOJsonResponseField hold_create { get; set; }
            [DataMember] public PKOJsonResponseField title { get; set; }
            [DataMember] public PKOJsonResponseField payment_date { get; set; }
            [DataMember] public PKOJsonResponseField payment_type { get; set; }
            [DataMember] public PKOJsonResponseField recipient_identifier { get; set; }

            protected override List<PKOJsonResponseFieldBaseBaseBase> Fields => new List<PKOJsonResponseFieldBaseBaseBase>() { recipient_address, recipient_name, recipient_account, source_account, recipient_id, money, hold_create, title, payment_date, payment_type, recipient_identifier };
        }

        [DataContract]
        public class PKOJsonResponseTransferInitResponseData
        {
            [DataMember] public string limit { get; set; }
            [DataMember] public string remaining_limit { get; set; }
            [DataMember] public string currency { get; set; }
            //        [DataMember] public ref_recipients_normal { get; set; }
            //    [DataMember] public ref_vat_whitelist_check { get; set; }
            //[DataMember] public ref_daily_limit_modify { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseTransfer : PKOJsonResponseFlowAuthBase<PKOJsonResponseTransferResponse>
        {
            protected override PKOJsonResponseFieldsBase FieldsBase => response.fields;
        }

        [DataContract]
        public class PKOJsonResponseTransferResponse : PKOJsonResponseResponseAuthBase
        {
            [DataMember] public PKOJsonResponseTransferResponseData data { get; set; }
            [DataMember] public PKOJsonResponseTransferResponseFields fields { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseTransferResponseData
        {
            [DataMember] public string recipient_address { get; set; }
            [DataMember] public string recipient_name { get; set; }
            [DataMember] public string recipient_account { get; set; }
            [DataMember] public string source_account { get; set; }
            [DataMember] public string recipient_bank { get; set; }
            [DataMember] public PKOJsonResponseAmount money { get; set; }
            [DataMember] public bool? hold_create { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string payment_date { get; set; }
            //TODO enum
            [DataMember] public string payment_type { get; set; }
            [DataMember] public string recipient_identifier { get; set; }
            [DataMember] public PKOJsonResponseTransferResponseDataTransactionFees transaction_fees { get; set; }
            [DataMember] public string no_tan_reason { get; set; }
            [DataMember] public PKOJsonResponseDataRefZxc ref_zxc { get; set; }
            [DataMember] public bool has_transfer_date_changed { get; set; }

            public DateTime PaymentDate => DateTime.Parse(payment_date);
        }

        [DataContract]
        public class PKOJsonResponseTransferResponseDataTransactionFees
        {
            [DataMember] public PKOJsonResponseAmount ELIXIR { get; set; }
            [DataMember] public PKOJsonResponseAmount SORBNET { get; set; }
            [DataMember(Name = "EXPRESS-ELIXIR")] public PKOJsonResponseAmount EXPRESSELIXIR { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseTaxTransferInit : PKOJsonResponseFlowBase<PKOJsonResponseTaxTransferInitResponse>
        {
            protected override PKOJsonResponseFieldsBase FieldsBase => response.fields;
        }

        [DataContract]
        public class PKOJsonResponseTaxTransferInitResponse : PKOJsonResponseResponseBase
        {
            [DataMember] public PKOJsonResponseTaxTransferResponseFields fields { get; set; }
            [DataMember] public PKOJsonResponseTaxTransferInitResponseData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseTaxTransferInitResponseData
        {
            [DataMember] public PKOJsonResponseTransferResponseDataTransactionFees transaction_fees { get; set; }
            [DataMember] public List<string> symbols_with_period { get; set; }
            [DataMember] public List<string> symbols_with_vat_account { get; set; }
            [DataMember] public List<string> symbols_with_irp { get; set; }
            [DataMember] public string recipient_name { get; set; }
            [DataMember] public string limit { get; set; }
            [DataMember] public string remaining_limit { get; set; }
            [DataMember] public string currency { get; set; }
            [DataMember] public string ref_daily_limit_modify { get; set; }

            [DataMember] public string symbol { get; set; }
            [DataMember] public string obligation_id { get; set; }
            [DataMember] public PKOJsonResponseTaxTransferResponseDataPeriod period { get; set; }
            [DataMember] public PKOJsonResponseTaxTransferResponseDataPayerIdentifier payer_identifier { get; set; }
            [DataMember] public string source_account { get; set; }
            [DataMember] public string amount { get; set; }
            [DataMember] public bool? hold_create { get; set; }
            [DataMember] public string payment_date { get; set; }
            //TODO enum
            [DataMember] public string tax_type_group { get; set; }
            //TODO enum
            [DataMember] public string payment_type { get; set; }
            //TODO enum
            [DataMember] public string recipient_account_type { get; set; }
            [DataMember] public string individual_tax_account { get; set; }
            [DataMember] public bool has_transfer_date_changed { get; set; }
            //[DataMember] public PKOJsonResponseTransferResponseDataTransactionFees transaction_fees { get; set; }
            //[DataMember] public string recipient_name { get; set; }
            [DataMember] public string no_tan_reason { get; set; }

            public double Amount
            {
                get => DoubleOperations.Parse(amount, ThousandSeparator.None, DecimalSeparator.Dot);
            }
            public DateTime? PaymentDateValue
            {
                get => DateTime.Parse(payment_date);
            }
        }

        [DataContract]
        public class PKOJsonResponseTaxTransferAccount : PKOJsonResponseFlowBase<PKOJsonResponseTaxTransferAccountResponse>
        {
            protected override PKOJsonResponseFieldsBase FieldsBase => response.fields;
        }

        [DataContract]
        public class PKOJsonResponseTaxTransferAccountResponse : PKOJsonResponseResponseBase
        {
            [DataMember] public PKOJsonResponseTaxTransferResponseFields fields { get; set; }
            [DataMember] public PKOJsonResponseTaxTransferAccountResponseData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseTaxTransferAccountResponseData
        {
            [DataMember] public string recipient_name { get; set; }
            [DataMember] public PKOJsonResponseTransferResponseDataTransactionFees transaction_fees { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseTaxOfficeSearch : PKOJsonResponseFlowBase<PKOJsonResponseTaxOfficeSearchResponse>
        {
            protected override PKOJsonResponseFieldsBase FieldsBase => response.data.filter_form;
        }

        [DataContract]
        public class PKOJsonResponseTaxOfficeSearchResponse : PKOJsonResponseResponseBase
        {
            [DataMember] public PKOJsonResponseTaxOfficeSearchResponseData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseTaxOfficeSearchResponseData
        {
            [DataMember] public PKOJsonResponseTaxTransferResponseFields filter_form { get; set; }
            [DataMember] public List<PKOJsonResponseTaxOfficeSearchResponseDataItem> items { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseTaxOfficeSearchResponseDataItem
        {
            [DataMember] public string name { get; set; }
            [DataMember] public PKOJsonResponseTaxOfficeSearchResponseDataItemAccount account { get; set; }
            [DataMember] public string city { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseTaxOfficeSearchResponseDataItemAccount
        {
            [DataMember] public string number { get; set; }
            [DataMember] public string format { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseTaxTransfer : PKOJsonResponseFlowAuthBase<PKOJsonResponseTaxTransferResponse>
        {
            protected override PKOJsonResponseFieldsBase FieldsBase => response.fields;
        }

        [DataContract]
        public class PKOJsonResponseTaxTransferResponse : PKOJsonResponseResponseAuthBase
        {
            [DataMember] public PKOJsonResponseTaxTransferInitResponseData data { get; set; }
            [DataMember] public PKOJsonResponseTaxTransferResponseFields fields { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseTaxTransferResponseFields : PKOJsonResponseFieldsBase
        {
            [DataMember] public PKOJsonResponseFieldSchema symbol { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema obligation_id { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema period { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema payer_identifier { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema source_account { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema amount { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema hold_create { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema recipient_id { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema payment_date { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema tax_type_group { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema payment_type { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema recipient_account_type { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema individual_tax_account { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema tax_office_account { get; set; }

            protected override List<PKOJsonResponseFieldBaseBaseBase> Fields => new List<PKOJsonResponseFieldBaseBaseBase>() { symbol, obligation_id, period, payer_identifier, source_account, amount, hold_create, recipient_id, payment_date, tax_type_group, payment_type, recipient_account_type, individual_tax_account, tax_office_account };
        }

        [DataContract]
        public class PKOJsonResponseTaxTransferResponseDataPeriod
        {
            //TODO enum
            [DataMember] public string type { get; set; }
            [DataMember] public string number { get; set; }
            [DataMember] public string month { get; set; }
            [DataMember] public string year { get; set; }
        }

        [DataContract]
        public class PKOJsonResponseTaxTransferResponseDataPayerIdentifier
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
        public class PKOJsonResponsePrepaidInit : PKOJsonResponseFlowBase<PKOJsonResponsePrepaidInitResponse>
        {
            protected override PKOJsonResponseFieldsBase FieldsBase => response.fields;
        }

        [DataContract]
        public class PKOJsonResponsePrepaidInitResponse : PKOJsonResponseResponseBase
        {
            [DataMember] public PKOJsonResponsePrepaidResponseFields fields { get; set; }
            [DataMember] public PKOJsonResponsePrepaidInitResponseData data { get; set; }
        }

        [DataContract]
        public class PKOJsonResponsePrepaidResponseFields : PKOJsonResponseFieldsBase
        {
            [DataMember(Name = "operator")] public PKOJsonResponseFieldSchema operatorValue { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema mobile_phone { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema source_account { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema invoice { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema invoice_receiver { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema use_default_mobile_phone { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema money { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema email { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema processing_regulations { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema recharge_regulations { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema confirm_polish_residence { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema waive_right_to_renounce { get; set; }
            [DataMember] public PKOJsonResponseFieldSchema ado_agreement { get; set; }

            protected override List<PKOJsonResponseFieldBaseBaseBase> Fields => new List<PKOJsonResponseFieldBaseBaseBase>() { operatorValue, mobile_phone, source_account, invoice, invoice_receiver, use_default_mobile_phone, money, email, processing_regulations, recharge_regulations, confirm_polish_residence, waive_right_to_renounce, ado_agreement };
        }

        [DataContract]
        public class PKOJsonResponsePrepaidInitResponseData
        {
            [DataMember] public List<PKOJsonResponsePrepaidInitResponseDataOperator> operators_info { get; set; }
            [DataMember] public string default_mobile_phone { get; set; }
            [DataMember] public string ref_daily_limit_modify { get; set; }
        }

        [DataContract]
        public class PKOJsonResponsePrepaidInitResponseDataOperator
        {
            [DataMember] public string id { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public PKOJsonResponsePrepaidInitResponseDataOperatorAmountAllowed amount_allowed { get; set; }
        }

        [DataContract]
        public class PKOJsonResponsePrepaidInitResponseDataOperatorAmountAllowed
        {
            [DataMember] public List<double> amounts { get; set; }
            [DataMember] public double? min_amount { get; set; }
            [DataMember] public double? max_amount { get; set; }
        }

        [DataContract]
        public class PKOJsonResponsePrepaid : PKOJsonResponseFlowAuthBase<PKOJsonResponsePrepaidResponse>
        {
            protected override PKOJsonResponseFieldsBase FieldsBase => response.fields;
        }

        [DataContract]
        public class PKOJsonResponsePrepaidResponse : PKOJsonResponseResponseAuthBase
        {
            [DataMember] public PKOJsonResponsePrepaidResponseData data { get; set; }
            [DataMember] public PKOJsonResponsePrepaidResponseFields fields { get; set; }
        }

        [DataContract]
        public class PKOJsonResponsePrepaidResponseData
        {
            [DataMember(Name = "operator")] public string operatorValue { get; set; }
            [DataMember] public string mobile_phone { get; set; }
            [DataMember] public string source_account { get; set; }
            [DataMember] public bool invoice { get; set; }
            [DataMember] public PKOJsonResponsePrepaidResponseDataInvoiceReceiver invoice_receiver { get; set; }
            [DataMember] public bool? use_default_mobile_phone { get; set; }
            [DataMember] public PKOJsonResponseAmount money { get; set; }
            [DataMember] public bool ado_agreement { get; set; }
            [DataMember] public bool confirm_polish_residence { get; set; }
            [DataMember] public string email { get; set; }
            [DataMember] public bool processing_regulations { get; set; }
            [DataMember] public bool recharge_regulations { get; set; }
            [DataMember] public bool waive_right_to_renounce { get; set; }
        }

        [DataContract]
        public class PKOJsonResponsePrepaidResponseDataInvoiceReceiver
        {
            [DataMember] public string invoice_business { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string address { get; set; }
            [DataMember] public string nip { get; set; }
        }
    }
}
