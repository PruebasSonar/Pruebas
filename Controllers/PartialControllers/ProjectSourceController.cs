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
using Microsoft.Data.SqlClient;
using PIPMUNI_ARG.Models.Reports;
using System.ComponentModel.DataAnnotations;
using Dapper;
using Microsoft.AspNetCore.Identity;
using PIPMUNI_ARG.Migrations;

namespace PIPMUNI_ARG.Controllers
{
    public partial class ProjectSourceController : Controller
    {


        public double projectTotalSourcePercentage(ProjectSource projectSource)
        {
            double total = 0;
            SqlConnection sqlConnection = new SqlConnection(context.Database.GetConnectionString());
            try
            {
                sqlConnection.Open();
                var result = sqlConnection.Query<AdvanceBasic>($"select * from project_fundingTotal({projectSource.Project},{projectSource.Id})").FirstOrDefault();
                if (result != null && result.programmed.HasValue)
                    if (projectSource.Percentage.HasValue)
                        total = result.programmed.Value + projectSource.Percentage.Value;
                    else
                        total = result.programmed.Value;
            }
            catch
            {
                throw;
            }
            finally
            {
                sqlConnection.Close();
            }
            return total;
        }






    }
}
