using System.Runtime.Serialization;

namespace BankService.Bank_GetinBank
{
    public class GetinBankJsonResponse
    {
        [DataContract]
        public class GetinJsonLoginPassword
        {
            [DataMember]
            public bool success { get; set; }
            [DataMember]
            public int type { get; set; }
            [DataMember]
            public string redirect { get; set; }
            [DataMember]
            public bool clear { get; set; }
        }

        [DataContract]
        public class GetinJsonConfirmation
        {
            [DataMember]
            public string url { get; set; }
            [DataMember]
            public string destination { get; set; }
            [DataMember]
            public bool confirmed { get; set; }
            [DataMember]
            public string confirmation_type { get; set; }
            [DataMember]
            public string confirmation_code { get; set; }
            [DataMember]
            public bool is_without_confirmation { get; set; }

            [DataMember]
            public bool is_last_try { get; set; }
            [DataMember]
            public bool allow { get; set; }
            [DataMember]
            public GetinJsonTransferConfirmValidationMessages validationMessages { get; set; }

            [DataMember]
            public string events { get; set; }
        }

        [DataContract]
        public class GetinJsonLoginConfirmation : GetinJsonConfirmation
        {
        }

        [DataContract]
        public class GetinJsonMake
        {
            [DataMember]
            public string confirmation { get; set; }
            [DataMember]
            public string buttons { get; set; }
            [DataMember]
            public bool allow { get; set; }
            [DataMember]
            public string step { get; set; }
            [DataMember]
            public string confirmation_code { get; set; }
            [DataMember]
            public string type { get; set; }
            [DataMember]
            public string destination_error { get; set; }
            [DataMember]
            public string destination_success { get; set; }
            [DataMember]
            public bool is_last_try { get; set; }
            [DataMember]
            public bool is_method_switch_available { get; set; }
        }

        [DataContract]
        public class GetinJsonUntrustedDeviceMake : GetinJsonMake
        {
        }

        [DataContract]
        public class GetinJsonUntrustedDeviceConfirmation : GetinJsonConfirmation
        {
        }

        [DataContract]
        public class GetinJsonMobileConfirmation
        {
            [DataMember]
            public string data_type { get; set; }
            [DataMember]
            public string title { get; set; }
            [DataMember]
            public string content { get; set; }
            [DataMember]
            public string type { get; set; }
            [DataMember]
            public bool error { get; set; }
            [DataMember]
            public int status { get; set; }
            [DataMember]
            public string msg { get; set; }
            [DataMember]
            public string confirmation_code { get; set; }
            [DataMember]
            public string url { get; set; }
            [DataMember]
            public string destination { get; set; }
            [DataMember]
            public bool confirmed { get; set; }
            [DataMember]
            public string confirmation_type { get; set; }
            //[DataMember]
            //public string confirmation_code { get; set; }
            [DataMember]
            public bool is_without_confirmation { get; set; }
            [DataMember]
            public string events { get; set; }
        }

        [DataContract]
        public class GetinJsonHash
        {
            [DataMember]
            public string hash { get; set; }
        }

        [DataContract]
        public class GetinJsonHeartbeat
        {
            [DataMember]
            public bool access { get; set; }
            [DataMember]
            public bool sessionExpire { get; set; }
            [DataMember]
            public string sessionText { get; set; }
            [DataMember]
            public string html { get; set; }
            [DataMember]
            public int sessionLeft { get; set; }
        }

        [DataContract]
        public class GetinJsonExpired
        {
            [DataMember]
            public int type { get; set; }
            [DataMember]
            public string redirect { get; set; }
            [DataMember]
            public bool clear { get; set; }
        }

        [DataContract]
        public class GetinJsonBankName
        {
            [DataMember]
            public bool allow { get; set; }
            [DataMember]
            public int bank_country { get; set; }
            [DataMember]
            public int bank_own { get; set; }
            [DataMember]
            public string bank_currency { get; set; }
            [DataMember]
            public string bank_name { get; set; }
            [DataMember]
            public int is_credit { get; set; }
            [DataMember]
            public int is_repayment_enabled { get; set; }
        }

        [DataContract]
        public class GetinJsonTransferMake : GetinJsonMake
        {
            //[DataMember]
            //public string code_confirmation { get; set; }
            //[DataMember]
            //public int confirmation_type { get; set; }
            //[DataMember]
            //public int sms_no { get; set; }
            //#region przelew
            //[DataMember]
            //public string confirmation_hash { get; set; }
            //[DataMember]
            //public string tan1_label { get; set; }
            //[DataMember]
            //public string tan2_label { get; set; }
            //[DataMember]
            //public string fraud_level { get; set; }
            //[DataMember]
            //public string code_define_cyclics { get; set; }
            //[DataMember]
            //public string code_transactions { get; set; }
            //#endregion
            //#region doładowanie telefonu
            //[DataMember]
            //public string title { get; set; }
            //[DataMember]
            //public string recipient_name { get; set; }
            //[DataMember]
            //public string recipient_address { get; set; }
            //[DataMember]
            //public string recipient_nrb { get; set; }
            //[DataMember]
            //public string recipient_bank_name { get; set; }
            //[DataMember]
            //public string transaction_date { get; set; }
            //#endregion
            //[DataMember]
            //public bool allow { get; set; }
        }

