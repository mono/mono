//
// TaskItem.cs: Represents an item belonging to a task.
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
using Microsoft.Build.Framework;
using Mono.XBuild.Utilities;

namespace Microsoft.Build.Utilities
{
	public sealed class TaskItem : MarshalByRefObject, ITaskItem
	{
		IDictionary		metadata;
		string			itemSpec;

		public TaskItem ()
		{
			this.itemSpec = String.Empty;
			this.metadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
		}

		public TaskItem (ITaskItem sourceItem)
		{
			if (sourceItem == null)
				throw new ArgumentNullException ("sourceItem");
			
			this.itemSpec = sourceItem.ItemSpec;
			this.metadata = sourceItem.CloneCustomMetadata ();
		}

		public TaskItem (string itemSpec)
		{
			if (itemSpec == null)
				throw new ArgumentNullException ("itemSpec");
			
			this.itemSpec = itemSpec;
			this.metadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();

			// FIXME: hack
			this.itemSpec = itemSpec.Replace ('\\', Path.DirectorySeparatorChar);
		}

		public TaskItem (string itemSpec, IDictionary itemMetadata)
		{
			if (itemSpec == null)
				throw new ArgumentNullException ("itemSpec");
			
			if (itemMetadata == null)
				throw new ArgumentNullException ("itemMetadata");
			
			this.itemSpec = itemSpec;
			this.metadata = CollectionsUtil.CreateCaseInsensitiveHashtable (itemMetadata);
		}

		public IDictionary CloneCustomMetadata ()
		{
			IDictionary clonedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
			foreach (DictionaryEntry de in metadata)
				clonedMetadata.Add (de.Key, de.Value);
			return clonedMetadata;
		}

		public void CopyMetadataTo (ITaskItem destinationItem)
		{
			foreach (DictionaryEntry e in metadata) {
				if (destinationItem.GetMetadata ((string)e.Key) == String.Empty) {
					destinationItem.SetMetadata ((string)e.Key, (string)e.Value);
				}
			}
		}

		public static explicit operator string (TaskItem taskItemToCast)
		{
			return taskItemToCast.ItemSpec;
		}

		public string GetMetadata (string metadataName)
		{
			if (ReservedNameUtils.IsReservedMetadataName (metadataName))
				return ReservedNameUtils.GetReservedMetadata (ItemSpec, metadataName, metadata);
			else if (metadata.Contains (metadataName))
				return (string) metadata [metadataName];
			else
				return String.Empty;
		}

		public override object InitializeLifetimeService ()
		{
			return null;
		}

		public void RemoveMetadata (string metadataName)
		{
			if (metadataName == null)
				throw new ArgumentNullException ("metadataName");
			if (ReservedNameUtils.IsReservedMetadataName (metadataName))
				throw new ArgumentException ("Can't remove reserved metadata");
			if (metadata.Contains (metadataName))
				metadata.Remove (metadataName);
		}

		public void SetMetadata (string metadataName, string metadataValue)
		{
			if (metadataName == null)
				throw new ArgumentNullException ("metadataName");
			if (metadataValue == null)
				throw new ArgumentNullException ("metadataValue");

			// allow RecursiveDir to be set, it gets set by DirectoryScanner
			if (String.Compare (metadataName, "RecursiveDir", StringComparison.InvariantCultureIgnoreCase) != 0 &&
				ReservedNameUtils.IsReservedMetadataName (metadataName))
				throw new ArgumentException ("Can't modify reserved metadata");
				
			if (metadata.Contains (metadataName))
				metadata.Remove (metadataName);
			metadata.Add (metadataName, metadataValue);
		}

		public override string ToString ()
		{
			return itemSpec;
		}
		
		public string ItemSpec {
			get { return itemSpec; }
			set { itemSpec = value; }
		}

		public int MetadataCount {
			get { return metadata.Count + 11; }
		}

		public ICollection MetadataNames {
			get {
				ArrayList list = new ArrayList ();
				
				foreach (string s in ReservedNameUtils.ReservedMetadataNames)
					list.Add (s);
				foreach (string s in metadata.Keys)
					list.Add (s);

				return list;
			}
		}
	}
}

#endif
