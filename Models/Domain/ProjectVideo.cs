using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Models.Domain
{
    [DisplayName("Video")]
    public class ProjectVideo : IValidatableObject
    {

        [Key]
        [DisplayName("id")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public int Id  { get; set; }

        [DisplayName("Proyecto")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccionar {0}")]
        public int Project { get; set; }

        [DisplayName("Enlace")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [StringLength(255)]
        public string Link  { get; set; } = string.Empty;

        [DisplayName("Descripción")]
        public string? Description  { get; set; }

        [DisplayName("Fecha del video")]
        [DataType(DataType.Date)]
        [DateRange]
        public DateTime? VideoDate  { get; set; }

        [DisplayName("Fecha de cargue")]
        [DataType(DataType.Date)]
        [DateRange]
        public DateTime? UploadDate  { get; set; }

        // linked fields
        [ForeignKey("Project")]
        virtual public Project? Project_info { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (VideoDate > DateTime.Now)
            {
                yield return new ValidationResult(
                    errorMessage: "Fecha del video no admite fechas futuras",
                    memberNames: new[] { "VideoDate" }
               );
            }
        }
    }
}
