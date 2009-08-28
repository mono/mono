//
// CombinePath.cs:
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
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {
	[MonoTODO]
	public class CombinePath : TaskExtension {
	
		string		base_path;
		ITaskItem []	combined_paths;
		ITaskItem []	paths;
		
		public CombinePath ()
		{
		}
		
		public override bool Execute ()
		{
			if (paths.Length == 0)
				return true;

			List <ITaskItem> combined = new List <ITaskItem> ();

			foreach (ITaskItem path in paths)
				if (String.IsNullOrEmpty (base_path))
					combined.Add (path);
				else
					combined.Add (new TaskItem (Path.Combine (base_path, path.ItemSpec)));

			combined_paths = combined.ToArray ();
			
			return true;
		}
		
		public string BasePath {
			get { return base_path; }
			set { base_path = value; }
		}
		
		[Output]
		public ITaskItem [] CombinedPaths {
			get { return combined_paths; }
			set { combined_paths = value; }
		}
		
		[Required]
		public ITaskItem [] Paths {
			get { return paths; }
			set { paths = value; }
		}
	}
}

#endif
