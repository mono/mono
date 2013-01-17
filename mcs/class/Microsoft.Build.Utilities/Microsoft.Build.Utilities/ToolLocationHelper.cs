// 
// ToolLocationHelper.cs:
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

using System;
using System.IO;

namespace Microsoft.Build.Utilities
{
	public static class ToolLocationHelper
	{
		static string lib_mono_dir;
		static string [] mono_dir;

		static ToolLocationHelper ()
		{
			string assemblyLocation;
			DirectoryInfo t1, t2;

			// /usr/local/lib/mono/1.0
			assemblyLocation = Path.GetDirectoryName (typeof (object).Assembly.Location);
			t1 = new DirectoryInfo (assemblyLocation);

			// usr/local/lib/mono
			t2 = t1.Parent;

			lib_mono_dir = t2.FullName;
			if (Environment.GetEnvironmentVariable ("TESTING_MONO") != null) {
				mono_dir = new string [] {
					Path.Combine (lib_mono_dir, "net_1_0"),
					Path.Combine (lib_mono_dir, "net_2_0"),
					Path.Combine (lib_mono_dir, "net_2_0"),
					Path.Combine (lib_mono_dir, "net_3_5"),
					Path.Combine (lib_mono_dir, "net_4_0"),
					Path.Combine (lib_mono_dir, "net_4_5")
				};	
			} else {
				mono_dir = new string [] {
					Path.Combine (lib_mono_dir, "1.0"),
					Path.Combine (lib_mono_dir, "2.0"),
					Path.Combine (lib_mono_dir, "2.0"),
					Path.Combine (lib_mono_dir, "3.5"),
					Path.Combine (lib_mono_dir, "4.0"),
					Path.Combine (lib_mono_dir, "4.5")
				};
			}

		}

		[MonoTODO]
		public static string GetDotNetFrameworkRootRegistryKey (TargetDotNetFrameworkVersion version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string GetDotNetFrameworkSdkInstallKeyValue (TargetDotNetFrameworkVersion version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string GetDotNetFrameworkVersionFolderPrefix (TargetDotNetFrameworkVersion version)
		{
			throw new NotImplementedException ();
		}

		public static string GetPathToDotNetFramework (TargetDotNetFrameworkVersion version)
		{
			return mono_dir [(int)version];
		}

		[MonoTODO]
		public static string GetPathToDotNetFrameworkFile (string fileName,
								  TargetDotNetFrameworkVersion version)
		{
			throw new NotImplementedException ();
		}

		public static string GetPathToDotNetFrameworkSdk (TargetDotNetFrameworkVersion version)
		{
			return GetPathToDotNetFramework (version);
		}

		[MonoTODO]
		public static string GetPathToDotNetFrameworkSdkFile (string fileName,
								      TargetDotNetFrameworkVersion version)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string GetPathToSystemFile (string fileName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string PathToSystem {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}
