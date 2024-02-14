using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Areas.AppReports.Models
{
    public class PerformanceReportModel
    {
        public int projectId { get; set; } = 0;
        [DisplayName("Obra/Contrato")]
        public string name { get; set; } = string.Empty;
        [DisplayName("Monto Proyectado")]
        public double? programado { get; set; } = null;
        [DisplayName("Monto Pagado")]
        public double? pagado { get; set; } = null;
        [DisplayName("% avance financiero")]
        public float? porcentajePagado { get; set; } = null;
        [DisplayName("% avance físico")]
        public float? porcentajeFisico { get; set; } = null;
        [DisplayName("Acta de Inicio")]
        public DateTime? fechaInicio { get; set; } = null;
        [DisplayName("Acta de Recepción")]
        public DateTime? fechaFin { get; set; } = null;
        [DisplayName("Fecha Finalización Prevista")]
        public DateTime? fechaProgramada { get; set; } = null;
        [DisplayName("Transcurrido")]
        public int? transcurrido { get; set; } = null;
        [DisplayName("% Tiempo")]
        public float? porcentajeTiempo { get; set; } = null;
    }
}
