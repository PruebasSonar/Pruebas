using Microsoft.AspNetCore.Mvc.Rendering;
using PIPMUNI_ARG.Models.Domain;

namespace PIPMUNI_ARG.Services.Utilities
{
    public class PipToolsService : IPipToolsService
    {
        const int FiscalYearMonth = 4;
        const int minValidFiscalYear = 1900;

        JaosDataTools dataTools = new JaosDataTools();

        #region Dates and Fiscal Year
        //======================================================================
        //
        // Dates and Fiscal Year
        //
        //----------------------------------------------------------------------

        //==============
        // Dates

        public DateTime? minDate(DateTime? date1, DateTime? date2)
        {
            if (date2.HasValue && date2.Value != DateTime.MinValue)
            {
                if (date1.HasValue && date1.Value != DateTime.MinValue)
                    return date1 < date2 ? date1 : date2;
                else
                    return date2;
            }
            else
                return date1;
        }

        public DateTime? maxDate(DateTime? date1, DateTime? date2)
        {
            if (date2.HasValue && date2.Value != DateTime.MinValue)
            {
                if (date1.HasValue && date1.Value != DateTime.MinValue)
                    return date1 > date2 ? date1 : date2;
                else
                    return date2;
            }
            else
                return date1;
        }

        public string monthRangeName(DateTime startDate, DateTime endDate)
        {
            if (startDate.Year > 0 && endDate.Year > 0)
            {
                if (startDate.Year == endDate.Year)
                    return $"{startDate.ToString("MMM")}-{endDate.ToString("MMM")} {startDate.ToString("yyyy")}";
                else if (startDate.Year > 0 && endDate.Year > 0)
                    return $"{startDate.ToString("MMM")}/{startDate.ToString("yyyy")}-{endDate.ToString("MMM")}/{endDate.ToString("yyyy")}";
                else if (startDate.Year > 0)
                    return $"{startDate.ToString("MMM")}/{startDate.ToString("yyyy")}-";
                else
                    return $"-{endDate.ToString("MMM")}/{endDate.ToString("yyyy")}";
            }
            return "";
        }

        public int numberOfMonths(DateTime startDate, DateTime endDate)
        {
            if (startDate != default && endDate != default)
                return (endDate.Year * 12 + endDate.Month) - (startDate.Year * 12 + startDate.Month) + 1;
            else
                return 0;
        }


        //==============
        // Fiscal Year

        public int? fiscalYearFor(DateTime? date)
        {
            if (date.HasValue && date.Value != DateTime.MinValue)
            {
                if (date.Value.Month >= FiscalYearMonth)
                    return date.Value.Year;
                else
                    return date.Value.Year - 1;
            }
            else
                return null;
        }

        public DateTime firstDateOfFiscalYear(int fiscalYear)
        {
            if (fiscalYear > minValidFiscalYear)
                return new DateTime(fiscalYear, FiscalYearMonth, 1);
            else
                return DateTime.MinValue;
        }

        public DateTime lastDateOfFiscalYear(int fiscalYear)
        {
            if (fiscalYear > minValidFiscalYear)
                return firstDateOfFiscalYear(fiscalYear).AddYears(1).AddDays(-1);
            else
                return DateTime.MinValue;
        }

        public bool inFiscalYear(DateTime date, int fiscalYear)
        {
            if (fiscalYear > minValidFiscalYear && date != DateTime.MinValue)
            {
                return date >= firstDateOfFiscalYear(fiscalYear) && date < firstDateOfFiscalYear(fiscalYear).AddYears(1); // to avoid time problems on last date
            }
            return false;
        }


        /// <summary>
        /// Indicates if a date is within a date range.
        /// </summary>
        /// <param name="date">Date to be validated</param>
        /// <param name="startDate">First date in range</param>
        /// <param name="endDate">Last date in range</param>
        /// <returns></returns>
        public bool inDateRange(DateTime date, DateTime startDate, DateTime endDate)
        {
            if (date != DateTime.MinValue && startDate != DateTime.MinValue && endDate != DateTime.MinValue)
            {
                DateTime start = new DateTime(startDate.Year, startDate.Month, 1);
                DateTime end = new DateTime(endDate.Year, startDate.Month, 1).AddMonths(1);
                return (date >= start) && (date < end); // to avoid day and time problems on last date
            }
            return false;
        }


        //=========================
        // Fiscal Year -> Time periods
        public DateTime startOfQuarter(int fiscalYear, int quarter)
        {
            if (fiscalYear < 1900 || fiscalYear > 2100 || quarter < 0 || quarter > 3)
                return DateTime.MinValue;
            if (quarter < 3)
                return new DateTime(fiscalYear, 4 + 3 * quarter, 1);
            else
                return new DateTime(fiscalYear + 1, 1, 1);
        }
        public DateTime endOfQuarter(int fiscalYear, int quarter)
        {
            if (fiscalYear < 1900 || fiscalYear > 2100 || quarter < 0 || quarter > 3)
                return DateTime.MinValue;
            if (quarter < 3)
                return startOfQuarter(fiscalYear, quarter + 1).AddDays(-1);
            else
                return startOfQuarter(fiscalYear + 1, 0).AddDays(-1);
        }
        public string quarterMonths(int fiscalYear, int quarter)
        {
            DateTime start = startOfQuarter(fiscalYear, quarter);
            DateTime end = endOfQuarter(fiscalYear, quarter);
            return $"{start.ToString("MMM")}-{end.ToString("MMM")} {start.ToString("yyyy")}";
        }

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

        public int? startFiscalYear(List<Contract> contracts)
        {
            return fiscalYearFor(startDateOf(contracts));
        }
        public int? endFiscalYear(List<Contract> contracts)
        {
            return fiscalYearFor(originalCompletionDate(contracts));
        }

