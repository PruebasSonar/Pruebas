using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Models.Domain
{
    [DisplayName("Perfil de Usuario")]
    public class UserProfile
    {

        [Key]
        [DisplayName("id")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        public int Id  { get; set; }

        [DisplayName("AspNet User Id")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [StringLength(255)]
        public string AspNetUserId  { get; set; } = string.Empty;

        [DisplayName("Correo electrónico")]
        [StringLength(100)]
        public string? Email  { get; set; }

        [DisplayName("Nombre")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [StringLength(25)]
        public string Name  { get; set; } = string.Empty;

        [DisplayName("Apellido")]
        [Required(ErrorMessage = "{0}: campo obligatorio.")]
        [StringLength(25)]
        public string Surname  { get; set; } = string.Empty;

        [DisplayName("Oficina")]
        public int? Office { get; set; }

        [DisplayName("Notas")]
        public string? Notes  { get; set; }

        // linked fields
        [ForeignKey("Office")]
        virtual public Office? Office_info { get; set; }
    }
}
