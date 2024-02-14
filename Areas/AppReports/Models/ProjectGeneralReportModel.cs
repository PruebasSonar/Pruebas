using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace PIPMUNI_ARG.Areas.AppReports.Models
{
    public class ProjectGeneralReportModel
    {
        public int id_project { get; set; } = 0;
        [DisplayName("Código")]
        public string code_project { get; set; } = string.Empty;
        [DisplayName("Proyecto")]
        public string name_project { get; set; } = string.Empty;
        [DisplayName("Oficina Responsable")]
        public string name_office { get; set; } = string.Empty;
        [DisplayName("Grupo")]
        public string name_sector { get; set; } = string.Empty;
        [DisplayName("Subgrupo")]
        public string name_subsector { get; set; } = string.Empty;
        [DisplayName("Etapa")]
        public string name_projectStage { get; set; } = string.Empty;
    }
}
