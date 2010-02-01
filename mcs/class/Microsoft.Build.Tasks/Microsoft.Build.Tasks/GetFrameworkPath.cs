//
// GetFrameworkPath.cs: Task that gets path to framework.
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

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {
	public class GetFrameworkPath : TaskExtension {
		
		string	path;
	
		public GetFrameworkPath ()
		{
		}

		public override bool Execute ()
		{
			path = ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version20);
			return true;
		}

		[Output]
		public string Path {
			get {
				return path;
			}
			set {
				path = value;
			}
		}

		[Output]
		public string FrameworkVersion11Path {
			get {
				return ToolLocationHelper.GetPathToDotNetFramework (
						TargetDotNetFrameworkVersion.Version11);
			}
		}

		[Output]
		public string FrameworkVersion20Path {
			get {
				return ToolLocationHelper.GetPathToDotNetFramework (
						TargetDotNetFrameworkVersion.Version20);
			}
		}

		[Output]
		public string FrameworkVersion30Path {
			get {
				return ToolLocationHelper.GetPathToDotNetFramework (
						TargetDotNetFrameworkVersion.Version30);
			}
		}

		[Output]
		public string FrameworkVersion35Path {
			get {
				return ToolLocationHelper.GetPathToDotNetFramework (
						TargetDotNetFrameworkVersion.Version35);
			}
		}

	}
}

#endif
