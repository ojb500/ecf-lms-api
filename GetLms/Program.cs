using LmsApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GetLms
{
    class Program
    {
        static void Main(string[] args)
        {
            var api = new Api(28757);
            //var fixt = api.GetMatches(" Division 2 - Weston Trophy");

            //foreach (var f in fixt)
            //{
            //    Console.WriteLine(f);
            //}
            var tbl = api.GetTable(" Divison 3 - Batley-Meek Memorial Trophy");
            var summ = tbl.SummarizeFor("Roth").ToList();

            var club = api.GetClub("Rotherham", " Divison 3 - Batley-Meek Memorial Trophy", " Division 2 - Weston Trophy", "Sam Haystead Memorial Trophy (Richardson Plate)", "Richardson Cup");

            var now = DateTime.Now;
            var (recents, upcoming) = club.GetRecent(now);
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
