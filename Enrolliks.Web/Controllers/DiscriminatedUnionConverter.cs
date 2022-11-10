using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Enrolliks.Web.Controllers
{
    internal class DiscriminatedUnionConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(DiscriminatedUnionModel<>);
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return (JsonConverter)typeof(DiscriminatedUnionConverter<>).MakeGenericType(typeToConvert.GenericTypeArguments[0]).GetConstructors().First().Invoke(Array.Empty<object>());
        }
    }

    internal class DiscriminatedUnionConverter<T> : JsonConverter<DiscriminatedUnionModel<T>>
        where T : notnull
    {
        private static readonly Type[] _caseTypes = typeof(T).GetNestedTypes();

        public override DiscriminatedUnionModel<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, DiscriminatedUnionModel<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString(nameof(DiscriminatedUnionModel<T>.Tag).ToPropertyName(options), value.Tag);

            writer.WritePropertyName(nameof(DiscriminatedUnionModel<T>.Value).ToPropertyName(options));

            var caseType = _caseTypes.FirstOrDefault(type => type.Name.Equals(value.Tag, StringComparison.InvariantCultureIgnoreCase)) ?? typeof(T);

            JsonSerializer.Serialize(writer, value.Value, caseType, options);

            writer.WriteEndObject();
        }
    }
}
