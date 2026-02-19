using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TimerWidget.Models;

namespace TimerWidget.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _newTimerTitle = string.Empty;
        private int _newTimerMinutes = 15;
        private bool _hasExpiredTimers;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? TimerExpired;
        public event EventHandler? AllTimersDismissed;

        public ObservableCollection<TimerItem> Timers { get; } = new();

        public string NewTimerTitle
        {
            get => _newTimerTitle;
            set { _newTimerTitle = value; OnPropertyChanged(); }
        }

        public int NewTimerMinutes
        {
            get => _newTimerMinutes;
            set
            {
                _newTimerMinutes = Math.Max(15, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(NewTimerMinutesDisplay));
            }
        }

        public string NewTimerMinutesDisplay => $"{_newTimerMinutes} min";

        public bool HasExpiredTimers
        {
            get => _hasExpiredTimers;
            set { _hasExpiredTimers = value; OnPropertyChanged(); }
        }

        public ICommand AddTimerCommand => new RelayCommand(_ => AddTimer(), _ => !string.IsNullOrWhiteSpace(NewTimerTitle));
        public ICommand IncrementCommand => new RelayCommand(_ => NewTimerMinutes += 15);
        public ICommand DecrementCommand => new RelayCommand(_ => NewTimerMinutes -= 15);
        public ICommand PauseResumeCommand => new RelayCommand(o => PauseResume((TimerItem)o!));
        public ICommand RemoveTimerCommand => new RelayCommand(o => RemoveTimer((TimerItem)o!));
        public ICommand ExtendTimerCommand => new RelayCommand(o => ExtendTimer((TimerItem)o!));
        public ICommand ResetTimerCommand => new RelayCommand(o => ResetTimer((TimerItem)o!));

        private void AddTimer()
        {
            var timer = new TimerItem
            {
                Title = NewTimerTitle.Trim(),
                TotalSeconds = _newTimerMinutes * 60,
                RemainingSeconds = _newTimerMinutes * 60
            };
            timer.Expired += OnSingleTimerExpired;
            Timers.Add(timer);
            timer.Start();

            NewTimerTitle = string.Empty;
            NewTimerMinutes = 15;
        }

        public void AddDevTimer10Sec()
        {
            var title = string.IsNullOrWhiteSpace(NewTimerTitle) ? "Dev Test" : NewTimerTitle.Trim();
            var timer = new TimerItem
            {
                Title = title,
                TotalSeconds = 10,
                RemainingSeconds = 10
            };
            timer.Expired += OnSingleTimerExpired;
            Timers.Add(timer);
            timer.Start();
            NewTimerTitle = string.Empty;
        }

        private void PauseResume(TimerItem timer)
        {
            if (timer.IsRunning) timer.Pause();
            else timer.Resume();
        }

        private void RemoveTimer(TimerItem timer)
        {
            timer.Stop();
            timer.Expired -= OnSingleTimerExpired;
            Timers.Remove(timer);
            CheckExpiredTimers();
        }

        private void ResetTimer(TimerItem timer)
        {
            timer.Reset();
            CheckExpiredTimers();
        }

        private void ExtendTimer(TimerItem timer)
        {
            timer.Extend(15);
            CheckExpiredTimers();
        }

        private void OnSingleTimerExpired(object? sender, EventArgs e)
        {
            HasExpiredTimers = true;
            TimerExpired?.Invoke(this, EventArgs.Empty);
        }

        private void CheckExpiredTimers()
        {
            bool anyExpired = false;
            foreach (var t in Timers)
            {
                if (t.IsExpired) { anyExpired = true; break; }
            }
            HasExpiredTimers = anyExpired;
            if (!anyExpired) AllTimersDismissed?.Invoke(this, EventArgs.Empty);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
    }
}
