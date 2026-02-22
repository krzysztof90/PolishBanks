using System;

namespace BankService.MandatoryTransferDatas
{
    public abstract class MandatoryTransferData
    {
        public bool MandatoryCondition { get; set; }

        public abstract bool ValidateData();

        public MandatoryTransferData(bool mandatoryCondition = true)
        {
            MandatoryCondition = mandatoryCondition;
        }

        public bool Validate()
        {
            return !MandatoryCondition || ValidateData();
        }
    }
}
