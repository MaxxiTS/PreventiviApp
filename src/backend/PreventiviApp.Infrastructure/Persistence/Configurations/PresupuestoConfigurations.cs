using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Infrastructure.Persistence.Configurations;

// Configuraciones del árbol de presupuesto.

public sealed class IvaConfiguration : IEntityTypeConfiguration<Iva>
{
    public void Configure(EntityTypeBuilder<Iva> builder)
    {
        builder.ToTable("iva");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Porcentaje).HasPrecision(5, 2);
    }
}

public sealed class PresupuestoConfiguration : IEntityTypeConfiguration<Presupuesto>
{
    public void Configure(EntityTypeBuilder<Presupuesto> builder)
    {
        builder.ToTable("presupuesto");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.CostesIndirectosPct).HasPrecision(7, 4);
        builder.Property(x => x.BajaPct).HasPrecision(7, 4);
        builder.Property(x => x.DescuentosImporte).HasPrecision(18, 2);

        builder.HasOne<Proyecto>().WithMany().HasForeignKey(x => x.ProyectoId);
        builder.HasOne(x => x.Iva).WithMany().HasForeignKey("IvaId").IsRequired();

        // El árbol de capítulos se reconstruye en el repositorio; se ignora la
        // navegación de raíces para evitar ambigüedad con los subcapítulos.
        builder.Ignore(x => x.CapitulosRaiz);

        builder.HasIndex(x => x.ProyectoId);
        builder.HasQueryFilter(x => !x.Eliminado);
    }
}

public sealed class CapituloPresupuestoConfiguration : IEntityTypeConfiguration<CapituloPresupuesto>
{
    public void Configure(EntityTypeBuilder<CapituloPresupuesto> builder)
    {
        builder.ToTable("capitulo_presupuesto");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Titulo).HasMaxLength(500).IsRequired();

        builder.Ignore(x => x.Dirty);
        builder.Ignore(x => x.SubtotalCache);

        builder.HasOne<Presupuesto>().WithMany().HasForeignKey(x => x.PresupuestoId);

        builder.HasMany(x => x.Subcapitulos)
            .WithOne()
            .HasForeignKey(x => x.PadreId)
            .IsRequired(false);

        builder.HasMany(x => x.Partidas)
            .WithOne()
            .HasForeignKey(x => x.CapituloPresupuestoId);

        builder.Navigation(x => x.Subcapitulos).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Partidas).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.PresupuestoId);
        builder.HasIndex(x => x.PadreId);
    }
}

public sealed class PartidaPresupuestoConfiguration : IEntityTypeConfiguration<PartidaPresupuesto>
{
    public void Configure(EntityTypeBuilder<PartidaPresupuesto> builder)
    {
        builder.ToTable("partida_presupuesto");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Resumen).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Precio).HasPrecision(18, 2);

        builder.HasOne(x => x.Medicion)
            .WithOne()
            .HasForeignKey<Medicion>(x => x.PartidaPresupuestoId);

        builder.HasIndex(x => x.CapituloPresupuestoId);
        builder.HasIndex(x => x.PartidaOrigenId);
    }
}

public sealed class MedicionConfiguration : IEntityTypeConfiguration<Medicion>
{
    public void Configure(EntityTypeBuilder<Medicion> builder)
    {
        builder.ToTable("medicion");
        builder.HasKey(x => x.Id);

        builder.HasMany(x => x.Lineas)
            .WithOne()
            .HasForeignKey(x => x.MedicionId);

        builder.Navigation(x => x.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.PartidaPresupuestoId).IsUnique();
    }
}

public sealed class LineaMedicionConfiguration : IEntityTypeConfiguration<LineaMedicion>
{
    public void Configure(EntityTypeBuilder<LineaMedicion> builder)
    {
        builder.ToTable("linea_medicion");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Comentario).HasMaxLength(500);
        builder.Property(x => x.Formula).HasMaxLength(500);
        builder.Property(x => x.Uds).HasPrecision(18, 4);
        builder.Property(x => x.Largo).HasPrecision(18, 4);
        builder.Property(x => x.Ancho).HasPrecision(18, 4);
        builder.Property(x => x.Alto).HasPrecision(18, 4);
        builder.Property(x => x.Coeficiente).HasPrecision(18, 4);

        builder.HasIndex(x => x.MedicionId);
    }
}
