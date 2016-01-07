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
using NUnit.Framework.Api;

namespace NUnit.Framework.Internal.Commands
{
    /// <summary>
    /// TODO: Documentation needed for class
    /// </summary>
    public class SkipCommand : TestCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkipCommand"/> class.
        /// </summary>
        /// <param name="test">The test being skipped.</param>
        public SkipCommand(Test test) : base(test)
        {
        }

        /// <summary>
        /// Overridden to simply set the CurrentResult to the
        /// appropriate Skipped state.
        /// </summary>
        /// <param name="context">The execution context for the test</param>
        /// <returns>A TestResult</returns>
        public override TestResult Execute(TestExecutionContext context)
        {
            //TestResult testResult = this.Test.MakeTestResult();
            TestResult testResult = context.CurrentResult;

            switch (Test.RunState)
            {
                default:
                case RunState.Skipped:
                    testResult.SetResult(ResultState.Skipped, GetSkipReason());
                    break;
                case RunState.Ignored:
                    testResult.SetResult(ResultState.Ignored, GetSkipReason());
                    break;
                case RunState.NotRunnable:
                    testResult.SetResult(ResultState.NotRunnable, GetSkipReason(), GetProviderStackTrace());
                    break;
            }

            return testResult;
        }

        private string GetSkipReason()
        {
            return (string)Test.Properties.Get(PropertyNames.SkipReason);
        }

        private string GetProviderStackTrace()
        {
            return (string)Test.Properties.Get(PropertyNames.ProviderStackTrace);
        }
    }
}