using Microsoft.AspNetCore.SignalR;

namespace SignalRChat.Hubs
{
    public class ChatHub : Hub
    // Hub類別管理連線、群組和傳訊
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SwitchOrderStatus(int customerId, int orderId, int status)
        {
            await Console.Out.WriteLineAsync("---------------------------------------------------------------------------------------------------------------------------");
            await Console.Out.WriteLineAsync($"SwitchOrderStatus called with customerId: {customerId} orderId: {orderId} status: {status}");
            await Console.Out.WriteLineAsync("---------------------------------------------------------------------------------------------------------------------------");

            await Clients.All.SendAsync("ReceiveOrderStatus", customerId, orderId, status);
        }
    }
}