using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ChestCounter.Models;

namespace ChestCounter.Services;

public class HistoryStore
{
    private readonly object _lock = new();
    private List<MatchRecord> _matches = new();

    public DateTime SessionStart { get; } = DateTime.Now;
    public event Action Changed;

    private static string FilePath => Path.Combine(Log.Dir, "history.json");

    public void Load()
    {
        try
        {
            if (File.Exists(FilePath))
                _matches = JsonSerializer.Deserialize<List<MatchRecord>>(File.ReadAllText(FilePath)) ?? new();
        }
        catch (Exception ex) { Log.Write("History load failed: " + ex.Message); }
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Log.Dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(_matches));
        }
        catch (Exception ex) { Log.Write("History save failed: " + ex.Message); }
    }

    /// <summary>True if this replay was already counted (same name, or same size+timestamp after a rename).</summary>
    public bool Has(string id, long size, long writeTicksUtc)
    {
        lock (_lock)
            return _matches.Any(m => m.Id == id || (m.SizeBytes == size && m.WriteTicksUtc == writeTicksUtc));
    }

    public void Add(MatchRecord record)
    {
        lock (_lock)
        {
            if (_matches.Any(m => m.Id == record.Id)) return;
            _matches.Add(record);
            Save();
        }
        Changed?.Invoke();
    }

    public List<MatchRecord> All()
    {
        lock (_lock) return _matches.OrderByDescending(m => m.PlayedAt).ToList();
    }

    public void Clear()
    {
        lock (_lock) { _matches.Clear(); Save(); }
        Changed?.Invoke();
    }

    public StatsSnapshot Snapshot(AppSettings s)
    {
        List<MatchRecord> m;
        lock (_lock) m = _matches.ToList();

        var today = DateTime.Today;
        var snap = new StatsSnapshot();
        int sum = m.Sum(x => x.Chests);

        snap.Total = s.ChestsBefore + sum;
        snap.GamesTotal = m.Count;

        var last = m.OrderByDescending(x => x.PlayedAt).FirstOrDefault();
        snap.LastGame = last?.Chests ?? 0;
        snap.LastEvent = last == null ? "Waiting for your first match" : $"{last.Chests} chests opened";

        var sess = m.Where(x => x.AddedAt >= SessionStart).ToList();
        snap.SessionChests = sess.Sum(x => x.Chests);
        snap.SessionGames = sess.Count;
        snap.SessionAvg = sess.Count > 0 ? Math.Round((double)snap.SessionChests / sess.Count, 1) : 0;

        snap.AvgPerGame = m.Count > 0 ? Math.Round((double)sum / m.Count, 1) : 0;
        snap.BestGame = m.Count > 0 ? m.Max(x => x.Chests) : 0;
        snap.Today = m.Where(x => x.PlayedAt.Date == today).Sum(x => x.Chests);

        var days = m.GroupBy(x => x.PlayedAt.Date)
                    .Select(g => new { Day = g.Key, Value = g.Sum(x => x.Chests) })
                    .ToList();
        var bestDay = days.OrderByDescending(d => d.Value).FirstOrDefault();
        snap.BestDay = bestDay?.Value ?? 0;
        snap.BestDayLabel = bestDay == null ? "Best day" : $"Best day {bestDay.Day:dd/MM}";

        snap.SinceInstall = m.Where(x => x.PlayedAt.Date >= s.FirstRun.Date).Sum(x => x.Chests);
        snap.SinceLabel = $"Since {s.FirstRun:dd/MM/yy}";
        snap.ProfileLabel = string.IsNullOrWhiteSpace(s.ProfileName) ? "" : "Profile: " + s.ProfileName;

        var bars = new List<DayBar>();
        for (int i = 6; i >= 0; i--)
        {
            var d = today.AddDays(-i);
            int v = days.Where(x => x.Day == d).Sum(x => x.Value);
            bars.Add(new DayBar { Label = d.ToString("ddd"), Value = v, IsToday = i == 0 });
        }
        int max = Math.Max(1, bars.Max(b => b.Value));
        foreach (var b in bars) b.Height = 4 + 86.0 * b.Value / max;
        snap.Last7 = bars;

        return snap;
    }
}
