using JaosLib.Services.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PIPMUNI_ARG.Data;
using PIPMUNI_ARG.Models.Domain;
using PIPMUNI_ARG.Services.basic;
using PIPMUNI_ARG.Services.Utilities;
using PIPMUNI_ARG.Models.Reports;
using Microsoft.EntityFrameworkCore;
using Dapper;
using PIPMUNI_ARG.Areas.Review.Models;
using System.Globalization;

namespace PIPMUNI_ARG.Areas.Review.Controllers
{
    [Authorize(Roles =ProjectGlobals.registeredRoles)]
    [Area("Review")]
    public partial class HomeController : Controller
    {
        const string currencyName = "$ ";

        private readonly PIPMUNI_ARGDbContext context;
        private readonly IParentContractService parentContractService;
        private readonly IPipToolsService pipToolsService;
        private readonly INPOIWordService wordService;

        public HomeController(PIPMUNI_ARGDbContext context
            , IParentContractService parentContractService
            , IPipToolsService pipToolsService
            , INPOIWordService wordService
            )
        {
            this.context = context;
            this.parentContractService = parentContractService;
            this.pipToolsService = pipToolsService;
            this.wordService = wordService;
        }

        JaosLibUtils jaosLibUtils = new JaosLibUtils();
        JaosDataTools dataTools = new JaosDataTools();

        //----------- Index

        // GET: Project
        public async Task<IActionResult> Index(int? id)
        {
            Contract? contract = await parentContractService.getContractFromIdOrSession(id, User, HttpContext.Session, ViewBag);
            if (contract == null)
                return await Task.Run(() => RedirectToAction("Select", "Contract", new { Area = "" }));

            contract = await context.Contract.
                Include(p => p.Project_info!).ThenInclude(p => p.Sector_info).
                Include(p => p.Project_info!).ThenInclude(p => p.Subsector_info).
                Include(p => p.Project_info!).ThenInclude(p => p.Office_info).
                Include(p => p.Project_info!).ThenInclude(p => p.Stage_info).
                Include(p => p.Project_info!).ThenInclude(p => p.ProjectSources!).ThenInclude(c => c.Source_info).
                Include(c => c.Contractor_info).
                Include(c => c.Office_info).
                Include(c => c.Stage_info).
                Include(c => c.Payments!).ThenInclude(y => y.Type_info).
                Include(c => c.Payments!).ThenInclude(y => y.Stage_info).
                Include(c => c.Payments!).ThenInclude(y => y.PaymentAttachments).
                Include(c => c.Additions!).ThenInclude(y => y.Stage_info).
                Include(c => c.Additions!).ThenInclude(y => y.AdditionAttachments).
                Include(c => c.Variantes!).ThenInclude(y => y.Stage_info).
                Include(c => c.Variantes!).ThenInclude(y => y.Motivo_info).
                Include(c => c.Variantes!).ThenInclude(y => y.VarianteAttachments).
                Include(c => c.Extensions!).ThenInclude(y => y.Stage_info).
                Include(c => c.Extensions!).ThenInclude(y => y.ExtensionAttachments).
                AsNoTracking().
                FirstOrDefaultAsync(p => p.Id == contract.Id);
            if (contract == null)
                return await Task.Run(() => RedirectToAction("Select", "Contract", new { Area = "" }));


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            setInfoViewBags(contract);
            produceCharts(contract);

            //                ViewBag.animationValue = "animate";
            ViewBag.navGeneral = "active";
            return View(contract);
        }


        [HttpPost]
        public async Task<IActionResult> Back()
        {
            return await Task.Run(() => RedirectToAction("Index"));
        }

        //========================================================================
        //
        //     General Methods
        //
        //------------------------------------------------------------------------



