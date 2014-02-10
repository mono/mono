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
#if !MICROSOFT_BUILD_DLL
	public
#endif
	sealed class TaskItem : MarshalByRefObject, ITaskItem
#if NET_4_0
		, ITaskItem2
#endif
	{
		IDictionary		escapedMetadata;
		string			escapedItemSpec;

		public TaskItem ()
		{
			this.escapedItemSpec = String.Empty;
			this.escapedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
		}

		public TaskItem (ITaskItem sourceItem)
		{
			if (sourceItem == null)
				throw new ArgumentNullException ("sourceItem");

#if NET_4_0
			var ti2 = sourceItem as ITaskItem2;
			if (ti2 != null) {
				escapedItemSpec = ti2.EvaluatedIncludeEscaped;
				escapedMetadata = ti2.CloneCustomMetadataEscaped ();
			} else
#endif
			{
				escapedItemSpec = MSBuildUtils.Escape (sourceItem.ItemSpec);
				escapedMetadata = sourceItem.CloneCustomMetadata ();
				foreach (string key in new ArrayList (escapedMetadata.Keys))
					escapedMetadata [key] = MSBuildUtils.Escape ((string)escapedMetadata [key]);
			}
		}

		public TaskItem (string itemSpec)
		{
			if (itemSpec == null)
				throw new ArgumentNullException ("itemSpec");
			
			escapedItemSpec = itemSpec;
			escapedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
		}

		public TaskItem (string itemSpec, IDictionary itemMetadata)
		{
			if (itemSpec == null)
				throw new ArgumentNullException ("itemSpec");
			
			if (itemMetadata == null)
				throw new ArgumentNullException ("itemMetadata");
			
			escapedItemSpec = itemSpec;
			escapedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable (itemMetadata);
		}

		public IDictionary CloneCustomMetadata ()
		{
			IDictionary clonedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
			foreach (DictionaryEntry de in escapedMetadata)
				clonedMetadata.Add (de.Key, MSBuildUtils.Unescape ((string) de.Value));
			return clonedMetadata;
		}

		IDictionary CloneCustomMetadataEscaped ()
		{
			return CollectionsUtil.CreateCaseInsensitiveHashtable (escapedMetadata);
		}

#if NET_4_0
		IDictionary ITaskItem2.CloneCustomMetadataEscaped ()
		{
			return CloneCustomMetadataEscaped ();
		}
#endif

		public void CopyMetadataTo (ITaskItem destinationItem)
		{
			foreach (DictionaryEntry e in escapedMetadata) {
				if (destinationItem.GetMetadata ((string)e.Key) == String.Empty) {
					destinationItem.SetMetadata ((string)e.Key, MSBuildUtils.Unescape ((string)e.Value));
				}
			}
		}

		public static explicit operator string (TaskItem taskItemToCast)
		{
			return taskItemToCast.ItemSpec;
		}

		public string GetMetadata (string metadataName)
		{
			return MSBuildUtils.Unescape (GetMetadataValue (metadataName));
		}

		string GetMetadataValue (string metadataName)
		{
			if (ReservedNameUtils.IsReservedMetadataName (metadataName))
				return ReservedNameUtils.GetReservedMetadata (ItemSpec, metadataName, escapedMetadata);
			return ((string) escapedMetadata [metadataName]) ?? String.Empty;
		}

#if NET_4_0
		string ITaskItem2.GetMetadataValueEscaped (string metadataName)
		{
			return GetMetadataValue (metadataName);
		}
#endif

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
			escapedMetadata.Remove (metadataName);
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
				
			escapedMetadata [metadataName] = metadataValue;
		}

#if NET_4_0
		void ITaskItem2.SetMetadataValueLiteral (string metadataName, string metadataValue)
		{
			SetMetadata (metadataName, MSBuildUtils.Escape (metadataValue));
		}
#endif
		public override string ToString ()
		{
			return escapedItemSpec;
		}
		
		public string ItemSpec {
			get { return MSBuildUtils.Unescape (escapedItemSpec); }
			set { escapedItemSpec = value; }
		}

#if NET_4_0
		string ITaskItem2.EvaluatedIncludeEscaped {
			get { return escapedItemSpec; }
			set { escapedItemSpec = value; }
		}
#endif

		public int MetadataCount {
			get { return escapedMetadata.Count + 11; }
		}

		public ICollection MetadataNames {
			get {
				ArrayList list = new ArrayList ();
				
				foreach (string s in ReservedNameUtils.ReservedMetadataNames)
					list.Add (s);
				foreach (string s in escapedMetadata.Keys)
					list.Add (s);

				return list;
			}
		}

	}
}

#endif
