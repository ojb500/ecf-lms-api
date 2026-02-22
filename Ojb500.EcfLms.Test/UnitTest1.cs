using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Ojb500.EcfLms.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void GetSDCA()
        {
            IModel Source = new Api();

            var org = Source.GetOrganisation(613);
            var comp = org.GetCompetition("Div 2 - Weston Trophy");
            var roth = org.GetClub("Rotherham", comp);
            var upc = roth.GetUpcoming().ToArray();
            var mcs = comp.GetMatches().ToArray();
            var res = roth.GetResults().ToArray();
            Assert.IsTrue(upc.Length + res.Length > 0);
            Assert.IsTrue(mcs.Length > 0);
        }
    }
}
