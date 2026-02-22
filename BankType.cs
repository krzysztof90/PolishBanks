using System;
using System.ComponentModel;

namespace BankService
{
    public enum BankType
    {
        [BankImage("pl_velo_bank")]
        [Description("Velo Bank")]
        VeloBank,
        //[BankImage("pl_getin_bank")]
        //[Description("GetinBank")]
        //GetinBank,
        [BankImage("pl_ing")]
        [Description("ING")]
        ING,
        [BankImage("pl_pocztowy")]
        [Description("Pocztowy")]
        Pocztowy,
        [BankImage("pl_nest")]
        [BankAuthorization(new Type[] { typeof(int) }, new string[] { "Awatar" })]
        [Description("Nest Bank")]
        Nest,
        [BankImage("pl_pko")]
        [Description("PKO")]
        PKO,
        [BankImage("pl_mBank")]
        [Description("mBank")]
        mBank,
        [BankImage("pl_volkswagen")]
        [Description("Volkswagen Bank")]
        Volkswagen,
        [BankImage("pt_santander")]
        [Description("Santander PT")]
        Santander,
    }
}
