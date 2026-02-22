using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Tools;

namespace BankService.Bank_PL_VeloBank
{
    public class VeloBankJsonResponse
    {
        [DataContract]
        public class VeloJsonResponseBase
        {
            [DataMember] public List<VeloJsonResponseError> errors { get; set; }
            [DataMember] public string hash_log { get; set; }

            public bool CheckErrorExists(int code)
            {
                return errors?.Any(e => e.error == code) ?? false;
            }
        }

        [DataContract]
        public class VeloJsonResponseError
        {
            [DataMember] public string error_description { get; set; }
            [DataMember] public int error { get; set; }
        }

        [DataContract]
        public class VeloJsonResponseHeartbeat : VeloJsonResponseBase
        {
        }

        [DataContract]
        public class VeloJsonResponseLoginLogin : VeloJsonResponseBase
        {
            [DataMember] public bool is_password_plain { get; set; }
            [DataMember] public string password_combinations { get; set; }
        }

        [DataContract]
        public class VeloJsonResponseLoginQRGenerate : VeloJsonResponseBase
        {
            [DataMember] public string qr_uuid { get; set; }
            [DataMember] public string qr_code { get; set; }
            [DataMember] public string short_code { get; set; }
            [DataMember] public int time_left { get; set; }
        }

        [DataContract]
        public class VeloJsonResponseLoginQRStatus : VeloJsonResponseBase
        {
            [DataMember] public string status { get; set; }
            [DataMember] public string qr_hash { get; set; }

            public VeloBankJsonLoginQRStatus? StatusValue
            {
                get { return status.GetEnumByJsonValue<VeloBankJsonLoginQRStatus>(); }
                set { status = value.GetEnumJsonValue<VeloBankJsonLoginQRStatus>(); }
            }
        }

        [DataContract]
        public class VeloJsonResponseLogout : VeloJsonResponseBase
        {
        }

        [DataContract]
        public class VeloJsonResponseConfirmable : VeloJsonResponseBase
        {
            [DataMember] public string uuid { get; set; }
            [DataMember] public string type { get; set; }
            [DataMember] public int sms_no { get; set; }
            [DataMember] public bool is_last_try { get; set; }
            [DataMember] public bool is_method_switch_available { get; set; }

            public VeloBankJsonConfirmType? TypeValue
            {
                get { return type.GetEnumByJsonValue<VeloBankJsonConfirmType>(); }
                set { type = value.GetEnumJsonValue<VeloBankJsonConfirmType>(); }
            }
        }

        [DataContract]
        public class VeloJsonResponseConfirmResponseSessionCreate
        {
            [DataMember] public string access_token { get; set; }
            [DataMember] public string device_name { get; set; }
            [DataMember] public int session_duration { get; set; }
            [DataMember] public int session_inactivity_interval { get; set; }
            [DataMember] public int session_expiration_warning { get; set; }
            [DataMember] public string default_context_hash { get; set; }
        }

        [DataContract]
        public class VeloJsonResponseLoginSessionCreate : VeloJsonResponseConfirmable
        {
            [DataMember] public VeloJsonResponseLoginPasswordConfirmationData confirmation_data { get; set; }
            [DataMember] public string access_token { get; set; }
            [DataMember] public VeloJsonResponseLoginPasswordConfirmationParams confirmation_params { get; set; }
            [DataMember] public string device_name { get; set; }
            [DataMember] public string device_fingerprint { get; set; }
            [DataMember] public int session_duration { get; set; }
            [DataMember] public int session_inactivity_interval { get; set; }
            [DataMember] public int session_expiration_warning { get; set; }
            [DataMember] public string default_context_hash { get; set; }
        }

        [DataContract]
        public class VeloJsonResponseLoginPasswordConfirmationParams
        {
            [DataMember] public bool is_confirmation_geolocalization { get; set; }
            [DataMember] public bool is_confirmation_device { get; set; }
            [DataMember] public bool is_show_add_device_popup { get; set; }
            [DataMember] public bool is_strong_auth_required { get; set; }
        }

        [DataContract]
        public class VeloJsonResponseLoginPasswordConfirmationData
        {
            [DataMember] public string type { get; set; }
        }

        [DataContract]
        public class VeloJsonResponseRememberDevice : VeloJsonResponseConfirmable
        {
        }

