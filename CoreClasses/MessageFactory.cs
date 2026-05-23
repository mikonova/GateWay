using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreClasses.Protocol
{
    /// <summary>
    /// Хелпер для создания и разбора NetworkMessage.
    /// Работает поверх NetworkStack — принимает/отдаёт byte[].
    /// </summary>
    public static class MessageFactory
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        // ===================== СОЗДАНИЕ =====================

        /// <summary>Создать внутреннее сообщение.</summary>
        public static byte[] CreateInternal<TPayload>(
            InternalCommand command,
            TPayload payload)
        {
            var message = new NetworkMessage
            {
                Direction = MessageDirection.Internal,
                Command = command.ToString(),
                Payload = SerializeToElement(payload)
            };

            return Serialize(message);
        }

        /// <summary>Создать внешнее сообщение (операция с БД).</summary>
        public static byte[] CreateExternal<TPayload>(
            ExternalCommand command,
            TPayload payload)
        {
            var message = new NetworkMessage
            {
                Direction = MessageDirection.External,
                Command = command.ToString(),
                Payload = SerializeToElement(payload)
            };

            return Serialize(message);
        }

        // ===================== РАЗБОР =====================

        /// <summary>Десериализовать сырые байты в NetworkMessage.</summary>
        public static NetworkMessage Parse(byte[] data)
        {
            var message = JsonSerializer.Deserialize<NetworkMessage>(data, _options);

            if (message == null)
                throw new InvalidOperationException("Не удалось десериализовать NetworkMessage.");

            return message;
        }

        /// <summary>Извлечь payload конкретного типа из NetworkMessage.</summary>
        public static TPayload ExtractPayload<TPayload>(NetworkMessage message)
        {
            var payload = message.Payload.Deserialize<TPayload>(_options);

            if (payload == null)
                throw new InvalidOperationException(
                    $"Не удалось десериализовать payload как {typeof(TPayload).Name}.");

            return payload;
        }

        // ===================== ПРИВАТНОЕ =====================

        private static byte[] Serialize(NetworkMessage message)
            => JsonSerializer.SerializeToUtf8Bytes(message, _options);

        private static JsonElement SerializeToElement<T>(T obj)
        {
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(obj, _options);
            return JsonDocument.Parse(bytes).RootElement.Clone();
        }
    }
}
