using System;
using System.Text.Json;
using UnityEngine;

namespace KindredSchematics.JsonConverters
{
    public class Vector3Converter : System.Text.Json.Serialization.JsonConverter<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Vector3 should be an object");
            }
            reader.Read();

            var result = new Vector3();
            result.x = reader.GetSingle();
            reader.Read();
            result.y = reader.GetSingle();
            reader.Read();
            result.z = reader.GetSingle();
            reader.Read();

            return result;
        }

        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.x);
            writer.WriteNumberValue(value.y);
            writer.WriteNumberValue(value.z);
            writer.WriteEndArray();
        }
    }
}
