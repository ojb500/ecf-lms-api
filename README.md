# ecf-lms-api
C# .NET library for talking to the English Chess Federation League Management System.

## nuget

Feed URL: `https://pkgs.dev.azure.com/ojb500/ecf-lms-api/_packaging/ecflms-api/nuget/v3/index.json`

Package: `Ojb500.EcfLms`

## usage

```cs
using Ojb500.EcfLms;

var sheffieldChess = Api.Default.GetOrganisation(28757);
var division2 = sheffieldChess.GetCompetition(" Division 2 - Weston Trophy"); 
// Competition name needs to match the one on the ECF LMS site exactly, including leading/trailing whitespace...
var division3 = sheffieldChess.GetCompetition(" Divison 3 - Batley-Meek Memorial Trophy"); // ..and any mis-spellings

var table = division2.GetTable();

var club = new ClubInfo("Rotherham", division2, division3); // Pass all competitions you care about
foreach (var fixture in club.GetUpcoming().Take(5))
{
  Console.WriteLine(fixture);
}

```
