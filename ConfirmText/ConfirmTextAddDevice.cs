namespace BankService.ConfirmText
{
    public class ConfirmTextAddDevice : ConfirmTextDevice
    {
        protected override string OperationName => "Dodawanie urządzenia do zarejestrowanych";

        public ConfirmTextAddDevice(string deviceName) : base(deviceName)
        {
        }
    }
}
