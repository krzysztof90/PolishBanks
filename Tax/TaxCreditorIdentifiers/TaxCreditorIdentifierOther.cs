namespace BankService.Tax.TaxCreditorIdentifiers
{
    public class TaxCreditorIdentifierOther : TaxCreditorIdentifier
    {
        public string Id { get; set; }

        public TaxCreditorIdentifierOther(string id) : base()
        {
            Id = id;
        }

        public override string GetId()
        {
            return Id;
        }
    }
}
