//
// BuildItem.cs:
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.XBuild.Utilities;

namespace Microsoft.Build.BuildEngine {
	public class BuildItem {

		List<BuildItem> child_items;
		XmlElement	itemElement;
		string		finalItemSpec;
		bool		isImported;
		string		itemInclude;
		string		name;
		BuildItemGroup	parent_item_group;
		BuildItem	parent_item;
		//string		recursiveDir;
		IDictionary	evaluatedMetadata;
		IDictionary	unevaluatedMetadata;
		bool		isDynamic;
		bool		keepDuplicates = true;
		string		removeMetadata, keepMetadata;

		BuildItem ()
		{
		}
		
		public BuildItem (string itemName, ITaskItem taskItem)
		{
			if (taskItem == null)
				throw new ArgumentNullException ("taskItem");

			this.name = itemName;
			this.finalItemSpec = taskItem.ItemSpec;
			this.itemInclude = MSBuildUtils.Escape (taskItem.ItemSpec);
			this.evaluatedMetadata = (Hashtable) taskItem.CloneCustomMetadata ();
			this.unevaluatedMetadata = (Hashtable) taskItem.CloneCustomMetadata ();
		}

		public BuildItem (string itemName, string itemInclude)
		{
			if (itemInclude == null)
				throw new ArgumentNullException ("itemInclude");
			if (itemInclude == String.Empty)
				throw new ArgumentException ("Parameter \"itemInclude\" cannot have zero length.");

			name = itemName;
			finalItemSpec = itemInclude;
			this.itemInclude = itemInclude;
			unevaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
			evaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
		}

		internal BuildItem (XmlElement itemElement, BuildItemGroup parentItemGroup)
		{
			child_items = new List<BuildItem> ();
			isImported = parentItemGroup.IsImported;
			unevaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
			evaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
			this.parent_item_group = parentItemGroup;
			
			this.itemElement = itemElement;
			isDynamic = parentItemGroup.IsDynamic;

			if (IsDynamic) {
				if (!string.IsNullOrEmpty (Remove)) {
					if (!string.IsNullOrEmpty (Include) || !string.IsNullOrEmpty (Exclude))
						throw new InvalidProjectFileException (string.Format ("The attribute \"Remove\" in element <{0}> is unrecognized.", Name));
					if (itemElement.HasChildNodes)
						throw new InvalidProjectFileException ("Children are not allowed below an item remove element.");
				}
				if (string.IsNullOrEmpty (Include) && !string.IsNullOrEmpty (Exclude))
					throw new InvalidProjectFileException (string.Format ("The attribute \"Exclude\" in element <{0}> is unrecognized.", Name));
			} else {
				if (string.IsNullOrEmpty (Include))
					throw new InvalidProjectFileException (string.Format ("The required attribute \"Include\" is missing from element <{0}>.", Name));
				if (!string.IsNullOrEmpty (Remove))
					throw new InvalidProjectFileException (string.Format ("The attribute \"Remove\" in element <{0}> is unrecognized.", Name));
			}

			foreach (XmlAttribute attr in itemElement.Attributes) {
				if (attr.Name == "Include" || attr.Name == "Exclude" || attr.Name == "Condition")
					continue;
				if (!IsDynamic)
					throw new InvalidProjectFileException (string.Format ("The attribute \"{0}\" in element <{1}> is unrecognized.", attr.Name, Name));

				switch (attr.Name) {
				case "Remove":
					Remove = attr.Value;
					break;
				case "KeepDuplicates":
					KeepDuplicates = bool.Parse (attr.Value);
					break;
				case "RemoveMetadata":
					removeMetadata = attr.Value;
					break;
				case "KeepMetadata":
					keepMetadata = attr.Value;
					break;
				default:
					throw new InvalidProjectFileException (string.Format ("The attribute \"{0}\" in element <{1}> is unrecognized.", attr.Name, Name));
				}
			}
		}

