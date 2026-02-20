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
        vm.NewTimerMinutes = 30;

        vm.AddTimerCommand.Execute(null);

        Assert.Single(vm.Timers);
        Assert.Equal("Meeting", vm.Timers[0].Title);
        Assert.Equal(30 * 60, vm.Timers[0].TotalSeconds);
        Assert.True(vm.Timers[0].IsRunning);
    }

    [Fact]
    public void AddTimerCommand_ResetsTitleAndMinutes()
    {
        var vm = new MainViewModel();
        vm.NewTimerTitle = "Meeting";
        vm.NewTimerMinutes = 30;

        vm.AddTimerCommand.Execute(null);

        Assert.Equal(string.Empty, vm.NewTimerTitle);
        Assert.Equal(15, vm.NewTimerMinutes);
    }

    [Fact]
    public void NewTimerMinutes_ClampedToMinimum15()
    {
        var vm = new MainViewModel();

        vm.NewTimerMinutes = 5;

        Assert.Equal(15, vm.NewTimerMinutes);
    }

    [Fact]
    public void NewTimerMinutesDisplay_FormatsCorrectly()
    {
        var vm = new MainViewModel();
        vm.NewTimerMinutes = 30;

        Assert.Equal("30 min", vm.NewTimerMinutesDisplay);
    }

    [Fact]
    public void IncrementCommand_AddsMinutes()
    {
        var vm = new MainViewModel();
        vm.NewTimerMinutes = 15;

        vm.IncrementCommand.Execute(null);

        Assert.Equal(30, vm.NewTimerMinutes);
    }

    [Fact]
    public void DecrementCommand_SubtractsMinutes_ClampedAt15()
    {
        var vm = new MainViewModel();
        vm.NewTimerMinutes = 30;

        vm.DecrementCommand.Execute(null);

        Assert.Equal(15, vm.NewTimerMinutes);

        vm.DecrementCommand.Execute(null);

        Assert.Equal(15, vm.NewTimerMinutes);
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
    public void ResetTimerCommand_ResetsTimer()
    {
        var vm = new MainViewModel();
        vm.NewTimerTitle = "Test";
        vm.NewTimerMinutes = 30;
        vm.AddTimerCommand.Execute(null);
        var timer = vm.Timers[0];
        timer.RemainingSeconds = 100;

        vm.ResetTimerCommand.Execute(timer);

        Assert.Equal(30 * 60, timer.RemainingSeconds);
        Assert.True(timer.IsRunning);
    }

    [Fact]
    public void ExtendTimerCommand_ResetsToFifteenMinutes()
    {
        var vm = new MainViewModel();
        vm.NewTimerTitle = "Test";
        vm.NewTimerMinutes = 30;
        vm.AddTimerCommand.Execute(null);
        var timer = vm.Timers[0];
        timer.RemainingSeconds = 0;
        timer.IsExpired = true;

        vm.ExtendTimerCommand.Execute(timer);

        Assert.Equal(15 * 60, timer.RemainingSeconds);
        Assert.Equal(15 * 60, timer.TotalSeconds);
        Assert.False(timer.IsExpired);
        Assert.True(timer.IsRunning);
    }

    [Fact]
    public void AddDevTimer10Sec_AddsTimerWith10Seconds()
    {
        var vm = new MainViewModel();
        vm.NewTimerTitle = "";

        vm.AddDevTimer10Sec();

        Assert.Single(vm.Timers);
        Assert.Equal("Dev Test", vm.Timers[0].Title);
        Assert.Equal(10, vm.Timers[0].TotalSeconds);
        Assert.True(vm.Timers[0].IsRunning);
    }

    [Fact]
    public void AddDevTimer10Sec_UsesCustomTitle()
    {
        var vm = new MainViewModel();
        vm.NewTimerTitle = "Quick Check";

        vm.AddDevTimer10Sec();

        Assert.Equal("Quick Check", vm.Timers[0].Title);
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
