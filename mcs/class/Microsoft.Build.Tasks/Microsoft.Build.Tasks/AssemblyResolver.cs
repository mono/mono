//
// AssemblyResolver.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Ankit Jain (jankit@novell.com)
// 
// (C) 2006 Marek Sieradzki
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.PkgConfig;

namespace Microsoft.Build.Tasks {
	internal class AssemblyResolver {

		// name -> (version -> assemblypath)
		Dictionary<string, TargetFrameworkAssemblies> target_framework_cache;
		Dictionary<string, Dictionary<Version, string>> gac;
		TaskLoggingHelper log;
		List<string> search_log;

		static LibraryPcFileCache cache;

		public AssemblyResolver ()
		{
			gac = new Dictionary<string, Dictionary<Version, string>> ();
			target_framework_cache = new Dictionary <string, TargetFrameworkAssemblies> ();

			GatherGacAssemblies ();
		}

		public void ResetSearchLogger ()
		{
			if (search_log == null)
				search_log = new List<string> ();
			else
				search_log.Clear ();
		}

		string GetGacPath ()
		{
			// NOTE: code from mcs/tools/gacutil/driver.cs
			PropertyInfo gac = typeof (System.Environment).GetProperty ("GacPath", BindingFlags.Static | BindingFlags.NonPublic);

			if (gac == null)
				return null;

			MethodInfo get_gac = gac.GetGetMethod (true);
			return (string) get_gac.Invoke (null, null);
		}

		void GatherGacAssemblies ()
		{
			string gac_path = GetGacPath ();
			if (gac_path == null)
				throw new InvalidOperationException ("XBuild must be run on Mono runtime");
			if (!Directory.Exists (gac_path))
				return; // in case mono isn't "installed".

			Version version;
			DirectoryInfo version_info, assembly_info;

			foreach (string assembly_name in Directory.GetDirectories (gac_path)) {
				assembly_info = new DirectoryInfo (assembly_name);
				foreach (string version_token in Directory.GetDirectories (assembly_name)) {
					foreach (string file in Directory.GetFiles (version_token, "*.dll")) {
						version_info = new DirectoryInfo (version_token);
						version = new Version (version_info.Name.Split (
							new char [] {'_'}, StringSplitOptions.RemoveEmptyEntries) [0]);

						Dictionary<Version, string> assembliesByVersion = new Dictionary <Version, string> ();
						if (!gac.TryGetValue (assembly_info.Name, out assembliesByVersion)) {
							assembliesByVersion = new Dictionary <Version, string> ();
							gac.Add (assembly_info.Name, assembliesByVersion);
						}

						string found_file;
						if (assembliesByVersion.TryGetValue (version, out found_file) &&
							File.GetLastWriteTime (file) <= File.GetLastWriteTime (found_file))
								// Duplicate found, take the newer file
								continue;

						assembliesByVersion [version] = file;
					}
				}
			}
		}

		public ResolvedReference FindInTargetFramework (ITaskItem reference, string framework_dir, bool specific_version)
		{
			if (!Directory.Exists (framework_dir))
				return null;
			
			AssemblyName key_aname;
			if (!TryGetAssemblyNameFromFullName (reference.ItemSpec, out key_aname))
				return null;

			TargetFrameworkAssemblies gac_asm;
			if (!target_framework_cache.TryGetValue (framework_dir, out gac_asm)) {
				// fill gac_asm
				gac_asm = target_framework_cache [framework_dir] = PopulateTargetFrameworkAssemblies (framework_dir);
			}

			KeyValuePair<AssemblyName, string> pair;
			if (gac_asm.NameToAssemblyNameCache.TryGetValue (key_aname.Name, out pair)) {
				if (AssemblyNamesCompatible (key_aname, pair.Key, specific_version)) {
					// gac and tgt frmwk refs are not copied private
					return GetResolvedReference (reference, pair.Value, pair.Key, false,
							SearchPath.TargetFrameworkDirectory);
				}

				LogSearchMessage ("Considered target framework dir {0}, assembly name '{1}' did not " +
						"match the expected '{2}' (SpecificVersion={3})",
						framework_dir, pair.Key, key_aname, specific_version);
			} else {
				LogSearchMessage ("Considered target framework dir {0}, assembly named '{1}' not found.",
						framework_dir, key_aname.Name);
			}
			return null;
		}

