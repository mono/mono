//
// GroupingCollection.cs: Represents group of BuildItemGroup,
// BuildPropertyGroup and BuildChoose.
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

using System.Collections;

namespace Microsoft.Build.BuildEngine {
	internal class GroupingCollection : IEnumerable {
		
		IList	allGroups;
		int	itemGroups;
		int	propertyGroups;
		int	chooses;
	
		public GroupingCollection ()
		{
			allGroups = new ArrayList ();
		}

		public IEnumerator GetChooseEnumerator ()
		{
			foreach (object o in allGroups)
				if (o is BuildChoose)
					yield return o;
		}

		public IEnumerator GetEnumerator ()
		{
			foreach (object o in allGroups) {
				if (o is BuildItemGroup) {
					yield return o;
					continue;
				}
				if (o is BuildPropertyGroup) {
					yield return o;
					continue;
				}
				if (o is BuildChoose) {
					yield return o;
				}
			}
		}

		public IEnumerator GetItemGroupAndChooseEnumerator ()
		{
			foreach (object o in allGroups) {
				if (o is BuildItemGroup) {
					yield return o;
					continue;
				}
				if (o is BuildChoose) {
					yield return o;
				}
			}
		}

		public IEnumerator GetItemGroupEnumerator ()
		{
			foreach (object o in allGroups)
				if (o is BuildItemGroup)
					yield return o;
		}

		public IEnumerator GetPropertyGroupAndChooseEnumerator ()
		{
			foreach (object o in allGroups) {
				if (o is BuildPropertyGroup) {
					yield return o;
					continue;
				}
				if (o is BuildChoose) {
					yield return o;
				}
			}
		}

		public IEnumerator GetPropertyGroupEnumerator ()
		{
			foreach (object o in allGroups) {
				if (o is BuildPropertyGroup)
					yield return o;
			}
		}
		
		internal void Add (BuildPropertyGroup bpg) {
			bpg.GroupingCollection = this;
			allGroups.Add (bpg);
			propertyGroups++;
		}
		
		internal void Add (BuildItemGroup big) {
			allGroups.Add (big);
			itemGroups++;
		}
		
		internal void Add (BuildChoose bc) {
			allGroups.Add (bc);
			chooses++;
		}
		
		internal int ItemGroups {
			get { return this.itemGroups; }
		}
		
		internal int PropertyGroups {
			get { return this.propertyGroups; }
		}
		
		internal int Chooses {
			get { return this.chooses; }
		} 
	}
}

#endif