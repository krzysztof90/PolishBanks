using System.Text;

namespace BankService.ConfirmText
{
    public abstract class ConfirmTextDevice : ConfirmTextBase
    {
        public string DeviceName { get; private set; }

        protected override string AdditionalText
        {
            get
            {
                StringBuilder message = new StringBuilder();
                if (DeviceName != null)
                    message.Append($" - {DeviceName}");
                return message.ToString();
            }
        }

        public ConfirmTextDevice(string deviceName) : base()
        {
            DeviceName = deviceName;
        }
    }
}
