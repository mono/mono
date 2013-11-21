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
					// null key is allowed and regarded as blind success(!) (as long as it could retrieve target)
					else if (!request.ProjectInstance.Targets.TryGetValue (targetName, out target))
						targetResult.Failure (null);
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
						foreach (var ti in target.Children.OfType<ProjectTaskInstance> ()) {
							var host = request.HostServices == null ? null : request.HostServices.GetHostObject (request.ProjectFullPath, targetName, ti.Name);
							if (!project.EvaluateCondition (ti.Condition))
								continue;
							current_task = ti;
							
							var factoryIdentityParameters = new Dictionary<string,string> ();
#if NET_4_5
							factoryIdentityParameters ["MSBuildRuntime"] = ti.MSBuildRuntime;
							factoryIdentityParameters ["MSBuildArchitecture"] = ti.MSBuildArchitecture;
#endif
							var task = buildTaskFactory.CreateTask (ti.Name, factoryIdentityParameters, this);
							task.HostObject = host;
							task.BuildEngine = this;
							// FIXME: this cannot be that simple, value has to be converted to the appropriate target type.
							var props = task.GetType ().GetProperties ()
								.Where (p => p.CanWrite && p.GetCustomAttributes (typeof (RequiredAttribute), true).Any ());
							var missings = props.Where (p => !ti.Parameters.Any (tp => tp.Key.Equals (p.Name, StringComparison.OrdinalIgnoreCase)));
							if (missings.Any ())
								throw new InvalidOperationException (string.Format ("Task {0} of type {1} is used without specifying mandatory property: {2}",
									ti.Name, task.GetType (), string.Join (", ", missings.Select (p => p.Name).ToArray ())));
							foreach (var p in ti.Parameters) {
								var prop = task.GetType ().GetProperty (p.Key);
								if (prop == null)
									throw new InvalidOperationException (string.Format ("Task {0} does not have property {1}", ti.Name, p.Key));
								if (!prop.CanWrite)
									throw new InvalidOperationException (string.Format ("Task {0} has property {1} but it is read-only.", ti.Name, p.Key));
								prop.SetValue (task, ConvertTo (p.Value, prop.PropertyType), null);
							}
							if (!task.Execute ()) {
								targetResult.Failure (null);
								if (!ContinueOnError)
									break;
							}
							foreach (var to in ti.Outputs) {
								var toItem = to as ProjectTaskOutputItemInstance;
								var toProp = to as ProjectTaskOutputPropertyInstance;
								string taskParameter = toItem != null ? toItem.TaskParameter : toProp.TaskParameter;
								var pi = task.GetType ().GetProperty (taskParameter);
								if (pi == null)
									throw new InvalidOperationException (string.Format ("Task {0} does not have property {1} specified as TaskParameter", ti.Name, toItem.TaskParameter));
								if (!pi.CanRead)
									throw new InvalidOperationException (string.Format ("Task {0} has property {1} specified as TaskParameter, but it is write-only", ti.Name, toItem.TaskParameter));
								if (toItem != null)
									project.AddItem (toItem.ItemType, ConvertFrom (pi.GetValue (task, null)));
								else
									project.SetProperty (toProp.PropertyName, ConvertFrom (pi.GetValue (task, null)));
							}
						}
						Func<string,ITaskItem> creator = s => new TargetOutputTaskItem () { ItemSpec = s };
						var items = project.GetAllItems (target.Outputs, string.Empty, creator, creator, s => true, (t, s) => {});
						targetResult.Success (items);
					}
						
					result.AddResultsForTarget (targetName, targetResult);
				}

				// FIXME: check .NET behavior, whether cancellation always results in failure.
				result.OverallResult = checkCancel () ? BuildResultCode.Failure : result.ResultsByTarget.Select (p => p.Value).Any (r => r.ResultCode == TargetResultCode.Failure) ? BuildResultCode.Failure : BuildResultCode.Success;
			}
		}
		
		object ConvertTo (string source, Type targetType)
		{
			if (targetType.IsSubclassOf (typeof (ITaskItem)))
				return new TargetOutputTaskItem () { ItemSpec = source };
			if (targetType.IsArray)
				return new ArrayList (source.Split (';').Select (s => ConvertTo (s, targetType.GetElementType ())).ToArray ())
						.ToArray (targetType.GetElementType ());
			else
				return Convert.ChangeType (source, targetType);
		}
		
		string ConvertFrom (object source)
		{
			if (source == null)
				return string.Empty;
			var type = source.GetType ();
			if (type.IsSubclassOf (typeof (ITaskItem)))
				return ((ITaskItem) source).ItemSpec;
			if (type.IsArray)
				return string.Join (":", ((Array) source).Cast<object> ().Select (o => ConvertFrom (o)).ToArray ());
			else
				return (string) Convert.ChangeType (source, typeof (string));
		}
		
		class TargetOutputTaskItem : ITaskItem2
		{
			#region ITaskItem2 implementation
			public string GetMetadataValueEscaped (string metadataName)
			{
				return null;
			}
			public void SetMetadataValueLiteral (string metadataName, string metadataValue)
			{
				throw new NotSupportedException ();
			}
			public IDictionary CloneCustomMetadataEscaped ()
			{
				return new Hashtable ();
			}
			public string EvaluatedIncludeEscaped {
				get { return ProjectCollection.Escape (ItemSpec); }
				set { ItemSpec = ProjectCollection.Unescape (value); }
			}
			#endregion
			#region ITaskItem implementation
			public IDictionary CloneCustomMetadata ()
			{
				return new Hashtable ();
			}
			public void CopyMetadataTo (ITaskItem destinationItem)
			{
				// do nothing
			}
			public string GetMetadata (string metadataName)
			{
				return null;
			}
			public void RemoveMetadata (string metadataName)
			{
				// do nothing
			}
			public void SetMetadata (string metadataName, string metadataValue)
			{
				throw new NotSupportedException ();
			}
			public string ItemSpec { get; set; }
			public int MetadataCount {
				get { return 0; }
			}
			public ICollection MetadataNames {
				get { return new ArrayList (); }
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
			get {
				switch (current_task.ContinueOnError) {
				case "WarnAndContinue":
				case "ErrorAndContinue":
					return true;
				case "ErrorAndStop":
					return false;
				}
				return project.EvaluateCondition (current_task.ContinueOnError);
			}
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

