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
    public partial class ContractorController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly ILogger<ContractorController> logger;


        public ContractorController(PIPMUNI_ARGDbContext context
        , ILogger<ContractorController> logger
        )
        {
            this.context = context;
            this.logger = logger;
        }

        JaosLibUtils jaosLibUtils = new JaosLibUtils();

        #region Index


        //----------- Index

        // GET: Contractor/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, string? returnUrl, string? bufferedUrl)
        {
            var contractors = from o in context.Contractor select o;
            if (!string.IsNullOrEmpty(searchText))
                contractors = contractors.Where(p => p.OfficialID!.Contains(searchText) || p.Name!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (contractors != null)
            {
                contractors = orderBySelectedOrDefault(sortOrder, contractors);
                return View(await contractors.ToListAsync());
            }
            return View(new List<Contractor>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: Contractor/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get Contractor
            if ((id == null) || (id <= 0) || (context.Contractor == null)) return NotFound();
            Contractor? contractor = await context.Contractor
                .FindAsync(id);
            if (contractor == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, returnUrl, bufferedUrl);

            return View(contractor);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: Contractor/Create
        [HttpGet]
        public IActionResult Create(string? returnUrl, string? bufferedUrl)
        {
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);

            return View();
        }



        // POST: Contractor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Contractor contractor, string? returnUrl, string? bufferedUrl)
        {
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(contractor);
                    await context.SaveChangesAsync();
                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(contractor));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = contractor.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Contratista";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            return View(contractor);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: Contractor/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get Contractor
            if ((id == null) || (id <= 0) || (context.Contractor == null)) return NotFound();
            Contractor? contractor = await context.Contractor
                .FindAsync(id);
            if (contractor == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, returnUrl, bufferedUrl);

            return View(contractor);
        }


        // POST: Contractor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] Contractor contractor, string? returnUrl, string? bufferedUrl)
        {
            if (id != contractor.Id)
            {
                return NotFound();
            }
            // Check if is officialID unique
            if (context.Contractor.Any(c => c.OfficialID == contractor.OfficialID && c.Id != contractor.Id))
            {
                ModelState.AddModelError("OfficialID", "CUIT existe en otro registro.");
            }
            // Check if is name unique
            if (context.Contractor.Any(c => c.Name == contractor.Name && c.Id != contractor.Id))
            {
                ModelState.AddModelError("Name", "Nombre existe en otro registro.");
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(contractor);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(contractor));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!ContractorExists(contractor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Contratista. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Contratista";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, returnUrl, bufferedUrl);

            return View(contractor);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: Contractor/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.Contractor == null)) return NotFound();
            Contractor? contractor = await context.Contractor
                .FirstAsync(r => r.Id == id);
            
            if (contractor == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            bool isLinked = await findExistingLinks(contractor);
            if (contractor != null && !isLinked)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Contractor.Remove(contractor);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(contractor));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Contratista";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }

            //---- if not saved reload view ----
            // set ViewBags
            setStandardViewBags("Delete", false, returnUrl, bufferedUrl);
            jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
            return Redirect(HttpContext.Request.Headers["Referer"]);
        }



        // Finds all the registers that are using the current registry from Contratista
        async Task<bool> findExistingLinks(Contractor contractor)
        {
            bool isLinked = false;
            string externalLinks = string.Empty;

            //search for Obras using this Contratista
            List<Contract>             contracts = await context.Contract
                .Include(p => p.Project_info)
                .Include(p => p.Stage_info)
                .Include(p => p.Office_info)
                .Include(p => p.Type_info)
                .Include(p => p.Contractor_info)
                .Where(r => r.Contractor == contractor.Id).ToListAsync();
            if (contracts?.Count > 0)
            {
                if (!isLinked) isLinked = true;
                externalLinks += "Se usa en  Obras:<br/>";
                foreach (Contract contract1 in contracts)
                    externalLinks += contract1?.Code + " - " + contract1?.Name + "<br/>";
                externalLinks += "<br/>";
            }

            if (isLinked) externalLinks = "Contratista no puede borrarse<br/>" + externalLinks;
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
        IQueryable<Contractor> orderBySelectedOrDefault(string sortOrder, IQueryable<Contractor> contractors)
        {
            ViewBag.nameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.officialIDSort = sortOrder == "officialID" ? "officialID_desc" : "officialID";
            ViewBag.officialIDIcon = "bi-caret-down";
            ViewBag.nameIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "officialID_desc":
                    contractors = contractors.OrderByDescending(o => o.OfficialID);
                    ViewBag.officialIDIcon = "bi-caret-up-fill";
                    break;
                case "officialID":
                    contractors = contractors.OrderBy(o => o.OfficialID);
                    ViewBag.officialIDIcon = "bi-caret-down-fill";
                    break;
                case "name_desc":
                    contractors = contractors.OrderByDescending(t => t.Name);
                    ViewBag.nameIcon = "bi-caret-up-fill";
                    break;
                default:
                    contractors = contractors.OrderBy(t => t.Name);
                    ViewBag.nameIcon = "bi-caret-down-fill";
                    break;
            }
            return contractors;
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
            // navContractor
            ViewBag.navContractor = $"{action}";
        }


        //----------------------------------------------
        //==============================================
        #endregion


        //========================================================

        private bool ContractorExists(int id)
        {
            return (context.Contractor?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
