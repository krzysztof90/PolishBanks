using System.Runtime.Serialization;

namespace BankService.Bank_GetinBank
{
    public class GetinBankBrowserJsonRequest
    {
        [DataContract]
        public class GetinBankBrowserJsonRequestFingerprint
        {
            [DataMember]
            public string userAgent { get; set; }
        }
    }
}
