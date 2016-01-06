// ***********************************************************************
// Copyright (c) 2008 Charlie Poole
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
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnit.TestData.TestCaseAttributeFixture;
using NUnit.TestUtilities;
using System.Collections;

namespace NUnit.Framework.Tests
{
    [TestFixture]
    public class TestCaseAttributeTests
    {
        [TestCase(12, 3, 4)]
        [TestCase(12, 2, 6)]
        [TestCase(12, 4, 3)]
        [TestCase(12, 0, 0, ExpectedException = typeof(System.DivideByZeroException))]
        [TestCase(12, 0, 0, ExpectedExceptionName = "System.DivideByZeroException")]
        public void IntegerDivisionWithResultPassedToTest(int n, int d, int q)
        {
            Assert.AreEqual(q, n / d);
        }

        [TestCase(12, 3, ExpectedResult = 4)]
        [TestCase(12, 2, ExpectedResult = 6)]
        [TestCase(12, 4, ExpectedResult = 3)]
        [TestCase(12, 0, ExpectedException = typeof(System.DivideByZeroException))]
        [TestCase(12, 0, ExpectedExceptionName = "System.DivideByZeroException",
            TestName = "DivisionByZeroThrowsException")]
        public int IntegerDivisionWithResultCheckedByNUnit(int n, int d)
        {
            return n / d;
        }

        [TestCase(2, 2, ExpectedResult=4)]
        public double CanConvertIntToDouble(double x, double y)
        {
            return x + y;
        }

        [TestCase("2.2", "3.3", ExpectedResult = 5.5)]
        public decimal CanConvertStringToDecimal(decimal x, decimal y)
        {
            return x + y;
        }

        [TestCase(2.2, 3.3, ExpectedResult = 5.5)]
        public decimal CanConvertDoubleToDecimal(decimal x, decimal y)
        {
            return x + y;
        }

        [TestCase(5, 2, ExpectedResult = 7)]
        public decimal CanConvertIntToDecimal(decimal x, decimal y)
        {
            return x + y;
        }

        [TestCase(5, 2, ExpectedResult = 7)]
        public short CanConvertSmallIntsToShort(short x, short y)
        {
            return (short)(x + y);
        }

        [TestCase(5, 2, ExpectedResult = 7)]
        public byte CanConvertSmallIntsToByte(byte x, byte y)
        {
            return (byte)(x + y);
        }

        [TestCase(5, 2, ExpectedResult = 7)]
        public sbyte CanConvertSmallIntsToSByte(sbyte x, sbyte y)
        {
            return (sbyte)(x + y);
        }

        [Test]
		public void ConversionOverflowMakesTestNotRunnable()
		{
			Test test = (Test)TestBuilder.MakeParameterizedMethodSuite(
				typeof(TestCaseAttributeFixture), "MethodCausesConversionOverflow").Tests[0];
			Assert.AreEqual(RunState.NotRunnable, test.RunState);
		}

        [TestCase("12-October-1942")]
        public void CanConvertStringToDateTime(DateTime dt)
        {
            Assert.AreEqual(1942, dt.Year);
        }

        [TestCase(42, ExpectedException = typeof(System.Exception),
                   ExpectedMessage = "Test Exception")]
        public void CanSpecifyExceptionMessage(int a)
        {
            throw new System.Exception("Test Exception");
        }

        [TestCase(42, ExpectedException = typeof(System.Exception),
           ExpectedMessage = "Test Exception",
           MatchType=MessageMatch.StartsWith)]
        public void CanSpecifyExceptionMessageAndMatchType(int a)
        {
            throw new System.Exception("Test Exception thrown here");
        }

#if CLR_2_0 || CLR_4_0
        [TestCase(null)]
        public void CanPassNullAsFirstArgument(object a)
        {
        	Assert.IsNull(a);
        }
#endif

        [TestCase(new object[] { 1, "two", 3.0 })]
        [TestCase(new object[] { "zip" })]
        public void CanPassObjectArrayAsFirstArgument(object[] a)
        {
        }
  
        [TestCase(new object[] { "a", "b" })]
        public void CanPassArrayAsArgument(object[] array)
        {
            Assert.AreEqual("a", array[0]);
            Assert.AreEqual("b", array[1]);
        }

        [TestCase("a", "b")]
        public void ArgumentsAreCoalescedInObjectArray(object[] array)
        {
            Assert.AreEqual("a", array[0]);
            Assert.AreEqual("b", array[1]);
        }

        [TestCase(1, "b")]
        public void ArgumentsOfDifferentTypeAreCoalescedInObjectArray(object[] array)
        {
            Assert.AreEqual(1, array[0]);
            Assert.AreEqual("b", array[1]);
        }

#if CLR_2_0 || CLR_4_0
        [TestCase(ExpectedResult = null)]
        public object ResultCanBeNull()
        {
            return null;
        }
#endif

        [TestCase("a", "b")]
        public void HandlesParamsArrayAsSoleArgument(params string[] array)
        {
            Assert.AreEqual("a", array[0]);
            Assert.AreEqual("b", array[1]);
        }

        [TestCase("a")]
        public void HandlesParamsArrayWithOneItemAsSoleArgument(params string[] array)
        {
            Assert.AreEqual("a", array[0]);
        }

        [TestCase("a", "b", "c", "d")]
        public void HandlesParamsArrayAsLastArgument(string s1, string s2, params object[] array)
        {
            Assert.AreEqual("a", s1);
            Assert.AreEqual("b", s2);
            Assert.AreEqual("c", array[0]);
            Assert.AreEqual("d", array[1]);
        }

        [TestCase("a", "b")]
        public void HandlesParamsArrayWithNoItemsAsLastArgument(string s1, string s2, params object[] array)
        {
            Assert.AreEqual("a", s1);
            Assert.AreEqual("b", s2);
            Assert.AreEqual(0, array.Length);
        }

