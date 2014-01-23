//
// ProjectItemDefinitionInstance.cs
//
// Author:
//   Atsushi Enomoto (atsushi@veritas-vos-liberabit.com)
//
// Copyright (C) 2012,2013 Xamarin Inc.
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

using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Build.Construction;
using System.Linq;

namespace Microsoft.Build.Execution
{
	public class ProjectItemDefinitionInstance
	{
		internal ProjectItemDefinitionInstance (ProjectItemDefinitionElement xml)
		{
			ItemType = xml.ItemType;
			AddItems (xml);
		}
		
		List<ProjectMetadataInstance> metadata = new List<ProjectMetadataInstance> ();
		
		public string ItemType { get; private set; }
		
		public ICollection<ProjectMetadataInstance> Metadata {
			get { return metadata; }
		}
		
		public int MetadataCount {
			get { return metadata.Count; }
		}
		
		public IEnumerable<string> MetadataNames {
			get { return metadata.Select (m => m.Name).ToArray (); }
		}
		
		internal void AddItems (ProjectItemDefinitionElement xml)
		{
			foreach (var item in xml.Metadata) {
				var existing = metadata.FirstOrDefault (i => i.Name == item.Name);
				if (existing != null)
					metadata.Remove (existing);
				metadata.Add (new ProjectMetadataInstance (item.Name, item.Value));
			}
		}
	}
}
