using Tools;

namespace BankService.Bank_PL_Pocztowy
{
    public enum PocztowyJsonConfirmType
    {
        [JsonValue("SMS")]
        SMS,
        [JsonValue("TOKEN")]
        Mobile,
    }
}
