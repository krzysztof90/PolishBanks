using System;

namespace BankService.BankCountry
{
    public abstract class BankPoland<A, H, F, AccDetResp> : BankBase<A, H, F, AccDetResp> where A : AccountData where H : HistoryItem where F : HistoryFilter where AccDetResp : class
    {
        public override Country Country => Country.Poland;

        public override bool EnabledFastTransfer => true;
        public override bool EnabledPaymentOfServices => false;
        public override bool EnabledPrepaidNIF => false;

        protected abstract bool MakePrepaidTransferMain(string recipient, string phoneNumber, double amount);

        protected override bool MakePrepaidTransfer(string recipient, string phoneNumber, double amount, string nif)
        {
            return MakePrepaidTransferMain(recipient, phoneNumber, amount);
        }

        public override bool MakePaymentOfServicesTransfer(string entity, string reference, double amount)
        {
            throw new ArgumentException();
        }
    }
}
