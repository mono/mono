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
		BuildPropertyGroup	environmentProperties;
		EventSource		eventSource;
		bool			buildStarted;
		BuildPropertyGroup	globalProperties;
		IDictionary		importedProjects;
		IList			loggers;
		bool			onlyLogCriticalEvents;
		IDictionary		projects;
		BuildPropertyGroup	reservedProperties;

		// FIXME: GlobalEngine static property uses this but what about GlobalEngineAccessor?
		static Engine		globalEngine;
		static Version		version;

		static Engine ()
		{
			version = new Version("0.1");
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
			this.projects = new Hashtable ();
			this.eventSource = new EventSource ();
			this.loggers = new ArrayList ();
			this.buildStarted = false;
			this.LoadEnvironmentProperties ();
			this.reservedProperties = new BuildPropertyGroup ();
			this.reservedProperties.AddNewProperty ("MSBuildBinPath", binPath, PropertyType.Reserved);
		}

		public bool BuildProject (Project project,
					  string[] targetNames,
					  IDictionary targetOutputs)
		{
			bool result;
			
			LogProjectStarted (project, targetNames);
				
			result =  project.Build (targetNames, targetOutputs);
			
			LogProjectFinished (project, result);
			
			return result;
		}

		public bool BuildProjectFile (string projectFileName,
					  string[] targetNames,
					  BuildPropertyGroup globalPropertiesToUse,
					  IDictionary targetOutputs)
		{
			bool result;
			Project project;
			
			if (projects.Contains (projectFileName)) {
				project = (Project) projects [projectFileName];
				LogProjectStarted (project, targetNames);
				result = project.Build (targetNames, targetOutputs);
			}
			else
				return false;
			
			LogProjectFinished (project, result);
			
			return result;
		}

		public Project CreateNewProject ()
		{
			if (buildStarted == false) {
				LogBuildStarted ();
				buildStarted = true;
			}
			Project p = new Project (this);
			p.EnvironmentProperties = this.environmentProperties;
			p.ReservedProperties = this.reservedProperties;
			if (globalProperties != null) {
				BuildPropertyGroup bpg = new BuildPropertyGroup ();
				foreach (BuildProperty bp in globalProperties)
					bpg.AddNewProperty (bp.Name, bp.Value, PropertyType.CommandLine);
				p.GlobalProperties = bpg;
			}
			return p;
		}

		public Project GetLoadedProject (string projectFullFileName)
		{
			return (Project) projects [projectFullFileName];
		}

		public void RegisterLogger (ILogger logger)
		{
			if (logger == null)
				throw new ArgumentNullException ("logger");
			logger.Initialize (eventSource);
			loggers.Add (logger);
		}
		
		[MonoTODO]
		public void UnloadAllProjects ()
		{
		}
		
		[MonoTODO]
		public void UnloadProject (Project project)
		{
		}

		public void UnregisterAllLoggers ()
		{
			// FIXME: check if build succeeded
			LogBuildFinished (true);
			foreach (ILogger i in loggers) {
				i.Shutdown ();
			}
			loggers.Clear ();
		}
		
		private void LoadEnvironmentProperties ()
		{
			environmentProperties = new BuildPropertyGroup ();
			IDictionary environment = Environment.GetEnvironmentVariables ();
			foreach (DictionaryEntry de in environment) {
				environmentProperties.AddNewProperty ((string) de.Key, (string) de.Value, PropertyType.Environment);
			}
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
			get { return globalEngine; }
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
