using Microsoft.AspNetCore.SignalR;

using StackExchange.Redis;
using Microsoft.AspNetCore.SignalR.StackExchangeRedis;
namespace RealTimeChatApp.API.Hubs
{
public class ChatHub : Hub
{
    private readonly IDatabase _redisDb;

        public ChatHub(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }

        public async Task SendMessage(string groupId, string senderName, string message, string fileUrl = null)
        {
            // Optional: Store message in Redis
            var fullMessage = $"{DateTime.Now:HH:mm:ss}|{senderName}: {message}";

            await _redisDb.ListLeftPushAsync($"chat:{groupId}:messages", fullMessage);
            await _redisDb.ListTrimAsync($"chat:{groupId}:messages", 0, 19);

            // Send to group via SignalR
            await Clients.Group(groupId).SendAsync("ReceiveMessage", senderName, message, fileUrl);
        }


    public override async Task OnConnectedAsync()
    {
        // You can log or use user context here if needed
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGroup(string groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
    }

    public async Task LeaveGroup(string groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
    }
}
}
