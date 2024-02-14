using PIPMUNI_ARG.Models.Domain;
using System.Security.Claims;

namespace PIPMUNI_ARG.Services.basic
{
    public partial interface IParentContractService
    {
        /// <summary>
        /// Get contract information from Session variable
        /// </summary>
        /// <param name="session">The HttpContext.Session where data is stored.</param>
        /// <param name="ViewBag">The ViewBag where information will be replicated.</param>
        /// <returns>A Contract model with the information, the same information is included in ViewBag.ParentContract.</returns>
        Contract? getSessionContract(ISession session, dynamic ViewBag);

        /// <summary>
        /// Gets the contract id from the session variable.
        /// </summary>
        /// <param name="session">The HttpContext.Session where data is stored.</param>
        /// <returns>The contract id</returns>
        int? getSessionContractId(ISession session);

        /// <summary>
        /// If the received id has a value, returns the contract with that id and updates the session variable. 
        /// If the id does not have a values, returns the contract available in the Session variable.
        /// </summary>
        /// <param name="id">The Contract id</param>
        /// <param name="user">The user performing the operation, will be used to control access to the requested information.</param>
        /// <param name="session">The HttpContext.Session where data is stored.</param>
        /// <param name="ViewBag">The ViewBag where information will be replicated.</param>
        /// <returns>A Contract Model with the information.</returns>
        Task<Contract?> getContractFromIdOrSession(int? id, ClaimsPrincipal user, ISession session, dynamic ViewBag);

        /// <summary>
        /// Retrieves the information for the contract with the received id.
        /// </summary>
        /// <param name="id">id for the contract to be retrieved</param>
        /// <param name="user">The user performing the operation, will be used to control access to the requested information.</param>
        /// <param name="session">The HttpContext.Session where data is stored.</param>
        /// <returns>The Contract with the received id.</returns>
        Task<Contract?> getContractFromId(int? id, ClaimsPrincipal user, ISession session);

        /// <summary>
        /// Sets the contract session information. contract: contains the active Contract information and contractId: contains the id for the active contract.
        /// </summary>
        /// <param name="contract">The contract to be set as active.</param>
        /// <param name="session">The HttpContext.Session where data is stored.</param>
        void setSessionContract(Contract? contract, ISession session);
    }
}
