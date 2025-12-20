using pyreApi.Models;
using Microsoft.EntityFrameworkCore;

namespace pyreApi.Data
{
    public class ApplicationDbContext : DbContext // Heredar de DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) // Constructor
            : base(options) { }

        // Mapeo de entidades con los nombres correctos según la base de datos
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Imagen> Imagen { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<EstadoFisicoHerramienta> EstadoFisicoHerramienta { get; set; }
        public DbSet<EstadoDisponibilidadHerramienta> EstadoDisponibilidadHerramienta { get; set; }
        public DbSet<FamiliaHerramientas> FamiliaHerramientas { get; set; }
        public DbSet<Planta> Planta { get; set; }
        public DbSet<Obra> Obra { get; set; }
        public DbSet<Cliente> Cliente { get; set; }
        public DbSet<Herramienta> Herramienta { get; set; }
        public DbSet<MovimientoHerramienta> MovimientoHerramienta { get; set; }
        public DbSet<Alerta> Alerta { get; set; }
        public DbSet<AuditorGeneral> AuditorGeneral { get; set; }
        public DbSet<TipoAlerta> TipoAlerta { get; set; }
        public DbSet<TipoMovimientoHerramienta> TipoMovimientoHerramienta { get; set; }
        public DbSet<Proveedor> Proveedor { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) // Mapeo de tablas y relaciones
        {
            base.OnModelCreating(modelBuilder);

            // Mapear tablas existentes
            modelBuilder.Entity<Usuario>().ToTable("usuario");
            modelBuilder.Entity<Imagen>().ToTable("imagen");
            modelBuilder.Entity<Rol>().ToTable("rol");
            modelBuilder.Entity<EstadoFisicoHerramienta>().ToTable("estadofisicoherramienta");
            modelBuilder.Entity<EstadoDisponibilidadHerramienta>().ToTable("estadodisponibilidadherramienta");
            modelBuilder.Entity<FamiliaHerramientas>().ToTable("familiaherramientas");
            modelBuilder.Entity<Planta>().ToTable("planta");
            modelBuilder.Entity<Obra>().ToTable("obra");
            modelBuilder.Entity<Cliente>().ToTable("cliente"); // Mapeo agregado para la entidad Cliente
            modelBuilder.Entity<Herramienta>().ToTable("herramienta");
            modelBuilder.Entity<MovimientoHerramienta>().ToTable("movimientoherramienta");
            modelBuilder.Entity<Alerta>().ToTable("alerta");
            modelBuilder.Entity<AuditorGeneral>().ToTable("auditorgeneral");
            modelBuilder.Entity<TipoAlerta>().ToTable("tipoalerta");
            modelBuilder.Entity<TipoMovimientoHerramienta>().ToTable("tipomovimientoherramienta");
            modelBuilder.Entity<Proveedor>().ToTable("proveedor");

            // Herramienta → EstadoFisicoHerramienta
            modelBuilder.Entity<Herramienta>()
                .HasOne(h => h.EstadoFisico)
                .WithMany(e => e.Herramientas)
                .HasForeignKey(h => h.IdEstadoFisico)
                .OnDelete(DeleteBehavior.Restrict);

            // Herramienta → EstadoDisponibilidadHerramienta
            modelBuilder.Entity<Herramienta>()
                .HasOne(h => h.EstadoDisponibilidad)
                .WithMany(e => e.Herramientas)
                .HasForeignKey(h => h.IdDisponibilidad)
                .OnDelete(DeleteBehavior.Restrict);

            // Herramienta → FamiliaHerramientas
            modelBuilder.Entity<Herramienta>()
                .HasOne(h => h.Familia)
                .WithMany(f => f.Herramientas)
                .HasForeignKey(h => h.IdFamilia)
                .OnDelete(DeleteBehavior.Restrict);

            // Herramienta → Planta
            modelBuilder.Entity<Herramienta>()
                .HasOne(h => h.Planta)
                .WithMany(p => p.Herramientas)
                .HasForeignKey(h => h.IdPlanta)
                .OnDelete(DeleteBehavior.Restrict);

            // Alerta → MovimientoHerramienta
            modelBuilder.Entity<Alerta>()
                .HasOne(a => a.MovimientoHerramienta)
                .WithMany(h => h.Alertas)
                .HasForeignKey(a => a.IdMovimiento)
                .OnDelete(DeleteBehavior.Cascade);

            // Alerta → Usuario
            modelBuilder.Entity<Alerta>()
                .HasOne(a => a.Usuario)
                .WithMany(h => h.Alertas)
                .HasForeignKey(a => a.IdModifica)
                .OnDelete(DeleteBehavior.Cascade);

            // Alerta → TipoAlerta
            modelBuilder.Entity<Alerta>()
                .HasOne(a => a.TipoAlerta)
                .WithMany(t => t.Alertas)
                .HasForeignKey(a => a.IdTipoAlerta)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