		// Look for %(Identity).{dll|exe|..}
		// if specific_version==true
		// 	resolve if assembly names match
		// else
		// 	resolve the valid assembly
		public ResolvedReference FindInDirectory (ITaskItem reference, string directory, string [] file_extensions, bool specific_version)
		{
			string filename = reference.ItemSpec;
			int comma_pos = filename.IndexOf (',');
			if (comma_pos >= 0)
				filename = filename.Substring (0, comma_pos);

			// Try as a filename
			string path = Path.GetFullPath (Path.Combine (directory, filename));
			AssemblyName aname = null;
			if (specific_version && !TryGetAssemblyNameFromFullName (reference.ItemSpec, out aname))
				return null;

			ResolvedReference resolved_ref = ResolveReferenceForPath (path, reference, aname, null, SearchPath.Directory, specific_version);
			if (resolved_ref != null)
				return resolved_ref;

			// try path + Include + {.dll|.exe|..}
			foreach (string extn in file_extensions) {
				resolved_ref = ResolveReferenceForPath (path + extn, reference, aname, null, SearchPath.Directory, specific_version);
				if (resolved_ref != null)
					return resolved_ref;
			}

			return null;
		}

		// tries to resolve reference from the given file path, and compares assembly names
		// if @specific_version == true, and logs accordingly
		ResolvedReference ResolveReferenceForPath (string filename, ITaskItem reference, AssemblyName aname,
					string error_message, SearchPath spath, bool specific_version)
		{
			AssemblyName found_aname;
			if (!TryGetAssemblyNameFromFile (filename, out found_aname)) {
				if (error_message != null)
					log.LogMessage (MessageImportance.Low, error_message);
				return null;
			}

			if (!specific_version || AssemblyNamesCompatible (aname, found_aname, specific_version)) {
				// Check compatibility only if specific_version == true
				return GetResolvedReference (reference, filename, found_aname, true, spath);
			} else {
				LogSearchMessage ("Considered '{0}', but assembly name '{1}' did not match the " +
						"expected '{2}' (SpecificVersion={3})", filename, found_aname, aname, specific_version);
				log.LogMessage (MessageImportance.Low, "Assembly names are not compatible.");
			}

			return null;
		}

		TargetFrameworkAssemblies PopulateTargetFrameworkAssemblies (string directory)
		{
			TargetFrameworkAssemblies gac_asm = new TargetFrameworkAssemblies (directory);
			foreach (string file in Directory.GetFiles (directory, "*.dll")) {
				AssemblyName aname;
				if (TryGetAssemblyNameFromFile (file, out aname))
					gac_asm.NameToAssemblyNameCache [aname.Name] =
						new KeyValuePair<AssemblyName, string> (aname, file);
			}

			return gac_asm;
		}

		public ResolvedReference ResolveGacReference (ITaskItem reference, bool specific_version)
		{
			AssemblyName name;
			if (!TryGetAssemblyNameFromFullName (reference.ItemSpec, out name))
				return null;

			if (!gac.ContainsKey (name.Name)) {
				LogSearchMessage ("Considered {0}, but could not find in the GAC.",
						reference.ItemSpec);
				return null;
			}

			if (name.Version != null) {
				string ret;
				if (gac [name.Name].TryGetValue (name.Version, out ret))
					return GetResolvedReference (reference, ret, name, false, SearchPath.Gac);

				// not found
				if (specific_version) {
					LogSearchMessage ("Considered '{0}', but an assembly with the specific version not found.",
							reference.ItemSpec);
					return null;
				}
			}

			Version [] versions = new Version [gac [name.Name].Keys.Count];
			gac [name.Name].Keys.CopyTo (versions, 0);
			Array.Sort (versions, (IComparer <Version>) null);
			Version highest = versions [versions.Length - 1];
			//FIXME: the aname being used here isn't correct, its version should
			//	 actually match "highest"
			return GetResolvedReference (reference, gac [name.Name] [highest], name, false, SearchPath.Gac);
		}

