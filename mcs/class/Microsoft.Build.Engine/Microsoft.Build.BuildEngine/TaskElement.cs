//
// TaskElement.cs: Represents XML element that is a task.
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
using System.Collections.Specialized;
using System.Reflection;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	public class TaskElement {
	
		XmlAttribute	condition;
		XmlAttribute	continueOnError;
		XmlElement	taskElement;
		Target		parentTarget;
		object		hostObject;
		string		name;
		bool		isImported;
		
		static Type	requiredAttribute;
		static Type	outputAttribute;
		
		public TaskElement ()
		{
			this.isImported = false;
		}
		
		static TaskElement ()
		{
			requiredAttribute = typeof (Microsoft.Build.Framework.RequiredAttribute);
			outputAttribute = typeof (Microsoft.Build.Framework.OutputAttribute);
		}

		public bool Execute ()
		{
			bool result;
			string rawParameter;
			string[] parameterNames;
			PropertyInfo[] properties;
			PropertyInfo currentProperty;
			Hashtable values = new Hashtable ();
			Task task;
			object value;
		
			LogTaskStarted ();
			
			task = (Task)Activator.CreateInstance (Type);
			task.BuildEngine = new BuildEngine (parentTarget.Project.ParentEngine, 0, 0,
				ContinueOnError, parentTarget.Project.FullFileName);
			parameterNames = GetParameterNames ();
			foreach (string parameter in parameterNames) {
				currentProperty = GetType ().GetProperty (parameter);
				if (currentProperty == null)
					throw new InvalidProjectFileException (String.Format ("Task does not have property \"{0}\" defined",
						parameter));
				rawParameter = GetParameterValue (parameter);
				value = GetObjectFromString (rawParameter, currentProperty.PropertyType);
				values.Add (parameter, value);
			}
			
			properties = GetType().GetProperties ();
			foreach (PropertyInfo pi in properties) {
				if (pi.IsDefined (requiredAttribute, false) && values.ContainsKey (pi.Name) == false) {
					throw new InvalidProjectFileException ("Required property not set.");
				}
				if (values.ContainsKey (pi.Name)) {
					pi.SetValue (task, values [pi.Name], null);
				}
			}
			
			result = task.Execute ();
			
			// output
			
			foreach (XmlNode xn in taskElement.ChildNodes) {
				if (xn is XmlElement) {
					XmlElement xe = (XmlElement) xn;
					PropertyInfo pi;
					string property, parameter, item;
					object o;
					Project p;
					
					if (xe.Name != "Output")
						throw new InvalidProjectFileException ("Only Output elements can be Task's child nodes.");
					if (xe.GetAttribute ("ItemName") != "" && xe.GetAttribute ("PropertyName") != "")
						throw new InvalidProjectFileException ("Only one of ItemName and ProperytyName attributes can be specified.");
					if (xe.GetAttribute ("TaskParameter") == "")
						throw new InvalidProjectFileException ("TaskParameter attribute must be specified.");
						
					property = xe.GetAttribute ("PropertyName");
					parameter = xe.GetAttribute ("TaskParameter");
					item = xe.GetAttribute ("ItemName");
					p = parentTarget.Project;
					
					pi = GetType ().GetProperty (parameter);
					if (pi == null)
						throw new Exception ("Could not get property info.");
					if (pi.IsDefined (outputAttribute, false) == false)
						throw new Exception ("This is not output property.");
					
					o = pi.GetValue (task, null);
					if (o == null)
						continue;
					if (xe.GetAttribute ("ItemName") != "") {
						BuildItemGroup newItems = HandleItemGroup (pi, o, item);
						if (p.EvaluatedItemsByName.Contains (item)) {
							BuildItemGroup big = (BuildItemGroup) p.EvaluatedItemsByName [item];
							big.Clear ();
							p.EvaluatedItemsByName.Remove (item);
							p.EvaluatedItemsByName.Add (item, newItems);
						} else {
							p.EvaluatedItemsByName.Add (item, newItems);
						}
					} else {
						BuildProperty bp = HandleProperty (pi, o, property);
						p.EvaluatedProperties.AddFromExistingProperty (bp);
					}
				}
			}
			
			// end of output
			
			LogTaskFinished (result);
			
			return result;
		}
		
		private BuildProperty HandleProperty (PropertyInfo propertyInfo, object o, string name)
		{
			string output = null;
			BuildProperty bp;
			
			if (propertyInfo == null)
				throw new ArgumentNullException ("propertyInfo");
			if (o == null)
				throw new ArgumentNullException ("o");
			if (name == null)
				throw new ArgumentNullException ("name");
			
			if (propertyInfo.PropertyType == typeof (ITaskItem)) {
				ITaskItem item = (ITaskItem) o;
				bp = ChangeType.TransformToBuildProperty (name, item);
			} else if (propertyInfo.PropertyType == typeof (ITaskItem[])) {
				ITaskItem[] items = (ITaskItem[]) o;
				bp = ChangeType.TransformToBuildProperty (name, items);
			} else {
				if (propertyInfo.PropertyType.IsArray == true) {
					output = ChangeType.TransformToString ((object[])o, propertyInfo.PropertyType);
			} else {
					output = ChangeType.TransformToString (o, propertyInfo.PropertyType);
				}
				bp = ChangeType.TransformToBuildProperty (name, output);
			}
			return bp;
		}
		
		private BuildItemGroup HandleItemGroup (PropertyInfo propertyInfo, object o, string name)
		{
			BuildItemGroup big;
			string temp;
			
			if (propertyInfo == null)
				throw new ArgumentNullException ("propertyInfo");
			if (o == null)
				throw new ArgumentNullException ("o");
			if (name == null)
				throw new ArgumentNullException ("name");
				
			if (propertyInfo.PropertyType == typeof (ITaskItem)) {
				ITaskItem item = (ITaskItem) o;
				big = ChangeType.TransformToBuildItemGroup (name, item);
			} else if (propertyInfo.PropertyType == typeof (ITaskItem[])) {
				ITaskItem[] items = (ITaskItem[]) o;
				big = ChangeType.TransformToBuildItemGroup (name, items);
			} else {
				if (propertyInfo.PropertyType.IsArray == true) {
					temp = ChangeType.TransformToString ((object[]) o, propertyInfo.PropertyType);
				} else {
					temp = ChangeType.TransformToString (o, propertyInfo.PropertyType);
				}
				big = ChangeType.TransformToBuildItemGroup (name, temp);
			}
			return big;	
		}

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

		public string GetParameterValue (string attributeName)
		{
			return taskElement.GetAttribute (attributeName);
		}
		
		private object GetObjectFromString (string raw, Type type)
		{
			Expression e;
			object result;
			
			e = new Expression (parentTarget.Project, raw);
			
			if (type == typeof (ITaskItem)) {
				result = (object) e.ToITaskItem ();
			} else if (type == typeof (ITaskItem[])) {
				result = (object) e.ToITaskItemArray ();
			} else {
				if (type.IsArray) {
					result = e.ToArray (type);
				} else {
					result = e.ToNonArray (type);
				}
			}
			
			return result;
		}

		public void SetParameterValue (string parameterName,
					       string parameterValue)
		{
			taskElement.SetAttribute (parameterName, parameterValue);
		}
		
		internal void BindToXml (XmlElement taskElement,
					 Target parentTarget)
		{
			if (taskElement == null)
				throw new ArgumentNullException ("taskElement");
			if (parentTarget == null)
				throw new ArgumentNullException ("parentTarget");
			this.taskElement =  taskElement;
			this.parentTarget = parentTarget;
			this.condition = taskElement.GetAttributeNode ("Condition");
			this.continueOnError = taskElement.GetAttributeNode ("ContinueOnError");
			this.name  = taskElement.Name;
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
		
		private new Type GetType ()
		{
			return parentTarget.Project.TaskDatabase.GetTypeFromClassName (name);
		}

		public string Condition {
			get {
				return condition.Value;
			}
			set {
				condition.Value = value;
			}
		}

		public bool ContinueOnError {
			get {
				// FIXME: insert here text parsing
				if (continueOnError == null)
					return false;
				else
					return Boolean.Parse (continueOnError.Value);
			}
			set {
				continueOnError.Value = value.ToString ();
			}
		}

		public object HostObject {
			get {
				return hostObject;
			}
			set {
				hostObject = value;
			}
		}

		public string Name {
			get {
				return name;
			}
		}

		public Type Type {
			get {
				return GetType ();
			}
		}
	}
}

#endif