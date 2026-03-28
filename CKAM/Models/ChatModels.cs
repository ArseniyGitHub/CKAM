using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CKAM.Models
{
    public record Message
    {
        [JsonPropertyName("sender_name")]
        public string SenderName { get; set; } = string.Empty;
        [property: JsonPropertyName("sender_id")]
        long SenderId;
        [JsonPropertyName("id")]
        long Id { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;
    }

    public record User
    {
        [property: JsonPropertyName("username")]
        string UserName { get; set; } = string.Empty;
        [property: JsonPropertyName("display_name")]
        string DisplayName { get; set; } = string.Empty;
        [property: JsonPropertyName("id")]
        long Id { get; set; }
    }

    public record LoginResponce
    {
        [property: JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
        [property: JsonPropertyName("user_id")]
        long UserId { get; set; }
    }
    public record Chat {
        [property: JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [property: JsonPropertyName("description")]
        public string Descr { get; set; } = string.Empty;
        [property: JsonPropertyName("chat_id")]
        long id { get; set; }
        [property: JsonPropertyName("chat_type")]
        public string ChatType { get; set; } = string.Empty;
    }

}