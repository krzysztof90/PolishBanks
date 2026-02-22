using System;
using System.Collections.Generic;
using System.Linq;
using Tools;

namespace BankService.Bank_PL_GetinBank
{
    public static class GetinBankAcountNumbersHistory
    {
        private static List<GetinBankHistoryFilter> GetFilters()
        {
            //TODO doesn't catch typ operacji: przelew na telefon
            return new List<GetinBankHistoryFilter>()
            {
                new GetinBankHistoryFilter()
                {
                    DateFrom = Properties.Settings.Default.GetinBankAcountNumbersDownloadDate == default(DateTime) ? DateTime.MinValue : Properties.Settings.Default.VeloBankAcountNumbersDownloadDate,
                    //card operations
                    ChannelType = GetinBankFilterChannel.Bank
                },
                new GetinBankHistoryFilter()
                {
                    DateFrom = Properties.Settings.Default.GetinBankAcountNumbersDownloadDate == default(DateTime) ? DateTime.MinValue : Properties.Settings.Default.VeloBankAcountNumbersDownloadDate,
                    OperationType = GetinBankFilterOperation.Elixir
                },
                new GetinBankHistoryFilter()
                {
                    DateFrom = Properties.Settings.Default.GetinBankAcountNumbersDownloadDate == default(DateTime) ? DateTime.MinValue : Properties.Settings.Default.VeloBankAcountNumbersDownloadDate,
                    OperationType = GetinBankFilterOperation.Blik
                }
            };
        }

        public static List<GetinBankHistoryItem> Download(GetinBank getinService)
        {
            List<GetinBankHistoryItem> operations = GetFilters().SelectMany(f => getinService.GetHistory(f)).Cast<GetinBankHistoryItem>().ToList();
            return SaveOperations(operations);
        }

        private static List<GetinBankHistoryItem> SaveOperations(List<GetinBankHistoryItem> operations)
        {
            List<GetinBankHistoryItem> emptyOperations = null;

            if (operations != null)
            {
                emptyOperations = operations.Where(o => (o.ReferenceNumber == null || o.ReferenceNumber == "-") && o.Type != OperationType.Card && !o.IsCommisionForTransfer()).OrderBy(o => o.OrderDate).ToList();

                if (emptyOperations.Count != 0)
                    Properties.Settings.Default.GetinBankAcountNumbersDownloadDate = emptyOperations.FirstOrDefault().OrderDate;
                else
                    Properties.Settings.Default.GetinBankAcountNumbersDownloadDate = DateTime.Today;

                if (Properties.Settings.Default.GetinBankAcountNumbers == null)
                    Properties.Settings.Default.GetinBankAcountNumbers = new SerializableStringDictionary();
                foreach (GetinBankHistoryItem operation in operations.Where(o => !(o.ReferenceNumber == null || o.ReferenceNumber == "-")))
                    if (!Properties.Settings.Default.GetinBankAcountNumbers.ContainsKey(operation.ReferenceNumber))
                        Properties.Settings.Default.GetinBankAcountNumbers.Add(operation.ReferenceNumber, operation.Direction == OperationDirection.Execute ? operation.ToAccountNumber : operation.FromAccountNumber);
                Properties.Settings.Default.Save();
            }

            return emptyOperations;
        }
    }
}
