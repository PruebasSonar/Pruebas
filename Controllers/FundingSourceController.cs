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
    public partial class FundingSourceController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly ILogger<FundingSourceController> logger;


        public FundingSourceController(PIPMUNI_ARGDbContext context
        , ILogger<FundingSourceController> logger
        )
        {
            this.context = context;
            this.logger = logger;
        }

        JaosLibUtils jaosLibUtils = new JaosLibUtils();

        #region Index


        //----------- Index

        // GET: FundingSource/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, string? returnUrl, string? bufferedUrl)
        {
            var fundingSources = from o in context.FundingSource select o;
            if (!string.IsNullOrEmpty(searchText))
                fundingSources = fundingSources.Where(p => p.Name!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (fundingSources != null)
            {
                fundingSources = orderBySelectedOrDefault(sortOrder, fundingSources);
                return View(await fundingSources.ToListAsync());
            }
            return View(new List<FundingSource>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: FundingSource/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get FundingSource
            if ((id == null) || (id <= 0) || (context.FundingSource == null)) return NotFound();
            FundingSource? fundingSource = await context.FundingSource
                .FindAsync(id);
            if (fundingSource == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, returnUrl, bufferedUrl);

            return View(fundingSource);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: FundingSource/Create
        [HttpGet]
        public IActionResult Create(string? returnUrl, string? bufferedUrl)
        {
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);

            return View();
        }



        // POST: FundingSource/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] FundingSource fundingSource, string? returnUrl, string? bufferedUrl)
        {
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(fundingSource);
                    await context.SaveChangesAsync();
                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(fundingSource));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = fundingSource.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Organísmos de Financiamiento";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            return View(fundingSource);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: FundingSource/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get FundingSource
            if ((id == null) || (id <= 0) || (context.FundingSource == null)) return NotFound();
            FundingSource? fundingSource = await context.FundingSource
                .FindAsync(id);
            if (fundingSource == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, returnUrl, bufferedUrl);

            return View(fundingSource);
        }


        // POST: FundingSource/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] FundingSource fundingSource, string? returnUrl, string? bufferedUrl)
        {
            if (id != fundingSource.Id)
            {
                return NotFound();
            }
            // Check if is name unique
            if (context.FundingSource.Any(c => c.Name == fundingSource.Name && c.Id != fundingSource.Id))
            {
                ModelState.AddModelError("Name", "Nombre existe en otro registro.");
            }
            // Check if is acronym unique
            if (context.FundingSource.Any(c => c.Acronym == fundingSource.Acronym && c.Id != fundingSource.Id))
            {
                ModelState.AddModelError("Acronym", "Sigla existe en otro registro.");
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(fundingSource);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(fundingSource));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!FundingSourceExists(fundingSource.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Organísmos de Financiamiento. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Organísmos de Financiamiento";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, returnUrl, bufferedUrl);

            return View(fundingSource);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: FundingSource/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.FundingSource == null)) return NotFound();
            FundingSource? fundingSource = await context.FundingSource
                .FirstAsync(r => r.Id == id);
            
            if (fundingSource == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            bool isLinked = await findExistingLinks(fundingSource);
            if (fundingSource != null && !isLinked)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.FundingSource.Remove(fundingSource);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(fundingSource));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Organísmos de Financiamiento";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }

            //---- if not saved reload view ----
            // set ViewBags
            setStandardViewBags("Delete", false, returnUrl, bufferedUrl);
            jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
            return Redirect(HttpContext.Request.Headers["Referer"]);
        }



        // Finds all the registers that are using the current registry from Organísmos de Financiamiento
        async Task<bool> findExistingLinks(FundingSource fundingSource)
        {
            bool isLinked = false;
            string externalLinks = string.Empty;

            //search for Fuente Financiamientos using this Organísmos de Financiamiento
            List<ProjectSource>             projectSources = await context.ProjectSource
                .Include(p => p.Project_info)
                .Include(p => p.Source_info)
                .Where(r => r.Source == fundingSource.Id).ToListAsync();
            if (projectSources?.Count > 0)
            {
                if (!isLinked) isLinked = true;
                externalLinks += "Se usa en  Fuente Financiamientos:<br/>";
                foreach (ProjectSource projectSource1 in projectSources)
                    externalLinks += projectSource1?.Project_info?.Name + " - " + projectSource1?.Project_info?.Code + " - " + projectSource1?.Source_info?.Name + "<br/>";
                externalLinks += "<br/>";
            }

            if (isLinked) externalLinks = "Organísmos de Financiamiento no puede borrarse<br/>" + externalLinks;
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
        IQueryable<FundingSource> orderBySelectedOrDefault(string sortOrder, IQueryable<FundingSource> fundingSources)
        {
            ViewBag.nameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.nameIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "name_desc":
                    fundingSources = fundingSources.OrderByDescending(t => t.Name);
                    ViewBag.nameIcon = "bi-caret-up-fill";
                    break;
                default:
                    fundingSources = fundingSources.OrderBy(t => t.Name);
                    ViewBag.nameIcon = "bi-caret-down-fill";
                    break;
            }
            return fundingSources;
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
            // navFundingSource
            ViewBag.navFundingSource = $"{action}";
        }


        //----------------------------------------------
        //==============================================
        #endregion


        //========================================================

        private bool FundingSourceExists(int id)
        {
            return (context.FundingSource?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
