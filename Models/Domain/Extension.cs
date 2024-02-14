using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Models.Domain
{
    [DisplayName("Ampliación de Plazo")]
    public class Extension : IValidatableObject
    {

        [Key]
        [DisplayName("id")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public int Id  { get; set; }

        [DisplayName("Obra/Contrato")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public int Contract { get; set; }

        [DisplayName("Código interno")]
        [StringLength(15)]
        public string? Code  { get; set; }

        [DisplayName("Días de ampliación")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [Range(int.MinValue, int.MaxValue, ErrorMessage = "{0} debe ser un número válido.")]
        public int? Days  { get; set; }
        [DisplayName("Observación")]
        [StringLength(250)]
        public string? Motive  { get; set; }

        [DisplayName("Fecha presentación")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [DataType(DataType.Date)]
        [DateRange]
        public DateTime? DateDelivery  { get; set; }
        [DisplayName("Fecha aprobación")]
        [DataType(DataType.Date)]
        [DateRange]
        public DateTime? DateApproved  { get; set; }

        [DisplayName("Expediente")]
        [StringLength(50)]
        public string? Record  { get; set; }

        [DisplayName("Estado")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccionar {0}")]
        public int Stage { get; set; }

        [DisplayName("Acto de aprobación")]
        public string? Attachment  { get; set; }


        // linked fields
        [ForeignKey("Contract")]
        virtual public Contract? Contract_info { get; set; }
        [ForeignKey("Stage")]
        virtual public AdditionStage? Stage_info { get; set; }

// children
        [DisplayName("Anexo Ampliaciones")]
        public ICollection<ExtensionAttachment>? ExtensionAttachments  { get; set; }

        // View fields for file upload (input type="file"
        [NotMapped]
        public IFormFile? AttachmentInput { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
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

            // Validaciones fechas y estados
            if ((Stage == ProjectGlobals.AdditionStage.Aprobada) && (DateApproved == null || DateApproved <= ProjectGlobals.MinValidDate))
                        {
                            yield return new ValidationResult(
                                errorMessage: "Fecha Aprobado requerida para estado Aprobada.",
                                memberNames: new[] { "DateApproved" }
                           );
                        }
            else
                        if ((Stage < ProjectGlobals.AdditionStage.Aprobada) && (DateApproved != null && DateApproved > ProjectGlobals.MinValidDate))
                        {
                            yield return new ValidationResult(
                                errorMessage: "Si tiene fecha Aprobada, debe pasar a Estado Aprobada.",
                                memberNames: new[] { "Stage" }
                           );
                        }

        }
    }
}
