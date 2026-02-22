using System.Runtime.Serialization;

namespace BankService.Bank_PL_GetinBank
{
    public class GetinBankJsonRequest
    {
        [DataContract]
        public class GetinBankJsonRequestBrowserFingerprint
        {
            [DataMember] public string userAgent { get; set; }
        }
    }
}
