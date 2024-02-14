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
    public partial class AdditionStageController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly ILogger<AdditionStageController> logger;


        public AdditionStageController(PIPMUNI_ARGDbContext context
        , ILogger<AdditionStageController> logger
        )
        {
            this.context = context;
            this.logger = logger;
        }

        JaosLibUtils jaosLibUtils = new JaosLibUtils();

        #region Index


        //----------- Index

        // GET: AdditionStage/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, string? returnUrl, string? bufferedUrl)
        {
            var additionStages = from o in context.AdditionStage select o;
            if (!string.IsNullOrEmpty(searchText))
                additionStages = additionStages.Where(p => p.Name!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (additionStages != null)
            {
                additionStages = orderBySelectedOrDefault(sortOrder, additionStages);
                return View(await additionStages.ToListAsync());
            }
            return View(new List<AdditionStage>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: AdditionStage/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get AdditionStage
            if ((id == null) || (id <= 0) || (context.AdditionStage == null)) return NotFound();
            AdditionStage? additionStage = await context.AdditionStage
                .FindAsync(id);
            if (additionStage == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, returnUrl, bufferedUrl);

            return View(additionStage);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: AdditionStage/Create
        [HttpGet]
        public IActionResult Create(string? returnUrl, string? bufferedUrl)
        {
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);

            return View();
        }



        // POST: AdditionStage/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] AdditionStage additionStage, string? returnUrl, string? bufferedUrl)
        {
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(additionStage);
                    await context.SaveChangesAsync();
                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(additionStage));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = additionStage.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Etapa Modificación";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
            {
                ViewBag.OrderValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Order");
            }

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            return View(additionStage);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: AdditionStage/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get AdditionStage
            if ((id == null) || (id <= 0) || (context.AdditionStage == null)) return NotFound();
            AdditionStage? additionStage = await context.AdditionStage
                .FindAsync(id);
            if (additionStage == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, returnUrl, bufferedUrl);

            return View(additionStage);
        }


        // POST: AdditionStage/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] AdditionStage additionStage, string? returnUrl, string? bufferedUrl)
        {
            if (id != additionStage.Id)
            {
                return NotFound();
            }
            // Check if is name unique
            if (context.AdditionStage.Any(c => c.Name == additionStage.Name && c.Id != additionStage.Id))
            {
                ModelState.AddModelError("Name", "Nombre existe en otro registro.");
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(additionStage);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(additionStage));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!AdditionStageExists(additionStage.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Etapa Modificación. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Etapa Modificación";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
            {
                ViewBag.OrderValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Order");
            }

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, returnUrl, bufferedUrl);

            return View(additionStage);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: AdditionStage/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.AdditionStage == null)) return NotFound();
            AdditionStage? additionStage = await context.AdditionStage
                .FirstAsync(r => r.Id == id);
            
            if (additionStage == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            bool isLinked = await findExistingLinks(additionStage);
            if (additionStage != null && !isLinked)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.AdditionStage.Remove(additionStage);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(additionStage));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Etapa Modificación";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }

            //---- if not saved reload view ----
            // set ViewBags
            setStandardViewBags("Delete", false, returnUrl, bufferedUrl);
            jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
            return Redirect(HttpContext.Request.Headers["Referer"]);
        }



        // Finds all the registers that are using the current registry from Etapa Modificación
        async Task<bool> findExistingLinks(AdditionStage additionStage)
        {
            bool isLinked = false;
            string externalLinks = string.Empty;

            //search for Ampliación de Plazos using this Etapa Modificación
            List<Extension>             extensions = await context.Extension
                .Include(p => p.Contract_info)
                .Include(p => p.Stage_info)
                .Where(r => r.Stage == additionStage.Id).ToListAsync();
            if (extensions?.Count > 0)
            {
                if (!isLinked) isLinked = true;
                externalLinks += "Se usa en  Ampliación de Plazos:<br/>";
                foreach (Extension extension1 in extensions)
                    externalLinks += extension1?.Code + " - " + ((extension1?.DateDelivery.HasValue == true) ? extension1?.DateDelivery.Value.ToString("yyyy-MMM-dd") : "") + "<br/>";
                externalLinks += "<br/>";
            }

            //search for Redeterminaciones using this Etapa Modificación
            List<Addition>             additions = await context.Addition
                .Include(p => p.Contract_info)
                .Include(p => p.Stage_info)
                .Where(r => r.Stage == additionStage.Id).ToListAsync();
            if (additions?.Count > 0)
            {
                if (!isLinked) isLinked = true;
                externalLinks += "Se usa en  Redeterminaciones:<br/>";
                foreach (Addition addition1 in additions)
                    externalLinks += addition1?.Code + " - " + ((addition1?.DateDelivery.HasValue == true) ? addition1?.DateDelivery.Value.ToString("yyyy-MMM-dd") : "") + "<br/>";
                externalLinks += "<br/>";
            }

            //search for Variante de Obras using this Etapa Modificación
            List<Variante>             variantes = await context.Variante
                .Include(p => p.Contract_info)
                .Include(p => p.Motivo_info)
                .Include(p => p.Stage_info)
                .Where(r => r.Stage == additionStage.Id).ToListAsync();
            if (variantes?.Count > 0)
            {
                if (!isLinked) isLinked = true;
                externalLinks += "Se usa en  Variante de Obras:<br/>";
                foreach (Variante variante1 in variantes)
                    externalLinks += variante1?.Code + "<br/>";
                externalLinks += "<br/>";
            }

            if (isLinked) externalLinks = "Etapa Modificación no puede borrarse<br/>" + externalLinks;
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
        IQueryable<AdditionStage> orderBySelectedOrDefault(string sortOrder, IQueryable<AdditionStage> additionStages)
        {
            ViewBag.nameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.nameIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "name_desc":
                    additionStages = additionStages.OrderByDescending(t => t.Order);
                    ViewBag.nameIcon = "bi-caret-up-fill";
                    break;
                default:
                    additionStages = additionStages.OrderBy(t => t.Order);
                    ViewBag.nameIcon = "bi-caret-down-fill";
                    break;
            }
            return additionStages;
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
            // navAdditionStage
            ViewBag.navAdditionStage = $"{action}";
        }


        //----------------------------------------------
        //==============================================
        #endregion


        //========================================================

        private bool AdditionStageExists(int id)
        {
            return (context.AdditionStage?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
