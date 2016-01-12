// ***********************************************************************
// Copyright (c) 2010 Charlie Poole
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
using NUnit.Framework.Api;

namespace NUnit.Framework.Internal.Commands
{
    /// <summary>
    /// TestMethodCommand is the lowest level concrete command
    /// used to run actual test cases.
    /// </summary>
    public class TestMethodCommand : TestCommand
    {
        private const string TaskWaitMethod = "Wait";
        private const string TaskResultProperty = "Result";
        private const string SystemAggregateException = "System.AggregateException";
        private const string InnerExceptionsProperty = "InnerExceptions";
        private const BindingFlags TaskResultPropertyBindingFlags = BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public;
        private readonly TestMethod testMethod;
        private readonly object[] arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestMethodCommand"/> class.
        /// </summary>
        /// <param name="testMethod">The test.</param>
        public TestMethodCommand(TestMethod testMethod) : base(testMethod)
        {
            this.testMethod = testMethod;
            this.arguments = testMethod.Arguments;
        }

        /// <summary>
        /// Runs the test, saving a TestResult in the execution context, as
        /// well as returning it. If the test has an expected result, it
        /// is asserts on that value. Since failed tests and errors throw
        /// an exception, this command must be wrapped in an outer command,
        /// will handle that exception and records the failure. This role
        /// is usually played by the SetUpTearDown command.
        /// </summary>
        /// <param name="context">The execution context</param>
        public override TestResult Execute(TestExecutionContext context)
        {
            // TODO: Decide if we should handle exceptions here
            object result = RunTestMethod(context);

            if (testMethod.HasExpectedResult)
                NUnit.Framework.Assert.AreEqual(testMethod.ExpectedResult, result);

            context.CurrentResult.SetResult(ResultState.Success);
            // TODO: Set assert count here?
            //context.CurrentResult.AssertCount = context.AssertCount;
            return context.CurrentResult;
        }

        private object RunTestMethod(TestExecutionContext context)
        {
#if NET_4_5
            if (MethodHelper.IsAsyncMethod(testMethod.Method))
                return RunAsyncTestMethod(context);
            //{
            //    if (testMethod.Method.ReturnType == typeof(void))
            //        return RunAsyncVoidTestMethod(context);
            //    else
            //        return RunAsyncTaskTestMethod(context);
            //}
            else
#endif
                return RunNonAsyncTestMethod(context);
        }

#if NET_4_5
        private object RunAsyncTestMethod(TestExecutionContext context)
        {
            using (AsyncInvocationRegion region = AsyncInvocationRegion.Create(testMethod.Method))
            {
                object result = Reflect.InvokeMethod(testMethod.Method, context.TestObject, arguments);

                try
                {
                    return region.WaitForPendingOperationsToComplete(result);
                }
                catch (Exception e)
                {
                    throw new NUnitException("Rethrown", e);
                }
            }
        }
#endif

        private object RunNonAsyncTestMethod(TestExecutionContext context)
        {
            return Reflect.InvokeMethod(testMethod.Method, context.TestObject, arguments);
        }

#if NET_4_5x
        private object RunAsyncVoidTestMethod(TestExecutionContext context)
        {
            var previousContext = SynchronizationContext.Current;
            var currentContext = new AsyncSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(currentContext);

            try
            {
                object result = Reflect.InvokeMethod(testMethod.Method, context.TestObject, arguments);

                currentContext.WaitForOperationCompleted();

                if (currentContext.Exceptions.Count > 0)
                    throw new NUnitException("Rethrown", currentContext.Exceptions[0]);

                return result;
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        private object RunAsyncTaskTestMethod(TestExecutionContext context)
        {
            try
            {
                object task = Reflect.InvokeMethod(testMethod.Method, context.TestObject, arguments);

                Reflect.InvokeMethod(testMethod.Method.ReturnType.GetMethod(TaskWaitMethod, new Type[0]), task);
                PropertyInfo resultProperty = testMethod.Method.ReturnType.GetProperty(TaskResultProperty, TaskResultPropertyBindingFlags);

                return resultProperty != null ? resultProperty.GetValue(task, null) : null;
            }
            catch (NUnitException e)
            {
                if (e.InnerException != null &&
                    e.InnerException.GetType().FullName.Equals(SystemAggregateException))
                {
                    IList<Exception> inner = (IList<Exception>)e.InnerException.GetType()
                        .GetProperty(InnerExceptionsProperty).GetValue(e.InnerException, null);

                    throw new NUnitException("Rethrown", inner[0]);
                }

                throw;
            }
        }
#endif
    }
}