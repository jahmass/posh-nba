using NbaData.PowerShell.Contracts;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace NbaData.PowerShell
{
    [Cmdlet(VerbsCommon.Show, Constants.Nouns.Scoreboard)]
    public class ShowScoreboardCommand : PSCmdlet
    {
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ValueFromRemainingArguments = true, Mandatory = true)]
        public IList<Match> Matches { get; set; }

        private ConsoleColor _defaultForegroundColor;
        private ConsoleColor _defaultBackgroundColor;

        protected override void BeginProcessing()
        {
            _defaultBackgroundColor = Host.UI.RawUI.BackgroundColor;
            _defaultForegroundColor = Host.UI.RawUI.ForegroundColor;

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (Matches != null && Matches.Count > 0)
            {
                foreach (var match in Matches)
                {
                    if (match.HasResult)
                    {
                        Host.UI.Write((match.VisitorScore > match.HomeScore) ? ConsoleColor.Green : ConsoleColor.Red, _defaultBackgroundColor, match.VisitorShortName);
                        Host.UI.Write(_defaultForegroundColor, _defaultBackgroundColor, $" : {match.VisitorScore.Value.ToString().PadLeft(3, ' ')} - {match.HomeScore.Value.ToString().PadRight(3, ' ')} : ");
                        Host.UI.Write((match.HomeScore > match.VisitorScore) ? ConsoleColor.Green : ConsoleColor.Red, _defaultBackgroundColor, match.HomeShortName);
                    }
                    else
                    {
                        Host.UI.Write($"{match.VisitorShortName} : TBD - TBD : {match.HomeShortName}");
                    }

                    // New line
                    Host.UI.WriteLine();
                }
            }
        }
    }
}
