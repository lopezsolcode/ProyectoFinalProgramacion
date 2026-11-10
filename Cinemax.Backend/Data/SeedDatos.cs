using Microsoft.EntityFrameworkCore;
using Cinemax.Backend.Models;
using Cinemax.Backend.Options;
using Cinemax.Backend.Utilidades;
using Cinemax.Shared.Enums;

namespace Cinemax.Backend.Data;

public static class SeedDatos
{
    public static async Task CrearAdministrador(DataContext context)
    {
        bool existe = await context.Usuarios
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Username == "admin");

        if (existe)
            return;

        var usuario = new Usuario
        {
            Username = "admin",
            Password = BCrypt.Net.BCrypt.EnhancedHashPassword("admin"),
            Email = "admin@tp6.local",
            Rol = Rol.Administrador
        };

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
    }

    // Datos de ejemplo SOLO para desarrollo: géneros, personas, salas, películas, funciones, snacks y un combo.
    // No hace nada si ya existe alguna película, así que se puede dejar activo sin duplicar datos.
    public static async Task CrearDatosDemo(DataContext context, SalaLayoutOptions layout, int margenLimpiezaMinutos)
    {
        if (await context.Peliculas.IgnoreQueryFilters().AnyAsync())
            return;

        // ---- Géneros ----
        string[] nombresGeneros = ["Acción", "Animación", "Comedia", "Drama", "Ciencia ficción", "Terror"];
        Dictionary<string, Genero> generos = await context.Generos.ToDictionaryAsync(g => g.Nombre);

        foreach (string nombre in nombresGeneros)
        {
            if (generos.ContainsKey(nombre)) continue;

            Genero genero = new() { Nombre = nombre };
            context.Generos.Add(genero);
            generos[nombre] = genero;
        }

        // ---- Personas ----
        string[] nombresPersonas =
        [
            "Lucía Ferrer", "Martín Aguirre", "Camila Roldán",
            "Tomás Beltrán", "Sofía Navarro", "Julián Ortega"
        ];
        Dictionary<string, Persona> personas = await context.Personas.ToDictionaryAsync(p => p.Nombre);

        foreach (string nombre in nombresPersonas)
        {
            if (personas.ContainsKey(nombre)) continue;

            Persona persona = new() { Nombre = nombre };
            context.Personas.Add(persona);
            personas[nombre] = persona;
        }

        // ---- Salas (con sus butacas, igual que SalaService.Crear) ----
        List<Sala> salas = await context.Salas.ToListAsync();

        if (salas.Count == 0)
        {
            for (int numero = 1; numero <= 2; numero++)
            {
                Sala sala = new() { Numero = numero, Butacas = GeneradorButacas.Generar(layout) };
                context.Salas.Add(sala);
                salas.Add(sala);
            }
        }

        await context.SaveChangesAsync(); // asigna los Id de lo anterior

        // ---- Películas ----
        DateOnly hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        Pelicula Nueva(string titulo, string sinopsis, Clasificacion clasificacion, int diasDesdeEstreno,
            int duracion, bool enCartelera, string[] nombresGen, string[] actores, string director) => new()
        {
            Titulo = titulo,
            Sinopsis = sinopsis,
            Clasificacion = clasificacion,
            FechaEstreno = hoy.AddDays(-diasDesdeEstreno),
            DuracionMinutos = duracion,
            EnCartelera = enCartelera,
            Generos = nombresGen.Select(n => new GeneroPelicula { GeneroId = generos[n].Id }).ToList(),
            Personas = actores
                .Select(a => new PersonaPelicula { PersonaId = personas[a].Id, Rol = RolPelicula.Actor })
                .Append(new PersonaPelicula { PersonaId = personas[director].Id, Rol = RolPelicula.Director })
                .ToList()
        };

        List<Pelicula> peliculas =
        [
            Nueva("El Último Faro",
                "Una farera solitaria descubre que las luces de su faro guían algo más que barcos.",
                Clasificacion.mas13, 20, 118, true, ["Drama", "Terror"],
                ["Lucía Ferrer", "Martín Aguirre"], "Julián Ortega"),
            Nueva("Ciudad de Neón",
                "En una metrópolis futurista, un mensajero encuentra un paquete que todos quieren recuperar.",
                Clasificacion.mas16, 10, 132, true, ["Acción", "Ciencia ficción"],
                ["Tomás Beltrán", "Camila Roldán"], "Sofía Navarro"),
            Nueva("Los Aventureros del Bosque",
                "Tres animales del bosque deben cruzar la montaña antes del primer invierno.",
                Clasificacion.ATP, 15, 96, true, ["Animación", "Comedia"],
                ["Camila Roldán", "Martín Aguirre"], "Sofía Navarro"),
            Nueva("Risas en la Oficina",
                "Un lunes cualquiera, una oficina entera queda atrapada en el ascensor con el jefe.",
                Clasificacion.ATP, 5, 101, true, ["Comedia"],
                ["Julián Ortega", "Lucía Ferrer"], "Tomás Beltrán"),
            Nueva("Noche de Susurros",
                "Una familia se muda a una casa donde las paredes repiten lo que se dice de noche.",
                Clasificacion.mas18, -14, 109, false, ["Terror"],
                ["Sofía Navarro", "Tomás Beltrán"], "Martín Aguirre")
        ];

        context.Peliculas.AddRange(peliculas);
        await context.SaveChangesAsync();

        // ---- Funciones: próximos 4 días, 3 horarios por sala (hora de Argentina) ----
        TimeZoneInfo zona;
        try
        {
            zona = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
        }
        catch (Exception e) when (e is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            zona = TimeZoneInfo.CreateCustomTimeZone("ART", TimeSpan.FromHours(-3), "ART", "ART");
        }

        DateOnly hoyLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zona));
        TimeOnly[] horarios = [new(15, 0), new(18, 0), new(21, 0)];
        List<Pelicula> cartelera = peliculas.Where(p => p.EnCartelera).ToList();
        int indice = 0;

        for (int dia = 0; dia < 4; dia++)
        {
            foreach (Sala sala in salas)
            {
                foreach (TimeOnly hora in horarios)
                {
                    Pelicula pelicula = cartelera[indice++ % cartelera.Count];
                    DateTime inicio = TimeZoneInfo.ConvertTimeToUtc(hoyLocal.AddDays(dia).ToDateTime(hora), zona);

                    if (inicio <= DateTime.UtcNow) continue;

                    bool animada = pelicula.Clasificacion == Clasificacion.ATP;

                    context.Funciones.Add(new Funcion
                    {
                        SalaId = sala.Id,
                        PeliculaId = pelicula.Id,
                        Formato = animada ? FormatoFuncion.TresD : FormatoFuncion.DosD,
                        Idioma = indice % 3 == 0 ? IdiomaFuncion.Inglés : IdiomaFuncion.Español,
                        FechaHora = inicio,
                        FechaFin = inicio.AddMinutes(pelicula.DuracionMinutos + margenLimpiezaMinutos),
                        PrecioEntrada = animada ? 7000m : 8500m
                    });
                }
            }
        }

        // ---- Snacks y un combo para probar el paso de snacks del POS ----
        if (!await context.Snacks.IgnoreQueryFilters().AnyAsync())
        {
            Snack pochoclo = new()
            {
                Nombre = "Pochoclos grandes",
                Precio = 4500m,
                Inventario = new Inventario { DescripcionUnidad = "balde", CantidadDisponible = 100 }
            };
            Snack gaseosa = new()
            {
                Nombre = "Gaseosa 500 ml",
                Precio = 2500m,
                Inventario = new Inventario { DescripcionUnidad = "vaso", CantidadDisponible = 150 }
            };
            Snack agua = new()
            {
                Nombre = "Agua mineral",
                Precio = 1800m,
                Inventario = new Inventario { DescripcionUnidad = "botella", CantidadDisponible = 80 }
            };

            context.Snacks.AddRange(pochoclo, gaseosa, agua);
            await context.SaveChangesAsync();

            context.Combos.Add(new Combo
            {
                Nombre = "Combo pareja",
                Precio = 8500m,
                Componentes =
                [
                    new ComboSnack { SnackId = pochoclo.Id, Cantidad = 1 },
                    new ComboSnack { SnackId = gaseosa.Id, Cantidad = 2 }
                ]
            });
        }

        await context.SaveChangesAsync();
    }
}
