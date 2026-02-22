using ToolsForms;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace BankService
{
    public class BankImageAttribute : ItemImageAttribute
    {
        public string ResourceName { get; set; }
        public override Image Image => (Image)Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, true, true).Cast<DictionaryEntry>().Single(e => (string)e.Key == ResourceName).Value;

        public BankImageAttribute(string resourceName)
        {
            ResourceName = resourceName;
        }
    }
}
