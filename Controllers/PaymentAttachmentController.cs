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
    public partial class PaymentAttachmentController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly IParentContractService parentContractService;
        private readonly IFileLoadService fileLoadService;
        private readonly ILogger<PaymentAttachmentController> logger;


        public PaymentAttachmentController(PIPMUNI_ARGDbContext context
        , IParentContractService parentContractService
        , IFileLoadService fileLoadService
        , ILogger<PaymentAttachmentController> logger
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

        // GET: PaymentAttachment/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, int? paymentId, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            var paymentAttachments = paymentId.HasValue
            ? from o in context.PaymentAttachment where (o.Payment == paymentId) select o
            : from o in context.PaymentAttachment select o;
            if (!string.IsNullOrEmpty(searchText))
                paymentAttachments = paymentAttachments.Where(p => p.Title!.Contains(searchText) || p.DateAttached.ToString()!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, paymentId, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (paymentAttachments != null)
            {
                paymentAttachments = orderBySelectedOrDefault(sortOrder, paymentAttachments);
                return View(await paymentAttachments.ToListAsync());
            }
            return View(new List<PaymentAttachment>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: PaymentAttachment/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, int? paymentId, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            // get PaymentAttachment
            if ((id == null) || (id <= 0) || (context.PaymentAttachment == null)) return NotFound();
            PaymentAttachment? paymentAttachment = await context.PaymentAttachment
                .FindAsync(id);
            if (paymentAttachment == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, paymentId, returnUrl, bufferedUrl);
            setViewBagsForLists(paymentAttachment);

            return View(paymentAttachment);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: PaymentAttachment/Create
        [HttpGet]
        public IActionResult Create(int? paymentId, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            // set ViewBags
            setStandardViewBags("Create", true, paymentId, returnUrl, bufferedUrl);
            setViewBagsForLists(null);

            PaymentAttachment paymentAttachment = new PaymentAttachment();
            if (paymentId.HasValue) paymentAttachment.Payment = paymentId.Value;
            return View(paymentAttachment);
        }



        // POST: PaymentAttachment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] PaymentAttachment paymentAttachment, int? paymentId, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");

            // set UploadDate
            paymentAttachment.DateAttached = DateTime.Now;

            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(paymentAttachment);
                    await context.SaveChangesAsync();

                    //---- File attachment ----
                    if (paymentAttachment.FileInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("File" ,paymentAttachment.Payment, paymentAttachment.Id, paymentAttachment.FileInput);
                        int result = await fileLoadService.UploadFile(serverFileName, paymentAttachment.FileInput, FileLoadService.PathPaymentAttachments);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(paymentAttachment.File, serverFileName);
                        // set
                        paymentAttachment.File = serverFileName;
                        context.Update(paymentAttachment);
                        await context.SaveChangesAsync();
                    }

                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(paymentAttachment));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = paymentAttachment.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Anexo Certificado";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, paymentId, returnUrl, bufferedUrl);
            setViewBagsForLists(paymentAttachment);
            return View(paymentAttachment);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: PaymentAttachment/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, int? paymentId, string? returnUrl, string? bufferedUrl)
        {
            // get PaymentAttachment
            if ((id == null) || (id <= 0) || (context.PaymentAttachment == null)) return NotFound();
            PaymentAttachment? paymentAttachment = await context.PaymentAttachment
                .FindAsync(id);
            if (paymentAttachment == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, paymentId, returnUrl, bufferedUrl);
            setViewBagsForLists(paymentAttachment);

            return View(paymentAttachment);
        }


        // POST: PaymentAttachment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] PaymentAttachment paymentAttachment, int? paymentId, string? returnUrl, string? bufferedUrl)
        {
            if (id != paymentAttachment.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(paymentAttachment);
                    await context.SaveChangesAsync();

                    //---- File attachment ----
                    if (paymentAttachment.FileInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("File" ,paymentAttachment.Payment, paymentAttachment.Id, paymentAttachment.FileInput);
                        int result = await fileLoadService.UploadFile(serverFileName, paymentAttachment.FileInput, FileLoadService.PathPaymentAttachments);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(paymentAttachment.File, serverFileName);
                        // set
                        paymentAttachment.File = serverFileName;
                        context.Update(paymentAttachment);
                        await context.SaveChangesAsync();
                    }

                    logger.LogWarning(JsonConvert.SerializeObject(paymentAttachment));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!PaymentAttachmentExists(paymentAttachment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Anexo Certificado. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Anexo Certificado";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, paymentId, returnUrl, bufferedUrl);
            setViewBagsForLists(paymentAttachment);

            return View(paymentAttachment);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: PaymentAttachment/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, int? paymentId, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.PaymentAttachment == null)) return NotFound();
            PaymentAttachment? paymentAttachment = await context.PaymentAttachment
                .FirstAsync(r => r.Id == id);
            
            if (paymentAttachment == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            if (paymentAttachment != null)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.PaymentAttachment.Remove(paymentAttachment);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(paymentAttachment));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Anexo Certificado";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }

            //---- if not saved reload view ----
            // set ViewBags
            setStandardViewBags("Delete", false, paymentId, returnUrl, bufferedUrl);
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

            string path = fileLoadService.serverFullPath(FileLoadService.PathPaymentAttachments);
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
        IQueryable<PaymentAttachment> orderBySelectedOrDefault(string sortOrder, IQueryable<PaymentAttachment> paymentAttachments)
        {
            ViewBag.dateAttachedSort = String.IsNullOrEmpty(sortOrder) ? "dateAttached_desc" : "";
            ViewBag.titleSort = sortOrder == "title" ? "title_desc" : "title";
            ViewBag.fileSort = sortOrder == "file" ? "file_desc" : "file";
            ViewBag.titleIcon = "bi-caret-down";
            ViewBag.fileIcon = "bi-caret-down";
            ViewBag.dateAttachedIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "title_desc":
                    paymentAttachments = paymentAttachments.OrderByDescending(o => o.Title);
                    ViewBag.titleIcon = "bi-caret-up-fill";
                    break;
                case "title":
                    paymentAttachments = paymentAttachments.OrderBy(o => o.Title);
                    ViewBag.titleIcon = "bi-caret-down-fill";
                    break;
                case "file_desc":
                    paymentAttachments = paymentAttachments.OrderByDescending(o => o.File);
                    ViewBag.fileIcon = "bi-caret-up-fill";
                    break;
                case "file":
                    paymentAttachments = paymentAttachments.OrderBy(o => o.File);
                    ViewBag.fileIcon = "bi-caret-down-fill";
                    break;
                case "dateAttached_desc":
                    paymentAttachments = paymentAttachments.OrderByDescending(t => t.DateAttached);
                    ViewBag.dateAttachedIcon = "bi-caret-up-fill";
                    break;
                default:
                    paymentAttachments = paymentAttachments.OrderBy(t => t.DateAttached);
                    ViewBag.dateAttachedIcon = "bi-caret-down-fill";
                    break;
            }
            return paymentAttachments;
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
        void setStandardViewBags(string action, bool returnsbyDefault, int? paymentId, string? returnUrl, string? bufferedUrl)
        {
            if (paymentId.HasValue) ViewBag.paymentId = paymentId;
            // returnUrl
            if (!string.IsNullOrEmpty(returnUrl))
                ViewBag.returnUrl = returnUrl;
            else if (returnsbyDefault)
                ViewBag.returnUrl = HttpContext.Request.Headers["Referer"];
            // bufferedUrl
            if (!string.IsNullOrEmpty(bufferedUrl)) ViewBag.bufferedUrl = bufferedUrl;
            // navPaymentAttachment
            ViewBag.navPaymentAttachment = $"{action}";
        }


        void setViewBagsForLists(PaymentAttachment? paymentAttachment)
        {

            // set options for Payment
            var listPayment = new SelectList(context.Payment.OrderBy(c => c.DateDelivery).Select(r => new SelectListItem(r.Code + " - " + ((r.DateDelivery.HasValue == true) ? r.DateDelivery.Value.ToString("yyyy-MMM-dd") : ""), r.Id.ToString())), "Value", "Text", paymentAttachment?.Payment).ToList();
            listPayment.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listPayment = listPayment;
        }


        //----------------------------------------------
        //==============================================
        #endregion


        //========================================================

        private bool PaymentAttachmentExists(int id)
        {
            return (context.PaymentAttachment?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
