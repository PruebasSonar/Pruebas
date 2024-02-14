using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Models.Domain
{
    [DisplayName("Obra")]
    public class Contract : IValidatableObject
    {

        [Key]
        [DisplayName("id")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public int Id  { get; set; }

        [DisplayName("Proyecto")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccionar {0}")]
        public int Project { get; set; }

        [DisplayName("Código interno")]
        [StringLength(15)]
        public string? Code  { get; set; }

        [DisplayName("Nombre o título")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [StringLength(250)]
        public string Name  { get; set; } = string.Empty;

        [DisplayName("Etapa")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccionar {0}")]
        public int Stage { get; set; }

        [DisplayName("Área responsable de ejecución de obra")]
        public int? Office { get; set; }

        [DisplayName("Monto")]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [Range(double.MinValue, double.MaxValue, ErrorMessage = "{0} debe ser un número válido.")]
        public double? OriginalValue  { get; set; }

        [DisplayName("Plazo")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [Range(int.MinValue, int.MaxValue, ErrorMessage = "{0} debe ser un número válido.")]
        public int? PlazoOriginal  { get; set; }

        [DisplayName("Fecha inicio estimada")]
        [DataType(DataType.Date)]
        [DateRange]
        public DateTime? PlannedStartDate  { get; set; }

        [DisplayName("Fecha fin estimada")]
        [DataType(DataType.Date)]
        [DateRange]
        [NotSmallerThan("PlannedStartDate")]
        public DateTime? PlannedEndDate  { get; set; }

        [DisplayName("Modalidad de ejecución")]
        public int? Type { get; set; }

        [DisplayName("Contratista")]
        public int? Contractor { get; set; }

        [DisplayName("Fecha firma contrato")]
        [DataType(DataType.Date)]
        [DateRange]
        public DateTime? ContractDate  { get; set; }

        [DisplayName("Fecha acta inicio")]
        [DataType(DataType.Date)]
        [DateRange]
        public DateTime? StartDate  { get; set; }

        [DisplayName("Fecha de recepción")]
        [DataType(DataType.Date)]
        [DateRange]
        [NotSmallerThan("StartDate")]
        public DateTime? EndDate  { get; set; }

        [DisplayName("Expediente")]
        [StringLength(50)]
        public string? Record  { get; set; }

        // linked fields
        [ForeignKey("Project")]
        virtual public Project? Project_info { get; set; }
        [ForeignKey("Stage")]
        virtual public ContractStage? Stage_info { get; set; }
        [ForeignKey("Office")]
        virtual public Office? Office_info { get; set; }
        [ForeignKey("Type")]
        virtual public ContractType? Type_info { get; set; }
        [ForeignKey("Contractor")]
        virtual public Contractor? Contractor_info { get; set; }

// children
        [DisplayName("Ampliación de Plazos")]
        public ICollection<Extension>? Extensions  { get; set; }
        [DisplayName("Certificados")]
        public ICollection<Payment>? Payments  { get; set; }
        [DisplayName("Redeterminaciones")]
        public ICollection<Addition>? Additions  { get; set; }
        [DisplayName("Variante de Obras")]
        public ICollection<Variante>? Variantes  { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ContractDate > DateTime.Now)
            {
                yield return new ValidationResult(
                    errorMessage: "Fecha firma contrato no admite fechas futuras",
                    memberNames: new[] { "ContractDate" }
               );
            }
            if (StartDate > DateTime.Now)
            {
                yield return new ValidationResult(
                    errorMessage: "Fecha acta inicio no admite fechas futuras",
                    memberNames: new[] { "StartDate" }
               );
            }
            if (EndDate > DateTime.Now)
            {
                yield return new ValidationResult(
                    errorMessage: "Fecha de recepción no admite fechas futuras",
                    memberNames: new[] { "EndDate" }
               );
            }
        }
    }
}
