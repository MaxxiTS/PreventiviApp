using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Infrastructure.Persistence.Configurations;

public sealed class ProyectoConfiguration : IEntityTypeConfiguration<Proyecto>
{
    public void Configure(EntityTypeBuilder<Proyecto> builder)
    {
        builder.ToTable("proyecto");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Direccion).HasMaxLength(500);
        builder.Property(x => x.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(x => x.ClienteId);
        builder.HasIndex(x => x.Estado);

        builder.HasQueryFilter(x => !x.Eliminado);
    }
}
