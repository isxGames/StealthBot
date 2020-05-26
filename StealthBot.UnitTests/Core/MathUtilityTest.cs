using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StealthBot.Core;

namespace StealthBot.UnitTests.Core
{
    [TestClass]
    public class MathUtilityTest
    {
        private MathUtility _utility;

        [TestInitialize]
        public void UtilityTestInitialize()
        {
            _utility = new MathUtility();
        }

        [TestMethod]
        public void DistanceShouldCorrectlyCalculateDistanceBetweenPoints()
        {
            //arrange
            var x1 = 0;
            var y1 = 1980509;
            var z1 = -7851798;

            var x2 = -8598;
            var y2 = 0580980;
            var z2 = 0985098;

            var expected = Math.Sqrt(
                Math.Pow(x1 - x2, 2) +
                Math.Pow(y1 - y2, 2) +
                Math.Pow(z1 - z2, 2)
                );

            //act
            var distance = _utility.Distance(x1, y1, z1, x2, y2, z2);

            //assert
            Assert.AreEqual(expected, distance);
        }
    }
}
