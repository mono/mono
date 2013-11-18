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
		}

		BuildSubmission submission;
		ProjectInstance current_project;
		ProjectTaskInstance current_task;
		EventSource event_source;
		
		public ProjectCollection Projects {
			get { return submission.BuildManager.OngoingBuildParameters.ProjectCollection; }
		}
		
		class BuildTask : ITask
		{
			public IBuildEngine BuildEngine { get; set; }
			public ITaskHost HostObject { get; set; }
		}

		public BuildResult BuildProject (Func<bool> checkCancel, ProjectInstance project, IEnumerable<string> targetNames, IDictionary<string,string> globalProperties, IDictionary<string,string> targetOutputs, string toolsVersion)
		{
			var result = new BuildResult () { SubmissionId = submission.SubmissionId };
			var request = submission.BuildRequest;
			var parameters = submission.BuildManager.OngoingBuildParameters;
			
			// null targets -> success. empty targets -> success(!)
			if (request.TargetNames == null)
				result.OverallResult = BuildResultCode.Success;
			else {
				foreach (var targetName in request.TargetNames.Where (t => t != null)) {
					if (checkCancel ())
						break;

					ProjectTargetInstance target;
					// null key is allowed and regarded as blind success(!)
					if (!request.ProjectInstance.Targets.TryGetValue (targetName, out target))
						result.AddResultsForTarget (targetName, new TargetResult (new ITaskItem [0], TargetResultCode.Failure));
					else {
						foreach (var c in target.Children.OfType<ProjectPropertyGroupTaskInstance> ())
							throw new NotImplementedException ();
						foreach (var c in target.Children.OfType<ProjectItemGroupTaskInstance> ())
							throw new NotImplementedException ();
						foreach (var c in target.Children.OfType<ProjectOnErrorInstance> ())
							throw new NotImplementedException ();
						foreach (var c in target.Children.OfType<ProjectTaskInstance> ()) {
							var host = request.HostServices == null ? null : request.HostServices.GetHostObject (request.ProjectFullPath, targetName, c.Name);
							var task = new BuildTask () { BuildEngine = this, HostObject = host };
							
							throw new NotImplementedException ();
						}
					}
				}
				
				// FIXME: check .NET behavior, whether cancellation always results in failure.
				result.OverallResult = checkCancel () ? BuildResultCode.Failure : result.ResultsByTarget.Select (p => p.Value).Any (r => r.ResultCode == TargetResultCode.Failure) ? BuildResultCode.Failure : BuildResultCode.Success;
			}
			return result;
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
			var project = GetProjectInstance (projectFileName, toolsVersion);
			var globalPropertiesThatMakeSense = new Dictionary<string,string> ();
			foreach (DictionaryEntry p in globalProperties)
				globalPropertiesThatMakeSense [(string) p.Key] = (string) p.Value;
			var outputs = new Dictionary<string, string> ();
			var result = BuildProject (() => false, project, targetNames, globalPropertiesThatMakeSense, outputs, toolsVersion);
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
			get { return current_project.EvaluateCondition (current_task.ContinueOnError); }
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

