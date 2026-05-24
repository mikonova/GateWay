using CoreClasses;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreClasses.Tests
{
    public class ChatStorageTests : IDisposable
    {
        private readonly string _testRoot;
        private readonly ChatStorage _storage;

        public ChatStorageTests()
        {
            _testRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        "GateWayTests",
        Guid.NewGuid().ToString());
            _storage = new ChatStorage(_testRoot);
        }

        // ===================== ЧАТЫ =====================

        [Fact]
        public void CreateChat_ShouldCreateFolderAndInfoFile()
        {
            var publicKey = new byte[] { 1, 2, 3, 4, 5 };
            var chatId = _storage.CreateChat("Алиса", publicKey);

            var chatPath = Path.Combine(_testRoot, "chats", chatId);
            Assert.True(Directory.Exists(chatPath));
            Assert.True(File.Exists(Path.Combine(chatPath, "info.txt")));
        }

        [Fact]
        public void CreateChat_SamePublicKey_ShouldReturnSameChatId()
        {
            var publicKey = new byte[] { 1, 2, 3, 4, 5 };

            var chatId1 = _storage.CreateChat("Алиса", publicKey);
            var chatId2 = _storage.CreateChat("Алиса", publicKey);

            Assert.Equal(chatId1, chatId2);
        }

        [Fact]
        public void GetPublicKey_ShouldReturnSavedKey()
        {
            var publicKey = new byte[] { 1, 2, 3, 4, 5 };
            var chatId = _storage.CreateChat("Алиса", publicKey);

            var loaded = _storage.GetPublicKey(chatId);

            Assert.Equal(publicKey, loaded);
        }

        [Fact]
        public void GetAllChats_ShouldReturnChatPreviews()
        {
            var key1 = new byte[] { 1, 2, 3 };
            var key2 = new byte[] { 4, 5, 6 };

            var chatId1 = _storage.CreateChat("Алиса", key1);
            var chatId2 = _storage.CreateChat("Боб", key2);

            _storage.SaveMessage(chatId1, new Message
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = "alice-001",
                Content = "Привет!",
                SentAt = DateTime.UtcNow,
                IsOutgoing = false
            });

            _storage.SaveMessage(chatId2, new Message
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = "me",
                Content = "Как дела?",
                SentAt = DateTime.UtcNow,
                IsOutgoing = true
            });

            var chats = _storage.GetAllChats();

            var chat1 = chats.First(c => c.ChatId == chatId1);
            Assert.Equal("Алиса", chat1.Name);
            Assert.Equal("Привет!", chat1.LastMessage);
            Assert.True(chat1.IsLastOutgoing);

            var chat2 = chats.First(c => c.ChatId == chatId2);
            Assert.Equal("Боб", chat2.Name);
            Assert.Equal("Как дела?", chat2.LastMessage);
            Assert.True(chat2.IsLastOutgoing);
        }

        [Fact]
        public void GetAllChats_NoMessages_ShouldReturnEmptyLastMessage()
        {
            var chatId = _storage.CreateChat("Алиса", new byte[] { 1, 2, 3 });

            var chats = _storage.GetAllChats();
            var chat = chats.First(c => c.ChatId == chatId);

            Assert.Equal("Алиса", chat.Name);
            Assert.Equal(string.Empty, chat.LastMessage);
            Assert.True(chat.IsLastOutgoing);
        }

        // ===================== СООБЩЕНИЯ =====================

        [Fact]
        public void SaveMessage_ShouldCreateMessagesFile()
        {
            var chatId = _storage.CreateChat("Алиса", new byte[] { 1, 2, 3 });

            _storage.SaveMessage(chatId, new Message
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = "alice-001",
                Content = "Привет!",
                SentAt = DateTime.UtcNow,
                IsOutgoing = false
            });

            Assert.True(File.Exists(Path.Combine(_testRoot, "chats", chatId, "messages_0001.json")));
        }

        [Fact]
        public void LoadPage_ShouldReturnSavedMessages()
        {
            var chatId = _storage.CreateChat("Алиса", new byte[] { 1, 2, 3 });

            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = "alice-001",
                Content = "Привет!",
                SentAt = DateTime.UtcNow,
                IsOutgoing = false
            };

            _storage.SaveMessage(chatId, message);
            var page = _storage.LoadPage(chatId, 1);

            Assert.Single(page);
            Assert.Equal(message.Content, page[0].Content);
            Assert.Equal(message.SenderId, page[0].SenderId);
            Assert.Equal(message.IsOutgoing, page[0].IsOutgoing);
        }

        [Fact]
        public void SaveMessage_Over100Messages_ShouldCreateNewPage()
        {
            var chatId = _storage.CreateChat("Алиса", new byte[] { 1, 2, 3 });

            for (int i = 0; i < 101; i++)
            {
                _storage.SaveMessage(chatId, new Message
                {
                    Id = Guid.NewGuid().ToString(),
                    SenderId = "alice-001",
                    Content = $"Сообщение {i}",
                    SentAt = DateTime.UtcNow,
                    IsOutgoing = i % 2 == 0
                });
            }

            Assert.True(File.Exists(Path.Combine(_testRoot, "chats", chatId, "messages_0001.json")));
            Assert.True(File.Exists(Path.Combine(_testRoot, "chats", chatId, "messages_0002.json")));
        }

        [Fact]
        public void LoadPage_NonExistentPage_ShouldReturnEmptyList()
        {
            var chatId = _storage.CreateChat("Алиса", new byte[] { 1, 2, 3 });

            var page = _storage.LoadPage(chatId, 99);

            Assert.Empty(page);
        }

        // ===================== CLEANUP =====================

        public void Dispose()
        {
            //if (Directory.Exists(_testRoot))
              //  Directory.Delete(_testRoot, recursive: true);
            Console.WriteLine($"Папка тестов: {_testRoot}");
        }
    }
}
