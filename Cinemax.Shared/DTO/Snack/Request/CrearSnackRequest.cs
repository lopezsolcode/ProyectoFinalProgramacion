using System;
using System.Collections.Generic;
using System.Text;

namespace Cinemax.Shared.DTO.Snack.Request
{
    public record CrearSnackRequest(
        string Nombre, 
        decimal Precio, 
        string DescripcionUnidad, 
        int StockInicial);
}
