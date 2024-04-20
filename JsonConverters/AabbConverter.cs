using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace KindredVignettes.JsonConverters
{
    public class AabbConverter : System.Text.Json.Serialization.JsonConverter<Aabb>
    {
        public override Aabb Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Aabb should be an object");
            }
            reader.Read();

            var max = float3.zero;
            var min = float3.zero;
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected property name");
                }
                var propertyName = reader.GetString();
                reader.Read();

                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw new JsonException("Expected array");
                }
                
                reader.Read();
                var x = reader.GetSingle();
                reader.Read();
                var y = reader.GetSingle();
                reader.Read();
                var z = reader.GetSingle();
                reader.Read();
                if (reader.TokenType != JsonTokenType.EndArray)
                {
                    throw new JsonException("Expected end of array");
                }
                reader.Read();

                if(propertyName == "max")
                    max = new float3(x, y, z);
                else if (propertyName == "min")
                    min = new float3(x, y, z);
                else
                {
                    throw new JsonException("Unknown property name");
                }
            }
            var result = new Aabb();
            result.Max = max;
            result.Min = min;
            return result;
        }

        public override void Write(Utf8JsonWriter writer, Aabb value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("max");
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Max.x);
            writer.WriteNumberValue(value.Max.y);
            writer.WriteNumberValue(value.Max.z);
            writer.WriteEndArray();
            writer.WritePropertyName("min");
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Min.x);
            writer.WriteNumberValue(value.Min.y);
            writer.WriteNumberValue(value.Min.z);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
