using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;


    public class SmsSagaDbContext : SagaDbContext
    {
        public SmsSagaDbContext(DbContextOptions<SmsSagaDbContext> options)
            : base(options) { }

        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get { yield return new OrderSmsSagaMap(); }
        }
    }


