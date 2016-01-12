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
using NUnit.Framework;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;

namespace NUnit.TestUtilities
{
    public class TestAssert
    {
        #region IsRunnable

        /// <summary>
        /// Verify that a test is runnable
        /// </summary>
        public static void IsRunnable(Test test)
        {
            Assert.AreEqual(RunState.Runnable, test.RunState);
        }

        /// <summary>
        /// Verify that the first child test is runnable
        /// </summary>
        public static void FirstChildIsRunnable(Test test)
        {
            IsRunnable((Test)test.Tests[0]);
        }

        /// <summary>
        /// Verify that a Type can be used to create a
        /// runnable fixture
        /// </summary>
        public static void IsRunnable(Type type)
        {
            TestSuite suite = TestBuilder.MakeFixture(type);
            Assert.NotNull(suite, "Unable to construct fixture");
            Assert.AreEqual(RunState.Runnable, suite.RunState);
        }

        /// <summary>
        /// Verify that a Type is runnable, then run it and
        /// verify the result.
        /// </summary>
        public static void IsRunnable(Type type, ResultState resultState)
        {
            TestSuite suite = TestBuilder.MakeFixture(type);
            Assert.NotNull(suite, "Unable to construct fixture");
            Assert.AreEqual(RunState.Runnable, suite.RunState);
            ITestResult result = TestBuilder.RunTest(suite);
            Assert.AreEqual(resultState, result.ResultState);
        }

        /// <summary>
        /// Verify that a named test method is runnable
        /// </summary>
        public static void IsRunnable(Type type, string name)
        {
            Test test = TestBuilder.MakeTestCase(type, name);
            Assert.That(test.RunState, Is.EqualTo(RunState.Runnable));
        }

        /// <summary>
        /// Verify that the first child (usually a test case)
        /// of a named test method is runnable
        /// </summary>
        public static void FirstChildIsRunnable(Type type, string name)
        {
            Test suite = TestBuilder.MakeTestCase(type, name);
            TestAssert.FirstChildIsRunnable(suite);
        }

        /// <summary>
        /// Verify that a named test method is runnable, then
        /// run it and verify the result.
        /// </summary>
        public static void IsRunnable(Type type, string name, ResultState resultState)
        {
            Test test = TestBuilder.MakeTestCase(type, name);
            Assert.That(test.RunState, Is.EqualTo(RunState.Runnable));
            object testObject = Activator.CreateInstance(type);
            ITestResult result = TestBuilder.RunTest(test, testObject);
            if (result.HasChildren) // In case it's a parameterized method
                result = (ITestResult)result.Children[0];
            Assert.That(result.ResultState, Is.EqualTo(resultState));
        }

        #endregion

        #region IsNotRunnable
        public static void IsNotRunnable(Test test)
        {
            Assert.AreEqual(RunState.NotRunnable, test.RunState);
            ITestResult result = TestBuilder.RunTest(test, null);
            Assert.AreEqual(ResultState.NotRunnable, result.ResultState);
        }

        public static void IsNotRunnable(Type type)
        {
            TestSuite fixture = TestBuilder.MakeFixture(type);
            Assert.NotNull(fixture, "Unable to construct fixture");
            IsNotRunnable(fixture);
        }

        public static void IsNotRunnable(Type type, string name)
        {
            IsNotRunnable(TestBuilder.MakeTestCase(type, name));
        }

        public static void FirstChildIsNotRunnable(Test suite)
        {
            IsNotRunnable((Test)suite.Tests[0]);
        }

        public static void FirstChildIsNotRunnable(Type type, string name)
        {
            FirstChildIsNotRunnable(TestBuilder.MakeParameterizedMethodSuite(type, name));
        }
        #endregion

        private TestAssert() { }
    }
}
