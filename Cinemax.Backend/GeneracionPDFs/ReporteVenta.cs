
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Cinemax.Shared.DTO.PDFs.Response;

namespace Cinemax.Backend.GeneracionPDFs
{
    public class ReporteVenta : IDocument
    {
        private readonly ComprobanteVentaResponse _comprobante;
        private readonly CultureInfo _cultura =
            CultureInfo.GetCultureInfo("es-AR");

        public ReporteVenta(ComprobanteVentaResponse comprobante)
        {
            _comprobante = comprobante;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            var venta = _comprobante.Venta;

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                // ENCABEZADO
                page.Header().Column(column =>
                {
                    column.Item()
                        .Text($"Venta #{venta.Id}")
                        .FontSize(20)
                        .Bold();

                    column.Item()
                        .Text($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm} UTC");

                    column.Item()
                        .Text($"Cliente: {venta.ClienteNombre ?? "Consumidor final"}");

                    column.Item()
                        .Text($"Estado: {venta.Estado}");
                });

                page.Content().PaddingVertical(20).Column(column =>
                {
                    // FUNCIÓN Y BUTACAS
                    if (!string.IsNullOrWhiteSpace(venta.FuncionDescripcion))
                    {
                        column.Item()
                            .Text("Entradas")
                            .FontSize(14)
                            .Bold();

                        column.Item()
                            .PaddingTop(5)
                            .Text($"Función: {venta.FuncionDescripcion}");

                        column.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Padding(5)
                                    .Text("Butaca").Bold();

                                header.Cell().Padding(5)
                                    .AlignRight()
                                    .Text("Precio").Bold();
                            });

                            foreach (var butaca in venta.Butacas)
                            {
                                table.Cell().Padding(5)
                                    .Text(butaca.Identificacion);

                                table.Cell().Padding(5)
                                    .AlignRight()
                                    .Text(butaca.Precio.ToString("C2", _cultura));
                            }
                        });
                    }

                    // SNACKS Y COMBOS
                    if (venta.Snacks.Count > 0)
                    {
                        column.Item()
                            .PaddingTop(20)
                            .Text("Snacks y combos")
                            .FontSize(14)
                            .Bold();

                        column.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Padding(5)
                                    .Text("Producto").Bold();

                                header.Cell().Padding(5)
                                    .AlignRight()
                                    .Text("Cantidad").Bold();

                                header.Cell().Padding(5)
                                    .AlignRight()
                                    .Text("Precio").Bold();

                                header.Cell().Padding(5)
                                    .AlignRight()
                                    .Text("Subtotal").Bold();
                            });

                            foreach (var snack in venta.Snacks)
                            {
                                table.Cell().Padding(5)
                                    .Text(snack.ProductoNombre);

                                table.Cell().Padding(5)
                                    .AlignRight()
                                    .Text(snack.Cantidad.ToString());

                                table.Cell().Padding(5)
                                    .AlignRight()
                                    .Text(snack.PrecioUnitario.ToString("C2", _cultura));

                                table.Cell().Padding(5)
                                    .AlignRight()
                                    .Text(snack.Subtotal.ToString("C2", _cultura));
                            }
                        });
                    }

                    // TOTAL
                    column.Item()
                        .PaddingTop(20)
                        .AlignRight()
                        .Text($"Total: {venta.Total.ToString("C2", _cultura)}")
                        .FontSize(16)
                        .Bold();

                    // PAGOS
                    column.Item()
                        .PaddingTop(20)
                        .Text("Pagos")
                        .FontSize(14)
                        .Bold();

                    if (_comprobante.Pagos.Count == 0)
                    {
                        column.Item()
                            .Text("No hay pagos registrados.");
                    }

                    foreach (var pago in _comprobante.Pagos)
                    {
                        column.Item().PaddingTop(10).Column(datos =>
                        {
                            datos.Item().Text(
                                $"Monto: {pago.Monto.ToString("C2", _cultura)}");

                            datos.Item().Text(
                                $"Medio de pago: {pago.Metodo}");

                            datos.Item().Text(
                                $"Referencia: {pago.ReferenciaPago ?? "Sin referencia"}");

                            datos.Item().Text(
                                $"Estado: {pago.Estado}");
                        });
                    }
                });

                // PIE DE PÁGINA
                page.Footer().AlignRight().Text(text =>
                {
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        }
    }
}
