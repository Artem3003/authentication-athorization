using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using authentication_athorization.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace authentication_athorization.Controllers;

[Route("[controller]")]
public class CryptoController : Controller
{
    private readonly IHubContext<PriceHub> hubContext;
    private static CancellationTokenSource cts;
    private readonly ILogger<CryptoController> logger;

    public CryptoController(IHubContext<PriceHub> hubContext, ILogger<CryptoController> logger)
    {
        this.hubContext = hubContext;
        this.logger = logger;
    }

    [HttpGet("StartTracking")]
    public IActionResult StartTracking()
    {
        if (cts != null)
        {
            logger.LogInformation("Tracking is already in progress.");
            return Ok("Tracking is already in progress.");
        }

        cts = new CancellationTokenSource();

        _ = Task.Run(() => ConnectToBinanceWebSocket(cts.Token));

        logger.LogInformation("Tracking started.");
        return Ok("Tracking started.");
    }

    [HttpGet("StopTracking")]
    public IActionResult StopTracking()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }

        logger.LogInformation("Tracking stopped.");
        return Ok("Tracking stopped.");
    }

    private async Task ConnectToBinanceWebSocket(CancellationToken cancellationToken)
    {
        var clientWebSocket = new ClientWebSocket();

        // Binance stream URLs for BTCUSDT, ETHUSDT and SOLUSDT
        var streams = new [] { "btcusdt@ticker", "ethusdt@ticker", "solusdt@ticker" };
        var streamPath = string.Join("/", streams);
        var url = $"wss://stream.binance.com:9443/stream?streams={streamPath}";
        
        await clientWebSocket.ConnectAsync(new Uri(url), cancellationToken);

        var buffer = new ArraySegment<byte>(new byte[8192]);

        try
        {
            while (IsClientConnected(clientWebSocket))
            {
                var result = await clientWebSocket.ReceiveAsync(buffer, cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                    break;
                }

                var messageBytes = buffer.Array.AsSpan(0, result.Count).ToArray();
                var messageString = Encoding.UTF8.GetString(messageBytes);


                // Deserialize and extract price
                dynamic messageObj = System.Text.Json.JsonDocument.Parse(messageString);
                var data = messageObj.RootElement.GetProperty("data");

                string streamType = messageObj.RootElement.GetProperty("stream").GetString();

                string symbol = data.GetProperty("s").GetString();
                string priceStr = data.GetProperty("c").GetString();


                // Send the price to clients via SignalR
                await hubContext.Clients.All.SendAsync("ReceivePrice", symbol, priceStr);
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions
            Console.WriteLine($"WebSocket error: {ex.Message}");
        }
        finally
        {
            if (clientWebSocket.State == WebSocketState.Open)
            {
                await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            clientWebSocket.Dispose();
        }
    }

    private bool IsClientConnected(ClientWebSocket webSocket)
    {
        return webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived;
    }
}
