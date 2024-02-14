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

namespace PIPMUNI_ARG.Controllers
{
    [Authorize(Roles =ProjectGlobals.registeredRoles)]
    public partial class ContractTypeController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly ILogger<ContractTypeController> logger;


        public ContractTypeController(PIPMUNI_ARGDbContext context
        , ILogger<ContractTypeController> logger
        )
        {
            this.context = context;
            this.logger = logger;
        }

        JaosLibUtils jaosLibUtils = new JaosLibUtils();

        #region Index


        //----------- Index

        // GET: ContractType/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, string? returnUrl, string? bufferedUrl)
        {
            var contractTypes = from o in context.ContractType select o;
            if (!string.IsNullOrEmpty(searchText))
                contractTypes = contractTypes.Where(p => p.Name!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (contractTypes != null)
            {
                contractTypes = orderBySelectedOrDefault(sortOrder, contractTypes);
                return View(await contractTypes.ToListAsync());
            }
            return View(new List<ContractType>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: ContractType/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get ContractType
            if ((id == null) || (id <= 0) || (context.ContractType == null)) return NotFound();
            ContractType? contractType = await context.ContractType
                .FindAsync(id);
            if (contractType == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, returnUrl, bufferedUrl);

            return View(contractType);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: ContractType/Create
        [HttpGet]
        public IActionResult Create(string? returnUrl, string? bufferedUrl)
        {
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);

            return View();
        }



        // POST: ContractType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ContractType contractType, string? returnUrl, string? bufferedUrl)
        {
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(contractType);
                    await context.SaveChangesAsync();
                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(contractType));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = contractType.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Modalidad de Ejecución";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            return View(contractType);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: ContractType/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get ContractType
            if ((id == null) || (id <= 0) || (context.ContractType == null)) return NotFound();
            ContractType? contractType = await context.ContractType
                .FindAsync(id);
            if (contractType == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, returnUrl, bufferedUrl);

            return View(contractType);
        }


        // POST: ContractType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] ContractType contractType, string? returnUrl, string? bufferedUrl)
        {
            if (id != contractType.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(contractType);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(contractType));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!ContractTypeExists(contractType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Modalidad de Ejecución. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Modalidad de Ejecución";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, returnUrl, bufferedUrl);

            return View(contractType);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: ContractType/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.ContractType == null)) return NotFound();
            ContractType? contractType = await context.ContractType
                .FirstAsync(r => r.Id == id);
            
            if (contractType == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            bool isLinked = await findExistingLinks(contractType);
            if (contractType != null && !isLinked)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.ContractType.Remove(contractType);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(contractType));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Modalidad de Ejecución";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }

            //---- if not saved reload view ----
            // set ViewBags
            setStandardViewBags("Delete", false, returnUrl, bufferedUrl);
            jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
            return Redirect(HttpContext.Request.Headers["Referer"]);
        }



        // Finds all the registers that are using the current registry from Modalidad de Ejecución
        async Task<bool> findExistingLinks(ContractType contractType)
        {
            bool isLinked = false;
            string externalLinks = string.Empty;

            //search for Obras using this Modalidad de Ejecución
            List<Contract>             contracts = await context.Contract
                .Include(p => p.Project_info)
                .Include(p => p.Stage_info)
                .Include(p => p.Office_info)
                .Include(p => p.Type_info)
                .Include(p => p.Contractor_info)
                .Where(r => r.Type == contractType.Id).ToListAsync();
            if (contracts?.Count > 0)
            {
                if (!isLinked) isLinked = true;
                externalLinks += "Se usa en  Obras:<br/>";
                foreach (Contract contract1 in contracts)
                    externalLinks += contract1?.Code + " - " + contract1?.Name + "<br/>";
                externalLinks += "<br/>";
            }

            if (isLinked) externalLinks = "Modalidad de Ejecución no puede borrarse<br/>" + externalLinks;
            ViewBag.warningMessage = externalLinks;
            return isLinked;
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
        IQueryable<ContractType> orderBySelectedOrDefault(string sortOrder, IQueryable<ContractType> contractTypes)
        {
            ViewBag.nameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.nameIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "name_desc":
                    contractTypes = contractTypes.OrderByDescending(t => t.Name);
                    ViewBag.nameIcon = "bi-caret-up-fill";
                    break;
                default:
                    contractTypes = contractTypes.OrderBy(t => t.Name);
                    ViewBag.nameIcon = "bi-caret-down-fill";
                    break;
            }
            return contractTypes;
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
            // navContractType
            ViewBag.navContractType = $"{action}";
        }


        //----------------------------------------------
        //==============================================
        #endregion


        //========================================================

        private bool ContractTypeExists(int id)
        {
            return (context.ContractType?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
