// ***********************************************************************
// Copyright (c) 2009 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using NUnit.Framework.Internal;
using NUnit.TestUtilities;
using NUnit.TestData.TheoryFixture;
using NUnit.Framework.Api;

namespace NUnit.Framework.Tests
{
    public class TheoryTests
    {
        static readonly Type fixtureType = typeof(TheoryFixture);

        [Test]
        public void TheoryWithNoArgumentsIsTreatedAsTest()
        {
            TestAssert.IsRunnable(fixtureType, "TheoryWithNoArguments", ResultState.Success);
        }

        [Test]
        public void TheoryWithNoDatapointsIsNotRunnable()
        {
            TestAssert.IsNotRunnable(fixtureType, "TheoryWithArgumentsButNoDatapoints");
        }

        [Test]
        public void TheoryWithDatapointsIsRunnable()
        {
            Test test = TestBuilder.MakeTestCase(fixtureType, "TheoryWithArgumentsAndDatapoints");
            TestAssert.IsRunnable(test);
            Assert.That(test.TestCaseCount, Is.EqualTo(9));
        }

        [Test]
        public void BooleanArgumentsAreSuppliedAutomatically()
        {
            Test test = TestBuilder.MakeTestCase(fixtureType, "TestWithBooleanArguments");
            TestAssert.IsRunnable(test);
            Assert.That(test.TestCaseCount, Is.EqualTo(4));
        }

        [Datapoint]
        internal object nullObj = null;

        [Theory]
        public void NullDatapointIsOK(object o)
        {
            Assert.Null(o);
            Assert.Null(nullObj); // to avoid a warning
        }


        [Test]
        public void EnumArgumentsAreSuppliedAutomatically()
        {
            Test test = TestBuilder.MakeTestCase(fixtureType, "TestWithEnumAsArgument");
            TestAssert.IsRunnable(test);
#if CLR_2_0 || CLR_4_0
            Assert.That(test.TestCaseCount, Is.EqualTo(16));
#else
            Assert.That(test.TestCaseCount, Is.EqualTo(15)); // No GenericParameter member
#endif
            Assert.That(test.TestCaseCount, Is.EqualTo(TypeHelper.GetEnumValues(typeof(AttributeTargets)).Length));
        }

        [Theory]
        public void SquareRootWithAllGoodValues(
            [Values(12.0, 4.0, 9.0)] double d)
        {
            SquareRootTest(d);
        }

        [Theory]
        public void SquareRootWithOneBadValue(
            [Values(12.0, -4.0, 9.0)] double d)
        {
            SquareRootTest(d);
        }

        [Datapoints]
        internal string[] vals = new string[] { "xyz1", "xyz2", "xyz3" };

        [Theory]
        public void ArrayWithDatapointsAttributeIsUsed(string s)
        {
            Assert.That(s.StartsWith("xyz"));
        }

        private static void SquareRootTest(double d)
        {
            Assume.That(d > 0);
            double root = Math.Sqrt(d);
            Assert.That(root * root, Is.EqualTo(d).Within(0.000001));
            Assert.That(root > 0);
        }

        [Test]
        public void SimpleTestIgnoresDataPoints()
        {
            Test test = TestBuilder.MakeTestCase(fixtureType, "TestWithArguments");
            Assert.That(test.TestCaseCount, Is.EqualTo(2));
        }

        [Theory]
        public void TheoryFailsIfAllTestsAreInconclusive()
        {
            ITestResult result = TestBuilder.RunTestCase(fixtureType, "TestWithAllBadValues");
            Assert.That(result.ResultState, Is.EqualTo(ResultState.Failure));
            Assert.That(result.Message, Is.EqualTo("All test cases were inconclusive"));
        }

        public class SqrtTests
        {
            [Datapoint]
            public double zero = 0;

            [Datapoint]
            public double positive = 1;

            [Datapoint]
            public double negative = -1;

            [Datapoint]
            public double max = double.MaxValue;

            [Datapoint]
            public double infinity = double.PositiveInfinity;

            [Theory]
            public void SqrtTimesItselfGivesOriginal(double num)
            {
                Assume.That(num >= 0.0 && num < double.MaxValue);

                double sqrt = Math.Sqrt(num);

                Assert.That(sqrt >= 0.0);
                Assert.That(sqrt * sqrt, Is.EqualTo(num).Within(0.000001));
            }
        }
    }
}
