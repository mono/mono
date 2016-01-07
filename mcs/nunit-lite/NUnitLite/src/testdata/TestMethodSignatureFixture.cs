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
using NUnit.Framework;

namespace NUnit.TestData.TestMethodSignatureFixture
{
	[TestFixture]
	public class TestMethodSignatureFixture
	{
		public static int Tests = 19;
		public static int Runnable = 11;
		public static int NotRunnable = 8;
	    public static int Errors = 3;
	    public static int Failures = 0;

		[Test]
		public void InstanceTestMethod() { }

		[Test]
		public static void StaticTestMethod() { }

        [Test]
        public void TestMethodWithArgumentsNotProvided(int x, int y, string label) { }

        [Test]
        public static void StaticTestMethodWithArgumentsNotProvided(int x, int y, string label) { }

		[TestCase(5, 2, "ABC")]
		public void TestMethodWithoutParametersWithArgumentsProvided() { }

        [TestCase(5, 2, "ABC")]
        public void TestMethodWithArgumentsProvided(int x, int y, string label)
        {
            Assert.AreEqual(5, x);
            Assert.AreEqual(2, y);
            Assert.AreEqual("ABC", label);
        }

        [TestCase(5, 2, "ABC")]
        public static void StaticTestMethodWithArgumentsProvided(int x, int y, string label)
        {
            Assert.AreEqual(5, x);
            Assert.AreEqual(2, y);
            Assert.AreEqual("ABC", label);
        }

        [TestCase(2, 2)]
        public void TestMethodWithWrongNumberOfArgumentsProvided(int x, int y, string label)
        {
        }

        [TestCase(2, 2, 3.5)]
        public void TestMethodWithWrongArgumentTypesProvided(int x, int y, string label)
        {
        }

        [TestCase(2, 2)]
        public static void StaticTestMethodWithWrongNumberOfArgumentsProvided(int x, int y, string label)
        {
        }

        [TestCase(2, 2, 3.5)]
        public static void StaticTestMethodWithWrongArgumentTypesProvided(int x, int y, string label)
        {
        }

        [TestCase(3.7, 2, 5.7)]
        public void TestMethodWithConvertibleArguments(double x, double y, double sum)
        {
            Assert.AreEqual(sum, x + y, 0.0001);
        }

        [TestCase(3.7, 2, 5.7)]
        public void TestMethodWithNonConvertibleArguments(int x, int y, int sum)
        {
            Assert.AreEqual(sum, x + y, 0.0001);
        }

        [TestCase(12, 3, 4)]
		[TestCase( 12, 2, 6 )]
		[TestCase( 12, 4, 3 )]
		public void TestMethodWithMultipleTestCases( int n, int d, int q )
		{
			Assert.AreEqual( q, n / d );
		}

//		[Test]
//		public abstract void AbstractTestMethod() { }

		[Test]
		protected void ProtectedTestMethod() { }

		[Test]
		private void PrivateTestMethod() { }

		[Test]
		public bool TestMethodWithReturnType() 
		{
			return true;
		}
	}
}
