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
using System.Threading;
using NUnit.Framework.Api;

namespace NUnit.Framework.Internal.Commands
{
    /// <summary>
    /// SetUpTearDownDecorator decorates a test command by running
    /// a setup method before the original command and a teardown
    /// method after it has executed.
    /// </summary>
    public class SetUpTearDownDecorator : ICommandDecorator
    {
        CommandStage ICommandDecorator.Stage
        {
            get { return CommandStage.SetUpTearDown; }
        }

        int ICommandDecorator.Priority
        {
            get { return 0; }
        }

        TestCommand ICommandDecorator.Decorate(TestCommand command)
        {
            return new SetUpTearDownCommand(command);
        }
    }

    /// <summary>
    /// TODO: Documentation needed for class
    /// </summary>
    public class SetUpTearDownCommand : DelegatingTestCommand
    {
        private readonly MethodInfo[] setUpMethods;
        private readonly MethodInfo[] tearDownMethods;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetUpTearDownCommand"/> class.
        /// </summary>
        /// <param name="innerCommand">The inner command.</param>
        public SetUpTearDownCommand(TestCommand innerCommand)
            : base(innerCommand)
        {
            this.setUpMethods = Test.SetUpMethods;
            this.tearDownMethods = Test.TearDownMethods;
        }

        /// <summary>
        /// Runs the test, saving a TestResult in the supplied TestExecutionContext.
        /// </summary>
        /// <param name="context">The context in which the test should run.</param>
        /// <returns>A TestResult</returns>
        public override TestResult Execute(TestExecutionContext context)
        {
            try
            {
                RunSetUpMethods(context);

                context.CurrentResult = innerCommand.Execute(context);
            }
            catch (Exception ex)
            {
#if !NETCF && !SILVERLIGHT && !__TVOS__ && !__WATCHOS__
                if (ex is ThreadAbortException)
                    Thread.ResetAbort();
#endif
                context.CurrentResult.RecordException(ex);
            }
            finally
            {
                RunTearDownMethods(context);
            }

            return context.CurrentResult;
        }

        private void RunSetUpMethods(TestExecutionContext context)
        {
            if (setUpMethods != null)
                foreach (MethodInfo setUpMethod in setUpMethods)
                    Reflect.InvokeMethod(setUpMethod, setUpMethod.IsStatic ? null : context.TestObject);
        }

        private void RunTearDownMethods(TestExecutionContext context)
        {
            try
            {
                if (tearDownMethods != null)
                {
                    int index = tearDownMethods.Length;
                    while (--index >= 0)
                        Reflect.InvokeMethod(tearDownMethods[index], tearDownMethods[index].IsStatic ? null : context.TestObject);
                }
            }
            catch (Exception ex)
            {
                if (ex is NUnitException)
                    ex = ex.InnerException;

                // TODO: What about ignore exceptions in teardown?
                ResultState resultState = context.CurrentResult.ResultState == ResultState.Cancelled
                    ? ResultState.Cancelled
                    : ResultState.Error;

                // TODO: Can we move this logic into TestResult itself?
                string message = "TearDown : " + ExceptionHelper.BuildMessage(ex);
                if (context.CurrentResult.Message != null)
                    message = context.CurrentResult.Message + NUnit.Env.NewLine + message;

                string stackTrace = "--TearDown" + NUnit.Env.NewLine + ExceptionHelper.BuildStackTrace(ex);
                if (context.CurrentResult.StackTrace != null)
                    stackTrace = context.CurrentResult.StackTrace + NUnit.Env.NewLine + stackTrace;

                context.CurrentResult.SetResult(resultState, message, stackTrace);
            }
        }
    }
}
