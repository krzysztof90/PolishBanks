using System.Runtime.Serialization;
using static BankService.Bank_PL_ING.INGJsonResponse;

namespace BankService.Bank_PL_ING
{
    //TODO to JsonResponse
    public class INGAccountsDetails
    {
        public INGAccountsDetailsData data { get; set; }
    }

    public class INGAccountsDetailsData
    {
        public INGAccountsDetailsDataAcct accts { get; set; }
        public INGJsonResponseAccountsDataInsurances insurances { get; set; }
        public string blik { get; set; }
        public INGJsonResponseAccountsDataRetirement retirement { get; set; }
        public string balvisible { get; set; }
        public string hidezeros { get; set; }
    }

    [DataContract]
    public class INGAccountsDetailsDataAcct
    {
        [DataMember] public INGJsonResponseAccountsDataTotal[] total { get; set; }
        [DataMember] public INGAccountsDetailsDataAcctCur cur { get; set; }
        [DataMember] public INGAccountsDetailsDataAcctSav sav { get; set; }
        [DataMember] public INGAccountsDetailsDataAcctLoan loan { get; set; }
        [DataMember] public INGAccountsDetailsDataAcctVat vat { get; set; }
    }

    [DataContract]
    public class INGAccountsDetailsDataAcctCur
    {
        [DataMember] public INGJsonResponseDataAcctAcct[] accts { get; set; }
        [DataMember] public INGJsonResponseAccountsDataTotal[] total { get; set; }
    }

    [DataContract]
    public class INGAccountsDetailsDataAcctSav
    {
        [DataMember] public INGJsonResponseDataAcctAcct[] accts { get; set; }
    }

    [DataContract]
    public class INGAccountsDetailsDataAcctLoan
    {
        [DataMember] public INGJsonResponseAccountsDataAcctLoanAcct accts { get; set; }
        [DataMember] public INGJsonResponseAccountsDataAcctLoanLeases leases { get; set; }
        [DataMember] public INGJsonResponseAccountsDataTotal[] total { get; set; }
    }

    [DataContract]
    public class INGAccountsDetailsDataAcctVat
    {
        [DataMember] public string[] accts { get; set; }
    }
}
