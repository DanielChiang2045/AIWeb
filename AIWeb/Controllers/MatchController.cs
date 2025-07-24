using Microsoft.AspNetCore.Mvc;

namespace AIWeb.Controllers;

[ApiController]
[Route("[controller]")]
public class MatchController : ControllerBase
{
    private readonly ILogger<MatchController> _logger;

    public MatchController(ILogger<MatchController> logger)
    {
        _logger = logger;
    }

    [HttpPost("UpdateMatchResult")]
    public string UpdateMatchResult(int matchId, int matchEvent)
    {
        // 取得比賽
        var match = MatchResultDatabase.CreateOrGetMatch(matchId);
        var originalResult = match.MatchResult;
        
        // 處理取消事件
        if (matchEvent == (int)MatchEvent.HomeCancel || matchEvent == (int)MatchEvent.AwayCancel)
        {
            return ProcessCancelEvent(matchId, matchEvent, match, originalResult);
        }
        
        // 處理NextPeriod事件 - 只允許一個分號
        if (matchEvent == (int)MatchEvent.NextPeriod)
        {
            // 如果已經有分號，就不處理
            if (originalResult.Contains(';'))
            {
                return match.MatchResult;
            }
            
            match.MatchResult += ";";
            return match.MatchResult;
        }
        
        // 根據MatchEvent更新結果
        var eventChar = matchEvent switch
        {
            (int)MatchEvent.HomeGoal => "H",
            (int)MatchEvent.AwayGoal => "A",
            _ => ""
        };

        // 更新比賽結果
        match.MatchResult += eventChar;
        
        return match.MatchResult;
    }

    private string ProcessCancelEvent(int matchId, int matchEvent, Match match, string originalResult)
    {
        var targetChar = matchEvent == (int)MatchEvent.HomeCancel ? 'H' : 'A';
        var cancelType = matchEvent == (int)MatchEvent.HomeCancel ? "HomeCancel" : "AwayCancel";
        
        // 找到最後一個非分號字符的位置
        int lastCharIndex = -1;
        for (int i = originalResult.Length - 1; i >= 0; i--)
        {
            if (originalResult[i] != ';')
            {
                lastCharIndex = i;
                break;
            }
        }
        
        // 如果沒有找到任何非分號字符，或者最後一個字符不匹配
        if (lastCharIndex == -1 || originalResult[lastCharIndex] != targetChar)
        {
            throw new UpdateMatchResultException(
                matchId, 
                originalResult, 
                $"Cannot perform {cancelType} on match {matchId}. Expected last character to be '{targetChar}' but found '{(lastCharIndex >= 0 ? originalResult[lastCharIndex] : "none")}'."
            );
        }
        
        // 移除最後一個匹配的字符
        match.MatchResult = originalResult.Remove(lastCharIndex, 1);
        
        return match.MatchResult;
    }

    [HttpGet("DisplayMatchResult")]
    public string DisplayMatchResult(int matchId)
    {
        var match = MatchResultDatabase.GetMatch(matchId);
        var matchResult = match?.MatchResult ?? "";
        
        if (string.IsNullOrEmpty(matchResult))
        {
            return "0 : 0 (First Half)";
        }
        
        return ConvertToScore(matchResult);
    }
    
    private string ConvertToScore(string matchResult)
    {
        // 用分號分割來確定當前時段
        var periods = matchResult.Split(';');
        var currentPeriod = periods.Length;
        
        // 計算總得分
        var homeScore = 0;
        var awayScore = 0;
        
        foreach (var period in periods)
        {
            foreach (var eventChar in period)
            {
                if (eventChar == 'H') homeScore++;
                else if (eventChar == 'A') awayScore++;
            }
        }
        
        // 確定時段名稱
        var periodName = currentPeriod switch
        {
            1 => "First Half",
            2 => "Second Half",
            3 => "Third Period",
            4 => "Fourth Period",
            _ => $"Period {currentPeriod}"
        };
        
        return $"{homeScore} : {awayScore} ({periodName})";
    }
} 