        [DataContract]
        public class VeloJsonResponseConfirm : VeloJsonResponseBase
        {
            [DataMember] public string status { get; set; }
            [DataMember] public VeloJsonResponseConfirmResponse response { get; set; }
            [DataMember] public int seconds_left { get; set; }

            public VeloBankJsonConfirmationStatusType? StatusValue
            {
                get { return status.GetEnumByJsonValue<VeloBankJsonConfirmationStatusType>(); }
                set { status = value.GetEnumJsonValue<VeloBankJsonConfirmationStatusType>(); }
            }
        }

        [DataContract]
        public class VeloJsonResponseConfirmResponse
        {
            [DataMember] public VeloJsonResponseConfirmResponseSessionCreate session_create { get; set; }
            [DataMember] public VeloJsonResponseConfirmResponseTransfersDomestic transfers_domestic { get; set; }
            [DataMember] public VeloJsonResponseConfirmResponseConsentAccept consent_accept { get; set; }
        }

        [DataContract]
        public class VeloJsonResponseConfirmResponseTransfersDomestic
        {
            [DataMember] public bool is_confirmed { get; set; }
            [DataMember] public string id_transactions { get; set; }
            [DataMember] public string id_define_cyclics { get; set; }
            [DataMember] public List<string> permitted_operations { get; set; }
            [DataMember] public string required_action { get; set; }
            [DataMember] public VeloBankJsonResponseAmount amount { get; set; }
        }

