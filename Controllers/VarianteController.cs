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
    public partial class VarianteController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly IParentContractService parentContractService;
        private readonly IFileLoadService fileLoadService;
        private readonly ILogger<VarianteController> logger;


        public VarianteController(PIPMUNI_ARGDbContext context
        , IParentContractService parentContractService
        , IFileLoadService fileLoadService
        , ILogger<VarianteController> logger
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

        // GET: Variante/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            var variantes = from o in context.Variante where (o.Contract == contract.Id) select o;
            if (!string.IsNullOrEmpty(searchText))
                variantes = variantes.Where(p => p.Code!.Contains(searchText) || p.Motivo_info!.Name!.Contains(searchText) || p.Value.ToString()!.Contains(searchText) || p.DateDelivery.ToString()!.Contains(searchText) || p.Stage_info!.Name!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (variantes != null)
            {
                variantes = orderBySelectedOrDefault(sortOrder, variantes);
                variantes = variantes
                    .Include(p => p.Motivo_info)
                    .Include(p => p.Stage_info);
                return View(await variantes.ToListAsync());
            }
            return View(new List<Variante>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: Variante/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            // get Variante
            if ((id == null) || (id <= 0) || (context.Variante == null)) return NotFound();
            Variante? variante = await context.Variante
                                    .Include(t => t.VarianteAttachments!)
                .FirstAsync(r => r.Id == id);
            if (variante == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, returnUrl, bufferedUrl);
            setViewBagsForLists(variante);

            return View(variante);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: Variante/Create
        [HttpGet]
        public IActionResult Create(string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            setViewBagsForLists(null);

            Variante variante = new Variante();
            variante.Contract = contract.Id;
            return View(variante);
        }



        // POST: Variante/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Variante variante, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            unselectedLinksNulled(variante);
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(variante);
                    await context.SaveChangesAsync();

                    //---- Attachment attachment ----
                    if (variante.AttachmentInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("Attachment" ,variante.Contract, variante.Id, variante.AttachmentInput);
                        int result = await fileLoadService.UploadFile(serverFileName, variante.AttachmentInput, FileLoadService.PathVariantes);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(variante.Attachment, serverFileName);
                        // set
                        variante.Attachment = serverFileName;
                        context.Update(variante);
                        await context.SaveChangesAsync();
                    }


                    // Assign Code when created
                    variante.Code = (variante.Id + 1000).ToString("d5");
                    context.Update(variante);
                    await context.SaveChangesAsync();

                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(variante));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = variante.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Variante de Obra";
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
            setViewBagsForLists(variante);
            return View(variante);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: Variante/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get Variante
            if ((id == null) || (id <= 0) || (context.Variante == null)) return NotFound();
            Variante? variante = await context.Variante
                                    .Include(t => t.VarianteAttachments!)
                .FirstAsync(r => r.Id == id);
            if (variante == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, returnUrl, bufferedUrl);
            setViewBagsForLists(variante);
            ViewBag.varianteId = variante.Id;

            return View(variante);
        }


        // POST: Variante/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] Variante variante, string? returnUrl, string? bufferedUrl)
        {
            if (id != variante.Id)
            {
                return NotFound();
            }
            unselectedLinksNulled(variante);
            // Check if is code unique
            if (context.Variante.Any(c => c.Code == variante.Code && c.Id != variante.Id))
            {
                ModelState.AddModelError("Code", "Código interno existe en otro registro.");
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(variante);
                    await context.SaveChangesAsync();

                    //---- Attachment attachment ----
                    if (variante.AttachmentInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("Attachment" ,variante.Contract, variante.Id, variante.AttachmentInput);
                        int result = await fileLoadService.UploadFile(serverFileName, variante.AttachmentInput, FileLoadService.PathVariantes);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(variante.Attachment, serverFileName);
                        // set
                        variante.Attachment = serverFileName;
                        context.Update(variante);
                        await context.SaveChangesAsync();
                    }

                    logger.LogWarning(JsonConvert.SerializeObject(variante));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!VarianteExists(variante.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Variante de Obra. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Variante de Obra";
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
            setViewBagsForLists(variante);
            ViewBag.varianteId = variante.Id;

            return View(variante);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: Variante/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.Variante == null)) return NotFound();
            Variante? variante = await context.Variante
                .Include(t => t.VarianteAttachments!)
                .FirstAsync(r => r.Id == id);
            
            if (variante == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            if (variante != null)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    deleteChildren(variante);
                    await context.SaveChangesAsync();
                    context.Variante.Remove(variante);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(variante));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Variante de Obra";
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

            string path = fileLoadService.serverFullPath(FileLoadService.PathVariantes);
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
        IQueryable<Variante> orderBySelectedOrDefault(string sortOrder, IQueryable<Variante> variantes)
        {
            ViewBag.codeSort = String.IsNullOrEmpty(sortOrder) ? "code_desc" : "";
            ViewBag.motivoSort = sortOrder == "motivo" ? "motivo_desc" : "motivo";
            ViewBag.valueSort = sortOrder == "value" ? "value_desc" : "value";
            ViewBag.dateDeliverySort = sortOrder == "dateDelivery" ? "dateDelivery_desc" : "dateDelivery";
            ViewBag.stageSort = sortOrder == "stage" ? "stage_desc" : "stage";
            ViewBag.codeIcon = "bi-caret-down";
            ViewBag.motivoIcon = "bi-caret-down";
            ViewBag.valueIcon = "bi-caret-down";
            ViewBag.dateDeliveryIcon = "bi-caret-down";
            ViewBag.stageIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "motivo_desc":
                    variantes = variantes.OrderByDescending(o => o.Motivo);
                    ViewBag.motivoIcon = "bi-caret-up-fill";
                    break;
                case "motivo":
                    variantes = variantes.OrderBy(o => o.Motivo);
                    ViewBag.motivoIcon = "bi-caret-down-fill";
                    break;
                case "value_desc":
                    variantes = variantes.OrderByDescending(o => o.Value);
                    ViewBag.valueIcon = "bi-caret-up-fill";
                    break;
                case "value":
                    variantes = variantes.OrderBy(o => o.Value);
                    ViewBag.valueIcon = "bi-caret-down-fill";
                    break;
                case "dateDelivery_desc":
                    variantes = variantes.OrderByDescending(o => o.DateDelivery);
                    ViewBag.dateDeliveryIcon = "bi-caret-up-fill";
                    break;
                case "dateDelivery":
                    variantes = variantes.OrderBy(o => o.DateDelivery);
                    ViewBag.dateDeliveryIcon = "bi-caret-down-fill";
                    break;
                case "stage_desc":
                    variantes = variantes.OrderByDescending(o => o.Stage);
                    ViewBag.stageIcon = "bi-caret-up-fill";
                    break;
                case "stage":
                    variantes = variantes.OrderBy(o => o.Stage);
                    ViewBag.stageIcon = "bi-caret-down-fill";
                    break;
                case "code_desc":
                    variantes = variantes;
                    ViewBag.codeIcon = "bi-caret-up-fill";
                    break;
                default:
                    variantes = variantes;
                    ViewBag.codeIcon = "bi-caret-down-fill";
                    break;
            }
            return variantes;
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
            // navVariante
            ViewBag.navVariante = $"{action}";
        }


        void setViewBagsForLists(Variante? variante)
        {

            // set options for Contract
            var listContract = new SelectList(context.Contract.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Code + " - " + r.Name, r.Id.ToString())), "Value", "Text", variante?.Contract).ToList();
            listContract.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listContract = listContract;

            // set options for Motivo
            var listMotivo = new SelectList(context.VarianteMotivo.OrderBy(c => c.Id).Select(r => new SelectListItem(r.Name, r.Id.ToString())), "Value", "Text", variante?.Motivo).ToList();
            listMotivo.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listMotivo = listMotivo;

            // set options for Stage
            var listStage = new SelectList(context.AdditionStage.OrderBy(c => c.Order).Select(r => new SelectListItem(r.Name, r.Id.ToString())), "Value", "Text", variante?.Stage).ToList();
            listStage.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listStage = listStage;
        }


        void unselectedLinksNulled(Variante variante)
        {
            // sets unselected lists to null
            if (variante.Motivo == 0) variante.Motivo = null;

        }

        //----------------------------------------------
        //==============================================
        // delete children
        async void deleteChildren(Variante variante)
        {
            if (variante.VarianteAttachments?.Count > 0)
                variante.VarianteAttachments.ToList().ForEach(c => context.VarianteAttachment.Remove(c));
        }
        #endregion


        //========================================================

        private bool VarianteExists(int id)
        {
            return (context.Variante?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
