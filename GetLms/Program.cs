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
            var api = Api.Default.GetOrganisation(613);

            var d2 = api.GetCompetition("Div 2 - Weston Trophy");
            var evts = d2.GetEvents().ToList();

            var club = api.GetClub("Rotherham", d2);
            var (recent, upc) = (club.GetResults().Take(5).ToList(),
     club.GetUpcoming().Take(5).ToList());

            Console.WriteLine("Upcoming matches:");
            foreach (var f in upc)
            {
                Console.WriteLine(f);
            }
            Console.WriteLine("Recent matches:");
            foreach (var f in recent)
            {
                Console.WriteLine(f);
            }
        }
    }
}
