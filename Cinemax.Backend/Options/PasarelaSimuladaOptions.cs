namespace Cinemax.Backend.Options
{
    public enum ModoPasarelaSimulada { Aprobar, Rechazar, SegunCentavos }
    public class PasarelaSimuladaOptions
    {
        public const string Seccion = "PasarelaSimulada";
        public ModoPasarelaSimulada Modo { get; set; } = ModoPasarelaSimulada.Aprobar;
    }
}
