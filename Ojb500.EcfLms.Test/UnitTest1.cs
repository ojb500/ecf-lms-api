using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ojb500.EcfLms.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task GetSDCA()
        {
            IModel Source = new Api();

            var org = Source.GetOrganisation(613);
            var comp = org.GetCompetition("Div 2 - Weston Trophy");
            var evts = await comp.GetEventsAsync();
            var mcs = await comp.GetMatchesAsync();
            Assert.IsTrue(evts.Length > 0);
            Assert.IsTrue(mcs.Length > 0);
        }
    }
}
