Get-NbaResults | Show-NbaScoreboard
Get-NbaResults | Where-Object { $_.Status -eq [NbaData.PowerShell.Contracts.MatchStatus]::Finished } | Show-NbaScoreboard