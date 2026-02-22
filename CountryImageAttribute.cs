using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Linq;
using ToolsForms;

namespace BankService
{
    public class CountryImageAttribute : ItemImageAttribute
    {
        public string ResourceName { get; set; }
        public override Image Image => (Image)Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, true, true).Cast<DictionaryEntry>().Single(e => (string)e.Key == ResourceName).Value;

        public CountryImageAttribute(string resourceName)
        {
            ResourceName = resourceName;
        }
    }
}
