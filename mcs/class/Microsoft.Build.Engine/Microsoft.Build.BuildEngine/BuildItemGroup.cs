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
using System.Collections.Generic;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	public class BuildItemGroup : IEnumerable {
	
		List <BuildItem>	buildItems;
		GroupingCollection	parentCollection;
		Project			parentProject;
		ImportedProject		importedProject;
		XmlElement		itemGroupElement;

		public BuildItemGroup ()
			: this (null, null, null)
		{
		}
		
		internal BuildItemGroup (XmlElement xmlElement, Project project, ImportedProject importedProject)
		{
			this.buildItems = new List <BuildItem> ();
			this.importedProject = importedProject;
			this.itemGroupElement = xmlElement;
			this.parentProject = project;
			
			if (!FromXml)
				return;

			foreach (XmlNode xn in xmlElement.ChildNodes) {
				if (!(xn is XmlElement))
					continue;
					
				XmlElement xe = (XmlElement) xn;
				BuildItem bi = new BuildItem (xe, this);
				buildItems.Add (bi);
			}
		}

		public BuildItem AddNewItem (string itemName,
					     string itemInclude)
		{
			return AddNewItem (itemName, itemInclude, false);
		}
		
		[MonoTODO]
		public BuildItem AddNewItem (string itemName,
					     string itemInclude,
					     bool treatItemIncludeAsLiteral)
		{
			if (treatItemIncludeAsLiteral)
				itemInclude = Utilities.Escape (itemInclude);

			BuildItem bi = new BuildItem (itemName, itemInclude);

			bi.Evaluate (null, true);

			buildItems.Add (bi);

			return bi;
		}
		
		public void Clear ()
		{
			buildItems = new List <BuildItem> ();
		}

		[MonoTODO]
		public BuildItemGroup Clone (bool deepClone)
		{
			if (deepClone) {
				if (FromXml)
					throw new NotImplementedException ();
				else
					throw new NotImplementedException ();
			} else {
				if (FromXml)
					throw new InvalidOperationException ("A shallow clone of this object cannot be created.");
				else
					throw new NotImplementedException ();
			}
		}

		public IEnumerator GetEnumerator ()
		{
			return buildItems.GetEnumerator ();
		}

		public void RemoveItem (BuildItem itemToRemove)
		{
			buildItems.Remove (itemToRemove);
		}

		public void RemoveItemAt (int index)
		{
			buildItems.RemoveAt (index);
		}

		public BuildItem[] ToArray ()
		{
			return buildItems.ToArray ();
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

		internal string ConvertToString (Expression transform,
						 Expression separator)
		{
			string separatorString;
			
			if (separator == null)
				separatorString = ";";
			else
				separatorString = (string) separator.ConvertTo (parentProject, typeof (string));
		
			string[] items = new string [buildItems.Count];
			int i = 0;
			foreach (BuildItem bi in  buildItems)
				items [i++] = bi.ConvertToString (transform);
			return String.Join (separatorString, items);
		}

		internal ITaskItem[] ConvertToITaskItemArray (Expression transform)
		{
			ITaskItem[] array = new ITaskItem [buildItems.Count];
			int i = 0;
			foreach (BuildItem item in buildItems)
				array [i++] = item.ConvertToITaskItem (transform);
			return array;
		}

		internal void Evaluate ()
		{
			foreach (BuildItem bi in buildItems) {
				if (bi.Condition == String.Empty)
					bi.Evaluate (parentProject, true);
				else {
					ConditionExpression ce = ConditionParser.ParseCondition (bi.Condition);
					bi.Evaluate (parentProject, ce.BoolEvaluate (parentProject));
				}
			}
		}		
		
		[MonoTODO]
		// FIXME: whether we can invoke get_Condition on BuildItemGroup not based on XML
		public string Condition {
			get {
				if (FromXml)
					return itemGroupElement.GetAttribute ("Condition");
				else
					return String.Empty;
			}
			set {
				if (FromXml)
					itemGroupElement.SetAttribute ("Condition", value);
				else
					throw new InvalidOperationException ("Cannot set a condition on an object not represented by an XML element in the project file.");
			}
		}

		public int Count {
			get { return buildItems.Count; }
		}

		public bool IsImported {
			get { return importedProject != null; }
		}

		
		[MonoTODO]
		public BuildItem this [int index] {
			get {
				return buildItems [index];
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
