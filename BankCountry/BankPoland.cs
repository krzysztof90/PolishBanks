using BankService.Tax.TaxCreditorIdentifiers;
using BankService.Tax.TaxPeriods;
using System;
using Tools;

namespace BankService.BankCountry
{
    public abstract class BankPoland<A, H, F, AccDetResp> : BankBase<A, H, F, AccDetResp> where A : AccountData where H : HistoryItem where F : HistoryFilter where AccDetResp : class
    {
        public override Country Country => Country.Poland;
        public override string TimeZoneName => "Central European Standard Time";

        public override bool EnabledFastTransfer => true;
        public override bool EnabledPaymentOfServices => false;
        public override bool EnabledPrepaidNIF => false;

        protected abstract bool MakePrepaidTransferMain(string recipient, string phoneNumber, double amount);

        protected override bool MakePrepaidTransfer(string recipient, string phoneNumber, double amount, string nif)
        {
            return MakePrepaidTransferMain(recipient, phoneNumber, amount);
        }

        protected override bool MakePaymentOfServicesTransfer(string entity, string reference, double amount)
        {
            throw new ArgumentException();
        }

        public static string GetTaxCreditorIdentifierTypeIdShort(TaxCreditorIdentifier creditorIdentifier)
        {
            if (creditorIdentifier is TaxCreditorIdentifierNIP)
                return "N";
            else if (creditorIdentifier is TaxCreditorIdentifierIDCard)
                return "1";
            else if (creditorIdentifier is TaxCreditorIdentifierPESEL)
                return "P";
            else if (creditorIdentifier is TaxCreditorIdentifierREGON)
                return "R";
            else if (creditorIdentifier is TaxCreditorIdentifierPassport)
                return "2";
            else if (creditorIdentifier is TaxCreditorIdentifierOther)
                return "3";
            else
                throw new ArgumentException();
        }

        public static string GetTaxPeriodValueShort(TaxPeriod period)
        {
            if (period is TaxPeriodDay taxPeriodDay)
                return $"{GetTaxPeriodYearValue(taxPeriodDay.Day.Year)}J{GetTaxPeriodNumberValue(taxPeriodDay.Day.Day)}{GetTaxPeriodNumberValue(taxPeriodDay.Day.Month)}";
            else if (period is TaxPeriodHalfYear taxPeriodHalfYear)
                return $"{GetTaxPeriodYearValue(taxPeriodHalfYear.Year)}P{GetTaxPeriodNumberValue(taxPeriodHalfYear.Half)}";
            else if (period is TaxPeriodMonth taxPeriodMonth)
                return $"{GetTaxPeriodYearValue(taxPeriodMonth.Year)}M{GetTaxPeriodNumberValue(taxPeriodMonth.Month)}";
            else if (period is TaxPeriodMonthDecade taxPeriodMonthDecade)
                return $"{GetTaxPeriodYearValue(taxPeriodMonthDecade.Year)}D{GetTaxPeriodNumberValue(taxPeriodMonthDecade.Decade)}{GetTaxPeriodNumberValue(taxPeriodMonthDecade.Month)}";
            else if (period is TaxPeriodQuarter taxPeriodQuarter)
                return $"{GetTaxPeriodYearValue(taxPeriodQuarter.Year)}K{GetTaxPeriodNumberValue(taxPeriodQuarter.Quarter)}";
            else if (period is TaxPeriodYear taxPeriodYear)
                return $"{GetTaxPeriodYearValue(taxPeriodYear.Year)}R";
            else
                throw new ArgumentException();
        }

        protected static string GetTaxPeriodNumberValue(int number)
        {
            return number.ToString("D2");
        }

        protected static string GetTaxPeriodYearValue(int year)
        {
            return year.ToString().SubstringFromEx(-2);
        }

    }
}
