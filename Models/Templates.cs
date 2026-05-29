using GateWay.ViewModels;
using GateWay.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        private readonly ApiService _api;
        private readonly WebSocketService _ws;
        private string user_name;

        private readonly string _serverUrl = "http://192.168.43.151:8000";
        private readonly string _wsUrl = "ws://192.168.43.151:8000";

        public Templates(string rootPath)
        {
            _keyStorage = new KeyStorage(rootPath);
            _chatStorage = new ChatStorage(rootPath);
            _crypto = new CryptoService();
            _api = new ApiService(_serverUrl);
            _ws = new WebSocketService(_wsUrl);
            _ws.MessageReceived += OnMessageReceived;
        }

        // ===================== ЗАПУСК =====================

        public async Task StartConnectionToServer(string token)
        {
            _api.SetToken(token);
            await _ws.ConnectAsync(token);
        }

        private async void OnMessageReceived(object? sender, JsonElement json)
        {
            try
            {
                var type = json.GetProperty("type").GetString();
                switch (type)
                {
                    case "new_message":
                        var chatId = json.GetProperty("chat_id").GetInt32().ToString();
                        var senderId = json.GetProperty("sender_id").GetInt32().ToString();
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
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            Mainwindow.LoadMessage(chatId, _chatStorage.GetName(chatId),
                                decryptedContent, sentAt, false);
                        });
                        break;

                    case "new_chat":
                        var newChatId = json.GetProperty("chat_id").GetInt32().ToString();
                        var creatorName = json.GetProperty("creator_name").GetString() ?? string.Empty;
                        var creatorKey = json.GetProperty("creator_key").GetString() ?? string.Empty;

                        var creatorKeyBytes = Convert.FromBase64String(creatorKey);
                        _chatStorage.CreateChat(creatorName, creatorKeyBytes, newChatId);

                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Mainwindow.AddChatToList(newChatId, creatorName, "Начните общение!", false);
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Templates] Ошибка обработки сообщения: {ex.Message}");
            }
        }

        // ===================== ПОЛЬЗОВАТЕЛЬ =====================

        public bool IsUserRegistered() => _keyStorage.KeysExist();

        public async Task<string> RegistrationUser(string name, string password)
        {
            var keyPair = _crypto.GenerateKeys();
            var token = await _api.RegisterAsync(name, password);

            _api.SetToken(token);
            await _api.UploadPublicKeyAsync(Convert.ToBase64String(keyPair.PublicKey));

            _keyStorage.SavePublicKey(keyPair.PublicKey);
            _keyStorage.SavePrivateKey(keyPair.PrivateKey);
            _keyStorage.SaveToken(token);

            return token;
        }

        public async Task<string> LoginUser(string name, string password)
        {
            var token = await _api.LoginAsync(name, password);
            _api.SetToken(token);
            _keyStorage.SaveToken(token);
            user_name = name;
            await this.SyncAllChats();
            return token;
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

            await _api.SendMessageAsync(int.Parse(chatId),
                Convert.ToBase64String(encryptedContent));

            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = user_name,
                Content = content,
                SentAt = DateTimeOffset.UtcNow,
                IsOutgoing = true
            };
            _chatStorage.SaveMessage(chatId, message);

            var msg = await Mainwindow.LoadMessage(chatId, user_name, content, DateTime.UtcNow.ToString(), true);
            Mainwindow.MessageReclipToBottom(msg);
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

        public byte[]? GetMyPublicKey()
        {
            if (!_keyStorage.KeysExist()) return null;
            return _keyStorage.LoadPublicKey();
        }

        public void DeleteChat(string chatId) => _chatStorage.DeleteChat(chatId);
        public async Task<string> CreateChat(string name, byte[] key) {
            var chat_id = await _api.CreateChatAsync($"chat with {name}", false,
            new List<string> { name, user_name });

            _chatStorage.CreateChat(name, key, chat_id.ToString());

            return chat_id.ToString();
        }

        public async Task SyncMessages(string chatId)
        {
            // Узнаём дату последнего локального сообщения
            var lastPage = _chatStorage.GetMaxListNumber(chatId);
            DateTimeOffset lastLocalTime = DateTimeOffset.MinValue;

            if (lastPage > 0)
            {
                var localMessages = _chatStorage.LoadPage(chatId, lastPage);
                if (localMessages.Count > 0)
                    lastLocalTime = localMessages.Last().SentAt;
            }

            // Подгружаем с сервера
            var serverMessages = await _api.GetMessagesAsync(int.Parse(chatId));
            var myPrivateKey = _keyStorage.LoadPrivateKey();
            var senderPublicKey = _chatStorage.GetPublicKey(chatId);

            // Берём только новее последнего локального, разворачиваем (сервер отдаёт DESC)
            var newMessages = serverMessages
            .Where(m => {
                var sender = m.GetProperty("sender").GetString();
                var sentAt = DateTimeOffset.Parse(m.GetProperty("sent_at").GetString()!);
                return sender != user_name && sentAt > lastLocalTime; // только входящие новые
            })
            .Reverse()
            .ToList();

            foreach (var m in newMessages)
            {
                var sender = m.GetProperty("sender").GetString() ?? string.Empty;
                var encryptedText = m.GetProperty("text").GetString() ?? string.Empty;
                var sentAt = m.GetProperty("sent_at").GetString() ?? string.Empty;
                var isOutgoing = sender == user_name;

                string content;
                try
                {
                    var encryptedBytes = Convert.FromBase64String(encryptedText);
                    // Если исходящее — расшифровываем своим ключом и ключом получателя
                    // Если входящее — расшифровываем своим ключом и ключом отправителя
                    var decryptedBytes = _crypto.Decrypt(encryptedBytes, myPrivateKey, senderPublicKey);
                    content = Encoding.UTF8.GetString(decryptedBytes);
                }
                catch
                {
                    content = "[не удалось расшифровать]";
                }

                var message = new Message
                {
                    Id = Guid.NewGuid().ToString(),
                    SenderId = sender,
                    Content = content,
                    SentAt = DateTimeOffset.Parse(sentAt),
                    IsOutgoing = isOutgoing
                };

                _chatStorage.SaveMessage(chatId, message);
            }
        }
        public async Task SyncAllChats()
        {
            var serverChats = await _api.GetMyChatsAsync();
            foreach (var chat in serverChats)
            {
                var chatId = chat.GetProperty("id").GetInt32().ToString();
                if (Directory.Exists(Path.Combine(/* rootPath */, "chats", chatId)))
                    await SyncMessages(chatId);
            }
        }

    }
}