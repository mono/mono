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
using System.Collections.Generic;

namespace Microsoft.Build.BuildEngine {
	internal class GroupingCollection : IEnumerable {
		
		int	imports;
		int	itemGroups;
		Project	project;
		int	propertyGroups;
		int	chooses;

		LinkedList <object>	list;
		LinkedListNode <object>	iterator;
	
		public GroupingCollection (Project project)
		{
			list = new LinkedList <object> ();
			iterator = null;
			this.project = project;
		}

		public IEnumerator GetChooseEnumerator ()
		{
			foreach (object o in list)
				if (o is BuildChoose)
					yield return o;
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public IEnumerator GetImportEnumerator ()
		{
			foreach (object o in list)
				if (o is Import)
					yield return o;
		}

		public IEnumerator GetItemGroupAndChooseEnumerator ()
		{
			foreach (object o in list)
				if (o is BuildItemGroup || o is BuildPropertyGroup)
					yield return o;
		}

		public IEnumerator GetItemGroupEnumerator ()
		{
			foreach (object o in list)
				if (o is BuildItemGroup)
					yield return o;
		}

		public IEnumerator GetPropertyGroupAndChooseEnumerator ()
		{
			foreach (object o in list)
				if (o is BuildPropertyGroup || o is BuildChoose)
					yield return o;
		}

		public IEnumerator GetPropertyGroupEnumerator ()
		{
			foreach (object o in list)
				if (o is BuildPropertyGroup)
					yield return o;
		}
		
		internal void Add (BuildPropertyGroup bpg)
		{
			bpg.GroupingCollection = this;
			propertyGroups++;
			if (iterator == null)
				list.AddLast (bpg);
			else
				list.AddAfter (iterator, bpg);
		}
		
		internal void Add (BuildItemGroup big)
		{
			itemGroups++;
			if (iterator == null)
				list.AddLast (big);
			else
				list.AddAfter (iterator, big);
		}
		
		internal void Add (BuildChoose bc)
		{
			chooses++;
			if (iterator == null)
				list.AddLast (bc);
			else
				list.AddAfter (iterator, bc);
		}

		internal void Add (Import import)
		{
			imports++;
			if (iterator == null)
				list.AddLast (import);
			else
				list.AddAfter (iterator, import);
		}

		internal void Evaluate ()
		{
			Evaluate (EvaluationType.Property);

			Evaluate (EvaluationType.Item);
		}

		// check what happens with order: import -> 1 2 (probably is entered in wrong order)
		void Evaluate (EvaluationType type)
		{
			BuildItemGroup big;
			BuildPropertyGroup bpg;
			Import import;

			if (type == EvaluationType.Property) {
				iterator = list.First;

				while (iterator != null) {
					if (iterator.Value is BuildPropertyGroup) {
						bpg = (BuildPropertyGroup) iterator.Value;
						if (ConditionParser.ParseAndEvaluate (bpg.Condition, project))
							bpg.Evaluate ();
					} else if (iterator.Value is Import) {
						import = (Import) iterator.Value;
						if (ConditionParser.ParseAndEvaluate (import.Condition, project))
							import.Evaluate ();
					}

					iterator = iterator.Next;
				}
			} else {
				iterator = list.First;

				while (iterator != null) {
					if (iterator.Value is BuildItemGroup) {
						big = (BuildItemGroup) iterator.Value;
						if (ConditionParser.ParseAndEvaluate (big.Condition, project))
							big.Evaluate ();
					}

					iterator = iterator.Next;
				}
			}
		}

		internal int Imports {
			get { return this.imports; }
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

	enum EvaluationType {
		Property,
		Item
	}
}

#endif
