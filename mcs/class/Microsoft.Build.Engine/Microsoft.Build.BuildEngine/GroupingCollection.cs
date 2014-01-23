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

using System;
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
		LinkedListNode <object>	add_iterator;
	
		public GroupingCollection (Project project)
		{
			list = new LinkedList <object> ();
			add_iterator = null;
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
			if (add_iterator == null)
				list.AddLast (bpg);
			else {
				list.AddAfter (add_iterator, bpg);
				add_iterator = add_iterator.Next;
			}
		}
		
		internal void Add (BuildItemGroup big)
		{
			itemGroups++;
			if (add_iterator == null)
				list.AddLast (big);
			else {
				list.AddAfter (add_iterator, big);
				add_iterator = add_iterator.Next;
			}
		}
		
		internal void Add (BuildChoose bc)
		{
			chooses++;
			if (add_iterator == null)
				list.AddLast (bc);
			else {
				list.AddAfter (add_iterator, bc);
				add_iterator = add_iterator.Next;
			}
		}

		internal void Add (Import import)
		{
			imports++;
			if (add_iterator == null)
				list.AddLast (import);
			else {
				list.AddAfter (add_iterator, import);
				add_iterator = add_iterator.Next;
			}
		}

		internal void Remove (BuildItemGroup big)
		{
			if (big.ParentProject != project)
				throw new InvalidOperationException (
					"The \"BuildItemGroup\" object specified does not belong to the correct \"Project\" object.");

			big.Detach ();
			list.Remove (big);
		}

		internal void Remove (BuildPropertyGroup bpg)
		{
			// FIXME: add bpg.Detach ();
			bpg.XmlElement.ParentNode.RemoveChild (bpg.XmlElement);
			list.Remove (bpg);
		}

		internal void Evaluate ()
		{
			Evaluate (EvaluationType.Property);
			Evaluate (EvaluationType.Item);
			Evaluate (EvaluationType.Choose);
		}

		internal void Evaluate (EvaluationType type)
		{
			BuildItemGroup big;
			BuildPropertyGroup bpg;
			LinkedListNode <object> evaluate_iterator;

			if (type == EvaluationType.Property) {
				evaluate_iterator = list.First;
				add_iterator = list.First;

				while (evaluate_iterator != null) {
					if (evaluate_iterator.Value is BuildPropertyGroup) {
						bpg = (BuildPropertyGroup) evaluate_iterator.Value;
						project.PushThisFileProperty (bpg.DefinedInFileName);
						try {
							if (ConditionParser.ParseAndEvaluate (bpg.Condition, project))
								bpg.Evaluate ();
						} finally {
							project.PopThisFileProperty ();
						}
					}

					// if it wasn't moved by adding anything because of evaluating a Import shift it
					if (add_iterator == evaluate_iterator)
						add_iterator = add_iterator.Next;

					evaluate_iterator = evaluate_iterator.Next;
				}
			} else if (type == EvaluationType.Item) {
				evaluate_iterator = list.First;
				add_iterator = list.First;

				while (evaluate_iterator != null) {
					if (evaluate_iterator.Value is BuildItemGroup) {
						big = (BuildItemGroup) evaluate_iterator.Value;
						project.PushThisFileProperty (big.DefinedInFileName);
						try {
							if (ConditionParser.ParseAndEvaluate (big.Condition, project))
								big.Evaluate ();
						} finally {
							project.PopThisFileProperty ();
						}
					}

					evaluate_iterator = evaluate_iterator.Next;
				}
			} else if (type == EvaluationType.Choose) {
				evaluate_iterator = list.First;
				add_iterator = list.First;

				while (evaluate_iterator != null) {
					if (evaluate_iterator.Value is BuildChoose) {
						BuildChoose bc = (BuildChoose)evaluate_iterator.Value;
						project.PushThisFileProperty (bc.DefinedInFileName);
						try {
							bool whenUsed = false;
							foreach (BuildWhen bw in bc.Whens) {
								if (ConditionParser.ParseAndEvaluate (bw.Condition, project)) {
									bw.Evaluate ();
									whenUsed = true;
									break;
								}
							}
							if (!whenUsed && bc.Otherwise != null &&
								ConditionParser.ParseAndEvaluate (bc.Otherwise.Condition, project)) {
								bc.Otherwise.Evaluate ();
							}
						} finally {
							project.PopThisFileProperty ();
						}
					}

					evaluate_iterator = evaluate_iterator.Next;
				}
			}

			add_iterator = null;
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
		Item,
		Choose
	}
}
