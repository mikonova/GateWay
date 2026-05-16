using System;
using System.Collections.Generic;
using System.Text;
namespace CoreClasses;

public class ChatPreview
{
    public string ChatId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string LastMessage { get; init; } = string.Empty;
    public string LastSenderId { get; init; } = string.Empty;
}
