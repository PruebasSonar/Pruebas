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
    public partial class PaymentStageController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly ILogger<PaymentStageController> logger;


        public PaymentStageController(PIPMUNI_ARGDbContext context
        , ILogger<PaymentStageController> logger
        )
        {
            this.context = context;
            this.logger = logger;
        }

        JaosLibUtils jaosLibUtils = new JaosLibUtils();

        #region Index


        //----------- Index

        // GET: PaymentStage/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, string? returnUrl, string? bufferedUrl)
        {
            var paymentStages = from o in context.PaymentStage select o;
            if (!string.IsNullOrEmpty(searchText))
                paymentStages = paymentStages.Where(p => p.Name!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (paymentStages != null)
            {
                paymentStages = orderBySelectedOrDefault(sortOrder, paymentStages);
                return View(await paymentStages.ToListAsync());
            }
            return View(new List<PaymentStage>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: PaymentStage/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get PaymentStage
            if ((id == null) || (id <= 0) || (context.PaymentStage == null)) return NotFound();
            PaymentStage? paymentStage = await context.PaymentStage
                .FindAsync(id);
            if (paymentStage == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, returnUrl, bufferedUrl);

            return View(paymentStage);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: PaymentStage/Create
        [HttpGet]
        public IActionResult Create(string? returnUrl, string? bufferedUrl)
        {
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);

            return View();
        }



        // POST: PaymentStage/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] PaymentStage paymentStage, string? returnUrl, string? bufferedUrl)
        {
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(paymentStage);
                    await context.SaveChangesAsync();
                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(paymentStage));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = paymentStage.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Etapa Certificado";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
            {
                ViewBag.OrderValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Order");
            }

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            return View(paymentStage);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: PaymentStage/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get PaymentStage
            if ((id == null) || (id <= 0) || (context.PaymentStage == null)) return NotFound();
            PaymentStage? paymentStage = await context.PaymentStage
                .FindAsync(id);
            if (paymentStage == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, returnUrl, bufferedUrl);

            return View(paymentStage);
        }


        // POST: PaymentStage/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] PaymentStage paymentStage, string? returnUrl, string? bufferedUrl)
        {
            if (id != paymentStage.Id)
            {
                return NotFound();
            }
            // Check if is name unique
            if (context.PaymentStage.Any(c => c.Name == paymentStage.Name && c.Id != paymentStage.Id))
            {
                ModelState.AddModelError("Name", "Nombre existe en otro registro.");
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(paymentStage);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(paymentStage));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!PaymentStageExists(paymentStage.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Etapa Certificado. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Etapa Certificado";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
            {
                ViewBag.OrderValidationMsg = jaosLibUtils.getMandatoryNumbersErrorMessages(ModelState, "Order");
            }

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, returnUrl, bufferedUrl);

            return View(paymentStage);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: PaymentStage/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.PaymentStage == null)) return NotFound();
            PaymentStage? paymentStage = await context.PaymentStage
                .FirstAsync(r => r.Id == id);
            
            if (paymentStage == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            bool isLinked = await findExistingLinks(paymentStage);
            if (paymentStage != null && !isLinked)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.PaymentStage.Remove(paymentStage);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(paymentStage));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Etapa Certificado";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }

            //---- if not saved reload view ----
            // set ViewBags
            setStandardViewBags("Delete", false, returnUrl, bufferedUrl);
            jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
            return Redirect(HttpContext.Request.Headers["Referer"]);
        }



        // Finds all the registers that are using the current registry from Etapa Certificado
        async Task<bool> findExistingLinks(PaymentStage paymentStage)
        {
            bool isLinked = false;
            string externalLinks = string.Empty;

            //search for Certificados using this Etapa Certificado
            List<Payment>             payments = await context.Payment
                .Include(p => p.Contract_info)
                .Include(p => p.Type_info)
                .Include(p => p.Stage_info)
                .Where(r => r.Stage == paymentStage.Id).ToListAsync();
            if (payments?.Count > 0)
            {
                if (!isLinked) isLinked = true;
                externalLinks += "Se usa en  Certificados:<br/>";
                foreach (Payment payment1 in payments)
                    externalLinks += payment1?.Code + " - " + ((payment1?.DateDelivery.HasValue == true) ? payment1?.DateDelivery.Value.ToString("yyyy-MMM-dd") : "") + "<br/>";
                externalLinks += "<br/>";
            }

            if (isLinked) externalLinks = "Etapa Certificado no puede borrarse<br/>" + externalLinks;
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
        IQueryable<PaymentStage> orderBySelectedOrDefault(string sortOrder, IQueryable<PaymentStage> paymentStages)
        {
            ViewBag.nameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.nameIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "name_desc":
                    paymentStages = paymentStages.OrderByDescending(t => t.Order);
                    ViewBag.nameIcon = "bi-caret-up-fill";
                    break;
                default:
                    paymentStages = paymentStages.OrderBy(t => t.Order);
                    ViewBag.nameIcon = "bi-caret-down-fill";
                    break;
            }
            return paymentStages;
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
            // navPaymentStage
            ViewBag.navPaymentStage = $"{action}";
        }


        //----------------------------------------------
        //==============================================
        #endregion


        //========================================================

        private bool PaymentStageExists(int id)
        {
            return (context.PaymentStage?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
