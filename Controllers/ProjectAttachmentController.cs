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
using JaosLib.Services.Utilities;

namespace PIPMUNI_ARG.Controllers
{
    [Authorize(Roles =ProjectGlobals.registeredRoles)]
    public partial class ProjectAttachmentController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly IFileLoadService fileLoadService;
        private readonly ILogger<ProjectAttachmentController> logger;


        public ProjectAttachmentController(PIPMUNI_ARGDbContext context
        , IFileLoadService fileLoadService
        , ILogger<ProjectAttachmentController> logger
        )
        {
            this.context = context;
            this.fileLoadService = fileLoadService;
            this.logger = logger;
        }

        JaosLibUtils jaosLibUtils = new JaosLibUtils();

        #region Index


        //----------- Index

        // GET: ProjectAttachment/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            var projectAttachments = projectId.HasValue
            ? from o in context.ProjectAttachment where (o.Project == projectId) select o
            : from o in context.ProjectAttachment select o;
            if (!string.IsNullOrEmpty(searchText))
                projectAttachments = projectAttachments.Where(p => p.Title!.Contains(searchText) || p.DateAttached.ToString()!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, projectId, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (projectAttachments != null)
            {
                projectAttachments = orderBySelectedOrDefault(sortOrder, projectAttachments);
                return View(await projectAttachments.ToListAsync());
            }
            return View(new List<ProjectAttachment>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: ProjectAttachment/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            // get ProjectAttachment
            if ((id == null) || (id <= 0) || (context.ProjectAttachment == null)) return NotFound();
            ProjectAttachment? projectAttachment = await context.ProjectAttachment
                .FindAsync(id);
            if (projectAttachment == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(projectAttachment);

            return View(projectAttachment);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: ProjectAttachment/Create
        [HttpGet]
        public IActionResult Create(int? projectId, string? returnUrl, string? bufferedUrl)
        {
            // set ViewBags
            setStandardViewBags("Create", true, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(null);

            ProjectAttachment projectAttachment = new ProjectAttachment();
            if (projectId.HasValue) projectAttachment.Project = projectId.Value;
            return View(projectAttachment);
        }



        // POST: ProjectAttachment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ProjectAttachment projectAttachment, int? projectId, string? returnUrl, string? bufferedUrl)
        {

            // set UploadDate
            projectAttachment.DateAttached = DateTime.Now;

            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(projectAttachment);
                    await context.SaveChangesAsync();

                    //---- FileName attachment ----
                    if (projectAttachment.FileNameInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("FileName" ,projectAttachment.Project, projectAttachment.Id, projectAttachment.FileNameInput);
                        int result = await fileLoadService.UploadFile(serverFileName, projectAttachment.FileNameInput, FileLoadService.PathProjectAttachments);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(projectAttachment.FileName, serverFileName);
                        // set
                        projectAttachment.FileName = serverFileName;
                        context.Update(projectAttachment);
                        await context.SaveChangesAsync();
                    }

                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(projectAttachment));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = projectAttachment.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Anexo Proyecto";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(projectAttachment);
            return View(projectAttachment);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: ProjectAttachment/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            // get ProjectAttachment
            if ((id == null) || (id <= 0) || (context.ProjectAttachment == null)) return NotFound();
            ProjectAttachment? projectAttachment = await context.ProjectAttachment
                .FindAsync(id);
            if (projectAttachment == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(projectAttachment);

            return View(projectAttachment);
        }


        // POST: ProjectAttachment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] ProjectAttachment projectAttachment, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            if (id != projectAttachment.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(projectAttachment);
                    await context.SaveChangesAsync();

                    //---- FileName attachment ----
                    if (projectAttachment.FileNameInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("FileName" ,projectAttachment.Project, projectAttachment.Id, projectAttachment.FileNameInput);
                        int result = await fileLoadService.UploadFile(serverFileName, projectAttachment.FileNameInput, FileLoadService.PathProjectAttachments);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(projectAttachment.FileName, serverFileName);
                        // set
                        projectAttachment.FileName = serverFileName;
                        context.Update(projectAttachment);
                        await context.SaveChangesAsync();
                    }

                    logger.LogWarning(JsonConvert.SerializeObject(projectAttachment));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!ProjectAttachmentExists(projectAttachment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Anexo Proyecto. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Anexo Proyecto";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(projectAttachment);

            return View(projectAttachment);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: ProjectAttachment/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.ProjectAttachment == null)) return NotFound();
            ProjectAttachment? projectAttachment = await context.ProjectAttachment
                .FirstAsync(r => r.Id == id);
            
            if (projectAttachment == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            if (projectAttachment != null)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.ProjectAttachment.Remove(projectAttachment);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(projectAttachment));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Anexo Proyecto";
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

        public IActionResult Download(string serverFileName,string downloadName)
        {
            if (string.IsNullOrEmpty(serverFileName)) return NoContent();

            string path = fileLoadService.serverFullPath(FileLoadService.PathProjectAttachments);
            string pathWithFile = Path.Combine(path, serverFileName);

            // Return the file as a FileResult
            return File(System.IO.File.OpenRead(pathWithFile), "application/octet-stream", downloadName + Path.GetExtension(serverFileName));
        }

        //----------------------------------------------
        //==============================================
        //----------------------------------------------
        #endregion
        #region Supporting Methods

         //-- Sort Table by default or user selected order
        IQueryable<ProjectAttachment> orderBySelectedOrDefault(string sortOrder, IQueryable<ProjectAttachment> projectAttachments)
        {
            ViewBag.dateAttachedSort = String.IsNullOrEmpty(sortOrder) ? "dateAttached_desc" : "";
            ViewBag.titleSort = sortOrder == "title" ? "title_desc" : "title";
            ViewBag.fileNameSort = sortOrder == "fileName" ? "fileName_desc" : "fileName";
            ViewBag.titleIcon = "bi-caret-down";
            ViewBag.fileNameIcon = "bi-caret-down";
            ViewBag.dateAttachedIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "title_desc":
                    projectAttachments = projectAttachments.OrderByDescending(o => o.Title);
                    ViewBag.titleIcon = "bi-caret-up-fill";
                    break;
                case "title":
                    projectAttachments = projectAttachments.OrderBy(o => o.Title);
                    ViewBag.titleIcon = "bi-caret-down-fill";
                    break;
                case "fileName_desc":
                    projectAttachments = projectAttachments.OrderByDescending(o => o.FileName);
                    ViewBag.fileNameIcon = "bi-caret-up-fill";
                    break;
                case "fileName":
                    projectAttachments = projectAttachments.OrderBy(o => o.FileName);
                    ViewBag.fileNameIcon = "bi-caret-down-fill";
                    break;
                case "dateAttached_desc":
                    projectAttachments = projectAttachments.OrderByDescending(t => t.DateAttached);
                    ViewBag.dateAttachedIcon = "bi-caret-up-fill";
                    break;
                default:
                    projectAttachments = projectAttachments.OrderBy(t => t.DateAttached);
                    ViewBag.dateAttachedIcon = "bi-caret-down-fill";
                    break;
            }
            return projectAttachments;
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
            // navProjectAttachment
            ViewBag.navProjectAttachment = $"{action}";
        }


        void setViewBagsForLists(ProjectAttachment? projectAttachment)
        {

            // set options for Project
            var listProject = new SelectList(context.Project.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Name + " - " + r.Code, r.Id.ToString())), "Value", "Text", projectAttachment?.Project).ToList();
            listProject.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listProject = listProject;
        }


        //----------------------------------------------
        //==============================================
        #endregion


        //========================================================

        private bool ProjectAttachmentExists(int id)
        {
            return (context.ProjectAttachment?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
