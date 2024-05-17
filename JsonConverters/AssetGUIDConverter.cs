using ProjectM;
using Stunlock.Core;
using System;
using System.Text.Json;

namespace KindredVignettes.JsonConverters
{
    public class AssetGUIDConverter : System.Text.Json.Serialization.JsonConverter<AssetGuid>
    {
        public override AssetGuid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("AssetGuid should be an array");
            }

            reader.Read();
            var a = reader.GetInt32();
            reader.Read();
            var b = reader.GetInt32();
            reader.Read();
            var c = reader.GetInt32();
            reader.Read();
            var d = reader.GetInt32();
            reader.Read();

            AssetGuid guid = new AssetGuid();
            guid._a = a;
            guid._b = b;
            guid._c = c;
            guid._d = d;
            return guid;
        }

        public override void Write(Utf8JsonWriter writer, AssetGuid value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value._a);
            writer.WriteNumberValue(value._b);
            writer.WriteNumberValue(value._c);
            writer.WriteNumberValue(value._d);
            writer.WriteEndArray();
        }
    }
}
