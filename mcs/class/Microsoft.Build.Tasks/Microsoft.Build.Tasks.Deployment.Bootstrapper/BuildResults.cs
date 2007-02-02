//
// BuildResults.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Tasks.Deployment.Bootstrapper {

	[ClassInterface (ClassInterfaceType.None)]
	[Guid ("FAD7BA7C-CA00-41e0-A5EF-2DA9A74E58E6")]
	[ComVisible (true)]
	public class BuildResults : IBuildResults {
	
		string []	component_files;
		string		key_file;
		BuildMessage []	messages;
		bool		succeeded;

		BuildResults ()
		{
		}
		
		public string[] ComponentFiles {
			get { return component_files; }
		}
		
		public string KeyFile {
			get { return key_file; }
		}
		
		public BuildMessage [] Messages {
			get { return messages; }
		}
		
		public bool Succeeded {
			get { return succeeded; }
		}
		
	}
}

#endif