        [TestCase("a", "b", "c")]
        public void HandlesParamsArrayWithOneItemAsLastArgument(string s1, string s2, params object[] array)
        {
            Assert.AreEqual("a", s1);
            Assert.AreEqual("b", s2);
            Assert.AreEqual("c", array[0]);
        }

        [Test]
        public void CanSpecifyDescription()
        {
			Test test = (Test)TestBuilder.MakeParameterizedMethodSuite(
				typeof(TestCaseAttributeFixture), "MethodHasDescriptionSpecified").Tests[0];
			Assert.AreEqual("My Description", test.Properties.Get(PropertyNames.Description));
		}

        [Test]
        public void CanSpecifyTestName()
        {
            Test test = (Test)TestBuilder.MakeParameterizedMethodSuite(
                typeof(TestCaseAttributeFixture), "MethodHasTestNameSpecified").Tests[0];
            Assert.AreEqual("XYZ", test.Name);
            Assert.AreEqual("NUnit.TestData.TestCaseAttributeFixture.TestCaseAttributeFixture.XYZ", test.FullName);
        }

        [Test]
        public void CanSpecifyCategory()
        {
            Test test = (Test)TestBuilder.MakeTestCase(
                typeof(TestCaseAttributeFixture), "MethodHasSingleCategory").Tests[0];
			IList categories = test.Properties["Category"];
            Assert.AreEqual(new string[] { "XYZ" }, categories);
        }
 
        [Test]
        public void CanSpecifyMultipleCategories()
        {
            Test test = (Test)TestBuilder.MakeTestCase(
                typeof(TestCaseAttributeFixture), "MethodHasMultipleCategories").Tests[0];
			IList categories = test.Properties["Category"];
            Assert.AreEqual(new string[] { "X", "Y", "Z" }, categories);
        }
 
        [Test]
        public void CanSpecifyExpectedException()
        {
            ITestResult result = (ITestResult)TestBuilder.RunTestCase(
                typeof(TestCaseAttributeFixture), "MethodThrowsExpectedException").Children[0];
            Assert.AreEqual(ResultState.Success, result.ResultState);
        }

        [Test]
        public void CanSpecifyExpectedException_WrongException()
        {
            ITestResult result = (ITestResult)TestBuilder.RunTestCase(
                typeof(TestCaseAttributeFixture), "MethodThrowsWrongException").Children[0];
            Assert.AreEqual(ResultState.Failure, result.ResultState);
            Assert.That(result.Message, Is.StringStarting("An unexpected exception type was thrown"));
        }

        [Test]
        public void CanSpecifyExpectedException_WrongMessage()
        {
            ITestResult result = (ITestResult)TestBuilder.RunTestCase(
                typeof(TestCaseAttributeFixture), "MethodThrowsExpectedExceptionWithWrongMessage").Children[0];
            Assert.AreEqual(ResultState.Failure, result.ResultState);
            Assert.That(result.Message, Is.StringStarting("The exception message text was incorrect"));
        }

        [Test]
        public void CanSpecifyExpectedException_NoneThrown()
        {
            ITestResult result = (ITestResult)TestBuilder.RunTestCase(
                typeof(TestCaseAttributeFixture), "MethodThrowsNoException").Children[0];
            Assert.AreEqual(ResultState.Failure, result.ResultState);
            Assert.AreEqual("System.ArgumentNullException was expected", result.Message);
        }

        [Test]
        public void IgnoreTakesPrecedenceOverExpectedException()
        {
            ITestResult result = (ITestResult)TestBuilder.RunTestCase(
                typeof(TestCaseAttributeFixture), "MethodCallsIgnore").Children[0];
            Assert.AreEqual(ResultState.Ignored, result.ResultState);
            Assert.AreEqual("Ignore this", result.Message);
        }

        [Test]
        public void CanIgnoreIndividualTestCases()
        {
            TestSuite test = (TestSuite)TestBuilder.MakeTestCase(
                typeof(TestCaseAttributeFixture), "MethodWithIgnoredTestCases");

            Test testCase = TestFinder.Find("MethodWithIgnoredTestCases(1)", test, false);
            Assert.That(testCase.RunState, Is.EqualTo(RunState.Runnable));
 
            testCase = TestFinder.Find("MethodWithIgnoredTestCases(2)", test, false);
            Assert.That(testCase.RunState, Is.EqualTo(RunState.Ignored));
 
			testCase = TestFinder.Find("MethodWithIgnoredTestCases(3)", test, false);
            Assert.That(testCase.RunState, Is.EqualTo(RunState.Ignored));
            Assert.That(testCase.Properties.GetSetting(PropertyNames.SkipReason, ""), Is.EqualTo("Don't Run Me!"));
		}

        [Test]
        public void CanMarkIndividualTestCasesExplicit()
        {
            TestSuite test = (TestSuite)TestBuilder.MakeTestCase(
                typeof(TestCaseAttributeFixture), "MethodWithExplicitTestCases");

            Test testCase = TestFinder.Find("MethodWithExplicitTestCases(1)", test, false);
            Assert.That(testCase.RunState, Is.EqualTo(RunState.Runnable));
 
            testCase = TestFinder.Find("MethodWithExplicitTestCases(2)", test, false);
            Assert.That(testCase.RunState, Is.EqualTo(RunState.Explicit));
 
			testCase = TestFinder.Find("MethodWithExplicitTestCases(3)", test, false);
            Assert.That(testCase.RunState, Is.EqualTo(RunState.Explicit));
            Assert.That(testCase.Properties.GetSetting(PropertyNames.SkipReason, ""), Is.EqualTo("Connection failing"));
		}
    }
}
