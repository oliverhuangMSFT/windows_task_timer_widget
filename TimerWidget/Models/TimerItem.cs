using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace TimerWidget.Models
{
    public class TimerItem : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private int _remainingSeconds;
        private int _totalSeconds;
        private bool _isRunning;
        private bool _isExpired;
        private bool _isEditing;
        private readonly DispatcherTimer _timer;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? Expired;

        public TimerItem()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public int RemainingSeconds
        {
            get => _remainingSeconds;
            set
            {
                _remainingSeconds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RemainingMinutesDisplay));
                OnPropertyChanged(nameof(Progress));
            }
        }

        public int TotalSeconds
        {
            get => _totalSeconds;
            set { _totalSeconds = value; OnPropertyChanged(); OnPropertyChanged(nameof(Progress)); }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set { _isRunning = value; OnPropertyChanged(); }
        }

        public bool IsExpired
        {
            get => _isExpired;
            set { _isExpired = value; OnPropertyChanged(); }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }

        /// <summary>Fraction of time remaining (1.0 = full, 0.0 = done)</summary>
        public double Progress => _totalSeconds > 0 ? (double)_remainingSeconds / _totalSeconds : 0;

        public string RemainingMinutesDisplay
        {
            get
            {
                if (IsExpired) return "0 sec";
                if (_remainingSeconds < 60) return $"{_remainingSeconds} sec";
                int minutes = (int)Math.Ceiling(_remainingSeconds / 60.0);
                if (minutes < 60) return $"{minutes} min";
                int hrs = minutes / 60;
                int rem = minutes % 60;
                return rem == 0 ? $"{hrs} hr" : $"{hrs} hr {rem} min";
            }
        }

        public void Start()
        {
            if (_remainingSeconds <= 0) return;
            IsRunning = true;
            IsExpired = false;
            _timer.Start();
        }

        public void Pause()
        {
            IsRunning = false;
            _timer.Stop();
        }

        public void Resume()
        {
            if (_remainingSeconds <= 0 || IsExpired) return;
            IsRunning = true;
            _timer.Start();
        }

        public void Extend(int minutes)
        {
            _timer.Stop();
            RemainingSeconds = minutes * 60;
            TotalSeconds = minutes * 60;
            IsExpired = false;
            IsRunning = true;
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(RemainingMinutesDisplay));
            _timer.Start();
        }

        public void Reset()
        {
            _timer.Stop();
            RemainingSeconds = TotalSeconds;
            IsExpired = false;
            IsRunning = true;
            OnPropertyChanged(nameof(RemainingMinutesDisplay));
            _timer.Start();
        }

        public void AddTime(int seconds)
        {
            if (IsExpired)
            {
                _timer.Stop();
                RemainingSeconds = seconds;
                TotalSeconds = seconds;
                IsExpired = false;
                IsRunning = true;
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(RemainingMinutesDisplay));
                _timer.Start();
            }
            else
            {
                RemainingSeconds += seconds;
                TotalSeconds = RemainingSeconds;
            }
        }

        public void SubtractTime(int seconds)
        {
            if (IsExpired) return;
            if (RemainingSeconds - seconds <= 0) return;
            RemainingSeconds -= seconds;
            TotalSeconds = RemainingSeconds;
        }

        public void Stop()
        {
            _timer.Stop();
            IsRunning = false;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            RemainingSeconds--;
            if (RemainingSeconds <= 0)
            {
                RemainingSeconds = 0;
                _timer.Stop();
                IsRunning = false;
                IsExpired = true;
                OnPropertyChanged(nameof(RemainingMinutesDisplay));
                Expired?.Invoke(this, EventArgs.Empty);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
