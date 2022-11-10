using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Enrolliks.Web.Controllers
{
    internal class ExceptionConverter : JsonConverter<Exception>
    {
        private const int _maxDepth = 16;

        public override Exception? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
        {
#if DEBUG
            Write(writer, value, options, currentDepth: 1);
#else
            writer.WriteNullValue();
#endif
        }

        private void Write(Utf8JsonWriter writer, Exception exception, JsonSerializerOptions options, int currentDepth)
        {
            writer.WriteStartObject();

            writer.WriteString("Type".ToPropertyName(options), exception.GetType().FullName);
            writer.WriteString("Message".ToPropertyName(options), exception.Message);

            if (exception.InnerException is Exception innerException && currentDepth < _maxDepth)
            {
                writer.WritePropertyName("InnerException".ToPropertyName(options));
                Write(writer, innerException, options, currentDepth + 1);
            }

            writer.WriteEndObject();
        }
    }
}
