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
                throw new JsonException("Vector3 should be an object");
            }
            reader.Read();

            float maxX = 0, maxY = 0, maxZ = 0, minX = 0, minY = 0, minZ = 0;
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
                    case "maxX":
                        maxX = reader.GetSingle();
                        break;
                    case "maxY":
                        maxY = reader.GetSingle();
                        break;
                    case "maxZ":
                        maxZ = reader.GetSingle();
                        break;
                    case "minX":
                        minX = reader.GetSingle();
                        break;
                    case "minY":
                        minY = reader.GetSingle();
                        break;
                    case "minZ":
                        minZ = reader.GetSingle();
                        break;
                    default:
                        throw new JsonException($"Unknown property {propertyName}");
                }
                reader.Read();
            }
            var result = new Aabb();
            result.Max = new float3(maxX, maxY, maxZ);
            result.Min = new float3(minX, minY, minZ);
            return result;
        }

        public override void Write(Utf8JsonWriter writer, Aabb value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("maxX", value.Max.x);
            writer.WriteNumber("maxY", value.Max.y);
            writer.WriteNumber("maxZ", value.Max.z);
            writer.WriteNumber("minX", value.Min.x);
            writer.WriteNumber("minY", value.Min.y);
            writer.WriteNumber("minZ", value.Min.z);
            writer.WriteEndObject();
        }
    }
}
