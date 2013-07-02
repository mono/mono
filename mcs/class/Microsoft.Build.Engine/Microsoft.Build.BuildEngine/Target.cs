//
// Target.cs: Represents a target.
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
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.XBuild.Utilities;

namespace Microsoft.Build.BuildEngine {
	public class Target : IEnumerable {
	
		TargetBatchingImpl batchingImpl;
		BuildState	buildState;
		Engine		engine;
		ImportedProject	importedProject;
		string		name;
		Project		project;
		XmlElement	targetElement;
		List <XmlElement>	onErrorElements;
		List <IBuildTask>	buildTasks;
		
		internal Target (XmlElement targetElement, Project project, ImportedProject importedProject)
		{
			if (project == null)
				throw new ArgumentNullException ("project");
			if (targetElement == null)
				throw new ArgumentNullException ("targetElement");

			this.targetElement = targetElement;
			this.name = targetElement.GetAttribute ("Name");

			this.project = project;
			this.engine = project.ParentEngine;
			this.importedProject = importedProject;

			this.onErrorElements  = new List <XmlElement> ();
			this.buildState = BuildState.NotStarted;
			this.buildTasks = new List <IBuildTask> ();
			this.batchingImpl = new TargetBatchingImpl (project, this.targetElement);

			bool onErrorFound = false;
			foreach (XmlNode xn in targetElement.ChildNodes) {
				if (xn is XmlElement) {
					XmlElement xe = (XmlElement) xn;
					if (xe.Name == "OnError") {
						onErrorElements.Add (xe);
						onErrorFound = true;
					} else if (onErrorFound)
						throw new InvalidProjectFileException (
							"The element <OnError> must be last under element <Target>. Found element <Error> instead.");
#if NET_3_5
					else if (xe.Name == "ItemGroup") {
						var group = new BuildTaskItemGroup (xe, this);
						buildTasks.AddRange (group.Items);
						continue;
					} else if (xe.Name == "PropertyGroup") {
						buildTasks.Add (new BuildTaskPropertyGroup (xe, this));
						continue;
					}
#endif
					else
						buildTasks.Add (new BuildTask (xe, this));
				}
			}
		}

		[MonoTODO]
		public BuildTask AddNewTask (string taskName)
		{
			if (taskName == null)
				throw new ArgumentNullException ("taskName");
		
			XmlElement task = project.XmlDocument.CreateElement (taskName, Project.XmlNamespace);
			targetElement.AppendChild (task);
			BuildTask bt = new BuildTask (task, this);
			buildTasks.Add (bt);
			
			return bt;
		}

		public IEnumerator GetEnumerator ()
		{
			return buildTasks.ToArray ().GetEnumerator ();
		}

		// FIXME: shouldn't we remove it from XML?
		public void RemoveTask (BuildTask buildTask)
		{
			if (buildTask == null)
				throw new ArgumentNullException ("buildTask");
			buildTasks.Remove (buildTask);
		}

		bool Build ()
		{
			return Build (null);
		}

		internal bool Build (string built_targets_key)
		{
			bool executeOnErrors;
			return Build (built_targets_key, out executeOnErrors);
		}

		bool Build (string built_targets_key, out bool executeOnErrors)
		{
			project.PushThisFileProperty (TargetFile);
			try {
				return BuildActual (built_targets_key, out executeOnErrors);
			} finally {
				project.PopThisFileProperty ();
			}
		}

