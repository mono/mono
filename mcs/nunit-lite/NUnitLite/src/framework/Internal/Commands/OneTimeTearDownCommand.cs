// ***********************************************************************
// Copyright (c) 2012 Charlie Poole
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
    /// OneTimeTearDownCommand performs any teardown actions
    /// specified for a suite and calls Dispose on the user
    /// test object, if any.
    /// </summary>
    public class OneTimeTearDownCommand : TestCommand
    {
        private readonly TestSuite suite;
        private readonly Type fixtureType;

        /// <summary>
        /// Construct a OneTimeTearDownCommand
        /// </summary>
        /// <param name="suite">The test suite to which the command applies</param>
        public OneTimeTearDownCommand(TestSuite suite)
            : base(suite)
        {
            this.suite = suite;
            this.fixtureType = suite.FixtureType;
        }

        /// <summary>
        /// Overridden to run the teardown methods specified on the test.
        /// </summary>
        /// <param name="context">The TestExecutionContext to be used.</param>
        /// <returns>A TestResult</returns>
        public override TestResult Execute(TestExecutionContext context)
        {
            TestResult suiteResult = context.CurrentResult;

            if (fixtureType != null)
            {
                MethodInfo[] teardownMethods =
                    Reflect.GetMethodsWithAttribute(fixtureType, typeof(TestFixtureTearDownAttribute), true);

                try
                {
                    int index = teardownMethods.Length;
                    while (--index >= 0)
                    {
                        MethodInfo fixtureTearDown = teardownMethods[index];
                        if (!fixtureTearDown.IsStatic && context.TestObject == null)
                            Console.WriteLine("TestObject should not be null!!!");
                        Reflect.InvokeMethod(fixtureTearDown, fixtureTearDown.IsStatic ? null : context.TestObject);
                    }

                    IDisposable disposable = context.TestObject as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
                catch (Exception ex)
                {
                    // Error in TestFixtureTearDown or Dispose causes the
                    // suite to be marked as a error, even if
                    // all the contained tests passed.
                    NUnitException nex = ex as NUnitException;
                    if (nex != null)
                        ex = nex.InnerException;

                    // TODO: Can we move this logic into TestResult itself?
                    string message = "TearDown : " + ExceptionHelper.BuildMessage(ex);
                    if (suiteResult.Message != null)
                        message = suiteResult.Message + NUnit.Env.NewLine + message;

                    string stackTrace = "--TearDown" + NUnit.Env.NewLine + ExceptionHelper.BuildStackTrace(ex);
                    if (suiteResult.StackTrace != null)
                        stackTrace = suiteResult.StackTrace + NUnit.Env.NewLine + stackTrace;

                    // TODO: What about ignore exceptions in teardown?
                    suiteResult.SetResult(ResultState.Error, message, stackTrace);
                }
            }

            return suiteResult;
        }
    }
}
