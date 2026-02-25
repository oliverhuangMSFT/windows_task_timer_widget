using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using TimerWidget.Helpers;
using TimerWidget.Models;
using TimerWidget.ViewModels;

namespace TimerWidget;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    private Storyboard? _flashStoryboard;
    private IntPtr _hwnd;
    private int _scrollAccumulator;
    private const int ScrollThreshold = 120;

    public MainWindow()
    {
        InitializeComponent();
        _vm = new MainViewModel();
        DataContext = _vm;

        _vm.TimerExpired += OnTimerExpired;
        _vm.AllTimersDismissed += OnAllTimersDismissed;

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _hwnd = new WindowInteropHelper(this).Handle;
        PositionBottomRight();

        _flashStoryboard = (Storyboard)FindResource("FlashBorder");
    }

    private void PositionBottomRight()
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 10;
        Top = workArea.Bottom - ActualHeight - 10;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        // Anchor to bottom-right: when height changes, shift Top so bottom edge stays fixed
        if (sizeInfo.HeightChanged)
        {
            Top -= sizeInfo.NewSize.Height - sizeInfo.PreviousSize.Height;
        }
    }

    private void OnTimerExpired(object? sender, EventArgs e)
    {
        _flashStoryboard?.Begin(this, true);
        FlashHelper.FlashWindow(_hwnd);
    }

    private void OnAllTimersDismissed(object? sender, EventArgs e)
    {
        _flashStoryboard?.Stop(this);
        FlashHelper.StopFlash(_hwnd);
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1) DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Widget_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        _scrollAccumulator += e.Delta;
        while (_scrollAccumulator >= ScrollThreshold)
        {
            _vm.IncrementCommand.Execute(null);
            _scrollAccumulator -= ScrollThreshold;
        }
        while (_scrollAccumulator <= -ScrollThreshold)
        {
            _vm.DecrementCommand.Execute(null);
            _scrollAccumulator += ScrollThreshold;
        }
        e.Handled = true;
    }

    private void TimerRow_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is TimerItem timer && timer.IsEditing)
        {
            _scrollAccumulator += e.Delta;
            while (_scrollAccumulator >= ScrollThreshold)
            {
                _vm.AddTimeToTimer(timer);
                _scrollAccumulator -= ScrollThreshold;
            }
            while (_scrollAccumulator <= -ScrollThreshold)
            {
                _vm.SubtractTimeFromTimer(timer);
                _scrollAccumulator += ScrollThreshold;
            }
            e.Handled = true;
        }
    }

    private void TitleInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && _vm.AddTimerCommand.CanExecute(null))
            _vm.AddTimerCommand.Execute(null);
    }
}