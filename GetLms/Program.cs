using Ojb500.EcfLms;
using System;
using System.Linq;

namespace GetLms
{
    class Program
    {
        static void Main(string[] args)
        {
            var org = Api.Default.GetOrganisation(613);

            var d2 = org.GetCompetition("Div 2 - Weston Trophy");
            var evts = d2.GetEvents().ToList();

            var now = DateTime.Now;
            var recent = evts.Where(e => e.DateTime <= now && !e.Result.IsEmpty)
                .OrderByDescending(e => e.DateTime).Take(5);
            var upcoming = evts.Where(e => e.DateTime > now).Take(5);

            Console.WriteLine("Upcoming matches:");
            foreach (var f in upcoming)
            {
                Console.WriteLine(f);
            }
            Console.WriteLine("Recent results:");
            foreach (var f in recent)
            {
                Console.WriteLine(f);
            }
        }
    }
}
