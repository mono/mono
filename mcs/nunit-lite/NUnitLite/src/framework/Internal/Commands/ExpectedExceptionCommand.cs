// ***********************************************************************
// Copyright (c) 2011 Charlie Poole
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
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework.Api;

namespace NUnit.Framework.Internal.Commands
{
    /// <summary>
    /// TODO: Documentation needed for class
    /// </summary>
    public class ExpectedExceptionCommand : DelegatingTestCommand
    {
        private ExpectedExceptionData exceptionData;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectedExceptionCommand"/> class.
        /// </summary>
        /// <param name="innerCommand">The inner command.</param>
        /// <param name="exceptionData">The exception data.</param>
        public ExpectedExceptionCommand(TestCommand innerCommand, ExpectedExceptionData exceptionData)
            : base(innerCommand)
        {
            this.exceptionData = exceptionData;
        }


        /// <summary>
        /// Runs the test, saving a TestResult in the supplied TestExecutionContext
        /// </summary>
        /// <param name="context">The context in which the test is to be run.</param>
        /// <returns>A TestResult</returns>
        public override TestResult Execute(TestExecutionContext context)
        {
            try
            {
                context.CurrentResult = innerCommand.Execute(context);

                if (context.CurrentResult.ResultState == ResultState.Success)
                    ProcessNoException(context);
            }
            catch (Exception ex)
            {
#if !NETCF && !SILVERLIGHT && !__TVOS__ && !__WATCHOS__
                if (ex is ThreadAbortException)
                    Thread.ResetAbort();
#endif
                ProcessException(ex, context);
            }

            return context.CurrentResult;
        }

        /// <summary>
        /// Handles processing when no exception was thrown.
        /// </summary>
        /// <param name="context">The execution context.</param>
        public void ProcessNoException(TestExecutionContext context)
        {
            context.CurrentResult.SetResult(ResultState.Failure, NoExceptionMessage());
        }

        /// <summary>
        /// Handles processing when an exception was thrown.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The execution context.</param>
        public void ProcessException(Exception exception, TestExecutionContext context)
        {
            if (exception is NUnitException)
                exception = exception.InnerException;

            if (IsExpectedExceptionType(exception))
            {
                if (IsExpectedMessageMatch(exception))
                {
                    if (context.TestObject != null)
                    {
                        MethodInfo exceptionMethod = exceptionData.GetExceptionHandler(context.TestObject.GetType());
                        if (exceptionMethod != null)
                        {
                            Reflect.InvokeMethod(exceptionMethod, context.TestObject, exception);
                        }
                        else
                        {
                            IExpectException handler = context.TestObject as IExpectException;
                            if (handler != null)
                                handler.HandleException(exception);
                        }
                    }

                    context.CurrentResult.SetResult(ResultState.Success);
                }
                else
                {
                    context.CurrentResult.SetResult(ResultState.Failure, WrongTextMessage(exception), GetStackTrace(exception));
                }
            }
            else
            {
                context.CurrentResult.RecordException(exception);

                // If it shows as an error, change it to a failure due to the wrong type
                if (context.CurrentResult.ResultState == ResultState.Error)
                    context.CurrentResult.SetResult(ResultState.Failure, WrongTypeMessage(exception), GetStackTrace(exception));
            }
        }

        #region Helper Methods

        private bool IsExpectedExceptionType(Exception exception)
        {
            return exceptionData.ExpectedExceptionName == null ||
                exceptionData.ExpectedExceptionName.Equals(exception.GetType().FullName);
        }

        private bool IsExpectedMessageMatch(Exception exception)
        {
            if (exceptionData.ExpectedMessage == null)
                return true;

            switch (exceptionData.MatchType)
            {
                case MessageMatch.Exact:
                default:
                    return exceptionData.ExpectedMessage.Equals(exception.Message);
                case MessageMatch.Contains:
                    return exception.Message.IndexOf(exceptionData.ExpectedMessage) >= 0;
                case MessageMatch.Regex:
                    return Regex.IsMatch(exception.Message, exceptionData.ExpectedMessage);
                case MessageMatch.StartsWith:
                    return exception.Message.StartsWith(exceptionData.ExpectedMessage);
            }
        }

        private string NoExceptionMessage()
        {
            string expectedType = exceptionData.ExpectedExceptionName == null ? "An Exception" : exceptionData.ExpectedExceptionName;
            return CombineWithUserMessage(expectedType + " was expected");
        }

        private string WrongTypeMessage(Exception exception)
        {
            return CombineWithUserMessage(
                "An unexpected exception type was thrown" + Env.NewLine +
                "Expected: " + exceptionData.ExpectedExceptionName + Env.NewLine +
                " but was: " + exception.GetType().FullName + " : " + exception.Message);
        }

        private string WrongTextMessage(Exception exception)
        {
            string expectedText;
            switch (exceptionData.MatchType)
            {
                default:
                case MessageMatch.Exact:
                    expectedText = "Expected: ";
                    break;
                case MessageMatch.Contains:
                    expectedText = "Expected message containing: ";
                    break;
                case MessageMatch.Regex:
                    expectedText = "Expected message matching: ";
                    break;
                case MessageMatch.StartsWith:
                    expectedText = "Expected message starting: ";
                    break;
            }

            return CombineWithUserMessage(
                "The exception message text was incorrect" + Env.NewLine +
                expectedText + exceptionData.ExpectedMessage + Env.NewLine +
                " but was: " + exception.Message);
        }

        private string CombineWithUserMessage(string message)
        {
            if (exceptionData.UserMessage == null)
                return message;
            return exceptionData.UserMessage + Env.NewLine + message;
        }

        private string GetStackTrace(Exception exception)
        {
            try
            {
                return exception.StackTrace;
            }
            catch (Exception)
            {
                return "No stack trace available";
            }
        }

        #endregion
    }
}