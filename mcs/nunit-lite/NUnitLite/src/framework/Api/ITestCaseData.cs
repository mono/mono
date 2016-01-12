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
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OFn
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Collections;

namespace NUnit.Framework.Api
{
    /// <summary>
    /// The ITestCaseData interface is implemented by a class
    /// that is able to return complete testcases for use by
    /// a parameterized test method.
    /// </summary>
    public interface ITestCaseData
    {
        /// <summary>
        /// Gets the name to be used for the test
        /// </summary>
        string TestName { get; }
		
		/// <summary>
		/// Gets the RunState for this test case.
		/// </summary>
		RunState RunState { get; }

        /// <summary>
        /// Gets the argument list to be provided to the test
        /// </summary>
        object[] Arguments { get; }

        /// <summary>
        /// Gets the expected result of the test case
        /// </summary>
        object ExpectedResult { get; }

        /// <summary>
        /// Returns true if an expected result has been set
        /// </summary>
        bool HasExpectedResult { get; }

        /// <summary>
        /// Gets data about any expected exception.
        /// </summary>
        ExpectedExceptionData ExceptionData { get; }

        /// <summary>
        /// Gets the property dictionary for the test case
        /// </summary>
        IPropertyBag Properties { get; }
    }
}
