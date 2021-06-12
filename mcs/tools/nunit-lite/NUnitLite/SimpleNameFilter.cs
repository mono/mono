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
#if CLR_2_0 || CLR_4_0
using System.Collections.Generic;
#endif
using NUnit.Framework.Api;

namespace NUnit.Framework.Internal.Filters
{
	/// <summary>
	/// SimpleName filter selects tests based on their name
	/// </summary>
    [Serializable]
    public class SimpleNameFilter : TestFilter
    {
#if CLR_2_0 || CLR_4_0
        private List<string> names = new List<string>();
#else
        private System.Collections.ArrayList names = new System.Collections.ArrayList();
#endif

        /// <summary>
		/// Construct an empty SimpleNameFilter
		/// </summary>
        public SimpleNameFilter() { }

        /// <summary>
        /// Construct a SimpleNameFilter for a single name
        /// </summary>
        /// <param name="nameToAdd">The name the filter will recognize.</param>
        public SimpleNameFilter(string nameToAdd)
        {
            Add(nameToAdd);
        }

        /// <summary>
        /// Construct a SimpleNameFilter for an array of names
        /// </summary>
        /// <param name="namesToAdd">The names the filter will recognize.</param>
        public SimpleNameFilter(string[] namesToAdd)
        {
            foreach (string name in namesToAdd)
                Add(name);
        }

        /// <summary>
		/// Add a name to a SimpleNameFilter
		/// </summary>
        /// <param name="name">The name to be added.</param>
        public void Add(string name)
		{
            Guard.ArgumentNotNullOrEmpty(name, "name");

            names.Add(name);
		}

		/// <summary>
		/// Check whether the filter matches a test
		/// </summary>
		/// <param name="test">The test to be matched</param>
		/// <returns>True if it matches, otherwise false</returns>
		public override bool Match( ITest test )
		{
			foreach( string name in names )
				if ( test.FullName == name )
					return true;

			return false;
		}
	}
}
