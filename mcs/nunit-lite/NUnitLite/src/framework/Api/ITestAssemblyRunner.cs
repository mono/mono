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
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Reflection;

namespace NUnit.Framework.Api
{
    /// <summary>
    /// The ITestAssemblyRunner interface is implemented by classes
    /// that are able to execute a suite of tests loaded
    /// from an assembly.
    /// </summary>
    public interface ITestAssemblyRunner
    {
        #region Properties

        /// <summary>
        /// Gets the tree of loaded tests, or null if
        /// no tests have been loaded.
        /// </summary>
        ITest LoadedTest { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the tests found in an Assembly, returning an 
        /// indication of whether or not the load succeeded.
        /// </summary>
        /// <param name="assemblyName">File name of the assembly to load</param>
        /// <param name="settings">Dictionary of settings to use in loading the test</param>
        /// <returns>True if the load was successful</returns>
        bool Load(string assemblyName, System.Collections.IDictionary settings);

        /// <summary>
        /// Loads the tests found in an Assembly, returning an 
        /// indication of whether or not the load succeeded.
        /// </summary>
        /// <param name="assembly">The assembly to load</param>
        /// <param name="settings">Dictionary of settings to use in loading the test</param>
        /// <returns>True if the load was successful</returns>
        bool Load(Assembly assembly, System.Collections.IDictionary settings);

        ///// <summary>
        ///// Count Test Cases using a filter
        ///// </summary>
        ///// <param name="filter">The filter to apply</param>
        ///// <returns>The number of test cases found</returns>
        //int CountTestCases(TestFilter filter);

        /// <summary>
        /// Run selected tests and return a test result. The test is run synchronously,
        /// and the listener interface is notified as it progresses.
        /// </summary>
        /// <param name="listener">Interface to receive ITestListener notifications.</param>
        /// <param name="filter">A test filter used to select tests to be run</param>
        ITestResult Run(ITestListener listener, ITestFilter filter);

        #endregion
    }
}
