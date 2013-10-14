// ProjectItemDefinition.cs
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
using Microsoft.Build.Construction;

namespace Microsoft.Build.Evaluation
{
	public class ProjectItemDefinition
	{
		internal ProjectItemDefinition (Project project, string itemType)
		{
			this.project = project;
			this.item_type = itemType;
		}

		Project project;
		string item_type;
		List<ProjectMetadata> metadata = new List<ProjectMetadata> ();

		public string ItemType {
			get { return item_type; }
		}

		public IEnumerable<ProjectMetadata> Metadata {
			get { return metadata; }
		}

		public int MetadataCount {
			get { return metadata.Count; }
		}

		public Project Project {
			get { return project; }
		}
		
		internal void AddItems (ProjectItemDefinitionElement xml)
		{
			foreach (var item in xml.Metadata)
				metadata.Add (new ProjectMetadata (project, ItemType, metadata, m => metadata.Remove (m), item));
		}
	}
}
