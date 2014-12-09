//
// Engine.cs: Main engine of XBuild.
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.XBuild.Utilities;

namespace Microsoft.Build.BuildEngine {
	public class Engine {
		
		string			binPath;
		bool			buildEnabled;
		Dictionary<string, TaskDatabase> defaultTasksTableByToolsVersion;
		const string		defaultTasksProjectName = "Microsoft.Common.tasks";
		EventSource		eventSource;
		bool			buildStarted;
		//ToolsetDefinitionLocations toolsetLocations;
		BuildPropertyGroup	global_properties;
		//IDictionary		importedProjects;
		List <ILogger>		loggers;
		//bool			onlyLogCriticalEvents;
		Dictionary <string, Project>	projects;
		string defaultToolsVersion;

		// the key here represents the project+target+global_properties set
		Dictionary <string, ITaskItem[]> builtTargetsOutputByName;
		Stack<Project> currentlyBuildingProjectsStack;

		static Engine		globalEngine;
		static Version		version;

		static Engine ()
		{
			version = new Version ("0.1");
		}
		
		public Engine ()
			: this (ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version20))
		{
		}

		public Engine (ToolsetDefinitionLocations locations)
			: this (ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version20))
		{
			//toolsetLocations = locations;
		}
		
		public Engine (BuildPropertyGroup globalProperties)
			: this (ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version20))
		{
			this.global_properties = globalProperties;
		}

		public Engine (BuildPropertyGroup globalProperties, ToolsetDefinitionLocations locations)
			: this (ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version20))
		{
			this.global_properties = globalProperties;
			//toolsetLocations = locations;
		}

		// engine should be invoked with path where binary files are
		// to find microsoft.build.tasks
		public Engine (string binPath)
		{
			this.binPath = binPath;
			this.buildEnabled = true;
			this.projects = new Dictionary <string, Project> ();
			this.eventSource = new EventSource ();
			this.loggers = new List <ILogger> ();
			this.buildStarted = false;
			this.global_properties = new BuildPropertyGroup ();
			this.builtTargetsOutputByName = new Dictionary<string, ITaskItem[]> ();
			this.currentlyBuildingProjectsStack = new Stack<Project> ();
			this.Toolsets = new ToolsetCollection ();
			LoadDefaultToolsets ();
			defaultTasksTableByToolsVersion = new Dictionary<string, TaskDatabase> ();
		}

		//FIXME: should be loaded from config file
		void LoadDefaultToolsets ()
		{
			Toolsets.Add (new Toolset ("2.0",
						ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version20)));
#if NET_3_5
			Toolsets.Add (new Toolset ("3.0",
						ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version30)));
			Toolsets.Add (new Toolset ("3.5",
						ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version35)));
#endif
#if NET_4_0
			Toolsets.Add (new Toolset ("4.0",
						ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version40)));
#endif
#if XBUILD_12
			Toolsets.Add (new Toolset ("12.0", ToolLocationHelper.GetPathToBuildTools ("12.0")));
