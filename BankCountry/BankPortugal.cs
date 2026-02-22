using BankService.Tax.TaxCreditorIdentifiers;
using BankService.Tax.TaxPeriods;
using System;
using System.Collections.Generic;

namespace BankService.BankCountry
{
    public abstract class BankPortugal<A, H, F, AccDetResp> : BankBase<A, H, F, AccDetResp> where A : AccountData where H : HistoryItem where F : HistoryFilter where AccDetResp : class
    {
        public override Country Country => Country.Portugal;

        public override bool EnabledFastTransfer => false;
        public override bool EnabledPaymentOfServices => true;
        public override bool EnabledPrepaidNIF => true;

        protected override bool LoginRequestForFastTransfer(string login, string password, List<object> additionalAuthorization, string transferId)
        {
            throw new ArgumentException();
        }

        protected override string CleanFastTransferUrl(string transferId)
        {
            throw new ArgumentException();
        }

        protected override string MakeFastTransfer(string transferId)
        {
            throw new ArgumentException();
        }

        public override bool MakeTaxTransfer(string taxType, string accountNumber, TaxPeriod period, TaxCreditorIdentifier creditorIdentifier, string creditorName, string obligationId, double amount)
        {
            throw new ArgumentException();
        }
    }
}
