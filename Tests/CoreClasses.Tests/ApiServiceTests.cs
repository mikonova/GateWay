using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CoreClasses;

namespace CoreClasses.Tests;

public class ApiServiceTests
{
    private readonly ApiService _api;
    private readonly string _testNickname = "testuser";
    private readonly string _testPassword = "testpass123";
    private readonly KeyStorage _keyStorage = new KeyStorage("C:\\Users\\Family\\Desktop\\GateWayTests");
    private string? _token;

    public ApiServiceTests()
    {
        _api = new ApiService("http://192.168.43.151:8000");
    }
    
    [Fact]
    public async Task Register_ShouldReturnToken()
    {
        _token = await _api.RegisterAsync(_testNickname, _testPassword);

        Assert.NotNull(_token);
        Assert.NotEmpty(_token);
        Console.WriteLine($"Токен: {_token}");
    }
    
    // ===================== ЛОГИН =====================
    
    [Fact]
    public async Task Login_ShouldReturnToken()
    {
        // Сначала регистрируемся
        //await _api.RegisterAsync(_testNickname, _testPassword);

        // Потом логинимся
        
        _token = await _api.LoginAsync(_testNickname, _testPassword);
        _keyStorage.SaveToken(_token);

        Assert.NotNull(_token);
        Assert.NotEmpty(_token);
        Console.WriteLine($"Токен: {_token}");
    }
    
    [Fact]
    public async Task Login_WrongPassword_ShouldThrow()
    {
        await _api.RegisterAsync(_testNickname, _testPassword);

        await Assert.ThrowsAsync<Exception>(() =>
            _api.LoginAsync(_testNickname, "wrongpassword"));
    }
    
    // ===================== ПУБЛИЧНЫЙ КЛЮЧ =====================
    
    [Fact]
    public async Task UploadPublicKey_ShouldSucceed()
    {
        //_token = await _api.RegisterAsync(_testNickname, _testPassword);
        _token = _keyStorage.LoadToken();
        _api.SetToken(_token);

        var crypto = new CryptoService();
        var keyPair = crypto.GenerateKeys();
        var publicKeyBase64 = Convert.ToBase64String(keyPair.PublicKey);

        // Не должно кинуть исключение
        await _api.UploadPublicKeyAsync(publicKeyBase64);
    }
    
    [Fact]
    public async Task GetPublicKeyByNickname_ShouldReturnKey()
    {
        _token = _keyStorage.LoadToken();
        _api.SetToken(_token);

        var crypto = new CryptoService();
        var keyPair = crypto.GenerateKeys();
        var publicKeyBase64 = Convert.ToBase64String(keyPair.PublicKey);
        await _api.UploadPublicKeyAsync(publicKeyBase64);

        var key = await _api.GetPublicKeyByNicknameAsync(_testNickname);

        Assert.NotNull(key);
        Assert.Equal(publicKeyBase64, key);
        Console.WriteLine($"Ключ: {key}");
    }
    
    
    [Fact]
    public async Task GetPublicKeyByNickname_NonExistent_ShouldReturnNull()
    {
        var key = await _api.GetPublicKeyByNicknameAsync("nonexistent_user_xyz");
        Assert.Null(key);
    }

    // ===================== ЧАТЫ =====================
    
    [Fact]
    public async Task CreateChat_ShouldReturnChatId()
    {
        // Регистрируем двух юзеров
        var user2 = $"testuser_{Guid.NewGuid().ToString()[..8]}";
        await _api.RegisterAsync(user2, _testPassword);

        _token = _keyStorage.LoadToken();
        _api.SetToken(_token);

        var chatId = await _api.CreateChatAsync("Тестовый чат", false,
            new List<string> { _testNickname, user2 });

        Assert.True(chatId > 0);
        Console.WriteLine($"Chat ID: {chatId}");
    }
    
    [Fact]
    public async Task GetMyChats_ShouldReturnList()
    {
        var user2 = $"testuser_{Guid.NewGuid().ToString()[..8]}";
        await _api.RegisterAsync(user2, _testPassword);

        _token = _keyStorage.LoadToken();
        _api.SetToken(_token);

        await _api.CreateChatAsync("Тестовый чат", false,
            new List<string> { _testNickname, user2 });

        var chats = await _api.GetMyChatsAsync();

        Assert.NotEmpty(chats);
        Console.WriteLine($"Чатов: {chats.Count}");
    }
    
    [Fact]
    public async Task GetChatParticipantKeys_ShouldReturnKeys()
    {
        var user2 = $"testuser_{Guid.NewGuid().ToString()[..8]}";
        var token2 = await _api.RegisterAsync(user2, _testPassword);

        // Загружаем ключ второму юзеру
        var api2 = new ApiService("http://192.168.43.151:8000");
        api2.SetToken(token2);
        var crypto = new CryptoService();
        var keyPair2 = crypto.GenerateKeys();
        await api2.UploadPublicKeyAsync(Convert.ToBase64String(keyPair2.PublicKey));

        _token = _keyStorage.LoadToken();
        _api.SetToken(_token);

        var chatId = await _api.CreateChatAsync("Тестовый чат", false,
            new List<string> { _testNickname, user2 });

        var keys = await _api.GetChatParticipantKeysAsync(chatId);

        Assert.NotEmpty(keys);
        Console.WriteLine($"Участников: {keys.Count}");
    }

    // ===================== СООБЩЕНИЯ =====================
    
    [Fact]
    public async Task SendMessage_ShouldSucceed()
    {
        var user2 = $"testuser_{Guid.NewGuid().ToString()[..8]}";
        await _api.RegisterAsync(user2, _testPassword);

        _token = _keyStorage.LoadToken();
        _api.SetToken(_token);

        var chatId = await _api.CreateChatAsync("Тестовый чат", false,
            new List<string> { _testNickname, user2 });

        // Не должно кинуть исключение
        await _api.SendMessageAsync(chatId, "SGVsbG8gV29ybGQ="); // base64 "Hello World"
    }
    
    [Fact]
    public async Task GetMessages_ShouldReturnList()
    {
        var user2 = $"testuser_{Guid.NewGuid().ToString()[..8]}";
        await _api.RegisterAsync(user2, _testPassword);

        _token = _keyStorage.LoadToken();
        _api.SetToken(_token);

        var chatId = await _api.CreateChatAsync("Тестовый чат", false,
            new List<string> { _testNickname, user2 });

        await _api.SendMessageAsync(chatId, "SGVsbG8gV29ybGQ=");

        var messages = await _api.GetMessagesAsync(chatId);

        Assert.NotEmpty(messages);
        Console.WriteLine($"Сообщений: {messages.Count}");
    }
}