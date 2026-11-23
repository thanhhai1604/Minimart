using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace MiniMart.Web.Helpers
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Thêm dấu ? sau T để cho phép trả về null an toàn, triệt tiêu hoàn toàn cảnh báo CS8603
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}