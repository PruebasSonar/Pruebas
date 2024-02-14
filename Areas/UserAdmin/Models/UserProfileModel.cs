using System.ComponentModel.DataAnnotations;
using PIPMUNI_ARG.Models.Domain;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using NPOI.SS.Formula.PTG;
using PIPMUNI_ARG.Areas.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace JaosLib.Areas.UserAdmin.Models
{
    public class UserProfileModel : UserProfile
    {
        public int? role { get; set; }
    }
}

