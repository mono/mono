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

using NUnit.Framework.Api;

namespace NUnit.Framework.Internal.Commands
{
    /// <summary>
    /// TheoryResultCommand adjusts the result of a Theory so that
    /// it fails if all the results were inconclusive.
    /// </summary>
    public class TheoryResultCommand : DelegatingTestCommand
    {
        /// <summary>
        /// Constructs a TheoryResultCommand 
        /// </summary>
        /// <param name="command">The command to be wrapped by this one</param>
        public TheoryResultCommand(TestCommand command) : base(command) { }

        /// <summary>
        /// Overridden to call the inner command and adjust the result
        /// in case all chlid results were inconclusive.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override TestResult Execute(TestExecutionContext context)
        {
            TestResult theoryResult = innerCommand.Execute(context);

            if (theoryResult.ResultState == ResultState.Success)
            {
                if (!theoryResult.HasChildren)
                    theoryResult.SetResult(ResultState.Failure, "No test cases were provided", null);
                else
                {
                    bool wasInconclusive = true;
                    foreach (TestResult childResult in theoryResult.Children)
                        if (childResult.ResultState == ResultState.Success)
                        {
                            wasInconclusive = false;
                            break;
                        }

                    if (wasInconclusive)
                        theoryResult.SetResult(ResultState.Failure, "All test cases were inconclusive", null);
                }
            }

            return theoryResult;
        }
    }
}
