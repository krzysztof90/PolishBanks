using System.Drawing;
using Tools;
using ToolsForms;

namespace BankService
{
    public class BankImageAttribute : ItemImageAttribute
    {
        public string ResourceName { get; set; }
        public override Image Image => (Image)ResourcesOperations.FindResourceFirst(Properties.Resources.ResourceManager, e => e == ResourceName).Value;

        public BankImageAttribute(string resourceName)
        {
            ResourceName = resourceName;
        }
    }
}
