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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	public class BuildTask {
	
		ITaskHost		hostObject;
		Target			parentTarget;
		XmlElement		taskElement;
		TaskLoggingHelper	logger;
	
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
			bool		result;
			TaskEngine	taskEngine;

			LogTaskStarted ();

			try {
				taskEngine = new TaskEngine (parentTarget.Project);		
				taskEngine.Prepare (InitializeTask (), this.taskElement, GetParameters (), this.Type);
				result = taskEngine.Execute ();
				if (result)
					taskEngine.PublishOutput ();
			} catch (Exception e) {
				logger.LogError ("Error executing task {0}: {1}", taskElement.LocalName, e.Message);
				logger.LogMessage (MessageImportance.Low,
						"Error executing task {0}: {1}", taskElement.LocalName, e.ToString ());
				result = false;
			}

			LogTaskFinished (result);
		
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
			TaskStartedEventArgs tsea = new TaskStartedEventArgs ("Task started.", null, parentTarget.Project.FullFileName,
				parentTarget.Project.FullFileName, taskElement.Name);
			parentTarget.Project.ParentEngine.EventSource.FireTaskStarted (this, tsea);
		}
		
		void LogTaskFinished (bool succeeded)
		{
			TaskFinishedEventArgs tfea = new TaskFinishedEventArgs ("Task finished.", null, parentTarget.Project.FullFileName,
				parentTarget.Project.FullFileName, taskElement.Name, succeeded);
			parentTarget.Project.ParentEngine.EventSource.FireTaskFinished (this, tfea);
		}
		
		ITask InitializeTask ()
		{
			ITask task;
			
			task = (ITask)Activator.CreateInstance (this.Type);
			task.BuildEngine = new BuildEngine (parentTarget.Project.ParentEngine, parentTarget.Project, 0, 0, ContinueOnError);
			logger = new TaskLoggingHelper (task);
			
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
					exp.Parse (str, true);
					return (bool) exp.ConvertTo (parentTarget.Project, typeof (bool));
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
		
	}
}

#endif