		BuildItem (BuildItem parent)
		{
			isImported = parent.isImported;
			name = parent.Name;
			parent_item = parent;
			parent_item.child_items.Add (this);
			parent_item_group = parent.parent_item_group;
			unevaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable (parent.unevaluatedMetadata);
			evaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable (parent.evaluatedMetadata);
		}
		
		public void CopyCustomMetadataTo (BuildItem destinationItem)
		{
			if (destinationItem == null)
				throw new ArgumentNullException ("destinationItem");

			foreach (DictionaryEntry de in unevaluatedMetadata)
				destinationItem.AddMetadata ((string) de.Key, (string) de.Value);
		}
		
		[MonoTODO]
		public BuildItem Clone ()
		{
			return (BuildItem) this.MemberwiseClone ();
		}

		public string GetEvaluatedMetadata (string metadataName)
		{
			if (ReservedNameUtils.IsReservedMetadataName (metadataName)) {
				string metadata = ReservedNameUtils.GetReservedMetadata (FinalItemSpec, metadataName, evaluatedMetadata);
				return MSBuildUtils.Unescape (metadata);
			}

			if (evaluatedMetadata.Contains (metadataName))
				return (string) evaluatedMetadata [metadataName];
			else
				return String.Empty;
		}

		public string GetMetadata (string metadataName)
		{
			if (ReservedNameUtils.IsReservedMetadataName (metadataName)) {
				return ReservedNameUtils.GetReservedMetadata (FinalItemSpec, metadataName, unevaluatedMetadata);
			} else if (unevaluatedMetadata.Contains (metadataName))
				return (string) unevaluatedMetadata [metadataName];
			else
				return String.Empty;
		}
		
		public bool HasMetadata (string metadataName)
		{
			if (ReservedNameUtils.IsReservedMetadataName (metadataName))
				return true;
			else
				return evaluatedMetadata.Contains (metadataName);
		}

		public void RemoveMetadata (string metadataName)
		{
			if (metadataName == null)
				throw new ArgumentNullException ("metadataName");
			
			if (ReservedNameUtils.IsReservedMetadataName (metadataName))
				throw new ArgumentException (String.Format ("\"{0}\" is a reserved item meta-data, and cannot be modified or deleted.",
					metadataName));

			if (FromXml) {
				if (unevaluatedMetadata.Contains (metadataName)) {
					XmlNode node = itemElement [metadataName];
					itemElement.RemoveChild (node);
				}
			} else if (HasParentItem) {
				if (parent_item.child_items.Count > 1)
					SplitParentItem ();
				parent_item.RemoveMetadata (metadataName);
			} 
			
			DeleteMetadata (metadataName);
		}

		public void SetMetadata (string metadataName,
					 string metadataValue)
		{
			SetMetadata (metadataName, metadataValue, false);
		}
		
		public void SetMetadata (string metadataName,
					 string metadataValue,
					 bool treatMetadataValueAsLiteral)
		{
			SetMetadata (metadataName, metadataValue, treatMetadataValueAsLiteral, false);
		}

		void SetMetadata (string metadataName,
					 string metadataValue,
					 bool treatMetadataValueAsLiteral,
					 bool fromDynamicItem)
		{
			if (metadataName == null)
				throw new ArgumentNullException ("metadataName");
			
			if (metadataValue == null)
				throw new ArgumentNullException ("metadataValue");
			
			if (ReservedNameUtils.IsReservedMetadataName (metadataName))
				throw new ArgumentException (String.Format ("\"{0}\" is a reserved item meta-data, and cannot be modified or deleted.",
					metadataName));

			if (treatMetadataValueAsLiteral && !HasParentItem)
				metadataValue = MSBuildUtils.Escape (metadataValue);

			if (FromXml) {
				XmlElement element = itemElement [metadataName];
				if (element == null) {
					element = itemElement.OwnerDocument.CreateElement (metadataName, Project.XmlNamespace);
					element.InnerText = metadataValue;
					itemElement.AppendChild (element);
				} else
					element.InnerText = metadataValue;
			} else if (HasParentItem) {
				if (parent_item.child_items.Count > 1)
					SplitParentItem ();
				parent_item.SetMetadata (metadataName, metadataValue, treatMetadataValueAsLiteral, fromDynamicItem);
			}

			// We don't want to reevalute the project for dynamic items
			if (!fromDynamicItem && !IsDynamic && (FromXml || HasParentItem)) {
				parent_item_group.ParentProject.MarkProjectAsDirty ();
				parent_item_group.ParentProject.NeedToReevaluate ();
			}
			
			DeleteMetadata (metadataName);
			AddMetadata (metadataName, metadataValue);
		}

