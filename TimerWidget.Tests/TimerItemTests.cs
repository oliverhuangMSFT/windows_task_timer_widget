using TimerWidget.Models;

namespace TimerWidget.Tests;

public class TimerItemTests
{
    private static TimerItem CreateTimer(int totalSeconds = 900)
    {
        return new TimerItem
        {
            Title = "Test",
            TotalSeconds = totalSeconds,
            RemainingSeconds = totalSeconds
        };
    }

    [Fact]
    public void Constructor_DefaultValues()
    {
        var timer = new TimerItem();

        Assert.Equal(string.Empty, timer.Title);
        Assert.Equal(0, timer.RemainingSeconds);
        Assert.Equal(0, timer.TotalSeconds);
        Assert.False(timer.IsRunning);
        Assert.False(timer.IsExpired);
    }

    [Fact]
    public void Start_SetsIsRunningTrue()
    {
        var timer = CreateTimer();

        timer.Start();

        Assert.True(timer.IsRunning);
        Assert.False(timer.IsExpired);
    }

    [Fact]
    public void Start_WithZeroSeconds_DoesNotStart()
    {
        var timer = CreateTimer(0);

        timer.Start();

        Assert.False(timer.IsRunning);
    }

    [Fact]
    public void Pause_SetsIsRunningFalse()
    {
        var timer = CreateTimer();
        timer.Start();

        timer.Pause();

        Assert.False(timer.IsRunning);
    }

    [Fact]
    public void Resume_AfterPause_SetsIsRunningTrue()
    {
        var timer = CreateTimer();
        timer.Start();
        timer.Pause();

        timer.Resume();

        Assert.True(timer.IsRunning);
    }

    [Fact]
    public void Resume_WhenExpired_DoesNotResume()
    {
        var timer = CreateTimer();
        timer.IsExpired = true;
        timer.RemainingSeconds = 0;

        timer.Resume();

        Assert.False(timer.IsRunning);
    }

    [Fact]
    public void Resume_WhenZeroRemaining_DoesNotResume()
    {
        var timer = CreateTimer();
        timer.RemainingSeconds = 0;

        timer.Resume();

        Assert.False(timer.IsRunning);
    }

    [Fact]
    public void Stop_SetsIsRunningFalse()
    {
        var timer = CreateTimer();
        timer.Start();

        timer.Stop();

        Assert.False(timer.IsRunning);
    }

    [Fact]
    public void Reset_RestoresRemainingToTotal()
    {
        var timer = CreateTimer(600);
        timer.Start();
        timer.RemainingSeconds = 100;

        timer.Reset();

        Assert.Equal(600, timer.RemainingSeconds);
        Assert.True(timer.IsRunning);
        Assert.False(timer.IsExpired);
    }

    [Fact]
    public void Reset_ClearsExpiredState()
    {
        var timer = CreateTimer(600);
        timer.IsExpired = true;
        timer.RemainingSeconds = 0;

        timer.Reset();

        Assert.Equal(600, timer.RemainingSeconds);
        Assert.False(timer.IsExpired);
        Assert.True(timer.IsRunning);
    }

    [Fact]
    public void Extend_ResetsProgressToFull()
    {
        var timer = CreateTimer(600);
        timer.Start();
        timer.RemainingSeconds = 0;
        timer.IsExpired = true;
        Assert.Equal(0.0, timer.Progress);

        timer.Extend(15);

        Assert.Equal(1.0, timer.Progress);
    }

    [Fact]
    public void Extend_ResetsTimerToExtensionAmount()
    {
        var timer = CreateTimer(600);
        timer.Start();
        timer.RemainingSeconds = 0;
        timer.IsExpired = true;

        timer.Extend(15);

        Assert.Equal(15 * 60, timer.RemainingSeconds);
        Assert.Equal(15 * 60, timer.TotalSeconds);
        Assert.False(timer.IsExpired);
        Assert.True(timer.IsRunning);
    }

    [Fact]
    public void Extend_DoesNotAccumulateTotalSeconds()
    {
        var timer = CreateTimer(600);
        timer.RemainingSeconds = 0;
        timer.IsExpired = true;

        timer.Extend(15);

        Assert.Equal(900, timer.TotalSeconds);
        Assert.Equal(900, timer.RemainingSeconds);

        // Simulate expiry again and extend once more
        timer.RemainingSeconds = 0;
        timer.IsExpired = true;
        timer.Extend(15);

        // Should still be 900, not 1800
        Assert.Equal(900, timer.TotalSeconds);
        Assert.Equal(900, timer.RemainingSeconds);
    }

    [Fact]
    public void Progress_FullWhenJustStarted()
    {
        var timer = CreateTimer(600);

        Assert.Equal(1.0, timer.Progress);
    }

    [Fact]
    public void Progress_ZeroWhenDone()
    {
        var timer = CreateTimer(600);
        timer.RemainingSeconds = 0;

        Assert.Equal(0.0, timer.Progress);
    }

    [Fact]
    public void Progress_HalfwayThrough()
    {
        var timer = CreateTimer(600);
        timer.RemainingSeconds = 300;

        Assert.Equal(0.5, timer.Progress);
    }

    [Fact]
    public void Progress_ZeroWhenTotalIsZero()
    {
        var timer = CreateTimer(0);

        Assert.Equal(0.0, timer.Progress);
    }

    [Fact]
    public void RemainingMinutesDisplay_ShowsCeilingMinutes()
    {
        var timer = CreateTimer(90); // 1.5 minutes

        Assert.Equal("2 min", timer.RemainingMinutesDisplay);
    }

    [Fact]
    public void RemainingMinutesDisplay_ExactMinutes()
    {
        var timer = CreateTimer(120);

        Assert.Equal("2 min", timer.RemainingMinutesDisplay);
    }

    [Fact]
    public void RemainingMinutesDisplay_WhenExpired_ShowsZero()
    {
        var timer = CreateTimer(600);
        timer.IsExpired = true;

        Assert.Equal("0 min", timer.RemainingMinutesDisplay);
    }

    [Fact]
    public void PropertyChanged_RaisedOnTitleChange()
    {
        var timer = new TimerItem();
        var raised = false;
        timer.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TimerItem.Title)) raised = true;
        };

        timer.Title = "New Title";

        Assert.True(raised);
    }

    [Fact]
    public void PropertyChanged_RaisedOnRemainingSecondsChange()
    {
        var timer = CreateTimer();
        var props = new List<string>();
        timer.PropertyChanged += (_, e) => props.Add(e.PropertyName!);

        timer.RemainingSeconds = 100;

        Assert.Contains(nameof(TimerItem.RemainingSeconds), props);
        Assert.Contains(nameof(TimerItem.RemainingMinutesDisplay), props);
        Assert.Contains(nameof(TimerItem.Progress), props);
    }
}
