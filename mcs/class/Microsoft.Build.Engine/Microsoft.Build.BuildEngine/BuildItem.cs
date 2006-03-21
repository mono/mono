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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	public class BuildItem {

		XmlElement	itemElement;
		XmlAttribute	condition;
		XmlAttribute	exclude;
		XmlAttribute	include;
		string		evaluatedItemSpec;
		Hashtable	evaluatedMetadata;
		string		finalItemSpec;
		bool		isImported;
		string		name;
		BuildItemGroup	parentItemGroup;
		string		recursiveDir;
		Hashtable	unevaluatedMetadata;

		internal bool FromXml {
			get {
				return itemElement != null;
			}
		}
	
		private BuildItem ()
		{
		}
		
		public BuildItem (string itemName, ITaskItem taskItem)
			: this (itemName, taskItem.ItemSpec)
		{
		}

		public BuildItem (string itemName, string itemInclude)
		{
			this.name = itemName;
			this.finalItemSpec = itemInclude;
			this.evaluatedItemSpec = itemInclude;
			this.isImported = false;
			this.unevaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
			this.evaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
		}
		
		internal BuildItem (XmlElement itemElement, BuildItemGroup parentItemGroup)
		{
			this.isImported = false;
			this.unevaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
			this.evaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
			this.parentItemGroup = parentItemGroup;
			BindToXml (itemElement);
		}
		
		private BuildItem (BuildItem parent)
		{
			this.isImported = parent.isImported;
			this.name = parent.name;
			this.parentItemGroup = parent.parentItemGroup;
			this.unevaluatedMetadata = (Hashtable) parent.unevaluatedMetadata.Clone ();
			this.evaluatedMetadata = (Hashtable) parent.evaluatedMetadata.Clone ();
			this.include = parent.include;
			this.exclude = parent.exclude;
		}
		
		internal BuildItem (string name, ITaskItem item,
				    BuildItemGroup parentItemGroup)
		{
			this.isImported = false;
			this.name = name;
			this.finalItemSpec = item.ItemSpec;
			this.evaluatedMetadata = (Hashtable) item.CloneCustomMetadata ();
			this.unevaluatedMetadata = (Hashtable) item.CloneCustomMetadata ();
		}
		
		public void CopyCustomMetadataTo (BuildItem destinationItem)
		{
			foreach (DictionaryEntry de in unevaluatedMetadata)
				destinationItem.SetMetadata ((string) de.Key, (string) de.Value);
		}
		
		[MonoTODO]
		public BuildItem Clone ()
		{
			return (BuildItem) this.MemberwiseClone ();
		}

		public string GetEvaluatedMetadata (string metadataName)
		{
			if (evaluatedMetadata.Contains (metadataName))
				return (string) evaluatedMetadata [metadataName];
			else
				return null;
		}

		public string GetMetadata (string metadataName)
		{
			if (evaluatedMetadata.Contains (metadataName) == true)
				return (string) evaluatedMetadata [metadataName];
			else
				return CheckBuiltinMetadata (metadataName);
		}
		
		private string CheckBuiltinMetadata (string metadataName)
		{
			switch (metadataName.ToLower ()) {
			case "fullpath":
				return Path.GetFullPath (finalItemSpec);
			case "rootdir":
				return "/";
			case "filename":
				return Path.GetFileNameWithoutExtension (finalItemSpec);
			case "extension":
				return Path.GetExtension (finalItemSpec);
			case "relativedir":
				return Path.GetDirectoryName (finalItemSpec);
			case "directory":
				return Path.GetDirectoryName (Path.GetFullPath (finalItemSpec));
			case "recursivedir":
				return recursiveDir;
			case "identity":
				return Path.Combine (Path.GetDirectoryName (finalItemSpec), Path.GetFileName (finalItemSpec));
			case "modifiedtime":
				if (File.Exists (finalItemSpec))
					return File.GetLastWriteTime (finalItemSpec).ToString ();
				return String.Empty;
			case "createdtime":
				if (File.Exists (finalItemSpec))
					return File.GetCreationTime (finalItemSpec).ToString ();
				return String.Empty;
			case "accessedtime":
				if (File.Exists (finalItemSpec))
					return File.GetLastAccessTime (finalItemSpec).ToString ();
				return String.Empty;
			default:
				return String.Empty;
			}
		}

		public bool HasMetadata (string metadataName)
		{
			return evaluatedMetadata.Contains (metadataName);
		}

		public void RemoveMetadata (string metadataName)
		{
			if (evaluatedMetadata.Contains (metadataName))
				evaluatedMetadata.Remove (metadataName);
			if (unevaluatedMetadata.Contains (metadataName))
				unevaluatedMetadata.Remove (metadataName);
		}

		public void SetMetadata (string metadataName,
					 string metadataValue)
		{
			SetMetadata (metadataName, metadataValue, false);
		}
		
		// FIXME: don't use expression when we treat it as literal
		[MonoTODO]
		public void SetMetadata (string metadataName,
					 string metadataValue,
					 bool treatMetadataValueAsLiteral)
		{
			RemoveMetadata (metadataName);
			unevaluatedMetadata.Add (metadataName, metadataValue);
			Expression finalValue = new Expression (parentItemGroup.Project, metadataValue);
			evaluatedMetadata.Add (metadataName, (string) finalValue.ToNonArray (typeof (string)));
		}
		
		private void BindToXml (XmlElement xmlElement)
		{
			if (xmlElement == null)
				throw new ArgumentNullException ("xmlElement");
			this.itemElement = xmlElement;
			this.name = xmlElement.Name;
			this.condition = xmlElement.GetAttributeNode ("Condition");
			this.exclude = xmlElement.GetAttributeNode ("Exclude");
			this.include = xmlElement.GetAttributeNode ("Include"); 
			if (include == null)
				throw new InvalidProjectFileException ("Item must have Include attribute.");
			foreach (XmlElement xe in xmlElement.ChildNodes) {
				this.SetMetadata (xe.Name, xe.InnerText);
			}
		}

		internal void Evaluate()
		{
			DirectoryScanner directoryScanner;
			Expression includeExpr, excludeExpr;
			string includes, excludes;

			includeExpr = new Expression (parentItemGroup.Project, Include);
			excludeExpr = new Expression (parentItemGroup.Project, Exclude);
			
			includes = (string) includeExpr.ToNonArray (typeof (string));
			excludes = (string) excludeExpr.ToNonArray (typeof (string));

			this.evaluatedItemSpec = includes;
			this.finalItemSpec = includes;
			
			directoryScanner = new DirectoryScanner ();
			
			directoryScanner.Includes = includes;
			directoryScanner.Excludes = excludes;
			if (parentItemGroup.Project.FullFileName != String.Empty) {
				directoryScanner.BaseDirectory = new DirectoryInfo (Path.GetDirectoryName (parentItemGroup.Project.FullFileName));
			} else {
				directoryScanner.BaseDirectory = new DirectoryInfo (Directory.GetCurrentDirectory ());
			}
			
			directoryScanner.Scan ();
			
			foreach (string matchedFile in directoryScanner.MatchedFilenames) {
				AddEvaluatedItem (matchedFile);
			}
		}
		
		private void AddEvaluatedItem (string itemSpec)
		{
			Project project = this.parentItemGroup.Project;
			
			BuildItem bi = new BuildItem (this);
			bi.finalItemSpec = itemSpec;
			project.EvaluatedItems.AddItem (bi);
			if (project.EvaluatedItemsByName.Contains (bi.name) == false) {
				BuildItemGroup big = new BuildItemGroup (null, project);
				project.EvaluatedItemsByName.Add (bi.name, big);
				big.AddItem (bi);
			} else {
				((BuildItemGroup) project.EvaluatedItemsByName [bi.name]).AddItem (bi);
			}
		}
		
		internal string ToString (Expression transform)
		{
			return GetItemSpecFromTransform (transform);
		}
		
		internal ITaskItem ToITaskItem (Expression transform)
		{
			TaskItem taskItem;
			taskItem = new TaskItem (GetItemSpecFromTransform (transform), (IDictionary) evaluatedMetadata.Clone ());
			return taskItem;
		}

		private string GetItemSpecFromTransform (Expression transform)
		{
			StringBuilder sb;
		
			if (transform == null)
				return finalItemSpec;
			else {
				sb = new StringBuilder ();
				foreach (object o in transform) {
					if (o is string) {
						sb.Append ((string)o);
					} else if (o is PropertyReference) {
						sb.Append (((PropertyReference)o).ToString ());
					} else if (o is ItemReference) {
						sb.Append (((ItemReference)o).ToString ());
					} else if (o is MetadataReference) {
						sb.Append (GetMetadata (((MetadataReference)o).MetadataName));
					}
				}
				return sb.ToString ();
			}
		}

		public string Condition {
			get { return itemElement.GetAttribute ("Condition"); }
			set { itemElement.SetAttribute ("Condition", value); }
		}

		public string Exclude {
			get { return itemElement.GetAttribute ("Exclude"); }
			set { itemElement.SetAttribute ("Exclude", value); }
		}

		public string FinalItemSpec {
			get { return finalItemSpec; }
		}

		public string Include {
			get { return itemElement.GetAttribute ("Include"); }
			set { itemElement.SetAttribute ("Include", value); }
		}

		public bool IsImported {
			get { return isImported; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}
	}
}

#endif
