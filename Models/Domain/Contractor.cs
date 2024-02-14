using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Models.Domain
{
    [DisplayName("Contratista")]
    public class Contractor : IValidatableObject
    {

        [Key]
        [DisplayName("id")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public int Id  { get; set; }

        [DisplayName("CUIT")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [StringLength(25)]
        public string OfficialID  { get; set; } = string.Empty;

        [DisplayName("Nombre")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [StringLength(100)]
        public string Name  { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            // Minimum Length for OfficialID
            if (OfficialID != null && OfficialID.Length < 13)
            {
                yield return new ValidationResult(
                    errorMessage: "Longitud de CUIT inválida.",
                    memberNames: new[] { "OfficialID" }
                );
            }

        }
    }
}
