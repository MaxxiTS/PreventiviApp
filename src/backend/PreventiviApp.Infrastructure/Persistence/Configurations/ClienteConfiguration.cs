using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Infrastructure.Persistence.Configurations;

public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("cliente");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Nif).HasMaxLength(20);
        builder.Property(x => x.Contacto).HasMaxLength(200);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Telefono).HasMaxLength(50);
        builder.Property(x => x.Direccion).HasMaxLength(500);

        builder.HasQueryFilter(x => !x.Eliminado);
    }
}
