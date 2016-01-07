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

namespace NUnit.Framework.Api
{
    /// <summary>
    /// The ITestResult interface represents the result of a test.
    /// </summary>
    public interface ITestResult : IXmlNodeBuilder
    {
        /// <summary>
        /// Gets the ResultState of the test result, which 
        /// indicates the success or failure of the test.
        /// </summary>
        ResultState ResultState
        {
            get;
        }

        /// <summary>
        /// Gets the name of the test result
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Gets the full name of the test result
        /// </summary>
        string FullName
        {
            get;
        }

        /// <summary>
        /// Gets the elapsed time for running the test
        /// </summary>
        TimeSpan Duration
        {
            get;
        }

        /// <summary>
        /// Gets the message associated with a test
        /// failure or with not running the test
        /// </summary>
        string Message
        {
            get;
        }

        /// <summary>
        /// Gets any stacktrace associated with an
        /// error or failure. Not available in
        /// the Compact Framework 1.0.
        /// </summary>
        string StackTrace
        {
            get;
        }

        /// <summary>
        /// Gets the number of asserts executed
        /// when running the test and all its children.
        /// </summary>
        int AssertCount
        {
            get;
        }


        /// <summary>
        /// Gets the number of test cases that failed
        /// when running the test and all its children.
        /// </summary>
        int FailCount
        {
            get;
        }

        /// <summary>
        /// Gets the number of test cases that passed
        /// when running the test and all its children.
        /// </summary>
        int PassCount
        {
            get;
        }

        /// <summary>
        /// Gets the number of test cases that were skipped
        /// when running the test and all its children.
        /// </summary>
        int SkipCount
        {
            get;
        }

        /// <summary>
        /// Gets the number of test cases that were inconclusive
        /// when running the test and all its children.
        /// </summary>
        int InconclusiveCount
        {
            get;
        }

        /// <summary>
        /// Indicates whether this result has any child results.
        /// Accessing HasChildren should not force creation of the
        /// Children collection in classes implementing this interface.
        /// </summary>
        bool HasChildren
        {
            get;
        }

        /// <summary>
        /// Gets the the collection of child results.
        /// </summary>
#if CLR_2_0 || CLR_4_0
        System.Collections.Generic.IList<ITestResult> Children
#else
        System.Collections.IList Children
#endif
        {
            get;
        }

        /// <summary>
        /// Gets the Test to which this result applies.
        /// </summary>
        ITest Test
        {
            get;
        }
    }
}
