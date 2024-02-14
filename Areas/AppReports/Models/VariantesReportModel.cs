using System.ComponentModel;

namespace PIPMUNI_ARG.Areas.AppReports.Models
{
    public class VariantesReportModel
    {

        [DisplayName("Id Obra")]
        public string contractCode { get; set; } = string.Empty;
        [DisplayName("Nombre Obra")]
        public string contractName { get; set; } = string.Empty;
        [DisplayName("Id Variante")]
        public string varianteCode { get; set; } = string.Empty;
        [DisplayName("Expediente Variante")]
        public string varianteRecord { get; set; } = string.Empty;
        [DisplayName("Tipo Variante")]
        public string motivo { get; set; } = string.Empty;
        [DisplayName("Estado Variante")]
        public string stageName { get; set; } = string.Empty;
        [DisplayName("Monto Redeterminado")]
        public double? varianteValue { get; set; } = null;
        [DisplayName("Fecha de Presentacion")]
        public DateTime? DateDelivery { get; set; } = null;
        [DisplayName("Fecha Devengado")]
        public DateTime? DateApproved { get; set; } = null;
        [DisplayName("Adjunta Convenio/Acta Redeterminacion (SI/NO)")]
        public string attached { get; set; } = string.Empty;
        [DisplayName("Adjunta Otra Documentacion (SI/NO)")]
        public string otherAttachments { get; set; } = string.Empty;
        [DisplayName("Demora Redeterminacion")]
        public int? VarianteDelay { get; set; } = null;


    }
}
