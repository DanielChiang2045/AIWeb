namespace AIWeb;

public class Match
{
    public int MatchId { get; set; }
    public string MatchResult { get; set; } = "";
}

public static class MatchResultDatabase
{
    private static readonly Dictionary<int, Match> _matches = new()
    {
        { 91, new Match { MatchId = 91, MatchResult = "" } }
    };

    public static Match? GetMatch(int matchId)
    {
        return _matches.TryGetValue(matchId, out var match) ? match : null;
    }

    public static Match CreateOrGetMatch(int matchId)
    {
        if (!_matches.ContainsKey(matchId))
        {
            _matches[matchId] = new Match { MatchId = matchId, MatchResult = "" };
        }
        return _matches[matchId];
    }

    public static void UpdateMatchResult(int matchId, string newResult)
    {
        var match = CreateOrGetMatch(matchId);
        match.MatchResult = newResult;
    }
} 