		void AddMetadata (string name, string value)
		{
			var options = IsDynamic ?
			              ParseOptions.AllowItemsMetadataAndSplit : ParseOptions.AllowItemsNoMetadataAndSplit;

			if (parent_item_group != null) {
				Expression e = new Expression ();
				e.Parse (value, options);
				evaluatedMetadata [name] = (string) e.ConvertTo (parent_item_group.ParentProject,
						typeof (string), ExpressionOptions.ExpandItemRefs);
			} else
				evaluatedMetadata [name] = MSBuildUtils.Unescape (value);
				
			unevaluatedMetadata [name] = value;
		}

		void DeleteMetadata (string name)
		{
			if (evaluatedMetadata.Contains (name))
				evaluatedMetadata.Remove (name);
			
			if (unevaluatedMetadata.Contains (name))
				unevaluatedMetadata.Remove (name);
		}

		internal void Evaluate (Project project, bool evaluatedTo)
		{
			// FIXME: maybe make Expression.ConvertTo (null, ...) work as MSBuildUtils.Unescape ()?
			if (project == null) {
				this.finalItemSpec = MSBuildUtils.Unescape (Include);
				return;
			}

			foreach (XmlNode xn in itemElement.ChildNodes) {
				XmlElement xe = xn as XmlElement;
				if (xe != null && ConditionParser.ParseAndEvaluate (xe.GetAttribute ("Condition"), project))
					AddMetadata (xe.Name, xe.InnerText);
			}

			if (IsDynamic) {
				if (!evaluatedTo)
					return;

				if (!string.IsNullOrEmpty (Remove)) {
					RemoveItems (project);
					return;
				}

				if (string.IsNullOrEmpty (Include)) {
					UpdateMetadata (project);
					return;
				}
			}

			DirectoryScanner directoryScanner;
			Expression includeExpr, excludeExpr;
			ITaskItem[] includes, excludes;

			var options = IsDynamic ?
				ParseOptions.AllowItemsMetadataAndSplit : ParseOptions.AllowItemsNoMetadataAndSplit;

			includeExpr = new Expression ();
			includeExpr.Parse (Include, options);
			excludeExpr = new Expression ();
			excludeExpr.Parse (Exclude, options);
			
			includes = (ITaskItem[]) includeExpr.ConvertTo (project, typeof (ITaskItem[]),
								ExpressionOptions.ExpandItemRefs);
			excludes = (ITaskItem[]) excludeExpr.ConvertTo (project, typeof (ITaskItem[]),
								ExpressionOptions.ExpandItemRefs);

			this.finalItemSpec = (string) includeExpr.ConvertTo (project, typeof (string),
							ExpressionOptions.ExpandItemRefs);

			directoryScanner = new DirectoryScanner ();
			
			directoryScanner.Includes = includes;
			directoryScanner.Excludes = excludes;

			if (project.FullFileName != String.Empty) {
				directoryScanner.ProjectFile = project.ThisFileFullPath;
				directoryScanner.BaseDirectory = new DirectoryInfo (Path.GetDirectoryName (project.FullFileName));
			}
			else
				directoryScanner.BaseDirectory = new DirectoryInfo (Directory.GetCurrentDirectory ());
			
			directoryScanner.Scan ();
			
			foreach (ITaskItem matchedItem in directoryScanner.MatchedItems)
				AddEvaluatedItem (project, evaluatedTo, matchedItem);
		}

