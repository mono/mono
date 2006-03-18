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
using System.Collections.Specialized;
using System.Reflection;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	public class BuildTask {
	
		XmlAttribute		condition;
		XmlAttribute		continueOnError;
		ITaskHost		hostObject;
		string			name;
		Target			parentTarget;
		XmlElement		taskElement;
		Type			type;
	
		// FIXME: implement
		internal BuildTask (XmlElement taskElement, Target parentTarget)
		{
			//if (taskElement == null)
			//	throw new ArgumentNullException ("taskElement");
			if (parentTarget == null)
				throw new ArgumentNullException ("parentTarget");
			if (taskElement != null) {
				this.taskElement =  taskElement;
				this.parentTarget = parentTarget;
				this.condition = taskElement.GetAttributeNode ("Condition");
				this.continueOnError = taskElement.GetAttributeNode ("ContinueOnError");
				this.name  = taskElement.Name;
			}
		}
		
		[MonoTODO]
		public void AddOutputItem (string taskParameter,
					   string itemName)
		{
		}
		
		[MonoTODO]
		public void AddOutputProperty (string taskParameter,
					       string propertyName)
		{
		}
		
		[MonoTODO]
		public bool Execute ()
		{
			bool		result;
			TaskEngine	taskEngine;

			LogTaskStarted ();
			
			taskEngine = new TaskEngine (parentTarget.Project);
			
			taskEngine.Prepare (InitializeTask (), this.taskElement,GetParameters (), this.Type);
			
			result = taskEngine.Execute ();
			
			taskEngine.PublishOutput ();
			
			LogTaskFinished (result);
		
			return result;
		}


		[MonoTODO]
		public string[] GetParameterNames ()
		{
			int attributesCount = 0;
			ArrayList tempNames = new ArrayList ();
			string[] names;
			
			foreach (XmlAttribute xmlAttribute in taskElement.Attributes) {
				if (xmlAttribute.Name == "Condition")
					continue;
				if (xmlAttribute.Name == "ContinueOnError")
					continue;
				tempNames.Add (xmlAttribute.Name);
			}
			names = new string [tempNames.Count];
			foreach (string name in tempNames)
				names [attributesCount++] = name;
			return names;
		}
		
		[MonoTODO]
		public string GetParameterValue (string attributeName)
		{
			return taskElement.GetAttribute (attributeName);
		}
		
		[MonoTODO]
		public void SetParameterValue (string parameterName,
					       string parameterValue)
		{
			SetParameterValue (parameterName, parameterValue, false);
		}
		
		[MonoTODO]
		public void SetParameterValue (string parameterName,
					       string parameterValue,
					       bool treatParameterValueAsLiteral)
		{
			// FIXME: use expression for parameterValue
			taskElement.SetAttribute (parameterName, parameterValue);
		}
		
		private void LogTaskStarted ()
		{
			TaskStartedEventArgs tsea = new TaskStartedEventArgs ("Task started.", null, parentTarget.Project.FullFileName,
				parentTarget.Project.FullFileName, taskElement.Name);
			parentTarget.Project.ParentEngine.EventSource.FireTaskStarted (this, tsea);
		}
		
		private void LogTaskFinished (bool succeeded)
		{
			TaskFinishedEventArgs tfea = new TaskFinishedEventArgs ("Task finished.", null, parentTarget.Project.FullFileName,
				parentTarget.Project.FullFileName, taskElement.Name, succeeded);
			parentTarget.Project.ParentEngine.EventSource.FireTaskFinished (this, tfea);
		}
		
		private ITask InitializeTask ()
		{
			ITask task;
			
			task = (ITask)Activator.CreateInstance (this.Type);
			task.BuildEngine = new BuildEngine (parentTarget.Project.ParentEngine, 0, 0, ContinueOnError,
				parentTarget.Project.FullFileName);
			
			return task;
		}
		
		private IDictionary GetParameters ()
		{
			IDictionary parameters = new Hashtable ();
			
			string[] parameterNames = GetParameterNames ();
			
			foreach (string s in parameterNames) {
				parameters.Add (s, GetParameterValue (s));
			}
			
			return parameters;
		}
		
		[MonoTODO]
		public string Condition {
			get { return condition.Value; }
			set { condition.Value = value; }
		}

		[MonoTODO]
		public bool ContinueOnError {
			get {
				if (continueOnError == null)
					return false;
				else
					return Boolean.Parse (continueOnError.Value);
			}
			set {
				continueOnError.Value = value.ToString ();
			}
		}
		
		[MonoTODO]
		public ITaskHost HostObject {
			get { return hostObject; }
			set { hostObject = value; }
		}
		
		[MonoTODO]
		public string Name {
			get { return name; }
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
			get { return parentTarget.Project.TaskDatabase.GetTypeFromClassName (name); }
		}
		
	}
}

#endif
