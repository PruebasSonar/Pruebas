using System.ComponentModel;

namespace PIPMUNI_ARG.Areas.AppReports.Models
{
    public class ContractDelaysModel
    {
        [DisplayName("Código Contrato")]
        public string? ContractCode { get; set; } = null;
        [DisplayName("Contrato")]
        public string? ContractName { get; set; } = null;

        [DisplayName("Cantidad de Certificados Pagados")]
        public int? Payment_QtyPayed { get; set; } = null;
        [DisplayName("Cantidad de Certificados Pendientes")]
        public int? Payment_QtyPending { get; set; } = null;
        [DisplayName("Fecha Ultimo Certificado Pagado")]
        public DateTime? Payment_LastPayedDate { get; set; } = null;
        [DisplayName("Fecha Ultimo Certificado Pendiente")]
        public DateTime? Payment_LastDeliveryDate { get; set; } = null;
        [DisplayName("Demora Ultimo Certificado Pagado")]
        public int? Payment_LastPayedDelay { get; set; } = null;
        [DisplayName("Demora Certificado Pendiente mas antiguo")]
        public int? Payment_OldestDeliveryDelay { get; set; } = null;


        [DisplayName("Cantidad de Redeterminaciones Aprobadas")]
        public int? Addition_QtyApproved { get; set; } = null;
        [DisplayName("Cantidad de Redeterminaciones Pendientes")]
        public int? Addition_QtyPending { get; set; } = null;
        [DisplayName("Ultimo Periodo de Salto Aprobado")]
        public DateTime? Addition_LastApprovedPeriod { get; set; } = null;
        [DisplayName("Demora Ultima Redeterminacion Aprobada")]
        public int? Addition_LastApprovedDelay { get; set; } = null;
        [DisplayName("Demora Ultima Redeterminacion Pendiente")]
        public int? Addition_OldestDeliveryDelay { get; set; } = null;


        [DisplayName("Cantidad de Variantes aprobadas")]
        public int? Variante_QtyApproved { get; set; } = null;
        [DisplayName("Cantidad de Variantes Pendientes")]
        public int? Variante_QtyPending { get; set; } = null;
        [DisplayName("Fecha Ultima Variante Aprobada")]
        public DateTime? Variante_LastApprovedDate { get; set; } = null;
        [DisplayName("Demora Ultima Variante Aprobada")]
        public int? Variante_LastApprovedDelay { get; set; } = null;
        [DisplayName("Demora  Variante Pendiente mas antigua")]
        public int? Variante_OldestDeliveryDelay { get; set; } = null;




        [DisplayName("Cantidad de Ampliaciones Aprobadas")]
        public int? Extension_QtyApproved { get; set; } = null;
        [DisplayName("Cantidad de Ampliaciones Pendientes")]
        public int? Extension_QtyPending { get; set; } = null;
        [DisplayName("Fecha Ultima Ampliacion Aprobada")]
        public DateTime? Extension_LastApprovedDate { get; set; } = null;
        [DisplayName("Demora Ultima Ampliacion Aprobada")]
        public int? Extension_LastApprovedDelay { get; set; } = null;
        [DisplayName("Demora  Ampliacion Pendiente mas antigua")]
        public int? Extension_OldestDeliveryDelay { get; set; } = null;

    }
}
