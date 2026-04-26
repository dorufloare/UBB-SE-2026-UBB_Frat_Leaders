using matchmaking.Models;
using System.Collections.Generic;

namespace matchmaking.Services
{
    public interface IUserStatusService
    {
        IReadOnlyList<ApplicationCardModel> GetApplicationsForUser(int userId);
    }
}