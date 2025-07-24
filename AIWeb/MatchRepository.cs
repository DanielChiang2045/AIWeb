namespace AIWeb;

public interface IMatchRepository
{
    Match? GetMatch(int matchId);
    Match GetExistingMatch(int matchId);
    void UpdateMatchResult(int matchId, string newResult);
    void Reset(); // 供測試使用
}

public class MatchRepository : IMatchRepository
{
    private static Dictionary<int, Match> _matches = new()
    {
        { 91, new Match { MatchId = 91, MatchResult = "" } }
    };

    public Match? GetMatch(int matchId)
    {
        return _matches.TryGetValue(matchId, out var match) ? match : null;
    }

    public Match GetExistingMatch(int matchId)
    {
        if (!_matches.ContainsKey(matchId))
        {
            throw new NullReferenceException($"Match with ID {matchId} not found");
        }
        return _matches[matchId];
    }

    public void UpdateMatchResult(int matchId, string newResult)
    {
        var match = GetExistingMatch(matchId);
        match.MatchResult = newResult;
    }

    // 供測試使用的重置方法
    public void Reset()
    {
        _matches = new Dictionary<int, Match>
        {
            { 91, new Match { MatchId = 91, MatchResult = "" } }
        };
    }
} 