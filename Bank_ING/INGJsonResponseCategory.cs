using Tools;

namespace BankService.Bank_ING
{
    public enum INGJsonResponseCategory
    {
        [JsonValueInt(1)]
        HouseAndBills,
        [JsonValueInt(2)]
        BaseExpenses,
        [JsonValueInt(3)]
        Finance,
        [JsonValueInt(4)]
        EntertainmentAndTravels,
        [JsonValueInt(5)]
        HealthAndBeauty,
        [JsonValueInt(6)]
        CarAndTransport,
        [JsonValueInt(8)]
        Education,
        //[JsonValueInt(11)]
        //InternalTransfer,
        //[JsonValueInt(11)] //TODO różnica, że pfs ustawiony na 10; powyżej w ogóle nie przekazany
        //None,
        [JsonValueInt(12)]
        Other,
        [JsonValueInt(13)]
        Income,
        [JsonValueInt(20)]
        ClothesAndShoes,
        [JsonValueInt(21)]
        Cash
    }
}
