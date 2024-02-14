using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIPMUNI_ARG.Areas.Review.Models.Approval;
using PIPMUNI_ARG.Data;
using PIPMUNI_ARG.Models.Domain;

namespace PIPMUNI_ARG.Areas.Review.Controllers
{
    [Authorize]
    [Area("Review")]
    public class ApprovalController : Controller
    {
        private readonly PIPMUNI_ARGDbContext context;

        public ApprovalController(PIPMUNI_ARGDbContext context
            )
        {
            this.context = context;
        }

        List<int> projectStages = new List<int> { 1, 2 };  // 1 iniciar 2 ejecucion 3 finalizada 4 rescindida
        List<int> contractStages = new List<int> { 1 }; // 1 iniciar 2 ejecucion 3 finalizada 4 rescindida 
        List<int> paymentStages = new List<int> { 1, 2, 3, 4 };  // 1 iniciado 2 tecnica 3 contable 4 devengado 5 pagado
        List<int> additionStages = new List<int> { 1, 2 };  // 1 a iniciar 2 en proceso 3 aprobada

        // GET: Approval/Payments
        public IActionResult Payments()
        {
            if (!User.IsInRole(ProjectGlobals.RoleDireccion) 
                && !User.IsInRole(ProjectGlobals.RoleAdmin))
                return NotFound();

            return View(fillPayments());
        }

        // GET: Approval/Additions
        public IActionResult Additions()
        {
            if (!User.IsInRole(ProjectGlobals.RoleDireccion)
                && !User.IsInRole(ProjectGlobals.RoleAdmin))
                return NotFound();

            return View(fillAdditions());
        }

        // GET: Approval/Variantes
        public IActionResult Variantes()
        {
            if (!User.IsInRole(ProjectGlobals.RoleDireccion)
                && !User.IsInRole(ProjectGlobals.RoleAdmin))
                return NotFound();

            return View(fillVariantes());
        }

        // GET: Approval/Extensions
        public IActionResult Extensions()
        {
            if (!User.IsInRole(ProjectGlobals.RoleDireccion)
                && !User.IsInRole(ProjectGlobals.RoleAdmin))
                return NotFound();

            return View(fillExtensions());
        }


        //==================================================================
        //
        //    Fill Data for Views

        List<Payment> fillPayments()
        {
            List<Payment> payments = context.Payment
                .Include(p => p.Contract_info)
                .Include(p => p.Stage_info)
                .Where(p => paymentStages.Contains(p.Stage)).ToList();
            return payments;
        }

        List<Addition> fillAdditions()
        {
            List<Addition> additions = context.Addition
                .Include(p => p.Contract_info)
                .Include(p => p.Stage_info)
                .Where(p => additionStages.Contains(p.Stage)).ToList();
            return additions;
        }

        List<Variante> fillVariantes()
        {
            List<Variante> variantes = context.Variante
                .Include(p => p.Contract_info)
                .Include(p => p.Stage_info)
                .Where(p => additionStages.Contains(p.Stage)).ToList();
            return variantes;
        }

        List<Extension> fillExtensions()
        {
            List<Extension> extensions = context.Extension
                .Include(p => p.Contract_info)
                .Include(p => p.Stage_info)
                .Where(p => additionStages.Contains(p.Stage)).ToList();
            return extensions;
        }


        List<GeneralApprovalModel> fillPendingSummary()
        {
            List<GeneralApprovalModel> generalApprovalModels = new List<GeneralApprovalModel>();

            return generalApprovalModels;

            // SQL generalApprovalModels

//            select 'Certificados' as title
//, count(id) as qty
//from Payment
//where Payment.Stage < 5
//union
//select 'Redeterminaciones' as title
//, count(id) as qty
//from Addition
//where Addition.Stage < 3
//union
//select 'Variantes' as title
//, count(id) as qty
//from Variante
//where Variante.Stage < 3
//union
//select 'Extensiones de plazos' as title
//, count(id) as qty
//from Extension
//where Extension.Stage < 3

        }

    }
}
