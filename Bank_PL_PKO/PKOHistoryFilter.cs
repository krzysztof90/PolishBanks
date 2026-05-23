using System;
using System.ComponentModel;

namespace BankService.Bank_PL_PKO
{
    public class PKOHistoryFilter : HistoryFilter
    {
        public PKOFilterOperationType? OperationType { get; set; }
        public PKOFilterSearchType? SearchType { get; set; }

        public PKOHistoryFilter() : base()
        {
        }

        public PKOHistoryFilter(OperationDirection? direction, string title, DateTime? dateFrom, DateTime? dateTo, double? amountExact) : base(direction, title, dateFrom, dateTo, amountExact)
        {
        }
    }

    [Description("Rodzaj operacji")]
    public enum PKOFilterOperationType
    {
        [Description("Wszystkie")]
        [FilterEnumParameterAttribute("ALL")]
        All,
        [Description("Uznania")]
        [FilterEnumParameterAttribute("CREDIT")]
        Credit,
        [Description("Obciążenia")]
        [FilterEnumParameterAttribute("DEBIT")]
        Debit,
        [Description("Przelewy przychodzące")]
        [FilterEnumParameterAttribute("TRANSFER-IN")]
        Incoming,
        [Description("Przelewy wychodzące")]
        [FilterEnumParameterAttribute("TRANSFER-OUT")]
        Outgoing,
        [Description("Przelewy do ZUS")]
        [FilterEnumParameterAttribute("ZUS")]
        ZUS,
        [Description("Przelewy podatkowe")]
        [FilterEnumParameterAttribute("US")]
        US,
        [Description("Spłata kredytu")]
        [FilterEnumParameterAttribute("LOAN-PAYOFF")]
        LoanPayoff,
        [Description("Zlecenia stałe")]
        [FilterEnumParameterAttribute("STANDING-ORDER")]
        StandingOrder,
        [Description("Podzielona płatność (split payment)")]
        [FilterEnumParameterAttribute("SPLIT-PAYMENT")]
        SplitPayment,
        [Description("Płatności kartą")]
        [FilterEnumParameterAttribute("CARD-PAYMENT")]
        CardPayment,
        [Description("Transakcje kartą")]
        [FilterEnumParameterAttribute("CARD-TX")]
        Card,
        [Description("Przelewy na kartę")]
        [FilterEnumParameterAttribute("CARDMONEY")]
        CardMoney,
        [Description("Przelewy z karty")]
        [FilterEnumParameterAttribute("PAYCARD-TRANSFER")]
        CardTransfer,
        [Description("Rozliczenie karty")]
        [FilterEnumParameterAttribute("CARD-CALCULATE")]
        CardCalculate,
        [Description("Opłaty i prowizje")]
        [FilterEnumParameterAttribute("FEE-AND-COMMISSION")]
        FeeCommission,
        [Description("Odsetki")]
        [FilterEnumParameterAttribute("INTEREST")]
        Interest,
        [Description("Wpłaty gotówkowe")]
        [FilterEnumParameterAttribute("CASH-IN")]
        CashIn,
        [Description("Wypłaty gotówkowe")]
        [FilterEnumParameterAttribute("CASH-OUT")]
        CashOut,
        [Description("Otwarcie lokaty")]
        [FilterEnumParameterAttribute("DEPOSIT-OPEN")]
        DepositOpen,
        [Description("Zerwanie lokaty")]
        [FilterEnumParameterAttribute("DEPOSIT-RENOUNCEMENT")]
        DepositRenouncement,
        [Description("Polecenia zapłaty")]
        [FilterEnumParameterAttribute("DIRECT-DEBIT")]
        DirectDebit,
        [Description("Odwołanie polecenia zapłaty")]
        [FilterEnumParameterAttribute("DIRECT-DEBIT-RECALLED")]
        DirectDebitRecalled,
        [Description("Korekty")]
        [FilterEnumParameterAttribute("CORRECTION")]
        Correction,
        [Description("Dyspozycja telefoniczna")]
        [FilterEnumParameterAttribute("PHONE-DISPOSAL")]
        PhoneDisposal,
        [Description("Zlecenie wysokokwotowe")]
        [FilterEnumParameterAttribute("HIGH-ORDER")]
        HighOrder,
        [Description("Zajęcia egzekucyjne")]
        [FilterEnumParameterAttribute("BAILIFF-TAKEOVER")]
        BailiffTakeover,
        [Description("Transakcje czekowe")]
        [FilterEnumParameterAttribute("CHEQUE-TX")]
        Cheque,
        [Description("Płatności mobilne")]
        [FilterEnumParameterAttribute("MOBILE-PAYMENTS")]
        MobilePayments,
        [Description("Płatności mobilne - kod mobilny")]
        [FilterEnumParameterAttribute("MOBILE-PAYMENTS-TX-CODE")]
        MobilePaymentsCode,
        [Description("Płatności mobilne w terminalu")]
        [FilterEnumParameterAttribute("MOBILE-PAYMENTS-POS")]
        MobilePaymentsPOS,
        [Description("Płatności mobilne w bankomacie")]
        [FilterEnumParameterAttribute("MOBILE-PAYMENTS-ATM")]
        MobilePaymentsATM,
        [Description("Płatności mobilne w internecie")]
        [FilterEnumParameterAttribute("MOBILE-PAYMENTS-POS-NO-CARD")]
        MobilePaymentsInternet,
        [Description("Płatności mobilne - przelewy")]
        [FilterEnumParameterAttribute("MOBILE-PAYMENTS-TRANSFERS")]
        MobilePaymentsTransfers,
        [Description("Płatności mobilne - czek")]
        [FilterEnumParameterAttribute("MOBILE-PAYMENTS-CHEQUE")]
        MobilePaymentsCheque,
        [Description("Płatności zbliżeniowe telefonem")]
        [FilterEnumParameterAttribute("MOBILE-CONTACTLESS-PAYMENT")]
        MobileContaclessPayment,
        [Description("Przelewy zagraniczne")]
        [FilterEnumParameterAttribute("FOREIGN-TRANSFER")]
        ForeignTransfer,
        [Description("Autooszczędzanie")]
        [FilterEnumParameterAttribute("AUTOSAVER")]
        Autosaver
    }

    [Description("Sposób szukania")]
    public enum PKOFilterSearchType
    {
        [Description("Odbiorcy lub nadawcy")]
        [FilterEnumParameterAttribute("OTHER_SIDE_OWNER")]
        Name,
        [Description("Tytuły transakcji")]
        [FilterEnumParameterAttribute("TITLE")]
        Title,
        [Description("Numery kont")]
        [FilterEnumParameterAttribute("OTHER_SIDE_NUMBER")]
        Number
    }
}
