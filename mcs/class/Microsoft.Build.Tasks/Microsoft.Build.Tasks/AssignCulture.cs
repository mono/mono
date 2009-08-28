//
// AssignCulture.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Ankit Jain (jankit@novell.com)
//
// (C) 2006 Marek Sieradzki
// Copyright 2008 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {
	public class AssignCulture : TaskExtension {
	
		ITaskItem[]	assignedFiles;
		ITaskItem[]	assignedFilesWithCulture;
		ITaskItem[]	assignedFilesWithNoCulture;
		ITaskItem[]	cultureNeutralAssignedFiles;
		ITaskItem[]	files;
	
		public AssignCulture ()
		{
		}
		
		public override bool Execute ()
		{
			if (files.Length == 0)
				return true;

			List<ITaskItem> all_files = new List<ITaskItem> ();
			List<ITaskItem> with_culture = new List<ITaskItem> ();
			List<ITaskItem> no_culture = new List<ITaskItem> ();
			List<ITaskItem> culture_neutral = new List<ITaskItem> ();

			foreach (ITaskItem item in files) {
				string only_filename, culture, extn;

				if (TrySplitResourceName (item.ItemSpec, out only_filename, out culture, out extn)) {
					//valid culture found
					ITaskItem new_item = new TaskItem (item);
					new_item.SetMetadata ("Culture", culture);
					all_files.Add (new_item);

					new_item = new TaskItem (item);
					new_item.SetMetadata ("Culture", culture);
					with_culture.Add (new_item);

					new_item = new TaskItem (item);
					new_item.SetMetadata ("Culture", culture);
					new_item.ItemSpec = only_filename + "." + extn;
					culture_neutral.Add (new_item);
				} else {
					//No valid culture

					all_files.Add (item);
					no_culture.Add (item);
					culture_neutral.Add (item);
				}
			}

			assignedFiles = all_files.ToArray ();
			assignedFilesWithCulture = with_culture.ToArray ();
			assignedFilesWithNoCulture = no_culture.ToArray ();
			cultureNeutralAssignedFiles = culture_neutral.ToArray ();

			return true;
		}
		
		[Output]
		public ITaskItem[] AssignedFiles {
			get { return assignedFiles; }
		}
		
		[Output]
		public ITaskItem[] AssignedFilesWithCulture {
			get { return assignedFilesWithCulture; }
		}
		
		[Output]
		public ITaskItem[] AssignedFilesWithNoCulture {
			get { return assignedFilesWithNoCulture; }
		}
		
		[Output]
		public ITaskItem[] CultureNeutralAssignedFiles {
			get { return cultureNeutralAssignedFiles; }
		}
		
		[Required]
		public ITaskItem[] Files {
			get { return files; }
			set { files = value; }
		}

		//Given a filename like foo.it.resx, splits it into - foo, it, resx
		//Returns true only if a valid culture is found
		//Note: hand-written as this can get called lotsa times
		internal static bool TrySplitResourceName (string fname, out string only_filename, out string culture, out string extn)
		{
			only_filename = culture = extn = null;

			int last_dot = -1;
			int culture_dot = -1;
			int i = fname.Length - 1;
			while (i >= 0) {
				if (fname [i] == '.') {
					last_dot = i;
					break;
				}
				i --;
			}
			if (i < 0)
				return false;

			i--;
			while (i >= 0) {
				if (fname [i] == '.') {
					culture_dot = i;
					break;
				}
				i --;
			}
			if (culture_dot < 0)
				return false;

			culture = fname.Substring (culture_dot + 1, last_dot - culture_dot - 1);
			if (!CultureNamesTable.ContainsKey (culture)) {
				culture = null;
				return false;
			}

			only_filename = fname.Substring (0, culture_dot);
			extn = fname.Substring (last_dot + 1);
			return true;
		}

		static Dictionary<string, string> cultureNamesTable;
		static Dictionary<string, string> CultureNamesTable {
			get {
				if (cultureNamesTable == null) {
					cultureNamesTable = new Dictionary<string, string> ();
					foreach (CultureInfo ci in CultureInfo.GetCultures (CultureTypes.AllCultures))
						cultureNamesTable [ci.Name] = ci.Name;
				}

				return cultureNamesTable;
			}
		}

	}
}

#endif
