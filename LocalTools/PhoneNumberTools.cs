using System;

namespace BankService.LocalTools
{
    public static class PhoneNumberTools
    {
        public static string SimplifyPhoneNumber(this string phoneNumber)
        {
            return phoneNumber.Replace("-", String.Empty).Replace(" ", String.Empty);
        }
    }
}
