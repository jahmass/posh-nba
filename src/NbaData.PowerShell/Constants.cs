namespace NbaData.PowerShell
{
    public static class Constants
    {
        public class Nouns
        {
            private const string Prefix = "Nba";
            public const string Results = Prefix + "Results";
            public const string Scoreboard = Prefix + "Scoreboard";
        }

        public class NbaDataApi
        {
            public const string BaseUrl = "http://data.nba.net/";
            // Parameter date  has the format: yearmonthday => 20171231
            public const string DateParameter = "{{date}}";
            public const string ScoreboardRequest = "data/10s/prod/v1/" + DateParameter + "/scoreboard.json";
        }
    }
}