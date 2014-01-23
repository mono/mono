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
	#if MICROSOFT_BUILD_DLL
	internal
	#else
	public
	#endif
	static class ToolLocationHelper
	{
		static string lib_mono_dir;
		static string [] mono_dir;
		static bool runningOnDotNet;

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

#if NET_4_0
			var windowsPath = Environment.GetFolderPath (Environment.SpecialFolder.Windows);
			runningOnDotNet = !string.IsNullOrEmpty (windowsPath) && lib_mono_dir.StartsWith (windowsPath);
#endif

			if (Environment.GetEnvironmentVariable ("TESTING_MONO") != null) {
				mono_dir = new string [] {
					Path.Combine (lib_mono_dir, "net_1_0"),
					Path.Combine (lib_mono_dir, "net_2_0"),
					Path.Combine (lib_mono_dir, "net_2_0"),
					Path.Combine (lib_mono_dir, "net_3_5"),
					// mono's 4.0 is not an actual framework directory with all tools etc
					// it's simply reference assemblies. So like .NET we consider 4.5 to
					// be a complete replacement for 4.0.
					Path.Combine (lib_mono_dir, "net_4_5"),
					Path.Combine (lib_mono_dir, "net_4_5"),
					Path.Combine (lib_mono_dir, "net_4_5")
				};	
			} else if (runningOnDotNet) {
				mono_dir = new string [] {
					Path.Combine (lib_mono_dir, "v1.0.3705"),
					Path.Combine (lib_mono_dir, "v2.0.50727"),
					Path.Combine (lib_mono_dir, "v2.0.50727"),
					Path.Combine (lib_mono_dir, "v3.5"),
					Path.Combine (lib_mono_dir, "v4.0.30319"),
					Path.Combine (lib_mono_dir, "v4.0.30319"),
					Path.Combine (lib_mono_dir, "v4.0.30319")
				};
			} else {
				mono_dir = new string [] {
					Path.Combine (lib_mono_dir, "1.0"),
					Path.Combine (lib_mono_dir, "2.0"),
					Path.Combine (lib_mono_dir, "2.0"),
					Path.Combine (lib_mono_dir, "3.5"),
					// see comment above regarding 4.0/4.5
					Path.Combine (lib_mono_dir, "4.5"),
					Path.Combine (lib_mono_dir, "4.5"),
					Path.Combine (lib_mono_dir, "4.5"),
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

#if XBUILD_12
		public static string CurrentToolsVersion {
			get {
				return XBuildConsts.Version;
			}
		}

		public static string GetPathToBuildTools (string toolsVersion)
		{
			if (toolsVersion != "12.0")
				return null;

			if (Environment.GetEnvironmentVariable ("TESTING_MONO") != null)
				return Path.Combine (lib_mono_dir, "xbuild_12");

			if (runningOnDotNet) {
				//see http://msdn.microsoft.com/en-us/library/vstudio/bb397428(v=vs.120).aspx
				var programFiles = Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86);
				return Path.Combine (programFiles, "MSBuild", toolsVersion, "bin");
			}

			return Path.Combine (lib_mono_dir, "xbuild", toolsVersion, "bin");
		}
#endif
	}
}
