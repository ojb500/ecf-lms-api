using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ojb500.EcfLms.Test
{
    [TestClass]
    public class ScorePointsTests
    {
        [TestMethod]
        public void PointsParsesNegative()
        {
            Assert.IsTrue(Points.TryParse("-3".AsSpan(), out var pts, out _));
            Assert.AreEqual(-6, pts.PointsX2);
            Assert.AreEqual("-3", pts.ToString());
        }

        [TestMethod]
        public void PointsParsesNegativeHalf()
        {
            Assert.IsTrue(Points.TryParse("-½".AsSpan(), out var pts, out _));
            Assert.AreEqual(-1, pts.PointsX2);
            Assert.AreEqual("-½", pts.ToString());
        }

        [TestMethod]
        public void PointsParsesHalf()
        {
            Assert.IsTrue(Points.TryParse("½".AsSpan(), out var pts, out _));
            Assert.AreEqual(1, pts.PointsX2);
            Assert.AreEqual("½", pts.ToString());
        }

        [TestMethod]
        public void PointsParsesIntegerWithHalf()
        {
            Assert.IsTrue(Points.TryParse("4½".AsSpan(), out var pts, out _));
            Assert.AreEqual(9, pts.PointsX2);
            Assert.AreEqual("4½", pts.ToString());
        }

        [TestMethod]
        public void ScoreFromStandardFormat()
        {
            var s = new Score("3 - 2");
            Assert.AreEqual(6, s.Home.PointsX2);
            Assert.AreEqual(4, s.Away.PointsX2);
        }

        [TestMethod]
        public void ScoreFromHalfPoints()
        {
            var s = new Score("4½ - 1½");
            Assert.AreEqual(9, s.Home.PointsX2);
            Assert.AreEqual(3, s.Away.PointsX2);
        }
    }
}
