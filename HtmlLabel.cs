using System;

namespace BankService
{
    public class HtmlLabel : Attribute
    {
        public string Value { get; set; }

        public HtmlLabel(string value)
        {
            Value = value;
        }
    }
}
