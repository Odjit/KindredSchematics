using System;
using System.Text.Json;
using Unity.Mathematics;

namespace KindredSchematics.JsonConverters
{
    public class int2Converter : System.Text.Json.Serialization.JsonConverter<int2>
    {
        public override int2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("int2 should be an array");
            }
            reader.Read();

            var result = new int2();
            result.x = reader.GetInt32();
            reader.Read();
            result.y = reader.GetInt32();
            reader.Read();

            return result;
        }

        public override void Write(Utf8JsonWriter writer, int2 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.x);
            writer.WriteNumberValue(value.y);
            writer.WriteEndArray();
        }
    }
}
