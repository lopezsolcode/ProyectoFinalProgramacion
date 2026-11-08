using System;
using System.Collections.Generic;
using System.Text;

namespace Cinemax.Shared.DTO.Combo.Request
{
    public record ActualizarComboRequest(
        string Nombre, 
        decimal Precio, 
        List<ComboItemRequest> Items);
}
