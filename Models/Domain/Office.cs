using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Models.Domain
{
    [DisplayName("Área Responsable")]
    public class Office
    {

        [Key]
        [DisplayName("id")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public int Id  { get; set; }

        [DisplayName("Nombre")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [StringLength(100)]
        public string Name  { get; set; } = string.Empty;
    }
}
