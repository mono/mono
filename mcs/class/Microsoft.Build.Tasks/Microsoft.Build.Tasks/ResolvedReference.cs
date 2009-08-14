//
// ResolvedReference.cs
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
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {
	
	class ResolvedReference {
		public ITaskItem TaskItem;
		public AssemblyName AssemblyName;
		public SearchPath FoundInSearchPath;
		public bool CopyLocal;
		public bool IsPrimary; //default: true

		string found_search_path_string;

		public ResolvedReference (ITaskItem item, AssemblyName asm_name, bool copy_local, SearchPath search_path,
				string original_item_spec)
		{
			this.TaskItem = item;
			AssemblyName = asm_name;
			CopyLocal = copy_local;
			IsPrimary = true;
			FoundInSearchPath = search_path;

			TaskItem.SetMetadata ("OriginalItemSpec", original_item_spec);
			TaskItem.SetMetadata ("ResolvedFrom", FoundInSearchPathToString ());
		}

		public string FoundInSearchPathAsString {
			get {
				if (found_search_path_string == null)
					return FoundInSearchPathToString ();
				else
					return found_search_path_string;
			}
			set { found_search_path_string = value; }
		}

		string FoundInSearchPathToString ()
		{
			switch (FoundInSearchPath) {
			case SearchPath.Gac:
				return "{GAC}";
			case SearchPath.TargetFrameworkDirectory:
				return "{TargetFrameworkDirectory}";
			case SearchPath.CandidateAssemblies:
				return "{CandidateAssemblies}";
			case SearchPath.HintPath:
				return "{HintPathFromItem}";
			case SearchPath.RawFileName:
				return "{RawFileName}";
			case SearchPath.Directory:
				return TaskItem.ItemSpec;
			case SearchPath.PkgConfig:
				return "{PkgConfig}";
			default:
				throw new NotImplementedException (String.Format (
						"Implement me for SearchPath: {0}", FoundInSearchPath));
			}
		}
	}


}

#endif
