using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using FortniteReplayReader;
using Unreal.Core.Models.Enums;

namespace ChestCounter.Services;

public class ParseResult
{
    public int Chests { get; set; }
    public string Note { get; set; } = "";
}

/// <summary>
/// ALL replay-format knowledge lives in this one file.
/// If Fortnite changes its replay format and the count goes wrong, this is the only place to fix.
///
/// The parser reads the replay with FortniteReplayReader and counts chest-open events.
/// It also writes the names of every data collection it finds to %APPDATA%\ChestCounter\log.txt,
/// so you can see exactly what the library exposes for your replays.
/// </summary>
public static class ChestParser
{
    // Property names containing any of these words are treated as chest data.
    public static readonly string[] ChestKeywords = { "chest" };

    // Replay parsing is heavy; only one at a time.
    private static readonly object Gate = new();

    public static ParseResult Parse(string path)
    {
        lock (Gate)
        {
            var reader = new ReplayReader(parseMode: ParseMode.Full);
            var replay = reader.ReadReplay(path);
            var result = new ParseResult();

            var gameData = replay.GameData;
            if (gameData == null)
            {
                result.Note = "No game data in replay";
                return result;
            }

            int chests = 0;
            bool foundChestData = false;

            foreach (var prop in gameData.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                object value;
                try { value = prop.GetValue(gameData); } catch { continue; }

                if (value is not IEnumerable seq || value is string) continue;
                int count = seq.Cast<object>().Count();
                Log.Write($"  replay data: {prop.Name} = {count}");

                if (ChestKeywords.Any(k => prop.Name.Contains(k, StringComparison.OrdinalIgnoreCase)))
                {
                    foundChestData = true;
                    chests += count;
                }
            }

            // TODO (needs a real replay to verify): count only the recording player's own chest opens.
            // Once the right collection name is visible in log.txt, filter it here by the owner's ID.
            result.Chests = chests;
            result.Note = foundChestData ? "" : "Library exposed no chest data";
            return result;
        }
    }
}
