//
// Consts.cs
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

using System;
using System.IO;
using Microsoft.Build.Utilities;

public static class Consts {

	public static bool RunningOnMono ()
	{
		return Type.GetType ("Mono.Runtime") != null;
	}
	
	public static string BinPath {
		get {
			if (RunningOnMono ()) {
#if XBUILD_14
				string profile = "xbuild_14";
#elif XBUILD_12
				string profile = "xbuild_12";
#elif NET_4_5
				string profile = "net_4_x";
#else
				#error "Unknown profile"
#endif
				var corlib = typeof (object).Assembly.Location;
				var lib = Path.GetDirectoryName (Path.GetDirectoryName (corlib));
				return Path.Combine (lib, profile);
			} else {
#if XBUILD_14
				return ToolLocationHelper.GetPathToBuildTools ("14.0");
#elif XBUILD_12
				return ToolLocationHelper.GetPathToBuildTools ("12.0");
#elif NET_4_5
				return ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version45);
#elif NET_4_0
				return ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version40);
#else
				return ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version20);
#endif
			}
		}
	}

	public static string ToolsVersionString {
		get {
#if XBUILD_14
			return " ToolsVersion='14.0'";
#elif XBUILD_12
			return " ToolsVersion='12.0'";
#elif NET_4_0
			return " ToolsVersion='4.0'";
#else
			return String.Empty;
#endif
		}
	}

	public static string GetTasksAsmPath ()
	{
#if XBUILD_14
		return Path.Combine (BinPath, "Microsoft.Build.Tasks.Core.dll");
#elif XBUILD_12
		return Path.Combine (BinPath, "Microsoft.Build.Tasks.v12.0.dll");
#elif NET_4_0
		return Path.Combine (BinPath, "Microsoft.Build.Tasks.v4.0.dll");
#else
		return Path.Combine (BinPath, "Microsoft.Build.Tasks.dll");
#endif
	}
}