        void setInfoViewBags(Contract contract)
        {
            // finance
            ViewBag.planned = contract.OriginalValue.HasValue ? currencyName + contract.OriginalValue.Value.ToString("n0") : "";
            AdvanceBasic? info = getFinanceChartData(contract);
            if (info != null)
            {
                if (info.programmed.HasValue)
                    ViewBag.programmed = info.programmed.Value > 0 ? currencyName + info.programmed.Value.ToString("n0") : "";
                if (info.actual.HasValue)
                    ViewBag.expended = info.actual.Value > 0 ? currencyName + info.actual.Value.ToString("n0") : "";
                if (info.programmed.HasValue)
                {
                    double? balance = dataTools.substract(info.programmed, info.actual);
                    ViewBag.balance = balance.HasValue && balance.Value > 0 ? currencyName + balance.Value.ToString("n0") : null;
                }
            }

            // available for Totals
            //ContractSummaryModel? summary = getContractSummary(contract);
            //ViewBag.summary = summary;

            // Dates and duration
            int? revisedDuration = pipToolsService.revisedDuration(contract);
            ViewBag.revisedDuration = revisedDuration.HasValue ? revisedDuration.Value.ToString("n0") : "";
            DateTime? originalCompletionDate = pipToolsService.originalCompletionDate(contract);
            ViewBag.originalCompletionDate = originalCompletionDate.HasValue ? originalCompletionDate.Value.ToString("dd/MM/yyyy") : null;
            DateTime? revisedCompletionDate = pipToolsService.revisedCompletionDate(contract);
            ViewBag.revisedCompletionDate = revisedCompletionDate.HasValue ? revisedCompletionDate.Value.ToString("dd/MM/yyyy") : null;
            DateTime? startDate =pipToolsService.startDateOf(contract);
            ViewBag.startDate = startDate.HasValue ? startDate.Value.ToString("dd/MM/yyyy") : null;

            // Project Information
            if (contract.Project_info != null)
            {
                Project project = contract.Project_info;
                if (project.ProjectSources != null && project.ProjectSources.Any())
                    ViewBag.source = project.ProjectSources.Where(i => i.Source_info != null).Select(i => i.Source_info!.Name).Aggregate((i, j) => i + ", " + j);

                if (project.Sector_info != null)
                    ViewBag.sector = project.Sector_info.Name;

                if (project.Office_info != null)
                    ViewBag.office = project.Office_info.Name;
            }

            // Totals x 4
            ViewBag.paymentTotals = getPaymentTotals(contract.Id);
            ViewBag.additionTotals = getAdditionTotals(contract.Id);
            ViewBag.varianteTotals = getVarianteTotals(contract.Id);
            ViewBag.extensionTotals = getExtensionTotals(contract.Id);
        }




        //=========================================================================================================
        //
        //                               C  H  A  R  T  S
        //
        //---------------------------------------------------------------------------------------------------------

        void produceCharts(Contract contract)
        {
            ViewBag.dataChartFinance = produceFinanceChart(contract);
            ViewBag.dataChartPhysical = produceIndicatorChart(contract);
            ViewBag.dataChart3 = produceTimeChart(contract);
        }

        //----------- Expenditure Chart

        string produceFinanceChart(Contract contract)
        {
            AdvanceBasic? info = getFinanceChartData(contract);
            if (info != null)
            {
                if (info.programmed.HasValue)
                {
                    double ejecutado = (info.actual.HasValue ? info.actual.Value : 0) / ((info.programmed.HasValue && info.programmed.Value > 0) ? info.programmed.Value : 1) * 100;
                    if (ejecutado < 0) ejecutado = 0;
                    double pendiente = 100 - ejecutado;

                    return string.Format(@"[[""%"",""Pagado"",""Disponible""],[""%"",{0},{1}]]", chartValue(ejecutado), chartValue(pendiente));
                }
            }
            return "";
        }

