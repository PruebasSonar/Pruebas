using Microsoft.EntityFrameworkCore;
using PIPMUNI_ARG.Models.Reports;

namespace PIPMUNI_ARG.Areas.Review.Models
{
    [Keyless]
    public class DashboardViewModel
    {
        public int Office { get; set; }
        public int Sector { get; set; }
        public int Subsector { get; set; }

        public List<DashboardInfo> DashboardProjectsInfo { get; set; } = new List<DashboardInfo>();
        public List<ItemQty> SectorsQty { get; set; } = new List<ItemQty>();

        public DashboardInfo Total { get; set; }

        public DashboardInfo totals(DashboardViewModel? model)
        {
            var temList = DashboardProjectsInfo;
            if (model?.Office > 0)
                temList = temList.Where(x => x.Office == model.Office).ToList();
            if (model?.Sector > 0)
                temList = temList.Where(x => x.Sector == model.Sector).ToList();
            if (model?.Subsector > 0)
                temList = temList.Where(x => x.Subsector == model.Subsector).ToList();
            this.Total = new DashboardInfo
            {
                Office = temList.Sum(x => x.Office),
                Sector = temList.Sum(x => x.Sector),
                Subsector = temList.Sum(x => x.Subsector),
                ProjectsWithoutContracts = temList.Sum(x => x.ProjectsWithoutContracts),
                ProjectsWithoutContractsValue = temList.Sum(x => x.ProjectsWithoutContractsValue),

                OngoingContractsCount = temList.Sum(x => x.OngoingContractsCount),
                OngoingContractsCost = temList.Sum(x => x.OngoingContractsCost),
                FinishedContractsCount = temList.Sum(x => x.FinishedContractsCount),
                FinishedContractsCost = temList.Sum(x => x.FinishedContractsCost),
                ToStartContractsCount = temList.Sum(x => x.ToStartContractsCount),
                CancelledContractsCount = temList.Sum(x => x.CancelledContractsCount),
                TotalCostOfAllContracts = temList.Sum(x => x.TotalCostOfAllContracts),
                TotalPayedValue = temList.Sum(x => x.TotalPayedValue),
                TotalRemainingValue = temList.Sum(x => x.TotalRemainingValue),

                PendingPaymentsCount = temList.Sum(x => x.PendingPaymentsCount),
                PendingPaymentsValue = temList.Sum(x => x.PendingPaymentsValue),
                PendingAdditionsCount = temList.Sum(x => x.PendingAdditionsCount),
                PendingAdditionsValue = temList.Sum(x => x.PendingAdditionsValue),
                PendingVariantesCount = temList.Sum(x => x.PendingVariantesCount),
                PendingVariantesValue = temList.Sum(x => x.PendingVariantesValue),
                PendingExtensionsCount = temList.Sum(x => x.PendingExtensionsCount),

                ContractsEndedCurrentYear = temList.Sum(x => x.ContractsEndedCurrentYear),
                ContractsEndedLastYear = temList.Sum(x => x.ContractsEndedLastYear),
                ContractsEndedTwoYearsAgo = temList.Sum(x => x.ContractsEndedTwoYearsAgo),
                ContractsEndedThreeYearsAgo = temList.Sum(x => x.ContractsEndedThreeYearsAgo),
                ContractsNotFinishedPlannedToEndInFuture = temList.Sum(x => x.ContractsNotFinishedPlannedToEndInFuture),
                ContractsNoEndDateOrPlannedEndDate = temList.Sum(x => x.ContractsNoEndDateOrPlannedEndDate),
                ProjectsNoAdvance = temList.Sum(x => x.ProjectsNoAdvance),
                ProjectsAdvance0to25 = temList.Sum(x => x.ProjectsAdvance0to25),
                ProjectsAdvance25to50 = temList.Sum(x => x.ProjectsAdvance25to50),
                ProjectsAdvance50to75 = temList.Sum(x => x.ProjectsAdvance50to75),
                ProjectsAdvance75to100 = temList.Sum(x => x.ProjectsAdvance75to100),

                ContractsStageAIniciar = temList.Sum(x => x.ContractsStageAIniciar),
                ContractsStageEnEjecucion = temList.Sum(x => x.ContractsStageEnEjecucion),
                ContractsStageFinalizada = temList.Sum(x => x.ContractsStageFinalizada),
                ContractsStageRescindida = temList.Sum(x => x.ContractsStageRescindida),
                LocationInfo = temList.FirstOrDefault()?.LocationInfo
            };
            return Total;
        }
    }


}
