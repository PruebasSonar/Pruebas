using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Models.Domain
{
    [DisplayName("Subsector")]
    public class Subsector
    {

        [Key]
        [DisplayName("id")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public int Id  { get; set; }

        [DisplayName("Sector")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccionar {0}")]
        public int Sector { get; set; }

        [DisplayName("Nombre")]
        [StringLength(100)]
        public string? Name  { get; set; }

        // linked fields
        [ForeignKey("Sector")]
        virtual public Sector? Sector_info { get; set; }
    }
}
