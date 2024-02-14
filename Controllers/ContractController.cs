using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using NuGet.Protocol;
using Newtonsoft.Json;
using PIPMUNI_ARG.Data;
using PIPMUNI_ARG.Models.Domain;
using PIPMUNI_ARG.Services.Utilities;
using PIPMUNI_ARG.Services.basic;

namespace PIPMUNI_ARG.Controllers
{
    [Authorize(Roles =ProjectGlobals.registeredRoles)]
    public partial class ContractController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly IParentContractService parentContractService;
        private readonly ILogger<ContractController> logger;


        public ContractController(PIPMUNI_ARGDbContext context
        , IParentContractService parentContractService
        , ILogger<ContractController> logger
        )
        {
            this.context = context;
            this.parentContractService = parentContractService;
            this.logger = logger;
        }

        JaosLibUtils jaosLibUtils = new JaosLibUtils();

        #region Index


        //----------- Index

        // GET: Contract/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, string? returnUrl, string? bufferedUrl)
        {
            var contracts = from o in context.Contract select o;
            if (!string.IsNullOrEmpty(searchText))
                contracts = contracts.Where(p => p.Code!.Contains(searchText) || p.Name!.Contains(searchText) || p.Stage_info!.Name!.Contains(searchText) || p.Office_info!.Name!.Contains(searchText) || p.Contractor_info!.Name!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (contracts != null)
            {
                contracts = orderBySelectedOrDefault(sortOrder, contracts);
                contracts = contracts
                    .Include(p => p.Stage_info)
                    .Include(p => p.Office_info)
                    .Include(p => p.Contractor_info);
                return View(await contracts.ToListAsync());
            }
            return View(new List<Contract>());
        }


        #endregion
        #region Select


        //----------- Select

        // GET: Contract/Select
        [HttpGet]
        public async Task<IActionResult> Select(string sortOrder, string searchText, string? returnUrl, string? bufferedUrl)
        {
            var contracts = from o in context.Contract select o;
            if (!string.IsNullOrEmpty(searchText))
                contracts = contracts.Where(p => p.Code!.Contains(searchText) || p.Name!.Contains(searchText) || p.Stage_info!.Name!.Contains(searchText) || p.Office_info!.Name!.Contains(searchText) || p.Contractor_info!.Name!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Select", false, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (contracts != null)
            {
                contracts = orderBySelectedOrDefault(sortOrder, contracts);
                contracts = contracts
                    .Include(p => p.Stage_info)
                    .Include(p => p.Office_info)
                    .Include(p => p.Contractor_info);
                return View(await contracts.ToListAsync());
            }
            else
                return View(null);
        }
        #endregion
        #region Display


        //----------- Display

        // GET: Contract/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, string? returnUrl, string? bufferedUrl)
        {
            Contract? contract = await parentContractService.getContractFromIdOrSession(id, User, HttpContext.Session, ViewBag);
            if (contract == null) return await Task.Run(() => RedirectToAction("Select", "Contract"));
            // set ViewBags
            setStandardViewBags("Display", true, returnUrl, bufferedUrl);
            setViewBagsForLists(contract);

            return View(contract);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: Contract/Create
        [HttpGet]
        public IActionResult Create(string? returnUrl, string? bufferedUrl)
        {
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            setViewBagsForLists(null);

            return View();
        }



        // POST: Contract/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Contract contract, string? returnUrl, string? bufferedUrl)
        {
            unselectedLinksNulled(contract);
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(contract);
                    await context.SaveChangesAsync();

                    // Assign Code when created
                    contract.Code = (contract.Id + 1000).ToString("d5");
                    context.Update(contract);
                    await context.SaveChangesAsync();

                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(contract));
                    transaction.Commit();
                    parentContractService.setSessionContract(contract, HttpContext.Session);
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = contract.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Obra";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
            {
                ViewBag.OriginalValueValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "OriginalValue");
                ViewBag.PlazoOriginalValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "PlazoOriginal");
            }

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            setViewBagsForLists(contract);
            return View(contract);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: Contract/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string? returnUrl, string? bufferedUrl)
        {
            Contract? contract = await parentContractService.getContractFromIdOrSession(id, User, HttpContext.Session, ViewBag);
            if (contract == null) return await Task.Run(() => RedirectToAction("Select", "Contract"));

            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, returnUrl, bufferedUrl);
            setViewBagsForLists(contract);

