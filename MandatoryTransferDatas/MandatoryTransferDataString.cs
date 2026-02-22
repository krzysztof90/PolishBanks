using System;

namespace BankService.MandatoryTransferDatas
{
    public class MandatoryTransferDataString : MandatoryTransferData
    {
        public string Data { get; set; }

        public MandatoryTransferDataString(string data, bool mandatoryCondition = true) : base(mandatoryCondition)
        {
            Data = data;
        }

        public override bool ValidateData()
        {
            return !String.IsNullOrEmpty(Data);
        }
    }
}
