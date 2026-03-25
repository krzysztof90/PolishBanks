using Fido2Authenticator.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BankService.Bank_PL_PKO
{
    [DataContract]
    public class Fido2Options : Options
    {
        [DataMember] public OptionsData data { get; set; }

        public override bool EncodeChallenge => true;
        public override string Challenge => data.publicKey.challenge;
        public override bool EncodeResult => false;
        public override IEnumerable<OptionsAllowCredentials> AllowCredentials => data.publicKey.allowCredentials;
        public override string RelyingParty => data.publicKey.rpId;
    }

    [DataContract]
    public class OptionsData
    {
        [DataMember] public OptionsDataPublicKey publicKey { get; set; }
        [DataMember] public string requestId { get; set; }
        [DataMember] public string username { get; set; }
    }

    [DataContract]
    public class OptionsDataPublicKey
    {
        [DataMember] public List<OptionsAllowCredentials> allowCredentials { get; set; }
        [DataMember] public string challenge { get; set; }
        [DataMember] public string rpId { get; set; }
        [DataMember] public int timeout { get; set; }
        [DataMember] public string userVerification { get; set; }
    }
}
