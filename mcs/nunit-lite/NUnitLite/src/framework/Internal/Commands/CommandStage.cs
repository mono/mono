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

namespace NUnit.Framework.Internal.Commands
{
    /// <summary>
    /// The CommandStage enumeration represents the defined stages
    /// of execution for a series of TestCommands. The int values
    /// of the enum are used to apply decorators in the proper 
    /// order. Lower values are applied first and are therefore
    /// "closer" to the actual test execution.
    /// </summary>
    /// <remarks>
    /// No CommandStage is defined for actual invocation of the test or
    /// for creation of the context. Execution may be imagined as 
    /// proceeding from the bottom of the list upwards, with cleanup
    /// after the test running in the opposite order.
    /// </remarks>
    public enum CommandStage
    {
        /// <summary>
        /// Use an application-defined default value.
        /// </summary>
        Default,

        // NOTE: The test is actually invoked here.

        /// <summary>
        /// Make adjustments needed before and after running
        /// the raw test - that is, after any SetUp has run
        /// and before TearDown.
        /// </summary>
        BelowSetUpTearDown,

        /// <summary>
        /// Run SetUp and TearDown for the test.  This stage is used
        /// internally by NUnit and should not normally appear
        /// in user-defined decorators.
        /// </summary>
        SetUpTearDown,

        /// <summary>
        /// Make adjustments needed before and after running 
        /// the entire test - including SetUp and TearDown.
        /// </summary>
        AboveSetUpTearDown

        // Note: The context is created here and destroyed
        // after the test has run.

        // Command Stages
        //   Create/Destroy Context
        //   Modify/Restore Context
        //   Create/Destroy fixture object
        //   Repeat test
        //   Create/Destroy thread
        //   Modify overall result
        //   SetUp/TearDown
        //   Modify raw result
    }
}
