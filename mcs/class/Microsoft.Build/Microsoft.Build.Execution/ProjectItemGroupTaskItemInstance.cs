//
// ProjectItemGroupTaskItemInstance.cs
//
// Author:
//    Atsushi Enomoto (atsushi@xamarin.com)
//
// (C) 2013 Xamarin Inc.
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
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Build.Execution
{
	public class ProjectItemGroupTaskItemInstance
	{
		internal ProjectItemGroupTaskItemInstance (ProjectItemElement xml)
		{
			Condition = xml.Condition;
			Exclude = xml.Exclude;
			Include = xml.Include;
			ItemType = xml.ItemType;
			Metadata = xml.Metadata.Select (m => new ProjectItemGroupTaskMetadataInstance (m)).ToArray ();
			Remove = xml.Remove;
			KeepDuplicates = xml.KeepDuplicates;
			KeepMetadata = xml.KeepMetadata;
			RemoveMetadata = xml.RemoveMetadata;
			
			ConditionLocation = xml.ConditionLocation;
			ExcludeLocation = xml.ExcludeLocation;
			IncludeLocation = xml.IncludeLocation;
			Location = xml.Location;
			KeepDuplicatesLocation = xml.KeepDuplicatesLocation;
			RemoveLocation = xml.RemoveLocation;
			RemoveMetadataLocation = xml.RemoveMetadataLocation;			
		}
		
		public string Condition { get; private set; }

		public string Exclude { get; private set; }

		public string Include { get; private set; }

		public string ItemType { get; private set; }

		public string KeepDuplicates { get; private set; }

		public string KeepMetadata { get; private set; }

		public ICollection<ProjectItemGroupTaskMetadataInstance> Metadata { get; private set; }

		public string Remove { get; private set; }

		public string RemoveMetadata { get; private set; }
		public ElementLocation ConditionLocation { get; private set; }

		public ElementLocation ExcludeLocation { get; private set; }

		public ElementLocation IncludeLocation { get; private set; }

		public ElementLocation KeepDuplicatesLocation { get; private set; }

		public ElementLocation KeepMetadataLocation { get; private set; }

		public ElementLocation Location { get; private set; }

		public ElementLocation RemoveLocation { get; private set; }

		public ElementLocation RemoveMetadataLocation { get; private set; }
	}
}

