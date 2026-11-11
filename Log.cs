using System;
using System.IO;

namespace ChestCounter.Services;

public static class Log
{
    private static readonly object Gate = new();

    public static string Dir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ChestCounter");

    public static void Write(string message)
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(Dir);
                File.AppendAllText(Path.Combine(Dir, "log.txt"),
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {message}{Environment.NewLine}");
            }
        }
        catch { }
    }
}
