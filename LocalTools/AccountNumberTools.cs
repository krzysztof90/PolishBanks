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
            return accountNumber1.SimplifyAccountNumber() == accountNumber2.SimplifyAccountNumber();
        }
    }
}
