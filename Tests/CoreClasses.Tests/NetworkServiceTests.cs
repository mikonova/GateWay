using CoreClasses;
using CoreClasses.Protocol;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static CoreClasses.NetworkService;

namespace CoreClasses.Tests;

public class NetworkServiceTests : IDisposable
{
    // Порты для тестов — чуть выше твоих рабочих, чтоб не конфликтовали
    private const string LocalHost = "127.0.0.1";
    private const int ServerPort = 45101;
    private const int ClientPort = 45100;

    private readonly NetworkStack _server;
    private readonly NetworkStack _client;

    public NetworkServiceTests()
    {
        _server = new NetworkStack(ServerPort);
        _client = new NetworkStack(ClientPort);
    }

    public void Dispose()
    {
        _client.Dispose();
        _server.Dispose();
    }

    // ===================== ЗАПУСК / ОСТАНОВКА =====================

    [Fact]
    public void Start_ShouldNotThrow()
    {
        var ex = Record.Exception(() => _server.Start());

        Assert.Null(ex);
    }

    [Fact]
    public void Stop_ShouldNotThrow()
    {
        _server.Start();

        var ex = Record.Exception(() => _server.Stop());

        Assert.Null(ex);
    }

    [Fact]
    public void Start_WhenAlreadyStarted_ShouldThrowInvalidOperationException()
    {
        _server.Start();

        Assert.Throws<InvalidOperationException>(() => _server.Start());
    }

    // ===================== ОТПРАВКА / ПРИЁМ =====================

    [Fact]
    public async Task SendAsync_ShouldDeliverPacketToServer()
    {
        // Arrange
        var received = new TaskCompletionSource<NetworkPacket>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        _server.PacketReceived += (_, packet) => received.TrySetResult(packet);
        _server.Start();
        _client.Start();

        byte[] payload = Encoding.UTF8.GetBytes("hello network");

        // Act
        await _client.SendAsync(LocalHost, ServerPort, payload);

        // Assert — ждём пакет максимум 2 секунды
        NetworkPacket arrivedPacket = await received.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.NotNull(arrivedPacket);
        Assert.Equal(payload, arrivedPacket.Data);
    }

    [Fact]
    public async Task SendAsync_PacketSenderEndPoint_ShouldNotBeEmpty()
    {
        // Arrange
        var received = new TaskCompletionSource<NetworkPacket>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        _server.PacketReceived += (_, packet) => received.TrySetResult(packet);
        _server.Start();
        _client.Start();

        // Act
        await _client.SendAsync(LocalHost, ServerPort, Encoding.UTF8.GetBytes("ping"));

        // Assert
        NetworkPacket arrivedPacket = await received.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.NotNull(arrivedPacket.SenderEndPoint);
        Assert.NotEmpty(arrivedPacket.SenderEndPoint);
    }

    [Fact]
    public async Task SendAsync_ReceivedAt_ShouldBeCloseToNow()
    {
        // Arrange
        var received = new TaskCompletionSource<NetworkPacket>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        _server.PacketReceived += (_, packet) => received.TrySetResult(packet);
        _server.Start();
        _client.Start();

        // Act
        await _client.SendAsync(LocalHost, ServerPort, Encoding.UTF8.GetBytes("time-check"));

        // Assert — метка времени не старше 5 секунд
        NetworkPacket arrivedPacket = await received.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.True((DateTime.UtcNow - arrivedPacket.ReceivedAt).TotalSeconds < 5);
    }

    [Fact]
    public async Task SendAsync_MultiplePackets_AllShouldArrive()
    {
        // Arrange
        const int packetCount = 5;
        int receivedCount = 0;
        var allReceived = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        _server.PacketReceived += (_, _) =>
        {
            if (Interlocked.Increment(ref receivedCount) == packetCount)
                allReceived.TrySetResult(true);
        };

        _server.Start();
        _client.Start();

        // Act
        for (int i = 0; i < packetCount; i++)
            await _client.SendAsync(LocalHost, ServerPort, Encoding.UTF8.GetBytes($"packet-{i}"));

        // Assert
        bool result = await allReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(result);
        Assert.Equal(packetCount, receivedCount);
    }

    // ===================== ОЧЕРЕДЬ =====================

    [Fact]
    public async Task TryDequeuePacket_AfterSend_ShouldReturnPacket()
    {
        // Arrange
        _server.Start();
        _client.Start();

        byte[] payload = Encoding.UTF8.GetBytes("queue-test");

        // Act
        await _client.SendAsync(LocalHost, ServerPort, payload);
        await Task.Delay(300); // Даём время на обработку

        // Assert
        bool dequeued = _server.TryDequeuePacket(out NetworkPacket? packet);

        Assert.True(dequeued);
        Assert.NotNull(packet);
        Assert.Equal(payload, packet!.Data);
    }

