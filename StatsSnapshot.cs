using System.Collections.Generic;

namespace ChestCounter.Models;

public class DayBar
{
    public string Label { get; set; } = "";
    public int Value { get; set; }
    public double Height { get; set; }
    public bool IsToday { get; set; }
}

public class StatsSnapshot
{
    public int Total { get; set; }
    public int LastGame { get; set; }
    public int GamesTotal { get; set; }
    public int SessionChests { get; set; }
    public int SessionGames { get; set; }
    public double SessionAvg { get; set; }
    public double AvgPerGame { get; set; }
    public int BestGame { get; set; }
    public int Today { get; set; }
    public int BestDay { get; set; }
    public string BestDayLabel { get; set; } = "Best day";
    public int SinceInstall { get; set; }
    public string SinceLabel { get; set; } = "Since install";
    public string LastEvent { get; set; } = "";
    public string ProfileLabel { get; set; } = "";
    public List<DayBar> Last7 { get; set; } = new();
}
