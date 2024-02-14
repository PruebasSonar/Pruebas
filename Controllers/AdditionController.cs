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
    public partial class AdditionController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly IParentContractService parentContractService;
        private readonly IFileLoadService fileLoadService;
        private readonly ILogger<AdditionController> logger;


        public AdditionController(PIPMUNI_ARGDbContext context
        , IParentContractService parentContractService
        , IFileLoadService fileLoadService
        , ILogger<AdditionController> logger
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

        // GET: Addition/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            var additions = from o in context.Addition where (o.Contract == contract.Id) select o;
            if (!string.IsNullOrEmpty(searchText))
                additions = additions.Where(p => p.Code!.Contains(searchText) || p.Value.ToString()!.Contains(searchText) || p.DateDelivery.ToString()!.Contains(searchText) || p.Stage_info!.Name!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (additions != null)
            {
                additions = orderBySelectedOrDefault(sortOrder, additions);
                additions = additions
                    .Include(p => p.Stage_info);
                return View(await additions.ToListAsync());
            }
            return View(new List<Addition>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: Addition/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            // get Addition
            if ((id == null) || (id <= 0) || (context.Addition == null)) return NotFound();
            Addition? addition = await context.Addition
                                    .Include(t => t.AdditionAttachments!)
                .FirstAsync(r => r.Id == id);
            if (addition == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, returnUrl, bufferedUrl);
            setViewBagsForLists(addition);

            return View(addition);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: Addition/Create
        [HttpGet]
        public IActionResult Create(string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            setViewBagsForLists(null);

            Addition addition = new Addition();
            addition.Contract = contract.Id;
            return View(addition);
        }



        // POST: Addition/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Addition addition, string? returnUrl, string? bufferedUrl)
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
                    context.Add(addition);
                    await context.SaveChangesAsync();

                    //---- Attachment attachment ----
                    if (addition.AttachmentInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("Attachment" ,addition.Contract, addition.Id, addition.AttachmentInput);
                        int result = await fileLoadService.UploadFile(serverFileName, addition.AttachmentInput, FileLoadService.PathAdditions);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(addition.Attachment, serverFileName);
                        // set
                        addition.Attachment = serverFileName;
                        context.Update(addition);
                        await context.SaveChangesAsync();
                    }


                    // Assign Code when created
                    addition.Code = (addition.Id + 1000).ToString("d5");
                    context.Update(addition);
                    await context.SaveChangesAsync();

                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(addition));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = addition.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Redeterminación";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
            {
                ViewBag.ValueValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Value");
            }

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            setViewBagsForLists(addition);
            return View(addition);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: Addition/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get Addition
            if ((id == null) || (id <= 0) || (context.Addition == null)) return NotFound();
            Addition? addition = await context.Addition
                                    .Include(t => t.AdditionAttachments!)
                .FirstAsync(r => r.Id == id);
            if (addition == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, returnUrl, bufferedUrl);
            setViewBagsForLists(addition);
            ViewBag.additionId = addition.Id;

            return View(addition);
        }


        // POST: Addition/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] Addition addition, string? returnUrl, string? bufferedUrl)
        {
            if (id != addition.Id)
            {
                return NotFound();
            }
            // Check if is code unique
            if (context.Addition.Any(c => c.Code == addition.Code && c.Id != addition.Id))
            {
                ModelState.AddModelError("Code", "Código interno existe en otro registro.");
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(addition);
                    await context.SaveChangesAsync();

                    //---- Attachment attachment ----
                    if (addition.AttachmentInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("Attachment" ,addition.Contract, addition.Id, addition.AttachmentInput);
                        int result = await fileLoadService.UploadFile(serverFileName, addition.AttachmentInput, FileLoadService.PathAdditions);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(addition.Attachment, serverFileName);
                        // set
                        addition.Attachment = serverFileName;
                        context.Update(addition);
                        await context.SaveChangesAsync();
                    }

                    logger.LogWarning(JsonConvert.SerializeObject(addition));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!AdditionExists(addition.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Redeterminación. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Redeterminación";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
            {
                ViewBag.ValueValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Value");
            }

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, returnUrl, bufferedUrl);
            setViewBagsForLists(addition);
            ViewBag.additionId = addition.Id;

            return View(addition);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: Addition/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.Addition == null)) return NotFound();
            Addition? addition = await context.Addition
                .Include(t => t.AdditionAttachments!)
                .FirstAsync(r => r.Id == id);
            
            if (addition == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            if (addition != null)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    deleteChildren(addition);
                    await context.SaveChangesAsync();
                    context.Addition.Remove(addition);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(addition));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Redeterminación";
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

            string path = fileLoadService.serverFullPath(FileLoadService.PathAdditions);
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
        IQueryable<Addition> orderBySelectedOrDefault(string sortOrder, IQueryable<Addition> additions)
        {
            ViewBag.dateDeliverySort = String.IsNullOrEmpty(sortOrder) ? "dateDelivery_desc" : "";
            ViewBag.codeSort = sortOrder == "code" ? "code_desc" : "code";
            ViewBag.valueSort = sortOrder == "value" ? "value_desc" : "value";
            ViewBag.stageSort = sortOrder == "stage" ? "stage_desc" : "stage";
            ViewBag.codeIcon = "bi-caret-down";
            ViewBag.valueIcon = "bi-caret-down";
            ViewBag.dateDeliveryIcon = "bi-caret-down";
            ViewBag.stageIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "code_desc":
                    additions = additions.OrderByDescending(o => o.Code);
                    ViewBag.codeIcon = "bi-caret-up-fill";
                    break;
                case "code":
                    additions = additions.OrderBy(o => o.Code);
                    ViewBag.codeIcon = "bi-caret-down-fill";
                    break;
                case "value_desc":
                    additions = additions.OrderByDescending(o => o.Value);
                    ViewBag.valueIcon = "bi-caret-up-fill";
                    break;
                case "value":
                    additions = additions.OrderBy(o => o.Value);
                    ViewBag.valueIcon = "bi-caret-down-fill";
                    break;
                case "stage_desc":
                    additions = additions.OrderByDescending(o => o.Stage);
                    ViewBag.stageIcon = "bi-caret-up-fill";
                    break;
                case "stage":
                    additions = additions.OrderBy(o => o.Stage);
                    ViewBag.stageIcon = "bi-caret-down-fill";
                    break;
                case "dateDelivery_desc":
                    additions = additions.OrderByDescending(t => t.DateDelivery);
                    ViewBag.dateDeliveryIcon = "bi-caret-up-fill";
                    break;
                default:
                    additions = additions.OrderBy(t => t.DateDelivery);
                    ViewBag.dateDeliveryIcon = "bi-caret-down-fill";
                    break;
            }
            return additions;
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
            // navAddition
            ViewBag.navAddition = $"{action}";
        }


        void setViewBagsForLists(Addition? addition)
        {

            // set options for Contract
            var listContract = new SelectList(context.Contract.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Code + " - " + r.Name, r.Id.ToString())), "Value", "Text", addition?.Contract).ToList();
            listContract.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listContract = listContract;

            // set options for Stage
            var listStage = new SelectList(context.AdditionStage.OrderBy(c => c.Order).Select(r => new SelectListItem(r.Name, r.Id.ToString())), "Value", "Text", addition?.Stage).ToList();
            listStage.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listStage = listStage;
        }


        //----------------------------------------------
        //==============================================
        // delete children
        async void deleteChildren(Addition addition)
        {
            if (addition.AdditionAttachments?.Count > 0)
                addition.AdditionAttachments.ToList().ForEach(c => context.AdditionAttachment.Remove(c));
        }
        #endregion


        //========================================================

        private bool AdditionExists(int id)
        {
            return (context.Addition?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
