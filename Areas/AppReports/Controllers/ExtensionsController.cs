﻿using JaosLib.Models.JaoTables;
using JaosLib.Services.JaoTables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Dapper;
using PIPMUNI_ARG.Areas.AppReports.Models;
using PIPMUNI_ARG.Data;
using PIPMUNI_ARG.Services.Utilities;

namespace PIPMUNI_ARG.Areas.AppReports.Controllers
{
    [Area("AppReports")]
    [Authorize(Roles = ProjectGlobals.registeredRoles)]

    public class ExtensionsController : Controller
    {

        private readonly PIPMUNI_ARGDbContext context;
        private readonly IJaoTableServices jts;
        private readonly IJaoTableExcelServices jtExcel;


        JaosLibUtils libUtils = new JaosLibUtils();

        public ExtensionsController(PIPMUNI_ARGDbContext context
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
            MemoryStream memoryStream = jtExcel.createExcelFile(jaoTable, "Ampliaciones", "Report", Response);
            return File(memoryStream, JaoTableExcelServices.fileStyle, jtExcel.fileName);
        }



        //==============================================
        //----------------------------------------------



        async Task<JaoTable> fillTable()
        {
            const string dbView = "[report_Extensions]";
            List<ExtensionsReportModel> records;
            SqlConnection sqlConnection = new SqlConnection(context.Database.GetConnectionString());
            try
            {
                sqlConnection.Open();
                records = (await sqlConnection.QueryAsync<ExtensionsReportModel>($"select * from {dbView}")).ToList();
                if (records.Count > 0)
                {
                    //records = records
                    //.Where(r => r.name_projectStage == "Implementation" || r.name_projectStage == "Close-Out")
                    //.OrderBy(r => r.name_office + r.name_project).ToList();
                    //.OrderBy(r => r.name_project).ToList();
                }
            }
            catch
            {
                throw;
            }
            return prepareTable(records);
        }



        JaoTable prepareTable(List<ExtensionsReportModel> records)
        {
            jts.setExcelWidths(new float[] { 50, 300
                , 50, 50, 50, 50, 50, 50
                , 50, 50, 100, 50
            });
            jts.setTitle("Reporte Ampliaciones de Plazo");
            jts.setSubtitle("");

            JaosLibUtils libUtils = new JaosLibUtils();

            fieldTitle(nameof(ExtensionsReportModel.contractCode)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(ExtensionsReportModel.contractName)!);

            fieldTitle(nameof(ExtensionsReportModel.extensionCode)!, $"numCel");
            fieldTitle(nameof(ExtensionsReportModel.extensionRecord)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(ExtensionsReportModel.stageName)!);
            fieldTitle(nameof(ExtensionsReportModel.days)!, $"numCel");
            fieldTitle(nameof(ExtensionsReportModel.DateDelivery)!, $"numCel");
            fieldTitle(nameof(ExtensionsReportModel.DateApproved)!, $"numCel");

            fieldTitle(nameof(ExtensionsReportModel.attached)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(ExtensionsReportModel.otherAttachments)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(ExtensionsReportModel.motivo)!);
            fieldTitle(nameof(ExtensionsReportModel.extensionDelay)!, $"numCel");

            foreach (var record in records)
            {
                jts.addRow();

                jts.addCell(record.contractCode, JaoTable.screenOnlyClass);
                jts.addCell(record.contractName);
                jts.addCell(record.extensionCode);
                jts.addCell(record.extensionRecord, JaoTable.screenOnlyClass);
                jts.addCell(record.stageName);
                jts.addCell(record.days);
                jts.addCell(record.DateDelivery);
                jts.addCell(record.DateApproved);
                jts.addCell(record.attached, JaoTable.screenOnlyClass);
                jts.addCell(record.otherAttachments, JaoTable.screenOnlyClass);
                jts.addCell(record.motivo, JaoTable.screenOnlyClass);
                jts.addCell(record.extensionDelay);
            }
            return jts.getTable();
        }


        void fieldTitle(string fieldName, string cssClass = "")
        {
            jts.addHeaderCell(libUtils.titleOf<ExtensionsReportModel>(fieldName));
        }




    }
}
