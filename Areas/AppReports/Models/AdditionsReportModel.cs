using System.ComponentModel;

namespace PIPMUNI_ARG.Areas.AppReports.Models
{
    public class AdditionsReportModel
    {

        [DisplayName("Id Obra")]
        public string contractCode { get; set; } = string.Empty;
        [DisplayName("Nombre Obra")]
        public string contractName { get; set; } = string.Empty;
        [DisplayName("Id Certificado")]
        public string additionCode { get; set; } = string.Empty;
        [DisplayName("Expediente Certificado")]
        public string additionRecord { get; set; } = string.Empty;
        [DisplayName("Estado Certificado")]
        public string stageName { get; set; } = string.Empty;

        [DisplayName("Periodo de Salto")]
        public DateTime? period { get; set; } = null;

        [DisplayName("Monto Redeterminado")]
        public double? additionValue { get; set; } = null;
        [DisplayName("Fecha de Presentacion")]
        public DateTime? DateDelivery { get; set; } = null;
        [DisplayName("Fecha Devengado")]
        public DateTime? DateApproved { get; set; } = null;
        [DisplayName("Adjunta Convenio/Acta Redeterminacion (SI/NO)")]
        public string attached { get; set; } = string.Empty;
        [DisplayName("Adjunta Otra Documentacion (SI/NO)")]
        public string otherAttachments { get; set; } = string.Empty;
        [DisplayName("Demora Redeterminacion")]
        public int? AdditionDelay { get; set; } = null;


    }
}
