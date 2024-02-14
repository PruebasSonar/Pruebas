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
    public partial class VarianteAttachmentController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly IParentContractService parentContractService;
        private readonly IFileLoadService fileLoadService;
        private readonly ILogger<VarianteAttachmentController> logger;


        public VarianteAttachmentController(PIPMUNI_ARGDbContext context
        , IParentContractService parentContractService
        , IFileLoadService fileLoadService
        , ILogger<VarianteAttachmentController> logger
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

        // GET: VarianteAttachment/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, int? varianteId, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            var varianteAttachments = varianteId.HasValue
            ? from o in context.VarianteAttachment where (o.Variante == varianteId) select o
            : from o in context.VarianteAttachment select o;
            if (!string.IsNullOrEmpty(searchText))
                varianteAttachments = varianteAttachments.Where(p => p.Title!.Contains(searchText) || p.DateAttached.ToString()!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, varianteId, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (varianteAttachments != null)
            {
                varianteAttachments = orderBySelectedOrDefault(sortOrder, varianteAttachments);
                return View(await varianteAttachments.ToListAsync());
            }
            return View(new List<VarianteAttachment>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: VarianteAttachment/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, int? varianteId, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            // get VarianteAttachment
            if ((id == null) || (id <= 0) || (context.VarianteAttachment == null)) return NotFound();
            VarianteAttachment? varianteAttachment = await context.VarianteAttachment
                .FindAsync(id);
            if (varianteAttachment == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, varianteId, returnUrl, bufferedUrl);
            setViewBagsForLists(varianteAttachment);

            return View(varianteAttachment);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: VarianteAttachment/Create
        [HttpGet]
        public IActionResult Create(int? varianteId, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            // set ViewBags
            setStandardViewBags("Create", true, varianteId, returnUrl, bufferedUrl);
            setViewBagsForLists(null);

            VarianteAttachment varianteAttachment = new VarianteAttachment();
            if (varianteId.HasValue) varianteAttachment.Variante = varianteId.Value;
            return View(varianteAttachment);
        }



        // POST: VarianteAttachment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] VarianteAttachment varianteAttachment, int? varianteId, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");

            // set UploadDate
            varianteAttachment.DateAttached = DateTime.Now;

            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(varianteAttachment);
                    await context.SaveChangesAsync();

                    //---- FileName attachment ----
                    if (varianteAttachment.FileNameInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("FileName" ,varianteAttachment.Variante, varianteAttachment.Id, varianteAttachment.FileNameInput);
                        int result = await fileLoadService.UploadFile(serverFileName, varianteAttachment.FileNameInput, FileLoadService.PathVarianteAttachments);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(varianteAttachment.FileName, serverFileName);
                        // set
                        varianteAttachment.FileName = serverFileName;
                        context.Update(varianteAttachment);
                        await context.SaveChangesAsync();
                    }

                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(varianteAttachment));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = varianteAttachment.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Anexo Variante";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, varianteId, returnUrl, bufferedUrl);
            setViewBagsForLists(varianteAttachment);
            return View(varianteAttachment);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: VarianteAttachment/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, int? varianteId, string? returnUrl, string? bufferedUrl)
        {
            // get VarianteAttachment
            if ((id == null) || (id <= 0) || (context.VarianteAttachment == null)) return NotFound();
            VarianteAttachment? varianteAttachment = await context.VarianteAttachment
                .FindAsync(id);
            if (varianteAttachment == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, varianteId, returnUrl, bufferedUrl);
            setViewBagsForLists(varianteAttachment);

            return View(varianteAttachment);
        }


        // POST: VarianteAttachment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] VarianteAttachment varianteAttachment, int? varianteId, string? returnUrl, string? bufferedUrl)
        {
            if (id != varianteAttachment.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(varianteAttachment);
                    await context.SaveChangesAsync();

                    //---- FileName attachment ----
                    if (varianteAttachment.FileNameInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("FileName" ,varianteAttachment.Variante, varianteAttachment.Id, varianteAttachment.FileNameInput);
                        int result = await fileLoadService.UploadFile(serverFileName, varianteAttachment.FileNameInput, FileLoadService.PathVarianteAttachments);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(varianteAttachment.FileName, serverFileName);
                        // set
                        varianteAttachment.FileName = serverFileName;
                        context.Update(varianteAttachment);
                        await context.SaveChangesAsync();
                    }

                    logger.LogWarning(JsonConvert.SerializeObject(varianteAttachment));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!VarianteAttachmentExists(varianteAttachment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Anexo Variante. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Anexo Variante";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, varianteId, returnUrl, bufferedUrl);
            setViewBagsForLists(varianteAttachment);

            return View(varianteAttachment);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: VarianteAttachment/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, int? varianteId, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.VarianteAttachment == null)) return NotFound();
            VarianteAttachment? varianteAttachment = await context.VarianteAttachment
                .FirstAsync(r => r.Id == id);
            
            if (varianteAttachment == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            if (varianteAttachment != null)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.VarianteAttachment.Remove(varianteAttachment);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(varianteAttachment));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Anexo Variante";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }

            //---- if not saved reload view ----
            // set ViewBags
            setStandardViewBags("Delete", false, varianteId, returnUrl, bufferedUrl);
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

            string path = fileLoadService.serverFullPath(FileLoadService.PathVarianteAttachments);
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
        IQueryable<VarianteAttachment> orderBySelectedOrDefault(string sortOrder, IQueryable<VarianteAttachment> varianteAttachments)
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
                    varianteAttachments = varianteAttachments.OrderByDescending(o => o.Title);
                    ViewBag.titleIcon = "bi-caret-up-fill";
                    break;
                case "title":
                    varianteAttachments = varianteAttachments.OrderBy(o => o.Title);
                    ViewBag.titleIcon = "bi-caret-down-fill";
                    break;
                case "fileName_desc":
                    varianteAttachments = varianteAttachments.OrderByDescending(o => o.FileName);
                    ViewBag.fileNameIcon = "bi-caret-up-fill";
                    break;
                case "fileName":
                    varianteAttachments = varianteAttachments.OrderBy(o => o.FileName);
                    ViewBag.fileNameIcon = "bi-caret-down-fill";
                    break;
                case "dateAttached_desc":
                    varianteAttachments = varianteAttachments.OrderByDescending(t => t.DateAttached);
                    ViewBag.dateAttachedIcon = "bi-caret-up-fill";
                    break;
                default:
                    varianteAttachments = varianteAttachments.OrderBy(t => t.DateAttached);
                    ViewBag.dateAttachedIcon = "bi-caret-down-fill";
                    break;
            }
            return varianteAttachments;
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
        void setStandardViewBags(string action, bool returnsbyDefault, int? varianteId, string? returnUrl, string? bufferedUrl)
        {
            if (varianteId.HasValue) ViewBag.varianteId = varianteId;
            // returnUrl
            if (!string.IsNullOrEmpty(returnUrl))
                ViewBag.returnUrl = returnUrl;
            else if (returnsbyDefault)
                ViewBag.returnUrl = HttpContext.Request.Headers["Referer"];
            // bufferedUrl
            if (!string.IsNullOrEmpty(bufferedUrl)) ViewBag.bufferedUrl = bufferedUrl;
            // navVarianteAttachment
            ViewBag.navVarianteAttachment = $"{action}";
        }


        void setViewBagsForLists(VarianteAttachment? varianteAttachment)
        {

            // set options for Variante
            var listVariante = new SelectList(context.Variante.Select(r => new SelectListItem(r.Code, r.Id.ToString())), "Value", "Text", varianteAttachment?.Variante).ToList();
            listVariante.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listVariante = listVariante;
        }


        //----------------------------------------------
        //==============================================
        #endregion


        //========================================================

        private bool VarianteAttachmentExists(int id)
        {
            return (context.VarianteAttachment?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
