//
// BuildEngine4.cs
//
// Author:
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// Copyright (C) 2013 Xamarin Inc. (http://www.xamarin.com)
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
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Evaluation;
using System.Linq;
using System.IO;

namespace Microsoft.Build.Internal
{
	class BuildEngine4 : IBuildEngine4
	{
		public BuildEngine4 (BuildSubmission submission)
		{
			this.submission = submission;
			event_source = new EventSource ();
			if (submission.BuildManager.OngoingBuildParameters.Loggers != null)
				foreach (var l in submission.BuildManager.OngoingBuildParameters.Loggers)
					l.Initialize (event_source);
		}

		BuildSubmission submission;
		ProjectInstance project;
		ProjectTaskInstance current_task;
		EventSource event_source;
		
		public ProjectCollection Projects {
			get { return submission.BuildManager.OngoingBuildParameters.ProjectCollection; }
		}

		// FIXME:
		// While we are not faced to implement those features, there are some modern task execution requirements.
		//
		// This will have to be available for "out of process" nodes (see NodeAffinity).
		// NodeAffinity is set per project file at BuildManager.HostServices.
		// When NodeAffinity is set to OutOfProc, it should probably launch different build host
		// that runs separate build tasks. (.NET has MSBuildTaskHost.exe which I guess is about that.)
		//
		// Also note that the complete implementation has to support LoadInSeparateAppDomainAttribute
		// (which is most likely derived from AppDomainIsolatedBuildTask) that marks a task to run
		// in separate AppDomain.
		//
		public void BuildProject (Func<bool> checkCancel, BuildResult result, ProjectInstance project, IEnumerable<string> targetNames, IDictionary<string,string> globalProperties, IDictionary<string,string> targetOutputs, string toolsVersion)
		{
			var request = submission.BuildRequest;
			var parameters = submission.BuildManager.OngoingBuildParameters;
			this.project = project;
			var buildTaskFactory = new BuildTaskFactory (BuildTaskDatabase.GetDefaultTaskDatabase (parameters.GetToolset (toolsVersion)), submission.BuildRequest.ProjectInstance.TaskDatabase);
			
			// null targets -> success. empty targets -> success(!)
			if (request.TargetNames == null)
				result.OverallResult = BuildResultCode.Success;
			else {
				foreach (var targetName in request.TargetNames.Where (t => t != null)) {
					if (checkCancel ())
						break;

					ProjectTargetInstance target;
					var targetResult = new TargetResult ();
					
					// FIXME: check skip condition
					if (false)
						targetResult.Skip ();
					// null key is allowed and regarded as blind success(!)
					else if (!request.ProjectInstance.Targets.TryGetValue (targetName, out target))
						targetResult.Success (new ITaskItem[0]);
					else {
						foreach (var c in target.Children.OfType<ProjectPropertyGroupTaskInstance> ()) {
							if (!project.EvaluateCondition (c.Condition))
								continue;
							throw new NotImplementedException ();
						}
						foreach (var c in target.Children.OfType<ProjectItemGroupTaskInstance> ()) {
							if (!project.EvaluateCondition (c.Condition))
								continue;
							throw new NotImplementedException ();
						}
						foreach (var c in target.Children.OfType<ProjectOnErrorInstance> ()) {
							if (!project.EvaluateCondition (c.Condition))
								continue;
							throw new NotImplementedException ();
						}
						foreach (var c in target.Children.OfType<ProjectTaskInstance> ()) {
							var host = request.HostServices == null ? null : request.HostServices.GetHostObject (request.ProjectFullPath, targetName, c.Name);
							if (!project.EvaluateCondition (c.Condition))
								continue;
							current_task = c;
							
							var factoryIdentityParameters = new Dictionary<string,string> ();
							factoryIdentityParameters ["MSBuildRuntime"] = c.MSBuildRuntime;
							factoryIdentityParameters ["MSBuildArchitecture"] = c.MSBuildArchitecture;
							var task = buildTaskFactory.GetTask (c.Name, factoryIdentityParameters, this);
							task.HostObject = host;
							task.BuildEngine = this;							
							if (!task.Execute ()) {
								targetResult.Failure (null);
								if (!project.EvaluateCondition (c.ContinueOnError))
									break;
							}
						}
					}
					result.AddResultsForTarget (targetName, targetResult);
				}
				
				// FIXME: check .NET behavior, whether cancellation always results in failure.
				result.OverallResult = checkCancel () ? BuildResultCode.Failure : result.ResultsByTarget.Select (p => p.Value).Any (r => r.ResultCode == TargetResultCode.Failure) ? BuildResultCode.Failure : BuildResultCode.Success;
			}
		}
		
		#region IBuildEngine4 implementation
		
		// task objects are not in use anyways though...
		
