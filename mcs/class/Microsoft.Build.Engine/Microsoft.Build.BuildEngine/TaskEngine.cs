//
// TaskEngine.cs: Class that executes each task.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Ankit Jain (jankit@novell.com)
// 
// (C) 2005 Marek Sieradzki
// Copyright 2009 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.BuildEngine {
	internal class TaskEngine {
		
		ITask		task;
		XmlElement	taskElement;
		Type		taskType;
		Project		parentProject;
		
		static Type	requiredAttribute;
		static Type	outputAttribute;
		
		static TaskEngine ()
		{
			requiredAttribute = typeof (Microsoft.Build.Framework.RequiredAttribute);
			outputAttribute = typeof (Microsoft.Build.Framework.OutputAttribute);
		}

		public TaskEngine (Project project)
		{
			parentProject = project;
		}

		// Rules (inferred) for property values incase of empty data
		//
		// Prop Type         Argument         Final Value     Required
		// string	     empty string	null	       Yes/no
		// string	     only whitespace    @arg	       Yes/no
		//
		// string/
		//   ITaskItem[]     empty/whitespace   empty array     Yes
		//
		// string/
		//   ITaskItem[]     empty/whitespace   null	        No
		
		public void Prepare (ITask task, XmlElement taskElement,
				     IDictionary <string, string> parameters, Type taskType)
		{
			Dictionary <string, object>	values;
			PropertyInfo	currentProperty;
			PropertyInfo[]	properties;
			object		value;
			
			this.task = task;
			this.taskElement = taskElement;
			this.taskType = taskType;
			values = new Dictionary <string, object> (StringComparer.InvariantCultureIgnoreCase);
			
			foreach (KeyValuePair <string, string> de in parameters) {
				currentProperty = taskType.GetProperty (de.Key, BindingFlags.Public | BindingFlags.Instance
						| BindingFlags.IgnoreCase);
				if (currentProperty == null)
					throw new InvalidProjectFileException (String.Format ("Task does not have property \"{0}\" defined",
						de.Key));
				
				try {
					if (TryGetObjectFromString (de.Value, currentProperty.PropertyType, out value))
						values.Add (de.Key, value);
				} catch (Exception e) {
					throw new InvalidProjectFileException (String.Format (
							"Error converting Property named '{0}' with value '{1}' to type {2}: {3}",
							de.Key, de.Value, currentProperty.PropertyType, e.Message), e);
				}
			}
			
			properties = taskType.GetProperties ();
			foreach (PropertyInfo pi in properties) {
				bool is_required = pi.IsDefined (requiredAttribute, false);

				if (is_required && values.ContainsKey (pi.Name) == false)
					throw new InvalidProjectFileException (String.Format ("Required property '{0}' not set.",
						pi.Name));

				if (!values.ContainsKey (pi.Name))
					continue;

				Type prop_type = pi.PropertyType;
				if (prop_type.IsArray)
					prop_type = prop_type.GetElementType ();

				// Valid data types: primitive types, DateTime, string and ITaskItem, and their arrays
				if (!prop_type.IsPrimitive && prop_type != typeof (string) && prop_type != typeof (ITaskItem))
					throw new InvalidProjectFileException (String.Format (
							"{0} is not a supported type for properties for msbuild tasks.",
							pi.PropertyType));

				object val = values [pi.Name];
				if (val == null && pi.PropertyType.IsArray && is_required) {
					if (pi.PropertyType == typeof (ITaskItem[]))
						val = new ITaskItem [0];
					else if (pi.PropertyType == typeof (string[]))
						val = new string [0];
				}

				InitializeParameter (pi, val);
			}
		}
		
		public bool Execute ()
		{
			return task.Execute ();
		}
		
		public void PublishOutput ()
		{
			XmlElement	xmlElement;
			PropertyInfo	propertyInfo;
			string		propertyName;
			string		taskParameter;
			string		itemName;
			object		o;
		
			foreach (XmlNode xmlNode in taskElement.ChildNodes) {
				if (!(xmlNode is XmlElement))
					continue;
			
				xmlElement = (XmlElement) xmlNode;
				
				if (xmlElement.Name != "Output")
					throw new InvalidProjectFileException ("Only Output elements can be Task's child nodes.");
				if (xmlElement.GetAttribute ("ItemName") != String.Empty && xmlElement.GetAttribute ("PropertyName") != String.Empty)
					throw new InvalidProjectFileException ("Only one of ItemName and PropertyName attributes can be specified.");
				if (xmlElement.GetAttribute ("TaskParameter") == String.Empty)
					throw new InvalidProjectFileException ("TaskParameter attribute must be specified.");

				if (!ConditionParser.ParseAndEvaluate (xmlElement.GetAttribute ("Condition"), parentProject))
					continue;
					
				taskParameter = xmlElement.GetAttribute ("TaskParameter");
				itemName = xmlElement.GetAttribute ("ItemName");
				propertyName = xmlElement.GetAttribute ("PropertyName");
				
				propertyInfo = taskType.GetProperty (taskParameter, BindingFlags.Public | BindingFlags.Instance |
							BindingFlags.IgnoreCase);
				if (propertyInfo == null)
					throw new InvalidProjectFileException (String.Format (
						"The parameter '{0}' was not found for the '{1}' task.", taskParameter, taskElement.Name));
				if (!propertyInfo.IsDefined (outputAttribute, false))
					throw new InvalidProjectFileException ("This is not output property.");
				
				o = propertyInfo.GetValue (task, null);
				// FIXME: maybe we should throw an exception here?
				if (o == null)
					continue;
				
				if (itemName != String.Empty) {
					PublishItemGroup (propertyInfo, o, itemName);
				} else {
					PublishProperty (propertyInfo, o, propertyName);
				}
			}
		}
		
		void InitializeParameter (PropertyInfo propertyInfo, object value)
		{
			propertyInfo.SetValue (task, value, null);
		}

		void PublishProperty (PropertyInfo propertyInfo,
					      object o,
					      string propertyName)
		{
			BuildProperty bp;
			try {
				bp = ChangeType.ToBuildProperty (o, propertyInfo.PropertyType, propertyName);
			} catch (Exception e) {
				throw new Exception (String.Format ("Error publishing Output from task property '{0} {1}' to property named '{2}' : {3}",
							propertyInfo.PropertyType, propertyInfo.Name, propertyName, e.Message),
							e);
			}
			parentProject.EvaluatedProperties.AddProperty (bp);
		}

		// FIXME: cleanup + test
		void PublishItemGroup (PropertyInfo propertyInfo,
					       object o,
					       string itemName)
		{
			BuildItemGroup newItems;
			try {
				newItems = ChangeType.ToBuildItemGroup (o, propertyInfo.PropertyType, itemName);
			} catch (Exception e) {
				throw new Exception (String.Format ("Error publishing Output from task property '{0} {1}' to item named '{2}' : {3}",
							propertyInfo.PropertyType, propertyInfo.Name, itemName, e.Message),
							e);
			}

			newItems.ParentProject = parentProject;
			
			if (parentProject.EvaluatedItemsByName.ContainsKey (itemName)) {
				BuildItemGroup big = parentProject.EvaluatedItemsByName [itemName];
				foreach (BuildItem item in newItems)
					big.AddItem (item);
			} else {
				parentProject.EvaluatedItemsByName.Add (itemName, newItems);
			}
			foreach (BuildItem bi in newItems)
				parentProject.EvaluatedItems.AddItem (bi);
		}

		// returns true, if the @result should be included in the values list
		bool TryGetObjectFromString (string raw, Type type, out object result)
		{
			Expression e;
			result = null;
			
			e = new Expression ();
			e.Parse (raw, ParseOptions.AllowItemsMetadataAndSplit);

			// See rules in comment for 'Prepare'
			string str = (string) e.ConvertTo (parentProject, typeof (string));
			if (!type.IsArray && str == String.Empty)
				return false;

			if (str.Trim ().Length == 0 && type.IsArray &&
				(type.GetElementType () == typeof (string) || type.GetElementType () == typeof (ITaskItem)))
				return true;

			result = e.ConvertTo (parentProject, type, ExpressionOptions.ExpandItemRefs);
			
			return true;
		}
	}
}

#endif