        [DataContract]
        public class GetinJsonTransferConfirm
        {
            [DataMember]
            public bool confirmed { get; set; }
            [DataMember]
            public string destination { get; set; }
            [DataMember]
            public string url { get; set; }
            [DataMember]
            public bool allow { get; set; }
            [DataMember]
            public GetinJsonTransferConfirmValidationMessages validationMessages { get; set; }
        }

        [DataContract]
        public class GetinJsonTransferConfirmValidationMessages
        {
            [DataMember]
            public string token { get; set; }
        }

        [DataContract]
        public class GetinJsonDepositDetails
        {
            [DataMember]
            public string code { get; set; }
            [DataMember]
            public bool ghost { get; set; }
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string symbol { get; set; }
            [DataMember]
            public int? min_amount { get; set; }
            [DataMember]
            public int? max_amount { get; set; }
            [DataMember]
            public string currency { get; set; }
            [DataMember]
            public bool rbt { get; set; }
            [DataMember]
            public int? limit { get; set; }
            [DataMember]
            public string description { get; set; }
            [DataMember]
            public bool personalized { get; set; }
            [DataMember]
            public int percent_type { get; set; }
            [DataMember]
            public int type { get; set; }
            [DataMember]
            public int subtype { get; set; }
            [DataMember]
            public string code_dict_offer_types { get; set; }
            [DataMember]
            public int? interval_group { get; set; }
            [DataMember]
            public string start_date { get; set; }
            [DataMember]
            public string external_url { get; set; }
            [DataMember]
            public string external_id { get; set; }
            [DataMember]
            public bool is_verification_required { get; set; }
            [DataMember]
            public bool is_verification_skip_possible { get; set; }
            [DataMember]
            public string code_dict_program_recommendation_types { get; set; }
            [DataMember]
            public GetinJsonDepositDetailsProduct[] products { get; set; }
            [DataMember]
            public int[] segments { get; set; }
        }

        [DataContract]
        public class GetinJsonDepositDetailsProduct
        {
            [DataMember]
            public string code { get; set; }
            [DataMember]
            public bool extendable { get; set; }
            [DataMember]
            public bool renewable { get; set; }
            [DataMember]
            public bool ghost { get; set; }
            [DataMember]
            public string personalization { get; set; }
            [DataMember]
            public string window_duration { get; set; }
            [DataMember]
            public string window_interval { get; set; }
            [DataMember]
            public int? min_amount { get; set; }
            [DataMember]
            public int? max_amount { get; set; }
            [DataMember]
            public string start_date { get; set; }
            [DataMember]
            public string end_date { get; set; }
            [DataMember]
            public int? limit { get; set; }
            [DataMember]
            public string symbol { get; set; }
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public int type { get; set; }
            [DataMember]
            public int subtype { get; set; }
            [DataMember]
            public string currency { get; set; }
            [DataMember]
            public int proposal_validity_interval { get; set; }
            [DataMember]
            public string code_dict_cust_profile { get; set; }
            [DataMember]
            public string code_parents { get; set; }
            [DataMember]
            public string code_off_mapper { get; set; }
            [DataMember]
            public GetinJsonDepositDetailsProductInterval[] intervals { get; set; }
            [DataMember]
            public GetinJsonDepositDetailsProductAgreement[] agreements { get; set; }
            [DataMember]
            public GetinJsonDepositDetailsProductLlinfoAgreement[] klinfo_agreements { get; set; }
        }
        [DataContract]
        public class GetinJsonDepositDetailsProductInterval
        {
            [DataMember]
            public string code { get; set; }
            [DataMember]
            public int time { get; set; }
            [DataMember]
            public int type { get; set; }
            [DataMember]
            public int? limit { get; set; }
            [DataMember]
            public GetinJsonDepositDetailsProductIntervalPercent[] percents { get; set; }
        }
        [DataContract]
        public class GetinJsonDepositDetailsProductIntervalPercent
        {
            [DataMember]
            public string code { get; set; }
            [DataMember]
            public string value { get; set; }
            [DataMember]
            public int? percent_promo { get; set; }
            [DataMember]
            public int? min_amount { get; set; }
            [DataMember]
            public int? max_amount { get; set; }
            [DataMember]
            public int newPercent { get; set; }
        }
        [DataContract]
        public class GetinJsonDepositDetailsProductAgreement
        {
            [DataMember]
            public string code { get; set; }
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string body { get; set; }
            [DataMember]
            public string body_after { get; set; }
            [DataMember]
            public string body_html { get; set; }
            [DataMember]
            public string body_html_after { get; set; }
            [DataMember]
            public bool is_required { get; set; }
            [DataMember]
            public int sequence { get; set; }
            [DataMember]
            public string id_klinfo { get; set; }
            [DataMember]
            public string code_klinfo_related { get; set; }
            [DataMember]
            public string custom_params { get; set; }
            [DataMember]
            public int id_dict_files { get; set; }
            [DataMember]
            public string code_parent_agreement { get; set; }
            [DataMember]
            public int agreement_type { get; set; }
            [DataMember]
            public string code_dict_cust_profile { get; set; }
            [DataMember]
            public GetinJsonDepositDetailsProductAgreementOption[] options { get; set; }
        }
        [DataContract]
        public class GetinJsonDepositDetailsProductAgreementOption
        {
        }
        [DataContract]
        public class GetinJsonDepositDetailsProductLlinfoAgreement
        {
        }
    }
}
