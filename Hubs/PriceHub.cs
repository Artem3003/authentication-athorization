using Microsoft.AspNetCore.SignalR;

namespace authentication_athorization.Hubs;
public class PriceHub : Hub
{
    public async Task SendPrice(string symbol, string price)
    {
        await Clients.All.SendAsync("ReceivePrice", symbol, price);
    }
}