		bool BuildActual (string built_targets_key, out bool executeOnErrors)
		{
			bool result = false;
			executeOnErrors = false;

			// built targets are keyed by the particular set of global
			// properties. So, a different set could allow a target
			// to run again
			built_targets_key = project.GetKeyForTarget (Name);
			if (project.ParentEngine.BuiltTargetsOutputByName.ContainsKey (built_targets_key)) {
				LogTargetSkipped ();
				return true;
			}

			// Push a null/empty batch, effectively clearing it
			project.PushBatch (null, null);
			if (!ConditionParser.ParseAndEvaluate (Condition, Project)) {
				LogMessage (MessageImportance.Low,
						"Target {0} skipped due to false condition: {1}",
						Name, Condition);
				project.PopBatch ();
				return true;
			}

			try {
				buildState = BuildState.Started;

#if NET_4_0
				result = BuildDependencies (out executeOnErrors) &&
						BuildBeforeThisTargets (out executeOnErrors) &&
						DoBuild (out executeOnErrors) && // deps & Before targets built fine, do main build
						BuildAfterThisTargets (out executeOnErrors);
#else
				result = BuildDependencies (out executeOnErrors) && DoBuild (out executeOnErrors);
#endif

				buildState = BuildState.Finished;
			} catch (Exception e) {
				LogError ("Error building target {0}: {1}", Name, e.ToString ());
				return false;
			} finally {
				project.PopBatch ();
			}

			ITaskItem[] outputs = (ITaskItem[]) OutputsAsITaskItems.Clone ();
			foreach (ITaskItem item in outputs) {
				item.SetMetadata ("MSBuildProjectFile", TargetFile);
				item.SetMetadata ("MSBuildTargetName", Name);
			}
			project.ParentEngine.BuiltTargetsOutputByName [built_targets_key] = outputs;

			return result;
		}

		bool BuildDependencies (out bool executeOnErrors)
		{
			executeOnErrors = false;

			if (String.IsNullOrEmpty (DependsOnTargets))
				return true;

			var expr = new Expression ();
			expr.Parse (DependsOnTargets, ParseOptions.AllowItemsNoMetadataAndSplit);
			string [] targetNames = (string []) expr.ConvertTo (Project, typeof (string []));

			bool result = BuildOtherTargets (targetNames,
							tname => engine.LogError ("Target '{0}', a dependency of target '{1}', not found.",
										tname, Name),
							out executeOnErrors);
			if (!result && executeOnErrors)
				ExecuteOnErrors ();

			return result;
		}

#if NET_4_0
		bool BuildBeforeThisTargets (out bool executeOnErrors)
		{
			executeOnErrors = false;
			bool result = BuildOtherTargets (BeforeThisTargets, null, out executeOnErrors);
			if (!result && executeOnErrors)
				ExecuteOnErrors ();

			return result;
		}

		bool BuildAfterThisTargets (out bool executeOnErrors)
		{
			executeOnErrors = false;
			//missing_target handler not required as these are picked from actual target's
			//"Before/AfterTargets attributes!
			bool result = BuildOtherTargets (AfterThisTargets, null, out executeOnErrors);
			if (!result && executeOnErrors)
				ExecuteOnErrors ();

			return result;
		}
#endif

		bool BuildOtherTargets (IEnumerable<string> targetNames, Action<string> missing_target, out bool executeOnErrors)
		{
			executeOnErrors = false;
			if (targetNames == null)
				// nothing to build
				return true;

			foreach (string target_name in targetNames) {
				var t = project.Targets [target_name.Trim ()];
				if (t == null) {
					if (missing_target != null)
						missing_target (target_name);
					return false;
				}

				if (t.BuildState == BuildState.NotStarted)
					if (!t.Build (null, out executeOnErrors))
						return false;

				if (t.BuildState == BuildState.Started)
					throw new InvalidProjectFileException ("Cycle in target dependencies detected");
			}

			return true;
		}
		
		bool DoBuild (out bool executeOnErrors)
		{
			executeOnErrors = false;
			bool result = true;

			if (BuildTasks.Count == 0)
				// nothing to do
				return true;
		
			try {
				result = batchingImpl.Build (this, out executeOnErrors);
			} catch (Exception e) {
				LogError ("Error building target {0}: {1}", Name, e.Message);
				LogMessage (MessageImportance.Low, "Error building target {0}: {1}", Name, e.ToString ());
				return false;
			}

			if (executeOnErrors == true)
				ExecuteOnErrors ();
				
			return result;
		}
		
