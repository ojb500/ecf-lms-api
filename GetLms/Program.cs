using Ojb500.EcfLms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GetLms
{
    class Program
    {
        private const string Div3 = " Divison 3 - Batley-Meek Memorial Trophy";
        private const string Div2 = " Division 2 - Weston Trophy";
        private const string Plate = "Sam Haystead Memorial Trophy (Richardson Plate)";
        private const string Cup = "Richardson Cup";

        static void Main(string[] args)
        {
            var api = Api.Default.GetOrganisation(28757);
            //var fixt = api.GetMatches(" Division 2 - Weston Trophy");

      
            var d3 = api.GetMatches(Div3).ToList();

            var club = api.GetClub("Rotherham", Div3, Div2, Plate, Cup);

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
