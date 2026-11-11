# Chest Counter

A free, open-source Windows app that counts the chests you open in Fortnite.

It never touches the game. It reads the `.replay` files Fortnite saves after each match
(`%LOCALAPPDATA%\FortniteGame\Saved\Demos`), so it does not interfere with Easy Anti-Cheat.

## Features
- Total chests, last game, session, average per game, best game, best day, today, and a 7-day chart
- Automatic detection of each new replay (about 20 seconds after a match ends)
- Match history report and a "chests before counter" offset
- Floating on-screen overlay (drag to move, right-click to close)
- OBS browser-source overlay at `http://localhost:7373/`
- Optional start with Windows

## Get the .exe (no installs needed)
1. Create a GitHub repository and push this folder to the `main` branch.
2. Open the **Actions** tab, wait for **Build ChestCounter.exe** to finish, and download the `ChestCounter` artifact.
3. To publish a release, push a tag such as `v1.0.0`. The exe is attached to the release automatically.

## Build locally
Requires the .NET 8 SDK on Windows:

    dotnet publish src/ChestCounter/ChestCounter.csproj -c Release -o out

The result is `out/ChestCounter.exe`, a single self-contained file.

## Replay parsing
All replay-format logic is in `src/ChestCounter/Services/ChestParser.cs`.
Parsing uses the [FortniteReplayReader](https://github.com/xNocken/replay-reader) library.
Recent Fortnite replays use Oodle compression. If parsing fails, check `%APPDATA%\ChestCounter\log.txt`
and the FortniteReplayReader README for the Oodle library it needs next to the exe.

Every parse logs the names and sizes of the data collections found in the replay.
If the chest count is wrong or zero, that log shows which collection holds chest opens,
and `ChestParser.cs` is the only file to change.

## Data
Everything is stored locally in `%APPDATA%\ChestCounter` (`settings.json`, `history.json`, `log.txt`).

## License
MIT. Not affiliated with or endorsed by Epic Games.
