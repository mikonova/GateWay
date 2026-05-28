using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace CoreClasses;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly string _serverUrl;

    public ApiService(string serverUrl)
    {
        _serverUrl = serverUrl;
        _http = new HttpClient();
    }

    public void SetToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    // ===================== АВТОРИЗАЦИЯ =====================

    public async Task<string> RegisterAsync(string name, string password)
    {
        var body = JsonSerializer.Serialize(new { nickname = name, password });
        var response = await _http.PostAsync($"{_serverUrl}/register",
            new StringContent(body, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
            throw new Exception("Сервер отклонил регистрацию: имя занято или некорректно");

        return await LoginAsync(name, password);
    }

    public async Task<string> LoginAsync(string name, string password)
    {
        var body = JsonSerializer.Serialize(new { nickname = name, password });
        var response = await _http.PostAsync($"{_serverUrl}/login",
            new StringContent(body, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
            throw new Exception("Неверный ник или пароль");

        var json = JsonSerializer.Deserialize<JsonElement>(
            await response.Content.ReadAsStringAsync());

        return json.GetProperty("access_token").GetString()
               ?? throw new Exception("Токен не получен");
    }

    public async Task UploadPublicKeyAsync(string publicKeyBase64)
    {
        var response = await _http.PostAsync($"{_serverUrl}/upload_public_key",
            new StringContent($"\"{publicKeyBase64}\"", Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
            throw new Exception("Не удалось загрузить публичный ключ");
    }

    // ===================== ЧАТЫ =====================

    public async Task<int> CreateChatAsync(string name, bool isGroup, List<string> participants)
    {
        var body = JsonSerializer.Serialize(new
        {
            name,
            is_group = isGroup,
            participants_names = participants
        });
        var response = await _http.PostAsync($"{_serverUrl}/chats",
            new StringContent(body, Encoding.UTF8, "application/json"));

        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Статус: {response.StatusCode}, Ответ: {content}");

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Не удалось создать чат: {content}");
        // ...

        var json = JsonSerializer.Deserialize<JsonElement>(
            await response.Content.ReadAsStringAsync());

        return json.GetProperty("chat_id").GetInt32();
    }

    public async Task<List<JsonElement>> GetMyChatsAsync()
    {
        var response = await _http.GetAsync($"{_serverUrl}/chats/me");
        if (!response.IsSuccessStatusCode)
            throw new Exception("Не удалось получить чаты");

        var json = JsonSerializer.Deserialize<JsonElement>(
            await response.Content.ReadAsStringAsync());

        return json.GetProperty("chats").EnumerateArray().ToList();
    }

    public async Task<string?> GetPublicKeyByNicknameAsync(string nickname)
    {
        var response = await _http.GetAsync($"{_serverUrl}/users/{nickname}/public-key");
        if (!response.IsSuccessStatusCode) return null;

        var json = JsonSerializer.Deserialize<JsonElement>(
            await response.Content.ReadAsStringAsync());

        return json.GetProperty("public_key").GetString();
    }

    public async Task<List<JsonElement>> GetChatParticipantKeysAsync(int chatId)
    {
        var response = await _http.GetAsync($"{_serverUrl}/chats/{chatId}/participants/keys");
        if (!response.IsSuccessStatusCode)
            throw new Exception("Не удалось получить ключи участников");

        var json = JsonSerializer.Deserialize<JsonElement>(
            await response.Content.ReadAsStringAsync());

        return json.GetProperty("participants").EnumerateArray().ToList();
    }

    // ===================== СООБЩЕНИЯ =====================

    public async Task SendMessageAsync(int chatId, string encryptedTextBase64)
    {
        var body = JsonSerializer.Serialize(new
        {
            chat_id = chatId,
            text = encryptedTextBase64
        });
        var response = await _http.PostAsync($"{_serverUrl}/send_message",
            new StringContent(body, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
            throw new Exception("Не удалось отправить сообщение");
    }

    public async Task<List<JsonElement>> GetMessagesAsync(int chatId, int limit = 50)
    {
        var response = await _http.GetAsync($"{_serverUrl}/messages/{chatId}?limit={limit}");
        if (!response.IsSuccessStatusCode)
            throw new Exception("Не удалось получить сообщения");

        var json = JsonSerializer.Deserialize<JsonElement>(
            await response.Content.ReadAsStringAsync());

        return json.GetProperty("messages").EnumerateArray().ToList();
    }
}
