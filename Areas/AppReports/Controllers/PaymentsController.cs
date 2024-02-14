using JaosLib.Models.JaoTables;
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

    public class PaymentsController : Controller
    {

        private readonly PIPMUNI_ARGDbContext context;
        private readonly IJaoTableServices jts;
        private readonly IJaoTableExcelServices jtExcel;


        JaosLibUtils libUtils = new JaosLibUtils();

        public PaymentsController(PIPMUNI_ARGDbContext context
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
            MemoryStream memoryStream = jtExcel.createExcelFile(jaoTable, "Certificaciones", "Report", Response);
            return File(memoryStream, JaoTableExcelServices.fileStyle, jtExcel.fileName);
        }



        //==============================================
        //----------------------------------------------



        async Task<JaoTable> fillTable()
        {
            const string dbView = "[report_Payments]";
            List<PaymentsReportModel> records;
            SqlConnection sqlConnection = new SqlConnection(context.Database.GetConnectionString());
            try
            {
                sqlConnection.Open();
                records = (await sqlConnection.QueryAsync<PaymentsReportModel>($"select * from {dbView}")).ToList();
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



        JaoTable prepareTable(List<PaymentsReportModel> records)
        {
            jts.setExcelWidths(new float[] { 50, 300
                , 50, 50, 50, 50, 50, 50
                , 50, 50, 50, 50, 50
                , 50, 50, 50, 50, 50
                , 50, 50, 50, 50, 50
            });
            jts.setTitle("Reporte Certificaciones");
            jts.setSubtitle("");

            JaosLibUtils libUtils = new JaosLibUtils();

            fieldTitle(nameof(PaymentsReportModel.contractCode)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(PaymentsReportModel.contractName)!);
            fieldTitle(nameof(PaymentsReportModel.paymentCode)!, $"numCel");
            fieldTitle(nameof(PaymentsReportModel.paymentRecord)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(PaymentsReportModel.stageName)!);
            fieldTitle(nameof(PaymentsReportModel.typeName)!, $"{JaoTable.screenOnlyClass}");
            fieldTitle(nameof(PaymentsReportModel.reportedMonth)!, $"numCel");
            fieldTitle(nameof(PaymentsReportModel.paymentValue)!, $"numCel");
            fieldTitle(nameof(PaymentsReportModel.PhysicalAdvance)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(PaymentsReportModel.totalValue)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(PaymentsReportModel.DateDelivery)!, $"numCel");
            fieldTitle(nameof(PaymentsReportModel.DateApproved)!, $"numCel");
            fieldTitle(nameof(PaymentsReportModel.DatePayed)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(PaymentsReportModel.attachedMedicion)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(PaymentsReportModel.attachedOrden)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(PaymentsReportModel.otherAttachments)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(PaymentsReportModel.PaymentDelay)!, $"numCel");

            foreach (var record in records)
            {
                jts.addRow();

                jts.addCell(record.contractCode, JaoTable.screenOnlyClass);
                jts.addCell(record.contractName);
                jts.addCell(record.paymentCode);
                jts.addCell(record.paymentRecord, JaoTable.screenOnlyClass);
                jts.addCell(record.stageName);
                jts.addCell(record.typeName, JaoTable.screenOnlyClass);
                jts.addCell(record.reportedMonth);
                jts.addCell(record.paymentValue, JaoTable.screenOnlyClass);
                jts.addCell(record.PhysicalAdvance, JaoTable.screenOnlyClass);
                jts.addCell(record.totalValue);
                jts.addCell(record.DateDelivery);
                jts.addCell(record.DateApproved);
                jts.addCell(record.DatePayed, JaoTable.screenOnlyClass);
                jts.addCell(record.attachedMedicion, JaoTable.screenOnlyClass);
                jts.addCell(record.attachedOrden, JaoTable.screenOnlyClass);
                jts.addCell(record.otherAttachments, JaoTable.screenOnlyClass);
                jts.addCell(record.PaymentDelay);



            }
            return jts.getTable();
        }


        void fieldTitle(string fieldName, string cssClass = "")
        {
            jts.addHeaderCell(libUtils.titleOf<PaymentsReportModel>(fieldName));
        }




    }
}
