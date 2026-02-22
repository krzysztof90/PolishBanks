namespace BankService.Tax.TaxCreditorIdentifiers
{
    public class TaxCreditorIdentifierPassport : TaxCreditorIdentifier
    {
        public string Id { get; set; }

        public TaxCreditorIdentifierPassport(string id) : base()
        {
            Id = id;
        }

        public override string GetId()
        {
            return Id;
        }
    }
}
