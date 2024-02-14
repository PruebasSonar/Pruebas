using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Models.Domain
{
    [DisplayName("Fuente Financiamiento")]
    public class ProjectSource
    {

        [Key]
        [DisplayName("id")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public int Id  { get; set; }

        [DisplayName("Proyecto")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccionar {0}")]
        public int Project { get; set; }

        [DisplayName("Fuente")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccionar {0}")]
        public int Source { get; set; }

        [DisplayName("Porcentaje")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [Range(0, 100, ErrorMessage = "{0} debe ser un número entre 0 y 100.")]
        public float? Percentage  { get; set; }
        // linked fields
        [ForeignKey("Project")]
        virtual public Project? Project_info { get; set; }
        [ForeignKey("Source")]
        virtual public FundingSource? Source_info { get; set; }
    }
}
