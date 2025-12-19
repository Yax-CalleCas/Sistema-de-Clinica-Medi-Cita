using MediCita.Web.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediCita.Web.Servicios.Contrato
{
    public interface IReporte
    {
        Task<List<ReporteCita>> ReporteCitas(DateTime fechaInicio, DateTime fechaFin);
        Task<List<ReporteVenta>> ReporteVentas(DateTime fechaInicio, DateTime fechaFin);
    }
}
    