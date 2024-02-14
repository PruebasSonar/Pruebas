using Microsoft.AspNetCore.Mvc;
using PIPMUNI_ARG.Data;
using Microsoft.EntityFrameworkCore;
using PIPMUNI_ARG.Models.Domain;
using JaosLib.Services.JaoTables;
using JaosLib.Models.JaoTables;
using PIPMUNI_ARG.Services.Utilities;
using PIPMUNI_ARG.Areas.AppReports.Models;

namespace PIPMUNI_ARG.Areas.AppReports.Controllers
{
    [Area("AppReports")]
    public class ProjectController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly IJaoTableServices jts;
        private readonly IJaoTableExcelServices jtExcel;

        JaosLibUtils libUtils = new JaosLibUtils();

        public ProjectController(PIPMUNI_ARGDbContext context
            , IJaoTableServices jaoTableServices
            , IJaoTableExcelServices jaoTableExcelServices
            )
        {
            this.context = context;
            jts = jaoTableServices;
            jtExcel = jaoTableExcelServices;
        }

        // GET: Index
        public async Task<IActionResult> Index(string sortOrder)
        {
            JaoTable jaoTable = await fillTable();
            return View(jaoTable);
        }

        // POST: Export
        public async Task<IActionResult> Export()
        {
            JaoTable jaoTable = await fillTable();
            MemoryStream memoryStream = jtExcel.createExcelFile(jaoTable, "ProjectsList", "Report", Response);
            return File(memoryStream, JaoTableExcelServices.fileStyle, jtExcel.fileName);
        }



        //==============================================
        //----------------------------------------------



        async Task<JaoTable> fillTable()
        {
            List<Project> records = new List<Project>();
            try
            {
                records = await context.Project
                                    .Include(t => t.ProjectSources!).ThenInclude(t => t.Source_info)
                                    .Include(t => t.Sector_info)
                                    .Include(t => t.Subsector_info)
                                    .Include(t => t.Stage_info)
                                    .Include(t => t.Office_info)
                                    .ToListAsync();

                //if (records.Count > 0)
                //{
                //    //records = records
                //    //    //.Where(r => r.name_projectStage == "Implementation" || r.name_projectStage == "Close-Out")
                //    //    //.OrderBy(r => r.name_office + r.name_project).ToList();
                //    //    .OrderBy(r => r.name_project).ToList();
                //}
            }
            catch
            {
                throw;
            }
            return prepareTable(records);
        }



        JaoTable prepareTable(List<Project> records)
        {
            JaosLibUtils libUtils = new JaosLibUtils();
            jts.setExcelWidths(new float[] { 50, 300, 150, 150, 150, 50, 50, 50, 300, 300, 150, 150, 50, 150, 50 });
            jts.setTitle("Reporte Proyectos");
            jts.setSubtitle("");

            jts.addHeaderCell("");
            fieldTitle(nameof(Project.Code)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(Project.Name)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(Project.Office)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(Project.Sector)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(Project.Subsector)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(Project.Stage)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(Project.Cost)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(Project.CostDate)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(Project.Description)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(Project.Objectives)!, $"numCel {JaoTable.screenOnlyClass}");
            jts.addHeaderCell(libUtils.titleOf<ProjectSource>(), $"numCel {JaoTable.screenOnlyClass}");
            jts.addHeaderCell(libUtils.titleOf<ProjectSource>(nameof(ProjectSource.Percentage)!), $"numCel {JaoTable.screenOnlyClass}");

            int number = 1;
            foreach (var record in records)
            {
                jts.addRow();
                jts.addCell(number++);
                jts.addCell(record.Code);
                jts.addCell(record.Name);
                jts.addCell(record.Office_info?.Name);
                jts.addCell(record.Sector_info?.Name);
                jts.addCell(record.Subsector_info?.Name);
                jts.addCell(record.Stage_info?.Name);
                jts.addCell(record.Cost);
                jts.addCell(record.CostDate);
                jts.addCell(record.Description);
                jts.addCell(record.Objectives);
                jts.addCell(record.ProjectSources?.Count > 0 ? record.ProjectSources.ToList()[0].Source_info?.Name : null);
                jts.addCell(record.ProjectSources?.Count > 0 ? record.ProjectSources.ToList()[0].Percentage : null);
            }
            return jts.getTable();
        }


        void fieldTitle(string fieldName, string cssClass)
        {
            jts.addHeaderCell(libUtils.titleOf<Project>(fieldName), cssClass);
        }

    }


}