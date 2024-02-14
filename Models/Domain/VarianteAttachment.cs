using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Models.Domain
{
    [DisplayName("Anexo Variante")]
    public class VarianteAttachment
    {

        [Key]
        [DisplayName("id")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public int Id  { get; set; }

        [DisplayName("Variante")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccionar {0}")]
        public int Variante { get; set; }

        [DisplayName("Título")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [StringLength(100)]
        public string Title  { get; set; } = string.Empty;

        [DisplayName("Archivo")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public string FileName  { get; set; } = string.Empty;

        [DisplayName("Fecha de registro")]
        [DataType(DataType.Date)]
        [DateRange]
        public DateTime? DateAttached  { get; set; }

        // linked fields
        [ForeignKey("Variante")]
        virtual public Variante? Variante_info { get; set; }

        // View fields for file upload (input type="file"
        [NotMapped]
        public IFormFile? FileNameInput { get; set; }
    }
}
