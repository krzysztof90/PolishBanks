namespace BankService.Tax.TaxCreditorIdentifiers
{
    public class TaxCreditorIdentifierREGON : TaxCreditorIdentifier
    {
        public string Id { get; set; }

        public TaxCreditorIdentifierREGON(string id) : base()
        {
            Id = id;
        }

        public override string GetId()
        {
            return Id;
        }
    }
}
