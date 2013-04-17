//
// BuildTask.cs: Represents a Task element in a project.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2006 Marek Sieradzki
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	public class BuildTask : IBuildTask {
	
		ITaskHost		hostObject;
		Target			parentTarget;
		XmlElement		taskElement;
		TaskLoggingHelper	task_logger;
	
		internal BuildTask (XmlElement taskElement, Target parentTarget)
		{
			if (taskElement == null)
				throw new ArgumentNullException ("taskElement");
			if (parentTarget == null)
				throw new ArgumentNullException ("parentTarget");

			this.taskElement =  taskElement;
			this.parentTarget = parentTarget;
		}
		
		[MonoTODO]
		public void AddOutputItem (string taskParameter,
					   string itemName)
		{
			XmlElement element = parentTarget.Project.XmlDocument.CreateElement ("Output", Project.XmlNamespace);
			taskElement.AppendChild (element);
			
			if (taskParameter != null)
				element.SetAttribute ("TaskParameter", taskParameter);
			if (itemName != null)
				element.SetAttribute ("ItemName", itemName);
		}
		
		[MonoTODO]
		public void AddOutputProperty (string taskParameter,
					       string propertyName)
		{
			XmlElement element = parentTarget.Project.XmlDocument.CreateElement ("Output", Project.XmlNamespace);
			taskElement.AppendChild (element);
			
			if (taskParameter != null)
				element.SetAttribute ("TaskParameter", taskParameter);
			if (propertyName != null)
				element.SetAttribute ("PropertyName", propertyName);
		}
		
		[MonoTODO]
		public bool Execute ()
		{
			bool		result = false;
			TaskEngine	taskEngine;

			LogTaskStarted ();
			ITask task = null;

			try {
				try {
					task = InitializeTask ();
				} catch (Exception e) {
					LogError ("Error initializing task {0}: {1}", taskElement.LocalName, e.Message);
					LogMessage (MessageImportance.Low, "Error initializing task {0}: {1}",
							taskElement.LocalName, e.ToString ());
					return false;
				}

				try {
					taskEngine = new TaskEngine (parentTarget.Project);
					taskEngine.Prepare (task, this.taskElement, GetParameters (), this.Type);
					result = taskEngine.Execute ();
					if (result)
						taskEngine.PublishOutput ();
				} catch (Exception e) {
					task_logger.LogError ("Error executing task {0}: {1}", taskElement.LocalName, e.Message);
					task_logger.LogMessage (MessageImportance.Low,
							"Error executing task {0}: {1}", taskElement.LocalName, e.ToString ());
					result = false;
				}
			} finally {
				LogTaskFinished (result);
			}

			return result;
		}


		public string[] GetParameterNames ()
		{
			List <string> tempNames = new List <string> ();
			
			foreach (XmlAttribute xmlAttribute in taskElement.Attributes) {
				if (xmlAttribute.Name == "Condition" || xmlAttribute.Name == "ContinueOnError")
					continue;
				tempNames.Add (xmlAttribute.Name);
			}

			return tempNames.ToArray ();
		}
		
		public string GetParameterValue (string attributeName)
		{
			if (attributeName == "Condition")
				throw new ArgumentException ("Condition attribute cannot be accessed using this method.");
			if (attributeName == "ContinueOnError")
				throw new ArgumentException ("ContinueOnError attribute cannot be accessed using this method.");

			return taskElement.GetAttribute (attributeName);
		}
		
		public void SetParameterValue (string parameterName,
					       string parameterValue)
		{
			SetParameterValue (parameterName, parameterValue, false);
		}
		
		public void SetParameterValue (string parameterName,
					       string parameterValue,
					       bool treatParameterValueAsLiteral)
		{
			if (treatParameterValueAsLiteral)
				taskElement.SetAttribute (parameterName, Utilities.Escape (parameterValue));
			else
				taskElement.SetAttribute (parameterName, parameterValue);
		}

		void LogTaskStarted ()
		{
			TaskStartedEventArgs tsea = new TaskStartedEventArgs ("Task started.", null,
					parentTarget.Project.FullFileName,
					parentTarget.TargetFile, taskElement.Name);
			parentTarget.Project.ParentEngine.EventSource.FireTaskStarted (this, tsea);
		}
		
		void LogTaskFinished (bool succeeded)
		{
			TaskFinishedEventArgs tfea = new TaskFinishedEventArgs ("Task finished.", null,
					parentTarget.Project.FullFileName,
					parentTarget.TargetFile, taskElement.Name, succeeded);
			parentTarget.Project.ParentEngine.EventSource.FireTaskFinished (this, tfea);
		}

		void LogError (string message,
				     params object[] messageArgs)
		{
			parentTarget.Project.ParentEngine.LogError (message, messageArgs);
		}
		
		void LogMessage (MessageImportance importance,
					string message,
					params object[] messageArgs)
		{
			parentTarget.Project.ParentEngine.LogMessage (importance, message, messageArgs);
		}

		ITask InitializeTask ()
		{
			ITask task;
			
			try {
				task = (ITask)Activator.CreateInstance (this.Type);
			} catch (InvalidCastException) {
				LogMessage (MessageImportance.Low, "InvalidCastException, ITask: {0} Task type: {1}",
						typeof (ITask).AssemblyQualifiedName, this.Type.AssemblyQualifiedName);
				throw;
			}
			parentTarget.Project.ParentEngine.LogMessage (
					MessageImportance.Low,
					"Using task {0} from {1}", Name, this.Type.AssemblyQualifiedName);

			task.BuildEngine = new BuildEngine (parentTarget.Project.ParentEngine, parentTarget.Project,
						parentTarget.TargetFile, 0, 0, ContinueOnError);
			task_logger = new TaskLoggingHelper (task);
			
			return task;
		}
		
		IDictionary <string, string> GetParameters ()
		{
			Dictionary <string, string> parameters = new Dictionary <string, string> ();
			
			string[] parameterNames = GetParameterNames ();
			
			foreach (string s in parameterNames)
				parameters.Add (s, GetParameterValue (s));
			
			return parameters;
		}
		
		public string Condition {
			get {
				return taskElement.GetAttribute ("Condition");
			}
			set {
				taskElement.SetAttribute ("Condition", value);
			}
		}

		[MonoTODO]
		public bool ContinueOnError {
			get {
				string str = taskElement.GetAttribute ("ContinueOnError");
				if (str == String.Empty)
					return false;
				else {
					Expression exp = new Expression ();
					exp.Parse (str, ParseOptions.AllowItemsNoMetadataAndSplit);
					return (bool) exp.ConvertTo (parentTarget.Project, typeof (bool),
							ExpressionOptions.ExpandItemRefs);
				}
			}
			set {
				taskElement.SetAttribute ("ContinueOnError", value.ToString ());
			}
		}
		
		[MonoTODO]
		public ITaskHost HostObject {
			get { return hostObject; }
			set { hostObject = value; }
		}
		
		public string Name {
			get { return taskElement.Name; }
		}
		
		internal Target ParentTarget {
			get { return parentTarget; }
			set { parentTarget = value; }
		}
		
		internal XmlElement TaskElement {
			get { return taskElement; }
			set { taskElement = value; }
		}

		[MonoTODO]
		public Type Type {
			get { return parentTarget.Project.TaskDatabase.GetTypeFromClassName (Name); }
		}

		public IEnumerable<string> GetAttributes ()
		{
			foreach (XmlAttribute attrib in TaskElement.Attributes)
				yield return attrib.Value;
		
			foreach (XmlNode xn in TaskElement.ChildNodes) {
				XmlElement xe = xn as XmlElement;
				if (xe == null)
					continue;
			
				//FIXME: error on any other child
				if (String.Compare (xe.LocalName, "Output", StringComparison.Ordinal) == 0) {
					foreach (XmlAttribute attrib in xe.Attributes)
						yield return attrib.Value;
				}
			}
		}

	}
}
