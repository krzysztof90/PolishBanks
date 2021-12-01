using System;

namespace BankService
{
    public class BankTypeAttribute : Attribute
    {
        public BankType BankType { get; set; }

        public BankTypeAttribute(BankType bankType)
        {
            BankType = bankType;
        }
    }
}
