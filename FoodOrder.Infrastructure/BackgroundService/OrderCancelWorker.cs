using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;


namespace FoodOrder.Infrastructure.BackgroundServices
{
    public class OrderCancelWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrderCancelWorker> _logger;

        public OrderCancelWorker(IServiceScopeFactory serviceScopeFactory, ILogger<OrderCancelWorker> logger)
        {
            _scopeFactory = serviceScopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker start");
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var cutoff = DateTime.Now.AddMinutes(-15);
                var orderToCancel = await db.Orders.Where(o => (o.Status ==
                OrderStatus.Pending && o.CreatedAt < cutoff)).ToListAsync(stoppingToken);
                foreach (var order in orderToCancel)
                {
                    order.Status = OrderStatus.Cancelled;
                    order.UpdatedAt = DateTime.Now;
                }
                if (orderToCancel.Count > 0)
                {
                    await db.SaveChangesAsync(stoppingToken);
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

        }
    }
}
