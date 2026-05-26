using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CoreClasses;

public class ChatStorage
{
    private const int PageSize = 100;
    private readonly string _root;
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ChatStorage(string root)
    {
        _root = root;
        Directory.CreateDirectory(Path.Combine(_root, "chats"));
    }

    // ===================== ЧАТЫ =====================

    public string CreateChat(string name, byte[] publicKey)
    {
        var chatId = ComputeChatId(publicKey);
        var chatPath = GetChatPath(chatId);

        Directory.CreateDirectory(chatPath);

        var infoPath = Path.Combine(chatPath, "info.txt");
        if (!File.Exists(infoPath))
        {
            File.WriteAllLines(infoPath, [
                name,
                Convert.ToBase64String(publicKey)
            ]);
        }

        return chatId;
    }

    public byte[] GetPublicKey(string chatId)
    {
        var lines = ReadInfo(chatId);
        return Convert.FromBase64String(lines[1]);
    }

    public string GetName(string chatId)
    {
        var lines = ReadInfo(chatId);
        return lines[0];
    }

    public List<ChatPreview> GetAllChats()
    {
        var chatsRoot = Path.Combine(_root, "chats");
        var result = new List<ChatPreview>();

        foreach (var chatDir in Directory.EnumerateDirectories(chatsRoot))
        {
            var chatId = Path.GetFileName(chatDir);
            var lines = ReadInfo(chatId);
            var name = lines[0];

            // Ищем последнее сообщение
            var lastPageNumber = GetLastPageNumber(chatId);
            var lastMessage = string.Empty;
            var lastSenderId = string.Empty;

            if (lastPageNumber > 0)
            {
                var page = LoadPage(chatId, lastPageNumber);
                if (page.Count > 0)
                {
                    var last = page[^1];
                    lastMessage = last.Content;
                    lastSenderId = last.SenderId;
                }
            }

            result.Add(new ChatPreview
            {
                ChatId = chatId,
                Name = name,
                LastMessage = lastMessage,
                IsLastOutgoing = true
            });
        }

        return result;
    }

    public void DeleteChat(string chatId)
    {
        var chatPath = GetChatPath(chatId);
        if (Directory.Exists(chatPath))
            Directory.Delete(chatPath, recursive: true);
    }

    // ===================== СООБЩЕНИЯ =====================

    public void SaveMessage(string chatId, Message message)
    {
        var pageNumber = GetLastPageNumber(chatId);
        if (pageNumber == 0) pageNumber = 1;

        var page = LoadPage(chatId, pageNumber);

        if (page.Count >= PageSize)
        {
            pageNumber++;
            page = [];
        }

        page.Add(message);
        WritePage(chatId, pageNumber, page);
    }

    public List<Message> LoadPage(string chatId, int pageNumber)
    {
        var path = GetPagePath(chatId, pageNumber);
        if (!File.Exists(path)) return [];

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<Message>>(json, _options) ?? [];
    }

    public int GetMaxListNumber(string chatId) => GetLastPageNumber(chatId);

    // ===================== ПРИВАТНОЕ =====================

    private void WritePage(string chatId, int pageNumber, List<Message> messages)
    {
        var path = GetPagePath(chatId, pageNumber);
        var json = JsonSerializer.Serialize(messages, _options);
        File.WriteAllText(path, json);
    }

    private int GetLastPageNumber(string chatId)
    {
        var chatPath = GetChatPath(chatId);
        var pages = Directory.EnumerateFiles(chatPath, "messages_*.json").ToList();
        if (pages.Count == 0) return 0;

        return pages
            .Select(p => int.Parse(Path.GetFileNameWithoutExtension(p).Replace("messages_", "")))
            .Max();
    }

    private string[] ReadInfo(string chatId)
        => File.ReadAllLines(Path.Combine(GetChatPath(chatId), "info.txt"));

    private string GetChatPath(string chatId)
        => Path.Combine(_root, "chats", chatId);

    private string GetPagePath(string chatId, int pageNumber)
        => Path.Combine(GetChatPath(chatId), $"messages_{pageNumber:D4}.json");

    private static string ComputeChatId(byte[] publicKey)
    {
        var hash = SHA256.HashData(publicKey);
        return Convert.ToHexString(hash).ToLower();
    }

    

    
}