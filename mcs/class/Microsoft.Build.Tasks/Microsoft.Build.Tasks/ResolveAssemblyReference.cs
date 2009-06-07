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
		static string []	assembly_file_search_paths;
		static string []	default_assembly_extensions;

		AssemblyResolver	assembly_resolver;
		List<ITaskItem>	tempSatelliteFiles, tempRelatedFiles, tempResolvedDepFiles;
		List<ITaskItem> tempResolvedFiles, tempCopyLocalFiles;

		static ResolveAssemblyReference ()
		{
			assembly_file_search_paths = new string [] {
				"{TargetFrameworkDirectory}", "{GAC}", String.Empty };
			default_assembly_extensions = new string [] { ".dll", ".exe" };
		}

		public ResolveAssemblyReference ()
		{
			assembly_resolver = new AssemblyResolver ();
		}

		//FIXME: make this reusable
		//FIXME: make sure finals refs are not repeated
		public override bool Execute ()
		{
			assembly_resolver.Log = Log;
			tempResolvedFiles = new List <ITaskItem> ();
			tempCopyLocalFiles = new List <ITaskItem> ();
			tempSatelliteFiles = new List<ITaskItem> ();
			tempRelatedFiles = new List<ITaskItem> ();
			tempResolvedDepFiles = new List<ITaskItem> ();

			foreach (ITaskItem item in assemblies) {
				if (!String.IsNullOrEmpty (item.GetMetadata ("SubType"))) {
					Log.LogWarning ("Reference '{0}' has non-empty SubType. Ignoring.", item.ItemSpec);
					continue;
				}

				Log.LogMessage (MessageImportance.Low, "Primary Reference {0}", item.ItemSpec);
				ResolvedReference resolved_ref = ResolveReference (item, searchPaths);
				if (resolved_ref == null) {
					Log.LogWarning ("\tReference '{0}' not resolved", item.ItemSpec);
					Log.LogMessage ("{0}", assembly_resolver.SearchLogger.ToString ());
				} else {
					Log.LogMessage (MessageImportance.Low,
							"\tReference {0} resolved to {1}. CopyLocal = {2}",
							item.ItemSpec, resolved_ref.TaskItem,
							resolved_ref.TaskItem.GetMetadata ("CopyLocal"));

					tempResolvedFiles.Add (resolved_ref.TaskItem);

					if (!IsFromGacOrTargetFramework (resolved_ref))
						ResolveAssemblyFileDependencies (resolved_ref.TaskItem,
								resolved_ref.TaskItem.GetMetadata ("CopyLocal"));
				}
			}

			ResolveAssemblyFiles ();
			
			resolvedFiles = tempResolvedFiles.ToArray ();
			copyLocalFiles = tempCopyLocalFiles.ToArray ();
			satelliteFiles = tempSatelliteFiles.ToArray ();
			relatedFiles = tempRelatedFiles.ToArray ();
			resolvedDependencyFiles = tempResolvedDepFiles.ToArray ();

			tempResolvedFiles.Clear ();
			tempCopyLocalFiles.Clear ();
			tempSatelliteFiles.Clear ();
			tempRelatedFiles.Clear ();
			tempResolvedDepFiles.Clear ();

			return true;
		}

		// Use @search_paths to resolve the reference
		ResolvedReference ResolveReference (ITaskItem item, string [] search_paths)
		{
			assembly_resolver.ResetSearchLogger ();

			ResolvedReference resolved = null;
			foreach (string spath in search_paths) {
				bool specific_version;
				if (!TryGetSpecificVersionValue (item, out specific_version))
					return null;

				assembly_resolver.SearchLogger.WriteLine ("For searchpath {0}", spath);

				if (String.Compare (spath, "{HintPathFromItem}") == 0) {
					resolved = assembly_resolver.ResolveHintPathReference (item, specific_version);
				} else if (String.Compare (spath, "{TargetFrameworkDirectory}") == 0) {
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
					if (assembly_resolver.GetAssemblyNameFromFile (item.ItemSpec) != null)
						resolved = assembly_resolver.GetResolvedReference (item, item.ItemSpec, true,
								SearchPath.RawFileName);
				} else if (String.Compare (spath, "{CandidateAssemblyFiles}") == 0) {
					assembly_resolver.SearchLogger.WriteLine (
							"Warning: {CandidateAssemblyFiles} not supported currently");
				} else {
					resolved = assembly_resolver.FindInDirectory (
							item, spath,
							allowedAssemblyExtensions ?? default_assembly_extensions);
				}

				if (resolved != null)
					break;
			}

			if (resolved != null)
				SetCopyLocal (resolved.TaskItem, resolved.CopyLocal.ToString ());

			return resolved;
		}

		bool TryGetSpecificVersionValue (ITaskItem item, out bool specific_version)
		{
			specific_version = true;
			string value = item.GetMetadata ("SpecificVersion");
			if (String.IsNullOrEmpty (value)) {
				AssemblyName name = new AssemblyName (item.ItemSpec);
				// If SpecificVersion is not specified, then
				// it is true if the Include is a strong name else false
				specific_version = assembly_resolver.IsStrongNamed (name);
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
			foreach (ITaskItem item in assemblyFiles) {
				if (!File.Exists (item.ItemSpec)) {
					Log.LogMessage (MessageImportance.Low,
							"Primary Reference from AssemblyFiles {0}, file not found. Ignoring",
							item.ItemSpec);
					continue;
				}

				Log.LogMessage (MessageImportance.Low, "Primary Reference from AssemblyFiles {0}", item.ItemSpec);
				string copy_local;

				ResolvedReference rr = assembly_resolver.GetResolvedReference (item, item.ItemSpec, true,
						SearchPath.RawFileName);
				copy_local = rr.CopyLocal.ToString ();

				tempResolvedFiles.Add (rr.TaskItem);
				SetCopyLocal (rr.TaskItem, copy_local);

				FindAndAddRelatedFiles (item.ItemSpec, copy_local);
				FindAndAddSatellites (item.ItemSpec, copy_local);

				if (FindDependencies && !IsFromGacOrTargetFramework (rr))
					ResolveAssemblyFileDependencies (item, copy_local);
			}
		}

		//FIXME: caching

		// Tries to resolve assemblies referenced by @item
		// Skips gac references
		// @item : filename
		void ResolveAssemblyFileDependencies (ITaskItem item, string parent_copy_local)
		{
			string basepath = Path.GetDirectoryName (item.ItemSpec);

			// set the 3rd search path to this ref's base path
			// Will be used for resolving the dependencies
			assembly_file_search_paths [2] = basepath;

			Dictionary<string, string> alreadyResolvedAssemblies = new Dictionary<string, string> ();

			Queue<string> dependencies = new Queue<string> ();
			dependencies.Enqueue (item.ItemSpec);

			while (dependencies.Count > 0) {
				Assembly asm = Assembly.ReflectionOnlyLoadFrom (dependencies.Dequeue ());
				if (alreadyResolvedAssemblies.ContainsKey (asm.FullName))
					continue;

				foreach (AssemblyName aname in asm.GetReferencedAssemblies ()) {
					if (alreadyResolvedAssemblies.ContainsKey (aname.FullName))
						continue;

					Log.LogMessage (MessageImportance.Low, "Dependency {0}", aname);
					Log.LogMessage (MessageImportance.Low, "\tRequired by {0}", asm.FullName);

					ResolvedReference resolved_ref = ResolveDependencyByAssemblyName (
							aname, parent_copy_local);

					if (resolved_ref != null && !IsFromGacOrTargetFramework (resolved_ref)) {
						dependencies.Enqueue (resolved_ref.TaskItem.ItemSpec);
						FindAndAddSatellites (resolved_ref.TaskItem.ItemSpec, parent_copy_local);
					}
				}
				alreadyResolvedAssemblies.Add (asm.FullName, String.Empty);
			}
		}

		// Resolves by looking assembly_file_search_paths
		// which has - gac, tgtfmwk, and base dir of the parent
		// reference
		ResolvedReference ResolveDependencyByAssemblyName (AssemblyName aname, string parent_copy_local)
		{
			// Look in TargetFrameworkDirectory, Gac
			ITaskItem item = new TaskItem (aname.FullName);
			item.SetMetadata ("SpecificVersion", "false");
			ResolvedReference resolved_ref = ResolveReference (
							item,
							assembly_file_search_paths);

			string copy_local = "false";
			if (resolved_ref != null) {
				Log.LogMessage (MessageImportance.Low, "\tReference {0} resolved to {1}.",
					aname, resolved_ref.TaskItem.ItemSpec);

				if (resolved_ref.FoundInSearchPath == SearchPath.Directory) {
					// override CopyLocal with parent's val
					resolved_ref.TaskItem.SetMetadata ("CopyLocal", parent_copy_local);

					Log.LogMessage (MessageImportance.Low,
							"\tThis is CopyLocal {0} as parent item has this value",
							copy_local);

					FindAndAddRelatedFiles (resolved_ref.TaskItem.ItemSpec, parent_copy_local);
				} else {
					//gac or tgtfmwk
					Log.LogMessage (MessageImportance.Low,
							"\tThis is CopyLocal {0} as it is in the gac or one " +
							"of the target framework directories",
							copy_local);

				}

				tempResolvedFiles.Add (resolved_ref.TaskItem);
			} else {
				Log.LogWarning ("\tReference '{0}' not resolved", aname);
				Log.LogMessage ("{0}", assembly_resolver.SearchLogger.ToString ());
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

					tempRelatedFiles.Add (item);
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
					tempSatelliteFiles.Add (item);
				}
			}
		}

		void SetCopyLocal (ITaskItem item, string copy_local)
		{
			item.SetMetadata ("CopyLocal", copy_local);

			// Assumed to be valid value
			if (Boolean.Parse (copy_local))
				tempCopyLocalFiles.Add (item);
		}

		bool IsCopyLocal (ITaskItem item)
		{
			return Boolean.Parse (item.GetMetadata ("CopyLocal"));
		}

		bool IsFromTargetFramework (string filename)
		{
			foreach (string fpath in targetFrameworkDirectories)
				if (filename.StartsWith (fpath))
					return true;

			return false;
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
}

#endif
