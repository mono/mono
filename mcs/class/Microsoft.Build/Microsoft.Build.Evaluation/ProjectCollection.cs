//
// ProjectCollection.cs
//
// Author:
//   Leszek Ciesielski (skolima@gmail.com)
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// (C) 2011 Leszek Ciesielski
// Copyright (C) 2011,2013 Xamarin Inc.
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
//

using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Reflection;
using System.Globalization;
using Mono.XBuild.Utilities;
using Microsoft.Build.Internal;

namespace Microsoft.Build.Evaluation
{
	public class ProjectCollection : IDisposable
	{
		public delegate void ProjectAddedEventHandler (object sender, ProjectAddedToProjectCollectionEventArgs e);
		
		public class ProjectAddedToProjectCollectionEventArgs : EventArgs
		{
			public ProjectAddedToProjectCollectionEventArgs (ProjectRootElement element)
			{
				if (element == null)
					throw new ArgumentNullException ("project");
				ProjectRootElement = element;
			}
			
			public ProjectRootElement ProjectRootElement { get; private set; }
		}

		// static members

		static readonly ProjectCollection global_project_collection;

		static ProjectCollection ()
		{
			global_project_collection = new ProjectCollection (new ReadOnlyDictionary<string, string> (new Dictionary<string, string> ()));
		}

		public static string Escape (string unescapedString)
		{
			return Mono.XBuild.Utilities.MSBuildUtils.Escape (unescapedString);
		}

		public static string Unescape (string escapedString)
		{
			return Mono.XBuild.Utilities.MSBuildUtils.Unescape (escapedString);
		}

		public static ProjectCollection GlobalProjectCollection {
			get { return global_project_collection; }
		}

		// semantic model part

		public ProjectCollection ()
			: this (null)
		{
		}

		public ProjectCollection (IDictionary<string, string> globalProperties)
        	: this (globalProperties, null, ToolsetDefinitionLocations.Registry | ToolsetDefinitionLocations.ConfigurationFile)
		{
		}

		public ProjectCollection (ToolsetDefinitionLocations toolsetLocations)
        	: this (null, null, toolsetLocations)
		{
		}

		public ProjectCollection (IDictionary<string, string> globalProperties, IEnumerable<ILogger> loggers,
				ToolsetDefinitionLocations toolsetDefinitionLocations)
			: this (globalProperties, loggers, null, toolsetDefinitionLocations, 1, false)
		{
		}

		public ProjectCollection (IDictionary<string, string> globalProperties,
				IEnumerable<ILogger> loggers, IEnumerable<ForwardingLoggerRecord> remoteLoggers,
				ToolsetDefinitionLocations toolsetDefinitionLocations,
				int maxNodeCount, bool onlyLogCriticalEvents)
		{
			global_properties = globalProperties ?? new Dictionary<string, string> ();
			this.loggers = loggers != null ? loggers.ToList () : new List<ILogger> ();
			toolset_locations = toolsetDefinitionLocations;
			MaxNodeCount = maxNodeCount;
			OnlyLogCriticalEvents = onlyLogCriticalEvents;

			LoadDefaultToolsets ();
		}
		
		[MonoTODO ("not fired yet")]
		public event ProjectAddedEventHandler ProjectAdded;
		[MonoTODO ("not fired yet")]
		public event EventHandler<ProjectChangedEventArgs> ProjectChanged;
		[MonoTODO ("not fired yet")]
		public event EventHandler<ProjectCollectionChangedEventArgs> ProjectCollectionChanged;
		[MonoTODO ("not fired yet")]
		public event EventHandler<ProjectXmlChangedEventArgs> ProjectXmlChanged;

		public void AddProject (Project project)
		{
			this.loaded_projects.Add (project);
			if (ProjectAdded != null)
				ProjectAdded (this, new ProjectAddedToProjectCollectionEventArgs (project.Xml));
		}

		public int Count {
			get { return loaded_projects.Count; }
		}

		string default_tools_version;
		public string DefaultToolsVersion {
			get { return default_tools_version; }
			set {
				if (GetToolset (value) == null)
					throw new InvalidOperationException (string.Format ("Toolset '{0}' does not exist", value));
				default_tools_version = value;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
			}
		}

