using CoreClasses.Protocol;
using GateWay.ViewModels;
using GateWay.Views;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoreClasses
{
    public class Templates
    {
        public MainWindow MainWindow;
        public MainWindowViewModel MainWindowViewModel;
        private readonly KeyStorage _keyStorage;
        private readonly ChatStorage _chatStorage;
        private readonly NetworkService.NetworkStack _local_service;
        private readonly CryptoService _crypto;
        private readonly string _target_host = "127.0.0.1";
        private readonly int _target_port = 15002;

        public Templates(string rootPath)
        {
            _keyStorage = new KeyStorage(rootPath);
            _chatStorage = new ChatStorage(rootPath);
            _local_service = new NetworkService.NetworkStack();
        }

        // ===================== ЗАПУСК =====================

        public async Task StartConnectionToServer()
        {
            _local_service.PacketReceived += OnPacketReceived;
            _local_service.Reconnected += (_, _) => Console.WriteLine("[Templates] Переподключились к серверу.");
            _local_service.Disconnected += (_, _) => Console.WriteLine("[Templates] Соединение потеряно.");
            await _local_service.ConnectAsync(_target_host, _target_port);
        }

        private void OnPacketReceived(object? sender, NetworkPacket packet)
        {
            try
            {
                var networkMessage = MessageFactory.Parse(packet.Data);

                switch (networkMessage.Command)
                {
                    case nameof(InternalCommand.RenderMessage):
                        var msgPayload = MessageFactory.ExtractPayload<RenderMessagePayload>(networkMessage);
                        var myPrivateKey = _keyStorage.LoadPrivateKey();
                        var senderPublicKey = Convert.FromBase64String(msgPayload.SenderId);

                        var crypto = new CryptoService();
                        var encryptedContent = Convert.FromBase64String(msgPayload.Content);
                        var decryptedBytes = crypto.Decrypt(encryptedContent, myPrivateKey, senderPublicKey);
                        var decryptedContent = System.Text.Encoding.UTF8.GetString(decryptedBytes);

                        var message = new Message
                        {
                            Id = Guid.NewGuid().ToString(),
                            SenderId = msgPayload.SenderId,
                            Content = decryptedContent,
                            SentAt = DateTimeOffset.UtcNow,
                            IsOutgoing = false
                        };

                        _chatStorage.SaveMessage(msgPayload.ChatId, message);
                        //MainWindow.AddMessageToChat(msgPayload.ChatId, message);
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


        private async Task<NetworkMessage> SendRegistraionRequest(RegistrationPayload? payload)
        {
            var message = MessageFactory.CreateExternal(ExternalCommand.Registration, payload);

            var packetReceived = new TaskCompletionSource<NetworkPacket>();
            _local_service.PacketReceived += (_, packet) => packetReceived.TrySetResult(packet);

            await _local_service.SendAsync(message);

            var completedTask = await Task.WhenAny(packetReceived.Task, Task.Delay(5000));
            if (completedTask != packetReceived.Task)
                throw new TimeoutException("Сервер не ответил за 5 секунд");

            var receivedPacket = await packetReceived.Task;
            return MessageFactory.Parse(receivedPacket.Data);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task RegistrationUser(string name)
        {
            var keyPair = _crypto.GenerateKeys();

            var payload = new RegistrationPayload
            {
                UserName = name,
                PublicKey = Convert.ToBase64String(keyPair.PublicKey)
            };
            var networkMessage = await SendRegistraionRequest(payload);

            while (networkMessage.Command == nameof(InternalCommand.RegistrationIncorrectKey))

            {
                keyPair = _crypto.GenerateKeys();

                payload = new RegistrationPayload
                {
                    UserName = name,
                    PublicKey = Convert.ToBase64String(keyPair.PublicKey)
                };


                networkMessage = await SendRegistraionRequest(payload);
            }

                switch (networkMessage.Command)
                {
                    case nameof(InternalCommand.RegistrationSuccess):
                        _keyStorage.SavePublicKey(keyPair.PublicKey);
                        _keyStorage.SavePrivateKey(keyPair.PrivateKey);
                        break;

                    case nameof(InternalCommand.RegistrationIncorrectName):
                        throw new Exception("Сервер отклонил регистрацию: имя занято или некорректно");

                    default:
                        throw new Exception($"Неожиданный ответ сервера: {networkMessage.Command}");

                }        
        }

        // ===================== ЧАТЫ =====================

        public void LoadAllChats()
        {
            foreach (var chat in _chatStorage.GetAllChats())
            {
                MainWindow.AddChatToList(
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
            var myPublicKey = _keyStorage.LoadPublicKey();
            var recipientPublicKey = _chatStorage.GetPublicKey(chatId);

            // Шифруем только контент
            
            var contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
            var encryptedContent = _crypto.Encrypt(contentBytes, myPrivateKey, recipientPublicKey);

            // Формируем сообщение для сервера

            var message_payload = new SendMessage { 
                SendedTo = _chatStorage.GetName(chatId),
                Content = encryptedContent,
                //SendedAt = DateTimeOffset.UtcNow
            };

            var message = MessageFactory.CreateExternal(ExternalCommand.SendMessage, message_payload);
            await _local_service.SendAsync(message);

            //var local_message = new RenderMessagePayload


            // Я ХЗ тут типа надо чтобы фронт проргузил у себя локально, потом или щас


            //_chatStorage.SaveMessage(chatId, message);
        }

        public byte[] GetMyPublicKey() => _keyStorage.LoadPublicKey();



        //public void DeleteChat(string chatId) => _chatStorage.DeleteChat(chatId);

        //добавить вход зареганного аользователя
        //добавить пароль

    }
}