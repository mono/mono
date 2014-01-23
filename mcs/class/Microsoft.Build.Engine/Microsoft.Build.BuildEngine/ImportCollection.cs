//
// ImportCollection.cs: Represents a collection of all Import elements in a
// project.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2006 Marek Sieradzki
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
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	public class ImportCollection : ICollection, IEnumerable {
		
		GroupingCollection groupingCollection;
		Dictionary <string, Import> filenames;
		
		internal ImportCollection (GroupingCollection groupingCollection)
		{
			this.groupingCollection = groupingCollection;
			filenames = new Dictionary <string, Import> ();
		}
		
		internal void Add (Import import)
		{
			if (!filenames.ContainsKey (import.EvaluatedProjectPath)) {
				groupingCollection.Add (import);
				filenames.Add (import.EvaluatedProjectPath, import);
			}
		}
		
		public void CopyTo (Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("Index was outside the bounds of the array.");
			if (array.Rank > 1)
				throw new ArgumentException ("array is multidimensional");
			if ((array.Length > 0) && (index >= array.Length))
				throw new ArgumentException ("Index was outside the bounds of the array.");
			if (index + this.Count > array.Length)
				throw new ArgumentException ("Not enough room from index to end of array for this BuildItemGroupCollection");

			IEnumerator it = GetEnumerator ();
			int i = index;
			while (it.MoveNext ()) {
				array.SetValue(it.Current, i++);
			}
		}

		internal bool Contains (Import import)
		{
			return filenames.ContainsKey (import.EvaluatedProjectPath);
		}

		internal bool TryGetImport (Import keyImport, out Import valueImport)
		{
			return filenames.TryGetValue (keyImport.EvaluatedProjectPath, out valueImport);
		}
		
		public void CopyTo (Import[] array, int index)
		{
			CopyTo ((Array) array, index);
		}
		
		public IEnumerator GetEnumerator ()
		{
			return groupingCollection.GetImportEnumerator ();
		}
		
		public int Count {
			get { return groupingCollection.Imports; }
		}
		
		public bool IsSynchronized  {
			get { return false; }
		}
		
		public object SyncRoot {
			get { return null; }
		}
	}
}
