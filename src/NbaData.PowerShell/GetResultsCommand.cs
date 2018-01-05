using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using NbaData.PowerShell.Contracts;
using NbaData.PowerShell.Core;
using Newtonsoft.Json.Linq;

namespace NbaData.PowerShell
{
    [Cmdlet(VerbsCommon.Get, Constants.Nouns.Results)]
    [OutputType(typeof(IList<Match>))]
    public class GetResultsCommand : AsyncCmdlet
    {
        private const string MediaType = "application/json";

        [Parameter(ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Mandatory = false)]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        public string FormattedDate => Date.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo);

        private HttpClient _client;
        private string _requestUri;

        protected override Task BeginProcessingAsync(CancellationToken cancellationToken)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(Constants.NbaDataApi.BaseUrl);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaType));

            // Create the request URI with the parameter
            _requestUri = Constants.NbaDataApi.ScoreboardRequest.Replace(Constants.NbaDataApi.DateParameter, FormattedDate);

            return base.BeginProcessingAsync(cancellationToken);
        }

        protected override async Task ProcessRecordAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await _client.GetAsync(_requestUri, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(json);

                var matches = new List<Match>();

                foreach (var game in data["games"])
                {
                    DateTime startTimeUTC = (DateTime)game["startTimeUTC"];
                    string hTeamTriCode = (string)game["hTeam"]["triCode"];
                    string vTeamTriCode = (string)game["vTeam"]["triCode"];

                    Match match = new Match
                    {
                        DateTimeUtc = startTimeUTC,
                        HomeShortName = hTeamTriCode,
                        VisitorShortName = vTeamTriCode
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

                var results = from match in matches
                              orderby match.DateTimeUtc descending
                              select match;

                WriteObject(results.ToList());
            }
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
    }
}
