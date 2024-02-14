using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Areas.AppReports.Models
{
    public class ContractGeneralReportModel
    {
        [DisplayName("Proyecto")]
        public string projectCode { get; set; } = string.Empty;
        [DisplayName("Código Obra")]
        public string Code { get; set; } = string.Empty;
        [DisplayName("Obra")]
        public string name { get; set; } = string.Empty;
        [DisplayName("Área Responsable")]
        public string office { get; set; } = string.Empty;
        [DisplayName("Sector")]
        public string Sector { get; set; } = string.Empty;
        [DisplayName("Subsector")]
        public string subsector { get; set; } = string.Empty;
        [DisplayName("Etapa")]
        public string stage { get; set; } = string.Empty;


        [DisplayName("Monto Original")]
        public double? originalValue { get; set; } = null;
        [DisplayName("Monto Actualizado")]
        public double? programado { get; set; } = null;
        [DisplayName("Monto Pagado")]
        public double? pagado { get; set; } = null;


        [DisplayName("% avance físico")]
        public float? porcentajeFisico { get; set; } = null;
        [DisplayName("% avance financiero")]
        public float? porcentajePagado { get; set; } = null;


        [DisplayName("Saldo a Pagar")]
        public double? saldo { get; set; } = null;

        [DisplayName("Saldo por Certificar")]
        public double? certificadosPendientes { get; set; } = null;


        [DisplayName("Plazo Original")]
        public int? plazoOriginal { get; set; } = null;
        [DisplayName("Plazo Ampliado")]
        public int? plazoAmpliado { get; set; } = null;


        [DisplayName("Contratista")]
        public string Contractor { get; set; } = string.Empty;



        [DisplayName("Firma Contrato")]
        public DateTime? ContractSigned { get; set; } = null;
        [DisplayName("Inicio Estimado")]
        public DateTime? PlannedStartDate { get; set; } = null;
        [DisplayName("Fin Estimado")]
        public DateTime? PlannedEndDate { get; set; } = null;
        [DisplayName("Acta de Inicio")]
        public DateTime? StartDate { get; set; } = null;


    }
}
