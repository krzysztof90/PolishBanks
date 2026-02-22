using System.Collections.Generic;
using System.Runtime.Serialization;
using Tools;

namespace BankService.Bank_PL_VeloBank
{
    public class VeloBankJsonRequest
    {
        [DataContract]
        public abstract class VeloJsonRequestBase
        {
            [DataMember] public VeloJsonRequestAdditionalData additional_data { get; set; }

            public VeloJsonRequestBase()
            {
                additional_data = new VeloJsonRequestAdditionalData()
                {
                    browser = new VeloJsonRequestAdditionalDataBrowser()
                    {
                        //with Constants.AppBrowserName doesn't work device remember
                        user_agent = Constants.RealBrowserName,
                        session_storage = true,
                        local_storage = true,
                        indexed_db = true,
                        ad_behavior = false,
                        open_database = false,
                    },
                    device = new VeloJsonRequestAdditionalDataDevice()
                    {
                    },
                    screen = new VeloJsonRequestAdditionalDataScreen()
                    {
                    },
                    system = new VeloJsonRequestAdditionalDataSystem()
                    {
                        timezone_offset = "0",
                        timezone = "Europe/Warsaw"
                    },
                };
            }
        }

        [DataContract]
        public class VeloJsonRequestAdditionalData
        {
            [DataMember] public VeloJsonRequestAdditionalDataBrowser browser { get; set; }
            [DataMember] public VeloJsonRequestAdditionalDataDevice device { get; set; }
            [DataMember] public VeloJsonRequestAdditionalDataScreen screen { get; set; }
            [DataMember] public VeloJsonRequestAdditionalDataSystem system { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestAdditionalDataBrowser
        {
            [DataMember] public string user_agent { get; set; }
            [DataMember] public bool session_storage { get; set; }
            [DataMember] public bool local_storage { get; set; }
            [DataMember] public bool indexed_db { get; set; }
            [DataMember] public bool ad_behavior { get; set; }
            [DataMember] public bool open_database { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestAdditionalDataDevice
        {
        }

        [DataContract]
        public class VeloJsonRequestAdditionalDataScreen
        {
        }

        [DataContract]
        public class VeloJsonRequestAdditionalDataSystem
        {
            [DataMember] public string timezone_offset { get; set; }
            [DataMember] public string timezone { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestLogin : VeloJsonRequestBase
        {
            [DataMember] public string login { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestLoginSessionCreate : VeloJsonRequestBase
        {
            [DataMember] public string module { get; set; }
            [DataMember] public string method { get; set; }

            public VeloBankJsonModuleType? ModuleValue
            {
                get { return module.GetEnumByJsonValue<VeloBankJsonModuleType>(); }
                set { module = value.GetEnumJsonValue<VeloBankJsonModuleType>(); }
            }
        }

        [DataContract]
        public class VeloJsonRequestLoginPassword : VeloJsonRequestLoginSessionCreate
        {
            [DataMember] public string login { get; set; }
            [DataMember] public string password { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestLoginPasswordFastTransferPA : VeloJsonRequestLoginPassword
        {
            [DataMember] public VeloJsonRequestLoginPasswordConsentRequestData consent_request_data { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestLoginQRCode : VeloJsonRequestLoginSessionCreate
        {
            [DataMember] public string code_verifier { get; set; }
            [DataMember] public string qr_hash { get; set; }
            [DataMember] public string qr_uuid { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestLoginPasswordConsentRequestData
        {
            [DataMember] public string authorize_request_key { get; set; }
            [DataMember] public string hash { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestLoginQRGenerate : VeloJsonRequestBase
        {
            [DataMember] public string code_challenge { get; set; }
            //TODO enum
            [DataMember] public string type { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestLoginQRStatus : VeloJsonRequestBase
        {
            [DataMember] public string uuid { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestConfirm : VeloJsonRequestBase
        {
            [DataMember] public string token { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestRememberDevice : VeloJsonRequestBase
        {
            [DataMember] public string option { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestFastTransferAcceptAuthorize : VeloJsonRequestBase
        {
            [DataMember] public List<VeloJsonRequestFastTransferAcceptAuthorizePrivilegeDetail> privilege_details { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestFastTransferAcceptPBL : VeloJsonRequestBase
        {
            [DataMember] public string id_product { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestFastTransferAcceptAuthorizePrivilegeDetail
        {
            [DataMember] public string id { get; set; }
            [DataMember] public List<VeloJsonRequestAccountNumber> products { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestTransferInfo : VeloJsonRequestBase
        {
            [DataMember] public VeloJsonRequestAmount amount { get; set; }
            [DataMember] public VeloJsonRequestAccountNumber sender_account_number { get; set; }
            [DataMember] public VeloJsonRequestAccountNumber recipient_account_number { get; set; }
            [DataMember] public string transfer_mode { get; set; }
            [DataMember] public string transfer_type { get; set; }
            [DataMember] public string transfer_date { get; set; }

            public VeloBankJsonTransferType? TransferValue
            {
                get { return transfer_type.GetEnumByJsonValue<VeloBankJsonTransferType>(); }
                set { transfer_type = value.GetEnumJsonValue<VeloBankJsonTransferType>(); }
            }
        }

        [DataContract]
        public class VeloJsonRequestDetailsFile : VeloJsonRequestBase
        {
            [DataMember] public List<string> id { get; set; }
            [DataMember] public string product { get; set; }
            [DataMember] public string type { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestAmount
        {
            [DataMember] public string amount { get; set; }
            [DataMember] public string currency { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestAccountNumberDomestic
        {
            [DataMember] public string account_number { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestAccountNumber : VeloJsonRequestAccountNumberDomestic
        {
            [DataMember] public string country_code { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestTransferCheck : VeloJsonRequestBase
        {
            [DataMember] public VeloJsonRequestAmount amount { get; set; }
            [DataMember] public VeloJsonRequestAccountNumber recipient_account_number { get; set; }
            [DataMember] public VeloJsonRequestPhone recipient_phone { get; set; }
            [DataMember] public string source_product { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string transfer_mode { get; set; }
            [DataMember] public string transfer_type { get; set; }
            [DataMember] public string payment_date { get; set; }

            public VeloBankJsonTransferType? TransferValue
            {
                get { return transfer_type.GetEnumByJsonValue<VeloBankJsonTransferType>(); }
                set { transfer_type = value.GetEnumJsonValue<VeloBankJsonTransferType>(); }
            }

            public bool ShouldSerializerecipient_account_number()
            {
                return TransferValue == VeloBankJsonTransferType.Transfer;
            }
            public bool ShouldSerializerecipient_phone()
            {
                return TransferValue == VeloBankJsonTransferType.Prepaid;
            }
            public bool ShouldSerializetitle()
            {
                return TransferValue == VeloBankJsonTransferType.Transfer;
            }
        }

        [DataContract]
        public class VeloJsonRequestPhone
        {
            [DataMember] public string id_operator { get; set; }
            [DataMember] public string phone_number { get; set; }
            [DataMember] public string prefix { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestTransferDomestic : VeloJsonRequestBase
        {
            [DataMember] public VeloJsonRequestAmount amount { get; set; }
            [DataMember] public VeloJsonRequestAccountNumber recipient_account_number { get; set; }
            [DataMember] public string recipient_address { get; set; }
            [DataMember] public string recipient_id { get; set; }
            [DataMember] public string recipient_name { get; set; }
            [DataMember] public bool retry_if_lack_of_funds { get; set; }
            [DataMember] public bool save_recipient { get; set; }
            [DataMember] public bool send_notification_to_email { get; set; }
            [DataMember] public string source_product { get; set; }
            [DataMember] public string title { get; set; }
            [DataMember] public string type { get; set; }
            [DataMember] public string payment_date { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestTransferTax : VeloJsonRequestBase
        {
            [DataMember] public VeloJsonRequestAmount amount { get; set; }
            [DataMember] public string source_product { get; set; }
            [DataMember] public VeloJsonRequestAccountNumberDomestic recipient_account_number { get; set; }
            [DataMember] public string recipient_id { get; set; }
            [DataMember] public bool save_recipient { get; set; }
            [DataMember] public string payment_date { get; set; }
            [DataMember] public VeloJsonRequestTransferTaxTaxDeclarationData tax_declaration_data { get; set; }
            [DataMember] public bool send_notification_to_email { get; set; }
            [DataMember] public bool retry_if_lack_of_funds { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestTransferTaxTaxDeclarationData
        {
            [DataMember] public string form_type { get; set; }
            [DataMember] public string identifier_type { get; set; }
            [DataMember] public string identifier_value { get; set; }
            [DataMember] public string obligation_identifier { get; set; }
            [DataMember] public string payer_address { get; set; }
            [DataMember] public string payer_name { get; set; }
            [DataMember] public string tax_office { get; set; }
            [DataMember] public string settlement_type { get; set; }
            [DataMember] public string settlement_value { get; set; }
            [DataMember] public string year { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestTransferPrepaid : VeloJsonRequestBase
        {
            [DataMember] public VeloJsonRequestAmount amount { get; set; }
            [DataMember] public VeloJsonRequestPhone phone_number { get; set; }
            [DataMember] public string source_product { get; set; }
            [DataMember] public string recipient_id { get; set; }
            [DataMember] public string recipient_name { get; set; }
            [DataMember] public bool save_recipient { get; set; }
            [DataMember] public string payment_date { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestHistory : VeloJsonRequestBase
        {
            [DataMember] public string search { get; set; }
            [DataMember] public VeloJsonRequestHistoryFilters filters { get; set; }
            [DataMember] public VeloJsonRequestHistoryPaginator paginator { get; set; }
            [DataMember] public bool show_blockades { get; set; }
        }

        [DataContract]
        public class VeloJsonRequestHistoryFilters
        {
            [DataMember] public string date_from { get; set; }
            [DataMember] public string date_to { get; set; }
            [DataMember] public string min_amount { get; set; }
            [DataMember] public string max_amount { get; set; }
            [DataMember] public string type { get; set; }
            [DataMember] public string kind { get; set; }
            [DataMember] public string status { get; set; }
            [DataMember] public List<string> products { get; set; }
            [DataMember] public List<string> cards { get; set; }

            public bool ShouldSerializedate_from()
            {
                return date_from != null;
            }
            public bool ShouldSerializedate_to()
            {
                return date_to != null;
            }
            public bool ShouldSerializemin_amount()
            {
                return min_amount != null;
            }
            public bool ShouldSerializemax_amount()
            {
                return max_amount != null;
            }
        }

        [DataContract]
        public class VeloJsonRequestHistoryPaginator
        {
            [DataMember] public int limit { get; set; }
            [DataMember] public int page { get; set; }
        }
    }
}
