namespace BankService.Tax.TaxCreditorIdentifiers
{
    public abstract class TaxCreditorIdentifier
    {
        //public TaxCreditorIdentifierType IdentifierType { get; set; }
        //public string Id { get; set; }

        public abstract string GetId();

        public TaxCreditorIdentifier()
        {
        }
    }
}
