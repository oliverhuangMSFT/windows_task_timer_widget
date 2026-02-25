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
        private int _newTimerSeconds = 900;
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

        public int NewTimerSeconds
        {
            get => _newTimerSeconds;
            set
            {
                _newTimerSeconds = Math.Max(10, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(NewTimerDurationDisplay));
            }
        }

        public string NewTimerDurationDisplay
        {
            get
            {
                if (_newTimerSeconds < 60) return $"{_newTimerSeconds} sec";
                int minutes = _newTimerSeconds / 60;
                if (minutes < 60) return $"{minutes} min";
                int hrs = minutes / 60;
                int rem = minutes % 60;
                return rem == 0 ? $"{hrs} hr" : $"{hrs} hr {rem} min";
            }
        }

        public bool HasExpiredTimers
        {
            get => _hasExpiredTimers;
            set { _hasExpiredTimers = value; OnPropertyChanged(); }
        }

        public ICommand AddTimerCommand => new RelayCommand(_ => AddTimer(), _ => !string.IsNullOrWhiteSpace(NewTimerTitle));
        public ICommand IncrementCommand => new RelayCommand(_ => NewTimerSeconds = GetNextDuration(NewTimerSeconds));
        public ICommand DecrementCommand => new RelayCommand(_ => NewTimerSeconds = GetPrevDuration(NewTimerSeconds));
        public ICommand PauseResumeCommand => new RelayCommand(o => PauseResume((TimerItem)o!));
        public ICommand RemoveTimerCommand => new RelayCommand(o => RemoveTimer((TimerItem)o!));
        public ICommand ToggleEditCommand => new RelayCommand(o => ((TimerItem)o!).IsEditing = !((TimerItem)o!).IsEditing);

        internal static int GetNextDuration(int currentSeconds)
        {
            if (currentSeconds < 10) return 10;
            if (currentSeconds < 30) return 30;
            if (currentSeconds < 60) return 60;
            if (currentSeconds < 300) return currentSeconds + 60;
            if (currentSeconds < 900) return currentSeconds + 300;
            if (currentSeconds < 7200) return currentSeconds + 900;
            return currentSeconds + 1800;
        }

        internal static int GetPrevDuration(int currentSeconds)
        {
            if (currentSeconds <= 10) return 10;
            if (currentSeconds <= 30) return 10;
            if (currentSeconds <= 60) return 30;
            if (currentSeconds <= 300) return currentSeconds - 60;
            if (currentSeconds <= 900) return currentSeconds - 300;
            if (currentSeconds <= 7200) return currentSeconds - 900;
            return currentSeconds - 1800;
        }

        private void AddTimer()
        {
            var timer = new TimerItem
            {
                Title = NewTimerTitle.Trim(),
                TotalSeconds = _newTimerSeconds,
                RemainingSeconds = _newTimerSeconds
            };
            timer.Expired += OnSingleTimerExpired;
            Timers.Add(timer);
            timer.Start();

            NewTimerTitle = string.Empty;
            NewTimerSeconds = 900;
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

        internal static int GetStepSize(int currentSeconds)
        {
            if (currentSeconds < 60) return 10;
            if (currentSeconds < 300) return 60;
            if (currentSeconds < 900) return 300;
            if (currentSeconds < 7200) return 900;
            return 1800;
        }

        public void AddTimeToTimer(TimerItem timer)
        {
            timer.AddTime(GetStepSize(timer.RemainingSeconds));
            CheckExpiredTimers();
        }

        public void SubtractTimeFromTimer(TimerItem timer)
        {
            timer.SubtractTime(GetStepSize(timer.RemainingSeconds));
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
