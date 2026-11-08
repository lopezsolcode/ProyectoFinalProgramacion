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
            modelBuilder.ApplyConfiguration(new SesionUsuarioConfiguracion());
            modelBuilder.ApplyConfiguration(new ClienteConfiguracion());
            modelBuilder.ApplyConfiguration(new ImagenConfiguracion());
            modelBuilder.ApplyConfiguration(new UsuarioConfiguracion());
            modelBuilder.ApplyConfiguration(new VentaConfiguracion());
            modelBuilder.ApplyConfiguration(new AuditoriaConfiguracion());
            modelBuilder.ApplyConfiguration(new PagoConfiguracion());
            modelBuilder.ApplyConfiguration(new PeliculaConfiguracion());
            modelBuilder.ApplyConfiguration(new GeneroPeliculaConfiguracion());
            modelBuilder.ApplyConfiguration(new SalaConfiguracion());
            modelBuilder.ApplyConfiguration(new ButacaConfiguracion());
            modelBuilder.ApplyConfiguration(new GeneroConfiguracion());
            modelBuilder.ApplyConfiguration(new PersonaConfiguracion());
            modelBuilder.ApplyConfiguration(new PersonaPeliculaConfiguracion());
            modelBuilder.ApplyConfiguration(new FuncionConfiguracion());
            modelBuilder.ApplyConfiguration(new SnackConfiguracion());
            modelBuilder.ApplyConfiguration(new InventarioConfiguracion());
            modelBuilder.ApplyConfiguration(new ComboConfiguracion());
            modelBuilder.ApplyConfiguration(new ComboSnackConfiguracion());
            modelBuilder.ApplyConfiguration(new EntradaVentaConfiguracion());
            modelBuilder.ApplyConfiguration(new DetalleVentaSnackConfiguracion());
            modelBuilder.ApplyConfiguration(new ConsumoStockVentaConfiguracion());

            // https://codewithmukesh.com/blog/global-query-filters-efcore/ Para no tener que poner si está o no eliminado. Esto lo tenemos que hacer con la mayoría de entidades.
            modelBuilder.Entity<Usuario>().HasQueryFilter(u => !u.Eliminado);
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Imagen> Imagenes { get; set; }
        public DbSet<SesionUsuario> SesionesUsuarios { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<GeneroPelicula> GenerosPeliculas { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Persona> Personas { get; set; }
        public DbSet<PersonaPelicula> PersonasPeliculas { get; set; }
        public DbSet<Sala> Salas { get; set; }
        public DbSet<Butaca> Butacas { get; set; }
        public DbSet<Funcion> Funciones { get; set; }
        public DbSet<Snack> Snacks { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<ComboSnack> ComboSnacks { get; set; }
        public DbSet<EntradaVenta> EntradasVenta { get; set; }
        public DbSet<ConsumoStockVenta> ConsumosStockVenta { get; set; }


    }
}
