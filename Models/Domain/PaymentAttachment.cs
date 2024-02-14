using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Models.Domain
{
    [DisplayName("Anexo Certificado")]
    public class PaymentAttachment
    {

        [Key]
        [DisplayName("id")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public int Id  { get; set; }

        [DisplayName("Certificado")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccionar {0}")]
        public int Payment { get; set; }

        [DisplayName("Título")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [StringLength(100)]
        public string Title  { get; set; } = string.Empty;

        [DisplayName("Archivo")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public string File  { get; set; } = string.Empty;

        [DisplayName("Fecha de carga")]
        [DataType(DataType.Date)]
        [DateRange]
        public DateTime? DateAttached  { get; set; }

        // linked fields
        [ForeignKey("Payment")]
        virtual public Payment? Payment_info { get; set; }

        // View fields for file upload (input type="file"
        [NotMapped]
        public IFormFile? FileInput { get; set; }
    }
}
