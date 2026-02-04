namespace FoodOrder.Infrastructure.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context)
        {
            string correlationId;
            if (!context.Request.Headers.ContainsKey("X-Correlation-Id") || string.IsNullOrEmpty(context.Request.Headers["X-Correlation-Id"]))
            {
                correlationId = Guid.NewGuid().ToString();
            }
            else
            {
                correlationId = context.Request.Headers["X-Correlation-Id"];
            }
            using (_logger.BeginScope("CorrelationId:{correlationId}", correlationId))
            {

                context.Items["CorrelationId"] = correlationId;
                context.Response.Headers["X-Correlation-Id"] = correlationId;
                await _next(context);
                _logger.LogInformation("[{Time}] {Method} {Path}",
                DateTime.Now,
                context.Request.Method,
                context.Request.Path);
            }
        }

    
}
}
