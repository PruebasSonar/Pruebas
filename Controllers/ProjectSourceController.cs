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
    public partial class ProjectSourceController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly ILogger<ProjectSourceController> logger;


        public ProjectSourceController(PIPMUNI_ARGDbContext context
        , ILogger<ProjectSourceController> logger
        )
        {
            this.context = context;
            this.logger = logger;
        }

        JaosLibUtils jaosLibUtils = new JaosLibUtils();

        #region Index


        //----------- Index

        // GET: ProjectSource/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            var projectSources = projectId.HasValue
            ? from o in context.ProjectSource where (o.Project == projectId) select o
            : from o in context.ProjectSource select o;
            if (!string.IsNullOrEmpty(searchText))
                projectSources = projectSources.Where(p => p.Source_info!.Name!.Contains(searchText) || p.Percentage.ToString()!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, projectId, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (projectSources != null)
            {
                projectSources = orderBySelectedOrDefault(sortOrder, projectSources);
                projectSources = projectSources
                    .Include(p => p.Source_info);
                return View(await projectSources.ToListAsync());
            }
            return View(new List<ProjectSource>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: ProjectSource/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            // get ProjectSource
            if ((id == null) || (id <= 0) || (context.ProjectSource == null)) return NotFound();
            ProjectSource? projectSource = await context.ProjectSource
                .FindAsync(id);
            if (projectSource == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(projectSource);

            return View(projectSource);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: ProjectSource/Create
        [HttpGet]
        public IActionResult Create(int? projectId, string? returnUrl, string? bufferedUrl)
        {
            // set ViewBags
            setStandardViewBags("Create", true, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(null);

            ProjectSource projectSource = new ProjectSource();
            if (projectId.HasValue) projectSource.Project = projectId.Value;
            return View(projectSource);
        }



        // POST: ProjectSource/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ProjectSource projectSource, int? projectId, string? returnUrl, string? bufferedUrl)
        {

            // validate totalPercentage > 100
            var totalPercentage = projectTotalSourcePercentage(projectSource);
                        if (totalPercentage > 100)
                        {
                            ViewBag.errorMessage = string.Format("Total de porcentajes superior al 100%. Total={0}%", totalPercentage.ToString("n2"));
                        } else

            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(projectSource);
                    await context.SaveChangesAsync();
                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(projectSource));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = projectSource.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Fuente Financiamiento";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
            {
                ViewBag.PercentageValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Percentage");
            }

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(projectSource);
            return View(projectSource);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: ProjectSource/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            // get ProjectSource
            if ((id == null) || (id <= 0) || (context.ProjectSource == null)) return NotFound();
            ProjectSource? projectSource = await context.ProjectSource
                .FindAsync(id);
            if (projectSource == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(projectSource);

            return View(projectSource);
        }


        // POST: ProjectSource/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] ProjectSource projectSource, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            if (id != projectSource.Id)
            {
                return NotFound();
            }

            // validate totalPercentage > 100
            var totalPercentage = projectTotalSourcePercentage(projectSource);
                        if (totalPercentage > 100)
                        {
                            ViewBag.errorMessage = string.Format("Total de porcentajes superior al 100%. Total={0}%", totalPercentage.ToString("n2"));
                        } else

            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(projectSource);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(projectSource));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!ProjectSourceExists(projectSource.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Fuente Financiamiento. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Fuente Financiamiento";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
            {
                ViewBag.PercentageValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Percentage");
            }

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(projectSource);

            return View(projectSource);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: ProjectSource/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.ProjectSource == null)) return NotFound();
            ProjectSource? projectSource = await context.ProjectSource
                .FirstAsync(r => r.Id == id);
            
            if (projectSource == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            if (projectSource != null)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.ProjectSource.Remove(projectSource);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(projectSource));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Fuente Financiamiento";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }

            //---- if not saved reload view ----
            // set ViewBags
            setStandardViewBags("Delete", false, projectId, returnUrl, bufferedUrl);
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
        IQueryable<ProjectSource> orderBySelectedOrDefault(string sortOrder, IQueryable<ProjectSource> projectSources)
        {
            ViewBag.percentageSort = String.IsNullOrEmpty(sortOrder) ? "percentage_desc" : "";
            ViewBag.sourceSort = sortOrder == "source" ? "source_desc" : "source";
            ViewBag.sourceIcon = "bi-caret-down";
            ViewBag.percentageIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "source_desc":
                    projectSources = projectSources.OrderByDescending(o => o.Source);
                    ViewBag.sourceIcon = "bi-caret-up-fill";
                    break;
                case "source":
                    projectSources = projectSources.OrderBy(o => o.Source);
                    ViewBag.sourceIcon = "bi-caret-down-fill";
                    break;
                case "percentage_desc":
                    projectSources = projectSources.OrderByDescending(t => t.Percentage);
                    ViewBag.percentageIcon = "bi-caret-up-fill";
                    break;
                default:
                    projectSources = projectSources.OrderBy(t => t.Percentage);
                    ViewBag.percentageIcon = "bi-caret-down-fill";
                    break;
            }
            return projectSources;
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
        void setStandardViewBags(string action, bool returnsbyDefault, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            if (projectId.HasValue) ViewBag.projectId = projectId;
            // returnUrl
            if (!string.IsNullOrEmpty(returnUrl))
                ViewBag.returnUrl = returnUrl;
            else if (returnsbyDefault)
                ViewBag.returnUrl = HttpContext.Request.Headers["Referer"];
            // bufferedUrl
            if (!string.IsNullOrEmpty(bufferedUrl)) ViewBag.bufferedUrl = bufferedUrl;
            // navProjectSource
            ViewBag.navProjectSource = $"{action}";
        }


        void setViewBagsForLists(ProjectSource? projectSource)
        {

            // set options for Project
            var listProject = new SelectList(context.Project.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Name + " - " + r.Code, r.Id.ToString())), "Value", "Text", projectSource?.Project).ToList();
            listProject.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listProject = listProject;

            // set options for Source
            var listSource = new SelectList(context.FundingSource.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Name, r.Id.ToString())), "Value", "Text", projectSource?.Source).ToList();
            listSource.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listSource = listSource;
        }


        //----------------------------------------------
        //==============================================
        #endregion


        //========================================================

        private bool ProjectSourceExists(int id)
        {
            return (context.ProjectSource?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
