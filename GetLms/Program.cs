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

            var a = new FileApi(".\\2019.3487432187");
            a.Update(Api.Default, 28757, getTables: true, Div2, Div3);
            //a.Update(Api.Default, 28757, getTables: false, Cup, Plate);


            //var a = Api.Default;
            var api = a.GetOrganisation(28757);
            //var fixt = api.GetMatches(" Division 2 - Weston Trophy");


            var d2 = api.GetCompetition(Div2);
            var d3 = api.GetCompetition(Div3);
            var evts = d2.GetEvents().ToList();

            var club = api.GetClub("Rotherham", d2, d3);
            var (recent, upc) = (club.GetResults().Take(5).ToList(),
     club.GetUpcoming().Take(5).ToList());
            var res = club.GetResults().ToList();
            var reg = new Registration(d2, 3);
            reg.Print("Rotherham A");

            foreach (var regClub in reg.GetRegisteredPlayers())
            {
                Console.WriteLine(regClub.Team);
                foreach (var regPlayer in regClub.Players)
                {
                    Console.WriteLine($" - {regPlayer}");
                }
            }

            //var club = api.GetClub("Rotherham", Div3, Div2, Plate, Cup);

            //var now = DateTime.Now;
            //var (recents, upcoming) = (club.GetResults(now).Take(5), club.GetUpcoming(now).Take(5));
            //Console.WriteLine("Upcoming matches:");
            //foreach (var f in upcoming)
            //{
            //    Console.WriteLine(f);
            //}
            //Console.WriteLine("Recent matches:");
            //foreach (var f in recents)
            //{
            //    Console.WriteLine(f);
            //}
        }
    }
}
