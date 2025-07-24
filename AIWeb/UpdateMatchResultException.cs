namespace AIWeb;

public class UpdateMatchResultException : Exception
{
    public int MatchId { get; }
    public string OriginalMatchResult { get; }

    public UpdateMatchResultException(int matchId, string originalMatchResult, string message) 
        : base(message)
    {
        MatchId = matchId;
        OriginalMatchResult = originalMatchResult;
    }

    public UpdateMatchResultException(int matchId, string originalMatchResult, string message, Exception innerException) 
        : base(message, innerException)
    {
        MatchId = matchId;
        OriginalMatchResult = originalMatchResult;
    }
} 