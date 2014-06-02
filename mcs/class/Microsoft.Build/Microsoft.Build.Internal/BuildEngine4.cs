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
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Evaluation;
using System.Linq;
using System.IO;
using Microsoft.Build.Exceptions;
using System.Globalization;
using Microsoft.Build.Construction;
using Microsoft.Build.Internal.Expressions;
using System.Xml;

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
			event_source = new Microsoft.Build.BuildEngine.EventSource ();
			if (submission.BuildManager.OngoingBuildParameters.Loggers != null)
				foreach (var l in submission.BuildManager.OngoingBuildParameters.Loggers)
					l.Initialize (event_source);
		}

		BuildSubmission submission;
		ProjectInstance project;
		ProjectTaskInstance current_task;
		Microsoft.Build.BuildEngine.EventSource event_source;
		
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
			LogMessageEvent (new BuildMessageEventArgs (string.Format ("Using Toolset version {0}.", toolsVersion), null, null, MessageImportance.Low));
			var buildTaskFactory = new BuildTaskFactory (BuildTaskDatabase.GetDefaultTaskDatabase (toolset), new BuildTaskDatabase (this, submission.BuildRequest.ProjectInstance));
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

			string directoryBackup = Directory.GetCurrentDirectory ();
			Directory.SetCurrentDirectory (project.Directory);
			event_source.FireBuildStarted (this, new BuildStartedEventArgs ("Build Started", null, DateTime.Now));
			
			try {
				
				var initialGlobalPropertiesFormatted = "Initial Global Properties:\n" + string.Join (Environment.NewLine, project.Properties.OrderBy (p => p.Name).Where (p => p.IsImmutable).Select (p => string.Format ("{0} = {1}", p.Name, p.EvaluatedValue)).ToArray ());
				LogMessageEvent (new BuildMessageEventArgs (initialGlobalPropertiesFormatted, null, null, MessageImportance.Low));
				var initialProjectPropertiesFormatted = "Initial Project Properties:\n" + string.Join (Environment.NewLine, project.Properties.OrderBy (p => p.Name).Where (p => !p.IsImmutable).Select (p => string.Format ("{0} = {1}", p.Name, p.EvaluatedValue)).ToArray ());
				LogMessageEvent (new BuildMessageEventArgs (initialProjectPropertiesFormatted, null, null, MessageImportance.Low));
				var initialItemsFormatted = "Initial Items:\n" + string.Join (Environment.NewLine, project.Items.OrderBy (i => i.ItemType).Select (i => string.Format ("{0} : {1}", i.ItemType, i.EvaluatedInclude)).ToArray ());
				LogMessageEvent (new BuildMessageEventArgs (initialItemsFormatted, null, null, MessageImportance.Low));
				
				// null targets -> success. empty targets -> success(!)
				foreach (var targetName in (request.ProjectInstance.InitialTargets).Where (t => t != null))
					BuildTargetByName (targetName, args);
				if (request.TargetNames == null)
					args.Result.OverallResult = args.CheckCancel () ? BuildResultCode.Failure : args.Result.ResultsByTarget.Any (p => p.Value.ResultCode == TargetResultCode.Failure) ? BuildResultCode.Failure : BuildResultCode.Success;
				else {
					foreach (var targetName in (args.TargetNames ?? request.TargetNames).Where (t => t != null))
						BuildTargetByName (targetName, args);
			
					// FIXME: check .NET behavior, whether cancellation always results in failure.
					args.Result.OverallResult = args.CheckCancel () ? BuildResultCode.Failure : args.Result.ResultsByTarget.Any (p => p.Value.ResultCode == TargetResultCode.Failure) ? BuildResultCode.Failure : BuildResultCode.Success;
				}
			} catch (Exception ex) {
				args.Result.OverallResult = BuildResultCode.Failure;
				LogErrorEvent (new BuildErrorEventArgs (null, null, project.FullPath, 0, 0, 0, 0, "Unhandled exception occured during a build", null, null));
				LogMessageEvent (new BuildMessageEventArgs ("Exception details: " + ex, null, null, MessageImportance.Low));
				throw; // BuildSubmission re-catches this.
			} finally {
				event_source.FireBuildFinished (this, new BuildFinishedEventArgs ("Build Finished.", null, args.Result.OverallResult == BuildResultCode.Success, DateTime.Now));
				Directory.SetCurrentDirectory (directoryBackup);
			}
		}
		
		bool BuildTargetByName (string targetName, InternalBuildArguments args)
		{
			var request = submission.BuildRequest;
			var parameters = submission.BuildManager.OngoingBuildParameters;
			ProjectTargetInstance target;
			TargetResult dummyResult;

			if (args.Result.ResultsByTarget.TryGetValue (targetName, out dummyResult) && dummyResult.ResultCode == TargetResultCode.Success) {
				LogMessageEvent (new BuildMessageEventArgs (string.Format ("Target '{0}' was skipped because it was already built successfully.", targetName), null, null, MessageImportance.Low));
				return true; // do not add result.
			}
			
			var targetResult = new TargetResult ();

			// null key is allowed and regarded as blind success(!) (as long as it could retrieve target)
			if (!request.ProjectInstance.Targets.TryGetValue (targetName, out target))
				// FIXME: from MSBuild.exe it is given MSB4057. Can we assign a number too?
				throw new InvalidOperationException (string.Format ("target '{0}' was not found in project '{1}'", targetName, project.FullPath));
			else if (!args.Project.EvaluateCondition (target.Condition)) {
				LogMessageEvent (new BuildMessageEventArgs (string.Format ("Target '{0}' was skipped because condition '{1}' was not met.", target.Name, target.Condition), null, null, MessageImportance.Low));
				targetResult.Skip ();
			} else {
				// process DependsOnTargets first.
				foreach (var dep in project.ExpandString (target.DependsOnTargets).Split (';').Select (s => s.Trim ()).Where (s => !string.IsNullOrEmpty (s))) {
					LogMessageEvent (new BuildMessageEventArgs (string.Format ("Target '{0}' depends on '{1}'.", target.Name, dep), null, null, MessageImportance.Low));
					if (!BuildTargetByName (dep, args)) {
						LogMessageEvent (new BuildMessageEventArgs (string.Format ("Quit target '{0}', as dependency target '{1}' has failed.", target.Name, dep), null, null, MessageImportance.Low));
						return false;
					}
				}
				
				Func<string,ITaskItem> creator = s => new TargetOutputTaskItem () { ItemSpec = s };
			
				event_source.FireTargetStarted (this, new TargetStartedEventArgs ("Target Started", null, target.Name, project.FullPath, target.FullPath));
				try {
					// FIXME: examine in which scenario Inputs/Outputs inconsistency results in errors. Now it rather prevents csproj build.
					/*if (!string.IsNullOrEmpty (target.Inputs) != !string.IsNullOrEmpty (target.Outputs)) {
						targetResult.Failure (new InvalidProjectFileException (target.Location, null, string.Format ("Target {0} has mismatching Inputs and Outputs specification. When one is specified, another one has to be specified too.", targetName), null, null, null));
					} else*/ {
						bool skip = false;
						if (!string.IsNullOrEmpty (target.Inputs)) {
							var inputs = args.Project.GetAllItems (target.Inputs, string.Empty, creator, creator, s => true, (t, s) => {
							});
							if (!inputs.Any ()) {
								LogMessageEvent (new BuildMessageEventArgs (string.Format ("Target '{0}' was skipped because there is no input.", target.Name), null, null, MessageImportance.Low));
								skip = true;
							} else {
								var outputs = args.Project.GetAllItems (target.Outputs, string.Empty, creator, creator, s => true, (t, s) => {
								});
								var needsUpdates = GetOlderOutputsThanInputs (inputs, outputs).FirstOrDefault ();
								if (needsUpdates != null)
									LogMessageEvent (new BuildMessageEventArgs (string.Format ("Target '{0}' needs to be built because new output {1} is needed.", target.Name, needsUpdates.ItemSpec), null, null, MessageImportance.Low));
								else {
									LogMessageEvent (new BuildMessageEventArgs (string.Format ("Target '{0}' was skipped because all the outputs are newer than all the inputs.", target.Name), null, null, MessageImportance.Low));
									skip = true;
								}
							}
						}
						if (skip) {
							targetResult.Skip ();
						} else {
							if (DoBuildTarget (target, targetResult, args)) {
								var items = args.Project.GetAllItems (target.Outputs, string.Empty, creator, creator, s => true, (t, s) => {
								});
								targetResult.Success (items);
							}
						}
					}
				} finally {
					event_source.FireTargetFinished (this, new TargetFinishedEventArgs ("Target Finished", null, targetName, project.FullPath, target.FullPath, targetResult.ResultCode != TargetResultCode.Failure));
				}
			}
			args.AddTargetResult (targetName, targetResult);
			
			return targetResult.ResultCode != TargetResultCode.Failure;
		}
		
		IEnumerable<ITaskItem> GetOlderOutputsThanInputs (IEnumerable<ITaskItem> inputs, IEnumerable<ITaskItem> outputs)
		{
			return outputs.Where (o => !File.Exists (o.GetMetadata ("FullPath")) || inputs.Any (i => string.CompareOrdinal (i.GetMetadata ("LastModifiedTime"), o.GetMetadata ("LastModifiedTime")) > 0));
		}

		// FIXME: Exception should be caught at caller site.
		bool DoBuildTarget (ProjectTargetInstance target, TargetResult targetResult, InternalBuildArguments args)
		{
			var request = submission.BuildRequest;
	
			// Here we check cancellation (only after TargetStarted event).
			if (args.CheckCancel ()) {
				targetResult.Failure (new BuildAbortedException ("Build has canceled"));
				return false;
			}
			
			try {
				foreach (var child in target.Children) {
					// Evaluate additional target properties
					var tp = child as ProjectPropertyGroupTaskInstance;
					if (tp != null) {
						if (!args.Project.EvaluateCondition (tp.Condition))
							continue;
						foreach (var p in tp.Properties) {
							if (!args.Project.EvaluateCondition (p.Condition))
								continue;
							var value = args.Project.ExpandString (p.Value);
							project.SetProperty (p.Name, value);
						}
						continue;
					}

					var ii = child as ProjectItemGroupTaskInstance;
					if (ii != null) {
						if (!args.Project.EvaluateCondition (ii.Condition))
							continue;
						foreach (var item in ii.Items) {
							if (!args.Project.EvaluateCondition (item.Condition))
								continue;
							project.AddItem (item.ItemType, project.ExpandString (item.Include));
						}
						continue;
					}
					
					var task = child as ProjectTaskInstance;
					if (task != null) {
						current_task = task;
						if (!args.Project.EvaluateCondition (task.Condition)) {
							LogMessageEvent (new BuildMessageEventArgs (string.Format ("Task '{0}' was skipped because condition '{1}' wasn't met.", task.Name, task.Condition), null, null, MessageImportance.Low));
							continue;
						}
						if (!RunBuildTask (target, task, targetResult, args))
							return false;
						continue;
					}

					var onError = child as ProjectOnErrorInstance;
					if (onError != null)
						continue; // evaluated under catch clause.

					throw new NotSupportedException (string.Format ("Unexpected Target element children \"{0}\"", child.GetType ()));
				}
			} catch (Exception ex) {
				// fallback task specified by OnError element
				foreach (var c in target.Children.OfType<ProjectOnErrorInstance> ()) {
					if (!args.Project.EvaluateCondition (c.Condition))
						continue;
					foreach (var fallbackTarget in project.ExpandString (c.ExecuteTargets).Split (';'))
						BuildTargetByName (fallbackTarget, args);
				}
				int line = target.Location != null ? target.Location.Line : 0;
				int col = target.Location != null ? target.Location.Column : 0;
				LogErrorEvent (new BuildErrorEventArgs (null, null, target.FullPath, line, col, 0, 0, ex.Message, null, null));
				targetResult.Failure (ex);
				return false;
			}
			return true;
		}
		
		bool RunBuildTask (ProjectTargetInstance target, ProjectTaskInstance taskInstance, TargetResult targetResult, InternalBuildArguments args)
		{
			var request = submission.BuildRequest;

			var host = request.HostServices == null ? null : request.HostServices.GetHostObject (request.ProjectFullPath, target.Name, taskInstance.Name);
			
			// Create Task instance.
			var factoryIdentityParameters = new Dictionary<string,string> ();
			#if NET_4_5
			factoryIdentityParameters ["MSBuildRuntime"] = taskInstance.MSBuildRuntime;
			factoryIdentityParameters ["MSBuildArchitecture"] = taskInstance.MSBuildArchitecture;
			#endif
			var task = args.BuildTaskFactory.CreateTask (taskInstance.Name, factoryIdentityParameters, this);
			if (task == null)
				throw new InvalidOperationException (string.Format ("TaskFactory {0} returned null Task", args.BuildTaskFactory));
			LogMessageEvent (new BuildMessageEventArgs (string.Format ("Using task {0} from {1}", taskInstance.Name, task.GetType ()), null, null, MessageImportance.Low));
			task.HostObject = host;
			task.BuildEngine = this;
			
			// Prepare task parameters.
			var evaluator = new ExpressionEvaluator (project);
			var evaluatedTaskParams = taskInstance.Parameters.Select (p => new KeyValuePair<string,string> (p.Key, project.ExpandString (evaluator, p.Value)));

			var requiredProps = task.GetType ().GetProperties ()
				.Where (p => p.CanWrite && p.GetCustomAttributes (typeof (RequiredAttribute), true).Any ());
			var missings = requiredProps.Where (p => !evaluatedTaskParams.Any (tp => tp.Key.Equals (p.Name, StringComparison.OrdinalIgnoreCase)));
			if (missings.Any ())
				throw new InvalidOperationException (string.Format ("Task {0} of type {1} is used without specifying mandatory property: {2}",
					taskInstance.Name, task.GetType (), string.Join (", ", missings.Select (p => p.Name).ToArray ())));
			
			foreach (var p in evaluatedTaskParams) {
				switch (p.Key.ToLower ()) {
				case "condition":
				case "continueonerror":
					continue;
				}
				var prop = task.GetType ().GetProperty (p.Key);
				if (prop == null)
					throw new InvalidOperationException (string.Format ("Task {0} does not have property {1}", taskInstance.Name, p.Key));
				if (!prop.CanWrite)
					throw new InvalidOperationException (string.Format ("Task {0} has property {1} but it is read-only.", taskInstance.Name, p.Key));
				if (string.IsNullOrEmpty (p.Value) && !requiredProps.Contains (prop))
					continue;
				try {
					prop.SetValue (task, ConvertTo (p.Value, prop.PropertyType, evaluator), null);
				} catch (Exception ex) {
					throw new InvalidOperationException (string.Format ("Failed to convert '{0}' for property '{1}' of type {2}", p.Value, prop.Name, prop.PropertyType), ex);
				}
			}
			
			// Do execute task.
			bool taskSuccess = false;
			event_source.FireTaskStarted (this, new TaskStartedEventArgs ("Task Started", null, project.FullPath, taskInstance.FullPath, taskInstance.Name));
			try {
				taskSuccess = task.Execute ();
			
				if (!taskSuccess) {
					targetResult.Failure (null);
					if (!ContinueOnError) {
						return false;
					}
				} else {
					// Evaluate task output properties and items.
					foreach (var to in taskInstance.Outputs) {
						if (!project.EvaluateCondition (to.Condition))
							continue;
						var toItem = to as ProjectTaskOutputItemInstance;
						var toProp = to as ProjectTaskOutputPropertyInstance;
						string taskParameter = toItem != null ? toItem.TaskParameter : toProp.TaskParameter;
						var pi = task.GetType ().GetProperty (taskParameter);
						if (pi == null)
							throw new InvalidOperationException (string.Format ("Task {0} does not have property {1} specified as TaskParameter", taskInstance.Name, toItem.TaskParameter));
						if (!pi.CanRead)
							throw new InvalidOperationException (string.Format ("Task {0} has property {1} specified as TaskParameter, but it is write-only", taskInstance.Name, toItem.TaskParameter));
						var value = pi.GetValue (task, null);
						var valueString = ConvertFrom (value);
						if (toItem != null) {
							LogMessageEvent (new BuildMessageEventArgs (string.Format ("Output Item {0} from TaskParameter {1}: {2}", toItem.ItemType, toItem.TaskParameter, valueString), null, null, MessageImportance.Low));
							Action<ITaskItem> addItem = i => {
								var metadata = new ArrayList (i.MetadataNames).ToArray ().Cast<string> ().Select (n => new KeyValuePair<string,string> (n, i.GetMetadata (n)));
								args.Project.AddItem (toItem.ItemType, i.ItemSpec, metadata);
							};
							var taskItemArray = value as ITaskItem [];
							if (taskItemArray != null) {
								foreach (var ti in taskItemArray)
									addItem (ti);
							} else {
								var taskItem = value as ITaskItem;
								if (taskItem != null) 
									addItem (taskItem);
								else
									foreach (var item in valueString.Split (';'))
										args.Project.AddItem (toItem.ItemType, item);
							}
						} else {
							LogMessageEvent (new BuildMessageEventArgs (string.Format ("Output Property {0} from TaskParameter {1}: {2}", toProp.PropertyName, toProp.TaskParameter, valueString), null, null, MessageImportance.Low));
							args.Project.SetProperty (toProp.PropertyName, valueString);
						}
					}
				}
			} finally {
				event_source.FireTaskFinished (this, new TaskFinishedEventArgs ("Task Finished", null, project.FullPath, taskInstance.FullPath, taskInstance.Name, taskSuccess));
			}
			return true;
		}

		object ConvertTo (string source, Type targetType, ExpressionEvaluator evaluator)
		{
			if (targetType == typeof (ITaskItem) || targetType.IsSubclassOf (typeof (ITaskItem))) {
				var item = evaluator.EvaluatedTaskItems.FirstOrDefault (i => string.Equals (i.ItemSpec, source.Trim (), StringComparison.OrdinalIgnoreCase));
				var ret = new TargetOutputTaskItem () { ItemSpec = source.Trim () };
				if (item != null)
					foreach (string name in item.MetadataNames)
						ret.SetMetadata (name, item.GetMetadata (name));
				return ret;
			}
			if (targetType.IsArray)
				return new ArrayList (source.Split (';').Select (s => s.Trim ()).Where (s => !string.IsNullOrEmpty (s)).Select (s => ConvertTo (s, targetType.GetElementType (), evaluator)).ToArray ())
						.ToArray (targetType.GetElementType ());
			if (targetType == typeof (bool)) {
				switch (source != null ? source.ToLower (CultureInfo.InvariantCulture) : string.Empty) {
				case "true":
				case "yes":
				case "on":
					return true;
				case "false":
				case "no":
				case "off":
				case "":
					return false;
				}
			}
			return Convert.ChangeType (source == "" ? null : source, targetType);
		}
		
		string ConvertFrom (object source)
		{
			if (source == null)
				return string.Empty;
			if (source is ITaskItem)
				return ((ITaskItem) source).ItemSpec;
			if (source.GetType ().IsArray)
				return string.Join (";", ((Array) source).Cast<object> ().Select (o => ConvertFrom (o)).ToArray ());
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
				var ret = (string) metadata [metadataName];
				return ret ?? string.Empty;
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

			public override string ToString ()
			{
				return ItemSpec;
			}
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

		// To NOT reuse this IBuildEngine instance for different build, we create another BuildManager and BuildSubmisson and then run it.
		public bool BuildProjectFile (string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs, string toolsVersion)
		{
			toolsVersion = string.IsNullOrEmpty (toolsVersion) ? project.ToolsVersion : toolsVersion;
			var globalPropertiesThatMakeSense = new Dictionary<string,string> ();
			foreach (DictionaryEntry p in globalProperties)
				globalPropertiesThatMakeSense [(string) p.Key] = (string) p.Value;
			var projectToBuild = new ProjectInstance (ProjectRootElement.Create (XmlReader.Create (projectFileName)), globalPropertiesThatMakeSense, toolsVersion, Projects);
			IDictionary<string,TargetResult> outs;
			var ret = projectToBuild.Build (targetNames ?? new string [] {"Build"}, Projects.Loggers, out outs);
			foreach (var p in outs)
				targetOutputs [p.Key] = p.Value.Items ?? new ITaskItem [0];
			return ret;
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

