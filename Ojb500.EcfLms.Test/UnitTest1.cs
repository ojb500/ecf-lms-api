using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
            var evts = comp.GetEvents().ToArray();
            var mcs = comp.GetMatches().ToArray();
            Assert.IsTrue(evts.Length > 0);
            Assert.IsTrue(mcs.Length > 0);
        }
    }
}
