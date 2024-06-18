using System;
using System.Text.Json;
using UnityEngine;

namespace KindredSchematics.JsonConverters
{
    public class Vector2Converter : System.Text.Json.Serialization.JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Vector2 should be an array");
            }
            reader.Read();

            var result = new Vector2();
            result.x = reader.GetSingle();
            reader.Read();
            result.y = reader.GetSingle();
            reader.Read();

            return result;
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.x);
            writer.WriteNumberValue(value.y);
            writer.WriteEndArray();
        }
    }
}
