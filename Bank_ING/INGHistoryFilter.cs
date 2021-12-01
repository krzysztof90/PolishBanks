namespace BankService.Bank_ING
{
    public class INGHistoryFilter : HistoryFilter
    {
        public bool? ShowIncomingTransfers { get; set; }
        public bool? ShowInternalTransfers { get; set; }
        public bool? ShowExternalTransfers { get; set; }
        public bool? ShowCardTransactionsBlocks { get; set; }
        public bool? ShowCardTransactions { get; set; }
        public bool? ShowATM { get; set; }
        public bool? ShowFees { get; set; }
        public bool? ShowSmartSaver { get; set; }
        public bool? ShowBlocksAndBlockReleases { get; set; }

        public INGHistoryFilter() : base()
        {
        }
    }
}
