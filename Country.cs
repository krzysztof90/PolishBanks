namespace BankService
{
    public enum Country
    {
        [CountryImage("pl")]
        [AreaCodeAttribute("Polska", 48)]
        Poland,
        [CountryImage("pt")]
        [AreaCodeAttribute("Portugal", 351)]
        Portugal
    }
}
