// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
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
using NUnit.Framework.Internal;

namespace NUnit.Framework.Extensibility
{
	/// <summary>
	/// The ISuiteBuilder interface is exposed by a class that knows how to
	/// build a suite from one or more Types. 
	/// </summary>
	public interface ISuiteBuilder
	{
		/// <summary>
		/// Examine the type and determine if it is suitable for
		/// this builder to use in building a TestSuite.
        /// 
        /// Note that returning false will cause the type to be ignored 
        /// in loading the tests. If it is desired to load the suite
        /// but label it as non-runnable, ignored, etc., then this
        /// method must return true.
        /// </summary>
		/// <param name="type">The type of the fixture to be used</param>
		/// <returns>True if the type can be used to build a TestSuite</returns>
		bool CanBuildFrom( Type type );

		/// <summary>
		/// Build a TestSuite from type provided.
		/// </summary>
		/// <param name="type">The type of the fixture to be used</param>
		/// <returns>A TestSuite</returns>
		Test BuildFrom( Type type );
	}
}
