//
// ResolveAssemblyReference.cs: Searches for assembly files.
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {
	public class ResolveAssemblyReference : TaskExtension {
	
		bool		autoUnify;
		ITaskItem[]	assemblyFiles;
		ITaskItem[]	assemblies;
		string		appConfigFile;
		string[]	allowedAssemblyExtensions;
		string[]	allowedRelatedFileExtensions;
		string[]	candidateAssemblyFiles;
		ITaskItem[]	copyLocalFiles;
		ITaskItem[]	filesWritten;
		bool		findDependencies;
		bool		findRelatedFiles;
		bool		findSatellites;
		bool		findSerializationAssemblies;
		string[]	installedAssemblyTables;
		ITaskItem[]	relatedFiles;
		ITaskItem[]	resolvedDependencyFiles;
		ITaskItem[]	resolvedFiles;
		ITaskItem[]	satelliteFiles;
		ITaskItem[]	scatterFiles;
		string[]	searchPaths;
		ITaskItem[]	serializationAssemblyFiles;
		bool 		silent;
		string		stateFile;
		ITaskItem[]	suggestedRedirects;
		string[]	targetFrameworkDirectories;
		string		targetProcessorArchitecture;
		static string []	default_assembly_extensions;

		AssemblyResolver	assembly_resolver;
		List<string> dependency_search_paths;
		Dictionary<string, ResolvedReference> assemblyNameToResolvedRef;
		Dictionary<string, ITaskItem>	tempSatelliteFiles, tempRelatedFiles,
			tempResolvedDepFiles, tempCopyLocalFiles;
		List<ITaskItem> tempResolvedFiles;
		List<PrimaryReference> primaryReferences;
		Dictionary<string, string> alreadyScannedAssemblyNames;
		Dictionary<string, string> conflictWarningsCache;

		//FIXME: construct and use a graph of the dependencies, useful across projects

		static ResolveAssemblyReference ()
		{
			default_assembly_extensions = new string [] { ".dll", ".exe" };
		}

		public ResolveAssemblyReference ()
		{
			assembly_resolver = new AssemblyResolver ();
		}

		//FIXME: make this reusable
		public override bool Execute ()
		{
			if (assemblies == null && assemblyFiles == null)
				// nothing to resolve
				return true;

			assembly_resolver.Log = Log;
			tempResolvedFiles = new List<ITaskItem> ();
			tempCopyLocalFiles = new Dictionary<string, ITaskItem> ();
			tempSatelliteFiles = new Dictionary<string, ITaskItem> ();
			tempRelatedFiles = new Dictionary<string, ITaskItem> ();
			tempResolvedDepFiles = new Dictionary<string, ITaskItem> ();

			primaryReferences = new List<PrimaryReference> ();
			assemblyNameToResolvedRef = new Dictionary<string, ResolvedReference> ();
			conflictWarningsCache = new Dictionary<string, string> ();

			ResolveAssemblies ();
			ResolveAssemblyFiles ();

			alreadyScannedAssemblyNames = new Dictionary<string, string> ();

			// the first element is place holder for parent assembly's dir
			dependency_search_paths = new List<string> () { String.Empty };
			dependency_search_paths.AddRange (searchPaths);

			// resolve dependencies
			foreach (PrimaryReference pref in primaryReferences)
				ResolveAssemblyFileDependencies (pref.TaskItem, pref.ParentCopyLocal);

			resolvedFiles = tempResolvedFiles.ToArray ();
			copyLocalFiles = tempCopyLocalFiles.Values.ToArray ();
			satelliteFiles = tempSatelliteFiles.Values.ToArray ();
			relatedFiles = tempRelatedFiles.Values.ToArray ();
			resolvedDependencyFiles = tempResolvedDepFiles.Values.ToArray ();

			tempResolvedFiles.Clear ();
			tempCopyLocalFiles.Clear ();
			tempSatelliteFiles.Clear ();
			tempRelatedFiles.Clear ();
			tempResolvedDepFiles.Clear ();
			alreadyScannedAssemblyNames.Clear ();
			primaryReferences.Clear ();
			assemblyNameToResolvedRef.Clear ();
			conflictWarningsCache.Clear ();
			dependency_search_paths = null;

			return true;
		}

		void ResolveAssemblies ()
		{
			if (assemblies == null || assemblies.Length == 0)
				return;

			foreach (ITaskItem item in assemblies) {
				if (!String.IsNullOrEmpty (item.GetMetadata ("SubType"))) {
					Log.LogWarning ("Reference '{0}' has non-empty SubType. Ignoring.", item.ItemSpec);
					continue;
				}

				LogWithPrecedingNewLine (MessageImportance.Low, "Primary Reference {0}", item.ItemSpec);
				ResolvedReference resolved_ref = ResolveReference (item, searchPaths, true);
				if (resolved_ref == null) {
					Log.LogWarning ("Reference '{0}' not resolved", item.ItemSpec);
					assembly_resolver.LogSearchLoggerMessages (MessageImportance.Normal);
				} else {
					if (Environment.GetEnvironmentVariable ("XBUILD_LOG_REFERENCE_RESOLVER") != null)
						assembly_resolver.LogSearchLoggerMessages (MessageImportance.Low);

					Log.LogMessage (MessageImportance.Low,
							"\tReference {0} resolved to {1}. CopyLocal = {2}",
							item.ItemSpec, resolved_ref.TaskItem,
							resolved_ref.TaskItem.GetMetadata ("CopyLocal"));

					Log.LogMessage (MessageImportance.Low,
							"\tReference found at search path {0}",
							resolved_ref.FoundInSearchPathAsString);

					if (TryAddNewReference (tempResolvedFiles, resolved_ref) &&
						!IsFromGacOrTargetFramework (resolved_ref) &&
						resolved_ref.FoundInSearchPath != SearchPath.PkgConfig) {
						primaryReferences.Add (new PrimaryReference (
								resolved_ref.TaskItem,
								resolved_ref.TaskItem.GetMetadata ("CopyLocal")));
					}
				}
			}
		}

		// Use @search_paths to resolve the reference
		ResolvedReference ResolveReference (ITaskItem item, IEnumerable<string> search_paths, bool set_copy_local)
		{
			ResolvedReference resolved = null;
			bool specific_version;

			assembly_resolver.ResetSearchLogger ();

			if (!TryGetSpecificVersionValue (item, out specific_version))
				return null;

			foreach (string spath in search_paths) {
				assembly_resolver.LogSearchMessage ("For searchpath {0}", spath);

				if (String.Compare (spath, "{HintPathFromItem}") == 0) {
					resolved = assembly_resolver.ResolveHintPathReference (item, specific_version);
				} else if (String.Compare (spath, "{TargetFrameworkDirectory}") == 0) {
					if (targetFrameworkDirectories == null)
						continue;
					foreach (string fpath in targetFrameworkDirectories) {
						resolved = assembly_resolver.FindInTargetFramework (item,
								fpath, specific_version);
						if (resolved != null)
							break;
					}
				} else if (String.Compare (spath, "{GAC}") == 0) {
					resolved = assembly_resolver.ResolveGacReference (item, specific_version);
				} else if (String.Compare (spath, "{RawFileName}") == 0) {
					//FIXME: identify assembly names, as extract the name, and try with that?
					AssemblyName aname;
					if (assembly_resolver.TryGetAssemblyNameFromFile (item.ItemSpec, out aname))
						resolved = assembly_resolver.GetResolvedReference (item, item.ItemSpec, aname, true,
								SearchPath.RawFileName);
				} else if (String.Compare (spath, "{CandidateAssemblyFiles}") == 0) {
					assembly_resolver.LogSearchMessage (
							"Warning: {{CandidateAssemblyFiles}} not supported currently");
				} else if (String.Compare (spath, "{PkgConfig}") == 0) {
					resolved = assembly_resolver.ResolvePkgConfigReference (item, specific_version);
				} else {
					resolved = assembly_resolver.FindInDirectory (
							item, spath,
							allowedAssemblyExtensions ?? default_assembly_extensions,
							specific_version);
				}

				if (resolved != null)
					break;
			}

			if (resolved != null && set_copy_local)
				SetCopyLocal (resolved.TaskItem, resolved.CopyLocal.ToString ());

			return resolved;
		}

		bool TryGetSpecificVersionValue (ITaskItem item, out bool specific_version)
		{
			specific_version = true;
			string value = item.GetMetadata ("SpecificVersion");
			if (String.IsNullOrEmpty (value)) {
				//AssemblyName name = new AssemblyName (item.ItemSpec);
				// If SpecificVersion is not specified, then
				// it is true if the Include is a strong name else false
				//specific_version = assembly_resolver.IsStrongNamed (name);

				// msbuild seems to just look for a ',' in the name :/
				specific_version = item.ItemSpec.IndexOf (',') >= 0;
				return true;
			}

			if (Boolean.TryParse (value, out specific_version))
				return true;

			Log.LogError ("Item '{0}' has attribute SpecificVersion with invalid value '{1}' " +
					"which could not be converted to a boolean.", item.ItemSpec, value);
			return false;
		}

		//FIXME: Consider CandidateAssemblyFiles also here
		void ResolveAssemblyFiles ()
		{
			if (assemblyFiles == null)
				return;

			foreach (ITaskItem item in assemblyFiles) {
				assembly_resolver.ResetSearchLogger ();

				if (!File.Exists (item.ItemSpec)) {
					LogWithPrecedingNewLine (MessageImportance.Low,
							"Primary Reference from AssemblyFiles {0}, file not found. Ignoring",
							item.ItemSpec);
					continue;
				}

				LogWithPrecedingNewLine (MessageImportance.Low, "Primary Reference from AssemblyFiles {0}", item.ItemSpec);
				string copy_local;

				AssemblyName aname;
				if (!assembly_resolver.TryGetAssemblyNameFromFile (item.ItemSpec, out aname)) {
					Log.LogWarning ("Reference '{0}' not resolved", item.ItemSpec);
					assembly_resolver.LogSearchLoggerMessages (MessageImportance.Normal);
					continue;
				}

				if (Environment.GetEnvironmentVariable ("XBUILD_LOG_REFERENCE_RESOLVER") != null)
					assembly_resolver.LogSearchLoggerMessages (MessageImportance.Low);

				ResolvedReference rr = assembly_resolver.GetResolvedReference (item, item.ItemSpec, aname, true,
						SearchPath.RawFileName);
				copy_local = rr.CopyLocal.ToString ();

				if (!TryAddNewReference (tempResolvedFiles, rr))
					// already resolved
					continue;

				SetCopyLocal (rr.TaskItem, copy_local);

				FindAndAddRelatedFiles (item.ItemSpec, copy_local);
				FindAndAddSatellites (item.ItemSpec, copy_local);

				if (FindDependencies && !IsFromGacOrTargetFramework (rr) &&
						rr.FoundInSearchPath != SearchPath.PkgConfig)
					primaryReferences.Add (new PrimaryReference (item, copy_local));
			}
		}

		// Tries to resolve assemblies referenced by @item
		// Skips gac references
		// @item : filename
		void ResolveAssemblyFileDependencies (ITaskItem item, string parent_copy_local)
		{
			Queue<string> dependencies = new Queue<string> ();
			dependencies.Enqueue (item.ItemSpec);

			while (dependencies.Count > 0) {
				string filename = Path.GetFullPath (dependencies.Dequeue ());
				Assembly asm = Assembly.ReflectionOnlyLoadFrom (filename);
				if (alreadyScannedAssemblyNames.ContainsKey (asm.FullName))
					continue;

				// set the 1st search path to this ref's base path
				// Will be used for resolving the dependencies
				dependency_search_paths [0] = Path.GetDirectoryName (filename);

				foreach (AssemblyName aname in asm.GetReferencedAssemblies ()) {
					if (alreadyScannedAssemblyNames.ContainsKey (aname.FullName))
						continue;

					ResolvedReference resolved_ref = ResolveDependencyByAssemblyName (
							aname, asm.FullName, parent_copy_local);

					if (resolved_ref != null && !IsFromGacOrTargetFramework (resolved_ref)
							&& resolved_ref.FoundInSearchPath != SearchPath.PkgConfig)
						dependencies.Enqueue (resolved_ref.TaskItem.ItemSpec);
				}
				alreadyScannedAssemblyNames.Add (asm.FullName, String.Empty);
			}
		}

		// Resolves by looking dependency_search_paths
		// which is dir of parent reference file, and
		// SearchPaths
		ResolvedReference ResolveDependencyByAssemblyName (AssemblyName aname, string parent_asm_name,
				string parent_copy_local)
		{
			// This will check for compatible assembly name/version
			ResolvedReference resolved_ref;
			if (TryGetResolvedReferenceByAssemblyName (aname, false, out resolved_ref))
				return resolved_ref;

			LogWithPrecedingNewLine (MessageImportance.Low, "Dependency {0}", aname);
			Log.LogMessage (MessageImportance.Low, "\tRequired by {0}", parent_asm_name);

			ITaskItem item = new TaskItem (aname.FullName);
			item.SetMetadata ("SpecificVersion", "false");
			resolved_ref = ResolveReference (item, dependency_search_paths, false);

			if (resolved_ref != null) {
				if (Environment.GetEnvironmentVariable ("XBUILD_LOG_REFERENCE_RESOLVER") != null)
						assembly_resolver.LogSearchLoggerMessages (MessageImportance.Low);

				Log.LogMessage (MessageImportance.Low, "\tReference {0} resolved to {1}.",
					aname, resolved_ref.TaskItem.ItemSpec);

				Log.LogMessage (MessageImportance.Low,
						"\tReference found at search path {0}",
						resolved_ref.FoundInSearchPathAsString);

				if (resolved_ref.FoundInSearchPath == SearchPath.Directory) {
					// override CopyLocal with parent's val
					SetCopyLocal (resolved_ref.TaskItem, parent_copy_local);

					Log.LogMessage (MessageImportance.Low,
							"\tThis is CopyLocal {0} as parent item has this value",
							parent_copy_local);

					if (TryAddNewReference (tempResolvedFiles, resolved_ref)) {
						FindAndAddRelatedFiles (resolved_ref.TaskItem.ItemSpec, parent_copy_local);
						FindAndAddSatellites (resolved_ref.TaskItem.ItemSpec, parent_copy_local);
					}
				} else {
					//gac or tgtfmwk
					Log.LogMessage (MessageImportance.Low,
							"\tThis is CopyLocal false as it is in the gac," +
							"target framework directory or provided by a package.");

					TryAddNewReference (tempResolvedFiles, resolved_ref);
				}
			} else {
				Log.LogWarning ("Reference '{0}' not resolved", aname);
				assembly_resolver.LogSearchLoggerMessages (MessageImportance.Normal);
			}

			return resolved_ref;
		}

		void FindAndAddRelatedFiles (string filename, string parent_copy_local)
		{
			if (!findRelatedFiles || allowedRelatedFileExtensions == null)
				return;

			foreach (string ext in allowedRelatedFileExtensions) {
				string rfile = filename + ext;
				if (File.Exists (rfile)) {
					ITaskItem item = new TaskItem (rfile);
					SetCopyLocal (item, parent_copy_local);

					tempRelatedFiles.AddUniqueFile (item);
				}
			}
		}

		void FindAndAddSatellites (string filename, string parent_copy_local)
		{
			if (!FindSatellites)
				return;

			string basepath = Path.GetDirectoryName (filename);
			string resource = String.Format ("{0}{1}{2}",
					Path.GetFileNameWithoutExtension (filename),
					".resources",
					Path.GetExtension (filename));

			string dir_sep = Path.DirectorySeparatorChar.ToString ();
			foreach (string dir in Directory.GetDirectories (basepath)) {
				string culture = Path.GetFileName (dir);
				if (!CultureNamesTable.ContainsKey (culture))
					continue;

				string res_path = Path.Combine (dir, resource);
				if (File.Exists (res_path)) {
					ITaskItem item = new TaskItem (res_path);
					SetCopyLocal (item, parent_copy_local);
					item.SetMetadata ("DestinationSubdirectory", culture + dir_sep);
					tempSatelliteFiles.AddUniqueFile (item);
				}
			}
		}

		// returns true is it was new
		bool TryAddNewReference (List<ITaskItem> file_list, ResolvedReference key_ref)
		{
			ResolvedReference found_ref;
			if (!TryGetResolvedReferenceByAssemblyName (key_ref.AssemblyName, key_ref.IsPrimary, out found_ref)) {
				assemblyNameToResolvedRef [key_ref.AssemblyName.Name] = key_ref;
				file_list.Add (key_ref.TaskItem);

				return true;
			}
			return false;
		}

		void SetCopyLocal (ITaskItem item, string copy_local)
		{
			item.SetMetadata ("CopyLocal", copy_local);

			// Assumed to be valid value
			if (Boolean.Parse (copy_local))
				tempCopyLocalFiles.AddUniqueFile (item);
		}

		bool TryGetResolvedReferenceByAssemblyName (AssemblyName key_aname, bool is_primary, out ResolvedReference found_ref)
		{
			found_ref = null;
			// Match by just name
			if (!assemblyNameToResolvedRef.TryGetValue (key_aname.Name, out found_ref))
				// not there
				return false;

			// match for full name
			if (AssemblyResolver.AssemblyNamesCompatible (key_aname, found_ref.AssemblyName, true, false))
				// exact match, so its already there, dont add anything
				return true;

			// we have a name match, but version mismatch!
			assembly_resolver.LogSearchMessage ("A conflict was detected between '{0}' and '{1}'",
					key_aname.FullName, found_ref.AssemblyName.FullName);

			if (is_primary == found_ref.IsPrimary) {
				assembly_resolver.LogSearchMessage ("Unable to choose between the two. " +
						"Choosing '{0}' arbitrarily.", found_ref.AssemblyName.FullName);
				return true;
			}

			// since all dependencies are processed after
			// all primary refererences, the one in the cache
			// has to be a primary
			// Prefer a primary reference over a dependency

			assembly_resolver.LogSearchMessage ("Choosing '{0}' as it is a primary reference.",
					found_ref.AssemblyName.FullName);

			LogConflictWarning (found_ref.AssemblyName.FullName, key_aname.FullName);

			return true;
		}

		void LogWithPrecedingNewLine (MessageImportance importance, string format, params object [] args)
		{
			Log.LogMessage (importance, String.Empty);
			Log.LogMessage (importance, format, args);
		}

		// conflict b/w @main and @conflicting, picking @main
		void LogConflictWarning (string main, string conflicting)
		{
			string key = main + ":" + conflicting;
			if (!conflictWarningsCache.ContainsKey (key)) {
				Log.LogWarning ("Found a conflict between : '{0}' and '{1}'. Using '{0}' reference.",
						main, conflicting);
				conflictWarningsCache [key] = key;
			}
		}

		bool IsFromGacOrTargetFramework (ResolvedReference rr)
		{
			return rr.FoundInSearchPath == SearchPath.Gac ||
				rr.FoundInSearchPath == SearchPath.TargetFrameworkDirectory;
		}

		public bool AutoUnify {
			get { return autoUnify; }
			set { autoUnify = value; }
		}
		
		public ITaskItem[] AssemblyFiles {
			get { return assemblyFiles; }
			set { assemblyFiles = value; }
		}
		
		public ITaskItem[] Assemblies {
			get { return assemblies; }
			set { assemblies = value; }
		}
		
		public string AppConfigFile {
			get { return appConfigFile; }
			set { appConfigFile = value; }
		}
		
		public string[] AllowedAssemblyExtensions {
			get { return allowedAssemblyExtensions; }
			set { allowedAssemblyExtensions = value; }
		}

		public string[] AllowedRelatedFileExtensions {
			get { return allowedRelatedFileExtensions; }
			set { allowedRelatedFileExtensions = value; }
		}
		
		public string[] CandidateAssemblyFiles {
			get { return candidateAssemblyFiles; }
			set { candidateAssemblyFiles = value; }
		}
		
		[Output]
		public ITaskItem[] CopyLocalFiles {
			get { return copyLocalFiles; }
		}
		
		[Output]
		public ITaskItem[] FilesWritten {
			get { return filesWritten; }
			set { filesWritten = value; }
		}
		
		public bool FindDependencies {
			get { return findDependencies; }
			set { findDependencies = value; }
		}
		
		public bool FindRelatedFiles {
			get { return findRelatedFiles; }
			set { findRelatedFiles = value; }
		}
		
		public bool FindSatellites {
			get { return findSatellites; }
			set { findSatellites = value; }
		}
		
		public bool FindSerializationAssemblies {
			get { return findSerializationAssemblies; }
			set { findSerializationAssemblies = value; }
		}
		
		public string[] InstalledAssemblyTables {
			get { return installedAssemblyTables; }
			set { installedAssemblyTables = value; }
		}
		
		[Output]
		public ITaskItem[] RelatedFiles {
			get { return relatedFiles; }
		}
		
		[Output]
		public ITaskItem[] ResolvedDependencyFiles {
			get { return resolvedDependencyFiles; }
		}
		
		[Output]
		public ITaskItem[] ResolvedFiles {
			get { return resolvedFiles; }
		}
		
		[Output]
		public ITaskItem[] SatelliteFiles {
			get { return satelliteFiles; }
		}
		
		[Output]
		public ITaskItem[] ScatterFiles {
			get { return scatterFiles; }
		}
		
		[Required]
		public string[] SearchPaths {
			get { return searchPaths; }
			set { searchPaths = value; }
		}
		
		[Output]
		public ITaskItem[] SerializationAssemblyFiles {
			get { return serializationAssemblyFiles; }
		}
		
		public bool Silent {
			get { return silent; }
			set { silent = value; }
		}
		
		public string StateFile {
			get { return stateFile; }
			set { stateFile = value; }
		}
		
		[Output]
		public ITaskItem[] SuggestedRedirects {
			get { return suggestedRedirects; }
		}

#if NET_4_0
		public string TargetFrameworkMoniker { get; set; }

		public string TargetFrameworkMonikerDisplayName { get; set; }
#endif

		public string TargetFrameworkVersion { get; set; }

		public string[] TargetFrameworkDirectories {
			get { return targetFrameworkDirectories; }
			set { targetFrameworkDirectories = value; }
		}
		
		public string TargetProcessorArchitecture {
			get { return targetProcessorArchitecture; }
			set { targetProcessorArchitecture = value; }
		}


                static Dictionary<string, string> cultureNamesTable;
                static Dictionary<string, string> CultureNamesTable {
                        get {
                                if (cultureNamesTable == null) {
                                        cultureNamesTable = new Dictionary<string, string> ();
                                        foreach (CultureInfo ci in CultureInfo.GetCultures (CultureTypes.AllCultures))
                                                cultureNamesTable [ci.Name] = ci.Name;
                                }

                                return cultureNamesTable;
                        }
                }
	}

	static class ResolveAssemblyReferenceHelper {
		public static void AddUniqueFile (this Dictionary<string, ITaskItem> dic, ITaskItem item)
		{
			if (dic == null)
				throw new ArgumentNullException ("dic");
			if (item == null)
				throw new ArgumentNullException ("item");

			string fullpath = Path.GetFullPath (item.ItemSpec);
			if (!dic.ContainsKey (fullpath))
				dic [fullpath] = item;
		}
	}

	struct PrimaryReference {
		public ITaskItem TaskItem;
		public string ParentCopyLocal;

		public PrimaryReference (ITaskItem item, string parent_copy_local)
		{
			TaskItem = item;
			ParentCopyLocal = parent_copy_local;
		}
	}

}

#endif
