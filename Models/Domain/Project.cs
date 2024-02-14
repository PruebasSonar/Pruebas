using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Models.Domain
{
    [DisplayName("Proyecto")]
    public class Project : IValidatableObject
    {

        [Key]
        [DisplayName("id")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public int Id  { get; set; }

        [DisplayName("Nombre")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [StringLength(250)]
        public string Name  { get; set; } = string.Empty;

        [DisplayName("Código interno")]
        [StringLength(20)]
        public string? Code  { get; set; }

        [DisplayName("Sector")]
        public int? Sector { get; set; }

        [DisplayName("Subsector")]
        public int? Subsector { get; set; }

        [DisplayName("Etapa")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccionar {0}")]
        public int Stage { get; set; }

        [DisplayName("Área responsable proyecto")]
        public int? Office { get; set; }

        [DisplayName("Descripción")]
        public string? Description  { get; set; }

        [DisplayName("Objetivos")]
        public string? Objectives  { get; set; }

        [DisplayName("Costo estimado")]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [Range(double.MinValue, double.MaxValue, ErrorMessage = "{0} debe ser un número válido.")]
        public double? Cost  { get; set; }

        [DisplayName("Fecha de estimación")]
        [DataType(DataType.Date)]
        [DateRange]
        public DateTime? CostDate  { get; set; }

        [DisplayName("Ubicación")]
        public string? Location  { get; set; }



        // linked fields
        [ForeignKey("Sector")]
        virtual public Sector? Sector_info { get; set; }
        [ForeignKey("Subsector")]
        virtual public Subsector? Subsector_info { get; set; }
        [ForeignKey("Stage")]
        virtual public ProjectStage? Stage_info { get; set; }
        [ForeignKey("Office")]
        virtual public Office? Office_info { get; set; }

// children
        [DisplayName("Anexo Proyectos")]
        public ICollection<ProjectAttachment>? ProjectAttachments  { get; set; }
        [DisplayName("Fuente Financiamientos")]
        public ICollection<ProjectSource>? ProjectSources  { get; set; }
        [DisplayName("Imagenes")]
        public ICollection<ProjectImage>? ProjectImages  { get; set; }
        [DisplayName("Videos")]
        public ICollection<ProjectVideo>? ProjectVideos  { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CostDate > DateTime.Now)
            {
                yield return new ValidationResult(
                    errorMessage: "Fecha de estimación no admite fechas futuras",
                    memberNames: new[] { "CostDate" }
               );
            }
        }
    }
}
