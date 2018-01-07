using NbaData.PowerShell.Contracts;
using NbaData.PowerShell.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace NbaData.PowerShell
{
    [CmdletBinding(DefaultParameterSetName = "Date")]
    [Cmdlet(VerbsCommon.Get, Constants.Nouns.Results)]
    [OutputType(typeof(IList<Match>))]
    public class GetResultsCommand : AsyncCmdlet
    {
        [Parameter(ParameterSetName = "Date", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;

        [Parameter(ParameterSetName = "Range", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        public DateTime? StartingDate { get; set; }

        [Parameter(ParameterSetName = "Range", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        public DateTime? EndingDate { get; set; }

        [Parameter()]
        public MatchStatus? Status { get; set; }

        private HttpClient _client;
        private List<Match> _results = new List<Match>();

        protected override Task BeginProcessingAsync(CancellationToken cancellationToken)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(Constants.NbaDataApi.BaseUrl);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.NbaDataApi.MediaType));

            return base.BeginProcessingAsync(cancellationToken);
        }

        protected override async Task ProcessRecordAsync(CancellationToken cancellationToken)
        {
            Queue<HttpResponseMessage> dates = new Queue<HttpResponseMessage>();

            if (StartingDate.HasValue && EndingDate.HasValue)
            {
                var d = StartingDate.Value.Date;

                do
                {
                    dates.Enqueue(await _client.GetAsync(Constants.NbaDataApi.ScoreboardRequest.Replace(Constants.NbaDataApi.DateParameter, GetFormattedDate(d)), cancellationToken));
                    d = d + TimeSpan.FromDays(1);
                } while (d != EndingDate.Value.Date);
            }
            else
            {
                dates.Enqueue(await _client.GetAsync(Constants.NbaDataApi.ScoreboardRequest.Replace(Constants.NbaDataApi.DateParameter, GetFormattedDate(Date)), cancellationToken));
            }

            do
            {
                var response = dates.Dequeue();
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(json);

                    var matches = new List<Match>();

                    foreach (var game in data["games"])
                    {
                        if (Status.HasValue && (int)game["statusNum"] != (int)Status) continue;

                        Match match = new Match
                        {
                            Status = (MatchStatus)(int)game["statusNum"],
                            DateTimeUtc = (DateTime)game["startTimeUTC"],
                            HomeShortName = (string)game["hTeam"]["triCode"],
                            VisitorShortName = (string)game["vTeam"]["triCode"]
                        };

                        if (int.TryParse((string)game["hTeam"]["score"], out int hTeamScore))
                        {
                            match.HomeScore = hTeamScore;
                        }
                        else
                        {
                            match.HomeScore = null;
                        }

                        if (int.TryParse((string)game["vTeam"]["score"], out int vTeamScore))
                        {
                            match.VisitorScore = vTeamScore;
                        }
                        else
                        {
                            match.VisitorScore = null;
                        }

                        matches.Add(match);
                    }

                    _results.AddRange(matches);

                }
            } while (dates.Count > 0);

            WriteObject((from match in _results
                         orderby match.DateTimeUtc ascending
                         select match).ToList());
        }

        protected override Task EndProcessingAsync(CancellationToken cancellationToken)
        {
            if (_client != null) _client.Dispose();

            return base.EndProcessingAsync(cancellationToken);
        }

        protected override Task StopProcessingAsync(CancellationToken cancellationToken)
        {
            if (_client != null) _client.CancelPendingRequests();

            return base.StopProcessingAsync(cancellationToken);
        }
        private string GetFormattedDate(DateTime date)
        {
            return date.ToString(Constants.NbaDataApi.DateParameterFormat, DateTimeFormatInfo.InvariantInfo);
        }
    }
}