    [Fact]
    public void TryDequeuePacket_WhenQueueEmpty_ShouldReturnFalse()
    {
        // Очередь пуста — ничего не слали
        bool dequeued = _server.TryDequeuePacket(out NetworkPacket? packet);

        Assert.False(dequeued);
        Assert.Null(packet);
    }

    // ===================== ВАЛИДАЦИЯ =====================

    [Fact]
    public async Task SendAsync_WithEmptyData_ShouldThrowArgumentException()
    {
        _client.Start();

        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.SendAsync(LocalHost, ServerPort, Array.Empty<byte>()));
    }

    [Fact]
    public async Task SendAsync_WithNullData_ShouldThrowArgumentException()
    {
        _client.Start();

        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.SendAsync(LocalHost, ServerPort, null!));
    }

    [Fact]
    public async Task SendAsync_ToUnreachablePort_ShouldThrow()
    {
        _client.Start();

        await Assert.ThrowsAsync<SocketException>(
            () => _client.SendAsync(LocalHost, 19999, Encoding.UTF8.GetBytes("unreachable")));
    }
    [Fact]
    public async Task FullFlow_EncryptSendReceiveDecrypt_ShouldReturnOriginalMessage()
    {
        var crypto = new CryptoService();
        var alice = crypto.GenerateKeys();
        var bob = crypto.GenerateKeys();

        // Исходное сообщение
        var originalMessage = "Привет, Боб! Это секретное сообщение."u8.ToArray();

        // Шифруем
        var encrypted = crypto.Encrypt(originalMessage, alice.PrivateKey, bob.PublicKey);

        // Поднимаем сервер на стороне Боба
        var bobStack = new NetworkService.NetworkStack(15000);
        NetworkPacket? receivedPacket = null;
        var packetReceived = new TaskCompletionSource<NetworkPacket>();

        bobStack.PacketReceived += (_, packet) => packetReceived.TrySetResult(packet);
        bobStack.Start();

        // Алиса отправляет
        var aliceStack = new NetworkService.NetworkStack(15001);
        await aliceStack.SendAsync("127.0.0.1", 15000, encrypted);

        // Ждём получения (таймаут 5 секунд)
        var completedTask = await Task.WhenAny(packetReceived.Task, Task.Delay(5000));
        Assert.True(completedTask == packetReceived.Task, "Пакет не получен за 5 секунд");

        receivedPacket = await packetReceived.Task;

        // Боб расшифровывает
        var decrypted = crypto.Decrypt(receivedPacket.Data, bob.PrivateKey, alice.PublicKey);

        // Сравниваем
        Assert.Equal(originalMessage, decrypted);

        // Чистим
        bobStack.Stop();
        bobStack.Dispose();
    }
    [Fact]
    public async Task FullFlow_EncryptSendReceiveDecrypt_WithMessageFactory_ShouldReturnOriginalMessage()
    {
        var crypto = new CryptoService();
        var alice = crypto.GenerateKeys();
        var bob = crypto.GenerateKeys();

        // Создаём сообщение через MessageFactory
        var payload = new RenderMessagePayload
        {
            ChatId = "chat-001",
            SenderId = "alice-001",
            SenderName = "Алиса",
            Content = "Привет, Боб! Это секретное сообщение."
        };

        var rawMessage = MessageFactory.CreateInternal(InternalCommand.RenderMessage, payload);

        // Шифруем
        var encrypted = crypto.Encrypt(rawMessage, alice.PrivateKey, bob.PublicKey);

        // Поднимаем сервер на стороне Боба
        var bobStack = new NetworkService.NetworkStack(15002);
        var packetReceived = new TaskCompletionSource<NetworkPacket>();

        bobStack.PacketReceived += (_, packet) => packetReceived.TrySetResult(packet);
        bobStack.Start();

        // Алиса отправляет
        var aliceStack = new NetworkService.NetworkStack(15003);
        await aliceStack.SendAsync("127.0.0.1", 15002, encrypted);

        // Ждём получения
        var completedTask = await Task.WhenAny(packetReceived.Task, Task.Delay(5000));
        Assert.True(completedTask == packetReceived.Task, "Пакет не получен за 5 секунд");

        var receivedPacket = await packetReceived.Task;

        // Боб расшифровывает
        var decrypted = crypto.Decrypt(receivedPacket.Data, bob.PrivateKey, alice.PublicKey);

        // Разбираем NetworkMessage
        var networkMessage = MessageFactory.Parse(decrypted);
        var receivedPayload = MessageFactory.ExtractPayload<RenderMessagePayload>(networkMessage);

        // Сравниваем
        Assert.Equal(InternalCommand.RenderMessage.ToString(), networkMessage.Command);
        Assert.Equal(payload.ChatId, receivedPayload.ChatId);
        Assert.Equal(payload.SenderId, receivedPayload.SenderId);
        Assert.Equal(payload.SenderName, receivedPayload.SenderName);
        Assert.Equal(payload.Content, receivedPayload.Content);

        // Чистим
        bobStack.Stop();
        bobStack.Dispose();
    }
}
