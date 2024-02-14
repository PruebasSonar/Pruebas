using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Models.Domain
{
    [DisplayName("Certificado")]
    public class Payment : IValidatableObject
    {

        [Key]
        [DisplayName("id")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public int Id  { get; set; }

        [DisplayName("Contrato")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public int Contract { get; set; }

        [DisplayName("Código interno")]
        [StringLength(15)]
        public string? Code  { get; set; }

        [DisplayName("Tipo")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccionar {0}")]
        public int Type { get; set; }

        [DisplayName("Número")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [Range(int.MinValue, int.MaxValue, ErrorMessage = "{0} debe ser un número válido.")]
        public int? Number  { get; set; }

        [DisplayName("Periodo medición")]
        [DataType(DataType.Date)]
        [DateRange]
        public DateTime? ReportedMonth  { get; set; }

        [DisplayName("Monto")]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [Range(double.MinValue, double.MaxValue, ErrorMessage = "{0} debe ser un número válido.")]
        public double? Value  { get; set; }

        [DisplayName("Diferencia redeterminada")]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [Range(double.MinValue, double.MaxValue, ErrorMessage = "{0} debe ser un número válido.")]
        public double? Diferencia  { get; set; }

        [DisplayName("Descuento anticipo financiero")]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [Range(double.MinValue, double.MaxValue, ErrorMessage = "{0} debe ser un número válido.")]
        public double? DescuentoFinanciero  { get; set; }

        [DisplayName("Otros descuentos")]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [Range(double.MinValue, double.MaxValue, ErrorMessage = "{0} debe ser un número válido.")]
        public double? DescuentoOtros  { get; set; }

        [DisplayName("Avance físico (%)")]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [Range(0, 100, ErrorMessage = "{0} debe ser un número entre 0 y 100.")]
        public float? PhysicalAdvance  { get; set; }

        [DisplayName("Importe Final")]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [Range(double.MinValue, double.MaxValue, ErrorMessage = "{0} debe ser un número válido.")]
        public double? Total  { get; set; }

        [DisplayName("Fecha presentación")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [DataType(DataType.Date)]
        [DateRange]
        public DateTime? DateDelivery  { get; set; }
        [DisplayName("Fecha aprobación")]
        [DataType(DataType.Date)]
        [DateRange]
        [NotSmallerThan("DateDelivery")]
        [NotGreaterThan("DatePayed")]
        public DateTime? DateApproved  { get; set; }

        [DisplayName("Fecha pagado")]
        [DataType(DataType.Date)]
        [DateRange]
        [NotSmallerThan("DateDelivery")]
        public DateTime? DatePayed  { get; set; }

        [DisplayName("# de expediente")]
        [StringLength(50)]
        public string? Record  { get; set; }

        [DisplayName("Estado")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccionar {0}")]
        public int Stage { get; set; }

        [DisplayName("Certificado")]
        public string? AttachmentMedicion  { get; set; }

        [DisplayName("Orden de pago")]
        public string? AttachmentOrden  { get; set; }


        // linked fields
        [ForeignKey("Contract")]
        virtual public Contract? Contract_info { get; set; }
        [ForeignKey("Type")]
        virtual public PaymentType? Type_info { get; set; }
        [ForeignKey("Stage")]
        virtual public PaymentStage? Stage_info { get; set; }

// children
        [DisplayName("Anexo Certificados")]
        public ICollection<PaymentAttachment>? PaymentAttachments  { get; set; }

        // View fields for file upload (input type="file"
        [NotMapped]
        public IFormFile? AttachmentMedicionInput { get; set; }
        [NotMapped]
        public IFormFile? AttachmentOrdenInput { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ReportedMonth > DateTime.Now)
            {
                yield return new ValidationResult(
                    errorMessage: "Periodo medición no admite fechas futuras",
                    memberNames: new[] { "ReportedMonth" }
               );
            }
            if (DateDelivery > DateTime.Now)
            {
                yield return new ValidationResult(
                    errorMessage: "Fecha presentación no admite fechas futuras",
                    memberNames: new[] { "DateDelivery" }
               );
            }
            if (DateApproved > DateTime.Now)
            {
                yield return new ValidationResult(
                    errorMessage: "Fecha aprobación no admite fechas futuras",
                    memberNames: new[] { "DateApproved" }
               );
            }
            if (DatePayed > DateTime.Now)
            {
                yield return new ValidationResult(
                    errorMessage: "Fecha pagado no admite fechas futuras",
                    memberNames: new[] { "DatePayed" }
               );
            }

            // Validaciones fechas y estados
            if ((Stage == ProjectGlobals.PaymentStage.Finalizado) && (DatePayed == null || DatePayed <= ProjectGlobals.MinValidDate))
            {
                yield return new ValidationResult(
                    errorMessage: "Fecha pagado requerida para estado Finalizado.",
                    memberNames: new[] { "DatePayed" }
               );
            }
            if ((Stage < ProjectGlobals.PaymentStage.Finalizado) && (DatePayed != null && DatePayed > ProjectGlobals.MinValidDate))
            {
                yield return new ValidationResult(
                    errorMessage: "Si tiene Fecha pagado, debe pasar a estado Finalizado.",
                    memberNames: new[] { "Stage" }
               );
            }

        }
    }
}
