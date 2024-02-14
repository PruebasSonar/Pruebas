using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Models.Domain
{
    [DisplayName("Anexo Proyecto")]
    public class ProjectAttachment
    {

        [Key]
        [DisplayName("id")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public int Id  { get; set; }

        [DisplayName("Proyecto")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccionar {0}")]
        public int Project { get; set; }

        [DisplayName("Título")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [StringLength(100)]
        public string Title  { get; set; } = string.Empty;

        [DisplayName("Archivo")]
        public string? FileName  { get; set; }

        [DisplayName("Fecha de carga")]
        [DataType(DataType.Date)]
        [DateRange]
        public DateTime? DateAttached  { get; set; }

        // linked fields
        [ForeignKey("Project")]
        virtual public Project? Project_info { get; set; }

        // View fields for file upload (input type="file"
        [NotMapped]
        public IFormFile? FileNameInput { get; set; }
    }
}