		class TaskObjectRegistration
		{
			public TaskObjectRegistration (object key, object obj, RegisteredTaskObjectLifetime lifetime, bool allowEarlyCollection)
			{
				Key = key;
				Object = obj;
				Lifetime = lifetime;
				AllowEarlyCollection = allowEarlyCollection;
			}
			public object Key { get; private set; }
			public object Object { get; private set; }
			public RegisteredTaskObjectLifetime Lifetime { get; private set; }
			public bool AllowEarlyCollection { get; private set; }
		}
		
		List<TaskObjectRegistration> task_objects = new List<TaskObjectRegistration> ();

		public object GetRegisteredTaskObject (object key, RegisteredTaskObjectLifetime lifetime)
		{
			var reg = task_objects.FirstOrDefault (t => t.Key == key && t.Lifetime == lifetime);
			return reg != null ? reg.Object : null;
		}

		public void RegisterTaskObject (object key, object obj, RegisteredTaskObjectLifetime lifetime, bool allowEarlyCollection)
		{
			task_objects.Add (new TaskObjectRegistration (key, obj, lifetime, allowEarlyCollection));
		}

		public object UnregisterTaskObject (object key, RegisteredTaskObjectLifetime lifetime)
		{
			var reg = task_objects.FirstOrDefault (t => t.Key == key && t.Lifetime == lifetime);
			if (reg != null)
				task_objects.Remove (reg);
			return reg.Object;
		}

		#endregion

		#region IBuildEngine3 implementation

		public BuildEngineResult BuildProjectFilesInParallel (string[] projectFileNames, string[] targetNames, IDictionary[] globalProperties, IList<string>[] removeGlobalProperties, string[] toolsVersion, bool returnTargetOutputs)
		{
			throw new NotImplementedException ();
		}

		public void Reacquire ()
		{
			throw new NotImplementedException ();
		}

		public void Yield ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region IBuildEngine2 implementation

		public bool BuildProjectFile (string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs, string toolsVersion)
		{
			var proj = GetProjectInstance (projectFileName, toolsVersion);
			var globalPropertiesThatMakeSense = new Dictionary<string,string> ();
			foreach (DictionaryEntry p in globalProperties)
				globalPropertiesThatMakeSense [(string) p.Key] = (string) p.Value;
			var result = new BuildResult ();
			var outputs = new Dictionary<string, string> ();
			BuildProject (() => false, result, proj, targetNames, globalPropertiesThatMakeSense, outputs, toolsVersion);
			foreach (var p in outputs)
				targetOutputs [p.Key] = p.Value;
			return result.OverallResult == BuildResultCode.Success;
		}

		public bool BuildProjectFilesInParallel (string[] projectFileNames, string[] targetNames, IDictionary[] globalProperties, IDictionary[] targetOutputsPerProject, string[] toolsVersion, bool useResultsCache, bool unloadProjectsOnCompletion)
		{
			throw new NotImplementedException ();
		}

		public bool IsRunningMultipleNodes {
			get {
				throw new NotImplementedException ();
			}
		}
		
		ProjectInstance GetProjectInstance (string projectFileName, string toolsVersion)
		{
			string fullPath = Path.GetFullPath (projectFileName);
			if (submission.BuildRequest.ProjectFullPath == fullPath)
				return submission.BuildRequest.ProjectInstance;
			// FIXME: could also be filtered by global properties
			// http://msdn.microsoft.com/en-us/library/microsoft.build.evaluation.projectcollection.getloadedprojects.aspx
			var project = Projects.GetLoadedProjects (projectFileName).FirstOrDefault (p => p.ToolsVersion == toolsVersion);
			if (project == null)
				throw new InvalidOperationException (string.Format ("Project '{0}' is not loaded", projectFileName));
			return submission.BuildManager.GetProjectInstanceForBuild (project);
		}

		#endregion

		#region IBuildEngine implementation

		public bool BuildProjectFile (string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
		{
			return BuildProjectFile (projectFileName, targetNames, globalProperties, targetOutputs, Projects.DefaultToolsVersion);
		}

		public void LogCustomEvent (CustomBuildEventArgs e)
		{
			event_source.FireCustomEventRaised (this, e);
		}

		public void LogErrorEvent (BuildErrorEventArgs e)
		{
			event_source.FireErrorRaised (this, e);
		}

		public void LogMessageEvent (BuildMessageEventArgs e)
		{
			event_source.FireMessageRaised (this, e);
		}

		public void LogWarningEvent (BuildWarningEventArgs e)
		{
			event_source.FireWarningRaised (this, e);
		}

		public int ColumnNumberOfTaskNode {
			get { return current_task.Location.Column; }
		}

		public bool ContinueOnError {
			get {
				switch (current_task.ContinueOnError) {
				case "true":
				case "WarnAndContinue":
				case "ErrorAndContinue":
					return true;
				}
				return false;
			}
		}

		public int LineNumberOfTaskNode {
			get { return current_task.Location.Line; }
		}

		public string ProjectFileOfTaskNode {
			get { return current_task.FullPath; }
		}

		#endregion
	}
}

