using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OrderSmsSagaMap : SagaClassMap<OrderSmsSagaState>
{
    protected override void Configure(EntityTypeBuilder<OrderSmsSagaState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState)
              .HasMaxLength(64);

        entity.Property(x => x.FirstSmsSent);
        entity.Property(x => x.ReadySmsSent);

    }
}
