using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.Repositories;

public class SqlMatchRepository(string connectionString) : SqlRepositoryBase(connectionString)
{
    public Match? GetById(int matchId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT MatchId, UserId, JobId, Status, Timestamp, FeedbackMessage FROM Match WHERE MatchId = @MatchId",
            connection);
        command.Parameters.AddWithValue("@MatchId", matchId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public IReadOnlyList<Match> GetAll()
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT MatchId, UserId, JobId, Status, Timestamp, FeedbackMessage FROM Match",
            connection);
        using var reader = command.ExecuteReader();

        var result = new List<Match>();
        while (reader.Read())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public void Add(Match match)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "INSERT INTO Match (MatchId, UserId, JobId, Status, Timestamp, FeedbackMessage) VALUES (@MatchId, @UserId, @JobId, @Status, @Timestamp, @FeedbackMessage)",
            connection);
        command.Parameters.AddWithValue("@MatchId", match.MatchId);
        command.Parameters.AddWithValue("@UserId", match.UserId);
        command.Parameters.AddWithValue("@JobId", match.JobId);
        command.Parameters.AddWithValue("@Status", (int)match.Status);
        command.Parameters.AddWithValue("@Timestamp", match.Timestamp);
        command.Parameters.AddWithValue("@FeedbackMessage", match.FeedbackMessage);
        command.ExecuteNonQuery();
    }

    public void Update(Match match)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "UPDATE Match SET UserId = @UserId, JobId = @JobId, Status = @Status, Timestamp = @Timestamp, FeedbackMessage = @FeedbackMessage WHERE MatchId = @MatchId",
            connection);
        command.Parameters.AddWithValue("@MatchId", match.MatchId);
        command.Parameters.AddWithValue("@UserId", match.UserId);
        command.Parameters.AddWithValue("@JobId", match.JobId);
        command.Parameters.AddWithValue("@Status", (int)match.Status);
        command.Parameters.AddWithValue("@Timestamp", match.Timestamp);
        command.Parameters.AddWithValue("@FeedbackMessage", match.FeedbackMessage);
        command.ExecuteNonQuery();
    }

    public void Remove(int matchId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "DELETE FROM Match WHERE MatchId = @MatchId",
            connection);
        command.Parameters.AddWithValue("@MatchId", matchId);
        command.ExecuteNonQuery();
    }

    private static Match Map(SqlDataReader reader)
    {
        return new Match
        {
            MatchId = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            JobId = reader.GetInt32(2),
            Status = (MatchStatus)reader.GetInt32(3),
            Timestamp = reader.GetDateTime(4),
            FeedbackMessage = reader.GetString(5)
        };
    }
}
