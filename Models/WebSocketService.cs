using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CoreClasses;

public class WebSocketService
{
    private ClientWebSocket? _ws;
    private CancellationTokenSource? _cts;
    private string? _token;
    private readonly string _wsUrl;

    public event EventHandler<JsonElement>? MessageReceived;
    public event EventHandler? Disconnected;
    public event EventHandler? Reconnected;

    public WebSocketService(string wsUrl)
    {
        _wsUrl = wsUrl;
    }

    public async Task ConnectAsync(string token)
    {
        _token = token;
        _cts = new CancellationTokenSource();
        await ConnectInternalAsync();
        _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
    }

    private async Task ConnectInternalAsync()
    {
        _ws = new ClientWebSocket();
        await _ws.ConnectAsync(new Uri($"{_wsUrl}/ws/{_token}"), CancellationToken.None);
        Console.WriteLine("[WebSocketService] Подключён.");
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
                    Disconnected?.Invoke(this, EventArgs.Empty);
                    await ReconnectAsync(ct);
                    break;
                }

                var data = buffer[..result.Count];
                var json = JsonSerializer.Deserialize<JsonElement>(data);
                MessageReceived?.Invoke(this, json);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebSocketService] Ошибка: {ex.Message}");
                Disconnected?.Invoke(this, EventArgs.Empty);
                await ReconnectAsync(ct);
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
                await ConnectInternalAsync();
                Reconnected?.Invoke(this, EventArgs.Empty);
                return;
            }
            catch
            {
                Console.WriteLine("[WebSocketService] Реконнект не удался, повторяем...");
            }
        }
    }

    public void Disconnect()
    {
        _cts?.Cancel();
        _ws?.Dispose();
    }
}