using Tools;

namespace BankService.Bank_PL_mBank
{
    public enum mBankJsonCategory
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