		void ExecuteOnErrors ()
		{
			foreach (XmlElement onError in onErrorElements) {
				if (onError.GetAttribute ("ExecuteTargets") == String.Empty)
					throw new InvalidProjectFileException ("ExecuteTargets attribute is required in OnError element.");

				string on_error_condition = onError.GetAttribute ("Condition");
				if (!ConditionParser.ParseAndEvaluate (on_error_condition, Project)) {
					LogMessage (MessageImportance.Low,
						"OnError for target {0} skipped due to false condition: {1}",
						Name, on_error_condition);
					continue;
				}

				string[] targetsToExecute = onError.GetAttribute ("ExecuteTargets").Split (';');
				foreach (string t in targetsToExecute)
					this.project.Targets [t].Build ();
			}
		}

		void LogTargetSkipped ()
		{
			BuildMessageEventArgs bmea;
			bmea = new BuildMessageEventArgs (String.Format (
						"Target {0} skipped, as it has already been built.", Name),
					null, null, MessageImportance.Low);

			project.ParentEngine.EventSource.FireMessageRaised (this, bmea);
		}

		void LogError (string message, params object [] messageArgs)
		{
			if (message == null)
				throw new ArgumentException ("message");

			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				null, null, null, 0, 0, 0, 0, String.Format (message, messageArgs),
				null, null);
			engine.EventSource.FireErrorRaised (this, beea);
		}

		void LogMessage (MessageImportance importance, string message, params object [] messageArgs)
		{
			if (message == null)
				throw new ArgumentNullException ("message");

			BuildMessageEventArgs bmea = new BuildMessageEventArgs (
				String.Format (message, messageArgs), null,
				null, importance);
			engine.EventSource.FireMessageRaised (this, bmea);
		}
	
		public string Condition {
			get { return targetElement.GetAttribute ("Condition"); }
			set { targetElement.SetAttribute ("Condition", value); }
		}

		public string DependsOnTargets {
			get { return targetElement.GetAttribute ("DependsOnTargets"); }
			set { targetElement.SetAttribute ("DependsOnTargets", value); }
		}

		public bool IsImported {
			get { return importedProject != null; }
		}

		public string Name {
			get { return name; }
		}
		
		internal Project Project {
			get { return project; }
		}

		internal string TargetFile {
			get {
				if (importedProject != null)
					return importedProject.FullFileName;
				return project != null ? project.FullFileName : String.Empty;
			}
		}

#if NET_4_0
		internal string BeforeTargets {
			get { return targetElement.GetAttribute ("BeforeTargets"); }
		}

		internal string AfterTargets {
			get { return targetElement.GetAttribute ("AfterTargets"); }
		}

		internal List<string> BeforeThisTargets { get; set; }
		internal List<string> AfterThisTargets { get; set; }
#endif

		internal List<IBuildTask> BuildTasks {
			get { return buildTasks; }
		}

		internal Engine Engine {
			get { return engine; }
		}
		
		internal BuildState BuildState {
			get { return buildState; }
		}

		public string Outputs {
			get { return targetElement.GetAttribute ("Outputs"); }
			set { targetElement.SetAttribute ("Outputs", value); }
		}

		ITaskItem [] OutputsAsITaskItems {
			get {
				string outputs = targetElement.GetAttribute ("Outputs");
				if (outputs == String.Empty)
					return new ITaskItem [0];

				Expression e = new Expression ();
				e.Parse (outputs, ParseOptions.AllowItemsNoMetadataAndSplit);

				return (ITaskItem []) e.ConvertTo (project, typeof (ITaskItem []));
			}
		}
	}
	
	internal enum BuildState {
		NotStarted,
		Started,
		Finished,
		Skipped
	}
}
