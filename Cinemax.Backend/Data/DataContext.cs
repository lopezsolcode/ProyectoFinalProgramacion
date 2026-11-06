using Microsoft.EntityFrameworkCore;
using Cinemax.Backend.Data.Configuraciones;
using Cinemax.Backend.Models;

namespace Cinemax.Backend.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base (options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CategoriaConfiguracion());
            modelBuilder.ApplyConfiguration(new SesionUsuarioConfiguracion());
            modelBuilder.ApplyConfiguration(new ClienteConfiguracion());
            modelBuilder.ApplyConfiguration(new DetalleIngresoConfiguracion());
            modelBuilder.ApplyConfiguration(new DetalleVentaConfiguracion());
            modelBuilder.ApplyConfiguration(new ImagenConfiguracion());
            modelBuilder.ApplyConfiguration(new IngresoConfiguracion());
            modelBuilder.ApplyConfiguration(new ProductoConfiguracion());
            modelBuilder.ApplyConfiguration(new ProveedorConfiguracion());
            modelBuilder.ApplyConfiguration(new UsuarioConfiguracion());
            modelBuilder.ApplyConfiguration(new VentaConfiguracion());
            modelBuilder.ApplyConfiguration(new UsuarioConfiguracion());
            modelBuilder.ApplyConfiguration(new AuditoriaConfiguracion());
            modelBuilder.ApplyConfiguration(new PagoConfiguracion());
            modelBuilder.ApplyConfiguration(new PeliculaConfiguracion());
            modelBuilder.ApplyConfiguration(new GeneroConfiguracion());
            modelBuilder.ApplyConfiguration(new ActorConfiguracion());
            modelBuilder.ApplyConfiguration(new DirectorConfiguracion());


            // https://codewithmukesh.com/blog/global-query-filters-efcore/ Para no tener que poner si está o no eliminado. Esto lo tenemos que hacer con la mayoría de entidades.
            modelBuilder.Entity<Usuario>().HasQueryFilter(u => !u.Eliminado);
            modelBuilder.Entity<Proveedor>().HasQueryFilter(p => !p.Eliminado);


        }

        public DbSet<Categoria> Categorias{ get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<DetalleIngreso> DetallesIngresos { get; set; }
        public DbSet<DetalleVenta> DetallesVentas { get; set; }
        public DbSet<Imagen> Imagenes { get; set; }
        public DbSet<Ingreso> Ingresos { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<SesionUsuario> SesionesUsuarios { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<Pelicula> Genero { get; set; }
        public DbSet<Pelicula> Actor { get; set; }
        public DbSet<Pelicula> Director { get; set; }

    }
}
