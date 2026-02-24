# ecf-lms-api
C# .NET library for talking to the English Chess Federation League Management System.

Package: `Ojb500.EcfLms`

## Usage

```csharp
using Ojb500.EcfLms;

// Connect to the ECF LMS for your organisation
var org = Api.Default.GetOrganisation(613); // e.g. Sheffield & District Chess Association

// --- League table ---
var div1 = org.GetCompetition("Div 1 - Davy Trophy");
var table = await div1.GetTableAsync();

foreach (var pos in table)
{
    Console.WriteLine($"{pos.Position}. {pos.Entry.Team.Name,-30} " +
                      $"P{pos.Entry.P} W{pos.Entry.W} D{pos.Entry.D} L{pos.Entry.L}  " +
                      $"Pts: {pos.Entry.Pts}");
}

// --- Summarised table for your club (top + neighbourhood) ---
var summary = table.SummarizeFor("Ecclesall", maxRows: 5);
foreach (var pos in summary)
{
    Console.WriteLine($"{pos.Position}. {pos.Entry.Team.Name} ({pos.Entry.Pts} pts)");
}

// --- Fixtures and results ---
var events = await div1.GetEventsAsync();
var upcoming = events.Where(e => e.DateTime > DateTime.Now).Take(5);
var results = events.Where(e => e.DateTime <= DateTime.Now && !e.Result.IsEmpty)
    .OrderByDescending(e => e.DateTime).Take(5);

// --- Detailed match cards with individual board pairings ---
foreach (var match in (await div1.GetMatchesAsync()).Take(3))
{
    Console.WriteLine($"\n{match.Left} v {match.Right}");
    foreach (var p in match.Pairings)
    {
        var colour = p.FirstPlayerWhite ? "W" : "B";
        Console.WriteLine($"  Bd {p.Board} ({colour}): " +
                          $"{p.FirstPlayer.FamilyName} ({p.FirstPlayer.Rating.Primary}) " +
                          $"{p.Result} " +
                          $"{p.SecondPlayer.FamilyName} ({p.SecondPlayer.Rating.Primary})");
    }
}

// --- All fixtures for a specific club (by club code) ---
var clubFixtures = await org.GetClubEventsAsync("8YSR");
foreach (var comp in clubFixtures)
{
    Console.WriteLine($"\n{comp.Title}:");
    foreach (var evt in comp.Events)
    {
        Console.WriteLine($"  {evt.Home} {evt.Result} {evt.Away}");
    }
}

// --- Browse seasons and competitions ---
var seasons = await org.GetSeasonsWithEventsAsync();
foreach (var (id, season) in seasons)
{
    Console.WriteLine($"\n{season.Name}:");
    foreach (var (eventId, name) in season.Events)
    {
        Console.WriteLine($"  {name} (id: {eventId})");
    }
}
```
