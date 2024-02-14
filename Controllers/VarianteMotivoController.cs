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
    public partial class VarianteMotivoController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly ILogger<VarianteMotivoController> logger;


        public VarianteMotivoController(PIPMUNI_ARGDbContext context
        , ILogger<VarianteMotivoController> logger
        )
        {
            this.context = context;
            this.logger = logger;
        }

        JaosLibUtils jaosLibUtils = new JaosLibUtils();

        #region Index


        //----------- Index

        // GET: VarianteMotivo/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, string? returnUrl, string? bufferedUrl)
        {
            var varianteMotivoes = from o in context.VarianteMotivo select o;
            if (!string.IsNullOrEmpty(searchText))
                varianteMotivoes = varianteMotivoes.Where(p => p.Name!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (varianteMotivoes != null)
            {
                varianteMotivoes = orderBySelectedOrDefault(sortOrder, varianteMotivoes);
                return View(await varianteMotivoes.ToListAsync());
            }
            return View(new List<VarianteMotivo>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: VarianteMotivo/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get VarianteMotivo
            if ((id == null) || (id <= 0) || (context.VarianteMotivo == null)) return NotFound();
            VarianteMotivo? varianteMotivo = await context.VarianteMotivo
                .FindAsync(id);
            if (varianteMotivo == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, returnUrl, bufferedUrl);

            return View(varianteMotivo);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: VarianteMotivo/Create
        [HttpGet]
        public IActionResult Create(string? returnUrl, string? bufferedUrl)
        {
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);

            return View();
        }



        // POST: VarianteMotivo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] VarianteMotivo varianteMotivo, string? returnUrl, string? bufferedUrl)
        {
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(varianteMotivo);
                    await context.SaveChangesAsync();
                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(varianteMotivo));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = varianteMotivo.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Motivo Variante";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            return View(varianteMotivo);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: VarianteMotivo/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get VarianteMotivo
            if ((id == null) || (id <= 0) || (context.VarianteMotivo == null)) return NotFound();
            VarianteMotivo? varianteMotivo = await context.VarianteMotivo
                .FindAsync(id);
            if (varianteMotivo == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, returnUrl, bufferedUrl);

            return View(varianteMotivo);
        }


        // POST: VarianteMotivo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] VarianteMotivo varianteMotivo, string? returnUrl, string? bufferedUrl)
        {
            if (id != varianteMotivo.Id)
            {
                return NotFound();
            }
            // Check if is name unique
            if (context.VarianteMotivo.Any(c => c.Name == varianteMotivo.Name && c.Id != varianteMotivo.Id))
            {
                ModelState.AddModelError("Name", "Nombre existe en otro registro.");
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(varianteMotivo);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(varianteMotivo));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!VarianteMotivoExists(varianteMotivo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Motivo Variante. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Motivo Variante";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, returnUrl, bufferedUrl);

            return View(varianteMotivo);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: VarianteMotivo/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.VarianteMotivo == null)) return NotFound();
            VarianteMotivo? varianteMotivo = await context.VarianteMotivo
                .FirstAsync(r => r.Id == id);
            
            if (varianteMotivo == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            bool isLinked = await findExistingLinks(varianteMotivo);
            if (varianteMotivo != null && !isLinked)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.VarianteMotivo.Remove(varianteMotivo);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(varianteMotivo));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Motivo Variante";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }

            //---- if not saved reload view ----
            // set ViewBags
            setStandardViewBags("Delete", false, returnUrl, bufferedUrl);
            jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
            return Redirect(HttpContext.Request.Headers["Referer"]);
        }



        // Finds all the registers that are using the current registry from Motivo Variante
        async Task<bool> findExistingLinks(VarianteMotivo varianteMotivo)
        {
            bool isLinked = false;
            string externalLinks = string.Empty;

            //search for Variante de Obras using this Motivo Variante
            List<Variante>             variantes = await context.Variante
                .Include(p => p.Contract_info)
                .Include(p => p.Motivo_info)
                .Include(p => p.Stage_info)
                .Where(r => r.Motivo == varianteMotivo.Id).ToListAsync();
            if (variantes?.Count > 0)
            {
                if (!isLinked) isLinked = true;
                externalLinks += "Se usa en  Variante de Obras:<br/>";
                foreach (Variante variante1 in variantes)
                    externalLinks += variante1?.Code + "<br/>";
                externalLinks += "<br/>";
            }

            if (isLinked) externalLinks = "Motivo Variante no puede borrarse<br/>" + externalLinks;
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
        IQueryable<VarianteMotivo> orderBySelectedOrDefault(string sortOrder, IQueryable<VarianteMotivo> varianteMotivoes)
        {
            ViewBag.nameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.nameIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "name_desc":
                    varianteMotivoes = varianteMotivoes.OrderByDescending(t => t.Id);
                    ViewBag.nameIcon = "bi-caret-up-fill";
                    break;
                default:
                    varianteMotivoes = varianteMotivoes.OrderBy(t => t.Id);
                    ViewBag.nameIcon = "bi-caret-down-fill";
                    break;
            }
            return varianteMotivoes;
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
            // navVarianteMotivo
            ViewBag.navVarianteMotivo = $"{action}";
        }


        //----------------------------------------------
        //==============================================
        #endregion


        //========================================================

        private bool VarianteMotivoExists(int id)
        {
            return (context.VarianteMotivo?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
