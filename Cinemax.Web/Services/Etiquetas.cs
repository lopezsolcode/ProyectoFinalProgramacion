using Cinemax.Shared.Enums;

namespace Cinemax.Web.Services;

// Textos para mostrar de los enums (los nombres internos no son presentables: mas13, DosD, Frances...).
public static class Etiquetas
{
    public static string DeClasificacion(Clasificacion c) => c switch
    {
        Clasificacion.ATP => "ATP",
        Clasificacion.mas13 => "+13",
        Clasificacion.mas16 => "+16",
        Clasificacion.mas18 => "+18",
        _ => c.ToString()
    };

    public static string DeFormato(FormatoFuncion f) => f switch
    {
        FormatoFuncion.DosD => "2D",
        FormatoFuncion.TresD => "3D",
        FormatoFuncion.CuatroD => "4D",
        _ => f.ToString()
    };

    public static string DeIdioma(IdiomaFuncion i) => i switch
    {
        IdiomaFuncion.Frances => "Francés",
        _ => i.ToString()
    };
}
