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

#if NET_2_0

using System;
using System.Collections;
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	public class ImportCollection : ICollection, IEnumerable {
		
		IList		imports;
		Project		parentProject;
		
		internal ImportCollection (Project parentProject)
		{
			this.parentProject = parentProject;
			this.imports = new ArrayList ();
		}
		
		internal void Add (Import import)
		{
			if (import == null)
				throw new ArgumentNullException ("import");
			
			if (imports.Contains (import))
				throw new InvalidOperationException ("Import already added.");
			
			imports.Add (import);
		}
		
		[MonoTODO]
		public void CopyTo (Array array, int index)
		{
		}
		
		[MonoTODO]
		public void CopyTo (Import[] array, int index)
		{
		}
		
		public IEnumerator GetEnumerator ()
		{
			foreach (Import i in imports)
				yield return i;
		}
		
		public int Count {
			get { return imports.Count; }
		}
		
		public bool IsSynchronized  {
			get { return false; }
		}
		
		public object SyncRoot {
			get { return this; }
		}
	}
}

#endif
