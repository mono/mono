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
using Microsoft.Build.Exceptions;

namespace Microsoft.Build.Internal
{
	class BuildEngine4
#if NET_4_5
		: IBuildEngine4
#else
		: IBuildEngine3
#endif
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
		ProjectTargetInstance current_target;
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
			if (toolsVersion == null)
				throw new ArgumentNullException ("toolsVersion");
			
			var parameters = submission.BuildManager.OngoingBuildParameters;
			var toolset = parameters.GetToolset (toolsVersion);
			if (toolset == null)
				throw new InvalidOperationException (string.Format ("Toolset version '{0}' was not resolved to valid toolset", toolsVersion));
			event_source.FireMessageRaised (this, new BuildMessageEventArgs (string.Format ("Using Toolset version {0}.", toolsVersion), null, null, MessageImportance.Low));
			var buildTaskFactory = new BuildTaskFactory (BuildTaskDatabase.GetDefaultTaskDatabase (toolset), submission.BuildRequest.ProjectInstance.TaskDatabase);
			BuildProject (new InternalBuildArguments () { CheckCancel = checkCancel, Result = result, Project = project, TargetNames = targetNames, GlobalProperties = globalProperties, TargetOutputs = targetOutputs, ToolsVersion = toolsVersion, BuildTaskFactory = buildTaskFactory });
		}

		class InternalBuildArguments
		{
			public Func<bool> CheckCancel;
			public BuildResult Result;
			public ProjectInstance Project;
			public IEnumerable<string> TargetNames;
			public IDictionary<string,string> GlobalProperties;
			public IDictionary<string,string> TargetOutputs;
			public string ToolsVersion;
			public BuildTaskFactory BuildTaskFactory;
			
			public void AddTargetResult (string targetName, TargetResult targetResult)
			{
				if (!Result.HasResultsForTarget (targetName))
					Result.AddResultsForTarget (targetName, targetResult);
			}
		}
		
		void BuildProject (InternalBuildArguments args)
		{
			var request = submission.BuildRequest;
			var parameters = submission.BuildManager.OngoingBuildParameters;
			this.project = args.Project;
			
			event_source.FireBuildStarted (this, new BuildStartedEventArgs ("Build Started", null));
			
			var initialPropertiesFormatted = "Initial Properties:\n" + string.Join (Environment.NewLine, project.Properties.OrderBy (p => p.Name).Select (p => string.Format ("{0} = {1}", p.Name, p.EvaluatedValue)).ToArray ());
			event_source.FireMessageRaised (this, new BuildMessageEventArgs (initialPropertiesFormatted, null, null, MessageImportance.Low));
			
			// null targets -> success. empty targets -> success(!)
			if (request.TargetNames == null)
				args.Result.OverallResult = BuildResultCode.Success;
			else {
				foreach (var targetName in request.TargetNames.Where (t => t != null))
					args.AddTargetResult (targetName, BuildTargetByName (targetName, args));
		
				// FIXME: check .NET behavior, whether cancellation always results in failure.
				args.Result.OverallResult = args.CheckCancel () ? BuildResultCode.Failure : args.Result.ResultsByTarget.Select (p => p.Value).Any (r => r.ResultCode == TargetResultCode.Failure) ? BuildResultCode.Failure : BuildResultCode.Success;
			}			
			event_source.FireBuildFinished (this, new BuildFinishedEventArgs ("Build Finished.", null, args.Result.OverallResult == BuildResultCode.Success));
		}
		
		TargetResult BuildTargetByName (string targetName, InternalBuildArguments args)
		{
			var targetResult = new TargetResult ();

			var request = submission.BuildRequest;
			var parameters = submission.BuildManager.OngoingBuildParameters;
			ProjectTargetInstance target;
			
			// FIXME: check skip condition
			if (false)
				targetResult.Skip ();
			// null key is allowed and regarded as blind success(!) (as long as it could retrieve target)
			else if (!request.ProjectInstance.Targets.TryGetValue (targetName, out target))
				targetResult.Failure (new InvalidOperationException (string.Format ("target '{0}' was not found in project '{1}'", targetName, project.FullPath)));
			else {
				current_target = target;
				try {
					if (!DoBuildTarget (targetResult, args))
						return targetResult;
				} finally {
					current_target = null;
				}
				Func<string,ITaskItem> creator = s => new TargetOutputTaskItem () { ItemSpec = s };
				var items = args.Project.GetAllItems (target.Outputs, string.Empty, creator, creator, s => true, (t, s) => {
				});
				targetResult.Success (items);
				event_source.FireTargetFinished (this, new TargetFinishedEventArgs ("Target Finished", null, targetName, project.FullPath, target.FullPath, true));
			}
			return targetResult;
		}
		
		bool DoBuildTarget (TargetResult targetResult, InternalBuildArguments args)
		{
			var request = submission.BuildRequest;
			var target = current_target;
	
			// process DependsOnTargets first.
			foreach (var dep in project.ExpandString (target.DependsOnTargets).Split (';').Where (s => !string.IsNullOrEmpty (s)).Select (s => s.Trim ())) {
				var result = BuildTargetByName (dep, args);
				args.AddTargetResult (dep, result);
				if (result.ResultCode == TargetResultCode.Failure) {
					targetResult.Failure (null);
					return false;
				}
			}
			
			event_source.FireTargetStarted (this, new TargetStartedEventArgs ("Target Started", null, target.Name, project.FullPath, target.FullPath));
			
			// Here we check cancellation (only after TargetStarted event).
			if (args.CheckCancel ()) {
				targetResult.Failure (new BuildAbortedException ("Build has canceled"));
				return false;
			}
			
			var propsToRestore = new Dictionary<string,string> ();
			var itemsToRemove = new List<ProjectItemInstance> ();
			try {
				// Evaluate additional target properties
				foreach (var c in target.Children.OfType<ProjectPropertyGroupTaskInstance> ()) {
					if (!args.Project.EvaluateCondition (c.Condition))
						continue;
					foreach (var p in c.Properties) {
						if (!args.Project.EvaluateCondition (p.Condition))
							continue;
						var value = args.Project.ExpandString (p.Value);
						propsToRestore.Add (p.Name, project.GetPropertyValue (value));
						project.SetProperty (p.Name, value);
					}
				}
				
				// Evaluate additional target items
				foreach (var c in target.Children.OfType<ProjectItemGroupTaskInstance> ()) {
					if (!args.Project.EvaluateCondition (c.Condition))
						continue;
					foreach (var item in c.Items) {
						Func<string,ProjectItemInstance> creator = i => new ProjectItemInstance (project, item.ItemType, item.Metadata.Select (m => new KeyValuePair<string,string> (m.Name, m.Value)), i);
						foreach (var ti in project.GetAllItems (item.Include, item.Exclude, creator, creator, s => s == item.ItemType, (ti, s) => ti.SetMetadata ("RecurseDir", s)))
							itemsToRemove.Add (ti);
					}
				}
				
				foreach (var c in target.Children.OfType<ProjectOnErrorInstance> ()) {
					if (!args.Project.EvaluateCondition (c.Condition))
						continue;
					throw new NotImplementedException ();
				}
				
				// run tasks
				foreach (var ti in target.Children.OfType<ProjectTaskInstance> ()) {
					current_task = ti;
					if (!args.Project.EvaluateCondition (ti.Condition)) {
						event_source.FireMessageRaised (this, new BuildMessageEventArgs (string.Format ("Task '{0}' was skipped because condition '{1}' wasn't met.", ti.Name, ti.Condition), null, null, MessageImportance.Low));
						continue;
					}
					if (!RunBuildTask (ti, targetResult, args))
						return false;
				}
			} finally {
				// restore temporary property state to the original state.
				foreach (var p in propsToRestore) {
					if (p.Value == string.Empty)
						project.RemoveProperty (p.Key);
					else
						project.SetProperty (p.Key, p.Value);
				}
				foreach (var item in itemsToRemove)
					project.RemoveItem (item);
			}
			return true;
		}
		
		bool RunBuildTask (ProjectTaskInstance ti, TargetResult targetResult, InternalBuildArguments args)
		{
			var request = submission.BuildRequest;
			var target = current_target;

			var host = request.HostServices == null ? null : request.HostServices.GetHostObject (request.ProjectFullPath, target.Name, ti.Name);
			
			// Create Task instance.
			var factoryIdentityParameters = new Dictionary<string,string> ();
			#if NET_4_5
			factoryIdentityParameters ["MSBuildRuntime"] = ti.MSBuildRuntime;
			factoryIdentityParameters ["MSBuildArchitecture"] = ti.MSBuildArchitecture;
			#endif
			var task = args.BuildTaskFactory.CreateTask (ti.Name, factoryIdentityParameters, this);
			event_source.FireMessageRaised (this, new BuildMessageEventArgs (string.Format ("Using task {0} from {1}", ti.Name, task.GetType ().AssemblyQualifiedName), null, null, MessageImportance.Low));
			task.HostObject = host;
			task.BuildEngine = this;
			
			// Prepare task parameters.
			var props = task.GetType ().GetProperties ()
				.Where (p => p.CanWrite && p.GetCustomAttributes (typeof (RequiredAttribute), true).Any ());
			var missings = props.Where (p => !ti.Parameters.Any (tp => tp.Key.Equals (p.Name, StringComparison.OrdinalIgnoreCase)));
			if (missings.Any ())
				throw new InvalidOperationException (string.Format ("Task {0} of type {1} is used without specifying mandatory property: {2}",
					ti.Name, task.GetType (), string.Join (", ", missings.Select (p => p.Name).ToArray ())));
			
			foreach (var p in ti.Parameters) {
				var prop = task.GetType ().GetProperty (p.Key);
				var value = project.ExpandString (p.Value);
				if (prop == null)
					throw new InvalidOperationException (string.Format ("Task {0} does not have property {1}", ti.Name, p.Key));
				if (!prop.CanWrite)
					throw new InvalidOperationException (string.Format ("Task {0} has property {1} but it is read-only.", ti.Name, p.Key));
				var valueInstance = ConvertTo (value, prop.PropertyType);
				prop.SetValue (task, valueInstance, null);
			}
			
			// Do execute task.
			event_source.FireTaskStarted (this, new TaskStartedEventArgs ("Task Started", null, project.FullPath, ti.FullPath, ti.Name));
			var taskSuccess = task.Execute ();
			
			if (!taskSuccess) {
				event_source.FireTaskFinished (this, new TaskFinishedEventArgs ("Task Finished", null, project.FullPath, ti.FullPath, ti.Name, false));
				targetResult.Failure (null);
				if (!ContinueOnError) {
					event_source.FireTargetFinished (this, new TargetFinishedEventArgs ("Target Failed", null, target.Name, project.FullPath, target.FullPath, false));
					return false;
				}
			} else {
				// Evaluate task output properties and items.
				event_source.FireTaskFinished (this, new TaskFinishedEventArgs ("Task Finished", null, project.FullPath, ti.FullPath, ti.Name, true));
				foreach (var to in ti.Outputs) {
					if (!project.EvaluateCondition (to.Condition))
						continue;
					var toItem = to as ProjectTaskOutputItemInstance;
					var toProp = to as ProjectTaskOutputPropertyInstance;
					string taskParameter = toItem != null ? toItem.TaskParameter : toProp.TaskParameter;
					var pi = task.GetType ().GetProperty (taskParameter);
					if (pi == null)
						throw new InvalidOperationException (string.Format ("Task {0} does not have property {1} specified as TaskParameter", ti.Name, toItem.TaskParameter));
					if (!pi.CanRead)
						throw new InvalidOperationException (string.Format ("Task {0} has property {1} specified as TaskParameter, but it is write-only", ti.Name, toItem.TaskParameter));
					if (toItem != null)
						args.Project.AddItem (toItem.ItemType, ConvertFrom (pi.GetValue (task, null)));
					else
						args.Project.SetProperty (toProp.PropertyName, ConvertFrom (pi.GetValue (task, null)));
				}
			}
			
			return true;
		}
		
		object ConvertTo (string source, Type targetType)
		{
			if (targetType == typeof (ITaskItem) || targetType.IsSubclassOf (typeof (ITaskItem)))
				return new TargetOutputTaskItem () { ItemSpec = WindowsCompatibilityExtensions.NormalizeFilePath (source) };
			if (targetType.IsArray)
				return new ArrayList (source.Split (';').Where (s => !string.IsNullOrEmpty (s)).Select (s => ConvertTo (s, targetType.GetElementType ())).ToArray ())
						.ToArray (targetType.GetElementType ());
			else
				return Convert.ChangeType (source, targetType);
		}
		
		string ConvertFrom (object source)
		{
			if (source == null)
				return string.Empty;
			if (source is ITaskItem)
				return ((ITaskItem) source).ItemSpec;
			if (source.GetType ().IsArray)
				return string.Join (":", ((Array) source).Cast<object> ().Select (o => ConvertFrom (o)).ToArray ());
			else
				return (string) Convert.ChangeType (source, typeof (string));
		}
		
		class TargetOutputTaskItem : ITaskItem2
		{
			Hashtable metadata = new Hashtable ();
			
			#region ITaskItem2 implementation
			public string GetMetadataValueEscaped (string metadataName)
			{
				return ProjectCollection.Escape ((string) metadata [metadataName]);
			}
			public void SetMetadataValueLiteral (string metadataName, string metadataValue)
			{
				metadata [metadataName] = ProjectCollection.Unescape (metadataValue);
			}
			public IDictionary CloneCustomMetadataEscaped ()
			{
				var ret = new Hashtable ();
				foreach (DictionaryEntry e in metadata)
					ret [e.Key] = ProjectCollection.Escape ((string) e.Value);
				return ret;
			}
			public string EvaluatedIncludeEscaped {
				get { return ProjectCollection.Escape (ItemSpec); }
				set { ItemSpec = ProjectCollection.Unescape (value); }
			}
			#endregion
			#region ITaskItem implementation
			public IDictionary CloneCustomMetadata ()
			{
				return new Hashtable (metadata);
			}
			public void CopyMetadataTo (ITaskItem destinationItem)
			{
				foreach (DictionaryEntry e in metadata)
					destinationItem.SetMetadata ((string) e.Key, (string) e.Value);
			}
			public string GetMetadata (string metadataName)
			{
				var wk = ProjectCollection.GetWellKnownMetadata (metadataName, ItemSpec, Path.GetFullPath, null);
				if (wk != null)
					return wk;
				return (string) metadata [metadataName];
			}
			public void RemoveMetadata (string metadataName)
			{
				metadata.Remove (metadataName);
			}
			public void SetMetadata (string metadataName, string metadataValue)
			{
				metadata [metadataName] = metadataValue;
			}
			public string ItemSpec { get; set; }
			public int MetadataCount {
				get { return metadata.Count; }
			}
			public ICollection MetadataNames {
				get { return metadata.Keys; }
			}
			#endregion
		}
		
#if NET_4_5
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
#endif

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
			get { return current_task.Location != null ? current_task.Location.Column : 0; }
		}

		public bool ContinueOnError {
			get { return current_task != null && project.EvaluateCondition (current_task.Condition) && EvaluateContinueOnError (current_task.ContinueOnError); }
		}
		
		bool EvaluateContinueOnError (string value)
		{
			switch (value) {
			case "WarnAndContinue":
			case "ErrorAndContinue":
				return true;
			case "ErrorAndStop":
				return false;
			}
			// empty means "stop on error", so don't pass empty string to EvaluateCondition().
			return !string.IsNullOrEmpty (value) && project.EvaluateCondition (value);
		}

		public int LineNumberOfTaskNode {
			get { return current_task.Location != null ? current_task.Location.Line : 0; }
		}

		public string ProjectFileOfTaskNode {
			get { return current_task.FullPath; }
		}

		#endregion
	}
}

