using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChestCounter.Models;

namespace ChestCounter.Services;

public class ReplayWatcher : IDisposable
{
    private readonly HistoryStore _store;
    private FileSystemWatcher _fsw;
    private readonly HashSet<string> _busy = new(StringComparer.OrdinalIgnoreCase);

    public ReplayWatcher(HistoryStore store) => _store = store;

    public bool Watching => _fsw != null;

    public void Start(string folder)
    {
        Stop();
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
        {
            Log.Write("Replay folder not found: " + folder);
            return;
        }

        try
        {
            _fsw = new FileSystemWatcher(folder, "*.replay")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };
            _fsw.Created += (_, e) => Queue(e.FullPath);
            _fsw.Changed += (_, e) => Queue(e.FullPath);
            _fsw.Renamed += (_, e) => Queue(e.FullPath);
            Log.Write("Watching " + folder);
        }
        catch (Exception ex)
        {
            Log.Write("Watcher failed: " + ex.Message);
            _fsw = null;
        }
    }

    public void Stop()
    {
        if (_fsw == null) return;
        _fsw.EnableRaisingEvents = false;
        _fsw.Dispose();
        _fsw = null;
    }

    private void Queue(string path)
    {
        string id = Path.GetFileName(path);
        lock (_busy)
        {
            if (!_busy.Add(id)) return;
        }

        Task.Run(async () =>
        {
            try
            {
                if (!await WaitUntilStable(path)) return;
                Process(path);
            }
            catch (Exception ex) { Log.Write($"Failed on {id}: {ex}"); }
            finally { lock (_busy) _busy.Remove(id); }
        });
    }

    /// <summary>Waits until Fortnite has finished writing the replay (untouched for 15s).</summary>
    private static async Task<bool> WaitUntilStable(string path)
    {
        long lastLength = -1;
        for (int i = 0; i < 180; i++)
        {
            try
            {
                var fi = new FileInfo(path);
                if (!fi.Exists) return false;
                bool idle = (DateTime.UtcNow - fi.LastWriteTimeUtc).TotalSeconds >= 15;
                if (idle && fi.Length > 0 && fi.Length == lastLength)
                {
                    using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    return true;
                }
                lastLength = fi.Length;
            }
            catch (IOException) { }
            await Task.Delay(5000);
        }
        return false;
    }

    /// <summary>Parses one replay and stores it. Returns true if a new match was recorded.</summary>
    private bool Process(string path)
    {
        var fi = new FileInfo(path);
        string id = fi.Name;
        if (_store.Has(id, fi.Length, fi.LastWriteTimeUtc.Ticks)) return false;

        Log.Write("Parsing " + id);
        var result = ChestParser.Parse(path);
        _store.Add(new MatchRecord
        {
            Id = id,
            PlayedAt = fi.LastWriteTime,
            AddedAt = DateTime.Now,
            SizeBytes = fi.Length,
            WriteTicksUtc = fi.LastWriteTimeUtc.Ticks,
            Chests = result.Chests,
            Note = result.Note
        });
        Log.Write($"{id}: {result.Chests} chests {result.Note}");
        return true;
    }

    /// <summary>Imports every replay in the folder that hasn't been counted yet.</summary>
    public int ImportExisting(string folder, IProgress<string> progress)
    {
        if (!Directory.Exists(folder)) { progress?.Report("Folder not found"); return 0; }

        var files = new DirectoryInfo(folder).GetFiles("*.replay").OrderBy(f => f.LastWriteTime).ToList();
        int added = 0, i = 0;
        foreach (var f in files)
        {
            i++;
            progress?.Report($"Reading {i} of {files.Count}");
            try { if (Process(f.FullName)) added++; }
            catch (Exception ex) { Log.Write($"Import failed on {f.Name}: {ex.Message}"); }
        }
        progress?.Report($"Imported {added} new match(es)");
        return added;
    }

    public void Dispose() => Stop();
}
