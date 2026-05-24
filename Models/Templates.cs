using CoreClasses.Protocol;
using GateWay.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GateWay;


namespace CoreClasses
{
    
    public class Templates
    {
        private App _app = App.Current as App;
        private readonly KeyStorage _keyStorage;
        private readonly ChatStorage _chatStorage;
        private readonly NetworkService.NetworkStack _local_service;
        private readonly int _listen_port = 15002;
        private readonly IPAddress _host = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork);
        private readonly string _target_host = "127.0.0.1";

        public Templates(string rootPath)
        {
            _keyStorage = new KeyStorage(rootPath);
            _chatStorage = new ChatStorage(rootPath);
            _local_service = new NetworkService.NetworkStack(_listen_port, _host);
        }

        public bool IsUserRegistered()
        {
            return _keyStorage.KeysExist();
        }

        public void LoadAllChats()
        {
            foreach (ChatPreview Chat in _chatStorage.GetAllChats()) {
                _app.mainWindow.AddChatToList(
                    Chat.ChatId,
                    Chat.Name,
                    Chat.LastMessage,
                    Chat.IsLastOutgoing
                    );
            }
        }

        public async Task RegistraitionUser(string name)
        {
            var crypto = new CryptoService();
            var keyPair = crypto.GenerateKeys(); // ← сохраняем всю пару

            var payload = new RegistrationPayload
            {
                UserName = name,
                PublicKey = Convert.ToBase64String(keyPair.PublicKey)
            };

            var message = MessageFactory.CreateExternal(ExternalCommand.Registration, payload);

            var packetReceived = new TaskCompletionSource<NetworkPacket>();
            _local_service.PacketReceived += (_, packet) => packetReceived.TrySetResult(packet);
            _local_service.Start();

            await _local_service.SendAsync(_target_host, _listen_port, message);

            var completedTask = await Task.WhenAny(packetReceived.Task, Task.Delay(5000));
            if (completedTask != packetReceived.Task)
                throw new TimeoutException("Сервер не ответил за 5 секунд");

            var receivedPacket = await packetReceived.Task;
            var networkMessage = MessageFactory.Parse(receivedPacket.Data);

            if (networkMessage.Command != InternalCommand.RegistrationSuccess.ToString())
                throw new Exception("Сервер отклонил регистрацию");

            // Сохраняем ключи
            var keyStorage = new KeyStorage(AppDomain.CurrentDomain.BaseDirectory);
            keyStorage.SavePublicKey(keyPair.PublicKey);
            keyStorage.SavePrivateKey(keyPair.PrivateKey);

        }

        public async Task UpdateCurrentDevice(string name)
        {
            try
            {
                var payload = new UpdateIPPayload
                {
                    UserName = name,
                    ip = _host
                };

                var message = MessageFactory.CreateExternal(ExternalCommand.Login, payload);

                var packetReceived = new TaskCompletionSource<NetworkPacket>();
                _local_service.PacketReceived += (_, packet) => packetReceived.TrySetResult(packet);
                _local_service.Start();

                await _local_service.SendAsync(_target_host, _listen_port, message);

                var completedTask = await Task.WhenAny(packetReceived.Task, Task.Delay(5000));
                if (completedTask != packetReceived.Task)
                    throw new TimeoutException("Сервер не ответил за 5 секунд");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка обновления устройства: {ex.Message}", ex);
            }
        }

        public async Task SendMessage(string chatId, string content)
        {
            // Загружаем ключи
            var keyStorage = new KeyStorage(AppDomain.CurrentDomain.BaseDirectory);
            var myPrivateKey = keyStorage.LoadPrivateKey();
            var myPublicKey = keyStorage.LoadPublicKey();

            // Загружаем публичный ключ собеседника
            var chatStorage = new ChatStorage(AppDomain.CurrentDomain.BaseDirectory);
            var recipientPublicKey = chatStorage.GetPublicKey(chatId);
            //var recipientName = chatStorage.GetChatInfo(chatId).Name;

            // Формируем сообщение
            var message = new Message
            {
                Id = Convert.ToBase64String(recipientPublicKey),
                SenderId = Convert.ToBase64String(myPublicKey),
                Content = content,
                SentAt = DateTimeOffset.UtcNow,
                IsOutgoing = true
            };

            // Сериализуем и шифруем
            var crypto = new CryptoService();
            var raw = JsonSerializer.SerializeToUtf8Bytes(message);
            var encrypted = crypto.Encrypt(raw, myPrivateKey, recipientPublicKey);

            // Оборачиваем в протокол
            var payload = new RenderMessagePayload
            {
                ChatId = chatId,
                SenderId = message.SenderId,
                SenderName = string.Empty,
                Content = content
            };
            var networkMessage = MessageFactory.CreateInternal(InternalCommand.RenderMessage, payload);

            // Отправляем
            await _local_service.SendAsync(_target_host, _listen_port, encrypted);

            // Сохраняем локально
            chatStorage.SaveMessage(chatId, message);
        }

        // TODO: Регистрация пользователя
        // TODO: передать функцию для передачи сообщения И чата тебе в бэк =
        // TODO: фолдер для чата
        // TODO: для отрисовки сообщения ответ
        // TODO: удаление чата(папки по ID)
    }
}
