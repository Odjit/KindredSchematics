using ProjectM;
using System;
using System.Text.Json;

namespace KindredVignettes.JsonConverters
{
    public class CurveReferenceConverter : System.Text.Json.Serialization.JsonConverter<CurveReference>
    {
        public override CurveReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("CurveReference should be an object");
            }

            reader.Read();

            var curveRef = new CurveReference();
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected property name");
                }

                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "CurveGuid":
                        curveRef.CurveGuid = reader.GetInt32();
                        break;
                    case "FlipY":
                        curveRef.FlipY = reader.GetBoolean();
                        break;
                    default:
                        throw new JsonException($"Unknown property {propertyName}");
                }

                reader.Read();
            }

            var prefabName = reader.GetString();

            return curveRef;
        }

        public override void Write(Utf8JsonWriter writer, CurveReference value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("CurveGuid", value.CurveGuid);
            writer.WriteBoolean("FlipY", value.FlipY);
            writer.WriteEndObject();
        }
    }
}
