using TimerWidget.ViewModels;

namespace TimerWidget.Tests;

public class MainViewModelTests
{
    [Fact]
    public void AddTimerCommand_CannotExecute_WhenTitleEmpty()
    {
        var vm = new MainViewModel();
        vm.NewTimerTitle = "";

        Assert.False(vm.AddTimerCommand.CanExecute(null));
    }

    [Fact]
    public void AddTimerCommand_CanExecute_WhenTitleSet()
    {
        var vm = new MainViewModel();
        vm.NewTimerTitle = "Meeting";

        Assert.True(vm.AddTimerCommand.CanExecute(null));
    }

    [Fact]
    public void AddTimerCommand_AddsTimerToCollection()
    {
        var vm = new MainViewModel();
        vm.NewTimerTitle = "Meeting";
        vm.NewTimerSeconds = 1800;

        vm.AddTimerCommand.Execute(null);

        Assert.Single(vm.Timers);
        Assert.Equal("Meeting", vm.Timers[0].Title);
        Assert.Equal(1800, vm.Timers[0].TotalSeconds);
        Assert.True(vm.Timers[0].IsRunning);
    }

    [Fact]
    public void AddTimerCommand_ResetsTitleAndSeconds()
    {
        var vm = new MainViewModel();
        vm.NewTimerTitle = "Meeting";
        vm.NewTimerSeconds = 1800;

        vm.AddTimerCommand.Execute(null);

        Assert.Equal(string.Empty, vm.NewTimerTitle);
        Assert.Equal(900, vm.NewTimerSeconds);
    }

    [Fact]
    public void NewTimerSeconds_ClampedToMinimum10()
    {
        var vm = new MainViewModel();

        vm.NewTimerSeconds = 5;

        Assert.Equal(10, vm.NewTimerSeconds);
    }

    [Fact]
    public void NewTimerDurationDisplay_ShowsSeconds_WhenUnderOneMinute()
    {
        var vm = new MainViewModel();
        vm.NewTimerSeconds = 30;

        Assert.Equal("30 sec", vm.NewTimerDurationDisplay);
    }

    [Fact]
    public void NewTimerDurationDisplay_ShowsMinutes_WhenAtOrAboveOneMinute()
    {
        var vm = new MainViewModel();
        vm.NewTimerSeconds = 1800;

        Assert.Equal("30 min", vm.NewTimerDurationDisplay);
    }

    [Fact]
    public void NewTimerDurationDisplay_ShowsHours_WhenAtOrAboveOneHour()
    {
        var vm = new MainViewModel();
        vm.NewTimerSeconds = 3600;

        Assert.Equal("1 hr", vm.NewTimerDurationDisplay);
    }

    [Fact]
    public void NewTimerDurationDisplay_ShowsHoursAndMinutes()
    {
        var vm = new MainViewModel();
        vm.NewTimerSeconds = 5400; // 1hr 30min

        Assert.Equal("1 hr 30 min", vm.NewTimerDurationDisplay);
    }

    [Fact]
    public void IncrementCommand_StepsUpCorrectly()
    {
        var vm = new MainViewModel();
        vm.NewTimerSeconds = 900; // 15 min

        vm.IncrementCommand.Execute(null);

        Assert.Equal(1800, vm.NewTimerSeconds); // 30 min (+15 min step)
    }

    [Fact]
    public void DecrementCommand_StepsDownCorrectly()
    {
        var vm = new MainViewModel();
        vm.NewTimerSeconds = 900; // 15 min

        vm.DecrementCommand.Execute(null);

        Assert.Equal(600, vm.NewTimerSeconds); // 10 min (-5 min step)
    }

    [Fact]
    public void DecrementCommand_ClampsAtMinimum()
    {
        var vm = new MainViewModel();
        vm.NewTimerSeconds = 10;

        vm.DecrementCommand.Execute(null);

        Assert.Equal(10, vm.NewTimerSeconds);
    }

    [Theory]
    [InlineData(10, 30)]
    [InlineData(30, 60)]
    [InlineData(60, 120)]
    [InlineData(240, 300)]
    [InlineData(300, 600)]
    [InlineData(600, 900)]
    [InlineData(900, 1800)]
    [InlineData(6300, 7200)]
    [InlineData(7200, 9000)]
    public void GetNextDuration_StepsCorrectly(int current, int expected)
    {
        Assert.Equal(expected, MainViewModel.GetNextDuration(current));
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(30, 10)]
    [InlineData(60, 30)]
    [InlineData(120, 60)]
    [InlineData(300, 240)]
    [InlineData(600, 300)]
    [InlineData(900, 600)]
    [InlineData(7200, 6300)]
    [InlineData(9000, 7200)]
    public void GetPrevDuration_StepsCorrectly(int current, int expected)
    {
        Assert.Equal(expected, MainViewModel.GetPrevDuration(current));
    }

