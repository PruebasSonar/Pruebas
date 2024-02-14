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
    public partial class UserProfileController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;
        private readonly ILogger<UserProfileController> logger;


        public UserProfileController(PIPMUNI_ARGDbContext context
        , ILogger<UserProfileController> logger
        )
        {
            this.context = context;
            this.logger = logger;
        }

        JaosLibUtils jaosLibUtils = new JaosLibUtils();

        #region Index


        //----------- Index

        // GET: UserProfile/Index/
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchText, string? returnUrl, string? bufferedUrl)
        {
            var userProfiles = from o in context.UserProfile select o;
            if (!string.IsNullOrEmpty(searchText))
                userProfiles = userProfiles.Where(p => p.Email!.Contains(searchText) || p.Name!.Contains(searchText) || p.Surname!.Contains(searchText));

            // set ViewBags
            setStandardViewBags("Index", false, returnUrl, bufferedUrl);
            ViewBag.searchText = searchText;

            if (userProfiles != null)
            {
                userProfiles = orderBySelectedOrDefault(sortOrder, userProfiles);
                return View(await userProfiles.ToListAsync());
            }
            return View(new List<UserProfile>());
        }


        #endregion
        #region Display


        //----------- Display

        // GET: UserProfile/Display/5
        [HttpGet]
        public async Task<IActionResult> Display(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get UserProfile
            if ((id == null) || (id <= 0) || (context.UserProfile == null)) return NotFound();
            UserProfile? userProfile = await context.UserProfile
                .FindAsync(id);
            if (userProfile == null) return NotFound();

            // set ViewBags
            setStandardViewBags("Display", true, returnUrl, bufferedUrl);
            setViewBagsForLists(userProfile);

            return View(userProfile);
        }


        #endregion
        #region Create


        //----------- Create

        // GET: UserProfile/Create
        [HttpGet]
        public IActionResult Create(string? returnUrl, string? bufferedUrl)
        {
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            setViewBagsForLists(null);

            return View();
        }



        // POST: UserProfile/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] UserProfile userProfile, string? returnUrl, string? bufferedUrl)
        {
            unselectedLinksNulled(userProfile);
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    // Insert
                    context.Add(userProfile);
                    await context.SaveChangesAsync();
                    // Commit
                    logger.LogWarning(JsonConvert.SerializeObject(userProfile));
                    transaction.Commit();
                    // Goto Edit
                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return RedirectToAction(nameof(Edit), new { id = userProfile.Id, returnUrl = returnUrl, bufferedUrl = bufferedUrl });
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error creando Perfil de Usuario";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Create", true, returnUrl, bufferedUrl);
            setViewBagsForLists(userProfile);
            return View(userProfile);
        }
        #endregion
        #region Edit


        //----------- Edit

        // GET: UserProfile/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string? returnUrl, string? bufferedUrl)
        {
            // get UserProfile
            if ((id == null) || (id <= 0) || (context.UserProfile == null)) return NotFound();
            UserProfile? userProfile = await context.UserProfile
                .FindAsync(id);
            if (userProfile == null) return NotFound();


            jaosLibUtils.receiveMessagesFromTempData(TempData, ViewBag);
            // set ViewBags
            setStandardViewBags("Edit", true, returnUrl, bufferedUrl);
            setViewBagsForLists(userProfile);

            return View(userProfile);
        }


        // POST: UserProfile/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [FromForm] UserProfile userProfile, string? returnUrl, string? bufferedUrl)
        {
            if (id != userProfile.Id)
            {
                return NotFound();
            }
            unselectedLinksNulled(userProfile);
            if (ModelState.IsValid)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.Update(userProfile);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(userProfile));
                    transaction.Commit();
                    ViewBag.okMessage = "La información ha sido actualizada.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    if (!UserProfileExists(userProfile.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ViewBag.errorMessage = "Concurrency Error Actualizando Perfil de Usuario. Por favor intente nuevamente..";
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error Actualizando Perfil de Usuario";
                    ViewBag.debugErrorMessage = ex.Message + ex.InnerException?.Message;
                }
            }
            else
                ViewBag.warningMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            //---- if not saved reload View ----
            // set ViewBags
            setStandardViewBags("Edit", false, returnUrl, bufferedUrl);
            setViewBagsForLists(userProfile);

            return View(userProfile);
        }
        #endregion
        #region Delete


        //----------- Delete

        // POST: UserProfile/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id, string? returnUrl, string? bufferedUrl)
        {
            if ((id == null) || (id <= 0) || (context.UserProfile == null)) return NotFound();
            UserProfile? userProfile = await context.UserProfile
                .FirstAsync(r => r.Id == id);
            
            if (userProfile == null) return NotFound();

            if (string.IsNullOrEmpty(returnUrl)) returnUrl = HttpContext.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(returnUrl)) return NotFound();

            if (userProfile != null)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    transaction.CreateSavepoint("startingPoint");
                    context.UserProfile.Remove(userProfile);
                    await context.SaveChangesAsync();
                    logger.LogWarning(JsonConvert.SerializeObject(userProfile));
                    transaction.Commit();

                    ViewBag.okMessage = "La información ha sido actualizada.";
                    jaosLibUtils.sendMessagesOnTempData(TempData, ViewBag);
                    return Redirect(returnUrl);
                }
                catch (Exception ex)
                {
                    transaction.RollbackToSavepoint("startingPoint");
                    ViewBag.errorMessage = "Error borrando Perfil de Usuario";
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


        //----------------------------------------------
        //==============================================
        //----------------------------------------------
        #endregion
        #region Supporting Methods

         //-- Sort Table by default or user selected order
        IQueryable<UserProfile> orderBySelectedOrDefault(string sortOrder, IQueryable<UserProfile> userProfiles)
        {
            ViewBag.emailSort = String.IsNullOrEmpty(sortOrder) ? "email_desc" : "";
            ViewBag.nameSort = sortOrder == "name" ? "name_desc" : "name";
            ViewBag.surnameSort = sortOrder == "surname" ? "surname_desc" : "surname";
            ViewBag.emailIcon = "bi-caret-down";
            ViewBag.nameIcon = "bi-caret-down";
            ViewBag.surnameIcon = "bi-caret-down";
            switch (sortOrder)
            {
                case "name_desc":
                    userProfiles = userProfiles.OrderByDescending(o => o.Name);
                    ViewBag.nameIcon = "bi-caret-up-fill";
                    break;
                case "name":
                    userProfiles = userProfiles.OrderBy(o => o.Name);
                    ViewBag.nameIcon = "bi-caret-down-fill";
                    break;
                case "surname_desc":
                    userProfiles = userProfiles.OrderByDescending(o => o.Surname);
                    ViewBag.surnameIcon = "bi-caret-up-fill";
                    break;
                case "surname":
                    userProfiles = userProfiles.OrderBy(o => o.Surname);
                    ViewBag.surnameIcon = "bi-caret-down-fill";
                    break;
                case "email_desc":
                    userProfiles = userProfiles.OrderByDescending(t => t.Email);
                    ViewBag.emailIcon = "bi-caret-up-fill";
                    break;
                default:
                    userProfiles = userProfiles.OrderBy(t => t.Email);
                    ViewBag.emailIcon = "bi-caret-down-fill";
                    break;
            }
            return userProfiles;
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
            // navUserProfile
            ViewBag.navUserProfile = $"{action}";
        }


        void setViewBagsForLists(UserProfile? userProfile)
        {

            // set options for Office
            var listOffice = new SelectList(context.Office.OrderBy(c => c.Name).Select(r => new SelectListItem(r.Name, r.Id.ToString())), "Value", "Text", userProfile?.Office).ToList();
            listOffice.Insert(0, new SelectListItem { Value = "0", Text = "Seleccionar una opción" });
            ViewBag.listOffice = listOffice;
        }


        void unselectedLinksNulled(UserProfile userProfile)
        {
            // sets unselected lists to null
            if (userProfile.Office == 0) userProfile.Office = null;

        }

        //----------------------------------------------
        //==============================================
        #endregion


        //========================================================

        private bool UserProfileExists(int id)
        {
            return (context.UserProfile?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
