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
		bool			isImported;
		GroupingCollection	parentCollection;
		Project			parentProject;
		IList			properties;
		IDictionary		propertiesByName;

		internal bool FromXml {
			get {
				return propertyGroup != null;
			}
		}
	
		public BuildPropertyGroup ()
			: this (null, null)
		{
		}

		internal BuildPropertyGroup (XmlElement xmlElement, Project project)
		{
			this.isImported = false;
			this.parentCollection = null;
			this.parentProject = project;
			this.isImported = false;
			this.propertyGroup = xmlElement;

			if (FromXml) {
				this.properties = new ArrayList ();
				foreach (XmlElement xe in propertyGroup.ChildNodes) {
					BuildProperty bp = new BuildProperty (parentProject, xe);
					AddProperty (bp);
				} 
			} else
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
			BuildProperty prop;

			if (FromXml) {
				XmlElement xe;
				
				xe = propertyGroup.OwnerDocument.CreateElement (propertyName);
				propertyGroup.AppendChild (xe);
				if (treatPropertyValueAsLiteral) {
					xe.InnerText = Utilities.Escape (propertyValue);
				} else {
					xe.InnerText = propertyValue;
				}
				prop = new BuildProperty (parentProject, xe);
			} else {
				prop = new BuildProperty (propertyName, propertyValue);
			}
			AddProperty (prop);
			return prop;
		}

		internal void AddProperty (BuildProperty property)
		{
			if (FromXml) {
				properties.Add (property);
			} else {
				if (propertiesByName.Contains (property.Name) == true) {
					BuildProperty existing = (BuildProperty) propertiesByName [property.Name];
					if (property.PropertyType <= existing.PropertyType) {
						propertiesByName.Remove (property.Name);
						propertiesByName.Add (property.Name, property);
					}
				} else {
					propertiesByName.Add (property.Name, property);
				}
			}
		}
		
		public void Clear ()
		{
		}

		public BuildPropertyGroup Clone (bool deepClone)
		{
			return null;
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
		
		internal void Evaluate ()
		{
			if (!FromXml) {
				throw new InvalidOperationException ();
			}
			foreach (BuildProperty bp in properties) {
				bp.Evaluate ();
			}
		}
		
		public string Condition {
			get {
				if (!FromXml)
					return String.Empty;
				return propertyGroup.GetAttribute ("Condition");
			}
			set {
				if (!FromXml)
					throw new InvalidOperationException ("Can only set condition on xml elements.");
				propertyGroup.SetAttribute ("Condition", value);
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
