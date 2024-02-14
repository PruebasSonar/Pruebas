using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using PIPMUNI_ARG.Models.Domain;

namespace PIPMUNI_ARG.Areas.Review.Models
{
    [DisplayName("Imagen")]
    public class ProjectImages : ProjectImage
    {

        // View fields for file upload (input type="file"
        [NotMapped]
        public List<IFormFile>? FilesInput { get; set; }

    }
}
