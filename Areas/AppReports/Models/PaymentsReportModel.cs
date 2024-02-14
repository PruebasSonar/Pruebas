using System.ComponentModel;

namespace PIPMUNI_ARG.Areas.AppReports.Models
{
    public class PaymentsReportModel
    {
        [DisplayName("Id Obra")]
        public string contractCode { get; set; } = string.Empty;
        [DisplayName("Nombre Obra")]
        public string contractName { get; set; } = string.Empty;
        [DisplayName("Id Certificado")]
        public string paymentCode { get; set; } = string.Empty;
        [DisplayName("Expediente Certificado")]
        public string paymentRecord { get; set; } = string.Empty;
        [DisplayName("Estado Certificado")]
        public string stageName { get; set; } = string.Empty;
        [DisplayName("Tipo Certificado")]
        public string typeName { get; set; } = string.Empty;
        [DisplayName("Periodo de Medicion")]
        public DateTime? reportedMonth { get; set; } = null;
        [DisplayName("Monto Medicion")]
        public double? paymentValue { get; set; } = null;
        [DisplayName("Avance Fisico")]
        public float? PhysicalAdvance { get; set; } = null;
        [DisplayName("Monto Total")]
        public double? totalValue { get; set; } = null;
        [DisplayName("Fecha de Presentacion")]
        public DateTime? DateDelivery { get; set; } = null;
        [DisplayName("Fecha Devengado")]
        public DateTime? DateApproved { get; set; } = null;
        [DisplayName("Fecha Pagado")]
        public DateTime? DatePayed { get; set; } = null;
        [DisplayName("Adjunta Certificado de Medicion (SI/NO)")]
        public string attachedMedicion { get; set; } = string.Empty;
        [DisplayName("Adjunta Orden de Pago (SI/NO)")]
        public string attachedOrden { get; set; } = string.Empty;
        [DisplayName("Adjunta Otros (SI/NO)")]
        public string otherAttachments { get; set; } = string.Empty;
        [DisplayName("Demora Certificado")]
        public double? PaymentDelay { get; set; } = null;


    }
}
