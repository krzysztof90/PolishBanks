using System.Text;

namespace BankService.ConfirmText
{
    public class ConfirmTextAuthorize : ConfirmTextDevice
    {
        protected override string OperationName => "Autoryzacja urządzenia";

        public ConfirmTextAuthorize(string deviceName) : base(deviceName)
        {
        }
    }
}
