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

namespace Microsoft.Build.Utilities
{
	public sealed class TaskItem : MarshalByRefObject, ITaskItem
	{
		IDictionary	metadata;
		string		itemSpec;
		string		recursiveDir;
		
		public TaskItem ()
		{
			this.itemSpec = String.Empty;
			this.recursiveDir = String.Empty;
			this.metadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
		}

		public TaskItem (ITaskItem sourceItem)
		{
			this.itemSpec = sourceItem.ItemSpec;
			this.recursiveDir = String.Empty;
			this.metadata = sourceItem.CloneCustomMetadata ();
		}

		public TaskItem (string itemSpec)
		{
			this.ItemSpec = itemSpec;
			this.recursiveDir = String.Empty;
			this.metadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
		}

		public TaskItem (string itemSpec, IDictionary itemMetadata)
		{
			this.itemSpec = itemSpec;
			this.recursiveDir = String.Empty;
			this.metadata = itemMetadata;
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
				destinationItem.SetMetadata ((string)e.Key, (string)e.Value);
			}
		}

		public static explicit operator string (TaskItem taskItemToCast)
		{
			return taskItemToCast.ToString ();
		}

		public string GetMetadata (string metadataName)
		{
			switch (metadataName.ToLower ()) {
			case "fullpath":
				return Path.GetFullPath (itemSpec);
			case "rootdir":
				return "/";
			case "filename":
				return Path.GetFileNameWithoutExtension (itemSpec);
			case "extension":
				return Path.GetExtension (itemSpec);
			case "relativedir":
				return Path.GetDirectoryName (itemSpec);
			case "directory":
				return Path.GetDirectoryName (Path.GetFullPath (itemSpec));
			case "recursivedir":
				return recursiveDir;
			case "identity":
				return Path.Combine (Path.GetDirectoryName (itemSpec), Path.GetFileName (itemSpec));
			case "modifiedtime":
				return File.GetLastWriteTime (itemSpec).ToString ();
			case "createdtime":
				return File.GetCreationTime (itemSpec).ToString ();
			case "accessedtime":
				return File.GetLastAccessTime (itemSpec).ToString ();
			default:
				return (string) metadata [metadataName];
			}
		}

		public override object InitializeLifetimeService ()
		{
			return null;
		}

		public void RemoveMetadata (string metadataName)
		{
			if (metadata.Contains (metadataName))
				metadata.Remove (metadataName);
		}

		public void SetMetadata (string metadataName, string metadataValue)
		{
			if (metadata.Contains (metadataName))
				metadata.Remove (metadataName);
			metadata.Add (metadataName, metadataValue);
		}

		public override string ToString ()
		{
			return itemSpec;
		}
		
		private ICollection CopyBasicMetadataNames (ICollection keys)
		{
			string[] basicMetadata = new string[] {"FullPath", "RootDir", "Filename", "Extension", "RelativeDir",
			"Directory", "RecursiveDir", "Identity", "ModifiedTime", "CreatedTime", "AccessedTime"};
			
			ArrayList al = new ArrayList ();
			
			foreach (string s in keys)
				al.Add (s);
			
			foreach (string s in basicMetadata)
				al.Add (s);
			
			return al;
		}

		public string ItemSpec {
			get { return itemSpec; }
			set { itemSpec = value; }
		}

		public int MetadataCount {
		// predefined metadata
			get { return metadata.Count + 11; }
		}

		public ICollection MetadataNames {
			get { return CopyBasicMetadataNames (metadata.Keys); }
		}
	}
}

#endif