            return View(contract);
        }


        // POST: Contract/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] Contract contract, string? returnUrl, string? bufferedUrl)
        {
            unselectedLinksNulled(contract);
            // Check if is code unique
            if (context.Contract.Any(c => c.Code == contract.Code && c.Id != contract.Id))
            {
                ModelState.AddModelError("Code", "Código interno existe en otro registro.");
            }
            // Check if is name unique
            if (context.Contract.Any(c => c.Name == contract.Name && c.Id != contract.Id))
            {
                ModelState.AddModelError("Name", "Nombre o título existe en otro registro.");
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(contract);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(contract));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!ContractExists(contract.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Obra. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Obra";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
            {
                ViewBag.OriginalValueValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "OriginalValue");
                ViewBag.PlazoOriginalValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "PlazoOriginal");
            }

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, returnUrl, bufferedUrl);
            setViewBagsForLists(contract);

            return View(contract);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: Contract/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.Contract == null)) return NotFound();
            Contract? contract = await context.Contract
                .Include(t => t.Extensions!)
                .ThenInclude(f => f.ExtensionAttachments)
                .Include(t => t.Payments!)
                .ThenInclude(f => f.PaymentAttachments)
                .Include(t => t.Additions!)
                .ThenInclude(f => f.AdditionAttachments)
                .Include(t => t.Variantes!)
                .ThenInclude(f => f.VarianteAttachments)
                .FirstAsync(r => r.Id == id);
            
            if (contract == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            if (contract != null)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    deleteChildren(contract);
                    await context.SaveChangesAsync();
                    context.Contract.Remove(contract);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(contract));
                    transaction.Commit();

                    parentContractService.setSessionContract(null, HttpContext.Session);
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Obra";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }

            //---- if not saved reload view ----
            // set ViewBags
            setStandardViewBags("Delete", false, returnUrl, bufferedUrl);
            jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
            return Redirect(HttpContext.Request.Headers["Referer"]);
        }
        #endregion
        #region Buttons


        //----------- Buttons


        [HttpPost]
        public async Task<IActionResult> Search()
        {
            return await Task.Run(() => RedirectToAction("Index"));
        }


        //----------------------------------------------
        //==============================================
        //----------------------------------------------
        #endregion
        #region Supporting Methods

         //-- Sort Table by default or user selected order
        IQueryable<Contract> orderBySelectedOrDefault(string sortOrder, IQueryable<Contract> contracts)
        {
            ViewBag.nameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.codeSort = sortOrder == "code" ? "code_desc" : "code";
            ViewBag.stageSort = sortOrder == "stage" ? "stage_desc" : "stage";
            ViewBag.officeSort = sortOrder == "office" ? "office_desc" : "office";
            ViewBag.contractorSort = sortOrder == "contractor" ? "contractor_desc" : "contractor";
            ViewBag.codeIcon = "bi-caret-down";
            ViewBag.nameIcon = "bi-caret-down";
            ViewBag.stageIcon = "bi-caret-down";
            ViewBag.officeIcon = "bi-caret-down";
            ViewBag.contractorIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "code_desc":
                    contracts = contracts.OrderByDescending(o => o.Code);
                    ViewBag.codeIcon = "bi-caret-up-fill";
                    break;
                case "code":
                    contracts = contracts.OrderBy(o => o.Code);
                    ViewBag.codeIcon = "bi-caret-down-fill";
                    break;
                case "stage_desc":
                    contracts = contracts.OrderByDescending(o => o.Stage);
                    ViewBag.stageIcon = "bi-caret-up-fill";
                    break;
                case "stage":
                    contracts = contracts.OrderBy(o => o.Stage);
                    ViewBag.stageIcon = "bi-caret-down-fill";
                    break;
                case "office_desc":
                    contracts = contracts.OrderByDescending(o => o.Office);
                    ViewBag.officeIcon = "bi-caret-up-fill";
                    break;
                case "office":
                    contracts = contracts.OrderBy(o => o.Office);
                    ViewBag.officeIcon = "bi-caret-down-fill";
                    break;
                case "contractor_desc":
                    contracts = contracts.OrderByDescending(o => o.Contractor);
                    ViewBag.contractorIcon = "bi-caret-up-fill";
                    break;
                case "contractor":
                    contracts = contracts.OrderBy(o => o.Contractor);
                    ViewBag.contractorIcon = "bi-caret-down-fill";
                    break;
                case "name_desc":
                    contracts = contracts.OrderByDescending(t => t.Name);
                    ViewBag.nameIcon = "bi-caret-up-fill";
                    break;
                default:
                    contracts = contracts.OrderBy(t => t.Name);
                    ViewBag.nameIcon = "bi-caret-down-fill";
                    break;
            }
            return contracts;
        }

        //==============================================
        //------------- Controller Methods -------------


        /// <summary>
        /// Assigns the standard ViewBags required for navigation.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="returnsbyDefault">If false. The return button will only be available 
        /// if returnUrl has a value.
        /// If true, will return to caller if no returnUrl is specified.</param>
        void setStandardViewBags(string action, bool returnsbyDefault, string? returnUrl, string? bufferedUrl)
        {
            // returnUrl
            if (!string.IsNullOrEmpty(returnUrl))
                ViewBag.returnUrl = returnUrl;
            else if (returnsbyDefault)
                ViewBag.returnUrl = HttpContext.Request.Headers["Referer"];
            // bufferedUrl
            if (!string.IsNullOrEmpty(bufferedUrl)) ViewBag.bufferedUrl = bufferedUrl;
            // navContract
            ViewBag.navContract = $"{action}";
        }


        void setViewBagsForLists(Contract? contract)
        {

            // set options for Project
            var listProject = new SelectList(context.Project.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Name + " - " + r.Code, r.Id.ToString())), "Value", "Text", contract?.Project).ToList();
            listProject.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listProject = listProject;

            // set options for Stage
            var listStage = new SelectList(context.ContractStage.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Name, r.Id.ToString())), "Value", "Text", contract?.Stage).ToList();
            listStage.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listStage = listStage;

            // set options for Office
            var listOffice = new SelectList(context.Office.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Name, r.Id.ToString())), "Value", "Text", contract?.Office).ToList();
            listOffice.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listOffice = listOffice;

            // set options for Type
            var listType = new SelectList(context.ContractType.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Name, r.Id.ToString())), "Value", "Text", contract?.Type).ToList();
            listType.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listType = listType;

            // set options for Contractor
            var listContractor = new SelectList(context.Contractor.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Name, r.Id.ToString())), "Value", "Text", contract?.Contractor).ToList();
            listContractor.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listContractor = listContractor;
        }


        void unselectedLinksNulled(Contract contract)
        {
            // sets unselected lists to null
            if (contract.Office == 0) contract.Office = null;
            if (contract.Type == 0) contract.Type = null;
            if (contract.Contractor == 0) contract.Contractor = null;

        }

        //----------------------------------------------
        //==============================================
        // delete children
        async void deleteChildren(Contract contract)
        {
            if (contract.Extensions?.Count > 0)
                foreach (var extension in contract.Extensions)
                {
                    if (extension.ExtensionAttachments?.Count > 0)
                        extension.ExtensionAttachments.ToList().ForEach(c => context.ExtensionAttachment.Remove(c));
                    context.Extension.Remove(extension);
                }
            if (contract.Payments?.Count > 0)
                foreach (var payment in contract.Payments)
                {
                    if (payment.PaymentAttachments?.Count > 0)
                        payment.PaymentAttachments.ToList().ForEach(c => context.PaymentAttachment.Remove(c));
                    context.Payment.Remove(payment);
                }
            if (contract.Additions?.Count > 0)
                foreach (var addition in contract.Additions)
                {
                    if (addition.AdditionAttachments?.Count > 0)
                        addition.AdditionAttachments.ToList().ForEach(c => context.AdditionAttachment.Remove(c));
                    context.Addition.Remove(addition);
                }
            if (contract.Variantes?.Count > 0)
                foreach (var variante in contract.Variantes)
                {
                    if (variante.VarianteAttachments?.Count > 0)
                        variante.VarianteAttachments.ToList().ForEach(c => context.VarianteAttachment.Remove(c));
                    context.Variante.Remove(variante);
                }
        }
        #endregion


        //========================================================

        private bool ContractExists(int id)
        {
            return (context.Contract?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
