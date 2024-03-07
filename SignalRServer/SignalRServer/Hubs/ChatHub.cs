using Microsoft.AspNetCore.SignalR;
using System.Data.SqlTypes;

namespace SignalRServer.Hubs;

public class ChatHub : Hub
{
    private readonly IDictionary<string, UserRoomConnection> _connection;

    public ChatHub(IDictionary<string, UserRoomConnection> connection)
    {
        _connection = connection;
    }
    public bool IsUserInRoom (string connectionId, string room)
    {
        return _connection.TryGetValue(connectionId, out UserRoomConnection userRoomConnection) && userRoomConnection.Room == room;
    }
    public async Task JoinRoom(UserRoomConnection userConnection, bool status)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room!);
        _connection[Context.ConnectionId] = userConnection;
        await Clients.Group(userConnection.Room!)
            .SendAsync("ReceiveMessage", "JoinRoomNotification", $"{userConnection.User} has Joined the Group", userConnection.Room!, status, DateTime.Now);
        await SendConnectedUser(userConnection.Room!);
    }

    public async Task SendMessage(string message, string receivedRoom, bool status)
    {
        if (_connection.TryGetValue(Context.ConnectionId, out UserRoomConnection userRoomConnection))
        {
            await Clients.Group(userRoomConnection.Room!)
                .SendAsync("ReceiveMessage", userRoomConnection.User, message, receivedRoom, status, DateTime.Now);
        }
    }

    public override Task OnDisconnectedAsync(Exception? exp)
    {
        if (!_connection.TryGetValue(Context.ConnectionId, out UserRoomConnection roomConnection))
        {
            return base.OnDisconnectedAsync(exp);
        }

        _connection.Remove(Context.ConnectionId);
        Clients.Group(roomConnection.Room!)
            .SendAsync("ReceiveMessage", "JoinRoomNotification", $"{roomConnection.User} has Left the Group", DateTime.Now);
        SendConnectedUser(roomConnection.Room!);
        return base.OnDisconnectedAsync(exp);
    }

    public Task SendConnectedUser(string     room)
    {
        var users = _connection.Values
            .Where(u => u.Room == room)
            .Select(s => s.User);
        return Clients.Group(room).SendAsync("ConnectedUser", users);
    }
}
