using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using AIWeb.Controllers;
using AIWeb;
using Assert = Xunit.Assert;

namespace AIWeb.Tests;

public class MatchControllerTests
{
    private readonly MatchController _controller;
    private readonly ILogger<MatchController> _logger;

    public MatchControllerTests()
    {
        _logger = Substitute.For<ILogger<MatchController>>();
        _controller = new MatchController(_logger);
        
        // 每次測試前清理靜態數據
        ClearStaticData();
    }

    private void ClearStaticData()
    {
        // 清理靜態數據，避免測試間相互影響
        var field = typeof(MatchResultDatabase).GetField("_matches", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (field != null)
        {
            var dict = new Dictionary<int, Match> { { 91, new Match { MatchId = 91, MatchResult = "" } } };
            field.SetValue(null, dict);
        }
    }
    [SetUp]
    public 

    [Fact]
    public void UpdateMatchResult_GivenEmptyMatch_WhenHomeGoalEvent_ThenShouldReturnH()
    {
        // Given
        int matchId = 91;
        int homeGoalEvent = (int)MatchEvent.HomeGoal;

        // When
        var result = _controller.UpdateMatchResult(matchId, homeGoalEvent);

        // Then
        Assert.Equal("H", result);
    }

    [Fact]
    public void UpdateMatchResult_GivenEmptyMatch_WhenAwayGoalEvent_ThenShouldReturnA()
    {
        // Given
        int matchId = 91;
        int awayGoalEvent = (int)MatchEvent.AwayGoal;

        // When
        var result = _controller.UpdateMatchResult(matchId, awayGoalEvent);

        // Then
        Assert.Equal("A", result);
    }

    [Fact]
    public void UpdateMatchResult_GivenEmptyMatch_WhenNextPeriodEvent_ThenShouldReturnSemicolon()
    {
        // Given
        int matchId = 91;
        int nextPeriodEvent = (int)MatchEvent.NextPeriod;

        // When
        var result = _controller.UpdateMatchResult(matchId, nextPeriodEvent);

        // Then
        Assert.Equal(";", result);
    }

    [Fact]
    public void UpdateMatchResult_GivenMatchWithSemicolon_WhenNextPeriodEvent_ThenShouldNotAddAnotherSemicolon()
    {
        // Given
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.NextPeriod); // 添加第一個分號
        
        // When
        var result = _controller.UpdateMatchResult(matchId, (int)MatchEvent.NextPeriod); // 嘗試添加第二個分號

        // Then
        Assert.Equal("H;", result); // 應該還是只有一個分號
    }

    [Fact]
    public void UpdateMatchResult_GivenEmptyMatch_WhenSequentialEvents_ThenShouldAccumulate()
    {
        // Given
        int matchId = 91;

        // When & Then
        var result1 = _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);
        Assert.Equal("H", result1);