		bool CheckCondition (Project project)
		{
			if (parent_item_group != null && !ConditionParser.ParseAndEvaluate (parent_item_group.Condition, project))
				return false;
			if (parent_item != null && !parent_item.CheckCondition (project))
				return false;
			return ConditionParser.ParseAndEvaluate (Condition, project);
		}

		void UpdateMetadata (Project project)
		{
			BuildItemGroup group;
			if (!project.TryGetEvaluatedItemByNameBatched (Name, out group))
				return;

			foreach (BuildItem item in group) {
				if (!item.CheckCondition (project))
					continue;
				
				foreach (string name in evaluatedMetadata.Keys) {
					item.SetMetadata (name, (string)evaluatedMetadata [name], false, IsDynamic);
				}

				AddAndRemoveMetadata (project, item);
			}
		}

		void AddAndRemoveMetadata (Project project, BuildItem item)
		{
			if (!string.IsNullOrEmpty (removeMetadata)) {
				var removeExpr = new Expression ();
				removeExpr.Parse (removeMetadata, ParseOptions.AllowItemsNoMetadataAndSplit);

				var removeSpec = (string[]) removeExpr.ConvertTo (
					project, typeof (string[]), ExpressionOptions.ExpandItemRefs);

				foreach (var remove in removeSpec) {
					item.DeleteMetadata (remove);
				}
			}

			if (!string.IsNullOrEmpty (keepMetadata)) {
				var keepExpr = new Expression ();
				keepExpr.Parse (keepMetadata, ParseOptions.AllowItemsNoMetadataAndSplit);

				var keepSpec = (string[]) keepExpr.ConvertTo (
					project, typeof (string[]), ExpressionOptions.ExpandItemRefs);

				var metadataNames = new string [item.evaluatedMetadata.Count];
				item.evaluatedMetadata.Keys.CopyTo (metadataNames, 0);

				foreach (string name in metadataNames) {
					if (!keepSpec.Contains (name))
						item.DeleteMetadata (name);
				}
			}
		}

		void RemoveItems (Project project)
		{
			BuildItemGroup group;
			if (!project.TryGetEvaluatedItemByNameBatched (Name, out group))
				return;

			var removeExpr = new Expression ();
			removeExpr.Parse (Remove, ParseOptions.AllowItemsNoMetadataAndSplit);

			var removes = (ITaskItem[]) removeExpr.ConvertTo (
				project, typeof (ITaskItem[]), ExpressionOptions.ExpandItemRefs);

			var directoryScanner = new DirectoryScanner ();
			
			directoryScanner.Includes = removes;

			if (project.FullFileName != String.Empty)
				directoryScanner.BaseDirectory = new DirectoryInfo (Path.GetDirectoryName (project.FullFileName));
			else
				directoryScanner.BaseDirectory = new DirectoryInfo (Directory.GetCurrentDirectory ());
			
			directoryScanner.Scan ();

			foreach (ITaskItem matchedItem in directoryScanner.MatchedItems) {
				group.RemoveItem (matchedItem);
			}
		}

		bool ContainsItem (Project project, ITaskItem taskItem)
		{
			BuildItemGroup group;
			if (!project.TryGetEvaluatedItemByNameBatched (Name, out group))
				return false;

			var item = group.FindItem (taskItem);
			if (item == null)
				return false;

			foreach (string metadataName in evaluatedMetadata.Keys) {
				string metadataValue = (string)evaluatedMetadata [metadataName];
				if (!metadataValue.Equals (item.evaluatedMetadata [metadataName]))
					return false;
			}
			
			foreach (string metadataName in item.evaluatedMetadata.Keys) {
				string metadataValue = (string)item.evaluatedMetadata [metadataName];
				if (!metadataValue.Equals (evaluatedMetadata [metadataName]))
					return false;
			}

			return true;
		}

