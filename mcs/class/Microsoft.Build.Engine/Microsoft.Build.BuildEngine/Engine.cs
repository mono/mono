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
using Microsoft.Build.Framework;

namespace Microsoft.Build.BuildEngine {
	public class Engine {
		
		string			binPath;
		bool			buildEnabled;
		EventSource		eventSource;
		bool			buildStarted;
		BuildPropertyGroup	globalProperties;
		IDictionary		importedProjects;
		IList			loggers;
		bool			onlyLogCriticalEvents;
		IDictionary		projects;

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
			this.projects = new Hashtable ();
			this.eventSource = new EventSource ();
			this.loggers = new ArrayList ();
			this.buildStarted = false;
			this.globalProperties = new BuildPropertyGroup ();
		}
		
		[MonoTODO]
		public bool BuildProject (Project project)
		{
			return project.Build ();
		}
		
		[MonoTODO]
		public bool BuildProject (Project project, string targetName)
		{
			return BuildProject (project, new string[] { targetName}, new Hashtable (), BuildSettings.None);
		}
		
		[MonoTODO]
		public bool BuildProject (Project project, string[] targetNames)
		{
			return BuildProject (project, targetNames, new Hashtable (), BuildSettings.None);
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
			bool result;
			
			LogProjectStarted (project, targetNames);
				
			result =  project.Build (targetNames, targetOutputs);
			
			LogProjectFinished (project, result);
			
			return result;
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
			return BuildProjectFile (projectFile, targetNames, globalProperties, new Hashtable (), BuildSettings.None);
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
			bool result;
			Project project;
			
			if (projects.Contains (projectFile)) {
				project = (Project) projects [projectFile];
				LogProjectStarted (project, targetNames);
				result = project.Build (targetNames, targetOutputs);
			}
			else
				return false;
			
			LogProjectFinished (project, result);
			
			return result;
		}

		private void CheckBinPath ()
		{
			if (BinPath == null) {
				throw new InvalidOperationException ("Before a project can be instantiated, " +
					"Engine.BinPath must be set to the location on disk where MSBuild " + 
					"is installed. This is used to evaluate $(MSBuildBinPath).");
			}
		}

		public Project CreateNewProject ()
		{
			CheckBinPath ();
			return new Project (this);
		}

		public Project GetLoadedProject (string projectFullFileName)
		{
			if (projectFullFileName == null)
				throw new ArgumentNullException ("projectFullFileName");
			
			return (Project) projects [projectFullFileName];
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
			if (project.ParentEngine != this)
				throw new InvalidOperationException ("This project is not loaded in this engine");
			
			project.CheckUnloaded ();
			
			if (project.FullFileName != String.Empty)
				projects.Remove (project.FullFileName);
			
			project.Unload ();
		}

		public void UnloadAllProjects ()
		{
			foreach (DictionaryEntry e in projects)
				UnloadProject ((Project) e.Value);
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
			LogBuildFinished (true);
			foreach (ILogger i in loggers) {
				i.Shutdown ();
			}
			loggers.Clear ();
		}
		
		private void LogProjectStarted (Project project, string[] targetNames)
		{
			ProjectStartedEventArgs psea;
			if (targetNames.Length == 0) {
				if (project.DefaultTargets != String.Empty)
					psea = new ProjectStartedEventArgs ("Project started.", null, project.FullFileName,
						project.DefaultTargets, null, null);
				else
					psea = new ProjectStartedEventArgs ("Project started.", null, project.FullFileName, "default", null, null);
			} else
			psea = new ProjectStartedEventArgs ("Project started.", null, project.FullFileName, String.Join (";",
				targetNames), null, null);
			eventSource.FireProjectStarted (this, psea);
		}
		
		private void LogProjectFinished (Project project, bool succeeded)
		{
			ProjectFinishedEventArgs pfea;
			pfea = new ProjectFinishedEventArgs ("Project started.", null, project.FullFileName, succeeded);
			eventSource.FireProjectFinished (this, pfea);
		}
		
		private void LogBuildStarted ()
		{
			BuildStartedEventArgs bsea;
			bsea = new BuildStartedEventArgs ("Build started.", null);
			eventSource.FireBuildStarted (this, bsea);
		}
		
		private void LogBuildFinished (bool succeeded)
		{
			BuildFinishedEventArgs bfea;
			bfea = new BuildFinishedEventArgs ("Build finished.", null, succeeded);
			eventSource.FireBuildFinished (this, bfea);
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
	}
}

#endif
