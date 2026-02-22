using Fido2Authenticator.Json;
using System.Runtime.Serialization;

namespace BankService.Bank_PL_ING
{
    public class Fido2AuthorizationData : AuthorizationData
    {
        [DataMember] public string type { get; set; }
        [DataMember] public string id { get; set; }
        [DataMember] public string rawId { get; set; }
        [DataMember] public AuthorizationDataResponse response { get; set; }
        [DataMember] public AuthorizationDataClientExtensionResults clientExtensionResults { get; set; }
    }

    public class AuthorizationDataResponse
    {
        [DataMember] public string clientDataJSON { get; set; }
        [DataMember] public string authenticatorData { get; set; }
        [DataMember] public string signature { get; set; }
        [DataMember] public string userHandle { get; set; }
    }

    public class AuthorizationDataClientExtensionResults
    {
        [DataMember] public string appid { get; set; }
        [DataMember] public string appidExclude { get; set; }
        [DataMember] public string credProps { get; set; }
    }
}
