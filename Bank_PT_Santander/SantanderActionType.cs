namespace BankService.Bank_PT_Santander
{
    public enum SantanderActionType
    {
        [SantanderActionNumberAttribute(1)] TransfersHistory,
        [SantanderActionNumberAttribute(1)] TransferPayToTheStateInitParameters,
        [SantanderActionNumberAttribute(2)] TransferInitParameters,
        [SantanderActionNumberAttribute(3)] Login,
        [SantanderActionNumberAttribute(3)] TransferPaymentOfServicesConfirm,
        [SantanderActionNumberAttribute(4)] TransferDetails,
        [SantanderActionNumberAttribute(5)] TransferTopupInitParameters,
        [SantanderActionNumberAttribute(6)] TransferPaymentOfServicesDetails,
        [SantanderActionNumberAttribute(6)] TransferPaymentOfServicesProcess,
        [SantanderActionNumberAttribute(7)] TransferDocument,
        [SantanderActionNumberAttribute(7)] TransferTopup,
        [SantanderActionNumberAttribute(8)] TransferTopupConfirm,
        [SantanderActionNumberAttribute(10)] TransferAuthenticateSMS,
        [SantanderActionNumberAttribute(11)] TransactionsHistory,
        [SantanderActionNumberAttribute(21)] TransferInitType,
        [SantanderActionNumberAttribute(22)] LoginConfirmAccessCode,
        [SantanderActionNumberAttribute(22)] TransactionsHistoryAccessCode,
        [SantanderActionNumberAttribute(23)] TransfersPaymentOfServicesHistory,
        [SantanderActionNumberAttribute(24)] LoginConfirmAfterAccessCode,
        [SantanderActionNumberAttribute(24)] TransactionsHistoryAfterAccessCode,
        [SantanderActionNumberAttribute(25)] TransactionDetails,
        [SantanderActionNumberAttribute(33)] TransferProcess,
        [SantanderActionNumberAttribute(44)] TransferConfirm,
        [SantanderActionNumberAttribute(88)] TransferPaymentOfServicesDocument,
    }
}
