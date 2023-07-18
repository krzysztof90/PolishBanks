using BankService.Bank_GetinBank;
using System;
using System.Collections.Generic;
using System.Linq;
using Tools;

namespace BankService.Bank_VeloBank
{
    public static class VeloBankAcountNumbersHistory
    {
        public static void Download(VeloBank veloService)
        {
            VeloBankHistoryFilter filter = new VeloBankHistoryFilter()
            {
                DateFrom = Properties.Settings.Default.VeloBankAcountNumbersDownloadDate == default(DateTime) ? DateTime.MinValue : Properties.Settings.Default.VeloBankAcountNumbersDownloadDate,
            };
            List<VeloBankHistoryItem> operations =  veloService.GetHistory(filter).Cast<VeloBankHistoryItem>().ToList();
             SaveOperations(operations);
        }

                //TODO szyfrowane + w getinbank
        private static void SaveOperations(List<VeloBankHistoryItem> operations)
        {
            if (operations != null)
            {
                    Properties.Settings.Default.VeloBankAcountNumbersDownloadDate = DateTime.Today;

                if (Properties.Settings.Default.VeloBankAcountNumbers == null)
                    Properties.Settings.Default.VeloBankAcountNumbers = new SerializableStringDictionary();
                foreach (VeloBankHistoryItem operation in operations.Where(o => !(o.ReferenceNumber == null || o.ReferenceNumber == "-")))
                    if (!Properties.Settings.Default.VeloBankAcountNumbers.ContainsKey(operation.ReferenceNumber))
                        Properties.Settings.Default.VeloBankAcountNumbers.Add(operation.ReferenceNumber, operation.Direction == OperationDirection.Execute ? operation.ToAccountNumber : operation.FromAccountNumber);
                Properties.Settings.Default.Save();
            }
        }
    }
}
