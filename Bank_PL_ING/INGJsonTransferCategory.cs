using Tools;

namespace BankService.Bank_PL_ING
{
    public enum INGJsonTransferCategory
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
        //[JsonValueInt(11)] //TODO difference is with pfs set to 10; above not passed at all
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
