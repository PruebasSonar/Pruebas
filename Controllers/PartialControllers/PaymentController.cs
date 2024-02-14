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
using Microsoft.CodeAnalysis;

namespace PIPMUNI_ARG.Controllers
{
    public partial class PaymentController : Controller
    {


        public AdvanceBasic? paymentsGreaterThanProgrammed(Payment payment)
        {
            AdvanceBasic? advance = null;
            SqlConnection sqlConnection = new SqlConnection(context.Database.GetConnectionString());
            JaosDataTools jaosDataTools = new JaosDataTools();
            try
            {
                sqlConnection.Open();
                var parameters = new[]
                {
                    new SqlParameter("@contractId", payment.Contract),
                    new SqlParameter("@paymentId", payment.Id > 0 ? payment.Id : (object)DBNull.Value) // Convert null to DBNull.Value
                };

                advance = sqlConnection.Query<AdvanceBasic>($"select * from [contract_ProgrammedAndPayments]({payment.Contract},{payment.Id})").FirstOrDefault();
                if (advance == null)
                    advance=new AdvanceBasic();
                advance.actual = jaosDataTools.add(advance.actual, payment.Total);
                if (advance.actual.HasValue && advance.actual.Value > 0)
                {
                    if (advance.programmed.HasValue)
                    {
                        // payments are greater than programmed
                        if (advance.programmed.Value < advance.actual.Value)
                            advance.invalid = true;
                    }
                    else // payment has value and programmed has no value.
                        advance.invalid = true;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                sqlConnection.Close();
            }
            return advance;
        }






    }
}
