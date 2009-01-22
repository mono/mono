//
// System.Web.Compilation.BuildManagerDirectoryBuilder
//
// Authors:
//      Marek Habersack (mhabersack@novell.com)
//
// (C) 2008-2009 Novell, Inc (http://www.novell.com)
//

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

#if NET_2_0
using System;
using System.Collections.Generic;

namespace System.Web.Compilation
{
	class BuildProviderGroup : List <BuildProvider>
	{
		// Prefix for the assembly to be generated
		public string NamePrefix {
			get;
			private set;
		}

		// Can the group accept only one build provider
		public bool Standalone {
			get; set;
		}

		// Is the group for global.asax
		public bool Application 
		{
			get;
			private set;
		}

		// Does the group contain the originally requested virtual path
		public bool Master {
			get; set;
		}

		// Compiler type for this group
		public CompilerType CompilerType {
			get;
			private set;
		}
		
		public BuildProviderGroup ()
		{
		}

		public void AddProvider (BuildProvider bp) 
		{
			if (Count == 0) {
				// We need to set the name prefix
				if (bp is ApplicationFileBuildProvider) {
					NamePrefix = "App_global.asax";
					Application = true;
				} else if (bp is ThemeDirectoryBuildProvider) {
					NamePrefix = "App_Theme";
					Master = true;
				} else
					NamePrefix = "App_Web";

				CompilerType ct = BuildManager.GetDefaultCompilerTypeForLanguage (bp.LanguageName, null);
				if (ct != null)
					CompilerType = ct;
			}

			Add (bp);
		}
	}
}
#endif