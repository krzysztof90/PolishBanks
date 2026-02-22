namespace BankService.Tax.TaxCreditorIdentifiers
{
    public class TaxCreditorIdentifierPESEL : TaxCreditorIdentifier
    {
        public string Id { get; set; }

        public TaxCreditorIdentifierPESEL(string id) : base()
        {
            Id = id;
        }

        public override string GetId()
        {
            return Id;
        }
    }
}
