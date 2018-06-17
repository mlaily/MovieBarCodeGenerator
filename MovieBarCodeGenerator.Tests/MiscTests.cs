using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MovieBarCodeGenerator.Core;

namespace MovieBarCodeGenerator.Tests
{
    [TestClass]
    public class MiscTests
    {
        [DataRow(0, "0")]
        [DataRow(-0, "0")]
        [DataRow(1, "1")]
        [DataRow(-1, "-1")]
        [DataRow(5, "5")]
        [DataRow(10, "10")]
        [DataRow(100, "100")]
        [DataRow(1000, "1000")]
        [DataRow(10000, "10000")]
        [DataRow(100000, "100000")]
        [DataRow(1000000, "1000000")]
        [DataRow(1000000000, "1000000000")]
        [DataRow(9999999999, "9999999999")]
        [DataRow(-9999999999, "-9999999999")]
        [DataRow(0.1, "0.1")]
        [DataRow(0.11, "0.11")]
        [DataRow(0.1000001, "0.1000001")]
        [DataRow(0.000000000000001, "0.000000000000001")]
        [DataRow(0.000000000000009, "0.000000000000009")]
        [DataRow(-0.000000000000001, "-0.000000000000001")]
        [DataRow(-0.000000000000009, "-0.000000000000009")]
        [DataRow(1.00000000000001, "1.00000000000001")]
        [DataRow(9.00000000000001, "9.00000000000001")]
        [DataRow(10.0000000000001, "10.0000000000001")]
        [DataRow(100000000.000001, "100000000.000001")]
        [DataRow(-100000000.000001, "-100000000.000001")]
        [DataRow(-9.00000000000001, "-9.00000000000001")]
        [DataRow(0.0000000000000001, "0")] // 16 decimals. athe default precision of a double is 15 digits.
        [DataRow(1.000000000000001, "1")] // same
        [DataRow(100000000.0000001, "100000000")] // same
        [TestMethod]
        public void ToInvariantString_Returns_Expected_Values(double input, string expectedOutput)
        {
            var result = Utils.ToInvariantString(input);
            Assert.AreEqual(expectedOutput, result);
        }
    }
}
