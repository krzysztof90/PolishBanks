using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ToolsNugetExtensionNewtonsoft.Json;
using static BankService.Bank_PL_PKO.PKOJsonResponse;

namespace BankService.Bank_PL_PKO
{
    public class PKOJsonFieldValueConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object primitiveResult = JsonExtension.GetValueFromReader(reader, out bool success);
            if (success)
                return primitiveResult;

            if (reader.TokenType == JsonToken.StartObject)
            {
                Dictionary<string, PKOJsonResponseFieldBaseBaseBase> instance = (Dictionary<string, PKOJsonResponseFieldBaseBaseBase>)serializer.Deserialize(reader, typeof(Dictionary<string, PKOJsonResponseFieldBaseBaseBase>));
                return instance;
            }

            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}
