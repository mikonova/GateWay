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
using GateWay.ViewModels;


namespace CoreClasses
{
    
    public class Templates
    {
        private readonly MainWindow _mainWindow;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly KeyStorage _keyStorage;
        private readonly ChatStorage _chatStorage;
        private readonly NetworkService.NetworkStack _local_service;
        private readonly CryptoService _crypto;
        private readonly int _listen_port = 15002;
        private readonly string _host = "127.0.0.1";
        private readonly string _target_host = "127.0.0.1";

        public Templates(string rootPath, MainWindow window, MainWindowViewModel viewModel)
        {
            _keyStorage = new KeyStorage(rootPath);
            _chatStorage = new ChatStorage(rootPath);
            _local_service = new NetworkService.NetworkStack(_listen_port);
            _crypto = new CryptoService();
            _mainWindow = window;
            _mainWindowViewModel = viewModel;
        }

        public bool IsUserRegistered()
        {
            return _keyStorage.KeysExist();
        }

        public void LoadAllChats()
        {
            foreach (ChatPreview Chat in _chatStorage.GetAllChats()) {
                _mainWindow.AddChatToList(
                    Chat.ChatId,
                    Chat.Name,
                    Chat.LastMessage,
                    Chat.IsLastOutgoing
                    );
            }
        }

        public async Task RegistraitionUser(string name)
        {
            
            var keyPair = _crypto.GenerateKeys(); // ← сохраняем всю пару

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

        public async Task UpdateCurrentDevice(string name, bool disconnect)
        {
            try
            {
                var payload = new UpdateIPPayload
                {
                    UserName = name,
                    ip = _host
                };

                var command = disconnect ? ExternalCommand.Logout : ExternalCommand.Login;
                var message = MessageFactory.CreateExternal(command, payload);

                if (disconnect)
                {
                    await _local_service.SendAsync(_target_host, _listen_port, message);
                    return;
                }

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

            var contentBytes = Encoding.UTF8.GetBytes(content);
            var encryptedContent = _crypto.Encrypt(contentBytes, myPrivateKey, recipientPublicKey);

            // Формируем сообщение
            var message = new Message
            {
                Id = Convert.ToBase64String(recipientPublicKey),
                SenderId = Convert.ToBase64String(myPublicKey),
                Content = Convert.ToBase64String(encryptedContent),
                SentAt = DateTimeOffset.UtcNow,
                IsOutgoing = true
            };

            // Сериализуем и шифруем
            
            var raw = JsonSerializer.SerializeToUtf8Bytes(message);
            var encrypted = _crypto.Encrypt(raw, myPrivateKey, recipientPublicKey);

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
        public void StartListening()
        {
            _local_service.PacketReceived += OnPacketReceived;
            _local_service.Start();
        }

        private void OnPacketReceived(object? sender, NetworkPacket packet)
        {
            var message = MessageFactory.Parse(packet.Data);

            switch (message.Command)
            {
                case nameof(InternalCommand.RenderMessage):
                    var payload = MessageFactory.ExtractPayload<RenderMessagePayload>(message);
                    // обновляем UI
                    break;

                case nameof(InternalCommand.RenderUserStatus):
                    // обновляем статус
                    break;
            }
        }



        // TODO: Регистрация пользователя
        // TODO: передать функцию для передачи сообщения И чата тебе в бэк =
        // TODO: фолдер для чата
        // TODO: для отрисовки сообщения ответ
        // TODO: удаление чата(папки по ID)
    }
}
