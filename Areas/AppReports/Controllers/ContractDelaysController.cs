using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Dapper;
using JaosLib.Models.JaoTables;
using JaosLib.Services.JaoTables;
using PIPMUNI_ARG.Areas.AppReports.Models;
using PIPMUNI_ARG.Data;
using PIPMUNI_ARG.Services.Utilities;

namespace PIPMUNI_ARG.Areas.AppReports.Controllers
{
    [Area("AppReports")]
    [Authorize(Roles = ProjectGlobals.registeredRoles)]
    public class ContractDelaysController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly IJaoTableServices jts;
        private readonly IJaoTableExcelServices jtExcel;


        JaosLibUtils libUtils = new JaosLibUtils();

        public ContractDelaysController(PIPMUNI_ARGDbContext context
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
            MemoryStream memoryStream = jtExcel.createExcelFile(jaoTable, "Obras Demoras", "Report", Response);
            return File(memoryStream, JaoTableExcelServices.fileStyle, jtExcel.fileName);
        }



        //==============================================
        //----------------------------------------------



        async Task<JaoTable> fillTable()
        {
            const string dbView = "[report_Gaps]";
            List<ContractDelaysModel> records;
            SqlConnection sqlConnection = new SqlConnection(context.Database.GetConnectionString());
            try
            {
                sqlConnection.Open();
                records = (await sqlConnection.QueryAsync<ContractDelaysModel>($"select * from {dbView}")).ToList();
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



        JaoTable prepareTable(List<ContractDelaysModel> records)
        {
            jts.setExcelWidths(new float[] { 50, 300
                , 50, 50, 50, 50, 50, 50
                , 50, 50, 50, 50, 50
                , 50, 50, 50, 50, 50
                , 50, 50, 50, 50, 50
            });
            jts.setTitle("Reporte de Gestión de Obra");
            jts.setSubtitle("");

            JaosLibUtils libUtils = new JaosLibUtils();

            fieldTitle(nameof(ContractDelaysModel.ContractCode)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(ContractDelaysModel.ContractName)!);

            fieldTitle(nameof(ContractDelaysModel.Payment_QtyPayed)!, $"numCel");
            fieldTitle(nameof(ContractDelaysModel.Payment_QtyPending)!, $"numCel");
            fieldTitle(nameof(ContractDelaysModel.Payment_LastPayedDate)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(ContractDelaysModel.Payment_LastDeliveryDate)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(ContractDelaysModel.Payment_LastPayedDelay)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(ContractDelaysModel.Payment_OldestDeliveryDelay)!, $"numCel");

            fieldTitle(nameof(ContractDelaysModel.Addition_QtyApproved)!, $"numCel");
            fieldTitle(nameof(ContractDelaysModel.Addition_QtyPending)!, $"numCel");
            fieldTitle(nameof(ContractDelaysModel.Addition_LastApprovedPeriod)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(ContractDelaysModel.Addition_LastApprovedDelay)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(ContractDelaysModel.Addition_OldestDeliveryDelay)!, $"numCel");

            fieldTitle(nameof(ContractDelaysModel.Variante_QtyApproved)!, $"numCel");
            fieldTitle(nameof(ContractDelaysModel.Variante_QtyPending)!, $"numCel");
            fieldTitle(nameof(ContractDelaysModel.Variante_LastApprovedDate)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(ContractDelaysModel.Variante_LastApprovedDelay)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(ContractDelaysModel.Variante_OldestDeliveryDelay)!, $"numCel");

            fieldTitle(nameof(ContractDelaysModel.Extension_QtyApproved)!, $"numCel");
            fieldTitle(nameof(ContractDelaysModel.Extension_QtyPending)!, $"numCel");
            fieldTitle(nameof(ContractDelaysModel.Extension_LastApprovedDate)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(ContractDelaysModel.Extension_LastApprovedDelay)!, $"numCel {JaoTable.screenOnlyClass}");
            fieldTitle(nameof(ContractDelaysModel.Extension_OldestDeliveryDelay)!, $"numCel");

            foreach (var record in records)
            {
                jts.addRow();

                jts.addCell(record.ContractCode, JaoTable.screenOnlyClass);
                jts.addCell(record.ContractName, JaoTable.screenOnlyClass);

                jts.addCell(record.Payment_QtyPayed, JaoTable.screenOnlyClass);
                jts.addCell(record.Payment_QtyPending, JaoTable.screenOnlyClass);
                jts.addCell(record.Payment_LastPayedDate, JaoTable.screenOnlyClass);
                jts.addCell(record.Payment_LastDeliveryDate, JaoTable.screenOnlyClass);
                jts.addCell(record.Payment_LastPayedDelay, JaoTable.screenOnlyClass);
                jts.addCell(record.Payment_OldestDeliveryDelay, JaoTable.screenOnlyClass);

                jts.addCell(record.Addition_QtyApproved, JaoTable.screenOnlyClass);
                jts.addCell(record.Addition_QtyPending, JaoTable.screenOnlyClass);
                jts.addCell(record.Addition_LastApprovedPeriod, JaoTable.screenOnlyClass);
                jts.addCell(record.Addition_LastApprovedDelay, JaoTable.screenOnlyClass);
                jts.addCell(record.Addition_OldestDeliveryDelay, JaoTable.screenOnlyClass);

                jts.addCell(record.Variante_QtyApproved, JaoTable.screenOnlyClass);
                jts.addCell(record.Variante_QtyPending, JaoTable.screenOnlyClass);
                jts.addCell(record.Variante_LastApprovedDate, JaoTable.screenOnlyClass);
                jts.addCell(record.Variante_LastApprovedDelay, JaoTable.screenOnlyClass);
                jts.addCell(record.Variante_OldestDeliveryDelay, JaoTable.screenOnlyClass);

                jts.addCell(record.Extension_QtyApproved, JaoTable.screenOnlyClass);
                jts.addCell(record.Extension_QtyPending, JaoTable.screenOnlyClass);
                jts.addCell(record.Extension_LastApprovedDate, JaoTable.screenOnlyClass);
                jts.addCell(record.Extension_LastApprovedDelay, JaoTable.screenOnlyClass);
                jts.addCell(record.Extension_OldestDeliveryDelay, JaoTable.screenOnlyClass);


            }
            return jts.getTable();
        }


        void fieldTitle(string fieldName, string cssClass = "")
        {
            jts.addHeaderCell(libUtils.titleOf<ContractDelaysModel>(fieldName));
        }



    }
}
