using System;
using System.Collections.Generic;
using System.Linq;

namespace BankService
{
    public class BankAuthorizationAttribute : Attribute
    {
        public List<(Type type, string name)> AdditionalAuthorization { get; }

        public BankAuthorizationAttribute(Type[] types, string[] names)
        {
            AdditionalAuthorization = Enumerable.Range(0, types.Length).Select(i => (types[i], names[i])).ToList();
        }
    }
}