        // Finance Data
        AdvanceBasic? getFinanceChartData(Contract contract)
        {
            AdvanceBasic? data = null;
            if (contract?.Id > 0)
            {
                SqlConnection sqlConnection = new SqlConnection(context.Database.GetConnectionString());
                try
                {
                    sqlConnection.Open();
                    List<AdvanceBasic> result = sqlConnection.Query<AdvanceBasic>(string.Format(@"
With 
contractPayments as (
SELECT 
Payment.[Contract] as contractId
, sum(payment.[Total]) AS [Total]
from Payment
where Payment.Stage in (4,5) --4: Devengado 5: pagado
GROUP BY Payment.[Contract]
)
,contractAdditions as (
SELECT 
Addition.[Contract] as contractId
, sum(addition.[Value]) as [value]
from Addition
where stage in (3) -- 3:Aprobada
group by Addition.[Contract]
)

,contractVariantes as (
SELECT 
Variante.[Contract] as contractId
, sum(Variante.[Value]) as [value]
from Variante
where stage in (3) -- 3:Aprobada
group by Variante.[Contract]
)

-- main SQL
select 
[Contract].id as [id]
,sum(contractPayments.[Total]) as actual
,nullif(
	isnull(sum([contract].OriginalValue),0) 
	+ isnull(sum(contractAdditions.[value]),0) 
	+ isnull(sum(contractVariantes.[value]),0)
,0) as [programmed]
,
case when 
1 < (sum(contractPayments.[Total]) /
 nullif(
	isnull(sum([contract].OriginalValue),0) 
		+ isnull(sum(contractAdditions.[value]),0)
 ,0))
 then 1
 else
sum(contractPayments.[Total]) /
 nullif(
	isnull(sum([contract].OriginalValue),0) 
	+ isnull(sum(contractAdditions.[value]),0) 
	+ isnull(sum(contractVariantes.[value]),0)
 ,0)
 end
 as [percent]
from [Contract]
left join contractPayments on [Contract].id = contractPayments.contractId
left join contractAdditions on [Contract].id = contractAdditions.contractId
left join contractVariantes on [Contract].id = contractVariantes.contractId
where [Contract].[id] = {0}
group by [contract].id
", contract.Id)).ToList();
                    if (result.Any())
                        data = result[0];
                }
                catch
                {
                    throw;
                }
                finally
                {
                    sqlConnection.Close();
                }
            }
            return data;
        }


        //----------- Indicator Chart

        string produceIndicatorChart(Contract contract)
        {
            AdvanceBasic? info = getPhysicalAdvanceChart(contract);
            if (info != null)
            {
                if (info.programmed.HasValue)
                {
                    double pendiente = (info.programmed.HasValue) ? info.programmed.Value * 100 : 0;
                    if (pendiente < 0) pendiente = 0;
                    double ejecutado = info.actual.HasValue ? info.actual.Value * 100 : 0;
                    if (ejecutado < 0) ejecutado = 0;
                    return string.Format(@"[[""%"",""Ejecutado"",""Pendiente""],[""%"",{0},{1}]]", chartValue(ejecutado), chartValue(pendiente));
                }
            }
            return "";
        }

        // Physical Advance Data
        AdvanceBasic? getPhysicalAdvanceChart(Contract contract)
        {
            AdvanceBasic? data = null;
            if (contract?.Id > 0)
            {
                SqlConnection sqlConnection = new SqlConnection(context.Database.GetConnectionString());
                try
                {
                    sqlConnection.Open();
                    List<AdvanceBasic> result = sqlConnection.Query<AdvanceBasic>(string.Format(@"
With 
contractPayments as (
SELECT 
Payment.[Contract] as contractId
, sum(payment.[PhysicalAdvance]) / 100 AS [performed]
from Payment
where Payment.Stage in (5) --5: pagado
GROUP BY Payment.[Contract]
)

-- main SQL
select 
[Contract].id as [id]
,avg(contractPayments.[performed]) as actual
,1 - isnull(avg(contractPayments.[performed]),0) as [programmed]
,avg(contractPayments.[performed]) as [percent]
from [Contract]
left join contractPayments on [Contract].id = contractPayments.contractId
where [Contract].[id] = {0}
group by [contract].id
", contract.Id)).ToList();
                    if (result.Any())
                    {
                        data = result[0];
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    sqlConnection.Close();
                }
            }
            return data;
        }



        //----------- Time Chart

        string produceTimeChart(Contract contract)
        {

            //------------------------- Dibujo 3 TimeLine
            DateTime? startDate = pipToolsService.startDateOf(contract);
            DateTime? completionDate = pipToolsService.revisedCompletionDate(contract);
            double? avance = null;
            double? pendiente = null;
            if (startDate.HasValue && startDate != DateTime.MinValue && completionDate.HasValue && completionDate != DateTime.MinValue)
            {
                if (startDate.Value < DateTime.Now && completionDate.Value > DateTime.Now)
                {
                    avance = DateTime.Now.Subtract(startDate.Value).TotalDays;
                    pendiente = completionDate.Value.Subtract(DateTime.Now).TotalDays;
                }
                if (startDate.Value < DateTime.Now && completionDate.Value < DateTime.Now)
                    if (startDate.Value < completionDate.Value)
                    {
                        avance = completionDate.Value.Subtract(startDate.Value).TotalDays;
                        pendiente = null;
                    }
                if (startDate.Value > DateTime.Now && completionDate.Value > DateTime.Now)
                    if (startDate.Value < completionDate.Value)
                    {
                        avance = null;
                        pendiente = completionDate.Value.Subtract(startDate.Value).TotalDays;
                    }
            }
            if (avance.HasValue || pendiente.HasValue)
                return string.Format(@"[[""días"",""Transcurridos"",""Faltantes""],[""días"",{0},{1}]]", chartIntValue(avance), chartIntValue(pendiente));
            else
                return "";
        }


        //===============================================
        //           Chart Utilities
        //-----------------------------------------------

        string chartSet(string title, double? value1, double? value2)
        {
            return string.Format(@",[""{0}"",{1},{2}]", title, chartValue(value1), chartValue(value2));
        }
        string chartValue(double? number)
        {
            CultureInfo usCulture = CultureInfo.CreateSpecificCulture("en-US");
            return number.HasValue ? number.Value.ToString("0.0", usCulture) : "null";
        }

        string chartIntValue(double? number)
        {
            return number.HasValue ? number.Value.ToString("f0") : "null";
        }

        string chartValue(DateTime? date)
        {
            if (date.HasValue)
                return date.Value.Year.ToString();
            else
                return ("null");
        }


        //string createChartData(List<FinanceBasic> data)
        //{
        //    string chartData = "";
        //    if (data?.Count > 0)
        //    {
        //        int startYear = data.FindAll(d => d.year > 0).Min(e => e.year);
        //        int endYear = data.FindAll(d => d.year > 0).Max(e => e.year);
        //        if (startYear > 0 && endYear > 0)
        //        {
        //            chartData = @"[[""Year"",""Actual"",""Programmed""]";
        //            for (int y = startYear; y <= endYear; y++)
        //            {
        //                FinanceBasic? yearData = data.Find(e => e.year == y);
        //                if (yearData != null)
        //                    chartData += chartSet(y.ToString(), yearData.actual, yearData.programmed);
        //                else
        //                    chartData += chartSet(y.ToString(), null, null);
        //                ;
        //            }
        //            chartData += "]";
        //        }
        //    }
        //    return chartData;
        //}



        //---------------------------------------------------------------------------------------------------------------




        // ====================================================================
        //
        //  Detailed information
        //---------------------------------------------------------------------

        ContractSummaryModel? getContractSummary(Contract contract)
        {
            ContractSummaryModel? data = null;
            if (contract.Id > 0)
            {
                SqlConnection sqlConnection = new SqlConnection(context.Database.GetConnectionString());
                try
                {
                    sqlConnection.Open();
                    List<ContractSummaryModel> result = sqlConnection.Query<ContractSummaryModel>(string.Format(@"
With 
contractPayments as (
SELECT 
Payment.[Contract] as contractId
, sum(payment.[Total]) AS [Total]
, sum(payment.[PhysicalAdvance]) AS [performed]
from Payment
where Payment.Stage in (4, 5) --4:devengado 5: pagado
GROUP BY Payment.[Contract]
)
,contractAdditions as (
SELECT 
Addition.[Contract] as contractId
, sum(addition.Value) as [value]
from Addition
where stage = 3 -- 3:Aprobada
group by Addition.[Contract]
)
,contractVariantes as (
SELECT 
Variante.[Contract] as contractId
, sum(Variante.[Value]) as [value]
from Variante
where stage in (3) -- 3:Aprobada
group by Variante.[Contract]
)

-- main SQL
select 
[contract].id as ContractId
,[contract].record as record
,[contract].[name] as [name]
,[contract].[OriginalValue] as [originalValue]
,
	isnull([contract].OriginalValue,0) 
	+ isnull(contractAdditions.[value],0) 
	+ isnull(contractVariantes.[value],0) 
	as [programmedValue]
,contractPayments.[Total] as actualValue
,contractPayments.[performed] as [advanced]
from [Contract]
left join contractPayments on [Contract].id = contractPayments.contractId
left join contractAdditions on [Contract].id = contractAdditions.contractId
left join contractVariantes on [Contract].id = contractVariantes.contractId
where [Contract].[id] = {0}
", contract.Id)).ToList();

                    if (result.Any())
                    {
                        data = result[0];
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    sqlConnection.Close();
                }
            }
            return data;
        }





    }
}
