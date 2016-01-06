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

namespace NUnit.Framework.Internal.Commands
{
    /// <summary>
    /// TestCommand is the abstract base class for all test commands
    /// in the framework. A TestCommand represents a single stage in
    /// the execution of a test, e.g.: SetUp/TearDown, checking for
    /// Timeout, verifying the returned result from a method, etc.
    /// 
    /// TestCommands may decorate other test commands so that the
    /// execution of a lower-level command is nested within that
    /// of a higher level command. All nested commands are executed
    /// synchronously, as a single unit. Scheduling test execution
    /// on separate threads is handled at a higher level, using the
    /// task dispatcher.
    /// </summary>
    public abstract class TestCommand
    {
        private Test test;

        /// <summary>
        /// Construct a TestCommand for a test.
        /// </summary>
        /// <param name="test">The test to be executed</param>
        public TestCommand(Test test)
        {
            this.test = test;
        }

        #region ITestCommandMembers

        /// <summary>
        /// Gets the test associated with this command.
        /// </summary>
        public Test Test
        {
            get { return test; }
        }

        /// <summary>
        /// Runs the test in a specified context, returning a TestResult.
        /// </summary>
        /// <param name="context">The TestExecutionContext to be used for running the test.</param>
        /// <returns>A TestResult</returns>
        public abstract TestResult Execute(TestExecutionContext context);

        #endregion
    }
}
