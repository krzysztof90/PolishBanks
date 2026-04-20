using System.Drawing;
using Tools;
using ToolsForms;

namespace BankService
{
    public class CountryImageAttribute : ItemImageAttribute
    {
        public string ResourceName { get; set; }
        public override Image Image => (Image)ResourcesOperations.FindResourceFirst(Properties.Resources.ResourceManager, e => e == ResourceName).Value;

        public CountryImageAttribute(string resourceName)
        {
            ResourceName = resourceName;
        }
    }
}
