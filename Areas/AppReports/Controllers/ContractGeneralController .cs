using Microsoft.AspNetCore.Mvc;
using PIPMUNI_ARG.Data;
using JaosLib.Models.JaoTables;
using JaosLib.Services.JaoTables;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Dapper;
using System.Reflection;
using System.ComponentModel;
using PIPMUNI_ARG.Areas.AppReports.Models;
using PIPMUNI_ARG.Models.Domain;
using PIPMUNI_ARG.Services.Utilities;

namespace PIPMUNI_ARG.Areas.AppReports.Controllers
{
    [Area("AppReports")]
    public class ContractGeneralController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly IJaoTableServices jts;
        private readonly IJaoTableExcelServices jtExcel;

        JaosLibUtils libUtils = new JaosLibUtils();
        public ContractGeneralController(PIPMUNI_ARGDbContext context
            , IJaoTableServices jaoTableServices
            , IJaoTableExcelServices jaoTableExcelServices
            )
        {
            this.context = context;
            jts = jaoTableServices;
            this.jtExcel = jaoTableExcelServices;
        }

        // GET: HomeController1
        public async Task<IActionResult> Index()
        {
            JaoTable jaoTable = await fillTable();
            ViewBag.NoContainer = true;
            return View(jaoTable);
        }


        async Task<JaoTable> fillTable()
        {
            const string dbView = "report_ContractGeneral";

            List<ContractGeneralReportModel> records = new List<ContractGeneralReportModel>();
            SqlConnection sqlConnection = new SqlConnection(context.Database.GetConnectionString());
            try
            {
                sqlConnection.Open();
                records = (await sqlConnection.QueryAsync<ContractGeneralReportModel>($"select * from {dbView}")).ToList();
                if (records.Count > 0)
                {
                    //if (User.IsInRole("Office"))
                    //{
                    //    UserProfile? userProfile = context.UserProfile.Include(p => p.Office_info).Where(p => p.Email == User.Identity.Name).FirstOrDefault();
                    //    string officeName = userProfile?.Office_info?.Name ?? "No entity available";
                    //    records = records.Where(r => r.name_office == officeName).ToList();
                    //}
                    //if (records.Count > 0)
                    //    records = records.Where(r => r.name_projectStage == "Planning" || r.name_projectStage == "Identification").OrderBy(r => r.name_entity + r.name_project).ToList();
                }
            }
            catch
            {
                throw;
            }
            return prepareTable(records);
        }


        public async Task<IActionResult> Export()
        {
            JaoTable jaoTable = await fillTable();
            MemoryStream memoryStream = jtExcel.createExcelFile(jaoTable, "Pipeline", "Report", Response);
            return File(memoryStream, JaoTableExcelServices.fileStyle, jtExcel.fileName);
        }


        JaoTable prepareTable(List<ContractGeneralReportModel> records)
        {
            jts.setExcelWidths(new float[] {
                60, 60,
                300,
                150, 150, 150, 90,
                60, 60, 60, 60,
                60, 60, 60,
                60, 60,
                150,
                70, 70, 70, 70
            });

            jts.setTitle("Obras - Datos Generales");
            jts.setSubtitle("");



            jts.addHeaderCell(fieldTitle("projectCode"), JaoTable.screenOnlyClass);
            jts.addHeaderCell(fieldTitle("Code"), JaoTable.screenOnlyClass);
            jts.addHeaderCell(fieldTitle("name"));
            jts.addHeaderCell(fieldTitle("office"), JaoTable.screenOnlyClass);
            jts.addHeaderCell(fieldTitle("Sector"), JaoTable.screenOnlyClass);
            jts.addHeaderCell(fieldTitle("subsector"), JaoTable.screenOnlyClass);
            jts.addHeaderCell(fieldTitle("stage"), JaoTable.screenOnlyClass);

            jts.addHeaderCell(fieldTitle("originalValue"), $"numCel {JaoTable.screenOnlyClass}");
            jts.addHeaderCell(fieldTitle("programado"), $"numCel {JaoTable.screenOnlyClass}");
            jts.addHeaderCell(fieldTitle("pagado"), "numCel");

            jts.addHeaderCell(fieldTitle("porcentajeFisico"), "numCel");
            jts.addHeaderCell(fieldTitle("porcentajePagado"), "numCel");

            jts.addHeaderCell(fieldTitle("saldo"), "numCel");

            jts.addHeaderCell(fieldTitle("certificadosPendientes"), $"numCel {JaoTable.screenOnlyClass}");

            jts.addHeaderCell(fieldTitle("plazoOriginal"), $"numCel {JaoTable.screenOnlyClass}");
            jts.addHeaderCell(fieldTitle("plazoAmpliado"), $"numCel {JaoTable.screenOnlyClass}");

            jts.addHeaderCell(fieldTitle("Contractor"), JaoTable.screenOnlyClass);

            jts.addHeaderCell(fieldTitle("ContractSigned"), $"dateCel {JaoTable.screenOnlyClass}");
            jts.addHeaderCell(fieldTitle("PlannedStartDate"), $"dateCel {JaoTable.screenOnlyClass}");
            jts.addHeaderCell(fieldTitle("PlannedEndDate"), $"dateCel {JaoTable.screenOnlyClass}");
            jts.addHeaderCell(fieldTitle("StartDate"), $"dateCel {JaoTable.screenOnlyClass}");

            foreach (var record in records)
            {
                jts.addRow();

                jts.addCell(record.projectCode, JaoTable.screenOnlyClass);
                jts.addCell(record.Code, JaoTable.screenOnlyClass);
                jts.addCell(record.name);
                jts.addCell(record.office, JaoTable.screenOnlyClass);
                jts.addCell(record.Sector, JaoTable.screenOnlyClass);
                jts.addCell(record.subsector, JaoTable.screenOnlyClass);
                jts.addCell(record.stage, JaoTable.screenOnlyClass);
                jts.addCell(record.originalValue, JaoTable.screenOnlyClass);
                jts.addCell(record.programado, JaoTable.screenOnlyClass);
                jts.addCell(record.pagado);
                jts.addFloatCell(record.porcentajeFisico);
                jts.addFloatCell(record.porcentajePagado);
                jts.addCell(record.saldo);
                jts.addCell(record.certificadosPendientes, JaoTable.screenOnlyClass);
                jts.addCell(record.plazoOriginal, JaoTable.screenOnlyClass);
                jts.addCell(record.plazoAmpliado, JaoTable.screenOnlyClass);
                jts.addCell(record.Contractor, JaoTable.screenOnlyClass);
                jts.addCell(record.ContractSigned, JaoTable.screenOnlyClass);
                jts.addCell(record.PlannedStartDate, JaoTable.screenOnlyClass);
                jts.addCell(record.PlannedEndDate, JaoTable.screenOnlyClass);
                jts.addCell(record.StartDate, JaoTable.screenOnlyClass);

            }
            return jts.getTable();
        }

        string fieldTitle(string fieldName)
        {
            return libUtils.titleOf<ContractGeneralReportModel>(fieldName);
        }
    }


}