		void AddEvaluatedItem (Project project, bool evaluatedTo, ITaskItem taskitem)
		{
			if (IsDynamic && evaluatedTo && !KeepDuplicates && ContainsItem (project, taskitem))
				return;

			BuildItemGroup big;			
			BuildItem bi = new BuildItem (this);
			bi.finalItemSpec = taskitem.ItemSpec;

			foreach (DictionaryEntry de in taskitem.CloneCustomMetadata ()) {
				bi.unevaluatedMetadata.Add ((string) de.Key, (string) de.Value);
				bi.evaluatedMetadata.Add ((string) de.Key, (string) de.Value);
			}

			project.EvaluatedItemsIgnoringCondition.AddItem (bi);

			if (evaluatedTo) {
				project.EvaluatedItems.AddItem (bi);
	
				if (!project.EvaluatedItemsByName.ContainsKey (bi.Name)) {
					big = new BuildItemGroup (null, project, null, true);
					project.EvaluatedItemsByName.Add (bi.Name, big);
				} else {
					big = project.EvaluatedItemsByName [bi.Name];
				}

				big.AddItem (bi);
			}

			if (!project.EvaluatedItemsByNameIgnoringCondition.ContainsKey (bi.Name)) {
				big = new BuildItemGroup (null, project, null, true);
				project.EvaluatedItemsByNameIgnoringCondition.Add (bi.Name, big);
			} else {
				big = project.EvaluatedItemsByNameIgnoringCondition [bi.Name];
			}

			big.AddItem (bi);

			if (IsDynamic)
				AddAndRemoveMetadata (project, bi);
		}
		
		// during item's eval phase, any item refs in this item, have either
		// already been expanded or are non-existant, so expand can be _false_
		//
		// during prop's eval phase, this isn't reached, as it parses expressions
		// with allowItems=false, so no ItemReferences are created at all
		//
		// at other times, item refs have already been expanded, so expand: false
		internal string ConvertToString (Expression transform, ExpressionOptions options)
		{
			return GetItemSpecFromTransform (transform, options);
		}
		
		internal ITaskItem ConvertToITaskItem (Expression transform, ExpressionOptions options)
		{
			TaskItem taskItem;
			taskItem = new TaskItem (GetItemSpecFromTransform (transform, options), evaluatedMetadata);
			return taskItem;
		}

		internal void Detach ()
		{
			if (FromXml)
				itemElement.ParentNode.RemoveChild (itemElement);
			else if (HasParentItem) {
				if (parent_item.child_items.Count > 1)
					SplitParentItem ();
				parent_item.Detach ();
			}
		}

		string GetItemSpecFromTransform (Expression transform, ExpressionOptions options)
		{
			StringBuilder sb;
		
			if (transform == null) {
				if (options == ExpressionOptions.ExpandItemRefs) {
					// With usual code paths, this will never execute,
					// but letting this be here, incase BI.ConvertTo*
					// is called directly
					Expression expr = new Expression ();
					expr.Parse (finalItemSpec, ParseOptions.AllowItemsNoMetadataAndSplit);

					return (string) expr.ConvertTo (parent_item_group.ParentProject,
							typeof (string), ExpressionOptions.ExpandItemRefs);
				} else {
					return finalItemSpec;
				}
			} else {
				// Transform, _DONT_ expand itemrefs
				sb = new StringBuilder ();
				foreach (object o in transform.Collection) {
					if (o is string) {
						sb.Append ((string)o);
					} else if (o is PropertyReference) {
						sb.Append (((PropertyReference)o).ConvertToString (
									parent_item_group.ParentProject,
									ExpressionOptions.DoNotExpandItemRefs));
					} else if (o is ItemReference) {
						sb.Append (((ItemReference)o).ConvertToString (
									parent_item_group.ParentProject,
									ExpressionOptions.DoNotExpandItemRefs));
					} else if (o is MetadataReference) {
						sb.Append (GetMetadata (((MetadataReference)o).MetadataName));
					}
				}
				return sb.ToString ();
			}
		}

		void SplitParentItem ()
		{
			BuildItem parent = parent_item;
			List <BuildItem> list = new List <BuildItem> ();
			XmlElement insertAfter = parent.itemElement;
			foreach (BuildItem bi in parent.child_items) {
				BuildItem added = InsertElementAfter (parent, bi, insertAfter);
				insertAfter = added.itemElement;
				list.Add (added);
			}
			parent.parent_item_group.ReplaceWith (parent, list);
			parent.itemElement.ParentNode.RemoveChild (parent.itemElement);			
		}