        [DataContract]
        public class VeloJsonResponseConfirmResponseConsentAccept
        {
            [DataMember] public string url { get; set; }
            [DataMember] public string redirect_type { get; set; }
            [DataMember] public string redirect_url { get; set; }
            [DataMember] public bool is_success { get; set; }
            [DataMember] public string status { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseAccounts : VeloJsonResponseBase
        {
            [DataMember] public VeloBankJsonResponseAccountsAccounts accounts { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseFastTransferAuthorize : VeloJsonResponseBase
        {
            [DataMember] public string type { get; set; }
            [DataMember] public DateTime time_limit { get; set; }
            [DataMember] public List<VeloBankJsonResponseFastTransferAuthorizeAgreement> agreements { get; set; }
            [DataMember] public string tpp { get; set; }
            [DataMember] public string redirect_uri { get; set; }
            [DataMember] public string authorize_request_key { get; set; }
            [DataMember] public int remaining_time_to_accept { get; set; }
            [DataMember] public int total_time_to_accept { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseFastTransferPBL : VeloJsonResponseBase
        {
            [DataMember] public string id { get; set; }
            [DataMember] public VeloBankJsonResponseFastTransferPBLPayment payment { get; set; }
            [DataMember] public List<VeloBankJsonResponseFastTransferProduct> products { get; set; }
            [DataMember] public List<string> owners { get; set; }
            [DataMember] public int remaining_time_to_accept { get; set; }
            [DataMember] public int total_time_to_accept { get; set; }
            [DataMember] public string redirect_uri { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseFastTransferPBLPayment
        {
            [DataMember] public string transaction_date { get; set; }
            [DataMember] public VeloBankJsonResponseAmount amount { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string recipient_name { get; set; }
            [DataMember] public string recipient_address { get; set; }
            [DataMember] public string remitter_name { get; set; }
            [DataMember] public string remitter_address { get; set; }
            [DataMember] public VeloBankJsonResponseAccountNumber nrb { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseFastTransferAcceptAuthorize : VeloJsonResponseConfirmable
        {
            [DataMember] public DateTime time_limit { get; set; }
            [DataMember] public List<VeloBankJsonResponseFastTransferAuthorizeAgreement> agreements { get; set; }
            [DataMember] public string tpp { get; set; }
            [DataMember] public string redirect_uri { get; set; }
            [DataMember] public string authorize_request_key { get; set; }
            [DataMember] public int remaining_time_to_accept { get; set; }
            [DataMember] public int total_time_to_accept { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseFastTransferAcceptPBL : VeloJsonResponseConfirmable
        {
        }

        [DataContract]
        public class VeloBankJsonResponseFastTransferAuthorizeAgreement
        {
            [DataMember] public string id_agreement { get; set; }
            [DataMember] public bool is_editable { get; set; }
            [DataMember] public List<VeloBankJsonResponseFastTransferAuthorizeAgreementProduct> products { get; set; }
            [DataMember] public List<string> owners { get; set; }
            [DataMember] public List<VeloBankJsonResponseFastTransferAuthorizeAgreementDetail> details { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseFastTransferProduct
        {
            [DataMember] public VeloBankJsonResponseAccountNumber account_number { get; set; }
            [DataMember] public VeloBankJsonResponseAmount available_funds { get; set; }
            [DataMember] public bool is_coowner { get; set; }
            [DataMember] public string id { get; set; }
            [DataMember] public string product_name { get; set; }
            [DataMember] public string display_name { get; set; }
            [DataMember] public bool is_default { get; set; }
            [DataMember] public List<string> permitted_operations { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseFastTransferAuthorizeAgreementProduct : VeloBankJsonResponseFastTransferProduct
        {
            [DataMember] public string type { get; set; }
            [DataMember] public List<string> related_accounts { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseFastTransferAuthorizeAgreementProductPayment
        {
            [DataMember] public string delivery_mode { get; set; }
            [DataMember] public string system { get; set; }
            [DataMember] public string execution_mode { get; set; }
            [DataMember] public VeloBankJsonResponseFastTransferAuthorizeAgreementProductPaymentRecipient recipient { get; set; }
            [DataMember] public VeloBankJsonResponseFastTransferAuthorizeAgreementProductPaymentSender sender { get; set; }
            [DataMember] public VeloBankJsonResponseFastTransferAuthorizeAgreementProductPaymentTransferData transfer_data { get; set; }
            [DataMember] public bool is_foreign { get; set; }
            [DataMember] public bool is_split_payment { get; set; }
            [DataMember] public bool is_commission_account_fixed { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseFastTransferAuthorizeAgreementProductPaymentRecipient
        {
            [DataMember] public VeloBankJsonResponseAccountNumber account_number { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string address { get; set; }
            [DataMember] public VeloBankJsonResponseBank bank { get; set; }
            [DataMember] public VeloBankJsonResponseCountry country { get; set; }
            [DataMember] public string account_type { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseFastTransferAuthorizeAgreementProductPaymentSender
        {
            [DataMember] public string name { get; set; }
            [DataMember] public string address { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseFastTransferAuthorizeAgreementProductPaymentTransferData
        {
            [DataMember] public string title { get; set; }
            [DataMember] public VeloBankJsonResponseAmount amount { get; set; }
            [DataMember] public string realization_date { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseCountry
        {
            [DataMember] public int id_country { get; set; }
            [DataMember] public string country_code { get; set; }
            [DataMember] public string description { get; set; }
            [DataMember] public bool is_eea_country { get; set; }
            [DataMember] public bool is_iban { get; set; }
            [DataMember] public bool is_bad_iban_accepted { get; set; }
            [DataMember] public int iban_length { get; set; }
            [DataMember] public int iban_bank_no_from { get; set; }
            [DataMember] public int iban_bank_no_to { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseFastTransferAuthorizeAgreementDetail
        {
            [DataMember] public string type { get; set; }
            [DataMember] public VeloBankJsonResponseAmount transfers_total_amount { get; set; }
            [DataMember] public List<VeloBankJsonResponseFastTransferAuthorizeAgreementProductPayment> payments { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseAccountsAccounts
        {
            [DataMember] public List<string> permitted_operations { get; set; }
            [DataMember] public List<VeloBankJsonResponseAccountsAccountsSummary> summary { get; set; }
            [DataMember] public int order { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseAccountsAccountsSummary
        {
            [DataMember] public List<VeloBankJsonResponseAccountsAccountsSummaryProduct> products { get; set; }
            [DataMember] public VeloBankJsonResponseAmount funds { get; set; }
            [DataMember] public int items { get; set; }
            [DataMember] public int order { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseAccountsAccountsSummaryProduct
        {
            [DataMember] public string type { get; set; }
            [DataMember] public bool is_coowner { get; set; }
            [DataMember] public VeloBankJsonResponseAccountNumber account_number { get; set; }
            [DataMember] public VeloBankJsonResponseAmount available_funds { get; set; }
            [DataMember] public VeloBankJsonResponseAmount balance { get; set; }
            [DataMember] public VeloBankJsonResponseAmount authorization_amount { get; set; }
            [DataMember] public string id { get; set; }
            [DataMember] public string product_name { get; set; }
            [DataMember] public string display_name { get; set; }
            [DataMember] public bool is_default { get; set; }
            [DataMember] public List<string> permitted_operations { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseAccountNumber
        {
            [DataMember] public string country_code { get; set; }
            [DataMember] public string account_number { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseAmount
        {
            [DataMember] public double amount { get; set; }
            [DataMember] public string currency { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseTransferInfo : VeloJsonResponseBase
        {
            [DataMember] public VeloBankJsonResponseBank recipient_bank { get; set; }
            [DataMember] public VeloBankJsonResponseTransferInfoPrognoseFee prognose_fee { get; set; }
            [DataMember] public bool sorbnet_status { get; set; }
            [DataMember] public VeloBankJsonResponseTransferStatus sorbnet { get; set; }
            [DataMember] public VeloBankJsonResponseTransferStatus express_elixir { get; set; }
            [DataMember] public string transfer_time { get; set; }
            [DataMember] public string estimated_delivery_date { get; set; }
            [DataMember] public List<string> currencies { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseTransferPrepaidInfo : VeloJsonResponseBase
        {
            [DataMember] public List<VeloBankJsonResponseTransferPrepaidInfoOperator> operators { get; set; }
            [DataMember] public VeloBankJsonResponseTransferPrepaidInfoAgreement agreement { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseTransferPrepaidInfoOperator
        {
            [DataMember] public string id { get; set; }
            [DataMember] public string display_name { get; set; }
            [DataMember] public List<VeloBankJsonResponseAmount> amounts { get; set; }
            [DataMember] public VeloBankJsonResponseAmount amount_min { get; set; }
            [DataMember] public VeloBankJsonResponseAmount amount_max { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseTransferPrepaidInfoAgreement
        {
            [DataMember] public string id { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string description { get; set; }
            [DataMember] public List<VeloBankJsonResponseFile> file { get; set; }
            [DataMember] public string filename { get; set; }
            [DataMember] public string filepath { get; set; }
            [DataMember] public string sha { get; set; }
            [DataMember] public string srcpath { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseFile
        {
            [DataMember] public string type { get; set; }
            [DataMember] public string filename { get; set; }
            [DataMember] public string filepath { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseBank
        {
            [DataMember] public string bic_or_swift { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string address { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseTransferInfoPrognoseFee
        {
            [DataMember] public VeloBankJsonResponseAmount amount { get; set; }
            [DataMember] public bool status { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseTransferStatus
        {
            [DataMember] public string status { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseTaxFormTypes : VeloJsonResponseBase
        {
            [DataMember] public List<VeloBankJsonResponseTaxFormTypesItem> items { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseTaxFormTypesItem
        {
            [DataMember] public string id_dict_us_form_types { get; set; }
            [DataMember] public string form_type { get; set; }
            [DataMember] public string description { get; set; }
            [DataMember] public bool is_vat_indicator { get; set; }
            [DataMember] public bool is_irp { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseTransferCheck : VeloJsonResponseBase
        {
            [DataMember] public List<string> permitted_operations { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseTransferDomestic : VeloJsonResponseConfirmable
        {
        }

        [DataContract]
        public class VeloBankJsonResponseTransferTax : VeloJsonResponseConfirmable
        {
        }

        [DataContract]
        public class VeloBankJsonResponseTransferPrepaid : VeloJsonResponseConfirmable
        {
            [DataMember] public List<string> permitted_operations { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseTaxOffice : VeloJsonResponseBase
        {
            [DataMember] public List<VeloBankJsonResponseTaxOfficeItem> items { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseTaxOfficeItem
        {
            [DataMember] public VeloBankJsonResponseTaxOfficeItemAccountNumber account_number { get; set; }
            [DataMember] public string id { get; set; }
            [DataMember] public string description { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseTaxOfficeItemAccountNumber
        {
            [DataMember] public string country_code { get; set; }
            [DataMember] public string account_number { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseHistory : VeloJsonResponseBase
        {
            [DataMember] public List<VeloBankJsonResponseHistoryItem> list { get; set; }
            [DataMember] public VeloBankJsonResponseHistoryPagination pagination { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseHistoryItem
        {
            [DataMember] public List<string> permitted_operations { get; set; }
            [DataMember] public string accounting_date { get; set; }
            [DataMember] public VeloBankJsonResponseAmount amount_pln { get; set; }
            [DataMember] public VeloBankJsonResponseAmount balance { get; set; }
            [DataMember] public string ref_no { get; set; }
            [DataMember] public string operation_type { get; set; }
            [DataMember] public VeloBankJsonResponseHistoryItemRemitter remitter { get; set; }
            [DataMember] public VeloBankJsonResponseHistoryItemRecipient recipient { get; set; }
            [DataMember] public string id_product { get; set; }
            [DataMember] public VeloBankJsonResponseHistoryItemUsInfo us_info { get; set; }
            [DataMember] public string id { get; set; }
            [DataMember] public string date { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string card_number { get; set; }
            [DataMember] public string card_provider { get; set; }
            [DataMember] public VeloBankJsonResponseHistoryItemMerchant merchant { get; set; }
            [DataMember] public VeloBankJsonResponseAmount amount { get; set; }
            [DataMember] public string side { get; set; }
            [DataMember] public string status { get; set; }
            [DataMember] public string category { get; set; }

            public DateTime DateValue
            {
                get { return DateTime.Parse(date); }
                set { date = value.Display("yyyy-MM-dd"); }
            }
            public VeloBankJsonCategoryType? CategoryValue
            {
                get { return category.GetEnumByJsonValue<VeloBankJsonCategoryType>(); }
                set { category = value.GetEnumJsonValue<VeloBankJsonCategoryType>(); }
            }
            public VeloBankJsonOperationType? OperationTypeValue
            {
                get { return operation_type.GetEnumByJsonValue<VeloBankJsonOperationType>(); }
                set { operation_type = value.GetEnumJsonValue<VeloBankJsonOperationType>(); }
            }
            public VeloBankJsonSideType? SideValue
            {
                get { return side.GetEnumByJsonValue<VeloBankJsonSideType>(); }
                set { side = value.GetEnumJsonValue<VeloBankJsonSideType>(); }
            }
            public VeloBankJsonOperationStatusType? StatusValue
            {
                get { return status?.GetEnumByJsonValue<VeloBankJsonOperationStatusType>(); }
                set { status = value.GetEnumJsonValue<VeloBankJsonOperationStatusType>(); }
            }
        }

        [DataContract]
        public class VeloBankJsonResponseUnit
        {
            [DataMember] public VeloBankJsonResponseAccountNumber nrb { get; set; }
            [DataMember] public bool inner_nrb { get; set; }
            [DataMember] public string name { get; set; }
            [DataMember] public string address { get; set; }
            [DataMember] public string bank_name { get; set; }
            [DataMember] public VeloBankJsonResponseBank bank { get; set; }
            [DataMember] public bool is_coowner { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseHistoryItemRemitter : VeloBankJsonResponseUnit
        {
            [DataMember] public string display_name { get; set; }
            [DataMember] public string account_type { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseHistoryItemRecipient : VeloBankJsonResponseUnit
        {
        }

        [DataContract]
        public class VeloBankJsonResponseHistoryItemUsInfo
        {
            [DataMember] public string tax_office { get; set; }
            [DataMember] public bool is_irp { get; set; }
            [DataMember] public string form_type { get; set; }
            [DataMember] public string payer_name { get; set; }
            [DataMember] public string payer_address { get; set; }
            [DataMember] public string year { get; set; }
            [DataMember] public string settlement_type { get; set; }
            [DataMember] public string settlement_value { get; set; }
            [DataMember] public string identifier_type { get; set; }
            [DataMember] public string identifier_value { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseHistoryItemMerchant
        {
            [DataMember] public string name { get; set; }
            [DataMember] public string city { get; set; }
            [DataMember] public string country_code { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseHistoryPagination
        {
            [DataMember] public bool has_next_page { get; set; }
            [DataMember] public int limit { get; set; }
        }

        [DataContract]
        public class VeloBankJsonResponseDetailsFile : VeloJsonResponseBase
        {
            [DataMember] public string path { get; set; }
        }
    }
}
