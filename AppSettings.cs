using System;
using System.IO;
using System.Text.Json;
using ChestCounter.Services;

namespace ChestCounter.Models;

public class AppSettings
{
    public string ReplayFolder { get; set; } = DefaultFolder();
    public int ChestsBefore { get; set; } = 0;
    public bool OverlayEnabled { get; set; } = true;
    public int OverlayPort { get; set; } = 7373;
    public bool StartWithWindows { get; set; } = false;
    public string ProfileName { get; set; } = "";
    public DateTime FirstRun { get; set; } = DateTime.Now;

    public static string DefaultFolder() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FortniteGame", "Saved", "Demos");

    private static string FilePath => Path.Combine(Log.Dir, "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new AppSettings();
        }
        catch (Exception ex) { Log.Write("Settings load failed: " + ex.Message); }

        var s = new AppSettings();
        s.Save();
        return s;
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Log.Dir);
            File.WriteAllText(FilePath,
                JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex) { Log.Write("Settings save failed: " + ex.Message); }
    }
}
