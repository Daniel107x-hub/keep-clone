using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace backend.Middlewares;

public class VerifyTokenCacheMiddleware
{
    private readonly RequestDelegate _next;
    
    public VerifyTokenCacheMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task Invoke(HttpContext context, IMemoryCache _memoryCache)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<AuthorizeAttribute>() == null)
        {
            await _next(context);
            return;
        }
        var userName = context?.User?.Identity?.Name;
        if (userName == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }
        var storedToken = _memoryCache.Get<string>(userName);
        var providedToken = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (storedToken == null || storedToken != providedToken)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }
        await _next(context);
    }
}

public static class VerifyTokenCacheMiddlewareExtensions
{
    public static IApplicationBuilder UseVerifyTokenCache(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<VerifyTokenCacheMiddleware>();
    }
}