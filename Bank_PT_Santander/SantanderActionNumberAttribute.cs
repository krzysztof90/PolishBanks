using System;

namespace BankService.Bank_PT_Santander
{
    public class SantanderActionNumberAttribute : Attribute
    {
        public int Number { get; protected set; }

        public SantanderActionNumberAttribute(int number)
        {
            Number = number;
        }
    }
}
