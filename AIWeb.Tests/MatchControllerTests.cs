using Microsoft.Extensions.Logging;
using Moq;
using AIWeb.Controllers;
using AIWeb;

namespace AIWeb.Tests;

public class MatchControllerTests
{
    private readonly MatchController _controller;
    private readonly Mock<ILogger<MatchController>> _mockLogger;

    public MatchControllerTests()
    {
        _mockLogger = new Mock<ILogger<MatchController>>();
        _controller = new MatchController(_mockLogger.Object);
        
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

    [Fact]
    public void UpdateMatchResult_HomeGoal_ShouldReturnH()
    {
        // Arrange
        int matchId = 91;
        int homeGoalEvent = (int)MatchEvent.HomeGoal;

        // Act
        var result = _controller.UpdateMatchResult(matchId, homeGoalEvent);

        // Assert
        Assert.Equal("H", result);
    }

    [Fact]
    public void UpdateMatchResult_AwayGoal_ShouldReturnA()
    {
        // Arrange
        int matchId = 91;
        int awayGoalEvent = (int)MatchEvent.AwayGoal;

        // Act
        var result = _controller.UpdateMatchResult(matchId, awayGoalEvent);

        // Assert
        Assert.Equal("A", result);
    }

    [Fact]
    public void UpdateMatchResult_NextPeriod_ShouldReturnSemicolon()
    {
        // Arrange
        int matchId = 91;
        int nextPeriodEvent = (int)MatchEvent.NextPeriod;

        // Act
        var result = _controller.UpdateMatchResult(matchId, nextPeriodEvent);

        // Assert
        Assert.Equal(";", result);
    }

    [Fact]
    public void UpdateMatchResult_SequentialEvents_ShouldAccumulate()
    {
        // Arrange
        int matchId = 91;

        // Act & Assert
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

    [Fact]
    public void UpdateMatchResult_NewMatch_ShouldCreateMatch()
    {
        // Arrange
        int newMatchId = 99;

        // Act
        var result = _controller.UpdateMatchResult(newMatchId, (int)MatchEvent.HomeGoal);

        // Assert
        Assert.Equal("H", result);
    }

    [Fact]
    public void DisplayMatchResult_EmptyResult_ShouldReturnFirstHalf00()
    {
        // Arrange
        int matchId = 91;

        // Act
        var result = _controller.DisplayMatchResult(matchId);

        // Assert
        Assert.Equal("0 : 0 (First Half)", result);
    }

    [Fact]
    public void DisplayMatchResult_H_ShouldReturn10FirstHalf()
    {
        // Arrange
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);

        // Act
        var result = _controller.DisplayMatchResult(matchId);

        // Assert
        Assert.Equal("1 : 0 (First Half)", result);
    }

    [Fact]
    public void DisplayMatchResult_HA_ShouldReturn11FirstHalf()
    {
        // Arrange
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal);

        // Act
        var result = _controller.DisplayMatchResult(matchId);

        // Assert
        Assert.Equal("1 : 1 (First Half)", result);
    }

    [Fact]
    public void DisplayMatchResult_HAH_ShouldReturn21FirstHalf()
    {
        // Arrange
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);

        // Act
        var result = _controller.DisplayMatchResult(matchId);

        // Assert
        Assert.Equal("2 : 1 (First Half)", result);
    }

    [Fact]
    public void DisplayMatchResult_HAHSemicolon_ShouldReturn21SecondHalf()
    {
        // Arrange
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.NextPeriod);

        // Act
        var result = _controller.DisplayMatchResult(matchId);

        // Assert
        Assert.Equal("2 : 1 (Second Half)", result);
    }

    [Fact]
    public void DisplayMatchResult_HAHSemicolonA_ShouldReturn22SecondHalf()
    {
        // Arrange
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.NextPeriod);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal);

        // Act
        var result = _controller.DisplayMatchResult(matchId);

        // Assert
        Assert.Equal("2 : 2 (Second Half)", result);
    }

    [Fact]
    public void DisplayMatchResult_NonExistentMatch_ShouldReturn00FirstHalf()
    {
        // Arrange
        int nonExistentMatchId = 999;

        // Act
        var result = _controller.DisplayMatchResult(nonExistentMatchId);

        // Assert
        Assert.Equal("0 : 0 (First Half)", result);
    }

    [Fact]
    public void DisplayMatchResult_MultiplePeriods_ShouldShowCorrectPeriod()
    {
        // Arrange
        int matchId = 91;
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.NextPeriod);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.AwayGoal);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.NextPeriod);
        _controller.UpdateMatchResult(matchId, (int)MatchEvent.HomeGoal);

        // Act
        var result = _controller.DisplayMatchResult(matchId);

        // Assert
        Assert.Equal("2 : 1 (Third Period)", result);
    }
} 