		public ICollection<Project> GetLoadedProjects (string fullPath)
		{
			return LoadedProjects.Where (p => p.FullPath != null && Path.GetFullPath (p.FullPath) == Path.GetFullPath (fullPath)).ToList ();
		}

		readonly IDictionary<string, string> global_properties;

		public IDictionary<string, string> GlobalProperties {
			get { return global_properties; }
		}

		readonly List<Project> loaded_projects = new List<Project> ();
		
		public Project LoadProject (string fileName)
		{
			return LoadProject (fileName, DefaultToolsVersion);
		}
		
		public Project LoadProject (string fileName, string toolsVersion)
		{
			return LoadProject (fileName, null, toolsVersion);
		}
		
		public Project LoadProject (string fileName, IDictionary<string,string> globalProperties, string toolsVersion)
		{
			var ret = new Project (fileName, globalProperties, toolsVersion);
			loaded_projects.Add (ret);
			return ret;
		}
		
		// These methods somehow don't add the project to ProjectCollection...
		public Project LoadProject (XmlReader xmlReader)
		{
			return LoadProject (xmlReader, DefaultToolsVersion);
		}
		
		public Project LoadProject (XmlReader xmlReader, string toolsVersion)
		{
			return LoadProject (xmlReader, null, toolsVersion);
		}
		
		public Project LoadProject (XmlReader xmlReader, IDictionary<string,string> globalProperties, string toolsVersion)
		{
			return new Project (xmlReader, globalProperties, toolsVersion);
		}
		
		public ICollection<Project> LoadedProjects {
			get { return loaded_projects; }
		}

		readonly List<ILogger> loggers = new List<ILogger> ();
		
		public ICollection<ILogger> Loggers {
			get { return loggers; }
		}

		[MonoTODO]
		public bool OnlyLogCriticalEvents { get; set; }

		[MonoTODO]
		public bool SkipEvaluation { get; set; }

		readonly ToolsetDefinitionLocations toolset_locations;
		public ToolsetDefinitionLocations ToolsetLocations {
			get { return toolset_locations; }
		}

		readonly List<Toolset> toolsets = new List<Toolset> ();
		// so what should we do without ToolLocationHelper in Microsoft.Build.Utilities.dll? There is no reference to it in this dll.
		public ICollection<Toolset> Toolsets {
			// For ConfigurationFile and None, they cannot be added externally.
			get { return (ToolsetLocations & ToolsetDefinitionLocations.Registry) != 0 ? toolsets : toolsets.ToList (); }
		}
		
		public Toolset GetToolset (string toolsVersion)
		{
			return Toolsets.FirstOrDefault (t => t.ToolsVersion == toolsVersion);
		}

		//FIXME: should also support config file, depending on ToolsetLocations
		void LoadDefaultToolsets ()
		{
			AddToolset (new Toolset ("4.0",
				ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version40), this, null));
#if XBUILD_12
			AddToolset (new Toolset ("12.0", ToolLocationHelper.GetPathToBuildTools ("12.0"), this, null));
#endif
#if XBUILD_14
			AddToolset (new Toolset ("14.0", ToolLocationHelper.GetPathToBuildTools ("14.0"), this, null));
#endif

