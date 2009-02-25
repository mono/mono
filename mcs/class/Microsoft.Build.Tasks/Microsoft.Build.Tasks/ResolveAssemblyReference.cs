//
// ResolveAssemblyReference.cs: Searches for assembly files.
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

#if NET_2_0

using System;
using System.Collections.Generic;
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

		AssemblyResolver	assembly_resolver;

		public ResolveAssemblyReference ()
		{
			assembly_resolver = new AssemblyResolver ();
		}

		public override bool Execute ()
		{
			assembly_resolver.Log = Log;
			List <ITaskItem> tempResolvedFiles = new List <ITaskItem> ();
		
			foreach (ITaskItem item in assemblies) {
				assembly_resolver.ResetSearchLogger ();
				//FIXME: Copy local
				string resolved = null;
				foreach (string spath in searchPaths) {
					bool specific_version;
					if (!TryGetSpecificVersionValue (item, out specific_version))
						return false;

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
							resolved = item.ItemSpec;
					} else {
						resolved = assembly_resolver.FindInDirectory (item, spath);
					}

					if (resolved != null)
						break;
				}

				if (resolved == null) {
					Log.LogWarning ("Reference '{0}' not resolved", item.ItemSpec);
					Log.LogMessage ("{0}", assembly_resolver.SearchLogger.ToString ());
				} else {
					Log.LogMessage (MessageImportance.Low, "Reference {0} resolved to {1}", item.ItemSpec, resolved);
					tempResolvedFiles.Add (new TaskItem (resolved));
				}
			}
			
			resolvedFiles = tempResolvedFiles.ToArray ();

			return true;
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
	}
}

#endif
