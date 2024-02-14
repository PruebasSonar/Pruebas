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

namespace PIPMUNI_ARG.Areas.AppReports.Controllers
{
    [Area("AppReports")]
    public class PerformanceController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly IJaoTableServices jts;
        private readonly IJaoTableExcelServices jtExcel;

        public PerformanceController(PIPMUNI_ARGDbContext context
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
            const string dbView = "report_ContractPerformance";

            List<PerformanceReportModel> records = new List<PerformanceReportModel>();
            SqlConnection sqlConnection = new SqlConnection(context.Database.GetConnectionString());
            try
            {
                sqlConnection.Open();
                records = (await sqlConnection.QueryAsync<PerformanceReportModel>($"select * from {dbView}")).ToList();
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


        JaoTable prepareTable(List<PerformanceReportModel> records)
        {
            jts.setExcelWidths(new float[] { 
                300, 60,
                60, 
                //150, 150, 
                60,
                60,
                70, 70, 70, 
                60
            });
            jts.setTitle("Reporte de avance");
            jts.setSubtitle("");

            jts.addHeaderCell(displayFieldTitle("name"));
            jts.addHeaderCell(displayFieldTitle("programado"), "numCel");
            jts.addHeaderCell(displayFieldTitle("pagado"), "numCel");
            jts.addHeaderCell(displayFieldTitle("porcentajePagado"), $"numCel {JaoTable.screenOnlyClass}" );
            jts.addHeaderCell(displayFieldTitle("porcentajeFisico"), "numCel");
            jts.addHeaderCell(displayFieldTitle("fechaInicio"), $"dateCel {JaoTable.screenOnlyClass}");
            jts.addHeaderCell(displayFieldTitle("fechaFin"), $"dateCel {JaoTable.screenOnlyClass}");
            jts.addHeaderCell(displayFieldTitle("fechaProgramada"), $"dateCel {JaoTable.padAndScreenClass}");
            jts.addHeaderCell(displayFieldTitle("porcentajeTiempo"), $"numCel {JaoTable.padAndScreenClass}");


            foreach (var record in records)
            {
                jts.addRow();
                jts.addCell(record.name);
                jts.addCell(record.programado);
                jts.addCell(record.pagado);
                jts.addFloatCell(record.porcentajePagado, JaoTable.screenOnlyClass);
                jts.addFloatCell(record.porcentajeFisico);
                jts.addCell(record.fechaInicio, JaoTable.screenOnlyClass);
                jts.addCell(record.fechaFin, JaoTable.screenOnlyClass);
                jts.addCell(record.fechaProgramada, JaoTable.padAndScreenClass);
                jts.addFloatCell(record.porcentajeTiempo, JaoTable.padAndScreenClass);
            }
            return jts.getTable();
        }

        string displayFieldTitle(string fieldName)
        {
            MemberInfo? property = typeof(PerformanceReportModel).GetProperty(fieldName);
            var attribute = property?.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                                    .Cast<DisplayNameAttribute>().Single();
            return attribute?.DisplayName ?? "";
        }
    }


}