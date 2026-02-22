using System.Collections.Generic;
using System.Runtime.Serialization;
using Tools;
using Tools.Enums;

namespace BankService.Bank_PT_Santander
{
    public class SantanderJsonResponse
    {
        [DataContract]
        public class SantanderJsonResponseBase
        {
            [DataMember] public bool sucesso { get; set; }
            [DataMember] public string erro { get; set; }
        }

        [DataContract]
        public class SantanderJsonResponseHeartbeat : SantanderJsonResponseBase
        {
        }

        [DataContract]
        public class SantanderJsonResponseAccountBalancesAndTransactions : SantanderJsonResponseBase
        {
        }

        [DataContract]
        public class SantanderJsonResponseConfirmLogin : SantanderJsonResponseBase
        {
            [DataMember] public new string sucesso { get; set; }

            public SantanderJsonSuccess? SuccessValue
            {
                get { return sucesso.GetEnumByJsonValue<SantanderJsonSuccess>(); }
                set { sucesso = value.GetEnumJsonValue<SantanderJsonSuccess>(); }
            }
        }

        [DataContract]
        public class SantanderJsonResponseAutenticacaOforteFunctions : SantanderJsonResponseBase
        {
            [DataMember] public string msg { get; set; }

            public SantanderJsonResponseStatus? MessageValue
            {
                get { return msg.GetEnumByJsonValue<SantanderJsonResponseStatus>(); }
                set { msg = value.GetEnumJsonValue<SantanderJsonResponseStatus>(); }
            }
        }

        [DataContract]
        public class SantanderJsonResponseOperator
        {
            [DataMember] public string codDestFreq { get; set; }
            [DataMember] public string contaEfetiva { get; set; }
            [DataMember] public string descripcion { get; set; }
            [DataMember] public List<string> destFreq { get; set; }
            [DataMember] public int hasMontantesPredefinidos { get; set; }
            [DataMember] public string identificadorHost { get; set; }
            [DataMember] public string labelReferencia { get; set; }
            [DataMember] public string maxAmount { get; set; }
            [DataMember] public string minAmount { get; set; }
            [DataMember] public List<SantanderJsonResponseOperatorPredefinedAmount> montantesPredefinidos { get; set; }
            [DataMember] public int tipoRef { get; set; }

            public double MaxAmountAmount => DoubleOperations.Parse(maxAmount.SubstringToEx(" "), ThousandSeparator.Space, DecimalSeparator.Dot);
            public string MaxAmountCurrency => maxAmount.SubstringFromEx(" ");
            public double MinAmountAmount => DoubleOperations.Parse(minAmount.SubstringToEx(" "), ThousandSeparator.Space, DecimalSeparator.Dot);
            public string MinAmountCurrency => minAmount.SubstringFromEx(" ");
        }

        [DataContract]
        public class SantanderJsonResponseOperatorPredefinedAmount
        {
            [DataMember] public string valor { get; set; }
            [DataMember] public string valorFormatado { get; set; }

            public double Amount => DoubleOperations.Parse(valor, ThousandSeparator.Space, DecimalSeparator.Dot);
        }
    }
}
