using PIPMUNI_ARG.Models.Domain;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace PIPMUNI_ARG.Areas.Review.Models
{
    public class ContractSummaryModel
    {
        public int ContractId { get; set; }

        [DisplayName("expediente")]
        public string record { get; set; } = string.Empty;

        [DisplayName("Obra/Contrato")]
        public string name { get; set; } = string.Empty;

        [DisplayName("Monto Original")]
        public double? originalValue { get; set; }

        [DisplayName("Monto Actual")]
        public double? programmedValue { get; set; }

        [DisplayName("Monto Pagado")]
        public double? actualValue { get; set; }
        [DisplayName("% Avance Físico")]
        public double? advanced { get; set; }
    }
}
