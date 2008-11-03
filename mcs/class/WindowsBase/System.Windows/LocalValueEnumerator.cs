//
// LocalValueEnumerator.cs
//
// Author:
//   Iain McCoy (iain@mccoy.id.au)
//
// (C) 2005 Iain McCoy
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


using System.Collections;
using System.Collections.Generic;

namespace System.Windows {
	public struct LocalValueEnumerator : IEnumerator {
		private IDictionaryEnumerator propertyEnumerator;
		private Dictionary<DependencyProperty,object> properties;

		private int count;

		internal LocalValueEnumerator(Dictionary<DependencyProperty,object> properties)
		{
			this.count = properties.Count;
			this.properties = properties;
			this.propertyEnumerator = properties.GetEnumerator();
		}

		public int Count {
			get { return count; }
		}

		public LocalValueEntry Current {
			get { return new LocalValueEntry((DependencyProperty)propertyEnumerator.Key, 
					propertyEnumerator.Value); }
		}
		object IEnumerator.Current {
			get { return this.Current; }
		}

		public bool MoveNext()
		{
			return propertyEnumerator.MoveNext();
		}
		public void Reset()
		{
			propertyEnumerator.Reset();
		}

		public static bool operator != (LocalValueEnumerator obj1, LocalValueEnumerator obj2)
		{
			throw new NotImplementedException ();
		}

		public static bool operator == (LocalValueEnumerator obj1, LocalValueEnumerator obj2)
		{
			throw new NotImplementedException ();
		}

		public override bool Equals (object obj)
		{
			throw new NotImplementedException ();
		}

		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}
	}
}
