using System.ComponentModel;

namespace BankService
{
    public class AreaCodeAttribute : DescriptionAttribute
    {
        public string Name { get; set; }
        public int AreaCode { get; set; }
        public string AreaCodeDisplay => $"+{AreaCode}";

        public AreaCodeAttribute(string name, int areaCode)
        {
            Name = name;
            AreaCode = areaCode;
        }

        public override string Description => $"{Name}: {AreaCodeDisplay}";
    }
}
