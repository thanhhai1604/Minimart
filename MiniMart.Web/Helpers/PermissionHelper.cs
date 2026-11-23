using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace MiniMart.Web.Helpers
{
    public static class PermissionHelper
    {
        // Extension cho Razor View (IHtmlHelper)
        public static bool HasPermission(this IHtmlHelper htmlHelper, string maPQ)
        {
            var context = htmlHelper.ViewContext.HttpContext;
            return HasPermission(context, maPQ);
        }

        // Dùng cho Controller hoặc nơi có HttpContext
        public static bool HasPermission(HttpContext httpContext, string maPQ)
        {
            if (httpContext?.Session == null) return false;
            var permissions = httpContext.Session.GetObjectFromJson<List<string>>("UserPermissions");
            if (permissions == null) return false;
            return permissions.Contains(maPQ);
        }
    }
}