			// We don't support these anymore
			AddToolset (new Toolset ("2.0",
				ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version20), this, null));
			AddToolset (new Toolset ("3.0",
				ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version30), this, null));
			AddToolset (new Toolset ("3.5",
				ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version35), this, null));

			default_tools_version = toolsets [0].ToolsVersion;
		}
		
		[MonoTODO ("not verified at all")]
		public void AddToolset (Toolset toolset)
		{
			toolsets.Add (toolset);
		}
		
		[MonoTODO ("not verified at all")]
		public void RemoveAllToolsets ()
		{
			toolsets.Clear ();
		}
		
		public void RegisterLogger (ILogger logger)
		{
			loggers.Add (logger);
		}
		
		public void RegisterLoggers (IEnumerable<ILogger> loggers)
		{
			foreach (var logger in loggers)
				this.loggers.Add (logger);
		}

		public void UnloadAllProjects ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not verified at all")]
		public void UnloadProject (Project project)
		{
			this.loaded_projects.Remove (project);
		}

		[MonoTODO ("Not verified at all")]
		public void UnloadProject (ProjectRootElement projectRootElement)
		{
			foreach (var proj in loaded_projects.Where (p => p.Xml == projectRootElement).ToArray ())
				UnloadProject (proj);
		}

		public static Version Version {
			get { throw new NotImplementedException (); }
		}

		// Execution part

		[MonoTODO]
		public bool DisableMarkDirty { get; set; }

		[MonoTODO]
		public HostServices HostServices { get; set; }

		[MonoTODO]
		public bool IsBuildEnabled { get; set; }
		
		internal string BuildStartupDirectory { get; set; }
		
		internal int MaxNodeCount { get; private set; }
		
		Stack<string> ongoing_imports = new Stack<string> ();
		
		internal Stack<string> OngoingImports {
			get { return ongoing_imports; }
		}
		
		// common part
		internal static IEnumerable<EnvironmentProjectProperty> GetWellKnownProperties (Project project)
		{
			Func<string,string,EnvironmentProjectProperty> create = (name, value) => new EnvironmentProjectProperty (project, name, value, true);
			return GetWellKnownProperties (create);
		}
		
		internal static IEnumerable<ProjectPropertyInstance> GetWellKnownProperties (ProjectInstance project)
		{
			Func<string,string,ProjectPropertyInstance> create = (name, value) => new ProjectPropertyInstance (name, true, value);
			return GetWellKnownProperties (create);
		}
		
		static IEnumerable<T> GetWellKnownProperties<T> (Func<string,string,T> create)
		{
			yield return create ("OS", OS);
			var ext = Environment.GetEnvironmentVariable ("MSBuildExtensionsPath") ?? DefaultExtensionsPath;
			yield return create ("MSBuildExtensionsPath", ext);
			var ext32 = Environment.GetEnvironmentVariable ("MSBuildExtensionsPath32") ?? ext;
			yield return create ("MSBuildExtensionsPath32", ext32);
			var ext64 = Environment.GetEnvironmentVariable ("MSBuildExtensionsPath64") ?? ext;
			yield return create ("MSBuildExtensionsPath64", ext64);
		}

		static string OS {
			get {
				PlatformID pid = Environment.OSVersion.Platform;
				switch ((int) pid) {
				case 128:
				case 4:
					return "Unix";
				case 6:
					return "OSX";
				default:
					return "Windows_NT";
				}
			}
		}

		#region Extension Paths resolution

		static string extensions_path;
		internal static string DefaultExtensionsPath {
			get {
				if (extensions_path == null) {
					// NOTE: code from mcs/tools/gacutil/driver.cs
					PropertyInfo gac = typeof (System.Environment).GetProperty (
							"GacPath", BindingFlags.Static | BindingFlags.NonPublic);

					if (gac != null) {
						MethodInfo get_gac = gac.GetGetMethod (true);
						string gac_path = (string) get_gac.Invoke (null, null);
						extensions_path = Path.GetFullPath (Path.Combine (
									gac_path, Path.Combine ("..", "xbuild")));
					}
				}
				return extensions_path;
			}
		}

		static string DotConfigExtensionsPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData),
			Path.Combine ("xbuild", "tasks"));
		const string MacOSXExternalXBuildDir = "/Library/Frameworks/Mono.framework/External/xbuild";
		static string PathSeparatorAsString = Path.PathSeparator.ToString ();

		// Gives a list of extensions paths to try for $(MSBuildExtensionsPath),
		// *in-order*
		internal static IEnumerable<string> GetApplicableExtensionsPaths (Action<string> logMessage)
		{
			string envvar = String.Join (PathSeparatorAsString, new string [] {
				// For mac osx, look in the 'External' dir on macosx,
				// see bug #663180
				MSBuildUtils.RunningOnMac ? MacOSXExternalXBuildDir : String.Empty,
				DotConfigExtensionsPath,
				DefaultExtensionsPath});

			var pathsTable = new Dictionary<string, string> ();
			foreach (string extn_path in envvar.Split (new char [] {Path.PathSeparator}, StringSplitOptions.RemoveEmptyEntries)) {
				if (pathsTable.ContainsKey (extn_path))
					continue;

				if (!Directory.Exists (extn_path)) {
					logMessage (string.Format ("Extension path '{0}' not found, ignoring.", extn_path));
					continue;
				}

				pathsTable [extn_path] = extn_path;
				yield return extn_path;
			}
		}

		internal static string FindFileInSeveralExtensionsPath (ref string extensionsPathOverride, Func<string,string> expandString, string file, Action<string> logMessage)
		{
			string ret = null;
			string ex = extensionsPathOverride;
			Func<bool> action = () => {
				string path = WindowsCompatibilityExtensions.FindMatchingPath (expandString (file));
				if (File.Exists (path))
					ret = path;
				else
					return false;
				return true;
			};

			try {
				if (!action ()) {
					foreach (var s in ProjectCollection.GetApplicableExtensionsPaths (logMessage)) {
						extensionsPathOverride = s;
						ex = s;
						if (action ())
							break;
					}
				}
			} finally {
				extensionsPathOverride = null;
			}

			return ret ?? WindowsCompatibilityExtensions.FindMatchingPath (expandString (file));
		}

		#endregion

		internal IEnumerable<ReservedProjectProperty> GetReservedProperties (Toolset toolset, Project project)
		{
			Func<string,Func<string>,ReservedProjectProperty> create = (name, value) => new ReservedProjectProperty (project, name, value);
			return GetReservedProperties<ReservedProjectProperty> (toolset, project.Xml, create, () => project.FullPath);
		}
		
		internal IEnumerable<ProjectPropertyInstance> GetReservedProperties (Toolset toolset, ProjectInstance project, ProjectRootElement xml)
		{
			Func<string,Func<string>,ProjectPropertyInstance> create = (name, value) => new ProjectPropertyInstance (name, true, null, value);
			return GetReservedProperties<ProjectPropertyInstance> (toolset, xml, create, () => project.FullPath);
		}
		
		// seealso http://msdn.microsoft.com/en-us/library/ms164309.aspx
		IEnumerable<T> GetReservedProperties<T> (Toolset toolset, ProjectRootElement project, Func<string,Func<string>,T> create, Func<string> projectFullPath)
		{
			yield return create ("MSBuildBinPath", () => toolset.ToolsPath);
			// FIXME: add MSBuildLastTaskResult
			// FIXME: add MSBuildNodeCount
			// FIXME: add MSBuildProgramFiles32
			yield return create ("MSBuildProjectDefaultTargets", () => project.DefaultTargets);
			yield return create ("MSBuildProjectDirectory", () => project.DirectoryPath + Path.DirectorySeparatorChar);
			yield return create ("MSBuildProjectDirectoryNoRoot", () => project.DirectoryPath.Substring (Path.GetPathRoot (project.DirectoryPath).Length));
			yield return create ("MSBuildProjectExtension", () => Path.GetExtension (project.FullPath));
			yield return create ("MSBuildProjectFile", () => Path.GetFileName (project.FullPath));
			yield return create ("MSBuildProjectFullPath", () => project.FullPath);
			yield return create ("MSBuildProjectName", () => Path.GetFileNameWithoutExtension (project.FullPath));
			yield return create ("MSBuildStartupDirectory", () => BuildStartupDirectory);
			yield return create ("MSBuildThisFile", () => Path.GetFileName (GetEvaluationTimeThisFile (projectFullPath)));
			yield return create ("MSBuildThisFileFullPath", () => GetEvaluationTimeThisFile (projectFullPath));
			yield return create ("MSBuildThisFileName", () => Path.GetFileNameWithoutExtension (GetEvaluationTimeThisFile (projectFullPath)));
			yield return create ("MSBuildThisFileExtension", () => Path.GetExtension (GetEvaluationTimeThisFile (projectFullPath)));

			yield return create ("MSBuildThisFileDirectory", () => Path.GetDirectoryName (GetEvaluationTimeThisFileDirectory (projectFullPath)));
			yield return create ("MSBuildThisFileDirectoryNoRoot", () => {
				string dir = GetEvaluationTimeThisFileDirectory (projectFullPath) + Path.DirectorySeparatorChar;
				return dir.Substring (Path.GetPathRoot (dir).Length);
				});
			yield return create ("MSBuildToolsPath", () => toolset.ToolsPath);
			yield return create ("MSBuildToolsVersion", () => toolset.ToolsVersion);

			// This is an implementation specific special property for this Microsoft.Build.dll to differentiate
			// the build from Microsoft.Build.Engine.dll. It is significantly used in some *.targets file we share
			// between old and new build engine.
			yield return create ("MonoUseMicrosoftBuildDll", () => "True");
		}
		
		// These are required for reserved property, represents dynamically changing property values.
		// This should resolve to either the project file path or that of the imported file.
		internal string GetEvaluationTimeThisFileDirectory (Func<string> nonImportingTimeFullPath)
		{
			var file = GetEvaluationTimeThisFile (nonImportingTimeFullPath);
			var dir = Path.IsPathRooted (file) ? Path.GetDirectoryName (file) : Directory.GetCurrentDirectory ();
			return dir + Path.DirectorySeparatorChar;
		}

		internal string GetEvaluationTimeThisFile (Func<string> nonImportingTimeFullPath)
		{
			return OngoingImports.Count > 0 ? OngoingImports.Peek () : (nonImportingTimeFullPath () ?? string.Empty);
		}
		
		static readonly char [] item_target_sep = {';'};
		
		internal static IEnumerable<T> GetAllItems<T> (Func<string,string> expandString, string include, string exclude, Func<string,T> creator, Func<string,ITaskItem> taskItemCreator, string directory, Action<T,string> assignRecurse, Func<ITaskItem,bool> isDuplicate)
		{
			var includes = expandString (include).Trim ().Split (item_target_sep, StringSplitOptions.RemoveEmptyEntries);
			var excludes = expandString (exclude).Trim ().Split (item_target_sep, StringSplitOptions.RemoveEmptyEntries);
			
			if (includes.Length == 0)
				yield break;
			if (includes.Length == 1 && includes [0].IndexOf ('*') < 0 && excludes.Length == 0) {
				// for most case - shortcut.
				var item = creator (includes [0]);
				yield return item;
			} else {
				var ds = new Microsoft.Build.BuildEngine.DirectoryScanner () {
					BaseDirectory = new DirectoryInfo (directory),
					Includes = includes.Where (s => !string.IsNullOrWhiteSpace (s)).Select (i => taskItemCreator (i)).ToArray (),
					Excludes = excludes.Where (s => !string.IsNullOrWhiteSpace (s)).Select (e => taskItemCreator (e)).ToArray (),
				};
				ds.Scan ();
				foreach (var taskItem in ds.MatchedItems) {
					if (isDuplicate (taskItem))
						continue; // skip duplicate
					var item = creator (taskItem.ItemSpec);
					string recurse = taskItem.GetMetadata ("RecursiveDir");
					assignRecurse (item, recurse);
					yield return item;
				}
			}
		}
		
		static readonly char [] path_sep = {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};
		
		internal static string GetWellKnownMetadata (string name, string file, Func<string,string> getFullPath, string recursiveDir)
		{
			switch (name.ToLower (CultureInfo.InvariantCulture)) {
			case "fullpath":
				return getFullPath (file);
			case "rootdir":
				return Path.GetPathRoot (getFullPath (file));
			case "filename":
				return Path.GetFileNameWithoutExtension (file);
			case "extension":
				return Path.GetExtension (file);
			case "relativedir":
					var idx = file.LastIndexOfAny (path_sep);
					return idx < 0 ? string.Empty : file.Substring (0, idx + 1);
			case "directory":
					var fp = getFullPath (file);
					return Path.GetDirectoryName (fp).Substring (Path.GetPathRoot (fp).Length);
			case "recursivedir":
				return recursiveDir;
			case "identity":
				return file;
			case "modifiedtime":
				return new FileInfo (getFullPath (file)).LastWriteTime.ToString ("yyyy-MM-dd HH:mm:ss.fffffff");
			case "createdtime":
				return new FileInfo (getFullPath (file)).CreationTime.ToString ("yyyy-MM-dd HH:mm:ss.fffffff");
			case "accessedtime":
				return new FileInfo (getFullPath (file)).LastAccessTime.ToString ("yyyy-MM-dd HH:mm:ss.fffffff");
			}
			return null;
		}
	}
}
