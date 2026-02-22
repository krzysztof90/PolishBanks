using Fido2Authenticator.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BankService.Bank_PL_ING
{
    [DataContract]
    public class Fido2Options : Options
    {
        [DataMember] public string challenge { get; set; }
        [DataMember] public List<string> hints { get; set; }
        [DataMember] public string rpId { get; set; }
        [DataMember] public List<OptionsAllowCredentials> allowCredentials { get; set; }
        [DataMember] public OptionsExtensions extensions { get; set; }
     
        public override bool EncodeChallenge => false;
        public override string Challenge => challenge;
        public override bool EncodeResult => true;
        public override IEnumerable<OptionsAllowCredentials> AllowCredentials => allowCredentials;
        public override string RelyingParty => rpId;
    }

    [DataContract]
    public class OptionsExtensions
    {
    }
}