		static BuildItem InsertElementAfter (BuildItem parent, BuildItem child, XmlElement insertAfter)
		{
			BuildItem newParent;

			XmlDocument doc = parent.itemElement.OwnerDocument;
			XmlElement newElement = doc.CreateElement (child.Name, Project.XmlNamespace);
			newElement.SetAttribute ("Include", child.FinalItemSpec);
			if (parent.itemElement.HasAttribute ("Condition"))
				newElement.SetAttribute ("Condition", parent.itemElement.GetAttribute ("Condition"));
			foreach (XmlNode xn in parent.itemElement)
				newElement.AppendChild (xn.Clone ());
			parent.itemElement.ParentNode.InsertAfter (newElement, insertAfter);

			newParent = new BuildItem (newElement, parent.parent_item_group);
			newParent.child_items.Add (child);
			child.parent_item = newParent;

			return newParent;
		}

		public string Condition {
			get {
				if (FromXml)
					return itemElement.GetAttribute ("Condition");
				else
					return String.Empty;
			}
			set {
				if (FromXml)
					itemElement.SetAttribute ("Condition", value);
				else if (!HasParentItem)
					throw new InvalidOperationException ("Cannot set a condition on an object not represented by an XML element in the project file.");
			}
		}

		public string Exclude {
			get {
				if (FromXml)
					return itemElement.GetAttribute ("Exclude");
				else
					return String.Empty;
			}
			set {
				if (FromXml)
					itemElement.SetAttribute ("Exclude", value);
				else
					throw new InvalidOperationException ("Assigning the \"Exclude\" attribute of a virtual item is not allowed.");
			}
		}

		public string FinalItemSpec {
			get { return finalItemSpec; }
		}

		public string Include {
			get {
				if (FromXml)
					return itemElement.GetAttribute ("Include");
				else if (HasParentItem)
					return parent_item.Include;
				else
					return itemInclude;
			}
			set {
				if (FromXml)
					itemElement.SetAttribute ("Include", value);
				else if (HasParentItem) {
					if (parent_item.child_items.Count > 1)
						SplitParentItem ();
					parent_item.Include = value;
				} else
					itemInclude = value;
			}
		}

		internal bool IsDynamic {
			get { return isDynamic; }
		}

		internal string Remove {
			get;
			private set;
		}

		internal bool KeepDuplicates {
			get { return keepDuplicates; }
			private set { keepDuplicates = value; }
		}

		public bool IsImported {
			get { return isImported; }
		}

		public string Name {
			get {
				if (FromXml)
					return itemElement.Name;
				else if (HasParentItem)
					return parent_item.Name;
				else
					return name;
			}
			set {
				if (FromXml) {
					XmlElement newElement = itemElement.OwnerDocument.CreateElement (value, Project.XmlNamespace);
					newElement.SetAttribute ("Include", itemElement.GetAttribute ("Include"));
					newElement.SetAttribute ("Condition", itemElement.GetAttribute ("Condition"));
					foreach (XmlNode xn in itemElement)
						newElement.AppendChild (xn.Clone ());
					itemElement.ParentNode.ReplaceChild (newElement, itemElement);
					itemElement = newElement;
				} else if (HasParentItem) {
					if (parent_item.child_items.Count > 1)
						SplitParentItem ();
					parent_item.Name = value;
				} else
					name = value;
			}
		}
		
		internal bool FromXml {
			get { return itemElement != null; }
		}

		internal XmlElement XmlElement {
			get { return itemElement; }
		}
		
		internal bool HasParentItem {
			get { return parent_item != null; }
		}

		internal BuildItem ParentItem {
			get { return parent_item; }
		}

		internal BuildItemGroup ParentItemGroup {
			get { return parent_item_group; }
			set { parent_item_group = value; }
		}
	}
}
