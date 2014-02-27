//
// ProjectItemInstance.cs
//
// Author:
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// Copyright (C) 2011,2013 Xamarin Inc.
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
//

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Evaluation;
using System.Collections;
using Microsoft.Build.Construction;
using System.Globalization;
using System.IO;

namespace Microsoft.Build.Execution
{
	public class ProjectItemInstance
                : ITaskItem2
	{
		internal ProjectItemInstance (ProjectInstance project, string itemType, IEnumerable<KeyValuePair<string,string>> metadata, string evaluatedInclude)
		{
			this.project = project;
			this.evaluated_include = evaluatedInclude;
			this.item_type = itemType;
			this.metadata = new List<ProjectMetadataInstance> ();
			SetMetadata (metadata);
		}
		
		readonly ProjectInstance project;
		readonly string item_type;
		string evaluated_include;
		readonly List<ProjectMetadataInstance> metadata;
		
		public ProjectMetadataInstance GetMetadata (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			// This does not return any Well Known metadata
			return Metadata.FirstOrDefault (m => m.Name.Equals (name, StringComparison.OrdinalIgnoreCase));
		}

		public string GetMetadataValue (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			var wk = ProjectCollection.GetWellKnownMetadata (name, EvaluatedInclude, project.GetFullPath, RecursiveDir);
			if (wk != null)
				return wk;
			var m = GetMetadata (name);
			return m != null ? m.EvaluatedValue : string.Empty;
		}

		public bool HasMetadata (string name)
		{
			return GetMetadata (name) != null;
		}

		public void RemoveMetadata (string metadataName)
		{
			var m = GetMetadata (metadataName);
			if (m != null)
				metadata.Remove (m);
		}

		public void SetMetadata (IEnumerable<KeyValuePair<string, string>> metadataDictionary)
		{
			foreach (var p in metadataDictionary)
				SetMetadata (p.Key, p.Value);
		}

		public ProjectMetadataInstance SetMetadata (string name, string evaluatedValue)
		{
			var m = metadata.FirstOrDefault (_ => _.Name.Equals (name, StringComparison.OrdinalIgnoreCase));
			if (m != null)
				metadata.Remove (m);
			m = new ProjectMetadataInstance (name, evaluatedValue);
			metadata.Add (m);
			return m;
		}

		public int DirectMetadataCount {
			get { throw new NotImplementedException (); }
		}

		public string EvaluatedInclude {
			get { return evaluated_include; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				evaluated_include = value;
			}
		}

		public string ItemType {
			get { return item_type; }
		}

		public IEnumerable<ProjectMetadataInstance> Metadata {
			get { return metadata; }
		}

		public int MetadataCount {
			get { return metadata.Count; }
		}

		public ICollection<string> MetadataNames {
			get { return metadata.Select (m => m.Name).ToArray (); }
		}

		public ProjectInstance Project {
			get { return project; }
		}
		
		internal string RecursiveDir { get; set; }

		#region ITaskItem2 implementation

		string ITaskItem2.GetMetadataValueEscaped (string metadataName)
		{
			return ProjectCollection.Escape (GetMetadataValue (metadataName));
		}

		void ITaskItem2.SetMetadataValueLiteral (string metadataName, string metadataValue)
		{
			SetMetadata (metadataName, metadataValue);
		}

		System.Collections.IDictionary ITaskItem2.CloneCustomMetadataEscaped ()
		{
			var dic = ((ITaskItem) this).CloneCustomMetadata ();
			foreach (DictionaryEntry p in dic)
				dic [p.Key] = ProjectCollection.Escape ((string) p.Value);
			return dic;
		}

		string ITaskItem2.EvaluatedIncludeEscaped {
			get { return ProjectCollection.Escape (EvaluatedInclude); }
			set { EvaluatedInclude = ProjectCollection.Unescape (value); }
		}

		#endregion

		#region ITaskItem implementation

		IDictionary ITaskItem.CloneCustomMetadata ()
		{
			var dic = new Hashtable ();
			foreach (var md in Metadata)
				dic [md.Name] = md.EvaluatedValue;
			return dic;
		}

		void ITaskItem.CopyMetadataTo (ITaskItem destinationItem)
		{
			if (destinationItem == null)
				throw new ArgumentNullException ("destinationItem");
			foreach (var md in Metadata)
				destinationItem.SetMetadata (md.Name, md.EvaluatedValue);
		}

		string ITaskItem.GetMetadata (string metadataName)
		{
			return GetMetadataValue (metadataName);
		}

		void ITaskItem.RemoveMetadata (string metadataName)
		{
			RemoveMetadata (metadataName);
		}

		void ITaskItem.SetMetadata (string metadataName, string metadataValue)
		{
			SetMetadata (metadataName, ProjectCollection.Unescape (metadataValue));
		}

		string ITaskItem.ItemSpec {
			get { return EvaluatedInclude; }
			set { EvaluatedInclude = value; }
		}

		int ITaskItem.MetadataCount {
			get { return MetadataCount; }
		}

		ICollection ITaskItem.MetadataNames {
			get { return MetadataNames.ToArray (); }
		}

		#endregion
	}
}

