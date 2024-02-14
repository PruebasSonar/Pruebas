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
    public partial class SubsectorController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly ILogger<SubsectorController> logger;


        public SubsectorController(PIPMUNI_ARGDbContext context
        , ILogger<SubsectorController> logger
        )
        {
            this.context = context;
            this.logger = logger;
        }

        JaosLibUtils jaosLibUtils = new JaosLibUtils();

        #region Index


        //----------- Index

        // GET: Subsector/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, int? sectorId, string? returnUrl, string? bufferedUrl)
        {
            var subsectors = sectorId.HasValue
            ? from o in context.Subsector where (o.Sector == sectorId) select o
            : from o in context.Subsector select o;
            if (!string.IsNullOrEmpty(searchText))
                subsectors = subsectors.Where(p => p.Sector_info!.Name!.Contains(searchText) || p.Name!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, sectorId, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (subsectors != null)
            {
                subsectors = orderBySelectedOrDefault(sortOrder, subsectors);
                subsectors = subsectors
                    .Include(p => p.Sector_info);
                return View(await subsectors.ToListAsync());
            }
            return View(new List<Subsector>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: Subsector/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, int? sectorId, string? returnUrl, string? bufferedUrl)
        {
            // get Subsector
            if ((id == null) || (id <= 0) || (context.Subsector == null)) return NotFound();
            Subsector? subsector = await context.Subsector
                .FindAsync(id);
            if (subsector == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, sectorId, returnUrl, bufferedUrl);
            setViewBagsForLists(subsector);

            return View(subsector);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: Subsector/Create
        [HttpGet]
        public IActionResult Create(int? sectorId, string? returnUrl, string? bufferedUrl)
        {
            // set ViewBags
            setStandardViewBags("Create", true, sectorId, returnUrl, bufferedUrl);
            setViewBagsForLists(null);

            Subsector subsector = new Subsector();
            if (sectorId.HasValue) subsector.Sector = sectorId.Value;
            return View(subsector);
        }



        // POST: Subsector/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Subsector subsector, int? sectorId, string? returnUrl, string? bufferedUrl)
        {
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(subsector);
                    await context.SaveChangesAsync();
                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(subsector));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = subsector.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Subsector";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, sectorId, returnUrl, bufferedUrl);
            setViewBagsForLists(subsector);
            return View(subsector);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: Subsector/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, int? sectorId, string? returnUrl, string? bufferedUrl)
        {
            // get Subsector
            if ((id == null) || (id <= 0) || (context.Subsector == null)) return NotFound();
            Subsector? subsector = await context.Subsector
                .FindAsync(id);
            if (subsector == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, sectorId, returnUrl, bufferedUrl);
            setViewBagsForLists(subsector);

            return View(subsector);
        }


        // POST: Subsector/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] Subsector subsector, int? sectorId, string? returnUrl, string? bufferedUrl)
        {
            if (id != subsector.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(subsector);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(subsector));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!SubsectorExists(subsector.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Subsector. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Subsector";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, sectorId, returnUrl, bufferedUrl);
            setViewBagsForLists(subsector);

            return View(subsector);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: Subsector/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, int? sectorId, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.Subsector == null)) return NotFound();
            Subsector? subsector = await context.Subsector
                .FirstAsync(r => r.Id == id);
            
            if (subsector == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            bool isLinked = await findExistingLinks(subsector);
            if (subsector != null && !isLinked)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Subsector.Remove(subsector);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(subsector));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Subsector";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }

            //---- if not saved reload view ----
            // set ViewBags
            setStandardViewBags("Delete", false, sectorId, returnUrl, bufferedUrl);
            jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
            return Redirect(HttpContext.Request.Headers["Referer"]);
        }



        // Finds all the registers that are using the current registry from Subsector
        async Task<bool> findExistingLinks(Subsector subsector)
        {
            bool isLinked = false;
            string externalLinks = string.Empty;

            //search for Proyectos using this Subsector
            List<Project>             projects = await context.Project
                .Include(p => p.Sector_info)
                .Include(p => p.Subsector_info)
                .Include(p => p.Stage_info)
                .Include(p => p.Office_info)
                .Where(r => r.Subsector == subsector.Id).ToListAsync();
            if (projects?.Count > 0)
            {
                if (!isLinked) isLinked = true;
                externalLinks += "Se usa en  Proyectos:<br/>";
                foreach (Project project1 in projects)
                    externalLinks += project1?.Name + " - " + project1?.Code + "<br/>";
                externalLinks += "<br/>";
            }

            if (isLinked) externalLinks = "Subsector no puede borrarse<br/>" + externalLinks;
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
        IQueryable<Subsector> orderBySelectedOrDefault(string sortOrder, IQueryable<Subsector> subsectors)
        {
            ViewBag.sectorSort = String.IsNullOrEmpty(sortOrder) ? "sector_desc" : "";
            ViewBag.nameSort = sortOrder == "name" ? "name_desc" : "name";
            ViewBag.sectorIcon = "bi-caret-down";
            ViewBag.nameIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "name_desc":
                    subsectors = subsectors.OrderByDescending(o => o.Name);
                    ViewBag.nameIcon = "bi-caret-up-fill";
                    break;
                case "name":
                    subsectors = subsectors.OrderBy(o => o.Name);
                    ViewBag.nameIcon = "bi-caret-down-fill";
                    break;
                case "sector_desc":
                    subsectors = subsectors.OrderByDescending(t => t.Name).OrderByDescending(t => t.Sector);
                    ViewBag.sectorIcon = "bi-caret-up-fill";
                    break;
                default:
                    subsectors = subsectors.OrderBy(t => t.Name).OrderBy(t => t.Sector);
                    ViewBag.sectorIcon = "bi-caret-down-fill";
                    break;
            }
            return subsectors;
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
        void setStandardViewBags(string action, bool returnsbyDefault, int? sectorId, string? returnUrl, string? bufferedUrl)
        {
            if (sectorId.HasValue) ViewBag.sectorId = sectorId;
            // returnUrl
            if (!string.IsNullOrEmpty(returnUrl))
                ViewBag.returnUrl = returnUrl;
            else if (returnsbyDefault)
                ViewBag.returnUrl = HttpContext.Request.Headers["Referer"];
            // bufferedUrl
            if (!string.IsNullOrEmpty(bufferedUrl)) ViewBag.bufferedUrl = bufferedUrl;
            // navSubsector
            ViewBag.navSubsector = $"{action}";
        }


        void setViewBagsForLists(Subsector? subsector)
        {

            // set options for Sector
            var listSector = new SelectList(context.Sector.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Name, r.Id.ToString())), "Value", "Text", subsector?.Sector).ToList();
            listSector.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listSector = listSector;
        }


        //----------------------------------------------
        //==============================================
        #endregion


        //========================================================

        private bool SubsectorExists(int id)
        {
            return (context.Subsector?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
