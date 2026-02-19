# ⏱ Timer Widget

A compact, always-on-top desktop timer widget for Windows. Manage multiple named countdown timers from a small, dark-themed widget docked to the corner of your screen.

## Features

- **Always-on-top** — stays visible over all windows
- **Multiple timers** — run as many concurrent timers as you need
- **Progress bar** — each timer row shows a green progress bar that shrinks as time elapses (yellow when paused)
- **Visual alerts** — when a timer expires, the widget border and the timer row flash red, and the taskbar flashes
- **Extend or dismiss** — expired timers show a `+15` button to add 15 more minutes, or `✕` to dismiss
- **15-minute increments** — simple `−15` / `+15` buttons to set timer duration
- **Grows upward** — the widget anchors to the bottom-right and expands upward as timers are added
- **Draggable** — click and drag the header to reposition
- **No sound** — visual-only notifications
- **Dark theme** — minimal, non-distracting UI

## Usage

1. Type a name in the title field (e.g., "Stand up")
2. Click `+15` / `−15` to set the duration in 15-minute increments
3. Click **Start** (or press Enter)
4. The timer appears above the input bar with a green progress bar
5. Click `⏸` to pause or `▶` to resume
6. When time's up, the row and border flash red — click `+15` to extend or `✕` to dismiss

## Running

### From the pre-built exe

No .NET installation required — just run:

```
publish\TimerWidget.exe
```

### From source

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later.

```bash
dotnet run --project TimerWidget
```

### Building a distributable exe

```bash
dotnet publish TimerWidget\TimerWidget.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

## Project Structure

```
TimerWidget/
├── App.xaml                        # Application entry point
├── MainWindow.xaml / .xaml.cs      # Main widget window (UI + code-behind)
├── Models/
│   └── TimerItem.cs                # Timer data model with countdown logic
├── ViewModels/
│   └── MainViewModel.cs            # ViewModel with timer collection and commands
├── Converters/
│   └── Converters.cs               # Bool-to-visibility and play/pause converters
└── Helpers/
    └── FlashHelper.cs              # Win32 FlashWindowEx interop for taskbar flash
```

## Tech Stack

- **WPF** (.NET 8, C#)
- **MVVM** pattern
- **Win32 interop** for taskbar flashing
