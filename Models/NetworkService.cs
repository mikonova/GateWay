using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace CoreClasses
{
    public class NetworkService
    {
        /// <summary>
        /// Базовый сетевой стек: слушает входящие TCP-соединения и отправляет пакеты.
        /// </summary>
        public class NetworkStack : IDisposable
        {
            // ===================== ПОЛЯ =====================

            private readonly IPAddress _listenAddress;
            private readonly int _listenPort;

            private TcpListener? _listener;
            private CancellationTokenSource? _cts;
            private Task? _listenerTask;

            // Потокобезопасная очередь входящих пакетов
            private readonly ConcurrentQueue<NetworkPacket> _incomingPackets = new();

            private bool _disposed = false;

            // ===================== СОБЫТИЯ =====================

            /// <summary>Срабатывает при получении нового пакета.</summary>
            public event EventHandler<NetworkPacket>? PacketReceived;

            /// <summary>Срабатывает при ошибке в сетевом стеке.</summary>
            public event EventHandler<Exception>? ErrorOccurred;

            // ===================== КОНСТРУКТОР =====================

            /// <summary>
            /// Инициализирует сетевой стек.
            /// </summary>
            /// <param name="listenPort">Порт для прослушивания входящих соединений.</param>
            /// <param name="listenAddress">IP-адрес для прослушивания. По умолчанию — localhost.</param>
            public NetworkStack(int listenPort, IPAddress? listenAddress = null)
            {
                _listenPort = listenPort;
                _listenAddress = listenAddress ?? IPAddress.Any;
            }

            // ===================== ЗАПУСК / ОСТАНОВКА =====================

            /// <summary>
            /// Запускает прослушивание входящих TCP-соединений.
            /// </summary>
            public void Start()
            {
                if (_listenerTask != null && !_listenerTask.IsCompleted)
                    throw new InvalidOperationException("NetworkStack уже запущен.");

                _cts = new CancellationTokenSource();
                _listener = new TcpListener(_listenAddress, _listenPort);
                _listener.Start();

                _listenerTask = Task.Run(() => AcceptLoopAsync(_cts.Token), _cts.Token);

                Console.WriteLine($"[NetworkStack] Слушаем {_listenAddress}:{_listenPort}");
            }

            /// <summary>
            /// Останавливает прослушивание.
            /// </summary>
            public void Stop()
            {
                _cts?.Cancel();
                _listener?.Stop();

                try { _listenerTask?.Wait(TimeSpan.FromSeconds(3)); }
                catch (AggregateException) { /* Ожидаемое при отмене */ }

                Console.WriteLine("[NetworkStack] Остановлен.");
            }

            // ===================== ПРИЁМ ПАКЕТОВ =====================

            /// <summary>
            /// Основной цикл принятия входящих подключений.
            /// </summary>
            private async Task AcceptLoopAsync(CancellationToken ct)
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        TcpClient client = await _listener!.AcceptTcpClientAsync(ct);
                        // Каждое соединение обрабатывается в отдельной задаче
                        _ = Task.Run(() => HandleClientAsync(client, ct), ct);
                    }
                    catch (OperationCanceledException)
                    {
                        break; // Штатная остановка
                    }
                    catch (Exception ex)
                    {
                        ErrorOccurred?.Invoke(this, ex);
                    }
                }
            }

            /// <summary>
            /// Читает пакеты от подключившегося клиента.
            /// </summary>
            private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
            {
                using (client)
                {
                    var remoteEndPoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
                    Console.WriteLine($"[NetworkStack] Подключился клиент: {remoteEndPoint}");

                    try
                    {
                        NetworkStream stream = client.GetStream();

                        while (!ct.IsCancellationRequested && client.Connected)
                        {
                            // Читаем заголовок: 4 байта = длина тела пакета
                            byte[]? rawPacket = await ReadPacketAsync(stream, ct);
                            if (rawPacket == null) break; // Соединение закрыто

                            var packet = new NetworkPacket
                            {
                                Data = rawPacket,
                                SenderEndPoint = remoteEndPoint,
                                ReceivedAt = DateTime.UtcNow
                            };

                            _incomingPackets.Enqueue(packet);
                            PacketReceived?.Invoke(this, packet);
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        ErrorOccurred?.Invoke(this, ex);
                    }
                    finally
                    {
                        Console.WriteLine($"[NetworkStack] Клиент отключился: {remoteEndPoint}");
                    }
                }
            }

            /// <summary>
            /// Читает один пакет из потока (заголовок длины + тело).
            /// </summary>
            /// <returns>Байты тела пакета или null если соединение закрыто.</returns>
            private async Task<byte[]?> ReadPacketAsync(NetworkStream stream, CancellationToken ct)
            {
                // -- Читаем 4 байта заголовка (длина тела) --
                byte[] lengthBuffer = new byte[4];
                int bytesRead = await ReadExactAsync(stream, lengthBuffer, ct);
                if (bytesRead == 0) return null; // EOF

                int bodyLength = BitConverter.ToInt32(lengthBuffer, 0);

                if (bodyLength <= 0 || bodyLength > 10 * 1024 * 1024) // Защита: макс 10 МБ
                    throw new InvalidOperationException($"Некорректная длина пакета: {bodyLength}");

                // -- Читаем тело --
                byte[] body = new byte[bodyLength];
                bytesRead = await ReadExactAsync(stream, body, ct);
                if (bytesRead == 0) return null;

                return body;
            }

            /// <summary>
            /// Гарантированно читает ровно buffer.Length байт из потока.
            /// </summary>
            private static async Task<int> ReadExactAsync(NetworkStream stream, byte[] buffer, CancellationToken ct)
            {
                int totalRead = 0;

                while (totalRead < buffer.Length)
                {
                    int read = await stream.ReadAsync(
                        buffer.AsMemory(totalRead, buffer.Length - totalRead), ct);

                    if (read == 0) return 0; // Соединение закрыто

                    totalRead += read;
                }

                return totalRead;
            }

            // ===================== ОТПРАВКА ПАКЕТОВ =====================

            /// <summary>
            /// Отправляет пакет на указанный хост и порт.
            /// </summary>
            /// <param name="targetHost">Хост получателя (например, "127.0.0.1").</param>
            /// <param name="targetPort">Порт получателя.</param>
            /// <param name="data">Байты данных для отправки.</param>
            public async Task SendAsync(string targetHost, int targetPort, byte[] data)
            {
                if (data == null || data.Length == 0)
                    throw new ArgumentException("Данные для отправки не могут быть пустыми.", nameof(data));

                using var client = new TcpClient();
                await client.ConnectAsync(targetHost, targetPort);

                NetworkStream stream = client.GetStream();
                await WritePacketAsync(stream, data);

                Console.WriteLine($"[NetworkStack] Отправлено {data.Length} байт на {targetHost}:{targetPort}");
            }

            /// <summary>
            /// Записывает пакет в поток (заголовок длины + тело).
            /// </summary>
            private static async Task WritePacketAsync(NetworkStream stream, byte[] data)
            {
                // Заголовок: длина тела (4 байта)
                byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

                // Пишем заголовок + тело одним буфером — меньше системных вызовов
                byte[] packet = new byte[lengthPrefix.Length + data.Length];
                Buffer.BlockCopy(lengthPrefix, 0, packet, 0, lengthPrefix.Length);
                Buffer.BlockCopy(data, 0, packet, lengthPrefix.Length, data.Length);

                await stream.WriteAsync(packet);
                await stream.FlushAsync();
            }

            // ===================== ОЧЕРЕДЬ ВХОДЯЩИХ ПАКЕТОВ =====================

            /// <summary>
            /// Пытается забрать пакет из очереди входящих. Альтернатива событию PacketReceived.
            /// </summary>
            public bool TryDequeuePacket(out NetworkPacket? packet)
                => _incomingPackets.TryDequeue(out packet);

            // ===================== IDisposable =====================

            public void Dispose()
            {
                if (_disposed) return;
                Stop();
                _cts?.Dispose();
                _disposed = true;
            }
        }
    }
}
