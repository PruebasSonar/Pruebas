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
using PIPMUNI_ARG.Services.basic;
using JaosLib.Services.Utilities;

namespace PIPMUNI_ARG.Controllers
{
    [Authorize(Roles =ProjectGlobals.registeredRoles)]
    public partial class ExtensionAttachmentController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly IParentContractService parentContractService;
        private readonly IFileLoadService fileLoadService;
        private readonly ILogger<ExtensionAttachmentController> logger;


        public ExtensionAttachmentController(PIPMUNI_ARGDbContext context
        , IParentContractService parentContractService
        , IFileLoadService fileLoadService
        , ILogger<ExtensionAttachmentController> logger
        )
        {
            this.context = context;
            this.parentContractService = parentContractService;
            this.fileLoadService = fileLoadService;
            this.logger = logger;
        }

        JaosLibUtils jaosLibUtils = new JaosLibUtils();

        #region Index


        //----------- Index

        // GET: ExtensionAttachment/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, int? extensionId, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            var extensionAttachments = extensionId.HasValue
            ? from o in context.ExtensionAttachment where (o.Extension == extensionId) select o
            : from o in context.ExtensionAttachment select o;
            if (!string.IsNullOrEmpty(searchText))
                extensionAttachments = extensionAttachments.Where(p => p.Title!.Contains(searchText) || p.DateAttached.ToString()!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, extensionId, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (extensionAttachments != null)
            {
                extensionAttachments = orderBySelectedOrDefault(sortOrder, extensionAttachments);
                return View(await extensionAttachments.ToListAsync());
            }
            return View(new List<ExtensionAttachment>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: ExtensionAttachment/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, int? extensionId, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            // get ExtensionAttachment
            if ((id == null) || (id <= 0) || (context.ExtensionAttachment == null)) return NotFound();
            ExtensionAttachment? extensionAttachment = await context.ExtensionAttachment
                .FindAsync(id);
            if (extensionAttachment == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, extensionId, returnUrl, bufferedUrl);
            setViewBagsForLists(extensionAttachment);

            return View(extensionAttachment);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: ExtensionAttachment/Create
        [HttpGet]
        public IActionResult Create(int? extensionId, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            // set ViewBags
            setStandardViewBags("Create", true, extensionId, returnUrl, bufferedUrl);
            setViewBagsForLists(null);

            ExtensionAttachment extensionAttachment = new ExtensionAttachment();
            if (extensionId.HasValue) extensionAttachment.Extension = extensionId.Value;
            return View(extensionAttachment);
        }



        // POST: ExtensionAttachment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ExtensionAttachment extensionAttachment, int? extensionId, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");

            // set UploadDate
            extensionAttachment.DateAttached = DateTime.Now;

            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(extensionAttachment);
                    await context.SaveChangesAsync();

                    //---- FileName attachment ----
                    if (extensionAttachment.FileNameInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("FileName" ,extensionAttachment.Extension, extensionAttachment.Id, extensionAttachment.FileNameInput);
                        int result = await fileLoadService.UploadFile(serverFileName, extensionAttachment.FileNameInput, FileLoadService.PathExtensionAttachments);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(extensionAttachment.FileName, serverFileName);
                        // set
                        extensionAttachment.FileName = serverFileName;
                        context.Update(extensionAttachment);
                        await context.SaveChangesAsync();
                    }

                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(extensionAttachment));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = extensionAttachment.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Anexo Ampliación";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, extensionId, returnUrl, bufferedUrl);
            setViewBagsForLists(extensionAttachment);
            return View(extensionAttachment);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: ExtensionAttachment/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, int? extensionId, string? returnUrl, string? bufferedUrl)
        {
            // get ExtensionAttachment
            if ((id == null) || (id <= 0) || (context.ExtensionAttachment == null)) return NotFound();
            ExtensionAttachment? extensionAttachment = await context.ExtensionAttachment
                .FindAsync(id);
            if (extensionAttachment == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, extensionId, returnUrl, bufferedUrl);
            setViewBagsForLists(extensionAttachment);

            return View(extensionAttachment);
        }


