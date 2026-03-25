using Fido2Authenticator.Json;
using System.Runtime.Serialization;

namespace BankService.Bank_PL_PKO
{
    public class Fido2AuthorizationData : AuthorizationData
    {
        [DataMember] public string requestId { get; set; }
        [DataMember] public AuthorizationDataAssertion assertion { get; set; }
    }

    [DataContract]
    public class AuthorizationDataAssertion
    {
        [DataMember] public string authenticatorData { get; set; }
        [DataMember] public string clientDataJSON { get; set; }
        [DataMember] public string signature { get; set; }
    }
}
