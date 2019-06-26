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
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Versioning;

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

			var windowsPath = Environment.GetFolderPath (Environment.SpecialFolder.Windows);
			runningOnDotNet = !string.IsNullOrEmpty (windowsPath) && lib_mono_dir.StartsWith (windowsPath);

			if (Environment.GetEnvironmentVariable ("TESTING_MONO") != null) {
				mono_dir = new string [] {                   // TargetDotNetFrameworkVersion:
					Path.Combine (lib_mono_dir, "net_1_0"),  // Version11
					Path.Combine (lib_mono_dir, "net_2_0"),  // Version20
					Path.Combine (lib_mono_dir, "net_2_0"),  // Version30
					Path.Combine (lib_mono_dir, "net_3_5"),  // Version35
					// mono's 4.0 is not an actual framework directory with all tools etc
					// it's simply reference assemblies. So like .NET we consider 4.5 to
					// be a complete replacement for 4.0.
					Path.Combine (lib_mono_dir, "net_4_x"),  // Version40
					Path.Combine (lib_mono_dir, "net_4_x"),  // Version45
					Path.Combine (lib_mono_dir, "net_4_x"),  // Version451
					Path.Combine (lib_mono_dir, "net_4_x"),  // Version46
					Path.Combine (lib_mono_dir, "net_4_x"),  // Version461
					Path.Combine (lib_mono_dir, "net_4_x"),  // Version462
				};	
			} else if (runningOnDotNet) {
				mono_dir = new string [] {
					Path.Combine (lib_mono_dir, "v1.0.3705"),   // Version11
					Path.Combine (lib_mono_dir, "v2.0.50727"),  // Version20
					Path.Combine (lib_mono_dir, "v2.0.50727"),  // Version30
					Path.Combine (lib_mono_dir, "v3.5"),        // Version35
					Path.Combine (lib_mono_dir, "v4.0.30319"),  // Version40
					Path.Combine (lib_mono_dir, "v4.0.30319"),  // Version45
					Path.Combine (lib_mono_dir, "v4.0.30319"),  // Version451
					Path.Combine (lib_mono_dir, "v4.0.30319"),  // Version46
					Path.Combine (lib_mono_dir, "v4.0.30319"),  // Version461
					Path.Combine (lib_mono_dir, "v4.0.30319"),  // Version462
				};
			} else {
				mono_dir = new string [] {
					Path.Combine (lib_mono_dir, "1.0"),  // Version11
					Path.Combine (lib_mono_dir, "2.0"),  // Version20
					Path.Combine (lib_mono_dir, "2.0"),  // Version30
					Path.Combine (lib_mono_dir, "3.5"),  // Version35
					// see comment above regarding 4.0/4.5
					Path.Combine (lib_mono_dir, "4.5"),  // Version40
					Path.Combine (lib_mono_dir, "4.5"),  // Version45
					Path.Combine (lib_mono_dir, "4.5"),  // Version451
					Path.Combine (lib_mono_dir, "4.5"),  // Version46
					Path.Combine (lib_mono_dir, "4.5"),  // Version461
					Path.Combine (lib_mono_dir, "4.5"),  // Version462
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

		public static string GetPathToDotNetFrameworkFile (string fileName,
								  TargetDotNetFrameworkVersion version)
		{
			string dir = GetPathToDotNetFramework (version);
			string file = Path.Combine (dir, fileName);
			if (File.Exists (file))
				return file;

			//Mono doesn't ship multiple versions of tools that are backwards/forwards compatible
			if (!runningOnDotNet) {
				//most of the 3.5 tools are in the 2.0 directory
				if (version == TargetDotNetFrameworkVersion.Version35)
					return GetPathToDotNetFrameworkFile (fileName, TargetDotNetFrameworkVersion.Version20);
				//unversioned tools are in the 4.5 directory
				if (version == TargetDotNetFrameworkVersion.Version20)
					return GetPathToDotNetFrameworkFile (fileName, (TargetDotNetFrameworkVersion)5);
			}

			return null;
		}

		public static string GetPathToDotNetFrameworkBinFile (string fileName)
		{
			string dir = Path.Combine(Directory.GetParent(Directory.GetParent(lib_mono_dir).FullName).FullName, "bin");
			string file = Path.Combine (dir, fileName);
			if (File.Exists (file))
				return file;

			return null;
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

		public static string GetPathToStandardLibraries (string targetFrameworkIdentifier,
								 string targetFrameworkVersion,
								 string targetFrameworkProfile)
		{
			return GetPathToStandardLibraries (targetFrameworkIdentifier, targetFrameworkVersion, targetFrameworkProfile, null);
		}

		[MonoTODO]
		#if XBUILD_12
		public
		#endif
		static string GetPathToStandardLibraries (string targetFrameworkIdentifier,
		                                          string targetFrameworkVersion,
		                                          string targetFrameworkProfile,
		                                          string platformTarget)
		{
			// FIXME: support platformTarget
			if (platformTarget != null)
				throw new NotImplementedException ("platformTarget support is not implemented");
			
			var ext = Environment.GetEnvironmentVariable ("XBUILD_FRAMEWORK_FOLDERS_PATH");
			var ret = ext != null ? GetPathToStandardLibrariesWith (ext, targetFrameworkIdentifier, targetFrameworkVersion, targetFrameworkProfile) : null;
			return ret ?? GetPathToStandardLibrariesWith (Path.GetFullPath (Path.Combine (lib_mono_dir, "xbuild-frameworks")), targetFrameworkIdentifier, targetFrameworkVersion, targetFrameworkProfile);
		}
			
		static string GetPathToStandardLibrariesWith (string xbuildFxDir,
							      string targetFrameworkIdentifier,
		                                              string targetFrameworkVersion,
		                                              string targetFrameworkProfile)
		{
			var path = Path.Combine (xbuildFxDir, targetFrameworkIdentifier);
			if (!string.IsNullOrEmpty (targetFrameworkVersion)) {
				path = Path.Combine (path, targetFrameworkVersion);
				if (!string.IsNullOrEmpty (targetFrameworkProfile))
					path = Path.Combine (path, "Profile", targetFrameworkProfile);
			}
			if (!Directory.Exists (path))
				return null;
			var flist = Path.Combine (path, "RedistList", "FrameworkList.xml");
			if (!File.Exists (flist))
				return null;
			var xml = XmlReader.Create (flist);
			xml.MoveToContent ();
			var targetFxDir = xml.GetAttribute ("TargetFrameworkDirectory");
			targetFxDir = targetFxDir != null ? Path.GetFullPath (Path.Combine (path, "dummy", targetFxDir.Replace ('\\', Path.DirectorySeparatorChar))) : null;
			if (Directory.Exists (targetFxDir))
				return targetFxDir;
			// I'm not sure if this is completely valid assumption...
			return path;
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
			string path;
			switch (toolsVersion) {
			case "12.0":
				path = "xbuild_12";
				break;
			case "14.0":
				path = "xbuild_14";
				break;
			default:
				return null;
			}

			if (Environment.GetEnvironmentVariable ("TESTING_MONO") != null)
				return Path.Combine (lib_mono_dir, path);

			if (runningOnDotNet) {
				//see http://msdn.microsoft.com/en-us/library/vstudio/bb397428(v=vs.120).aspx
				var programFiles = Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86);
				return Path.Combine (programFiles, "MSBuild", toolsVersion, "bin");
			}

			return Path.Combine (lib_mono_dir, "xbuild", toolsVersion, "bin");
		}
#endif

		// These have no meaning in mono except to exist, so return empty lists for each.
		public static IList<string> GetPathToReferenceAssemblies(string targetFrameworkIdentifier, string targetFrameworkVersion, string targetFrameworkProfile)
		{
			return new List<string>();
		}

		public static IList<string> GetPathToReferenceAssemblies(FrameworkName frameworkName)
		{
			return new List<string>();
		}

		public static IList<string> GetPathToReferenceAssemblies(string targetFrameworkRootPath, FrameworkName frameworkName)
		{
			return new List<string>();
		}

		public static IList<string> GetSupportedTargetFrameworks()
		{
			throw new NotImplementedException();
		}
	}
}
