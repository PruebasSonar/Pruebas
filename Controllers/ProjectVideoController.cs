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
    public partial class ProjectVideoController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly ILogger<ProjectVideoController> logger;


        public ProjectVideoController(PIPMUNI_ARGDbContext context
        , ILogger<ProjectVideoController> logger
        )
        {
            this.context = context;
            this.logger = logger;
        }

        JaosLibUtils jaosLibUtils = new JaosLibUtils();

        #region Index


        //----------- Index

        // GET: ProjectVideo/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            var projectVideos = projectId.HasValue
            ? from o in context.ProjectVideo where (o.Project == projectId) select o
            : from o in context.ProjectVideo select o;

            // set ViewBags
            setStandardViewBags("Index", false, projectId, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (projectVideos != null)
            {
                projectVideos = orderBySelectedOrDefault(sortOrder, projectVideos);
                return View(await projectVideos.ToListAsync());
            }
            return View(new List<ProjectVideo>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: ProjectVideo/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            // get ProjectVideo
            if ((id == null) || (id <= 0) || (context.ProjectVideo == null)) return NotFound();
            ProjectVideo? projectVideo = await context.ProjectVideo
                .FindAsync(id);
            if (projectVideo == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(projectVideo);

            return View(projectVideo);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: ProjectVideo/Create
        [HttpGet]
        public IActionResult Create(int? projectId, string? returnUrl, string? bufferedUrl)
        {
            // set ViewBags
            setStandardViewBags("Create", true, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(null);

            ProjectVideo projectVideo = new ProjectVideo();
            if (projectId.HasValue) projectVideo.Project = projectId.Value;
            return View(projectVideo);
        }



        // POST: ProjectVideo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ProjectVideo projectVideo, int? projectId, string? returnUrl, string? bufferedUrl)
        {

            // Ajustar link de YouTube y asignar fecha de cargue
            projectVideo.UploadDate = DateTime.Now;
            if (!string.IsNullOrEmpty(projectVideo.Link))
                projectVideo.Link = projectVideo.Link.Replace("/watch?v=", "/embed/");

            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(projectVideo);
                    await context.SaveChangesAsync();
                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(projectVideo));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = projectVideo.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Video";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(projectVideo);
            return View(projectVideo);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: ProjectVideo/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            // get ProjectVideo
            if ((id == null) || (id <= 0) || (context.ProjectVideo == null)) return NotFound();
            ProjectVideo? projectVideo = await context.ProjectVideo
                .FindAsync(id);
            if (projectVideo == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(projectVideo);

            return View(projectVideo);
        }


        // POST: ProjectVideo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] ProjectVideo projectVideo, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            if (id != projectVideo.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(projectVideo);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(projectVideo));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!ProjectVideoExists(projectVideo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Video. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Video";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(projectVideo);

            return View(projectVideo);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: ProjectVideo/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.ProjectVideo == null)) return NotFound();
            ProjectVideo? projectVideo = await context.ProjectVideo
                .FirstAsync(r => r.Id == id);
            
            if (projectVideo == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            if (projectVideo != null)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.ProjectVideo.Remove(projectVideo);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(projectVideo));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Video";
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
        IQueryable<ProjectVideo> orderBySelectedOrDefault(string sortOrder, IQueryable<ProjectVideo> projectVideos)
        {
            return projectVideos;
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
            // navProjectVideo
            ViewBag.navProjectVideo = $"{action}";
        }


        void setViewBagsForLists(ProjectVideo? projectVideo)
        {

            // set options for Project
            var listProject = new SelectList(context.Project.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Name + " - " + r.Code, r.Id.ToString())), "Value", "Text", projectVideo?.Project).ToList();
            listProject.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listProject = listProject;
        }


        //----------------------------------------------
        //==============================================
        #endregion


        //========================================================

        private bool ProjectVideoExists(int id)
        {
            return (context.ProjectVideo?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
