using System;

namespace ChestCounter.Models;

public class MatchRecord
{
    public string Id { get; set; } = "";          // replay file name
    public DateTime PlayedAt { get; set; }        // replay file last-write time (local)
    public DateTime AddedAt { get; set; }         // when the app recorded it
    public long SizeBytes { get; set; }
    public long WriteTicksUtc { get; set; }
    public int Chests { get; set; }
    public string Note { get; set; } = "";
}
