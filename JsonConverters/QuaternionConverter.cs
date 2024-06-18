using System;
using System.Text.Json;
using UnityEngine;

namespace KindredSchematics.JsonConverters
{
    public class QuaternionConverter : System.Text.Json.Serialization.JsonConverter<Quaternion>
    {
        public override Quaternion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Quaternion should be an object");
            }

            var euler = new Vector3();
            reader.Read();
            euler.x = reader.GetSingle();
            reader.Read();
            euler.y = reader.GetSingle();
            reader.Read();
            euler.z = reader.GetSingle();
            reader.Read();

            return Quaternion.Euler(euler);
        }

        public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options)
        {
            var euler = value.ToEuler();
            euler *= Mathf.Rad2Deg;
            writer.WriteStartArray();
            writer.WriteNumberValue(Mathf.Abs(euler.x) <= float.Epsilon ? 0f : euler.x);
            writer.WriteNumberValue(Mathf.Abs(euler.y) <= float.Epsilon ? 0f : euler.y);
            writer.WriteNumberValue(Mathf.Abs(euler.z) <= float.Epsilon ? 0f : euler.z);
            writer.WriteEndArray();
        }
    }
}
