using MovieBarCodeGenerator.Core;
using NUnit.Framework;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class MiscTests
{
    [TestCase(0, "0")]
    [TestCase(-0, "0")]
    [TestCase(1, "1")]
    [TestCase(-1, "-1")]
    [TestCase(5, "5")]
    [TestCase(10, "10")]
    [TestCase(100, "100")]
    [TestCase(1000, "1000")]
    [TestCase(10000, "10000")]
    [TestCase(100000, "100000")]
    [TestCase(1000000, "1000000")]
    [TestCase(1000000000, "1000000000")]
    [TestCase(9999999999, "9999999999")]
    [TestCase(-9999999999, "-9999999999")]
    [TestCase(0.1, "0.1")]
    [TestCase(0.11, "0.11")]
    [TestCase(0.1000001, "0.1000001")]
    [TestCase(0.000000000000001, "0.000000000000001")]
    [TestCase(0.000000000000009, "0.000000000000009")]
    [TestCase(-0.000000000000001, "-0.000000000000001")]
    [TestCase(-0.000000000000009, "-0.000000000000009")]
    [TestCase(1.00000000000001, "1.00000000000001")]
    [TestCase(9.00000000000001, "9.00000000000001")]
    [TestCase(10.0000000000001, "10.0000000000001")]
    [TestCase(100000000.000001, "100000000.000001")]
    [TestCase(-100000000.000001, "-100000000.000001")]
    [TestCase(-9.00000000000001, "-9.00000000000001")]
    [TestCase(0.0000000000000001, "0")] // 16 decimals. athe default precision of a double is 15 digits.
    [TestCase(1.000000000000001, "1")] // same
    [TestCase(100000000.0000001, "100000000")] // same
    [Test]
    public void ToInvariantString_Returns_Expected_Values(double input, string expectedOutput)
    {
        var result = Utils.ToInvariantString(input);
        Assert.AreEqual(expectedOutput, result);
    }
}
