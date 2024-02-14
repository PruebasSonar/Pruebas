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
    public partial class ProjectController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly ILogger<ProjectController> logger;


        public ProjectController(PIPMUNI_ARGDbContext context
        , ILogger<ProjectController> logger
        )
        {
            this.context = context;
            this.logger = logger;
        }

        JaosLibUtils jaosLibUtils = new JaosLibUtils();

        #region Index


        //----------- Index

        // GET: Project/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, string? returnUrl, string? bufferedUrl)
        {
            var projects = from o in context.Project select o;
            if (!string.IsNullOrEmpty(searchText))
                projects = projects.Where(p => p.Name!.Contains(searchText) || p.Code!.Contains(searchText) || p.Sector_info!.Name!.Contains(searchText) || p.Stage_info!.Name!.Contains(searchText) || p.Office_info!.Name!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (projects != null)
            {
                projects = orderBySelectedOrDefault(sortOrder, projects);
                projects = projects
                    .Include(p => p.Sector_info)
                    .Include(p => p.Stage_info)
                    .Include(p => p.Office_info);
                return View(await projects.ToListAsync());
            }
            return View(new List<Project>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: Project/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get Project
            if ((id == null) || (id <= 0) || (context.Project == null)) return NotFound();
            Project? project = await context.Project
                                    .Include(t => t.ProjectSources!).ThenInclude(t => t.Source_info)
                                    .Include(t => t.ProjectAttachments!)
                .FirstAsync(r => r.Id == id);
            if (project == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, returnUrl, bufferedUrl);
            setViewBagsForLists(project);

            return View(project);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: Project/Create
        [HttpGet]
        public IActionResult Create(string? returnUrl, string? bufferedUrl)
        {
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            setViewBagsForLists(null);

            return View();
        }



        // POST: Project/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Project project, string? returnUrl, string? bufferedUrl)
        {
            unselectedLinksNulled(project);
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(project);
                    await context.SaveChangesAsync();

                    // Assign Code when created
                    project.Code = (project.Id + 1000).ToString("d5");
                    context.Update(project);
                    await context.SaveChangesAsync();

                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(project));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = project.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Proyecto";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
            {
                ViewBag.CostValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Cost");
            }

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            setViewBagsForLists(project);
            return View(project);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: Project/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get Project
            if ((id == null) || (id <= 0) || (context.Project == null)) return NotFound();
            Project? project = await context.Project
                                    .Include(t => t.ProjectSources!).ThenInclude(t => t.Source_info)
                                    .Include(t => t.ProjectAttachments!)
                .FirstAsync(r => r.Id == id);
            if (project == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, returnUrl, bufferedUrl);
            setViewBagsForLists(project);
            ViewBag.projectId = project.Id;

            return View(project);
        }


        // POST: Project/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] Project project, string? returnUrl, string? bufferedUrl)
        {
            if (id != project.Id)
            {
                return NotFound();
            }
            unselectedLinksNulled(project);
            // Check if is name unique
            if (context.Project.Any(c => c.Name == project.Name && c.Id != project.Id))
            {
                ModelState.AddModelError("Name", "Nombre existe en otro registro.");
            }
            // Check if is code unique
            if (context.Project.Any(c => c.Code == project.Code && c.Id != project.Id))
            {
                ModelState.AddModelError("Code", "Código interno existe en otro registro.");
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(project);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(project));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Proyecto. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Proyecto";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
            {
                ViewBag.CostValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Cost");
            }

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, returnUrl, bufferedUrl);
            setViewBagsForLists(project);
            ViewBag.projectId = project.Id;

            return View(project);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: Project/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.Project == null)) return NotFound();
            Project? project = await context.Project
                .Include(t => t.ProjectAttachments!)
                .Include(t => t.ProjectSources!)
                .Include(t => t.ProjectImages!)
                .Include(t => t.ProjectVideos!)
                .FirstAsync(r => r.Id == id);
            
            if (project == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            bool isLinked = await findExistingLinks(project);
            if (project != null && !isLinked)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    deleteChildren(project);
                    await context.SaveChangesAsync();
                    context.Project.Remove(project);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(project));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Proyecto";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }

            //---- if not saved reload view ----
            // set ViewBags
            setStandardViewBags("Delete", false, returnUrl, bufferedUrl);
            jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
            return Redirect(HttpContext.Request.Headers["Referer"]);
        }



        // Finds all the registers that are using the current registry from Proyecto
        async Task<bool> findExistingLinks(Project project)
        {
            bool isLinked = false;
            string externalLinks = string.Empty;

            //search for Obras using this Proyecto
            List<Contract>             contracts = await context.Contract
                .Include(p => p.Project_info)
                .Include(p => p.Stage_info)
                .Include(p => p.Office_info)
                .Include(p => p.Type_info)
                .Include(p => p.Contractor_info)
                .Where(r => r.Project == project.Id).ToListAsync();
            if (contracts?.Count > 0)
            {
                if (!isLinked) isLinked = true;
                externalLinks += "Se usa en  Obras:<br/>";
                foreach (Contract contract1 in contracts)
                    externalLinks += contract1?.Code + " - " + contract1?.Name + "<br/>";
                externalLinks += "<br/>";
            }

            if (isLinked) externalLinks = "Proyecto no puede borrarse<br/>" + externalLinks;
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
        IQueryable<Project> orderBySelectedOrDefault(string sortOrder, IQueryable<Project> projects)
        {
            ViewBag.nameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.codeSort = sortOrder == "code" ? "code_desc" : "code";
            ViewBag.sectorSort = sortOrder == "sector" ? "sector_desc" : "sector";
            ViewBag.stageSort = sortOrder == "stage" ? "stage_desc" : "stage";
            ViewBag.officeSort = sortOrder == "office" ? "office_desc" : "office";
            ViewBag.nameIcon = "bi-caret-down";
            ViewBag.codeIcon = "bi-caret-down";
            ViewBag.sectorIcon = "bi-caret-down";
            ViewBag.stageIcon = "bi-caret-down";
            ViewBag.officeIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "code_desc":
                    projects = projects.OrderByDescending(o => o.Code);
                    ViewBag.codeIcon = "bi-caret-up-fill";
                    break;
                case "code":
                    projects = projects.OrderBy(o => o.Code);
                    ViewBag.codeIcon = "bi-caret-down-fill";
                    break;
                case "sector_desc":
                    projects = projects.OrderByDescending(o => o.Sector);
                    ViewBag.sectorIcon = "bi-caret-up-fill";
                    break;
                case "sector":
                    projects = projects.OrderBy(o => o.Sector);
                    ViewBag.sectorIcon = "bi-caret-down-fill";
                    break;
                case "stage_desc":
                    projects = projects.OrderByDescending(o => o.Stage);
                    ViewBag.stageIcon = "bi-caret-up-fill";
                    break;
                case "stage":
                    projects = projects.OrderBy(o => o.Stage);
                    ViewBag.stageIcon = "bi-caret-down-fill";
                    break;
                case "office_desc":
                    projects = projects.OrderByDescending(o => o.Office);
                    ViewBag.officeIcon = "bi-caret-up-fill";
                    break;
                case "office":
                    projects = projects.OrderBy(o => o.Office);
                    ViewBag.officeIcon = "bi-caret-down-fill";
                    break;
                case "name_desc":
                    projects = projects.OrderByDescending(t => t.Name);
                    ViewBag.nameIcon = "bi-caret-up-fill";
                    break;
                default:
                    projects = projects.OrderBy(t => t.Name);
                    ViewBag.nameIcon = "bi-caret-down-fill";
                    break;
            }
            return projects;
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
            // navProject
            ViewBag.navProject = $"{action}";
        }


        void setViewBagsForLists(Project? project)
        {

            // set options for Sector
            var listSector = new SelectList(context.Sector.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Name, r.Id.ToString())), "Value", "Text", project?.Sector).ToList();
            listSector.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listSector = listSector;

            // set options for Subsector
            var listSubsector = new SelectList(context.Subsector.OrderBy(c => c.Sector).OrderBy(c => c.Name).Select(r => new SelectListItem(r.Name, r.Id.ToString())), "Value", "Text", project?.Subsector).ToList();
            listSubsector.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listSubsector = listSubsector;
            ViewBag.listSubsectorParent = context.Subsector.Select(r => new { parentId = r.Sector.ToString(), id = r.Id.ToString() }).ToList().ToJson();

            // set options for Stage
            var listStage = new SelectList(context.ProjectStage.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Name, r.Id.ToString())), "Value", "Text", project?.Stage).ToList();
            listStage.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listStage = listStage;

            // set options for Office
            var listOffice = new SelectList(context.Office.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Name, r.Id.ToString())), "Value", "Text", project?.Office).ToList();
            listOffice.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listOffice = listOffice;
        }


        void unselectedLinksNulled(Project project)
        {
            // sets unselected lists to null
            if (project.Sector == 0) project.Sector = null;
            if (project.Subsector == 0) project.Subsector = null;
            if (project.Office == 0) project.Office = null;

        }

        //----------------------------------------------
        //==============================================
        // delete children
        async void deleteChildren(Project project)
        {
            if (project.ProjectAttachments?.Count > 0)
                project.ProjectAttachments.ToList().ForEach(c => context.ProjectAttachment.Remove(c));
            if (project.ProjectSources?.Count > 0)
                project.ProjectSources.ToList().ForEach(c => context.ProjectSource.Remove(c));
            if (project.ProjectImages?.Count > 0)
                project.ProjectImages.ToList().ForEach(c => context.ProjectImage.Remove(c));
            if (project.ProjectVideos?.Count > 0)
                project.ProjectVideos.ToList().ForEach(c => context.ProjectVideo.Remove(c));
        }
        #endregion


        //========================================================

        private bool ProjectExists(int id)
        {
            return (context.Project?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
