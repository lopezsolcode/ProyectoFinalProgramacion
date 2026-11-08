using Cinemax.Backend.Models;
using Cinemax.Backend.Options;

namespace Cinemax.Backend.Utilidades
{
    public static class GeneradorButacas
    {
        public static List<Butaca> Generar(SalaLayoutOptions o)
        {
            List<Butaca> butacas = new();

            for (int f = 0; f < o.Filas; f++)
            {
                string fila = ((char)('A' + f)).ToString();
                bool ultimaFila = f == o.Filas - 1;
                int numero = 0;
                int columna = 0;

                for (int b = 0; b < o.Bloques.Count; b++)
                {
                    if (b > 0) columna += o.AnchoPasillo;

                    bool bloqueCentral = b > 0 && b < o.Bloques.Count - 1;
                    bool omitir = ultimaFila && o.UltimaFilaSinBloqueCentral && bloqueCentral;

                    for (int i = 0; i < o.Bloques[b]; i++)
                    {
                        numero++;
                        columna++;
                        if (omitir) continue; // la numeración y la columna avanzan igual

                        butacas.Add(new Butaca
                        {
                            Fila = fila,
                            Numero = numero,
                            Columna = columna,
                            Accesible = ultimaFila && o.UltimaFilaAccesibles.Contains(numero)
                        });
                    }
                }
            }

            return butacas;
        }
    }
}
