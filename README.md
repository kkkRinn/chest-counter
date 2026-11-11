# Chest Counter

A free, open-source Windows app that counts the chests you open in Fortnite.

It never touches the game. It reads the `.replay` files Fortnite saves after each match (`%LOCALAPPDATA%\\FortniteGame\\Saved\\Demos`), so it does not interfere with Easy Anti-Cheat.

## Download and run

The app is a self-contained Windows x64 executable; you do not need to install .NET.

- **Latest release:** Open [Releases](https://github.com/kkkRinn/chest-counter/releases/latest) and download `ChestCounter.exe`.
- **Latest build:** Open the [Actions](https://github.com/kkkRinn/chest-counter/actions/workflows/blank.yml) page, select a successful **Build ChestCounter.exe** run, and download the `ChestCounter-windows-x64` artifact. GitHub may ask you to sign in to download workflow artifacts.

To publish a versioned release, push a tag beginning with `v` (for example, `v1.0.0`). GitHub Actions builds the executable and attaches it to that release automatically. New commits to `main` also build an artifact.

## Features

- Total chests, last game, session, average per game, best game, best day, today, and a 7-day chart
- Automatic detection of each new replay (about 20 seconds after a match ends)
- Match history report and a "chests before counter" offset
- Floating on-screen overlay (drag to move, right-click to close)
- OBS browser-source overlay at `http://localhost:7373/`
- Optional start with Windows

## Build locally

Requires the .NET 8 SDK on Windows:

```powershell
dotnet publish .\\ChestCounter.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o out
```

The result is `out\\ChestCounter.exe`, a single self-contained file.

## Replay parsing

Parsing uses the [FortniteReplayReader](https://github.com/xNocken/replay-reader) library. Recent Fortnite replays use Oodle compression. If parsing fails, check `%APPDATA%\\ChestCounter\\log.txt` and the FortniteReplayReader README for the Oodle library it needs next to the exe.

## Data

Everything is stored locally in `%APPDATA%\\ChestCounter` (`settings.json`, `history.json`, `log.txt`).

## License

MIT. Not affiliated with or endorsed by Epic Games.
