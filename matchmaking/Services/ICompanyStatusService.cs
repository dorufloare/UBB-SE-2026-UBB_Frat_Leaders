using matchmaking.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace matchmaking.Services
{
    public interface ICompanyStatusService
    {
        Task<UserApplicationResult?> GetApplicantByMatchIdAsync(int companyId, int matchId);
        Task<IReadOnlyList<UserApplicationResult>> GetApplicantsForCompanyAsync(int companyId);
    }
}