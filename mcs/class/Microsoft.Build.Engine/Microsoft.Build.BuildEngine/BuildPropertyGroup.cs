//
// BuildPropertyGroup.cs: Represents a group of properties
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
using System.Text;
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	public class BuildPropertyGroup : IEnumerable {
	
		XmlElement		propertyGroup;
		XmlAttribute		condition;
		string			importedFromFilename;
		bool			isImported;
		GroupingCollection	parentCollection;
		Project			parentProject;
		IList			properties;
		IDictionary		propertiesByName;
	
		public BuildPropertyGroup ()
			: this (true, null)
		{
		}
		
		internal BuildPropertyGroup (bool forXml, Project project)
		{
			this.propertyGroup = null;
			this.condition = null;
			this.importedFromFilename = null;
			this.isImported = false;
			this.parentCollection = null;
			this.parentProject = project;
			if (forXml == true)
				this.properties = new ArrayList ();
			else
				this.propertiesByName = CollectionsUtil.CreateCaseInsensitiveHashtable ();
		}

		public BuildProperty AddNewProperty (string propertyName,
						     string propertyValue)
		{
			return AddNewProperty (propertyName, propertyValue, false);
		}
		
		public BuildProperty AddNewProperty (string propertyName,
						     string propertyValue,
						     bool treatPropertyValueAsLiteral)
		{
			return AddNewProperty (propertyName, propertyValue, treatPropertyValueAsLiteral,
				PropertyType.Normal);
		}
		
		// FIXME: use treatPropertyValueAsLiteral
		internal BuildProperty AddNewProperty (string propertyName,
						       string propertyValue,
						       bool treatPropertyValueAsLiteral,
						       PropertyType propertyType)
		{
			BuildProperty added, existing;
			
			added = new BuildProperty (propertyName, propertyValue);
			added.PropertyType = propertyType;
			if (properties != null) {
				properties.Add (added);
			} else if (propertiesByName != null) {
				if (propertiesByName.Contains (propertyName) == true) {
					existing = (BuildProperty) propertiesByName [added.Name];
					if (added.PropertyType <= existing.PropertyType) {
						propertiesByName.Remove (added.Name);
						propertiesByName.Add (added.Name, added);
					}
				} else {
					propertiesByName.Add (added.Name, added);
				}
			} else
				throw new Exception ("PropertyGroup is not initialized.");
			return added;
		}
		
		internal void AddFromExistingProperty (BuildProperty buildProperty)
		{
			BuildProperty added, existing;
			
			added = buildProperty.Clone (false);
			if (propertiesByName.Contains (added.Name) == true) {
				existing = (BuildProperty) propertiesByName [added.Name];
				if (added.PropertyType <= existing.PropertyType) {
					propertiesByName.Remove (added.Name);
					propertiesByName.Add (added.Name, added);
				}
			} else
				propertiesByName.Add (added.Name, added);
		}
		
		public void Clear ()
		{
		}

		public BuildPropertyGroup Clone (bool deepClone)
		{
			return null;
		}

		// FIXME: what it is doing?
		internal void Evaluate (BuildPropertyGroup evaluatedPropertyBag,
					       bool ignoreCondition,
					       bool honorCondition,
					       Hashtable conditionedPropertiesTable,
					       ProcessingPass pass)
		{
		}
		
		public IEnumerator GetEnumerator ()
		{
			if (properties != null)
				foreach (BuildProperty bp in properties)
					yield return bp;
			else if (propertiesByName != null)
				foreach (DictionaryEntry de in propertiesByName)
					yield return (BuildProperty) de.Value;
			else
				throw new Exception ("PropertyGroup is not initialized.");
		}

		public void RemoveProperty (BuildProperty propertyToRemove)
		{
			if (properties == null)
				throw new Exception ("PropertyGroup is not initialized.");
			properties.Remove (propertyToRemove);
		}

		public void RemoveProperty (string propertyName)
		{
			if (propertiesByName == null)
				throw new Exception ("PropertyGroup is not initialized.");
			propertiesByName.Remove (propertyName);
		}

		public void SetProperty (string propertyName,
					 string propertyValue)
		{
			SetProperty (propertyName, propertyValue, false);
		}
		
		// FIXME: use treatPropertyValueAsLiteral
		[MonoTODO]
		public void SetProperty (string propertyName,
					 string propertyValue,
					 bool treatPropertyValueAsLiteral)
		{
			if (propertiesByName.Contains (propertyName) == false) {
				AddNewProperty (propertyName, propertyValue);
			}
			((BuildProperty) propertiesByName [propertyName]).Value = propertyValue;
		}
		
		internal void BindToXml (XmlElement propertyGroupElement)
		{
			if (propertyGroupElement == null)
				throw new ArgumentNullException ();
			this.properties = new ArrayList ();
			this.propertyGroup = propertyGroupElement;
			this.condition = propertyGroupElement.GetAttributeNode ("Condition");
			this.importedFromFilename = null;
			this.isImported = false;
			foreach (XmlElement xe in propertyGroupElement.ChildNodes) {
				BuildProperty bp = AddNewProperty(xe.Name, xe.InnerText);
				bp.PropertyType = PropertyType.Normal;
				bp.BindToXml (xe);
				Expression finalValue = new Expression (parentProject, bp.Value);
				bp.FinalValue = (string) finalValue.ToNonArray (typeof (string));
				parentProject.EvaluatedProperties.AddFromExistingProperty (bp);
			} 
		}
		
		public string Condition {
			get {
				if (condition == null)
					return null;
				else
					return condition.Value;
			}
			set {
				if (condition != null)
					condition.Value = value;
			}
		}

		public int Count {
			get {
				if (properties != null)
					return properties.Count;
				else if (propertiesByName != null)
					return propertiesByName.Count;
				else
					throw new Exception ("PropertyGroup is not initialized.");
			}
		}

		internal string ImportedFromFilename {
			get {
				return importedFromFilename;
			}
		}

		public bool IsImported {
			get {
				return isImported;
			}
		}

		public BuildProperty this[string propertyName] {
			get {
				if (propertiesByName.Contains (propertyName)) {
					return (BuildProperty) propertiesByName [propertyName];
				} else {
					return null;
				}
			}
			set {
				propertiesByName [propertyName] = value;
			}
		}
		
		internal GroupingCollection GroupingCollection {
			get { return parentCollection; }
			set { parentCollection = value; }
		}
	}
}

#endif