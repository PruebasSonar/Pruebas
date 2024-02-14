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
    public partial class ProjectImageController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly IFileLoadService fileLoadService;
        private readonly ILogger<ProjectImageController> logger;


        public ProjectImageController(PIPMUNI_ARGDbContext context
        , IFileLoadService fileLoadService
        , ILogger<ProjectImageController> logger
        )
        {
            this.context = context;
            this.fileLoadService = fileLoadService;
            this.logger = logger;
        }

        JaosLibUtils jaosLibUtils = new JaosLibUtils();

        #region Index


        //----------- Index

        // GET: ProjectImage/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            var projectImages = projectId.HasValue
            ? from o in context.ProjectImage where (o.Project == projectId) select o
            : from o in context.ProjectImage select o;
            if (!string.IsNullOrEmpty(searchText))
                projectImages = projectImages.Where(p => p.Description!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, projectId, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (projectImages != null)
            {
                projectImages = orderBySelectedOrDefault(sortOrder, projectImages);
                return View(await projectImages.ToListAsync());
            }
            return View(new List<ProjectImage>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: ProjectImage/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            // get ProjectImage
            if ((id == null) || (id <= 0) || (context.ProjectImage == null)) return NotFound();
            ProjectImage? projectImage = await context.ProjectImage
                .FindAsync(id);
            if (projectImage == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(projectImage);

            return View(projectImage);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: ProjectImage/Create
        [HttpGet]
        public IActionResult Create(int? projectId, string? returnUrl, string? bufferedUrl)
        {
            // set ViewBags
            setStandardViewBags("Create", true, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(null);

            ProjectImage projectImage = new ProjectImage();
            if (projectId.HasValue) projectImage.Project = projectId.Value;
            return View(projectImage);
        }



        // POST: ProjectImage/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ProjectImage projectImage, int? projectId, string? returnUrl, string? bufferedUrl)
        {

            // set UploadDate
            projectImage.UploadDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(projectImage);
                    await context.SaveChangesAsync();

                    //---- File attachment ----
                    if (projectImage.FileInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("File" ,projectImage.Project, projectImage.Id, projectImage.FileInput);
                        int result = await fileLoadService.UploadFile(serverFileName, projectImage.FileInput, FileLoadService.PathProjectImages);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(projectImage.File, serverFileName);
                        // set
                        projectImage.File = serverFileName;
                        context.Update(projectImage);
                        await context.SaveChangesAsync();
                    }

                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(projectImage));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = projectImage.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Imagen";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(projectImage);
            return View(projectImage);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: ProjectImage/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            // get ProjectImage
            if ((id == null) || (id <= 0) || (context.ProjectImage == null)) return NotFound();
            ProjectImage? projectImage = await context.ProjectImage
                .FindAsync(id);
            if (projectImage == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(projectImage);

            return View(projectImage);
        }


        // POST: ProjectImage/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] ProjectImage projectImage, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            if (id != projectImage.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(projectImage);
                    await context.SaveChangesAsync();

                    //---- File attachment ----
                    if (projectImage.FileInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("File" ,projectImage.Project, projectImage.Id, projectImage.FileInput);
                        int result = await fileLoadService.UploadFile(serverFileName, projectImage.FileInput, FileLoadService.PathProjectImages);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(projectImage.File, serverFileName);
                        // set
                        projectImage.File = serverFileName;
                        context.Update(projectImage);
                        await context.SaveChangesAsync();
                    }

                    logger.LogWarning(JsonConvert.SerializeObject(projectImage));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!ProjectImageExists(projectImage.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Imagen. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Imagen";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, projectId, returnUrl, bufferedUrl);
            setViewBagsForLists(projectImage);

            return View(projectImage);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: ProjectImage/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, int? projectId, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.ProjectImage == null)) return NotFound();
            ProjectImage? projectImage = await context.ProjectImage
                .FirstAsync(r => r.Id == id);
            
            if (projectImage == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            if (projectImage != null)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.ProjectImage.Remove(projectImage);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(projectImage));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Imagen";
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

            string path = fileLoadService.serverFullPath(FileLoadService.PathProjectImages);
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
        IQueryable<ProjectImage> orderBySelectedOrDefault(string sortOrder, IQueryable<ProjectImage> projectImages)
        {
            ViewBag.descriptionSort = String.IsNullOrEmpty(sortOrder) ? "description_desc" : "";
            ViewBag.descriptionIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "description_desc":
                    projectImages = projectImages.OrderByDescending(t => t.ImageDate);
                    ViewBag.descriptionIcon = "bi-caret-up-fill";
                    break;
                default:
                    projectImages = projectImages.OrderBy(t => t.ImageDate);
                    ViewBag.descriptionIcon = "bi-caret-down-fill";
                    break;
            }
            return projectImages;
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
            // navProjectImage
            ViewBag.navProjectImage = $"{action}";
        }


        void setViewBagsForLists(ProjectImage? projectImage)
        {

            // set options for Project
            var listProject = new SelectList(context.Project.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Name + " - " + r.Code, r.Id.ToString())), "Value", "Text", projectImage?.Project).ToList();
            listProject.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listProject = listProject;
        }


        //----------------------------------------------
        //==============================================
        #endregion


        //========================================================

        private bool ProjectImageExists(int id)
        {
            return (context.ProjectImage?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