		public ResolvedReference ResolvePkgConfigReference (ITaskItem reference, bool specific_version)
		{
			PackageAssemblyInfo pkg = null;

			if (specific_version) {
				pkg = PcCache.GetAssemblyLocation (reference.ItemSpec);
			} else {
				// if not specific version, then just match simple name
				string name = reference.ItemSpec;
				if (name.IndexOf (',') > 0)
					name = name.Substring (0, name.IndexOf (','));
				pkg = PcCache.ResolveAssemblyName (name).FirstOrDefault ();
			}

			if (pkg == null) {
				LogSearchMessage ("Considered {0}, but could not find in any pkg-config files.",
						reference.ItemSpec);
				return null;
			}

			AssemblyName aname;
			if (!TryGetAssemblyNameFromFullName (pkg.FullName, out aname))
				return null;

			ResolvedReference rr = GetResolvedReference (reference, pkg.File, aname,
						false, SearchPath.PkgConfig);
			rr.FoundInSearchPathAsString = String.Format ("{{PkgConfig}} provided by package named {0}",
							pkg.ParentPackage.Name);

			return rr;
		}

		// HintPath has a valid assembly
		// if specific_version==true
		// 	resolve if assembly names match
		// else
		// 	resolve the valid assembly
		public ResolvedReference ResolveHintPathReference (ITaskItem reference, bool specific_version)
		{
			string hintpath = reference.GetMetadata ("HintPath");
			if (String.IsNullOrEmpty (hintpath)) {
				LogSearchMessage ("HintPath attribute not found");
				return null;
			}

			if (!File.Exists (hintpath)) {
				log.LogMessage (MessageImportance.Low, "HintPath {0} does not exist.", hintpath);
				LogSearchMessage ("Considered {0}, but it does not exist.", hintpath);
				return null;
			}

			AssemblyName aname;
			if (!TryGetAssemblyNameFromFullName (reference.ItemSpec, out aname))
				return null;

			return ResolveReferenceForPath (hintpath, reference, aname,
						String.Format ("File at HintPath {0}, is either an invalid assembly or the file does not exist.", hintpath),
						SearchPath.HintPath, specific_version);
		}

		public bool TryGetAssemblyNameFromFile (string filename, out AssemblyName aname)
		{
			aname = null;
			filename = Path.GetFullPath (filename);
			try {
				aname = AssemblyName.GetAssemblyName (filename);
			} catch (FileNotFoundException) {
				LogSearchMessage ("Considered '{0}' as a file, but the file does not exist",
						filename);
			} catch (BadImageFormatException) {
				LogSearchMessage ("Considered '{0}' as a file, but it is an invalid assembly",
						filename);
			}

			return aname != null;
		}

		bool TryGetAssemblyNameFromFullName (string full_name, out AssemblyName aname)
		{
			aname = null;
			try {
				aname = new AssemblyName (full_name);
			} catch (FileLoadException) {
				LogSearchMessage ("Considered '{0}' as an assembly name, but it is invalid.", full_name);
			}

			return aname != null;
		}

		internal static bool AssemblyNamesCompatible (AssemblyName a, AssemblyName b, bool specificVersion)
		{
			return AssemblyNamesCompatible (a, b, specificVersion, true);
		}

