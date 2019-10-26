# ecf-lms-api
C# .NET library for talking to the English Chess Federation League Management System.

I use this with [Wyam](https://wyam.io) to generate the website for [Rotherham Chess Club](https://rotherham.cc)

[![Build Status](https://dev.azure.com/ojb500/ecf-lms-api/_apis/build/status/ojb500.ecf-lms-api?branchName=master)](https://dev.azure.com/ojb500/ecf-lms-api/_build/latest?definitionId=3&branchName=master)

## nuget

Feed URL: `https://pkgs.dev.azure.com/ojb500/ecf-lms-api/_packaging/ecflms-api/nuget/v3/index.json`

Package: `Ojb500.EcfLms`

## usage

```cs
using Ojb500.EcfLms;

var sheffieldChess = Api.Default.GetOrganisation(28757);
string division2 = " Division 2 - Weston Trophy"; // Competition name needs to match the one on the ECF LMS site exactly, including leading/trailing whitespace...
string division3 = " Divison 3 - Batley-Meek Memorial Trophy"; // ..and any mis-spellings
var table = sheffieldChess.GetTable(division2);

var club = sheffieldChess.GetClub("Rotherham", division2, division3); // Pass all competitions you care about
foreach (var fixture in club.GetUpcoming().Take(5))
{
  Console.WriteLine(fixture);
}

```
