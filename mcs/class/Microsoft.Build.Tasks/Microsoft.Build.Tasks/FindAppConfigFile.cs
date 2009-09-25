//
// FindAppConfigFile.cs: Finds a app.config in a list of files
//
// Author:
//   Ankit Jain (jankit@novell.com)
//
// Copyright 2009 Novell, Inc (http://www.novell.com)
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
using System.IO;

using Microsoft.Build.Framework; 
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {
	//FIXME: This should be in v3.5 only
	public sealed class FindAppConfigFile : TaskExtension {

		public FindAppConfigFile ()
		{
		}

		// rules: (see FindAppConfigFileTest)
		// 1. Check PrimaryList, app.config in top dir
		// 2. Check SecondaryList, app.conf in top dir
		// 3. Check PrimaryList, app.config in subdir
		// 4. Check SecondaryList, app.conf in subdir
		public override bool Execute ()
		{
			AppConfigFile = FindAppConfig ();
			if (AppConfigFile != null)
				AppConfigFile.SetMetadata ("TargetPath", TargetPath);

			return true;
		}

		ITaskItem FindAppConfig ()
		{
			foreach (ITaskItem item in PrimaryList)
				if (IsAppConfig (item, false))
					return new TaskItem (item);

			foreach (ITaskItem item in SecondaryList)
				if (IsAppConfig (item, false))
					return new TaskItem (item);

			foreach (ITaskItem item in PrimaryList)
				if (IsAppConfig (item, true))
					return new TaskItem (item);

			foreach (ITaskItem item in SecondaryList)
				if (IsAppConfig (item, true))
					return new TaskItem (item);

			return null;
		}

		bool IsAppConfig (ITaskItem item, bool require_subdir)
		{
			if (String.Compare (Path.GetFileName (item.ItemSpec), "app.config", true) != 0)
				return false;

			bool has_dir = Path.GetDirectoryName (item.ItemSpec).Length > 0;

			return require_subdir == has_dir;
		}

		[Output]
		public ITaskItem AppConfigFile {
			get; set;
		}

		[Required]
		public ITaskItem[] PrimaryList {
			get; set;
		}

		[Required]
		public ITaskItem[] SecondaryList {
			get; set;
		}

		[Required]
		public string TargetPath {
			get; set;
		}
	}
}

#endif
