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
    [Authorize(Roles = ProjectGlobals.registeredRoles)]
    public partial class PaymentController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly IParentContractService parentContractService;
        private readonly IFileLoadService fileLoadService;
        private readonly ILogger<PaymentController> logger;


        public PaymentController(PIPMUNI_ARGDbContext context
        , IParentContractService parentContractService
        , IFileLoadService fileLoadService
        , ILogger<PaymentController> logger
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

        // GET: Payment/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            var payments = from o in context.Payment where (o.Contract == contract.Id) select o;
            if (!string.IsNullOrEmpty(searchText))
                payments = payments.Where(p => p.Code!.Contains(searchText) || p.Type_info!.Name!.Contains(searchText) || p.Total.ToString()!.Contains(searchText) || p.DateDelivery.ToString()!.Contains(searchText) || p.Stage_info!.Name!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (payments != null)
            {
                payments = orderBySelectedOrDefault(sortOrder, payments);
                payments = payments
                    .Include(p => p.Type_info)
                    .Include(p => p.Stage_info);
                return View(await payments.ToListAsync());
            }
            return View(new List<Payment>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: Payment/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            // get Payment
            if ((id == null) || (id <= 0) || (context.Payment == null)) return NotFound();
            Payment? payment = await context.Payment
                                    .Include(t => t.PaymentAttachments!)
                .FirstAsync(r => r.Id == id);
            if (payment == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, returnUrl, bufferedUrl);
            setViewBagsForLists(payment);

            return View(payment);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: Payment/Create
        [HttpGet]
        public IActionResult Create(string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            setViewBagsForLists(null);

            Payment payment = new Payment();
            payment.Contract = contract.Id;
            return View(payment);
        }



        // POST: Payment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Payment payment, string? returnUrl, string? bufferedUrl)
        {
            Contract contract = parentContractService.getSessionContract(HttpContext.Session, ViewBag);
            if (!(contract?.Id > 0)) return RedirectToAction("Select", "Contract");

            // Validate Total Payment Greater than Project Cost
            // checks if total payments is greater than total programmed.
            AdvanceBasic? advance = paymentsGreaterThanProgrammed(payment);
            if (advance != null && advance.invalid)
            {
                ModelState.AddModelError("Total",
                string.Format("Suma de montos Certificados ({0}) superior a monto actual de la obra ({1})."
                                , advance.actual.HasValue ? advance.actual.Value.ToString("n2") : ""
                                , advance.programmed.HasValue ? advance.programmed.Value.ToString("n2") : ""
                            ));
            }

            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(payment);
                    await context.SaveChangesAsync();

                    //---- AttachmentMedicion attachment ----
                    if (payment.AttachmentMedicionInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("AttachmentMedicion", payment.Contract, payment.Id, payment.AttachmentMedicionInput);
                        int result = await fileLoadService.UploadFile(serverFileName, payment.AttachmentMedicionInput, FileLoadService.PathPayments);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(payment.AttachmentMedicion, serverFileName);
                        // set
                        payment.AttachmentMedicion = serverFileName;
                        context.Update(payment);
                        await context.SaveChangesAsync();
                    }

                    //---- AttachmentOrden attachment ----
                    if (payment.AttachmentOrdenInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("AttachmentOrden", payment.Contract, payment.Id, payment.AttachmentOrdenInput);
                        int result = await fileLoadService.UploadFile(serverFileName, payment.AttachmentOrdenInput, FileLoadService.PathPayments);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(payment.AttachmentOrden, serverFileName);
                        // set
                        payment.AttachmentOrden = serverFileName;
                        context.Update(payment);
                        await context.SaveChangesAsync();
                    }


                    // Assign Code when created
                    payment.Code = (payment.Id + 1000).ToString("d5");
                    context.Update(payment);
                    await context.SaveChangesAsync();

                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(payment));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = payment.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Certificado";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
            {
                ViewBag.NumberValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Number");
                ViewBag.ValueValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Value");
                ViewBag.DiferenciaValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Diferencia");
                ViewBag.DescuentoFinancieroValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "DescuentoFinanciero");
                ViewBag.DescuentoOtrosValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "DescuentoOtros");
                ViewBag.PhysicalAdvanceValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "PhysicalAdvance");
                ViewBag.TotalValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Total");
            }

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            setViewBagsForLists(payment);


            ViewBag.AttachmentMedicionInput = payment.AttachmentMedicionInput;

            return View(payment);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: Payment/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get Payment
            if ((id == null) || (id <= 0) || (context.Payment == null)) return NotFound();
            Payment? payment = await context.Payment
                                    .Include(t => t.PaymentAttachments!)
                .FirstAsync(r => r.Id == id);
            if (payment == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, returnUrl, bufferedUrl);
            setViewBagsForLists(payment);
            ViewBag.paymentId = payment.Id;

            return View(payment);
        }


        // POST: Payment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, [FromForm] Payment payment, string? returnUrl, string? bufferedUrl)
        {
            if (id != payment.Id)
            {
                return NotFound();
            }

            // Validate Total Payment Greater than Project Cost
            // checks if total payments is greater than total programmed.
            AdvanceBasic? advance = paymentsGreaterThanProgrammed(payment);
            if (advance != null && advance.invalid)
            {
                ModelState.AddModelError("Total",
                string.Format("Suma de montos Certificados ({0}) superior a monto actual de la obra ({1})."
                                , advance.actual.HasValue ? advance.actual.Value.ToString("n2") : ""
                                , advance.programmed.HasValue ? advance.programmed.Value.ToString("n2") : ""
                            ));
            }

            // Check if is code unique
            if (context.Payment.Any(c => c.Code == payment.Code && c.Id != payment.Id))
            {
                ModelState.AddModelError("Code", "Código interno existe en otro registro.");
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(payment);
                    await context.SaveChangesAsync();

                    //---- AttachmentMedicion attachment ----
                    if (payment.AttachmentMedicionInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("AttachmentMedicion", payment.Contract, payment.Id, payment.AttachmentMedicionInput);
                        int result = await fileLoadService.UploadFile(serverFileName, payment.AttachmentMedicionInput, FileLoadService.PathPayments);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(payment.AttachmentMedicion, serverFileName);
                        // set
                        payment.AttachmentMedicion = serverFileName;
                        context.Update(payment);
                        await context.SaveChangesAsync();
                    }
                    //---- AttachmentMedicion attachment ----
                    if (payment.AttachmentMedicionInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("AttachmentMedicion", payment.Contract, payment.Id, payment.AttachmentMedicionInput);
                        int result = await fileLoadService.UploadFile(serverFileName, payment.AttachmentMedicionInput, FileLoadService.PathPayments);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(payment.AttachmentMedicion, serverFileName);
                        // set
                        payment.AttachmentMedicion = serverFileName;
                        context.Update(payment);
                        await context.SaveChangesAsync();
                    }

                    //---- AttachmentOrden attachment ----
                    if (payment.AttachmentOrdenInput != null)
                    {
                        // upload
                        string serverFileName = fileLoadService.serverFileName("AttachmentOrden", payment.Contract, payment.Id, payment.AttachmentOrdenInput);
                        int result = await fileLoadService.UploadFile(serverFileName, payment.AttachmentOrdenInput, FileLoadService.PathPayments);
                        if (result != FileLoadService.resultOK)
                            throw new FileLoadService.UploadFileException(payment.AttachmentOrden, serverFileName);
                        // set
                        payment.AttachmentOrden = serverFileName;
                        context.Update(payment);
                        await context.SaveChangesAsync();
                    }

                    logger.LogWarning(JsonConvert.SerializeObject(payment));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!PaymentExists(payment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Certificado. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Certificado";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
            {
                ViewBag.NumberValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Number");
                ViewBag.ValueValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Value");
                ViewBag.DiferenciaValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Diferencia");
                ViewBag.DescuentoFinancieroValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "DescuentoFinanciero");
                ViewBag.DescuentoOtrosValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "DescuentoOtros");
                ViewBag.PhysicalAdvanceValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "PhysicalAdvance");
                ViewBag.TotalValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Total");
            }

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, returnUrl, bufferedUrl);
            setViewBagsForLists(payment);
            ViewBag.paymentId = payment.Id;

            return View(payment);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: Payment/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.Payment == null)) return NotFound();
            Payment? payment = await context.Payment
                .Include(t => t.PaymentAttachments!)
                .FirstAsync(r => r.Id == id);

            if (payment == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            if (payment != null)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    deleteChildren(payment);
                    await context.SaveChangesAsync();
                    context.Payment.Remove(payment);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(payment));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Certificado";
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

        public IActionResult Download(string serverFileName, string downloadName)
        {
            if (string.IsNullOrEmpty(serverFileName)) return NoContent();

            string path = fileLoadService.serverFullPath(FileLoadService.PathPayments);
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
        IQueryable<Payment> orderBySelectedOrDefault(string sortOrder, IQueryable<Payment> payments)
        {
            ViewBag.dateDeliverySort = String.IsNullOrEmpty(sortOrder) ? "dateDelivery_desc" : "";
            ViewBag.codeSort = sortOrder == "code" ? "code_desc" : "code";
            ViewBag.typeSort = sortOrder == "type" ? "type_desc" : "type";
            ViewBag.totalSort = sortOrder == "total" ? "total_desc" : "total";
            ViewBag.stageSort = sortOrder == "stage" ? "stage_desc" : "stage";
            ViewBag.codeIcon = "bi-caret-down";
            ViewBag.typeIcon = "bi-caret-down";
            ViewBag.totalIcon = "bi-caret-down";
            ViewBag.dateDeliveryIcon = "bi-caret-down";
            ViewBag.stageIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "code_desc":
                    payments = payments.OrderByDescending(o => o.Code);
                    ViewBag.codeIcon = "bi-caret-up-fill";
                    break;
                case "code":
                    payments = payments.OrderBy(o => o.Code);
                    ViewBag.codeIcon = "bi-caret-down-fill";
                    break;
                case "type_desc":
                    payments = payments.OrderByDescending(o => o.Type);
                    ViewBag.typeIcon = "bi-caret-up-fill";
                    break;
                case "type":
                    payments = payments.OrderBy(o => o.Type);
                    ViewBag.typeIcon = "bi-caret-down-fill";
                    break;
                case "total_desc":
                    payments = payments.OrderByDescending(o => o.Total);
                    ViewBag.totalIcon = "bi-caret-up-fill";
                    break;
                case "total":
                    payments = payments.OrderBy(o => o.Total);
                    ViewBag.totalIcon = "bi-caret-down-fill";
                    break;
                case "stage_desc":
                    payments = payments.OrderByDescending(o => o.Stage);
                    ViewBag.stageIcon = "bi-caret-up-fill";
                    break;
                case "stage":
                    payments = payments.OrderBy(o => o.Stage);
                    ViewBag.stageIcon = "bi-caret-down-fill";
                    break;
                case "dateDelivery_desc":
                    payments = payments.OrderByDescending(t => t.DateDelivery);
                    ViewBag.dateDeliveryIcon = "bi-caret-up-fill";
                    break;
                default:
                    payments = payments.OrderBy(t => t.DateDelivery);
                    ViewBag.dateDeliveryIcon = "bi-caret-down-fill";
                    break;
            }
            return payments;
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
            // navPayment
            ViewBag.navPayment = $"{action}";
        }


        void setViewBagsForLists(Payment? payment)
        {

            // set options for Contract
            var listContract = new SelectList(context.Contract.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Code + " - " + r.Name, r.Id.ToString())), "Value", "Text", payment?.Contract).ToList();
            listContract.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listContract = listContract;

            // set options for Type
            var listType = new SelectList(context.PaymentType.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Name, r.Id.ToString())), "Value", "Text", payment?.Type).ToList();
            listType.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listType = listType;

            // set options for Stage
            var listStage = new SelectList(context.PaymentStage.OrderBy(c => c.Order).Select(r => new SelectListItem(r.Name, r.Id.ToString())), "Value", "Text", payment?.Stage).ToList();
            listStage.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listStage = listStage;
        }


        //----------------------------------------------
        //==============================================
        // delete children
        async void deleteChildren(Payment payment)
        {
            if (payment.PaymentAttachments?.Count > 0)
                payment.PaymentAttachments.ToList().ForEach(c => context.PaymentAttachment.Remove(c));
        }
        #endregion


        //========================================================

        private bool PaymentExists(int id)
        {
            return (context.Payment?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
