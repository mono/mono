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
    /// Immutable class that stores a property entry as a Name/Value pair.
    /// </summary>
    public class PropertyEntry
    {
        private readonly string name;
        private readonly object value;

        /// <summary>
        /// Initializes a new immutable instance of the <see cref="PropertyEntry"/> class.  
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public PropertyEntry(string name, object value)
        {
            this.name = name;
            this.value = value;
        }

        /// <summary>Name of the PropertyEntry.</summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>Value of the PropertyEntry.</summary>
        public object Value
        {
            get { return value; }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}={1}", name, value);
        }
    }
}
