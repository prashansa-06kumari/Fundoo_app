using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Middleware.Services.Cache
{
    public class CachedResponse
    {
        public int StatusCode { get; set; }
        public string? ContentType { get; set; }
        public byte[] Body { get; set; } = Array.Empty<byte>();
    }

    public class ResponseCacheMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ResponseCacheMiddleware> _logger;

        public ResponseCacheMiddleware(
            RequestDelegate next,
            ICacheService cacheService,
            ILogger<ResponseCacheMiddleware> logger)
        {
            _next = next;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string method = context.Request.Method;

            // ---------- Write requests (POST/PUT/PATCH/DELETE) ----------
            // Kaam successful ho gaya to is user ka cache version badha do,
            // taaki uske purane cached GET responses bekaar ho jaayein.
            if (HttpMethods.IsPost(method) ||
                HttpMethods.IsPut(method) ||
                HttpMethods.IsPatch(method) ||
                HttpMethods.IsDelete(method))
            {
                await _next(context);

                string? writerId = GetUserId(context);

                if (writerId != null &&
                    context.Response.StatusCode >= 200 &&
                    context.Response.StatusCode < 300)
                {
                    await BumpVersionAsync(writerId);
                    _logger.LogInformation("CACHE INVALIDATED for user {UserId}", writerId);
                }

                return;
            }

            // ---------- GET requests ----------
            var attr = context.GetEndpoint()?
                .Metadata.GetMetadata<CacheResponseAttribute>();

            if (attr == null || !HttpMethods.IsGet(method))
            {
                await _next(context);
                return;
            }

            string key = await BuildKeyAsync(context);

            // 1. Cache HIT -> controller call hi nahi hota
            CachedResponse? cached =
                await _cacheService.GetAsync<CachedResponse>(key);

            if (cached != null)
            {
                _logger.LogInformation("CACHE HIT {Key}", key);

                context.Response.StatusCode = cached.StatusCode;
                context.Response.ContentType = cached.ContentType;
                context.Response.ContentLength = cached.Body.Length;
                context.Response.Headers["X-Cache"] = "HIT";

                await context.Response.Body.WriteAsync(cached.Body);
                return;
            }

            // 2. Cache MISS -> controller chalao aur response capture karo
            _logger.LogInformation("CACHE MISS {Key}", key);

            Stream originalBody = context.Response.Body;
            using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            try
            {
                await _next(context);

                byte[] body = buffer.ToArray();
                context.Response.Headers["X-Cache"] = "MISS";

                // Sirf successful (200) response cache hota hai
                if (context.Response.StatusCode == StatusCodes.Status200OK)
                {
                    await _cacheService.SetAsync(
                        key,
                        new CachedResponse
                        {
                            StatusCode = context.Response.StatusCode,
                            ContentType = context.Response.ContentType,
                            Body = body
                        },
                        TimeSpan.FromSeconds(attr.Seconds));
                }

                buffer.Position = 0;
                await buffer.CopyToAsync(originalBody);
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }

        private static string? GetUserId(HttpContext context)
        {
            return context.User.FindFirst("sub")?.Value
                ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private static string VersionKey(string userId) => $"ver:{userId}";

        private async Task BumpVersionAsync(string userId)
        {
            long current = await _cacheService.GetAsync<long>(VersionKey(userId));

            await _cacheService.SetAsync(
                VersionKey(userId),
                current + 1,
                TimeSpan.FromDays(30));
        }

        // Key me userId aur version hai:
        // - ek user ka data dusre ko nahi dikhta
        // - write hone par version badhta hai, to purani keys use hi nahi hoti
        private async Task<string> BuildKeyAsync(HttpContext context)
        {
            string userId = GetUserId(context) ?? "anonymous";

            long version = userId == "anonymous"
                ? 0
                : await _cacheService.GetAsync<long>(VersionKey(userId));

            string path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
            string query = context.Request.QueryString.Value ?? "";

            return $"resp:{userId}:v{version}:{path}{query}";
        }
    }
}