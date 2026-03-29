using System.Collections.Generic;
using System.Threading.Tasks;
using matchmaking.DTOs;

namespace matchmaking.Services;

public interface ITestingModuleAdapter
{
    Task<TestResult?> GetResultForMatch(int matchId);

    Task<TestResult?> GetLatestResultForCandidate(int externalUserId, int positionId);

    Task<IReadOnlyList<TestResult>> GetResultHistoryForCandidate(int externalUserId, int positionId);
}
