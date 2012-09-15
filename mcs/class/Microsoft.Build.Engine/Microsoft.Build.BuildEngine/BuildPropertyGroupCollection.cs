//
// BuildPropertyGroupCollection.cs: Collection for group of properties
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2005 Marek Sieradzki
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

using System;
using System.Collections;

namespace Microsoft.Build.BuildEngine {
	public class BuildPropertyGroupCollection : ICollection, IEnumerable {

		GroupingCollection	groupingCollection;
	
		BuildPropertyGroupCollection ()
		{
			groupingCollection = new GroupingCollection (null);
		}

		internal BuildPropertyGroupCollection (GroupingCollection groupingCollection)
		{
			this.groupingCollection = groupingCollection;
		}
		
		public void CopyTo (Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new IndexOutOfRangeException ("Index was outside the bounds of the array.");
			if (array.Rank > 1)
				throw new ArgumentException ("array is multidimensional");
			if ((array.Length > 0) && (index >= array.Length))
				throw new IndexOutOfRangeException ("Index was outside the bounds of the array.");
			if (index + this.Count > array.Length)
				throw new IndexOutOfRangeException ("Index was outside the bounds of the array.");
		
			IEnumerator it = GetEnumerator ();
			int i = index;
			while (it.MoveNext ()) {
				array.SetValue (it.Current, i++);
			}
		}

		public IEnumerator GetEnumerator ()
		{
			return groupingCollection.GetPropertyGroupEnumerator ();
		}
		
		internal void Add (BuildPropertyGroup bpg)
		{
			bpg.GroupingCollection = groupingCollection;
			groupingCollection.Add (bpg);
		}

		public int Count {
			get {
				return groupingCollection.PropertyGroups;
			}
		}

		public bool IsSynchronized {
			get {
				return false;
			}
		}

		public object SyncRoot {
			get {
				return this;
			}
		}
	}
}
