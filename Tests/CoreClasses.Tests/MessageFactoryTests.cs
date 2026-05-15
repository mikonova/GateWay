using System;
using System.Collections.Generic;
using System.Text.Json;
using CoreClasses.Protocol;
using Xunit;

namespace CoreClasses.Tests
{
    public class MessageFactoryTests
    {
        // ===================== CreateInternal =====================

        [Fact]
        public void CreateInternal_ShouldReturnNonEmptyBytes()
        {
            var payload = new RenderMessagePayload
            {
                ChatId = "chat_1",
                SenderId = "user_1",
                SenderName = "Иван",
                Content = "Привет!"
            };

            byte[] result = MessageFactory.CreateInternal(InternalCommand.RenderMessage, payload);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void CreateInternal_ShouldContainCorrectDirection()
        {
            var payload = new RenderMessagePayload { ChatId = "chat_1" };

            byte[] result = MessageFactory.CreateInternal(InternalCommand.RenderMessage, payload);
            NetworkMessage message = MessageFactory.Parse(result);

            Assert.Equal(MessageDirection.Internal, message.Direction);
        }

        [Fact]
        public void CreateInternal_ShouldContainCorrectCommand()
        {
            var payload = new RenderMessagePayload { ChatId = "chat_1" };

            byte[] result = MessageFactory.CreateInternal(InternalCommand.RenderMessage, payload);
            NetworkMessage message = MessageFactory.Parse(result);

            Assert.Equal(nameof(InternalCommand.RenderMessage), message.Command);
        }

        [Fact]
        public void CreateInternal_ShouldContainNonEmptyId()
        {
            var payload = new RenderMessagePayload { ChatId = "chat_1" };

            byte[] result = MessageFactory.CreateInternal(InternalCommand.Ping, payload);
            NetworkMessage message = MessageFactory.Parse(result);

            Assert.NotNull(message.Id);
            Assert.NotEmpty(message.Id);
        }

        [Fact]
        public void CreateInternal_TwoMessages_ShouldHaveDifferentIds()
        {
            var payload = new RenderMessagePayload { ChatId = "chat_1" };

            byte[] first = MessageFactory.CreateInternal(InternalCommand.Ping, payload);
            byte[] second = MessageFactory.CreateInternal(InternalCommand.Ping, payload);

            NetworkMessage msgFirst = MessageFactory.Parse(first);
            NetworkMessage msgSecond = MessageFactory.Parse(second);

            Assert.NotEqual(msgFirst.Id, msgSecond.Id);
        }

        [Fact]
        public void CreateInternal_Timestamp_ShouldBeCloseToNow()
        {
            var payload = new RenderMessagePayload { ChatId = "chat_1" };

            byte[] result = MessageFactory.CreateInternal(InternalCommand.Ping, payload);
            NetworkMessage message = MessageFactory.Parse(result);

            Assert.True((DateTime.UtcNow - message.Timestamp).TotalSeconds < 5);
        }

        // ===================== CreateExternal =====================

        [Fact]
        public void CreateExternal_ShouldReturnNonEmptyBytes()
        {
            var payload = new DatabasePayload
            {
                Table = "messages",
                Keys = new Dictionary<string, string> { ["chat_id"] = "chat_1" },
                Data = new Dictionary<string, string> { ["content"] = "Привет!" }
            };

            byte[] result = MessageFactory.CreateExternal(ExternalCommand.Insert, payload);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void CreateExternal_ShouldContainCorrectDirection()
        {
            var payload = new DatabasePayload { Table = "messages" };

            byte[] result = MessageFactory.CreateExternal(ExternalCommand.Insert, payload);
            NetworkMessage message = MessageFactory.Parse(result);

            Assert.Equal(MessageDirection.External, message.Direction);
        }

        [Fact]
        public void CreateExternal_ShouldContainCorrectCommand()
        {
            var payload = new DatabasePayload { Table = "messages" };

            byte[] result = MessageFactory.CreateExternal(ExternalCommand.Insert, payload);
            NetworkMessage message = MessageFactory.Parse(result);

            Assert.Equal(nameof(ExternalCommand.Insert), message.Command);
        }

        [Fact]
        public void CreateExternal_AllCommands_ShouldSerializeCorrectly()
        {
            var payload = new DatabasePayload { Table = "test" };

            foreach (ExternalCommand cmd in Enum.GetValues<ExternalCommand>())
            {
                byte[] result = MessageFactory.CreateExternal(cmd, payload);
                NetworkMessage message = MessageFactory.Parse(result);

                Assert.Equal(cmd.ToString(), message.Command);
            }
        }

        // ===================== Parse =====================

        [Fact]
        public void Parse_ValidBytes_ShouldReturnNetworkMessage()
        {
            var payload = new RenderMessagePayload { ChatId = "chat_1" };
            byte[] bytes = MessageFactory.CreateInternal(InternalCommand.RenderMessage, payload);

            NetworkMessage result = MessageFactory.Parse(bytes);

            Assert.NotNull(result);
        }

        [Fact]
        public void Parse_InvalidBytes_ShouldThrowInvalidOperationException()
        {
            byte[] garbage = "это не json"u8.ToArray();

            Assert.Throws<JsonException>(() => MessageFactory.Parse(garbage));
        }

        [Fact]
        public void Parse_EmptyBytes_ShouldThrow()
        {
            Assert.ThrowsAny<Exception>(() => MessageFactory.Parse(Array.Empty<byte>()));
        }

        // ===================== ExtractPayload — RenderMessagePayload =====================

        [Fact]
        public void ExtractPayload_RenderMessage_ShouldReturnCorrectChatId()
        {
            var original = new RenderMessagePayload { ChatId = "chat_42" };
            byte[] bytes = MessageFactory.CreateInternal(InternalCommand.RenderMessage, original);
            NetworkMessage message = MessageFactory.Parse(bytes);

            RenderMessagePayload extracted = MessageFactory.ExtractPayload<RenderMessagePayload>(message);

            Assert.Equal("chat_42", extracted.ChatId);
        }

        [Fact]
        public void ExtractPayload_RenderMessage_ShouldReturnCorrectSenderId()
        {
            var original = new RenderMessagePayload { SenderId = "user_7" };
            byte[] bytes = MessageFactory.CreateInternal(InternalCommand.RenderMessage, original);
            NetworkMessage message = MessageFactory.Parse(bytes);

            RenderMessagePayload extracted = MessageFactory.ExtractPayload<RenderMessagePayload>(message);

            Assert.Equal("user_7", extracted.SenderId);
        }

        [Fact]
        public void ExtractPayload_RenderMessage_ShouldReturnCorrectContent()
        {
            var original = new RenderMessagePayload { Content = "Привет, мир!" };
            byte[] bytes = MessageFactory.CreateInternal(InternalCommand.RenderMessage, original);
            NetworkMessage message = MessageFactory.Parse(bytes);

            RenderMessagePayload extracted = MessageFactory.ExtractPayload<RenderMessagePayload>(message);

            Assert.Equal("Привет, мир!", extracted.Content);
        }

        [Fact]
        public void ExtractPayload_RenderMessage_AllFields_ShouldRoundTrip()
        {
            var original = new RenderMessagePayload
            {
                ChatId = "chat_1",
                SenderId = "user_1",
                SenderName = "Иван",
                Content = "Тест"
            };

            byte[] bytes = MessageFactory.CreateInternal(InternalCommand.RenderMessage, original);
            NetworkMessage message = MessageFactory.Parse(bytes);
            RenderMessagePayload extracted = MessageFactory.ExtractPayload<RenderMessagePayload>(message);

            Assert.Equal(original.ChatId, extracted.ChatId);
            Assert.Equal(original.SenderId, extracted.SenderId);
            Assert.Equal(original.SenderName, extracted.SenderName);
            Assert.Equal(original.Content, extracted.Content);
        }

        // ===================== ExtractPayload — RenderUserStatusPayload =====================

        [Fact]
        public void ExtractPayload_RenderUserStatus_IsOnline_True_ShouldRoundTrip()
        {
            var original = new RenderUserStatusPayload { UserId = "user_5", IsOnline = true };
            byte[] bytes = MessageFactory.CreateInternal(InternalCommand.RenderUserStatus, original);
            NetworkMessage message = MessageFactory.Parse(bytes);

            RenderUserStatusPayload extracted =
                MessageFactory.ExtractPayload<RenderUserStatusPayload>(message);

            Assert.Equal("user_5", extracted.UserId);
            Assert.True(extracted.IsOnline);
        }

        [Fact]
        public void ExtractPayload_RenderUserStatus_IsOnline_False_ShouldRoundTrip()
        {
            var original = new RenderUserStatusPayload { UserId = "user_5", IsOnline = false };
            byte[] bytes = MessageFactory.CreateInternal(InternalCommand.RenderUserStatus, original);
            NetworkMessage message = MessageFactory.Parse(bytes);

            RenderUserStatusPayload extracted =
                MessageFactory.ExtractPayload<RenderUserStatusPayload>(message);

            Assert.False(extracted.IsOnline);
        }

        // ===================== ExtractPayload — DatabasePayload =====================

        [Fact]
        public void ExtractPayload_DatabasePayload_ShouldReturnCorrectTable()
        {
            var original = new DatabasePayload { Table = "messages" };
            byte[] bytes = MessageFactory.CreateExternal(ExternalCommand.Select, original);
            NetworkMessage message = MessageFactory.Parse(bytes);

            DatabasePayload extracted = MessageFactory.ExtractPayload<DatabasePayload>(message);

            Assert.Equal("messages", extracted.Table);
        }

        [Fact]
        public void ExtractPayload_DatabasePayload_Keys_ShouldRoundTrip()
        {
            var original = new DatabasePayload
            {
                Table = "messages",
                Keys = new Dictionary<string, string>
                {
                    ["chat_id"] = "chat_1",
                    ["user_id"] = "user_7"
                }
            };

            byte[] bytes = MessageFactory.CreateExternal(ExternalCommand.Select, original);
            NetworkMessage message = MessageFactory.Parse(bytes);
            DatabasePayload extracted = MessageFactory.ExtractPayload<DatabasePayload>(message);

            Assert.Equal(original.Keys["chat_id"], extracted.Keys["chat_id"]);
            Assert.Equal(original.Keys["user_id"], extracted.Keys["user_id"]);
        }

        [Fact]
        public void ExtractPayload_DatabasePayload_Data_ShouldRoundTrip()
        {
            var original = new DatabasePayload
            {
                Table = "messages",
                Data = new Dictionary<string, string>
                {
                    ["content"] = "Привет!",
                    ["sent_at"] = "2025-01-01T00:00:00Z"
                }
            };

            byte[] bytes = MessageFactory.CreateExternal(ExternalCommand.Insert, original);
            NetworkMessage message = MessageFactory.Parse(bytes);
            DatabasePayload extracted = MessageFactory.ExtractPayload<DatabasePayload>(message);

            Assert.Equal(original.Data["content"], extracted.Data["content"]);
            Assert.Equal(original.Data["sent_at"], extracted.Data["sent_at"]);
        }

        [Fact]
        public void ExtractPayload_DatabasePayload_EmptyKeysAndData_ShouldNotThrow()
        {
            var original = new DatabasePayload { Table = "messages" };
            byte[] bytes = MessageFactory.CreateExternal(ExternalCommand.Delete, original);
            NetworkMessage message = MessageFactory.Parse(bytes);

            var ex = Record.Exception(
                () => MessageFactory.ExtractPayload<DatabasePayload>(message));

            Assert.Null(ex);
        }
    }
}

