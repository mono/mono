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

namespace NUnit
{
    /// <summary>
    /// ObjectList represents a collection of objects. It is implemented 
    /// as a List&lt;object&gt; in .NET 2.0 or higher and as an ArrayList otherwise.
    /// ObjectList does not attempt to be a general replacement for either of
    /// these classes but only implements what is needed within the framework.
    /// </summary>
#if CLR_2_0 || CLR_4_0
    public class ObjectList : System.Collections.Generic.List<object>
    {
        /// <summary>
        /// Adds a range of values to the collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public void AddRange(System.Collections.ICollection collection)
        {
            foreach (object item in collection)
                Add(item);
        }
    }
#else
    public class ObjectList : System.Collections.ArrayList
    {
    }
#endif
}
