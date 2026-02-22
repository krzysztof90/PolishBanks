namespace BankService.Tax.TaxCreditorIdentifiers
{
    public class TaxCreditorIdentifierNIP : TaxCreditorIdentifier
    {
        public string Id { get; set; }

        public TaxCreditorIdentifierNIP(string id) : base()
        {
            Id = id;
        }

        public override string GetId()
        {
            return Id;
        }
    }
}
