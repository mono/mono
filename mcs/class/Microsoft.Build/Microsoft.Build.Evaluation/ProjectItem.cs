//
// ProjectItem.cs
//
// Author:
//   Leszek Ciesielski (skolima@gmail.com)
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// (C) 2011 Leszek Ciesielski
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
using System.Diagnostics;
using System.Linq;
using Microsoft.Build.Construction;
using System.IO;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Evaluation
{
	[DebuggerDisplay ("{ItemType}={EvaluatedInclude} [{UnevaluatedInclude}] #DirectMetadata={DirectMetadataCount}")]
	public class ProjectItem
	{
		internal ProjectItem (Project project, ProjectItemElement xml, string evaluatedInclude)
		{
			this.project = project;
			this.xml = xml;
			if (project.ItemDefinitions.ContainsKey (ItemType))
				foreach (var md in project.ItemDefinitions [ItemType].Metadata)
					metadata.Add (md);
			foreach (var md in xml.Metadata)
				metadata.Add (new ProjectMetadata (project, ItemType, metadata, m => metadata.Remove (m), md));
			this.evaluated_include = evaluatedInclude;
			is_imported = project.ProjectCollection.OngoingImports.Any ();			
		}
		
		readonly Project project;
		readonly ProjectItemElement xml;
		readonly List<ProjectMetadata> metadata = new List<ProjectMetadata> ();
		readonly bool is_imported;
		readonly string evaluated_include;
		
		internal string RecursiveDir { get; set; }

		public ProjectMetadata GetMetadata (string name)
		{
			return metadata.FirstOrDefault (m => m.Name == name);
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

		public bool RemoveMetadata (string name)
		{
			var m = GetMetadata (name);
			if (m == null)
				return false;
			return metadata.Remove (m);
		}

		public void Rename (string name)
		{
			throw new NotImplementedException ();
		}

		public ProjectMetadata SetMetadataValue (string name, string unevaluatedValue)
		{
			// This has to do several tasks:
			// - it cannot directly change Xml.Metadata because the ProjectItemElement might be shared
			//   among multiple ProjectItems.
			// - hence it has to create another ProjectItemElement instance and add it to the project
			//   XML construction, with specific Include value that is assigned to this instance, and
			//   metadata values that are assigned to this instance.
			throw new NotImplementedException ();
		}

		public IEnumerable<ProjectMetadata> DirectMetadata {
			get {
				var list = new List<ProjectMetadata> ();
				foreach (var xm in xml.Metadata)
					yield return new ProjectMetadata (project, ItemType, list, p => list.Remove (p), xm);
			}
		}

		public int DirectMetadataCount {
			get { return xml.Metadata.Count; }
		}

		public string EvaluatedInclude {
			get { return evaluated_include; }
		}

		public bool IsImported {
			get { return is_imported; }
		}

		public string ItemType {
			get { return Xml.ItemType; }
			set { Xml.ItemType = value; }
		}

		public ICollection<ProjectMetadata> Metadata {
			get { return metadata; }
		}

		public int MetadataCount {
			get { return metadata.Count; }
		}

		public Project Project {
			get { return project; }
		}

		public string UnevaluatedInclude {
			get { return Xml.Include; }
			set { Xml.Include = value; }
		}

		public ProjectItemElement Xml {
			get { return xml; }
		}		
	}
}
