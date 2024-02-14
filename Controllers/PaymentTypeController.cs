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
    public partial class PaymentTypeController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly ILogger<PaymentTypeController> logger;


        public PaymentTypeController(PIPMUNI_ARGDbContext context
        , ILogger<PaymentTypeController> logger
        )
        {
            this.context = context;
            this.logger = logger;
        }

        JaosLibUtils jaosLibUtils = new JaosLibUtils();

        #region Index


        //----------- Index

        // GET: PaymentType/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, string? returnUrl, string? bufferedUrl)
        {
            var paymentTypes = from o in context.PaymentType select o;
            if (!string.IsNullOrEmpty(searchText))
                paymentTypes = paymentTypes.Where(p => p.Name!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (paymentTypes != null)
            {
                paymentTypes = orderBySelectedOrDefault(sortOrder, paymentTypes);
                return View(await paymentTypes.ToListAsync());
            }
            return View(new List<PaymentType>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: PaymentType/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get PaymentType
            if ((id == null) || (id <= 0) || (context.PaymentType == null)) return NotFound();
            PaymentType? paymentType = await context.PaymentType
                .FindAsync(id);
            if (paymentType == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, returnUrl, bufferedUrl);

            return View(paymentType);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: PaymentType/Create
        [HttpGet]
        public IActionResult Create(string? returnUrl, string? bufferedUrl)
        {
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);

            return View();
        }



        // POST: PaymentType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] PaymentType paymentType, string? returnUrl, string? bufferedUrl)
        {
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(paymentType);
                    await context.SaveChangesAsync();
                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(paymentType));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = paymentType.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Tipo de Certificado";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            return View(paymentType);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: PaymentType/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get PaymentType
            if ((id == null) || (id <= 0) || (context.PaymentType == null)) return NotFound();
            PaymentType? paymentType = await context.PaymentType
                .FindAsync(id);
            if (paymentType == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, returnUrl, bufferedUrl);

            return View(paymentType);
        }


        // POST: PaymentType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] PaymentType paymentType, string? returnUrl, string? bufferedUrl)
        {
            if (id != paymentType.Id)
            {
                return NotFound();
            }
            // Check if is name unique
            if (context.PaymentType.Any(c => c.Name == paymentType.Name && c.Id != paymentType.Id))
            {
                ModelState.AddModelError("Name", "Nombre existe en otro registro.");
            }
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(paymentType);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(paymentType));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!PaymentTypeExists(paymentType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Tipo de Certificado. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Tipo de Certificado";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, returnUrl, bufferedUrl);

            return View(paymentType);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: PaymentType/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.PaymentType == null)) return NotFound();
            PaymentType? paymentType = await context.PaymentType
                .FirstAsync(r => r.Id == id);
            
            if (paymentType == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            bool isLinked = await findExistingLinks(paymentType);
            if (paymentType != null && !isLinked)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.PaymentType.Remove(paymentType);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(paymentType));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Tipo de Certificado";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }

            //---- if not saved reload view ----
            // set ViewBags
            setStandardViewBags("Delete", false, returnUrl, bufferedUrl);
            jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
            return Redirect(HttpContext.Request.Headers["Referer"]);
        }



        // Finds all the registers that are using the current registry from Tipo de Certificado
        async Task<bool> findExistingLinks(PaymentType paymentType)
        {
            bool isLinked = false;
            string externalLinks = string.Empty;

            //search for Certificados using this Tipo de Certificado
            List<Payment>             payments = await context.Payment
                .Include(p => p.Contract_info)
                .Include(p => p.Type_info)
                .Include(p => p.Stage_info)
                .Where(r => r.Type == paymentType.Id).ToListAsync();
            if (payments?.Count > 0)
            {
                if (!isLinked) isLinked = true;
                externalLinks += "Se usa en  Certificados:<br/>";
                foreach (Payment payment1 in payments)
                    externalLinks += payment1?.Code + " - " + ((payment1?.DateDelivery.HasValue == true) ? payment1?.DateDelivery.Value.ToString("yyyy-MMM-dd") : "") + "<br/>";
                externalLinks += "<br/>";
            }

            if (isLinked) externalLinks = "Tipo de Certificado no puede borrarse<br/>" + externalLinks;
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
        IQueryable<PaymentType> orderBySelectedOrDefault(string sortOrder, IQueryable<PaymentType> paymentTypes)
        {
            ViewBag.nameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.nameIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "name_desc":
                    paymentTypes = paymentTypes.OrderByDescending(t => t.Name);
                    ViewBag.nameIcon = "bi-caret-up-fill";
                    break;
                default:
                    paymentTypes = paymentTypes.OrderBy(t => t.Name);
                    ViewBag.nameIcon = "bi-caret-down-fill";
                    break;
            }
            return paymentTypes;
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
            // navPaymentType
            ViewBag.navPaymentType = $"{action}";
        }


        //----------------------------------------------
        //==============================================
        #endregion


        //========================================================

        private bool PaymentTypeExists(int id)
        {
            return (context.PaymentType?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
