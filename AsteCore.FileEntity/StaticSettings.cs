using System.Text.Json;

namespace AsteCore.FileEntity
{
    internal static class StaticSettings
    {
        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, // Игнорирует регистр
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Явно указываем политику именования
        };
    }
}
