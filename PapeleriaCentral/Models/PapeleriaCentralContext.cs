using Microsoft.EntityFrameworkCore;

namespace PapeleriaCentral.Models;

public partial class PapeleriaCentralContext : DbContext
{
    public PapeleriaCentralContext()
    {
    }

    public PapeleriaCentralContext(DbContextOptions<PapeleriaCentralContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Ordene> Ordenes { get; set; }

    public virtual DbSet<TiposCliente> TiposClientes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ordene>(entity =>
        {
            entity.HasKey(e => e.IdOrden);

            entity.HasIndex(e => e.NumeroOrden)
                .IsUnique();

            entity.Property(e => e.Cliente)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.DescuentoAplicado)
                .HasColumnType("decimal(5, 2)");

            entity.Property(e => e.MetodoPago)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.MontoFinal)
                .HasColumnType("decimal(12, 2)");

            entity.Property(e => e.MontoTotal)
                .HasColumnType("decimal(12, 2)");

            entity.Property(e => e.NumeroOrden)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.IdTipoClienteNavigation)
                .WithMany(p => p.Ordenes)
                .HasForeignKey(d => d.IdTipoCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ordenes_TiposCliente");
        });

        modelBuilder.Entity<TiposCliente>(entity =>
        {
            entity.HasKey(e => e.IdTipoCliente);

            entity.ToTable("TiposCliente");

            entity.HasIndex(e => e.Nombre)
                .IsUnique();

            entity.Property(e => e.Descuento)
                .HasColumnType("decimal(5, 2)");

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}