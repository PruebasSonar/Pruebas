using System.Security.Claims;
using System.Text.Json;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PIPMUNI_ARG.Areas.Identity.Models;
using PIPMUNI_ARG.Data;
using PIPMUNI_ARG.Models.Domain;

namespace PIPMUNI_ARG.Services.basic
{
    public partial class ParentContractService : IParentContractService
    {
        PIPMUNI_ARGDbContext context;
        public ParentContractService(PIPMUNI_ARGDbContext context)
        {
            this.context = context;
        }

        public Contract? getSessionContract(ISession session, dynamic ViewBag)
        {
            string? textContract = session.GetString("contract");
            Contract? contract = (!string.IsNullOrEmpty(textContract)) ? JsonSerializer.Deserialize<Contract > (textContract) : null;
            if ((contract != null) && !(contract?.Id > 0))
                contract = null;
            if (ViewBag != null)
                ViewBag.ParentContract = contract;
            return contract;
        }
        
        public int? getSessionContractId(ISession session)
        {
            int id;
            if (int.TryParse(session.GetString("contractId"), out id)) 
                return id;
            return null;
        }
        public async Task<Contract?> getContractFromIdOrSession(int? id, ClaimsPrincipal user, ISession session, dynamic ViewBag)
        {
            // use the id from parameter or from session variable.
            Contract? contract = null;
            if (id != null) // load info for id
            {
                contract = await getContractFromId(id, user, session);
                ViewBag.ParentContract = contract;
            }
            else // retrieve from session variable
            {
                contract = getSessionContract(session, ViewBag);
            }
            return contract;
        }

                public async Task<Contract?> getContractFromId(int? id, ClaimsPrincipal user, ISession session)
                {
                    Contract? contract = null;
                    if (id != null) // load info for id
                    {
                        if (context.Contract != null)
                        {
                            contract = await context.Contract.FindAsync(id);
                            if (contract == null)
                                return null;
                        }
                    }
                    setSessionContract(contract, session);
                    return contract;
                }
                public void setSessionContract(Contract? contract, ISession session)
                {
                    if (contract != null)
                    {
                            session.SetString("contract", JsonSerializer.Serialize(contract));
                            session.SetString("contractId", contract.Id.ToString());
                    }
                    else
                    {
                        session.SetString("contract", "");
                        session.SetString("contractId", "");
                    }
                }
    }
}
