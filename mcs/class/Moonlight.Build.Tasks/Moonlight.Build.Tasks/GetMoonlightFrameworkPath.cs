//
// GetMoonlightFrameworkPath.cs
//
// Author:
//	Michael Hutchinson <mhutchinson@novell.com>
//	Ankit Jain <jankit@novell.com>
//
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
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

using SI = System.IO;

using System;
using System.Text;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Moonlight.Build.Tasks {
	public class GetMoonlightFrameworkPath : Task {

		public override bool Execute ()
		{
			return true;
		}

		[Required]
		public string SilverlightVersion {
			get; set;
		}

		[Output]
		public string FrameworkPath {
			get {
				if (string.IsNullOrEmpty (SilverlightVersion))
					return FrameworkVersion30Path;

				return SI.Path.GetFullPath (
						PathCombine (ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version20),
						"..", "..", "moonlight", SilverlightVersion));
			}
		}

		[Output]
		public string FrameworkVersion20Path {
			get {
				return SI.Path.GetFullPath (
						PathCombine (ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version20),
						"..", "..", "moonlight", "2.0"));
			}
		}

		[Output]
		public string FrameworkVersion30Path {
			get {
				return SI.Path.GetFullPath (
						PathCombine (ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version20),
						"..", "..", "moonlight", "3.0"));
			}
		}

		static string PathCombine (string path1, params string[] parts)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (path1);
			foreach (string part in parts)
				sb.AppendFormat ("{0}{1}", SI.Path.DirectorySeparatorChar, part);

			return sb.ToString ();
		}
	}
}
