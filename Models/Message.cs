using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CoreClasses;

public class Message
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("sender_id")]
    public string SenderId { get; init; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;

    [JsonPropertyName("sent_at")]
    public DateTimeOffset SentAt { get; init; }

    [JsonPropertyName("is_outgoing")]
    public bool IsOutgoing { get; init; }
}