        // POST: ExtensionAttachment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] ExtensionAttachment extensionAttachment, int? extensionId, string? returnUrl, string? bufferedUrl)
        {
            if (id != extensionAttachment.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(extensionAttachment);
                    await context.SaveChangesAsync();

                    //---- FileName attachment ----
                    if (extensionAttachment.FileNameInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("FileName" ,extensionAttachment.Extension, extensionAttachment.Id, extensionAttachment.FileNameInput);
                        int result = await fileLoadService.UploadFile(serverFileName, extensionAttachment.FileNameInput, FileLoadService.PathExtensionAttachments);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(extensionAttachment.FileName, serverFileName);
                        // set
                        extensionAttachment.FileName = serverFileName;
                        context.Update(extensionAttachment);
                        await context.SaveChangesAsync();
                    }

                    logger.LogWarning(JsonConvert.SerializeObject(extensionAttachment));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!ExtensionAttachmentExists(extensionAttachment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Anexo Ampliación. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Anexo Ampliación";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, extensionId, returnUrl, bufferedUrl);
            setViewBagsForLists(extensionAttachment);

            return View(extensionAttachment);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: ExtensionAttachment/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, int? extensionId, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.ExtensionAttachment == null)) return NotFound();
            ExtensionAttachment? extensionAttachment = await context.ExtensionAttachment
                .FirstAsync(r => r.Id == id);
            
            if (extensionAttachment == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            if (extensionAttachment != null)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.ExtensionAttachment.Remove(extensionAttachment);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(extensionAttachment));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Anexo Ampliación";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }

            //---- if not saved reload view ----
            // set ViewBags
            setStandardViewBags("Delete", false, extensionId, returnUrl, bufferedUrl);
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

            string path = fileLoadService.serverFullPath(FileLoadService.PathExtensionAttachments);
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
        IQueryable<ExtensionAttachment> orderBySelectedOrDefault(string sortOrder, IQueryable<ExtensionAttachment> extensionAttachments)
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
                    extensionAttachments = extensionAttachments.OrderByDescending(o => o.Title);
                    ViewBag.titleIcon = "bi-caret-up-fill";
                    break;
                case "title":
                    extensionAttachments = extensionAttachments.OrderBy(o => o.Title);
                    ViewBag.titleIcon = "bi-caret-down-fill";
                    break;
                case "fileName_desc":
                    extensionAttachments = extensionAttachments.OrderByDescending(o => o.FileName);
                    ViewBag.fileNameIcon = "bi-caret-up-fill";
                    break;
                case "fileName":
                    extensionAttachments = extensionAttachments.OrderBy(o => o.FileName);
                    ViewBag.fileNameIcon = "bi-caret-down-fill";
                    break;
                case "dateAttached_desc":
                    extensionAttachments = extensionAttachments.OrderByDescending(t => t.DateAttached);
                    ViewBag.dateAttachedIcon = "bi-caret-up-fill";
                    break;
                default:
                    extensionAttachments = extensionAttachments.OrderBy(t => t.DateAttached);
                    ViewBag.dateAttachedIcon = "bi-caret-down-fill";
                    break;
            }
            return extensionAttachments;
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
        void setStandardViewBags(string action, bool returnsbyDefault, int? extensionId, string? returnUrl, string? bufferedUrl)
        {
            if (extensionId.HasValue) ViewBag.extensionId = extensionId;
            // returnUrl
            if (!string.IsNullOrEmpty(returnUrl))
                ViewBag.returnUrl = returnUrl;
            else if (returnsbyDefault)
                ViewBag.returnUrl = HttpContext.Request.Headers["Referer"];
            // bufferedUrl
            if (!string.IsNullOrEmpty(bufferedUrl)) ViewBag.bufferedUrl = bufferedUrl;
            // navExtensionAttachment
            ViewBag.navExtensionAttachment = $"{action}";
        }


        void setViewBagsForLists(ExtensionAttachment? extensionAttachment)
        {

            // set options for Extension
            var listExtension = new SelectList(context.Extension.OrderBy(c => c.DateDelivery).Select(r => new SelectListItem(r.Code + " - " + ((r.DateDelivery.HasValue == true) ? r.DateDelivery.Value.ToString("yyyy-MMM-dd") : ""), r.Id.ToString())), "Value", "Text", extensionAttachment?.Extension).ToList();
            listExtension.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listExtension = listExtension;
        }


        //----------------------------------------------
        //==============================================
        #endregion


        //========================================================

        private bool ExtensionAttachmentExists(int id)
        {
            return (context.ExtensionAttachment?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
