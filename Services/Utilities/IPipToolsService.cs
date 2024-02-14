using Microsoft.AspNetCore.Mvc.Rendering;
using PIPMUNI_ARG.Models.Domain;

namespace PIPMUNI_ARG.Services.Utilities
{
    public interface IPipToolsService
    {
        #region Dates and Fiscal Year
        //======================================================================
        //
        // Dates and Fiscal Year
        //
        //----------------------------------------------------------------------

        //==============
        // Dates

        public DateTime? minDate(DateTime? date1, DateTime? date2);

        public DateTime? maxDate(DateTime? date1, DateTime? date2);

        public string monthRangeName(DateTime startDate, DateTime endDate);

        public int numberOfMonths(DateTime startDate, DateTime endDate);


        //==============
        // Fiscal Year

        public int? fiscalYearFor(DateTime? date);

        public DateTime firstDateOfFiscalYear(int fiscalYear);

        public DateTime lastDateOfFiscalYear(int fiscalYear);

        public bool inFiscalYear(DateTime date, int fiscalYear);


        /// <summary>
        /// Indicates if a date is within a date range.
        /// </summary>
        /// <param name="date">Date to be validated</param>
        /// <param name="startDate">First date in range</param>
        /// <param name="endDate">Last date in range</param>
        /// <returns></returns>
        public bool inDateRange(DateTime date, DateTime startDate, DateTime endDate);


        //=========================
        // Fiscal Year -> Time periods
        public DateTime startOfQuarter(int fiscalYear, int quarter);
        public DateTime endOfQuarter(int fiscalYear, int quarter);
        public string quarterMonths(int fiscalYear, int quarter);

        #endregion
        #region Contracts Dates
        //==============================================================================
        //
        //                          CONTRACTS
        //
        //------------------------------------------------------------------------------

        //---------------------------- Project related ---------------------------------
        //======================
        // Fiscal Year

        int? startFiscalYear(List<Contract> contracts);
        int? endFiscalYear(List<Contract> contracts);
        List<int>? getFiscalYears(List<Contract> contracts);

        int? setFiscalYearsViewBag(List<Contract> contracts, dynamic ViewBag);

        DateTime earliestDateOfProjectInFiscalYear(List<Contract> contracts, int fiscalYear);
        DateTime latestDateOfProjectInFiscalYear(List<Contract> contracts, int fiscalYear);


        bool isProjectInFiscalYear(List<Contract> contracts, int fiscalYear);


        //======================
        // Start and Completion dates

        int? revisedDuration(Contract contract);
        DateTime? startDateOf(Contract contract);
        DateTime? originalCompletionDate(Contract? contract);
        DateTime? revisedCompletionDate(Contract? contract);

        DateTime? startDateOf(List<Contract> contracts);
        DateTime? originalCompletionDate(List<Contract> contracts);
        DateTime? revisedCompletionDate(List<Contract> contracts);


        #endregion

    }
}
