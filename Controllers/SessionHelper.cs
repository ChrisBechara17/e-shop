using Microsoft.AspNetCore.Http;

namespace EShopOnWeb.Controllers;

/// <summary>
/// Centralizes creation and reuse of the anonymous session ID cookie.
/// </summary>
public static class SessionHelper
{
    public const string SessionCookieName = "EShopOnWeb.SessionId";

    public static string GetOrCreateSessionId(HttpContext httpContext)
    {
        if (httpContext.Request.Cookies.TryGetValue(SessionCookieName, out var existing) &&
            !string.IsNullOrWhiteSpace(existing))
        {
            return existing;
        }

        var sessionId = Guid.NewGuid().ToString();
        httpContext.Response.Cookies.Append(SessionCookieName, sessionId, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });

        return sessionId;
    }
}