        var result2 = _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal);
        Assert.Equal("HA", result2);

        var result3 = _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal);
        Assert.Equal("HAA", result3);

        var result4 = _controller.UpdateMatchResult(matchId, (int)MatchEvent.NextPeriod);
        Assert.Equal("HAA;", result4);

        var result5 = _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);
        Assert.Equal("HAA;H", result5);
    }

    // Cancel功能測試案例

    [Fact]
    public void UpdateMatchResult_GivenMatchResultHHA_WhenAwayCancelEvent_ThenShouldReturnHH()
    {
        // Given
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal); // H
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal); // HH
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal); // HHA

        // When
        var result = _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayCancel);

        // Then
        Assert.Equal("HH", result);
    }

    [Fact]
    public void UpdateMatchResult_GivenMatchResultHH_WhenHomeCancelEvent_ThenShouldReturnH()
    {
        // Given
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal); // H
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal); // HH

        // When
        var result = _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeCancel);

        // Then
        Assert.Equal("H", result);
    }

    [Fact]
    public void UpdateMatchResult_GivenMatchResultH_WhenHomeCancelEvent_ThenShouldReturnEmpty()
    {
        // Given
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal); // H

        // When
        var result = _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeCancel);

        // Then
        Assert.Equal("", result);
    }

    [Fact]
    public void UpdateMatchResult_GivenMatchResultHASemicolon_WhenAwayCancelEvent_ThenShouldReturnHSemicolon()
    {
        // Given
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal); // H
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal); // HA
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.NextPeriod); // HA;

        // When
        var result = _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayCancel);

        // Then
        Assert.Equal("H;", result);
    }

    [Fact]
    public void UpdateMatchResult_GivenMatchResultAHSemicolon_WhenHomeCancelEvent_ThenShouldReturnASemicolon()
    {
        // Given
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal); // A
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal); // AH
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.NextPeriod); // AH;

        // When
        var result = _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeCancel);

        // Then
        Assert.Equal("A;", result);
    }

    [Fact]
    public void UpdateMatchResult_GivenMatchResultAH_WhenAwayCancelEvent_ThenShouldThrowException()
    {
        // Given
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal); // A
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal); // AH

        // When & Then
        var exception = Assert.Throws<UpdateMatchResultException>(() =>
            _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayCancel));

        Assert.Equal(matchId, exception.MatchId);
        Assert.Equal("AH", exception.OriginalMatchResult);
        Assert.Contains("AwayCancel", exception.Message);
        Assert.Contains("Expected last character to be 'A' but found 'H'", exception.Message);
    }

    [Fact]
    public void UpdateMatchResult_GivenEmptyResult_WhenHomeCancelEvent_ThenShouldThrowException()
    {
        // Given
        int matchId = 91;

        // When & Then
        var exception = Assert.Throws<UpdateMatchResultException>(() =>
            _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeCancel));

        Assert.Equal(matchId, exception.MatchId);
        Assert.Equal("", exception.OriginalMatchResult);
        Assert.Contains("HomeCancel", exception.Message);
    }

    [Fact]
    public void UpdateMatchResult_GivenMatchResultH_WhenAwayCancelEvent_ThenShouldThrowException()
    {
        // Given
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal); // H

        // When & Then
        var exception = Assert.Throws<UpdateMatchResultException>(() =>
            _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayCancel));

        Assert.Equal(matchId, exception.MatchId);
        Assert.Equal("H", exception.OriginalMatchResult);
        Assert.Contains("AwayCancel", exception.Message);
        Assert.Contains("Expected last character to be 'A' but found 'H'", exception.Message);
    }

    [Fact]
    public void UpdateMatchResult_GivenNewMatchId_WhenHomeGoalEvent_ThenShouldCreateMatchAndReturnH()
    {
        // Given
        int newMatchId = 99;

        // When
        var result = _controller.UpdateMatchResult(newMatchId, (int)MatchEvent.HomeGoal);

        // Then
        Assert.Equal("H", result);
    }

    [Fact]
    public void DisplayMatchResult_GivenEmptyResult_WhenCalled_ThenShouldReturnFirstHalf00()
    {
        // Given
        int matchId = 91;

        // When
        var result = _controller.DisplayMatchResult(matchId);

        // Then
        Assert.Equal("0 : 0 (First Half)", result);
    }

    [Fact]
    public void DisplayMatchResult_GivenMatchResultH_WhenCalled_ThenShouldReturn10FirstHalf()
    {
        // Given
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);

        // When
        var result = _controller.DisplayMatchResult(matchId);

        // Then
        Assert.Equal("1 : 0 (First Half)", result);
    }

    [Fact]
    public void DisplayMatchResult_GivenMatchResultHA_WhenCalled_ThenShouldReturn11FirstHalf()
    {
        // Given
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal);

        // When
        var result = _controller.DisplayMatchResult(matchId);

        // Then
        Assert.Equal("1 : 1 (First Half)", result);
    }

    [Fact]
    public void DisplayMatchResult_GivenMatchResultHAH_WhenCalled_ThenShouldReturn21FirstHalf()
    {
        // Given
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);

        // When
        var result = _controller.DisplayMatchResult(matchId);

        // Then
        Assert.Equal("2 : 1 (First Half)", result);
    }

    [Fact]
    public void DisplayMatchResult_GivenMatchResultHAHSemicolon_WhenCalled_ThenShouldReturn21SecondHalf()
    {
        // Given
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.NextPeriod);

        // When
        var result = _controller.DisplayMatchResult(matchId);

        // Then
        Assert.Equal("2 : 1 (Second Half)", result);
    }

    [Fact]
    public void DisplayMatchResult_GivenMatchResultHAHSemicolonA_WhenCalled_ThenShouldReturn22SecondHalf()
    {
        // Given
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.NextPeriod);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal);

        // When
        var result = _controller.DisplayMatchResult(matchId);

        // Then
        Assert.Equal("2 : 2 (Second Half)", result);
    }

    [Fact]
    public void DisplayMatchResult_GivenNonExistentMatch_WhenCalled_ThenShouldReturn00FirstHalf()
    {
        // Given
        int nonExistentMatchId = 999;

        // When
        var result = _controller.DisplayMatchResult(nonExistentMatchId);

        // Then
        Assert.Equal("0 : 0 (First Half)", result);
    }

    [Fact]
    public void DisplayMatchResult_GivenMatchAfterCancel_WhenCalled_ThenShouldShowCorrectScore()
    {
        // Given
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal); // H
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal); // HA
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal); // HAH
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeCancel); // HA

        // When
        var result = _controller.DisplayMatchResult(matchId);

        // Then
        Assert.Equal("1 : 1 (First Half)", result);
    }
} 