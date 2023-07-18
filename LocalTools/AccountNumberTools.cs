using System;
using Tools;

namespace BankService.LocalTools
{
    public static class AccountNumberTools
    {
        public static string SimplifyAccountNumber(this string accountNumber)
        {
            return accountNumber.Replace(" ", "").Replace(" ", "").SubstringFromEx("PL");
        }

        public static bool CompareAccountNumbers(string accountNumber1, string accountNumber2)
        {
            if (String.IsNullOrEmpty(accountNumber1) || String.IsNullOrEmpty(accountNumber2))
                return String.IsNullOrEmpty(accountNumber1) && String.IsNullOrEmpty(accountNumber2);
            
            return accountNumber1.SimplifyAccountNumber() == accountNumber2.SimplifyAccountNumber();
        }
    }
}
