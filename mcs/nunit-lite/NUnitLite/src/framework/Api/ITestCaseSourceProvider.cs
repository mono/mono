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
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OFn
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
    /// The ITestCaseSourceProvider interface is implemented by Types that 
    /// are able to provide a test case source for use by a test method.
    /// </summary>
    public interface IDynamicTestCaseSource
    {
        /// <summary>
        /// Returns a test case source. May be called on a provider
        /// implementing the source internally or able to create
        /// a source instance on it's own.
        /// </summary>
        /// <returns></returns>
        ITestCaseSource GetTestCaseSource();
        
        /// <summary>
        /// Returns a test case source based on an instance of a 
        /// source object.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        ITestCaseSource GetTestCaseSource(object instance);
    }
}
