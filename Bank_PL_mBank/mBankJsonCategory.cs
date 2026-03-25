using Tools;

namespace BankService.Bank_PL_MBank
{
    public enum MBankJsonCategory
    {
        [JsonValue("Bez kategorii")]
        NoCategory,
        [JsonValue("Podatki")]
        Taxes,
        [JsonValue("TV, internet, telefon")]
        Media,
        [JsonValue("Wpływy - inne")]
        OtherIncomes
    }
}