        public List<int>? getFiscalYears(List<Contract> contracts)
        {
            if (contracts?.Count > 0)
            {
                int? startYear = startFiscalYear(contracts);
                int? endYear = endFiscalYear(contracts);
                if (startYear.HasValue && endYear.HasValue && startYear.Value <= endYear.Value)
                {
                    List<int> years = new List<int>();
                    for (int year = startYear.Value; year <= endYear.Value; year++)
                        years.Add(year);
                    return years;
                }
                else
                    return null;
            }
            else
                return null;
        }

        public int? setFiscalYearsViewBag(List<Contract> contracts, dynamic ViewBag)
        {
            List<int>? years = getFiscalYears(contracts);
            if (years != null && years.Count > 0)
            {
                List<SelectListItem> yearsList = new List<SelectListItem>();
                foreach (int year in years)
                    yearsList.Add(new SelectListItem { Text = $"{year}-{year + 1}", Value = $"{year}" });
                ViewBag.yearList = yearsList;
                return years[0];
            }
            else
            {
                ViewBag.yearList = null;
                return null;
            }
        }

        public DateTime earliestDateOfProjectInFiscalYear(List<Contract> contracts, int fiscalYear)
        {
            if (fiscalYear > minValidFiscalYear)
            {
                DateTime firstDate = startDateOf(contracts).GetValueOrDefault();
                DateTime lastDate = originalCompletionDate(contracts).GetValueOrDefault();
                if (firstDate != default && lastDate != default)
                {
                    if (inFiscalYear(firstDate, fiscalYear))
                        return firstDate;
                    else if (inFiscalYear(lastDate, fiscalYear))
                        return firstDateOfFiscalYear(fiscalYear);
                    else if (firstDate < firstDateOfFiscalYear(fiscalYear) && lastDate > lastDateOfFiscalYear(fiscalYear))
                        return firstDateOfFiscalYear(fiscalYear);
                }
            }
            return default;
        }
        public DateTime latestDateOfProjectInFiscalYear(List<Contract> contracts, int fiscalYear)
        {
            if (fiscalYear > minValidFiscalYear)
            {
                DateTime firstDate = startDateOf(contracts).GetValueOrDefault();
                DateTime lastDate = originalCompletionDate(contracts).GetValueOrDefault();
                if (firstDate != default && lastDate != default)
                {
                    if (inFiscalYear(lastDate, fiscalYear))
                        return lastDate;
                    else if (inFiscalYear(firstDate, fiscalYear))
                        return lastDateOfFiscalYear(fiscalYear);
                    else if (firstDate < firstDateOfFiscalYear(fiscalYear) && lastDate > lastDateOfFiscalYear(fiscalYear))
                        return lastDateOfFiscalYear(fiscalYear);
                }
            }
            return default;
        }


        public bool isProjectInFiscalYear(List<Contract> contracts, int fiscalYear)
        {
            return earliestDateOfProjectInFiscalYear(contracts, fiscalYear) != default;
        }



        //======================
        // Start and Completion dates

        public int? revisedDuration(Contract contract)
        {
            int? duration = null;
            if (contract != null)
            {
                duration = contract.PlazoOriginal;
                if (contract.Extensions?.Count > 0)
                {
                    foreach (Extension extension in contract.Extensions)
                        duration = dataTools.add(duration, extension.Days);
                }
            }
            return duration;
        }

        public DateTime? originalCompletionDate(Contract? contract)
        {
            if (contract != null)
            {
                if (contract.PlannedStartDate.HasValue && contract.PlazoOriginal.HasValue)
                    if (contract.PlannedStartDate != DateTime.MinValue)
                        return contract.PlannedStartDate.Value.AddDays(contract.PlazoOriginal.Value);
            }
            return null;
        }
        public DateTime? revisedCompletionDate(Contract? contract)
        {
            DateTime? completionDate = null;
            if (contract != null)
            {
                completionDate = startDateOf(contract);
                if (completionDate != null && completionDate != DateTime.MinValue)
                {
                    int? duration = revisedDuration(contract);
                    if (duration.HasValue)
                        completionDate = completionDate.Value.AddDays(duration.Value);
                }
            }
            return completionDate;
        }

        public DateTime? startDateOf(Contract contract)
        {
            if (contract != null)
            {
                if (contract.StartDate != null && contract.StartDate != DateTime.MinValue)
                    return contract.StartDate;
                else if (contract.ContractDate != null && contract.ContractDate != DateTime.MinValue)
                    return contract.ContractDate;
                else if (contract.PlannedStartDate != null && contract.PlannedStartDate != DateTime.MinValue)
                    return contract.PlannedStartDate;
            }
            return null;
        }


        public DateTime? startDateOf(List<Contract> contracts)
        {
            if (contracts != null)
            {
                DateTime? date = null;
                foreach (var contract in contracts)
                {
                    date = minDate(date, startDateOf(contract));
                }
                return date;
            }
            else
                return null;
        }

        public DateTime? originalCompletionDate(List<Contract> contracts)
        {
            if (contracts != null)
            {
                DateTime? date = null;
                foreach (var contract in contracts)
                {
                    date = maxDate(date, originalCompletionDate(contract));
                }
                return date;
            }
            else
                return null;
        }

        public DateTime? revisedCompletionDate(List<Contract> contracts)
        {
            if (contracts != null)
            {
                DateTime? date = null;
                foreach (var contract in contracts)
                {
                    date = maxDate(date, revisedCompletionDate(contract));
                }
                return date;
            }
            else
                return null;
        }


        #endregion



    }
}
