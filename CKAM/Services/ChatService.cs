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
using System.Text.Json;
using System.Diagnostics;

namespace CKAM.Services
{
    internal class ChatService
    {
        private string? token;
        private long? user_id;
        private ClientWebSocket webSocket;
        private readonly HttpClient httpClient;
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
                user_id = loginResponce?.UserId;
                return true;
            }
            catch { return false; }
        }
        public async Task<string?> RegisterAsync(string username, string password)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("/api/register", new { username, password });
                if (!response.IsSuccessStatusCode) return "Ошибка регистрации: " + response.StatusCode;
                return null;
            }
            catch (Exception ex) { return $"Ошибка регистрации: {ex.Message}"; }
        }
        public async Task<List<Message>> GetHistoryAsync()
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, "api/messages");
                if (!string.IsNullOrEmpty(token))
                {
                    req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", token);
                }
                var responce = await httpClient.SendAsync(req);
                if (responce.IsSuccessStatusCode)
                {
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return await responce.Content.ReadFromJsonAsync<List<Message>>(options) ?? new();
                }
                return new();
            } 
            catch(Exception ex)
            { 
                Debug.WriteLine($"Ошибка получений истории: {ex.Message}");
                return new(); 
            }
        }
        public async Task ConnectWebSocketAsync()
        {
            webSocket.Options.SetRequestHeader("Authorization", $"Bearer {token}");
            await webSocket.ConnectAsync(new Uri("wss://localhost:24242"), CancellationToken.None);
            _ = Task.Run(ReceiveLoop);
        }

        public async Task ReceiveLoop()
        {
            var buffer = new byte[1024 * 4];
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close) break;

                    var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var message = System.Text.Json.JsonSerializer.Deserialize<Message>(messageJson);
                    if (message != null) OnMessageReceived?.Invoke(message);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка в WebSocket: {ex.Message}");
                await Task.Delay(3000);
                await ConnectWebSocketAsync();
            }
        }

        public async Task SendMessageAsync(string content)
        {
            if (webSocket == null || webSocket.State != WebSocketState.Open) return;
            var json = System.Text.Json.JsonSerializer.Serialize(new { user_id = user_id, content = content });
            var bytes = Encoding.UTF8.GetBytes(json);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

        }
        public ChatService()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:24242") }; 
            webSocket = new ClientWebSocket();
            webSocket.Options.RemoteCertificateValidationCallback = (message, cert, chain, errors) => true;
        }
    }
}
