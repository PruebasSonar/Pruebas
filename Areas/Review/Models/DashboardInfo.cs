namespace PIPMUNI_ARG.Areas.Review.Models
{
	public class DashboardInfo
	{
		public int? Office { get; set; }
		public int? Sector { get; set; }
		public int? Subsector { get; set; }
		public int? ProjectsWithoutContracts { get; set; }
		public double? ProjectsWithoutContractsValue { get; set; }
		public int? OngoingContractsCount { get; set; }
		public double? OngoingContractsCost { get; set; }
		public int? FinishedContractsCount { get; set; }
		public double? FinishedContractsCost { get; set; }
		public int? ToStartContractsCount { get; set; }
		public int? CancelledContractsCount { get; set; }
		public double? TotalCostOfAllContracts { get; set; }
		public double? TotalPayedValue { get; set; }
		public double? TotalRemainingValue { get; set; }

        public int? PendingPaymentsCount { get; set; }
        public double? PendingPaymentsValue { get; set; }
        public int? PendingAdditionsCount { get; set; }
        public double? PendingAdditionsValue { get; set; }
        public int? PendingVariantesCount { get; set; }
        public double? PendingVariantesValue { get; set; }
        public int? PendingExtensionsCount { get; set; }


        public int? ContractsEndedCurrentYear { get; set; }
		public int? ContractsEndedLastYear { get; set; }
		public int? ContractsEndedTwoYearsAgo { get; set; }
		public int? ContractsEndedThreeYearsAgo { get; set; }
		public int? ContractsNotFinishedPlannedToEndInFuture { get; set; }
		public int? ContractsNoEndDateOrPlannedEndDate { get; set; }
		public int? ProjectsNoAdvance { get; set; }
		public int? ProjectsAdvance0to25 { get; set; }
		public int? ProjectsAdvance25to50 { get; set; }
		public int? ProjectsAdvance50to75 { get; set; }
		public int? ProjectsAdvance75to100 { get; set; }

        public int? ContractsStageAIniciar { get; set; }
        public int? ContractsStageEnEjecucion { get; set; }
        public int? ContractsStageFinalizada { get; set; }
        public int? ContractsStageRescindida { get; set; }

		public string? LocationInfo { get; set; }
    }
}
