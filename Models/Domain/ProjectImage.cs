using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Models.Domain
{
    [DisplayName("Imagen")]
    public class ProjectImage : IValidatableObject
    {

        [Key]
        [DisplayName("id")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public int Id  { get; set; }

        [DisplayName("Proyecto")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccionar {0}")]
        public int Project { get; set; }

        [DisplayName("Archivo")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public string File  { get; set; } = string.Empty;

        [DisplayName("Descripción")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public string Description  { get; set; } = string.Empty;

        [DisplayName("Fecha")]
        [DataType(DataType.Date)]
        [DateRange]
        public DateTime? ImageDate  { get; set; }

        [DisplayName("Fecha de cargue")]
        [DataType(DataType.Date)]
        [DateRange]
        public DateTime? UploadDate  { get; set; }

        // linked fields
        [ForeignKey("Project")]
        virtual public Project? Project_info { get; set; }

        // View fields for file upload (input type="file"
        [NotMapped]
        public IFormFile? FileInput { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ImageDate > DateTime.Now)
            {
                yield return new ValidationResult(
                    errorMessage: "Fecha no admite fechas futuras",
                    memberNames: new[] { "ImageDate" }
               );
            }
        }
    }
}
