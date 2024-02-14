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
    public partial class AdditionAttachmentController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly IParentContractService parentContractService;
        private readonly IFileLoadService fileLoadService;
        private readonly ILogger<AdditionAttachmentController> logger;


        public AdditionAttachmentController(PIPMUNI_ARGDbContext context
        , IParentContractService parentContractService
        , IFileLoadService fileLoadService
        , ILogger<AdditionAttachmentController> logger
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

        // GET: AdditionAttachment/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, int? additionId, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            var additionAttachments = additionId.HasValue
            ? from o in context.AdditionAttachment where (o.Addition == additionId) select o
            : from o in context.AdditionAttachment select o;
            if (!string.IsNullOrEmpty(searchText))
                additionAttachments = additionAttachments.Where(p => p.Title!.Contains(searchText) || p.DateAttached.ToString()!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, additionId, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (additionAttachments != null)
            {
                additionAttachments = orderBySelectedOrDefault(sortOrder, additionAttachments);
                return View(await additionAttachments.ToListAsync());
            }
            return View(new List<AdditionAttachment>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: AdditionAttachment/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, int? additionId, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            // get AdditionAttachment
            if ((id == null) || (id <= 0) || (context.AdditionAttachment == null)) return NotFound();
            AdditionAttachment? additionAttachment = await context.AdditionAttachment
                .FindAsync(id);
            if (additionAttachment == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, additionId, returnUrl, bufferedUrl);
            setViewBagsForLists(additionAttachment);

            return View(additionAttachment);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: AdditionAttachment/Create
        [HttpGet]
        public IActionResult Create(int? additionId, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            // set ViewBags
            setStandardViewBags("Create", true, additionId, returnUrl, bufferedUrl);
            setViewBagsForLists(null);

            AdditionAttachment additionAttachment = new AdditionAttachment();
            if (additionId.HasValue) additionAttachment.Addition = additionId.Value;
            return View(additionAttachment);
        }



        // POST: AdditionAttachment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] AdditionAttachment additionAttachment, int? additionId, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");

            // set UploadDate
            additionAttachment.DateAttached = DateTime.Now;

            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(additionAttachment);
                    await context.SaveChangesAsync();

                    //---- FileName attachment ----
                    if (additionAttachment.FileNameInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("FileName" ,additionAttachment.Addition, additionAttachment.Id, additionAttachment.FileNameInput);
                        int result = await fileLoadService.UploadFile(serverFileName, additionAttachment.FileNameInput, FileLoadService.PathAdditionAttachments);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(additionAttachment.FileName, serverFileName);
                        // set
                        additionAttachment.FileName = serverFileName;
                        context.Update(additionAttachment);
                        await context.SaveChangesAsync();
                    }

                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(additionAttachment));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = additionAttachment.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Anexo Redeterminación";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, additionId, returnUrl, bufferedUrl);
            setViewBagsForLists(additionAttachment);
            return View(additionAttachment);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: AdditionAttachment/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, int? additionId, string? returnUrl, string? bufferedUrl)
        {
            // get AdditionAttachment
            if ((id == null) || (id <= 0) || (context.AdditionAttachment == null)) return NotFound();
            AdditionAttachment? additionAttachment = await context.AdditionAttachment
                .FindAsync(id);
            if (additionAttachment == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, additionId, returnUrl, bufferedUrl);
            setViewBagsForLists(additionAttachment);

            return View(additionAttachment);
        }


        // POST: AdditionAttachment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] AdditionAttachment additionAttachment, int? additionId, string? returnUrl, string? bufferedUrl)
        {
            if (id != additionAttachment.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(additionAttachment);
                    await context.SaveChangesAsync();

                    //---- FileName attachment ----
                    if (additionAttachment.FileNameInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("FileName" ,additionAttachment.Addition, additionAttachment.Id, additionAttachment.FileNameInput);
                        int result = await fileLoadService.UploadFile(serverFileName, additionAttachment.FileNameInput, FileLoadService.PathAdditionAttachments);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(additionAttachment.FileName, serverFileName);
                        // set
                        additionAttachment.FileName = serverFileName;
                        context.Update(additionAttachment);
                        await context.SaveChangesAsync();
                    }

                    logger.LogWarning(JsonConvert.SerializeObject(additionAttachment));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!AdditionAttachmentExists(additionAttachment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Anexo Redeterminación. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Anexo Redeterminación";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, additionId, returnUrl, bufferedUrl);
            setViewBagsForLists(additionAttachment);

            return View(additionAttachment);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: AdditionAttachment/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, int? additionId, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.AdditionAttachment == null)) return NotFound();
            AdditionAttachment? additionAttachment = await context.AdditionAttachment
                .FirstAsync(r => r.Id == id);
            
            if (additionAttachment == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            if (additionAttachment != null)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.AdditionAttachment.Remove(additionAttachment);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(additionAttachment));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Anexo Redeterminación";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }

            //---- if not saved reload view ----
            // set ViewBags
            setStandardViewBags("Delete", false, additionId, returnUrl, bufferedUrl);
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

            string path = fileLoadService.serverFullPath(FileLoadService.PathAdditionAttachments);
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
        IQueryable<AdditionAttachment> orderBySelectedOrDefault(string sortOrder, IQueryable<AdditionAttachment> additionAttachments)
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
                    additionAttachments = additionAttachments.OrderByDescending(o => o.Title);
                    ViewBag.titleIcon = "bi-caret-up-fill";
                    break;
                case "title":
                    additionAttachments = additionAttachments.OrderBy(o => o.Title);
                    ViewBag.titleIcon = "bi-caret-down-fill";
                    break;
                case "fileName_desc":
                    additionAttachments = additionAttachments.OrderByDescending(o => o.FileName);
                    ViewBag.fileNameIcon = "bi-caret-up-fill";
                    break;
                case "fileName":
                    additionAttachments = additionAttachments.OrderBy(o => o.FileName);
                    ViewBag.fileNameIcon = "bi-caret-down-fill";
                    break;
                case "dateAttached_desc":
                    additionAttachments = additionAttachments.OrderByDescending(t => t.DateAttached);
                    ViewBag.dateAttachedIcon = "bi-caret-up-fill";
                    break;
                default:
                    additionAttachments = additionAttachments.OrderBy(t => t.DateAttached);
                    ViewBag.dateAttachedIcon = "bi-caret-down-fill";
                    break;
            }
            return additionAttachments;
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
        void setStandardViewBags(string action, bool returnsbyDefault, int? additionId, string? returnUrl, string? bufferedUrl)
        {
            if (additionId.HasValue) ViewBag.additionId = additionId;
            // returnUrl
            if (!string.IsNullOrEmpty(returnUrl))
                ViewBag.returnUrl = returnUrl;
            else if (returnsbyDefault)
                ViewBag.returnUrl = HttpContext.Request.Headers["Referer"];
            // bufferedUrl
            if (!string.IsNullOrEmpty(bufferedUrl)) ViewBag.bufferedUrl = bufferedUrl;
            // navAdditionAttachment
            ViewBag.navAdditionAttachment = $"{action}";
        }


        void setViewBagsForLists(AdditionAttachment? additionAttachment)
        {

            // set options for Addition
            var listAddition = new SelectList(context.Addition.OrderBy(c => c.DateDelivery).Select(r => new SelectListItem(r.Code + " - " + ((r.DateDelivery.HasValue == true) ? r.DateDelivery.Value.ToString("yyyy-MMM-dd") : ""), r.Id.ToString())), "Value", "Text", additionAttachment?.Addition).ToList();
            listAddition.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listAddition = listAddition;
        }


        //----------------------------------------------
        //==============================================
        #endregion


        //========================================================

        private bool AdditionAttachmentExists(int id)
        {
            return (context.AdditionAttachment?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
