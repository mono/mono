//
// BuildItemGroup.cs: Represents a group of build items.
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
using System.Reflection;
using System.Collections;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	public class BuildItemGroup : IEnumerable {
	
		XmlAttribute		condition;
		bool			isImported;
		IList			buildItems;
		GroupingCollection	parentCollection;
		Project			parentProject;
		XmlElement		itemGroupElement;

		public BuildItemGroup ()
			: this (null, null)
		{
		}
		
		internal BuildItemGroup (XmlElement xmlElement, Project project)
		{
			this.itemGroupElement = xmlElement;
			this.buildItems = new ArrayList ();
			this.isImported = false;
			this.parentProject = project;
			
			if (FromXml == false)
				return;
			
			this.condition = xmlElement.GetAttributeNode ("Condition");
			foreach (XmlNode xn in xmlElement.ChildNodes) {
				if (xn is XmlElement == false)
					return;
					
				XmlElement xe = (XmlElement) xn;
				BuildItem bi = new BuildItem (xe, this);
				buildItems.Add (bi);
			}
		}

		internal void Evaluate ()
		{
			foreach (BuildItem bi in buildItems) {
				bi.Evaluate ();
			}
		}

		public BuildItem AddNewItem (string itemName,
					     string itemInclude)
		{
			return AddNewItem (itemName, itemInclude, false);
		}
		
		// FIXME: use expression
		[MonoTODO]
		public BuildItem AddNewItem (string itemName,
					     string itemInclude,
					     bool treatItemIncludeAsLiteral)
		{
			BuildItem bi = new BuildItem (itemName, itemInclude);
			buildItems.Add (bi);
			return bi;
		}
		
		internal void AddItem (BuildItem buildItem)
		{
			buildItems.Add (buildItem);
		}
		
		internal void AddItem (string name, ITaskItem taskItem)
		{
			BuildItem buildItem;
			buildItem = new BuildItem (name, taskItem);
			buildItems.Add (buildItem);
		}

		[MonoTODO]
		public void Clear ()
		{
			//FIXME: should this remove all build items?
			buildItems = new ArrayList ();
		}

		[MonoTODO]
		public BuildItemGroup Clone (bool deepClone)
		{
			BuildItemGroup big = new BuildItemGroup ();
			// FIXME: add copying of items
			return big;
		}

		public IEnumerator GetEnumerator ()
		{
			return buildItems.GetEnumerator ();
		}

		[MonoTODO]
		public void RemoveItem (BuildItem itemToRemove)
		{
			buildItems.Remove (itemToRemove);
		}

		[MonoTODO]
		public void RemoveItemAt (int index)
		{
			buildItems.RemoveAt (index);
		}

		public BuildItem[] ToArray ()
		{
			BuildItem[] array;
			array = new BuildItem [Count];
			buildItems.CopyTo (array, 0);
			return array;
		}
		
		internal string ToString (Expression transform, string separator)
		{
			string[] items = new string [buildItems.Count];
			int i = 0;
			foreach (BuildItem bi in  buildItems)
				items [i++] = bi.ToString (transform);
			return String.Join (separator, items);
		}
		
		internal ITaskItem[] ToITaskItemArray (Expression transform)
		{
			ITaskItem[] array = new ITaskItem [buildItems.Count];
			int i = 0;
			foreach (BuildItem item in buildItems)
				array [i++] = item.ToITaskItem (transform);
			return array;
		}

		public string Condition {
			get {
				if (condition != null)
					return condition.Value;
				else
					return null;
			}
			set {
				if (condition != null)
					condition.Value = value;
			}
		}

		public int Count {
			get {
				if (buildItems != null)
					return buildItems.Count;
				else
					return 0;
			}
		}

		public bool IsImported {
			get {
				return isImported;
			}
		}

		public BuildItem this [int index] {
			get {
				return (BuildItem) buildItems [index];
			}
		}
		
		internal GroupingCollection GroupingCollection {
			get { return parentCollection; }
			set { parentCollection = value; }
		}
		
		internal Project Project {
			get { return parentProject; }
		}

		internal bool FromXml {
			get {
				return itemGroupElement != null;
			}
		}
	}
}

#endif
