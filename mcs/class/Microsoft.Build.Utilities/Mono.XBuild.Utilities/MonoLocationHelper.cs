// 
// MonoLocationHelper.cs: Returns paths like libdir, bindir etc.
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
using System.IO;

namespace Mono.XBuild.Utilities {
	internal class MonoLocationHelper {
	
		static string binDir;
		static string libDir;
		static string assembliesDir;
		//static string xbuildDir;
	
		static MonoLocationHelper ()
		{
			string assemblyLocation;
			DirectoryInfo t1, t2, t3, t4;
			
			assemblyLocation = Path.GetDirectoryName (typeof (object).Assembly.Location);
			assembliesDir = assemblyLocation;
			// /usr/local/lib/mono/1.0
			t1 = new DirectoryInfo (assemblyLocation);
			// /usr/local/lib/mono
			t2 = t1.Parent;
			// /usr/local/lib/mono/xbuild
			//xbuildDir = Path.Combine (t2.FullName, "xbuild");
			// /usr/local/lib
			t3 = t2.Parent;
			// /usr/local
			t4 = t3.Parent;
			binDir = Path.Combine (t4.FullName, "bin");
			libDir = Path.Combine (t4.FullName, "lib");
		}
	
		internal static string GetBinDir ()
		{
			return binDir;
		}
		
		internal static string GetLibDir ()
		{
			return libDir;
		}
		
		internal static string GetAssembliesDir ()
		{
			return assembliesDir;
		}
	}
}

#endif
