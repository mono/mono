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

using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	public class BuildItemGroup : IEnumerable {
	
		List <BuildItem>	buildItems;
		ImportedProject		importedProject;
		XmlElement		itemGroupElement;
		GroupingCollection	parentCollection;
		Project			parentProject;
		bool			read_only;
		bool			evaluated;
		bool			isDynamic;

		public BuildItemGroup ()
			: this (null, null, null, false)
		{
		}

		internal BuildItemGroup (Project project)
			: this (null, project, null, false)
		{
		}

		internal BuildItemGroup (XmlElement xmlElement, Project project, ImportedProject importedProject, bool readOnly)
			: this (xmlElement, project, importedProject, readOnly, false)
		{
		}

		internal BuildItemGroup (XmlElement xmlElement, Project project, ImportedProject importedProject, bool readOnly, bool dynamic)
		{
			this.buildItems = new List <BuildItem> ();
			this.importedProject = importedProject;
			this.itemGroupElement = xmlElement;
			this.parentProject = project;
			this.read_only = readOnly;
			this.isDynamic = dynamic;
			
			if (!FromXml)
				return;

			foreach (XmlNode xn in xmlElement.ChildNodes) {
				if (!(xn is XmlElement))
					continue;
					
				XmlElement xe = (XmlElement) xn;
				BuildItem bi = CreateItem (project, xe);
				buildItems.Add (bi);
				project.LastItemGroupContaining [bi.Name] = this;
			}

			DefinedInFileName = importedProject != null ? importedProject.FullFileName :
						project != null ? project.FullFileName : null;
		}

		internal virtual BuildItem CreateItem (Project project, XmlElement xe)
		{
			return new BuildItem (xe, this);
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
			BuildItem item;

			if (treatItemIncludeAsLiteral)
				itemInclude = Utilities.Escape (itemInclude);

			if (FromXml) {
				XmlElement element = itemGroupElement.OwnerDocument.CreateElement (itemName, Project.XmlNamespace);
				itemGroupElement.AppendChild (element);
				element.SetAttribute ("Include", itemInclude);
				item = new BuildItem (element, this);
			} else {
				item = new BuildItem (itemName, itemInclude);
				item.ParentItemGroup = this;
			}

			item.Evaluate (null, true);

			if (!read_only)
				buildItems.Add (item);

			if (parentProject != null) {
				parentProject.MarkProjectAsDirty ();
				parentProject.NeedToReevaluate ();
			}

			return item;
		}
		
		public void Clear ()
		{
			if (FromXml)
				itemGroupElement.RemoveAll ();
			
			buildItems = new List <BuildItem> ();

			if (parentProject != null) {
				parentProject.MarkProjectAsDirty ();
				parentProject.NeedToReevaluate ();
			}
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
			if (itemToRemove == null)
				return;

			itemToRemove.Detach ();

			buildItems.Remove (itemToRemove);
		}

		public void RemoveItemAt (int index)
		{
			BuildItem item = buildItems [index];

			RemoveItem (item);
		}

		internal BuildItem FindItem (ITaskItem taskItem)
		{
			return buildItems.FirstOrDefault (i => i.FinalItemSpec == taskItem.ItemSpec);
		}

		internal void RemoveItem (ITaskItem itemToRemove)
		{
			if (itemToRemove == null)
				return;

			var item = FindItem (itemToRemove);
			if (item == null)
				return;

			item.Detach ();
			buildItems.Remove (item);
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
			buildItem.ParentItemGroup = this;
			buildItems.Add (buildItem);
		}

		// In eval phase, any ref'ed item would've already been expanded
		// or it doesnt exist, so dont expand again
		// In non-eval, items have _already_ been expanded, so dont expand again
		// So, ignore @options
		internal string ConvertToString (Expression transform,
						 Expression separator, ExpressionOptions options)
		{
			string separatorString;
			
			// Item refs are not expanded for separator or transform
			if (separator == null)
				separatorString = ";";
			else
				separatorString = (string) separator.ConvertTo (parentProject, typeof (string),
								ExpressionOptions.DoNotExpandItemRefs);
		
			string[] items = new string [buildItems.Count];
			int i = 0;
			foreach (BuildItem bi in  buildItems)
				items [i++] = bi.ConvertToString (transform, ExpressionOptions.DoNotExpandItemRefs);
			return String.Join (separatorString, items);
		}

		// In eval phase, any ref'ed item would've already been expanded
		// or it doesnt exist, so dont expand again
		// In non-eval, items have _already_ been expanded, so dont expand again
		// So, ignore @options
		internal ITaskItem[] ConvertToITaskItemArray (Expression transform, Expression separator, ExpressionOptions options)
		{
			if (separator != null)
				// separator present, so return as a single "join'ed" string
				return new ITaskItem [] {
					new TaskItem (ConvertToString (transform, separator, options))
				};

			ITaskItem[] array = new ITaskItem [buildItems.Count];
			int i = 0;
			foreach (BuildItem item in buildItems)
				array [i++] = item.ConvertToITaskItem (transform, ExpressionOptions.DoNotExpandItemRefs);
			return array;
		}

		internal void Detach ()
		{
			if (!FromXml)
				throw new InvalidOperationException ();

			itemGroupElement.ParentNode.RemoveChild (itemGroupElement);
		}

		internal void Evaluate ()
		{
			if (!isDynamic && evaluated)
				return;
			foreach (BuildItem bi in buildItems) {
				if (bi.Condition == String.Empty)
					bi.Evaluate (parentProject, true);
				else {
					ConditionExpression ce = ConditionParser.ParseCondition (bi.Condition);
					bi.Evaluate (parentProject, ce.BoolEvaluate (parentProject));
				}
			}
			evaluated = true;
		}		

		internal void ReplaceWith (BuildItem item, List <BuildItem> list)
		{
			int index = buildItems.IndexOf (item);
			buildItems.RemoveAt (index);
			buildItems.InsertRange (index, list);
		}
		
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
		
		internal Project ParentProject {
			get { return parentProject; }
			set {
				if (parentProject != null)
					throw new InvalidOperationException ("parentProject is already set");
				parentProject = value;
			}
		}

		internal string DefinedInFileName { get; private set; }

		internal bool FromXml {
			get {
				return itemGroupElement != null;
			}
		}

		internal XmlElement XmlElement {
			get {
				return itemGroupElement;
			}	
		}

		internal bool IsDynamic {
			get {
				return isDynamic;
			}
		}
	}
}
