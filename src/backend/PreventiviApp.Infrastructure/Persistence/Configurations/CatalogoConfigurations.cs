using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Infrastructure.Persistence.Configurations;

// Configuraciones del catálogo (preciosario). Los nombres de tabla siguen snake_case.

public sealed class PreciosarioConfiguration : IEntityTypeConfiguration<Preciosario>
{
    public void Configure(EntityTypeBuilder<Preciosario> builder)
    {
        builder.ToTable("preciosario");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.VersionPreciosario).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Fuente).HasMaxLength(200);
        builder.Property(x => x.OrigenDcf).HasMaxLength(500);
        builder.Property(x => x.HashArchivo).HasMaxLength(64);
        builder.HasQueryFilter(x => !x.Eliminado);
    }
}

public sealed class UnidadConfiguration : IEntityTypeConfiguration<Unidad>
{
    public void Configure(EntityTypeBuilder<Unidad> builder)
    {
        builder.ToTable("unidad");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Codigo).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Codigo);
    }
}

public sealed class CapituloConfiguration : IEntityTypeConfiguration<Capitulo>
{
    public void Configure(EntityTypeBuilder<Capitulo> builder)
    {
        builder.ToTable("capitulo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Titulo).HasMaxLength(500).IsRequired();

        builder.HasOne<Preciosario>().WithMany().HasForeignKey(x => x.PreciosarioId);
        builder.HasOne<Capitulo>().WithMany().HasForeignKey(x => x.PadreId).IsRequired(false);

        builder.HasIndex(x => x.PreciosarioId);
        builder.HasIndex(x => new { x.PreciosarioId, x.Codigo });
    }
}

public sealed class RecursoConfiguration : IEntityTypeConfiguration<Recurso>
{
    public void Configure(EntityTypeBuilder<Recurso> builder)
    {
        builder.ToTable("recurso");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Precio).HasPrecision(18, 4);

        builder.HasOne<Preciosario>().WithMany().HasForeignKey(x => x.PreciosarioId);
        builder.HasOne<Unidad>().WithMany().HasForeignKey(x => x.UnidadId).IsRequired(false);

        builder.HasIndex(x => new { x.PreciosarioId, x.Codigo });
    }
}

public sealed class PartidaConfiguration : IEntityTypeConfiguration<Partida>
{
    public void Configure(EntityTypeBuilder<Partida> builder)
    {
        builder.ToTable("partida");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Resumen).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.TextoLargo);
        builder.Property(x => x.Precio).HasPrecision(18, 2);

        builder.HasOne<Capitulo>().WithMany().HasForeignKey(x => x.CapituloId);
        builder.HasOne<Unidad>().WithMany().HasForeignKey(x => x.UnidadId).IsRequired(false);

        builder.HasIndex(x => x.CapituloId);
        builder.HasIndex(x => x.Codigo);
    }
}

public sealed class DescompuestoConfiguration : IEntityTypeConfiguration<Descompuesto>
{
    public void Configure(EntityTypeBuilder<Descompuesto> builder)
    {
        builder.ToTable("descompuesto");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Rendimiento).HasPrecision(18, 4);
        builder.Property(x => x.PrecioUnitario).HasPrecision(18, 4);
        builder.Ignore(x => x.Importe); // calculado

        builder.HasOne<Partida>().WithMany().HasForeignKey(x => x.PartidaId);
        builder.HasOne<Recurso>().WithMany().HasForeignKey(x => x.RecursoId);

        builder.HasIndex(x => x.PartidaId);
    }
}

public sealed class PrecioConfiguration : IEntityTypeConfiguration<Precio>
{
    public void Configure(EntityTypeBuilder<Precio> builder)
    {
        builder.ToTable("precio");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Valor).HasPrecision(18, 2);
        builder.Property(x => x.Moneda).HasMaxLength(3).IsRequired();

        builder.HasOne<Partida>().WithMany().HasForeignKey(x => x.PartidaId);
        builder.HasIndex(x => x.PartidaId);
    }
}
