using Ojb500.EcfLms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GetLms
{
    class Program
    {
        static void Main(string[] args)
        {
            var api = Api.Default.GetOrganisation(28757);
            //var fixt = api.GetMatches(" Division 2 - Weston Trophy");

            //foreach (var f in fixt)
            //{
            //    Console.WriteLine(f);
            //}
            var tbl = api.GetTable(" Divison 3 - Batley-Meek Memorial Trophy");
            var summ = tbl.SummarizeFor("Roth");

            var club = api.GetClub("Rotherham", " Divison 3 - Batley-Meek Memorial Trophy", " Division 2 - Weston Trophy", "Sam Haystead Memorial Trophy (Richardson Plate)", "Richardson Cup");

            var now = DateTime.Now;
            var (recents, upcoming) = (club.GetResults(now).Take(5), club.GetUpcoming(now).Take(5));
            Console.WriteLine("Upcoming matches:");
            foreach (var f in upcoming)
            {
                Console.WriteLine(f);
            }
            Console.WriteLine("Recent matches:");
            foreach (var f in recents)
            {
                Console.WriteLine(f);
            }
        }
    }
}