		// if @specificVersion is true then match full name, else just the simple name
		internal static bool AssemblyNamesCompatible (AssemblyName a, AssemblyName b, bool specificVersion,
				bool ignoreCase)
		{
			if (String.Compare (a.Name, b.Name, ignoreCase) != 0)
				return false;

			if (!specificVersion)
				// ..and simple names match
				return true;

			if (a.CultureInfo != null && !a.CultureInfo.Equals (b.CultureInfo))
				return false;

			if (a.Version != null && a.Version != b.Version)
				return false;

			byte [] a_bytes = a.GetPublicKeyToken ();
			byte [] b_bytes = b.GetPublicKeyToken ();

			bool a_is_empty = (a_bytes == null || a_bytes.Length == 0);
			bool b_is_empty = (b_bytes == null || b_bytes.Length == 0);

			if (a_is_empty || b_is_empty)
				return true;

			for (int i = 0; i < a_bytes.Length; i++)
				if (a_bytes [i] != b_bytes [i])
					return false;

			return true;
		}

		public bool IsStrongNamed (AssemblyName name)
		{
			return (name.Version != null &&
					name.GetPublicKeyToken () != null &&
					name.GetPublicKeyToken ().Length != 0);
		}

		// FIXME: to get default values of CopyLocal, compare with TargetFrameworkDirectories

		// If metadata 'Private' is present then use that or use @default_copy_local_value
		// as the value for CopyLocal
		internal ResolvedReference GetResolvedReference (ITaskItem reference, string filename,
				AssemblyName aname, bool default_copy_local_value, SearchPath search_path)
		{
			string pvt = reference.GetMetadata ("Private");

			bool copy_local = default_copy_local_value;
			if (!String.IsNullOrEmpty (pvt))
				//FIXME: log a warning for invalid value
				Boolean.TryParse (pvt, out copy_local);

			ITaskItem new_item = new TaskItem (reference);
			new_item.ItemSpec = filename;
			return new ResolvedReference (new_item, aname, copy_local, search_path, reference.ItemSpec);
		}

		public void LogSearchMessage (string msg, params object [] args)
		{
			search_log.Add (String.Format (msg, args));
		}

		public void LogSearchLoggerMessages (MessageImportance importance)
		{
			foreach (string msg in search_log)
				log.LogMessage (importance, msg);
		}

		public TaskLoggingHelper Log {
			set {
				log = value;
				PcFileCacheContext.Log = value;
			}
		}

		static LibraryPcFileCache PcCache  {
			get {
				if (cache == null) {
					var context = new PcFileCacheContext ();
					cache = new LibraryPcFileCache (context);
					cache.Update ();
				}

				return cache;
			}
		}
	}

	class TargetFrameworkAssemblies {
		public string Path;

		// assembly (simple) name -> (AssemblyName, file path)
		public Dictionary <string, KeyValuePair<AssemblyName, string>> NameToAssemblyNameCache;

		public TargetFrameworkAssemblies (string path)
		{
			this.Path = path;
			NameToAssemblyNameCache = new Dictionary<string, KeyValuePair<AssemblyName, string>> (
					StringComparer.InvariantCultureIgnoreCase);
		}
	}

	class PcFileCacheContext : IPcFileCacheContext<LibraryPackageInfo>
	{
		public static TaskLoggingHelper Log;

		// In the implementation of this method, the host application can extract
		// information from the pc file and store it in the PackageInfo object
		public void StoreCustomData (PcFile pcfile, LibraryPackageInfo pkg)
		{
		}

		// Should return false if the provided package does not have required
		// custom data
		public bool IsCustomDataComplete (string pcfile, LibraryPackageInfo pkg)
		{
			return true;
		}

		// Called to report errors
		public void ReportError (string message, Exception ex)
		{
			Log.LogMessage (MessageImportance.Low, "Error loading pkg-config files: {0} : {1}",
					message, ex.ToString ());
		}
	}

	enum SearchPath
	{
		Gac,
		TargetFrameworkDirectory,
		CandidateAssemblies,
		HintPath,
		Directory,
		RawFileName,
		PkgConfig
	}
}



#endif
