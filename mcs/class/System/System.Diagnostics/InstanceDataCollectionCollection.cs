//
// System.Diagnostics.InstanceDataCollectionCollection.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

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
//

using System;
using System.Collections;
using System.Diagnostics;

namespace System.Diagnostics {

	public class InstanceDataCollectionCollection : DictionaryBase {

		private static void CheckNull (object value, string name)
		{
			if (value == null)
				throw new ArgumentNullException (name);
		}

		// may throw ArgumentNullException
		[Obsolete ("Use PerformanceCounterCategory.ReadCategory()")]
		public InstanceDataCollectionCollection ()
		{
		}

		// may throw ArgumentNullException
		public InstanceDataCollection this [string counterName] {
			get {
				CheckNull (counterName, "counterName");
				return (InstanceDataCollection) Dictionary [counterName];
			}
		}

		public ICollection Keys {
			get {return Dictionary.Keys;}
		}

		public ICollection Values {
			get {return Dictionary.Values;}
		}

		// may throw ArgumentNullException
		public bool Contains (string counterName)
		{
			CheckNull (counterName, "counterName");
			return Dictionary.Contains (counterName);
		}

		public void CopyTo (InstanceDataCollection[] counters, int index)
		{
			Dictionary.CopyTo (counters, index);
		}
	}
}

