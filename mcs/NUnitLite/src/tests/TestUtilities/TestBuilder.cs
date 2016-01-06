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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Api;
using NUnit.Framework.Builders;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using NUnit.Framework.Extensibility;
using NUnit.Framework.Internal.WorkItems;
using System.Threading;

namespace NUnit.TestUtilities
{
    /// <summary>
    /// Utility Class used to build NUnit tests for use as test data
    /// </summary>
    public class TestBuilder
    {
        private static NUnitTestFixtureBuilder fixtureBuilder = new NUnitTestFixtureBuilder();
        private static NUnitTestCaseBuilder testBuilder = new NUnitTestCaseBuilder();

#if !NUNITLITE
        static TestBuilder()
        {
            if (!CoreExtensions.Host.Initialized)
                CoreExtensions.Host.Initialize();
        }
#endif

        public static TestSuite MakeFixture(Type type)
        {
            return (TestSuite)fixtureBuilder.BuildFrom(type);
        }

        public static TestSuite MakeFixture(object fixture)
        {
            return (TestSuite)fixtureBuilder.BuildFrom(fixture.GetType());
        }

        public static TestSuite MakeParameterizedMethodSuite(Type type, string methodName)
        {
            return (TestSuite)MakeTestCase(type, methodName);
        }

        public static Test MakeTestCase(Type type, string methodName)
        {
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
                Assert.Fail("Unable to find method {0} in type {1}", methodName, type.FullName);
            return testBuilder.BuildFrom(method);
        }

        public static Test MakeTestCase(object fixture, string methodName)
        {
            return MakeTestCase(fixture.GetType(), methodName);
        }

        public static TestResult RunTestFixture(Type type)
        {
            TestSuite suite = MakeFixture(type);
            
            TestExecutionContext context = new TestExecutionContext();
            context.TestObject = null;

            CompositeWorkItem work = new CompositeWorkItem(suite, TestFilter.Empty);
            return ExecuteAndWaitForResult(work, context);
        }

        public static TestResult RunTestFixture(object fixture)
        {
            TestSuite suite = MakeFixture(fixture);
            
            TestExecutionContext context = new TestExecutionContext();
            context.TestObject = fixture;

            WorkItem work = suite.CreateWorkItem(TestFilter.Empty);
            return ExecuteAndWaitForResult(work, context);
        }

        public static ITestResult RunTestCase(Type type, string methodName)
        {
            Test test = MakeTestCase(type, methodName);

            object testObject = null;
            if (!IsStaticClass(type))
                testObject = Activator.CreateInstance(type);

            return RunTest(test, testObject);
        }

        public static ITestResult RunTestCase(object fixture, string methodName)
        {
            Test test = MakeTestCase(fixture, methodName);
            return RunTest(test, fixture);
        }

        public static WorkItem RunTestCaseAsync(object fixture, string methodName)
        {
            Test test = MakeTestCase(fixture, methodName);
            return RunTestAsync(test, fixture);
        }

        public static ITestResult RunTest(Test test)
        {
            return RunTest(test, null);
        }

        public static WorkItem RunTestAsync(Test test)
        {
            return RunTestAsync(test, (object)null);
        }

        public static WorkItem RunTestAsync(Test test, object testObject)
        {
            TestExecutionContext context = new TestExecutionContext();
            context.TestObject = testObject;

            WorkItem work = test.CreateWorkItem(TestFilter.Empty);
            work.Execute(context);

            return work;
        }

        public static ITestResult RunTest(Test test, object testObject)
        {
            TestExecutionContext context = new TestExecutionContext();
            context.TestObject = testObject;

            WorkItem work = test.CreateWorkItem(TestFilter.Empty);
            return ExecuteAndWaitForResult(work, context);
        }

        private static TestResult ExecuteAndWaitForResult(WorkItem work, TestExecutionContext context)
        {
            work.Execute(context);

            // TODO: Replace with an event
            while (work.State != WorkItemState.Complete)
                Thread.Sleep(1);

            return work.Result;
        }

        private static bool IsStaticClass(Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }

        private TestBuilder() { }
    }
}
