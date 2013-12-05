// Toolset.cs
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

using Microsoft.Build.Construction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Build.Evaluation
{
	public class ProjectMetadata
	{
		internal ProjectMetadata (Project project, string itemType, IEnumerable<ProjectMetadata> existingMetadata, Action<ProjectMetadata> remover, ProjectMetadataElement xml)
		{
			this.xml = xml;
			this.project = project;
			item_type = itemType;
			predecessor = existingMetadata.FirstOrDefault (m => m.Name == xml.Name);
			if (predecessor != null)
				remover (predecessor);
			is_imported = Project.ProjectCollection.OngoingImports.Any ();
		}

		readonly Project project;
		readonly string item_type;
		readonly ProjectMetadataElement xml;
		readonly ProjectMetadata predecessor;
		readonly bool is_imported;

		public string EvaluatedValue {
			get { return project.ExpandString (xml.Value); }
		}

		public bool IsImported {
			get { return is_imported; }
		}

		public string ItemType {
			get { return item_type; }
		}

		public string Name {
			get { return xml.Name; }
		}

		public ProjectMetadata Predecessor {
			get { return predecessor; }
		}

		public Project Project {
			get { return project; }
		}

		public string UnevaluatedValue {
			get { return xml.Value; }
		}

		public ProjectMetadataElement Xml {
			get { return xml; }
		}
	}
}

