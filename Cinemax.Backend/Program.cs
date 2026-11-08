using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Data.Common;
using System.Security.Claims;
using System.Text;
using Cinemax.Backend.Data;
using Cinemax.Backend.Implementacion;
using Cinemax.Backend.Options;
using Cinemax.Backend.PasarelaSimulada.Pagos;
using Cinemax.Backend.Repositories;
using Cinemax.Backend.Services;
using Cinemax.Backend.Utilidades;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community; // se pone como loquita si no agrego esto

var bootstrapLogger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Logger = bootstrapLogger;

try
{
    Log.Information("Iniciando aplicación Cinemax.Backend...");
    var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddControllers();

    string[] origenes = builder.Configuration.GetSection("Cors:Origenes").Get<string[]>() ?? [];

    builder.Services.AddCors(options =>
        options.AddPolicy("Web", policy => policy
            .WithOrigins(origenes)
            .AllowAnyHeader()
            .AllowAnyMethod()));

    builder.Services.AddAuthorization();

        builder.Services.AddDbContext<DataContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwt = builder.Configuration
                    .GetRequiredSection("JwtSettings")
                    .Get<JwtSettings>()!;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    NameClaimType = ClaimTypes.Email,
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddOptions<SalaLayoutOptions>()
            .Bind(builder.Configuration.GetSection(SalaLayoutOptions.Seccion))
            .Validate(o => o.Filas is >= 1 and <= 26
                           && o.Bloques.Count > 0 && o.Bloques.All(b => b > 0)
                           && o.AnchoPasillo >= 0,
                "SalaLayout inválido: 1-26 filas, bloques mayores a cero y pasillo no negativo.")
            .ValidateOnStart();

            builder.Services.AddOptions<FuncionOptions>()
            .Bind(builder.Configuration.GetSection(FuncionOptions.Seccion))
            .Validate(o => o.MargenLimpiezaMinutos is >= 0 and <= 120, "Funciones:MargenLimpiezaMinutos inválido.")
            .ValidateOnStart();

            builder.Services.AddOptions<VentaOptions>()
            .Bind(builder.Configuration.GetSection(VentaOptions.Seccion))
            .Validate(o => o.MinutosVencimiento is >= 1 and <= 120 && o.MaxButacasPorVenta is >= 1 and <= 30,
                "Ventas: configuración inválida.")
            .ValidateOnStart();

        builder.Services.AddScoped<IClienteRepository, ClienteRepositoryPsqlEF>();
        builder.Services.AddScoped<IImagenRepository, ImagenRepositoryPsqlEF>();
        builder.Services.AddScoped<IRegisterRepository, RegisterRepositoryPsqlEF>();
        builder.Services.AddScoped<ISesionUsuarioRepository, SesionUsuarioRepositoryPsqlEF>();
        builder.Services.AddScoped<IUsuarioRepository, UsuarioRepositoryPsqlEF>();
        builder.Services.AddScoped<IVentaRepository, VentaRepositoryPsqlEF>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IPasarelaPago, PasarelaPagoSimulada>();
        builder.Services.AddScoped<IPagoRepository, PagoRepositoryPsqlEF>();
        builder.Services.AddScoped<IPeliculaRepository, PeliculaRepositoryPsqlEF>();
        builder.Services.AddScoped<IGeneroRepository, GeneroRepositoryPsqlEF>();
        builder.Services.AddScoped<IPersonaRepository, PersonaRepositoryPsqlEF>();
        builder.Services.AddScoped<ISalaRepository, SalaRepositoryPsqlEF>();
        builder.Services.AddScoped<IFuncionRepository, FuncionRepositoryPsqlEF>();
        builder.Services.AddScoped<ISnackRepository, SnackRepositoryPsqlEF>();
        builder.Services.AddScoped<IInventarioRepository, InventarioRepositoryPsqlEF>();
        builder.Services.AddScoped<IComboRepository, ComboRepositoryPsqlEF>();
        builder.Services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajoPsqlEF>();
        builder.Services.AddScoped<IVentaRepository, VentaRepositoryPsqlEF>();
        builder.Services.AddScoped<IPasarelaPago, PasarelaPagoSimulada>();

        builder.Services.AddScoped<ClienteService>();
        builder.Services.AddScoped<ImagenService>();
        builder.Services.AddScoped<LoginService>();
        builder.Services.AddScoped<PagoService>();
        builder.Services.AddScoped<RegisterService>();
        builder.Services.AddScoped<SesionService>();
        builder.Services.AddScoped<UsuarioService>();
        builder.Services.AddScoped<VentaService>();
        builder.Services.AddScoped<PeliculaService>();
        builder.Services.AddScoped<PersonaService>();
        builder.Services.AddScoped<GeneroService>();
        builder.Services.AddScoped<SalaService>();
        builder.Services.AddScoped<FuncionService>();
        builder.Services.AddScoped<SnackService>();
        builder.Services.AddScoped<ComboService>();
        builder.Services.AddScoped<InventarioService>();
        builder.Services.AddScoped<PagoService>();
        builder.Services.AddScoped<VentaService>();
        builder.Services.AddScoped<ImagenMapeador>();

        builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    var webRoot = app.Environment.WebRootPath
    ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");

    Directory.CreateDirectory(webRoot);
    app.Environment.WebRootPath = webRoot;
    Log.Information("WebRoot: {WebRoot} | ContentRoot: {ContentRoot}", webRoot, app.Environment.ContentRootPath);

    using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            await SeedDatos.CrearAdministrador(context);
        }

        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                var exceptionFeature =
                    context.Features.Get<IExceptionHandlerFeature>();

                Exception? exception = exceptionFeature?.Error;

                var logger = context.RequestServices
                    .GetRequiredService<ILogger<Program>>();

                context.Response.ContentType = "application/json";
                context.Response.StatusCode =
                    StatusCodes.Status500InternalServerError;

                DbException? dbException =
                    BuscarDbExceptionEnCadena(exception);

                if (dbException is not null)
                {
                    logger.LogError(
                        exception,
                        "Error de base de datos al procesar {Method} {Path}",
                        context.Request.Method,
                        context.Request.Path);

                    await context.Response.WriteAsJsonAsync(
                        "Ocurrió un error interno en el servidor. Por favor, intentá nuevamente más tarde.");
                }
                else
                {
                    logger.LogError(
                        exception,
                        "Excepción no controlada al procesar {Method} {Path}",
                        context.Request.Method,
                        context.Request.Path);

                    await context.Response.WriteAsJsonAsync(
                        "Ocurrió un error interno en el servidor.");
                }
            });
        });
        static DbException? BuscarDbExceptionEnCadena(Exception? ex)
        {
            while (ex is not null)
            {
                if (ex is DbException dbEx)
                    return dbEx;

                ex = ex.InnerException;
            }

            return null;
        }

        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";
        });

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseCors("Web");

    var imagenesPath = Path.Combine(webRoot, "uploads");

    Directory.CreateDirectory(imagenesPath);

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(imagenesPath),
        RequestPath = "/uploads"
    });
    app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        Log.Information("Aplicación iniciada correctamente.");

        app.Run();
}

catch (Exception ex)

{
    Log.Fatal(ex,"Fallo crítico no controlado durante el inicio de la aplicación.");
}
finally
{
    Log.CloseAndFlush();
}




