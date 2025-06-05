using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class UserStatusHub : Hub
{
    private static readonly Dictionary<string, string> Connections = new();

    public override Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier; 
        if (userId != null)
        {
            Connections[Context.ConnectionId] = userId;
            Clients.All.SendAsync("UserCameOnline", userId);
        }
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (Connections.TryGetValue(Context.ConnectionId, out var userId))
        {
            Clients.All.SendAsync("UserWentOffline", userId);
            Connections.Remove(Context.ConnectionId);
        }
        return base.OnDisconnectedAsync(exception);
    }
}