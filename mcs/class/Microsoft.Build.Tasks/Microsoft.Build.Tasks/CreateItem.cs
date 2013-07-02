//
// CreateItem.cs: Creates build item.
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
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Tasks {
	public class CreateItem : TaskExtension {
	
		string[]	additionalMetadata;
		ITaskItem[]	exclude;
		ITaskItem[]	include;
		bool		preserveExistingMetadata;

		public CreateItem ()
		{
		}

		public override bool Execute ()
		{
			if (include == null || include.Length == 0)
				return true;

			// Handle wild cards
			var directoryScanner = new Microsoft.Build.BuildEngine.DirectoryScanner ();
			directoryScanner.Includes = include;
			directoryScanner.Excludes = exclude;
			directoryScanner.BaseDirectory = new DirectoryInfo (Directory.GetCurrentDirectory ());

			directoryScanner.Scan ();

			List<ITaskItem> output = new List<ITaskItem> ();
			foreach (ITaskItem matchedItem in directoryScanner.MatchedItems) {
				output.Add (matchedItem);
				if (AdditionalMetadata == null)
					continue;

				foreach (string metadata in AdditionalMetadata) {
					//a=1
					string [] parts = metadata.Split (new char [] {'='}, 2, StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length == 2) {
						string name = parts [0].Trim ();
						string oldValue = matchedItem.GetMetadata (name);
						if (!preserveExistingMetadata || string.IsNullOrEmpty (oldValue))
							matchedItem.SetMetadata (name, parts [1].Trim ());
					}
				}
			}

			include = output.ToArray ();

			return true;
		}

		public string[] AdditionalMetadata {
			get { return additionalMetadata; }
			set { additionalMetadata = value; }
		}

		public ITaskItem[] Exclude {
			get { return exclude; }
			set { exclude = value; }
		}

		[Output]
		public ITaskItem[] Include {
			get { return include; }
			set { include = value; }
		}

#if NET_3_5
		public bool PreserveExistingMetadata {
			get { return preserveExistingMetadata; }
			set { preserveExistingMetadata = value; }
		}
#endif
	}
}

#endif