    [Fact]
    public void RemoveTimerCommand_RemovesTimer()
    {
        var vm = new MainViewModel();
        vm.NewTimerTitle = "Test";
        vm.AddTimerCommand.Execute(null);
        var timer = vm.Timers[0];

        vm.RemoveTimerCommand.Execute(timer);

        Assert.Empty(vm.Timers);
        Assert.False(timer.IsRunning);
    }

    [Fact]
    public void PauseResumeCommand_TogglesRunning()
    {
        var vm = new MainViewModel();
        vm.NewTimerTitle = "Test";
        vm.AddTimerCommand.Execute(null);
        var timer = vm.Timers[0];
        Assert.True(timer.IsRunning);

        vm.PauseResumeCommand.Execute(timer);
        Assert.False(timer.IsRunning);

        vm.PauseResumeCommand.Execute(timer);
        Assert.True(timer.IsRunning);
    }

    [Fact]
    public void AddTimeToTimer_AddsStepToRunningTimer()
    {
        var vm = new MainViewModel();
        vm.NewTimerTitle = "Test";
        vm.NewTimerSeconds = 1800; // 30 min
        vm.AddTimerCommand.Execute(null);
        var timer = vm.Timers[0];
        timer.RemainingSeconds = 600; // 10 min remaining

        vm.AddTimeToTimer(timer);

        // 10 min is in 5-15min range, step = 5 min (300s)
        Assert.Equal(900, timer.RemainingSeconds);
        Assert.Equal(900, timer.TotalSeconds);
    }

    [Fact]
    public void AddTimeToTimer_RestartsExpiredTimer()
    {
        var vm = new MainViewModel();
        vm.NewTimerTitle = "Test";
        vm.AddTimerCommand.Execute(null);
        var timer = vm.Timers[0];
        timer.RemainingSeconds = 0;
        timer.IsExpired = true;

        vm.AddTimeToTimer(timer);

        // Expired at 0s, step = 10s
        Assert.Equal(10, timer.RemainingSeconds);
        Assert.False(timer.IsExpired);
        Assert.True(timer.IsRunning);
    }

    [Fact]
    public void SubtractTimeFromTimer_SubtractsStep()
    {
        var vm = new MainViewModel();
        vm.NewTimerTitle = "Test";
        vm.NewTimerSeconds = 1800;
        vm.AddTimerCommand.Execute(null);
        var timer = vm.Timers[0];
        timer.RemainingSeconds = 600; // 10 min

        vm.SubtractTimeFromTimer(timer);

        Assert.Equal(300, timer.RemainingSeconds);
    }

    [Fact]
    public void SubtractTimeFromTimer_DoesNotGoBelowZero()
    {
        var vm = new MainViewModel();
        vm.NewTimerTitle = "Test";
        vm.NewTimerSeconds = 60;
        vm.AddTimerCommand.Execute(null);
        var timer = vm.Timers[0];
        timer.RemainingSeconds = 5; // less than step size of 10

        vm.SubtractTimeFromTimer(timer);

        Assert.Equal(5, timer.RemainingSeconds);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(30, 10)]
    [InlineData(59, 10)]
    [InlineData(60, 60)]
    [InlineData(299, 60)]
    [InlineData(300, 300)]
    [InlineData(899, 300)]
    [InlineData(900, 900)]
    [InlineData(7199, 900)]
    [InlineData(7200, 1800)]
    public void GetStepSize_ReturnsCorrectStep(int current, int expected)
    {
        Assert.Equal(expected, MainViewModel.GetStepSize(current));
    }

    [Fact]
    public void MultipleTimers_CanBeAddedAndRemoved()
    {
        var vm = new MainViewModel();

        vm.NewTimerTitle = "Timer 1";
        vm.AddTimerCommand.Execute(null);
        vm.NewTimerTitle = "Timer 2";
        vm.AddTimerCommand.Execute(null);
        vm.NewTimerTitle = "Timer 3";
        vm.AddTimerCommand.Execute(null);

        Assert.Equal(3, vm.Timers.Count);

        vm.RemoveTimerCommand.Execute(vm.Timers[1]);

        Assert.Equal(2, vm.Timers.Count);
        Assert.Equal("Timer 1", vm.Timers[0].Title);
        Assert.Equal("Timer 3", vm.Timers[1].Title);
    }
}
