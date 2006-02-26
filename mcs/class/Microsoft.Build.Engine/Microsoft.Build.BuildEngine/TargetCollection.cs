//
// TargetCollection.cs: Collection of targets.
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

#if NET_2_0

using System;
using System.Collections;
using System.Reflection;

namespace Microsoft.Build.BuildEngine {
	public class TargetCollection : ICollection, IEnumerable {
		
		IDictionary	targetsByName;
		Project		parentProject;
	
		internal TargetCollection (Project project)
		{
			this.targetsByName = new Hashtable ();
			this.parentProject = project;
		}
	
		public Target AddNewTarget (string targetName)
		{
			Target t;
			
			t = new Target (parentProject, targetName);
			targetsByName.Add (targetName, t);
			
			return t;
		}

		public void CopyTo (Array array, int index)
		{
			targetsByName.Values.CopyTo (array, index);
		}

		public void CopyToStronglyTypedArray (Target[] array,
						      int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index");
			if (array.Rank > 1)
				throw new ArgumentException ("array is multidimensional");
			if ((array.Length > 0) && (index >= array.Length))
				throw new ArgumentException ("index is equal or greater than array.Length");
			if (index + targetsByName.Count > array.Length)
				throw new ArgumentException ("Not enough room from index to end of array for this BuildPropertyGroupCollection");
		
			IEnumerator it = GetEnumerator ();
			int i = index;
			while (it.MoveNext ()) {
				array.SetValue((Target) it.Current, i++);
			}
		}

		public bool Exists (string targetName)
		{
			return targetsByName.Contains (targetName);
		}

		public IEnumerator GetEnumerator ()
		{
			foreach (DictionaryEntry de in targetsByName) {
				yield return (Target)de.Key;
			}
		}

		public void RemoveTarget (Target targetToRemove)
		{
			targetsByName.Remove (targetToRemove.Name);
		}

		public int Count {
			get {
				return targetsByName.Count;
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

		public Target this[string index] {
			get {
				return (Target) targetsByName [index];
			}
		}
	}
}

#endif