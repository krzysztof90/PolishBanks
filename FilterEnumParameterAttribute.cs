using System;

namespace BankService
{
    public class FilterEnumParameterAttribute : Attribute
    {
        public string Parameter { get; set; }

        public FilterEnumParameterAttribute(string parameter)
        {
            Parameter = parameter;
        }
    }
}
