using CoreClasses.Protocol;
using GateWay.ViewModels;
using GateWay.Views;
using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CoreClasses
{
    public class Templates
    {
        public MainWindow Mainwindow;
        public MainWindowViewModel MainWindowViewModel;
        private readonly KeyStorage _keyStorage;
        private readonly ChatStorage _chatStorage;
        private readonly CryptoService _crypto;
        private readonly HttpClient _http;

        private ClientWebSocket? _ws;
        private CancellationTokenSource? _wsCts;
        private string? _token;

        private readonly string _serverUrl = "http://127.0.0.1:8000";
        private readonly string _wsUrl = "ws://127.0.0.1:8000";

        public Templates(string rootPath)
        {
            _keyStorage = new KeyStorage(rootPath);
            _chatStorage = new ChatStorage(rootPath);
            _crypto = new CryptoService();
            _http = new HttpClient();
        }

        // ===================== ЗАПУСК =====================

        public async Task StartConnectionToServer(string token)
        {
            _token = token;
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            await ConnectWebSocketAsync();
        }

        private async Task ConnectWebSocketAsync()
        {
            _wsCts = new CancellationTokenSource();
            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(new Uri($"{_wsUrl}/ws/{_token}"), _wsCts.Token);
            Console.WriteLine("[Templates] WebSocket подключён.");
            _ = Task.Run(() => ReceiveLoopAsync(_wsCts.Token));
        }

        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            var buffer = new byte[1024 * 64];

            while (!ct.IsCancellationRequested && _ws?.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _ws.ReceiveAsync(buffer, ct);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("[Templates] Сервер закрыл соединение, реконнект...");
                        await ReconnectAsync(ct);
                        break;
                    }

                    var data = buffer[..result.Count];
                    OnDataReceived(data);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Templates] WS ошибка: {ex.Message}, реконнект...");
                    await ReconnectAsync(ct);
                    break;
                }
            }
        }

        private async Task ReconnectAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(3000, ct);
                    _ws?.Dispose();
                    await ConnectWebSocketAsync();
                    Console.WriteLine("[Templates] Переподключились.");
                    return;
                }
                catch
                {
                    Console.WriteLine("[Templates] Реконнект не удался, повторяем...");
                }
            }
        }

        private void OnDataReceived(byte[] data)
        {
            try
            {
                var json = JsonSerializer.Deserialize<JsonElement>(data);
                var type = json.GetProperty("type").GetString();

                switch (type)
                {
                    case "new_message":
                        var chatId = json.GetProperty("chat_id").GetString() ?? string.Empty;
                        var senderId = json.GetProperty("sender_id").GetString() ?? string.Empty;
                        var encryptedContent = json.GetProperty("text").GetString() ?? string.Empty;
                        var sentAt = json.GetProperty("sent_at").GetString() ?? string.Empty;

                        var myPrivateKey = _keyStorage.LoadPrivateKey();
                        var senderPublicKey = _chatStorage.GetPublicKey(chatId);

                        var encryptedBytes = Convert.FromBase64String(encryptedContent);
                        var decryptedBytes = _crypto.Decrypt(encryptedBytes, myPrivateKey, senderPublicKey);
                        var decryptedContent = Encoding.UTF8.GetString(decryptedBytes);

                        var message = new Message
                        {
                            Id = Guid.NewGuid().ToString(),
                            SenderId = senderId,
                            Content = decryptedContent,
                            SentAt = DateTimeOffset.Parse(sentAt),
                            IsOutgoing = false
                        };

                        _chatStorage.SaveMessage(chatId, message);
                        Mainwindow.LoadMessage(chatId, _chatStorage.GetName(chatId),
                            decryptedContent, sentAt, false);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Templates] Ошибка обработки пакета: {ex.Message}");
            }
        }

        // ===================== ПОЛЬЗОВАТЕЛЬ =====================

        public bool IsUserRegistered() => _keyStorage.KeysExist();

        public async Task<string> RegistrationUser(string name, string password)
        {
            var keyPair = _crypto.GenerateKeys();

            // 1. Регистрация
            var regBody = JsonSerializer.Serialize(new { nickname = name, password });
            var regResponse = await _http.PostAsync($"{_serverUrl}/register",
                new StringContent(regBody, Encoding.UTF8, "application/json"));

            if (!regResponse.IsSuccessStatusCode)
                throw new Exception("Сервер отклонил регистрацию: имя занято или некорректно");

            // 2. Логин — получаем токен
            var token = await LoginUser(name, password);

            // 3. Загружаем публичный ключ
            var keyBody = JsonSerializer.Serialize(Convert.ToBase64String(keyPair.PublicKey));
            var keyResponse = await _http.PostAsync($"{_serverUrl}/upload_public_key",
                new StringContent(keyBody, Encoding.UTF8, "application/json"));

            if (!keyResponse.IsSuccessStatusCode)
                throw new Exception("Не удалось загрузить публичный ключ");

            // 4. Сохраняем ключи локально
            _keyStorage.SavePublicKey(keyPair.PublicKey);
            _keyStorage.SavePrivateKey(keyPair.PrivateKey);

            return token;
        }

        public async Task<string> LoginUser(string name, string password)
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

        // ===================== ЧАТЫ =====================

        public void LoadAllChats()
        {
            foreach (var chat in _chatStorage.GetAllChats())
            {
                Mainwindow.AddChatToList(
                    chat.ChatId,
                    chat.Name,
                    chat.LastMessage,
                    chat.IsLastOutgoing
                );
            }
        }

        public async Task SendMessage(string chatId, string content)
        {
            var myPrivateKey = _keyStorage.LoadPrivateKey();
            var recipientPublicKey = _chatStorage.GetPublicKey(chatId);

            var contentBytes = Encoding.UTF8.GetBytes(content);
            var encryptedContent = _crypto.Encrypt(contentBytes, myPrivateKey, recipientPublicKey);

            var body = JsonSerializer.Serialize(new
            {
                chat_id = chatId,
                text = Convert.ToBase64String(encryptedContent)
            });

            await _http.PostAsync($"{_serverUrl}/send_message",
                new StringContent(body, Encoding.UTF8, "application/json"));

            // Сохраняем локально
            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = Convert.ToBase64String(_keyStorage.LoadPublicKey()),
                Content = content,
                SentAt = DateTimeOffset.UtcNow,
                IsOutgoing = true
            };
            _chatStorage.SaveMessage(chatId, message);
        }

        public void LoadMessages(string chatId, int downloaded)
        {
            int pages = _chatStorage.GetMaxListNumber(chatId);
            var senderAlias = _chatStorage.GetName(chatId);
            int toLoad = pages - downloaded;
            var raw = _chatStorage.LoadPage(chatId, toLoad);

            foreach (var msg in raw)
            {
                Mainwindow.LoadMessage(chatId, senderAlias, msg.Content,
                    msg.SentAt.ToString(), msg.IsOutgoing);
            }
        }

        public byte[] GetMyPublicKey() => _keyStorage.LoadPublicKey();
    }
}