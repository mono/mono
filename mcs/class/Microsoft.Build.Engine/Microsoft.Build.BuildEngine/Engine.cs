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
		BuildPropertyGroup	globalProperties;
		//IDictionary		importedProjects;
		List <ILogger>		loggers;
		//bool			onlyLogCriticalEvents;
		Dictionary <string, Project>	projects;
		Dictionary <string, ITaskItem[]> builtTargetsOutputByName;

		static Engine		globalEngine;
		static Version		version;

		static Engine ()
		{
			version = new Version ("0.1");
		}
		
		public Engine ()
			: this (null)
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
			this.globalProperties = new BuildPropertyGroup ();
			this.builtTargetsOutputByName = new Dictionary<string, ITaskItem[]> ();
			
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
		
		[MonoTODO ("use buildFlags")]
		public bool BuildProject (Project project,
					  string[] targetNames,
					  IDictionary targetOutputs,
					  BuildSettings buildFlags)
		{
			if (project == null)
				throw new ArgumentException ("project");
			if (targetNames == null)
				return false;

			StartBuild ();
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
		
		[MonoTODO ("use buildFlags")]
		public bool BuildProjectFile (string projectFile,
					      string[] targetNames,
					      BuildPropertyGroup globalProperties,
					      IDictionary targetOutputs,
					      BuildSettings buildFlags)
		{
			Project project;

			StartBuild ();
			
			if (projects.ContainsKey (projectFile)) {
				project = (Project) projects [projectFile];
			} else {
				project = CreateNewProject ();
				project.Load (projectFile);
			}

			project.GlobalProperties = globalProperties;
			return project.Build (targetNames, targetOutputs, buildFlags);
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
			foreach (KeyValuePair <string, Project> e in projects)
				UnloadProject (e.Value);
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
			LogBuildFinished (true);
			foreach (ILogger i in loggers) {
				i.Shutdown ();
			}
			loggers.Clear ();
		}

		internal void StartBuild ()
		{
			if (!buildStarted) {
				LogBuildStarted ();
				buildStarted = true;
			}
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
			get { return globalProperties; }
			set { globalProperties = value; }
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
