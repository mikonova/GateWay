using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
namespace CoreClasses.Protocol
{
    // ===================== ТИПЫ =====================

    /// <summary>Направление сообщения.</summary>
    public enum MessageDirection
    {
        Internal,   // Внутреннее — UI, логика приложения
        External    // Внешнее — операции с БД
    }

    /// <summary>Внутренние команды (что делать UI/логике).</summary>
    public enum InternalCommand
    {
        RenderMessage,      // Отрисовать сообщение в чате
        RenderChatList,     // Обновить список чатов
        RenderUserStatus,   // Обновить статус пользователя
        RegistrationSuccess,
        RegistrationIncorrectKey,
        RegistrationIncorrectName,
        Ping,
        Pong
    }

    /// <summary>Внешние команды (операции с БД).</summary>
    public enum ExternalCommand
    {
        Insert,     // Вставить запись
        Select,     // Получить запись
        Update,     // Обновить запись
        Delete,      // Удалить запись
        Registration,
        SendMessage,
    }

    // ===================== КОНВЕРТ =====================

    /// <summary>
    /// Общая обёртка для любого сообщения в протоколе.
    /// Именно это сериализуется и летит по TCP.
    /// </summary>
    public class NetworkMessage
    {
        /// <summary>Уникальный ID сообщения — для трекинга и ответов.</summary>
        [JsonPropertyName("id")]
        public string Id { get; init; } = Guid.NewGuid().ToString();

        /// <summary>Направление: Internal или External.</summary>
        [JsonPropertyName("direction")]
        public MessageDirection Direction { get; init; }

        /// <summary>Команда в виде строки (InternalCommand или ExternalCommand).</summary>
        [JsonPropertyName("command")]
        public string Command { get; init; } = string.Empty;

        /// <summary>Тело сообщения — сырой JSON конкретного payload.</summary>
        [JsonPropertyName("payload")]
        public JsonElement Payload { get; init; }

        /// <summary>Временная метка отправки (UTC).</summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }

    // ===================== ВНУТРЕННИЕ PAYLOAD-Ы =====================

    /// <summary>Payload для RenderMessage — отрисовать сообщение в чате.</summary>
    public class RenderMessagePayload
    {
        [JsonPropertyName("chat_id")]
        public string ChatId { get; init; } = string.Empty;

        [JsonPropertyName("sender_id")]
        public string SenderId { get; init; } = string.Empty;

        [JsonPropertyName("sender_name")]
        public string SenderName { get; init; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; init; } = string.Empty;
    }

    /// <summary>Payload для RenderUserStatus — обновить статус пользователя.</summary>
    public class RenderUserStatusPayload
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; init; } = string.Empty;

        [JsonPropertyName("is_online")]
        public bool IsOnline { get; init; }
    }

    // ===================== ВНЕШНИЕ PAYLOAD-Ы =====================

    /// <summary>Payload для операций с БД.</summary>
    public class DatabasePayload
    {
        /// <summary>Таблица БД с которой работаем.</summary>
        [JsonPropertyName("table")]
        public string Table { get; init; } = string.Empty;

        /// <summary>Аргументы для определения строки (WHERE условия).</summary>
        [JsonPropertyName("keys")]
        public Dictionary<string, string> Keys { get; init; } = new();

        /// <summary>Данные для записи (поля и значения).</summary>
        [JsonPropertyName("data")]
        public Dictionary<string, string> Data { get; init; } = new();
    }

    public class RegistrationPayload {
        [JsonPropertyName("user_name")]
        public string UserName { get; init; } = string.Empty;

        [JsonPropertyName("publi_key")]
        public string PublicKey { get; init; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; init; } = string.Empty;
    }

    public class SendMessage
    {
        [JsonPropertyName("sended_to")]
        public string SendedTo { get; init; } = string.Empty;

        [JsonPropertyName("content")]
        public byte[] Content { get; set; }
    }
}


