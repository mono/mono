//
// GroupingCollection.cs: Represents group of BuildItemGroup,
// BuildPropertyGroup and BuildChoose.
//
// Authors:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Marek Safar (marek.safar@gmail.com)
// 
// (C) 2005 Marek Sieradzki
// Copyright (c) 2014 Xamarin Inc. (http://www.xamarin.com)
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
		int	itemDefGroups;
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

		public IEnumerator GetItemDefinitionGroupEnumerator ()
		{
			foreach (object o in list)
				if (o is BuildItemDefinitionGroup)
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

		internal void Add (BuildItemDefinitionGroup bidg)
		{
			itemDefGroups++;
			if (add_iterator == null)
				list.AddLast (bidg);
			else {
				list.AddAfter (add_iterator, bidg);
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
			Evaluate (EvaluationType.Property | EvaluationType.Choose);
			Evaluate (EvaluationType.Definition);
			Evaluate (EvaluationType.Item);
		}

		internal void Evaluate (EvaluationType type)
		{
			add_iterator = list.First;

			for (var evaluate_iterator = list.First; evaluate_iterator != null; evaluate_iterator = evaluate_iterator.Next) {
				var bpg = evaluate_iterator.Value as BuildPropertyGroup;
				if (bpg != null) {
					if ((type & EvaluationType.Property) != 0) {
						EvaluateBuildPropertyGroup (bpg);

						// if it wasn't moved by adding anything because of evaluating a Import shift it
						if (add_iterator == evaluate_iterator)
							add_iterator = add_iterator.Next;
					}

					continue;
				}

				var bidg = evaluate_iterator.Value as BuildItemDefinitionGroup;
				if (bidg != null) {
					if ((type & EvaluationType.Definition) != 0)
						EvaluateBuildItemDefinitionGroup (bidg);
					continue;
				}

				var big = evaluate_iterator.Value as BuildItemGroup;
				if (big != null) {
					if ((type & EvaluationType.Item) != 0)
						EvaluateBuildItemGroup (big);
					continue;
				}

				var bc = evaluate_iterator.Value as BuildChoose;
				if (bc != null) {
					if ((type & EvaluationType.Choose) != 0)
						EvaluateBuildChoose (bc);
					continue;
				}

				// Should not be reached
			}

			add_iterator = null;
		}

		void EvaluateBuildItemDefinitionGroup (BuildItemDefinitionGroup bidg)
		{
			project.PushThisFileProperty (bidg.DefinedInFileName);
			try {
				if (ConditionParser.ParseAndEvaluate (bidg.Condition, project))
					bidg.Evaluate ();
			} finally {
				project.PopThisFileProperty ();
			}
		}

		void EvaluateBuildPropertyGroup (BuildPropertyGroup bpg)
		{
			project.PushThisFileProperty (bpg.DefinedInFileName);
			try {
				if (ConditionParser.ParseAndEvaluate (bpg.Condition, project))
					bpg.Evaluate ();
			} finally {
				project.PopThisFileProperty ();
			}
		}

		void EvaluateBuildItemGroup (BuildItemGroup big)
		{
			project.PushThisFileProperty (big.DefinedInFileName);
			try {
				if (ConditionParser.ParseAndEvaluate (big.Condition, project))
					big.Evaluate ();
			} finally {
				project.PopThisFileProperty ();
			}
		}

		void EvaluateBuildChoose (BuildChoose bc)
		{
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

		internal int Imports {
			get { return this.imports; }
		}

		internal int ItemDefinitionGroups {
			get { return this.itemDefGroups; }
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

	[Flags]
	enum EvaluationType {
		Property = 1 << 0,
		Item = 1 << 1,
		Choose = 1 << 2,
		Definition = 1 << 3,

		Any = Property | Item | Choose | Definition
	}
}
