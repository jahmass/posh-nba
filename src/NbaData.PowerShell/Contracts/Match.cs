using System;

namespace NbaData.PowerShell.Contracts
{
    public class Match
    {
        public DateTime DateTimeUtc { get; set; }
        public string VisitorShortName { get; set; }
        public int? VisitorScore { get; set; }
        public string HomeShortName { get; set; }
        public int? HomeScore { get; set; }
        public MatchStatus Status { get; set; }

        public bool HasResult => VisitorScore.HasValue && HomeScore.HasValue;
    }
}
