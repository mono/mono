//
// MetadataReference.cs: Represents a metadata reference in expression.
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
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	internal class MetadataReference : IReference {
	
		string		itemName;
		string		metadataName;
		int start;
		int length;
		string original;
	
		public MetadataReference (string original, string itemName, string metadataName, int start, int length)
		{
			this.original = original;
			this.itemName = itemName;
			this.metadataName = metadataName;
			this.start = start;
			this.length = length;
		}
		
		public string ItemName {
			get { return itemName; }
		}
		
		public string MetadataName {
			get { return metadataName; }
		}
		
		public bool IsQualified {
			get { return (itemName == null) ? false : true; }
		}

		public int Start {
			get { return start; }
		}

		public int End {
			get { return start + length - 1; }
		}

		public string ConvertToString (Project project, ExpressionOptions options)
		{
			return project.GetMetadataBatched (itemName, metadataName);
		}

		public ITaskItem [] ConvertToITaskItemArray (Project project, ExpressionOptions options)
		{
			List<ITaskItem> items = new List<ITaskItem> ();
			if (IsQualified) {
				// Bucket would have item lists with same metadata values,
				// so just get the value from the first item
				BuildItemGroup group;
				if (project.TryGetEvaluatedItemByNameBatched (itemName, out group))
					BuildItemGroupToITaskItems (group, items, true);
			} else {
				// Get unique metadata values from _all_ item lists
				foreach (BuildItemGroup group in project.GetAllItemGroups ())
					BuildItemGroupToITaskItems (group, items, false);
			}

			return items.Count == 0 ? null : items.ToArray ();
		}

		// Gets metadata values from build item @group and adds as ITaskItem
		// objects to @items
		// @only_one: Batched case, all item lists would have same metadata values,
		//	      just return first one
		void BuildItemGroupToITaskItems (BuildItemGroup group, List<ITaskItem> items, bool only_one)
		{
			foreach (BuildItem item in group) {
				if (!item.HasMetadata (metadataName))
					continue;

				string metadata = item.GetMetadata (metadataName);
				if (HasTaskItem (items, metadata))
					//return only unique metadata values
					continue;

				items.Add (new TaskItem (metadata));
				if (only_one)
					break;
			}
		}

		private bool HasTaskItem (List<ITaskItem> items, string itemspec)
		{
			foreach (ITaskItem task_item in items)
				if (task_item.ItemSpec == itemspec)
					return true;
			return false;
		}

		public override string ToString ()
		{
			if (IsQualified)
				return String.Format ("%({0}.{1})", itemName, metadataName);
			else
				return String.Format ("%({0})", metadataName);
		}

		public string OriginalString {
			get { return original; }
		}
	}
}
