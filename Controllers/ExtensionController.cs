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
    public partial class ExtensionController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly IParentContractService parentContractService;
        private readonly IFileLoadService fileLoadService;
        private readonly ILogger<ExtensionController> logger;


        public ExtensionController(PIPMUNI_ARGDbContext context
        , IParentContractService parentContractService
        , IFileLoadService fileLoadService
        , ILogger<ExtensionController> logger
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

        // GET: Extension/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            var extensions = from o in context.Extension where (o.Contract == contract.Id) select o;
            if (!string.IsNullOrEmpty(searchText))
                extensions = extensions.Where(p => p.Code!.Contains(searchText) || p.Days.ToString()!.Contains(searchText) || p.DateDelivery.ToString()!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (extensions != null)
            {
                extensions = orderBySelectedOrDefault(sortOrder, extensions);
                return View(await extensions.ToListAsync());
            }
            return View(new List<Extension>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: Extension/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            // get Extension
            if ((id == null) || (id <= 0) || (context.Extension == null)) return NotFound();
            Extension? extension = await context.Extension
                                    .Include(t => t.ExtensionAttachments!)
                .FirstAsync(r => r.Id == id);
            if (extension == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, returnUrl, bufferedUrl);
            setViewBagsForLists(extension);

            return View(extension);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: Extension/Create
        [HttpGet]
        public IActionResult Create(string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            setViewBagsForLists(null);

            Extension extension = new Extension();
            extension.Contract = contract.Id;
            return View(extension);
        }



        // POST: Extension/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Extension extension, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(extension);
                    await context.SaveChangesAsync();

                    //---- Attachment attachment ----
                    if (extension.AttachmentInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("Attachment" ,extension.Contract, extension.Id, extension.AttachmentInput);
                        int result = await fileLoadService.UploadFile(serverFileName, extension.AttachmentInput, FileLoadService.PathExtensions);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(extension.Attachment, serverFileName);
                        // set
                        extension.Attachment = serverFileName;
                        context.Update(extension);
                        await context.SaveChangesAsync();
                    }


                    // Assign Code when created
                    extension.Code = (extension.Id + 1000).ToString("d5");
                    context.Update(extension);
                    await context.SaveChangesAsync();

                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(extension));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = extension.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Ampliación de Plazo";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
            {
                ViewBag.DaysValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Days");
            }

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            setViewBagsForLists(extension);
            return View(extension);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: Extension/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get Extension
            if ((id == null) || (id <= 0) || (context.Extension == null)) return NotFound();
            Extension? extension = await context.Extension
                                    .Include(t => t.ExtensionAttachments!)
                .FirstAsync(r => r.Id == id);
            if (extension == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, returnUrl, bufferedUrl);
            setViewBagsForLists(extension);
            ViewBag.extensionId = extension.Id;

            return View(extension);
        }


        // POST: Extension/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] Extension extension, string? returnUrl, string? bufferedUrl)
        {
            if (id != extension.Id)
            {
                return NotFound();
            }
            // Check if is code unique
            if (context.Extension.Any(c => c.Code == extension.Code && c.Id != extension.Id))
            {
                ModelState.AddModelError("Code", "Código interno existe en otro registro.");
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(extension);
                    await context.SaveChangesAsync();

                    //---- Attachment attachment ----
                    if (extension.AttachmentInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("Attachment" ,extension.Contract, extension.Id, extension.AttachmentInput);
                        int result = await fileLoadService.UploadFile(serverFileName, extension.AttachmentInput, FileLoadService.PathExtensions);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(extension.Attachment, serverFileName);
                        // set
                        extension.Attachment = serverFileName;
                        context.Update(extension);
                        await context.SaveChangesAsync();
                    }

                    logger.LogWarning(JsonConvert.SerializeObject(extension));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!ExtensionExists(extension.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Ampliación de Plazo. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Ampliación de Plazo";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
            {
                ViewBag.DaysValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Days");
            }

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, returnUrl, bufferedUrl);
            setViewBagsForLists(extension);
            ViewBag.extensionId = extension.Id;

            return View(extension);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: Extension/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.Extension == null)) return NotFound();
            Extension? extension = await context.Extension
                .Include(t => t.ExtensionAttachments!)
                .FirstAsync(r => r.Id == id);
            
            if (extension == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            if (extension != null)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    deleteChildren(extension);
                    await context.SaveChangesAsync();
                    context.Extension.Remove(extension);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(extension));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Ampliación de Plazo";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }

            //---- if not saved reload view ----
            // set ViewBags
            setStandardViewBags("Delete", false, returnUrl, bufferedUrl);
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

            string path = fileLoadService.serverFullPath(FileLoadService.PathExtensions);
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
        IQueryable<Extension> orderBySelectedOrDefault(string sortOrder, IQueryable<Extension> extensions)
        {
            ViewBag.dateDeliverySort = String.IsNullOrEmpty(sortOrder) ? "dateDelivery_desc" : "";
            ViewBag.codeSort = sortOrder == "code" ? "code_desc" : "code";
            ViewBag.daysSort = sortOrder == "days" ? "days_desc" : "days";
            ViewBag.codeIcon = "bi-caret-down";
            ViewBag.daysIcon = "bi-caret-down";
            ViewBag.dateDeliveryIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "code_desc":
                    extensions = extensions.OrderByDescending(o => o.Code);
                    ViewBag.codeIcon = "bi-caret-up-fill";
                    break;
                case "code":
                    extensions = extensions.OrderBy(o => o.Code);
                    ViewBag.codeIcon = "bi-caret-down-fill";
                    break;
                case "days_desc":
                    extensions = extensions.OrderByDescending(o => o.Days);
                    ViewBag.daysIcon = "bi-caret-up-fill";
                    break;
                case "days":
                    extensions = extensions.OrderBy(o => o.Days);
                    ViewBag.daysIcon = "bi-caret-down-fill";
                    break;
                case "dateDelivery_desc":
                    extensions = extensions.OrderByDescending(t => t.DateDelivery);
                    ViewBag.dateDeliveryIcon = "bi-caret-up-fill";
                    break;
                default:
                    extensions = extensions.OrderBy(t => t.DateDelivery);
                    ViewBag.dateDeliveryIcon = "bi-caret-down-fill";
                    break;
            }
            return extensions;
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
            // navExtension
            ViewBag.navExtension = $"{action}";
        }


        void setViewBagsForLists(Extension? extension)
        {

            // set options for Contract
            var listContract = new SelectList(context.Contract.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Code + " - " + r.Name, r.Id.ToString())), "Value", "Text", extension?.Contract).ToList();
            listContract.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listContract = listContract;

            // set options for Stage
            var listStage = new SelectList(context.AdditionStage.OrderBy(c => c.Order).Select(r => new SelectListItem(r.Name, r.Id.ToString())), "Value", "Text", extension?.Stage).ToList();
            listStage.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listStage = listStage;
        }


        //----------------------------------------------
        //==============================================
        // delete children
        async void deleteChildren(Extension extension)
        {
            if (extension.ExtensionAttachments?.Count > 0)
                extension.ExtensionAttachments.ToList().ForEach(c => context.ExtensionAttachment.Remove(c));
        }
        #endregion


        //========================================================

        private bool ExtensionExists(int id)
        {
            return (context.Extension?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
