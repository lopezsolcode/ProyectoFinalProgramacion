using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Cinemax.Web;
using Cinemax.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["Apis:Cinemax"] ?? throw new InvalidOperationException("Falta configurar Apis:Cinemax.")) });

builder.Services.AddMudServices();
builder.Services.AddScoped<AlmacenamientoTokensService>();
builder.Services.AddScoped<AutenticacionService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<EstadoAutenticacionProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<EstadoAutenticacionProvider>());
builder.Services.AddScoped<VentasService>();
builder.Services.AddScoped<PosService>();
builder.Services.AddScoped<PosEstadoVenta>();
builder.Services.AddScoped<PeliculaService>();
builder.Services.AddScoped<GeneroService>();
builder.Services.AddScoped<PersonaService>();


await builder.Build().RunAsync();
