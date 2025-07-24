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
        // 取得或建立比賽
        var match = MatchResultDatabase.CreateOrGetMatch(matchId);
        
        // 根據MatchEvent更新結果
        var eventChar = matchEvent switch
        {
            (int)MatchEvent.HomeGoal => "H",
            (int)MatchEvent.AwayGoal => "A",
            (int)MatchEvent.NextPeriod => ";",
            (int)MatchEvent.HomeCancel => "",  // 暫時不處理取消事件
            (int)MatchEvent.AwayCancel => "",  // 暫時不處理取消事件
            _ => ""
        };

        // 更新比賽結果
        match.MatchResult += eventChar;
        
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