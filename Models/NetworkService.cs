using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CoreClasses
{
    public class NetworkService
    {
        public class NetworkStack : IDisposable
        {
            // ===================== ПОЛЯ =====================

            private TcpClient? _client;
            private NetworkStream? _stream;
            private CancellationTokenSource? _cts;
            private Task? _receiveTask;

            private readonly ConcurrentQueue<NetworkPacket> _incomingPackets = new();
            private readonly SemaphoreSlim _sendLock = new(1, 1);

            private string _host = string.Empty;
            private int _port;
            private bool _disposed = false;

            // ===================== СОБЫТИЯ =====================

            public event EventHandler<NetworkPacket>? PacketReceived;
            public event EventHandler<Exception>? ErrorOccurred;
            public event EventHandler? Disconnected;
            public event EventHandler? Reconnected;

            // ===================== ПОДКЛЮЧЕНИЕ =====================

            public async Task ConnectAsync(string host, int port)
            {
                _host = host;
                _port = port;

                await ConnectInternalAsync();

                _cts = new CancellationTokenSource();
                _receiveTask = Task.Run(() => ReceiveLoopAsync(_cts.Token));

                Console.WriteLine($"[NetworkStack] Подключились к {host}:{port}");
            }

            private async Task ConnectInternalAsync()
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_host, _port);
                _stream = _client.GetStream();
            }

            public void Disconnect()
            {
                _cts?.Cancel();
                _client?.Close();
                Console.WriteLine("[NetworkStack] Отключились.");
            }

            // ===================== ПРИЁМ =====================

            private async Task ReceiveLoopAsync(CancellationToken ct)
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var data = await ReadPacketAsync(_stream!, ct);
                        if (data == null)
                        {
                            // Соединение оборвалось — пробуем реконнект
                            await ReconnectAsync(ct);
                            continue;
                        }

                        var packet = new NetworkPacket
                        {
                            Data = data,
                            SenderEndPoint = _host,
                            ReceivedAt = DateTime.UtcNow
                        };

                        _incomingPackets.Enqueue(packet);
                        PacketReceived?.Invoke(this, packet);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        ErrorOccurred?.Invoke(this, ex);
                        await ReconnectAsync(ct);
                    }
                }
            }

            // ===================== РЕКОННЕКТ =====================

            private async Task ReconnectAsync(CancellationToken ct)
            {
                Console.WriteLine("[NetworkStack] Соединение потеряно, реконнект...");
                Disconnected?.Invoke(this, EventArgs.Empty);

                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(3000, ct); // ждём 3 секунды перед попыткой
                        _client?.Close();
                        await ConnectInternalAsync();
                        Console.WriteLine("[NetworkStack] Переподключились.");
                        Reconnected?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                    catch
                    {
                        Console.WriteLine("[NetworkStack] Реконнект не удался, повторяем...");
                    }
                }
            }

            // ===================== ОТПРАВКА =====================

            public async Task SendAsync(byte[] data)
            {
                if (data == null || data.Length == 0)
                    throw new ArgumentException("Данные не могут быть пустыми.", nameof(data));

                if (_stream == null)
                    throw new InvalidOperationException("Нет соединения. Сначала вызови ConnectAsync.");

                // Лок чтобы не перемешивать пакеты при параллельной отправке
                await _sendLock.WaitAsync();
                try
                {
                    await WritePacketAsync(_stream, data);
                    Console.WriteLine($"[NetworkStack] Отправлено {data.Length} байт.");
                }
                finally
                {
                    _sendLock.Release();
                }
            }

            // ===================== ЧТЕНИЕ / ЗАПИСЬ =====================

            private async Task<byte[]?> ReadPacketAsync(NetworkStream stream, CancellationToken ct)
            {
                byte[] lengthBuffer = new byte[4];
                int bytesRead = await ReadExactAsync(stream, lengthBuffer, ct);
                if (bytesRead == 0) return null;

                int bodyLength = BitConverter.ToInt32(lengthBuffer, 0);

                if (bodyLength <= 0 || bodyLength > 10 * 1024 * 1024)
                    throw new InvalidOperationException($"Некорректная длина пакета: {bodyLength}");

                byte[] body = new byte[bodyLength];
                bytesRead = await ReadExactAsync(stream, body, ct);
                if (bytesRead == 0) return null;

                return body;
            }

            private static async Task<int> ReadExactAsync(NetworkStream stream, byte[] buffer, CancellationToken ct)
            {
                int totalRead = 0;
                while (totalRead < buffer.Length)
                {
                    int read = await stream.ReadAsync(
                        buffer.AsMemory(totalRead, buffer.Length - totalRead), ct);
                    if (read == 0) return 0;
                    totalRead += read;
                }
                return totalRead;
            }

            private static async Task WritePacketAsync(NetworkStream stream, byte[] data)
            {
                byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
                byte[] packet = new byte[lengthPrefix.Length + data.Length];
                Buffer.BlockCopy(lengthPrefix, 0, packet, 0, lengthPrefix.Length);
                Buffer.BlockCopy(data, 0, packet, lengthPrefix.Length, data.Length);
                await stream.WriteAsync(packet);
                await stream.FlushAsync();
            }

            // ===================== ОЧЕРЕДЬ =====================

            public bool TryDequeuePacket(out NetworkPacket? packet)
                => _incomingPackets.TryDequeue(out packet);

            // ===================== IDisposable =====================

            public void Dispose()
            {
                if (_disposed) return;
                Disconnect();
                _cts?.Dispose();
                _sendLock.Dispose();
                _disposed = true;
            }
        }
    }
}