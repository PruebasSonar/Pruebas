using System.ComponentModel;

namespace PIPMUNI_ARG.Areas.AppReports.Models
{
    public class ExtensionsReportModel
    {

        [DisplayName("Id Obra")]
        public string contractCode { get; set; } = string.Empty;
        [DisplayName("Nombre Obra")]
        public string contractName { get; set; } = string.Empty;
        [DisplayName("Id Ampliación")]
        public string extensionCode { get; set; } = string.Empty;
        [DisplayName("Expediente Ampliación")]
        public string extensionRecord { get; set; } = string.Empty;
        [DisplayName("Estado Ampliación")]
        public string stageName { get; set; } = string.Empty;
        [DisplayName("Días de Ampliación")]
        public int? days { get; set; } = null;
        [DisplayName("Fecha de Presentacion")]
        public DateTime? DateDelivery { get; set; } = null;
        [DisplayName("Fecha Devengado")]
        public DateTime? DateApproved { get; set; } = null;
        [DisplayName("Adjunta Convenio/Acta Redeterminacion (SI/NO)")]
        public string attached { get; set; } = string.Empty;
        [DisplayName("Adjunta Otra Documentacion (SI/NO)")]
        public string otherAttachments { get; set; } = string.Empty;
        [DisplayName("Observaciones")]
        public string motivo { get; set; } = string.Empty;
        [DisplayName("Demora Redeterminacion")]
        public int? extensionDelay { get; set; } = null;


    }
}
