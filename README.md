# ecf-lms-api
C# .NET library for talking to the English Chess Federation League Management System.

Package: `Ojb500.EcfLms`

## Usage

```csharp
using Ojb500.EcfLms;

// Connect to the ECF LMS for your organisation
var org = Api.Default.GetOrganisation(28757); // e.g. Sheffield & District Chess Association

// --- League table ---
var div1 = org.GetCompetition(" Division 1 - Davy Trophy");
var table = div1.GetTable();

foreach (var pos in table)
{
    Console.WriteLine($"{pos.Position}. {pos.Entry.Team.Name,-30} " +
                      $"P{pos.Entry.P} W{pos.Entry.W} D{pos.Entry.D} L{pos.Entry.L}  " +
                      $"Pts: {pos.Entry.Pts}");
}

// --- Summarised table for your club (top + neighbourhood) ---
var summary = table.SummarizeFor("Rotherham", maxRows: 5);
foreach (var pos in summary)
{
    Console.WriteLine($"{pos.Position}. {pos.Entry.Team.Name} ({pos.Entry.Pts} pts)");
}

// --- Fixtures and results ---
var events = div1.GetEvents();
var upcoming = events.Where(e => e.DateTime > DateTime.Now).Take(5);
var results = events.Where(e => e.DateTime <= DateTime.Now && !e.Result.IsEmpty)
    .OrderByDescending(e => e.DateTime).Take(5);

// --- Detailed match cards with individual board pairings ---
foreach (var match in div1.GetMatches().Take(3))
{
    Console.WriteLine($"\n{match.Left} v {match.Right}");
    foreach (var p in match.Pairings)
    {
        var colour = p.FirstPlayerWhite ? "W" : "B";
        Console.WriteLine($"  Bd {p.Board} ({colour}): " +
                          $"{p.FirstPlayer.FamilyName} ({p.FirstPlayer.Rating.Primary}) " +
                          $"{p.Result.Result} " +
                          $"{p.SecondPlayer.FamilyName} ({p.SecondPlayer.Rating.Primary})");
    }
}

// --- All fixtures for a specific club (by club code) ---
var clubFixtures = org.GetClubEvents("ROT");
foreach (var comp in clubFixtures)
{
    Console.WriteLine($"\n{comp.Title}:");
    foreach (var evt in comp.Events)
    {
        Console.WriteLine($"  {evt.Home} {evt.Result} {evt.Away}");
    }
}

// --- Browse seasons and competitions ---
var seasons = org.GetSeasonsWithEvents();
foreach (var (id, season) in seasons)
{
    Console.WriteLine($"\n{season.Name}:");
    foreach (var (eventId, name) in season.Events)
    {
        Console.WriteLine($"  {name} (id: {eventId})");
    }
}
```
