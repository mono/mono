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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.XBuild.Utilities;

namespace Microsoft.Build.BuildEngine {
	public class Engine {
		
		string			binPath;
		bool			buildEnabled;
		TaskDatabase		defaultTasks;
		bool			defaultTasksRegistered;
		const string		defaultTasksProjectName = "Microsoft.Common.tasks";
		EventSource		eventSource;
		bool			buildStarted;
		BuildPropertyGroup	global_properties;
		//IDictionary		importedProjects;
		List <ILogger>		loggers;
		//bool			onlyLogCriticalEvents;
		Dictionary <string, Project>	projects;

		// the key here represents the project+target+global_properties set
		Dictionary <string, ITaskItem[]> builtTargetsOutputByName;
		Stack<Project> currentlyBuildingProjectsStack;
		
		public bool BuildSuccess{ get; set; }

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
			this.BuildSuccess = true;
			
			RegisterDefaultTasks ();
		}
		
		[MonoTODO]
		public bool BuildProject (Project project)
		{
			if (project == null)
				throw new ArgumentException ("project");
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

			return project.Build (targetNames, targetOutputs, buildFlags);
		}

		[MonoTODO]
		public bool BuildProjectFile (string projectFile)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool BuildProjectFile (string projectFile,
					      string targetName)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool BuildProjectFile (string projectFile,
					      string[] targetNames)
		{
			throw new NotImplementedException ();
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
			Project project;

			if (projects.ContainsKey (projectFile)) {
				project = (Project) projects [projectFile];
			} else {
				project = CreateNewProject ();
				project.Load (projectFile);
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
				project.NeedToReevaluate ();
			}

			try {
				return project.Build (targetNames, targetOutputs, buildFlags);
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
			if (defaultTasksRegistered)
				CheckBinPath ();
			return new Project (this);
		}

		public Project GetLoadedProject (string projectFullFileName)
		{
			if (projectFullFileName == null)
				throw new ArgumentNullException ("projectFullFileName");
			
			// FIXME: test it
			return projects [projectFullFileName];
		}

		internal void RemoveLoadedProject (Project p)
		{
			if (p.FullFileName != String.Empty)
				projects.Remove (p.FullFileName);
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
			
			if (project.FullFileName != String.Empty)
				projects.Remove (project.FullFileName);
			
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

		internal void StartProjectBuild (Project project, string [] target_names)
		{
			if (!buildStarted) {
				LogBuildStarted ();
				buildStarted = true;
			}

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
							
			BuildSuccess = BuildSuccess && succeeded;

			if (currentlyBuildingProjectsStack.Count == 0 ||
				String.Compare (top_project.FullFileName, currentlyBuildingProjectsStack.Peek ().FullFileName) != 0)
				LogProjectFinished (top_project, succeeded);

			if (currentlyBuildingProjectsStack.Count == 0) {
				LogBuildFinished (succeeded);
				buildStarted = false;
			}
		}

		void LogProjectStarted (Project project, string [] target_names)
		{
			ProjectStartedEventArgs psea;
			if (target_names == null || target_names.Length == 0)
				psea = new ProjectStartedEventArgs ("Project started.", null, project.FullFileName,
						String.Empty, null, null);
			else
				psea = new ProjectStartedEventArgs ("Project started.", null, project.FullFileName,
						String.Join (";", target_names), null, null);
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
		
		void RegisterDefaultTasks ()
		{
			this.defaultTasksRegistered = false;
			
			Project defaultTasksProject = CreateNewProject ();
			
			if (binPath != null) {
				if (File.Exists (Path.Combine (binPath, defaultTasksProjectName))) {
					defaultTasksProject.Load (Path.Combine (binPath, defaultTasksProjectName));
					defaultTasks = defaultTasksProject.TaskDatabase;
				} else
					defaultTasks = new TaskDatabase ();
			} else
				defaultTasks = new TaskDatabase ();
			
			this.defaultTasksRegistered = true;
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

		public bool OnlyLogCriticalEvents {
			get { return eventSource.OnlyLogCriticalEvents; }
			set { eventSource.OnlyLogCriticalEvents = value; }
		}
		
		internal EventSource EventSource {
			get { return eventSource; }
		}
		
		internal bool DefaultTasksRegistered {
			get { return defaultTasksRegistered; }
		}
		
		internal TaskDatabase DefaultTasks {
			get { return defaultTasks; }
		}

		internal Dictionary<string, ITaskItem[]> BuiltTargetsOutputByName {
			get { return builtTargetsOutputByName; }
		}
	}
}

#endif