#endif
		}
		
		[MonoTODO]
		public bool BuildProject (Project project)
		{
			if (project == null)
				throw new ArgumentException ("project");
			builtTargetsOutputByName.Clear ();
			return project.Build ();
		}
		
		[MonoTODO]
		public bool BuildProject (Project project, string targetName)
		{
			if (project == null)
				throw new ArgumentException ("project");
			if (targetName == null)
				return false;

			return BuildProject (project, new string[] { targetName}, null, BuildSettings.None);
		}
		
		[MonoTODO]
		public bool BuildProject (Project project, string[] targetNames)
		{
			return BuildProject (project, targetNames, null, BuildSettings.None);
		}

		[MonoTODO]
		public bool BuildProject (Project project,
					  string[] targetNames,
					  IDictionary targetOutputs)
		{
			return BuildProject (project, targetNames, targetOutputs, BuildSettings.None);
		}
		
		public bool BuildProject (Project project,
					  string[] targetNames,
					  IDictionary targetOutputs,
					  BuildSettings buildFlags)
		{
			if (project == null)
				throw new ArgumentException ("project");
			if (targetNames == null)
				return false;

			if ((buildFlags & BuildSettings.DoNotResetPreviouslyBuiltTargets) != BuildSettings.DoNotResetPreviouslyBuiltTargets)
				builtTargetsOutputByName.Clear ();

			if (defaultToolsVersion != null)
				// it has been explicitly set, xbuild does this..
				project.ToolsVersion = defaultToolsVersion;
			return project.Build (targetNames, targetOutputs, buildFlags);
		}

		[MonoTODO]
		public bool BuildProjectFile (string projectFile)
		{
			return BuildProjectFile (projectFile, new string [0]);
		}
		
		[MonoTODO]
		public bool BuildProjectFile (string projectFile,
					      string targetName)
		{
			return BuildProjectFile (projectFile,
			                         targetName == null ? new string [0] : new string [] {targetName});
		}
		
		[MonoTODO]
		public bool BuildProjectFile (string projectFile,
					      string[] targetNames)
		{
			return BuildProjectFile (projectFile, targetNames, null);
		}
		
		[MonoTODO]
		public bool BuildProjectFile (string projectFile,
					      string[] targetNames,
					      BuildPropertyGroup globalProperties)
		{
			return BuildProjectFile (projectFile, targetNames, globalProperties, null, BuildSettings.None);
		}
		
		[MonoTODO]
		public bool BuildProjectFile (string projectFile,
					      string[] targetNames,
					      BuildPropertyGroup globalProperties,
					      IDictionary targetOutputs)
		{
			return BuildProjectFile (projectFile, targetNames, globalProperties, targetOutputs, BuildSettings.None);
		}
		
		public bool BuildProjectFile (string projectFile,
					      string[] targetNames,
					      BuildPropertyGroup globalProperties,
					      IDictionary targetOutputs,
					      BuildSettings buildFlags)
		{
			return BuildProjectFile (projectFile, targetNames, globalProperties, targetOutputs, buildFlags, null);
		}
			
		//FIXME: add a test for null @toolsVersion
		public bool BuildProjectFile (string projectFile,
					      string[] targetNames,
					      BuildPropertyGroup globalProperties,
					      IDictionary targetOutputs,
					      BuildSettings buildFlags, string toolsVersion)
		{
			bool result = false;
			try {
				StartEngineBuild ();
				result = BuildProjectFileInternal (projectFile, targetNames, globalProperties, targetOutputs, buildFlags, toolsVersion);
				return result;
			} catch (InvalidProjectFileException ie) {
				this.LogErrorWithFilename (projectFile, ie.Message);
				this.LogMessage (MessageImportance.Low, String.Format ("{0}: {1}", projectFile, ie.ToString ()));
				return false;
			} catch (Exception e) {
				if (buildStarted) {
					this.LogErrorWithFilename (projectFile, e.Message);
					this.LogMessage (MessageImportance.Low, String.Format ("{0}: {1}", projectFile, e.ToString ()));
				}
				throw;
			} finally {
				EndEngineBuild (result);
			}
		}

		bool BuildProjectFileInternal (string projectFile,
					      string[] targetNames,
					      BuildPropertyGroup globalProperties,
					      IDictionary targetOutputs,
					      BuildSettings buildFlags, string toolsVersion)
		{

			if ((buildFlags & BuildSettings.DoNotResetPreviouslyBuiltTargets) != BuildSettings.DoNotResetPreviouslyBuiltTargets)
				builtTargetsOutputByName.Clear ();

			Project project;

			bool newProject = false;
			if (!projects.TryGetValue (projectFile, out project)) {
				project = CreateNewProject ();
				newProject = true;
			}

			BuildPropertyGroup engine_old_grp = null;
			BuildPropertyGroup project_old_grp = null;
			if (globalProperties != null) {
				engine_old_grp = GlobalProperties.Clone (true);
				project_old_grp = project.GlobalProperties.Clone (true);

				// Override project's global properties with the
				// ones explicitlcur_y specified here
				foreach (BuildProperty bp in globalProperties)
					project.GlobalProperties.AddProperty (bp);

				if (!newProject)
					project.NeedToReevaluate ();
			}

			if (newProject)
				project.Load (projectFile);

			try {
				string oldProjectToolsVersion = project.ToolsVersion;
				if (String.IsNullOrEmpty (toolsVersion) && defaultToolsVersion != null)
					// no tv specified, let the project inherit it from the
					// engine. 'defaultToolsVersion' will be effective only
					// it has been overridden. Otherwise, the project's own
					// tv will be used.
					project.ToolsVersion = defaultToolsVersion;
				else
					project.ToolsVersion = toolsVersion;

				try {
					return project.Build (targetNames, targetOutputs, buildFlags);
				} finally {
					project.ToolsVersion = oldProjectToolsVersion;
				}
			} finally {
				if (globalProperties != null) {
					GlobalProperties = engine_old_grp;
					project.GlobalProperties = project_old_grp;
				}
			}
		}

		void CheckBinPath ()
		{
			if (BinPath == null) {
				throw new InvalidOperationException ("Before a project can be instantiated, " +
					"Engine.BinPath must be set to the location on disk where MSBuild " + 
					"is installed. This is used to evaluate $(MSBuildBinPath).");
			}
		}

		public Project CreateNewProject ()
		{
			return new Project (this);
		}

		public Project GetLoadedProject (string projectFullFileName)
		{
			if (projectFullFileName == null)
				throw new ArgumentNullException ("projectFullFileName");
			
			Project project;
			projects.TryGetValue (projectFullFileName, out project);

			return project;
		}

		internal void RemoveLoadedProject (Project p)
		{
			if (!String.IsNullOrEmpty (p.FullFileName)) {
				ClearBuiltTargetsForProject (p);
				projects.Remove (p.FullFileName);
			}
		}

		internal void AddLoadedProject (Project p)
		{
			if (p.FullFileName != String.Empty)
				projects.Add (p.FullFileName, p);
		}
	
		public void UnloadProject (Project project)
		{
			if (project == null)
				throw new ArgumentNullException ("project");

			if (project.ParentEngine != this)
				throw new InvalidOperationException ("The \"Project\" object specified does not belong to the correct \"Engine\" object.");
			
			project.CheckUnloaded ();
			
			RemoveLoadedProject (project);
			
			project.Unload ();
		}

		public void UnloadAllProjects ()
		{
			IList<Project> values = new List<Project> (projects.Values);
			foreach (Project p in values)
				UnloadProject (p);
		}

		[MonoTODO]
		public void RegisterLogger (ILogger logger)
		{
			if (logger == null)
				throw new ArgumentNullException ("logger");
			
			logger.Initialize (eventSource);
			loggers.Add (logger);
		}
		
		[MonoTODO]
		public void UnregisterAllLoggers ()
		{
			// FIXME: check if build succeeded
			// FIXME: it shouldn't be here
			if (buildStarted)
				LogBuildFinished (true);
			foreach (ILogger i in loggers) {
				i.Shutdown ();
			}
			loggers.Clear ();
		}

		void StartEngineBuild ()
		{
			if (!buildStarted) {
				LogBuildStarted ();
				buildStarted = true;
			}
		}

		void EndEngineBuild (bool succeeded)
		{
			if (buildStarted && currentlyBuildingProjectsStack.Count == 0) {
				LogBuildFinished (succeeded);
				buildStarted = false;
			}
		}

		internal void StartProjectBuild (Project project, string [] target_names)
		{
			StartEngineBuild ();

			if (currentlyBuildingProjectsStack.Count == 0 ||
				String.Compare (currentlyBuildingProjectsStack.Peek ().FullFileName, project.FullFileName) != 0)
					LogProjectStarted (project, target_names);

			currentlyBuildingProjectsStack.Push (project);
		}

		internal void EndProjectBuild (Project project, bool succeeded)
		{
			if (!buildStarted)
				throw new Exception ("build isnt started currently");

			Project top_project = currentlyBuildingProjectsStack.Pop ();

			if (String.Compare (project.FullFileName, top_project.FullFileName) != 0)
				throw new Exception (String.Format (
							"INTERNAL ERROR: Project finishing is not the same as the one on top " +
							"of the stack. Project: {0} Top of stack: {1}",
							project.FullFileName, top_project.FullFileName));

			if (currentlyBuildingProjectsStack.Count == 0 ||
				String.Compare (top_project.FullFileName, currentlyBuildingProjectsStack.Peek ().FullFileName) != 0)
				LogProjectFinished (top_project, succeeded);

			EndEngineBuild (succeeded);
		}

		internal void ClearBuiltTargetsForProject (Project project)
		{
			string project_key = project.GetKeyForTarget (String.Empty, false);
			var to_remove_keys = BuiltTargetsOutputByName.Keys.Where (key => key.StartsWith (project_key)).ToList ();
			foreach (string to_remove_key in to_remove_keys)
				BuiltTargetsOutputByName.Remove (to_remove_key);
		}

		void LogProjectStarted (Project project, string [] target_names)
		{
			string targets;
			if (target_names == null || target_names.Length == 0)
				targets = String.Empty;
			else
				targets = String.Join (";", target_names);

			ProjectStartedEventArgs psea = new ProjectStartedEventArgs ("Project started.", null, project.FullFileName, targets,
					project.EvaluatedPropertiesAsDictionaryEntries, project.EvaluatedItemsByNameAsDictionaryEntries);

			eventSource.FireProjectStarted (this, psea);
		}

		void LogProjectFinished (Project project, bool succeeded)
		{
			ProjectFinishedEventArgs pfea;
			pfea = new ProjectFinishedEventArgs ("Project started.", null, project.FullFileName, succeeded);
			eventSource.FireProjectFinished (this, pfea);
		}

		void LogBuildStarted ()
		{
			BuildStartedEventArgs bsea;
			bsea = new BuildStartedEventArgs ("Build started.", null);
			eventSource.FireBuildStarted (this, bsea);
		}
		
		void LogBuildFinished (bool succeeded)
		{
			BuildFinishedEventArgs bfea;
			bfea = new BuildFinishedEventArgs ("Build finished.", null, succeeded);
			eventSource.FireBuildFinished (this, bfea);
		}

		internal TaskDatabase GetDefaultTasks (string toolsVersion)
		{
			TaskDatabase db;
			if (defaultTasksTableByToolsVersion.TryGetValue (toolsVersion, out db))
				return db;

			var toolset = Toolsets [toolsVersion];
			if (toolset == null)
				throw new UnknownToolsVersionException (toolsVersion);

			string toolsPath = toolset.ToolsPath;
			string tasksFile = Path.Combine (toolsPath, defaultTasksProjectName);
			this.LogMessage (MessageImportance.Low, "Loading default tasks for ToolsVersion: {0} from {1}", toolsVersion, tasksFile);

			// set a empty taskdb here, because the project loading the tasks
			// file will try to get the default task db
			defaultTasksTableByToolsVersion [toolsVersion] = new TaskDatabase ();

			db = defaultTasksTableByToolsVersion [toolsVersion] = RegisterDefaultTasks (tasksFile);

			return db;
		}
		
		TaskDatabase RegisterDefaultTasks (string tasksFile)
		{
			Project defaultTasksProject = CreateNewProject ();
			TaskDatabase db;
			
			if (File.Exists (tasksFile)) {
				defaultTasksProject.Load (tasksFile);
				db = defaultTasksProject.TaskDatabase;
			} else {
				this.LogWarning ("Default tasks file {0} not found, ignoring.", tasksFile);
				db = new TaskDatabase ();
			}

			return db;
		}

		public string BinPath {
			get { return binPath; }
			set { binPath = value; }
		}

		public bool BuildEnabled {
			get { return buildEnabled; }
			set { buildEnabled = value; }
		}

		public static Version Version {
			get { return version; }
		}

		public static Engine GlobalEngine {
			get {
				if (globalEngine == null)
					globalEngine = new Engine ();
				return globalEngine;
			}
		}

		public BuildPropertyGroup GlobalProperties {
			get { return global_properties; }
			set { global_properties = value; }
		}
		
		public ToolsetCollection Toolsets {
			get; private set;
		}

		public string DefaultToolsVersion {
			get {
				// This is used as the fall back version if the
				// project can't find a version to use
				return String.IsNullOrEmpty (defaultToolsVersion)
						?
#if NET_4_0						
						 "4.0"
#else
						"2.0"
#endif						
						: defaultToolsVersion;
			}
			set {
				if (Toolsets [value] == null)
					throw new UnknownToolsVersionException (value);
				defaultToolsVersion = value;
			}
		}
		
		public bool IsBuilding {
			get { return buildStarted; }
		}
		
		public bool OnlyLogCriticalEvents {
			get { return eventSource.OnlyLogCriticalEvents; }
			set { eventSource.OnlyLogCriticalEvents = value; }
		}
		
		internal EventSource EventSource {
			get { return eventSource; }
		}
		
		internal Dictionary<string, ITaskItem[]> BuiltTargetsOutputByName {
			get { return builtTargetsOutputByName; }
		}
	}
}
