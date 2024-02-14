using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Models.Domain
{
    [DisplayName("Organísmos de Financiamiento")]
    public class FundingSource
    {

        [Key]
        [DisplayName("id")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public int Id  { get; set; }

        [DisplayName("Nombre")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [StringLength(150)]
        public string Name  { get; set; } = string.Empty;

        [DisplayName("Sigla")]
        [StringLength(20)]
        public string? Acronym  { get; set; }
    }
}
