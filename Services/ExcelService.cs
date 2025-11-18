using ClosedXML.Excel;
using Lab08_JeanLazarinos.DTOs;
using System.IO;

namespace Lab08_JeanLazarinos.Services
{
    public class ExcelService : IExcelService
    {
        // --- Reporte 1: Ventas por Cliente (CORREGIDO) ---
        public byte[] GenerateVentasPorClienteReport(IEnumerable<VentasPorClienteDto> ventas)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("VentasPorCliente");
                var currentRow = 1;

                // Encabezados
                worksheet.Cell(currentRow, 1).Value = "ID Cliente";
                worksheet.Cell(currentRow, 2).Value = "Nombre Cliente";
                worksheet.Cell(currentRow, 3).Value = "Email Cliente";
                worksheet.Cell(currentRow, 4).Value = "Total Ventas";
                worksheet.Range(currentRow, 1, currentRow, 4).Style.Font.SetBold(true);

                // Datos
                foreach (var item in ventas)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.ClientId;
                    worksheet.Cell(currentRow, 2).Value = item.ClientName;
                    worksheet.Cell(currentRow, 3).Value = item.ClientEmail; // CORREGIDO
                    worksheet.Cell(currentRow, 4).Value = item.TotalVentas; // CORREGIDO
                }

                worksheet.Column(4).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        // --- Reporte 2: Detalle de un Pedido (CORREGIDO) ---
        public byte[] GeneratePedidoDetalladoReport(PedidoConDetallesDto pedido)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DetallePedido");

                // Información del Pedido
                worksheet.Cell(1, 1).Value = "ID Pedido:";
                worksheet.Cell(1, 2).Value = pedido.OrderId;
                worksheet.Cell(2, 1).Value = "Cliente:";
                worksheet.Cell(2, 2).Value = pedido.Cliente.Name; // CORREGIDO (objeto anidado)
                worksheet.Cell(3, 1).Value = "Fecha:";
                worksheet.Cell(3, 2).Value = pedido.OrderDate;
                worksheet.Cell(3, 2).Style.DateFormat.Format = "dd/MM/yyyy";
                
                // Encabezados de Detalles
                var currentRow = 5;
                worksheet.Cell(currentRow, 1).Value = "Producto";
                worksheet.Cell(currentRow, 2).Value = "Precio Unitario";
                worksheet.Cell(currentRow, 3).Value = "Cantidad";
                worksheet.Cell(currentRow, 4).Value = "Subtotal"; // CORREGIDO
                worksheet.Range(currentRow, 1, currentRow, 4).Style.Font.SetBold(true);

                // Datos de Detalles
                foreach (var item in pedido.Detalles)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.ProductName; // CORREGIDO
                    worksheet.Cell(currentRow, 2).Value = item.ProductPrice; // CORREGIDO
                    worksheet.Cell(currentRow, 3).Value = item.Quantity;
                    worksheet.Cell(currentRow, 4).Value = item.Subtotal; // CORREGIDO
                }
                
                // Formato de moneda
                worksheet.Range(6, 2, currentRow, 2).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Range(6, 4, currentRow, 4).Style.NumberFormat.Format = "$#,##0.00";
                
                // Total General
                currentRow += 2;
                worksheet.Cell(currentRow, 3).Value = "Total General:";
                worksheet.Cell(currentRow, 4).Value = pedido.TotalPedido; // CORREGIDO
                worksheet.Range(currentRow, 3, currentRow, 4).Style.Font.SetBold(true);
                worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "$#,##0.00";

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}