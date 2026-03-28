using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using CKAM.Models;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Threading;

namespace CKAM.Services
{
    internal class ChatService
    {
        private string? token;
        private ClientWebSocket webSocket;
        private readonly HttpClient httpClient = new() { BaseAddress = new Uri("http://localhost:24242") };
        public event Action<Message> OnMessageReceived;
        public event Action<Message> OnMessageSent;
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("/api/login", new { username, password });
                if (!response.IsSuccessStatusCode) return false;
                var loginResponce = await response.Content.ReadFromJsonAsync<LoginResponce>();
                token = loginResponce?.Token;
                return true;
            }
            catch { return false; }
        }
        public async Task<List<Message>> GetHistoryAsync()
        {
            return await httpClient.GetFromJsonAsync<List<Message>>("/api/messages") ?? new();
        }
        public async Task ConnectWebSocketAsync()
        {
            webSocket = new ClientWebSocket();
            webSocket.Options.SetRequestHeader("Authorization", $"Bearer {token}");
            await webSocket.ConnectAsync(new Uri("ws://localhost:24242/ws"), CancellationToken.None);
            _ = Task.Run(ReceiveLoop);
        }

        public async Task ReceiveLoop()
        {
            var buffer = new byte[1024 * 4];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) break;

                var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var message = System.Text.Json.JsonSerializer.Deserialize<Message>(messageJson);
                if (message != null) OnMessageReceived?.Invoke(message);
            }
        }

        public async Task SendMessageAsync(long userId, string content)
        {
            if (webSocket == null || webSocket.State != WebSocketState.Open) return;
            var json = System.Text.Json.JsonSerializer.Serialize(new { user_id = userId, content = content });
            var bytes = Encoding.UTF8.GetBytes(json);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

        }
    }
}
