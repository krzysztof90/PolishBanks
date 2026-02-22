namespace BankService.Tax.TaxCreditorIdentifiers
{
    public class TaxCreditorIdentifierIDCard : TaxCreditorIdentifier
    {
        public string Id { get; set; }

        public TaxCreditorIdentifierIDCard(string id) : base()
        {
            Id = id;
        }

        public override string GetId()
        {
            return Id;
        }
    }
}
