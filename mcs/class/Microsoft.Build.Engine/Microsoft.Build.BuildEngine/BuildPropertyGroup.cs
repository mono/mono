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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	public class BuildPropertyGroup : IEnumerable {
	
		bool			read_only;
		ImportedProject		importedProject;
		XmlElement		propertyGroup;
		GroupingCollection	parentCollection;
		Project			parentProject;
		List <BuildProperty>	properties;
		Dictionary <string, BuildProperty>	propertiesByName;
		bool evaluated;
		bool isDynamic;

		public BuildPropertyGroup ()
			: this (null, null, null, false)
		{
		}

		internal BuildPropertyGroup (XmlElement xmlElement, Project project, ImportedProject importedProject, bool readOnly)
			: this (xmlElement, project, importedProject, readOnly, false)
		{
		}

		internal BuildPropertyGroup (XmlElement xmlElement, Project project, ImportedProject importedProject, bool readOnly, bool isDynamic)
		{
			this.importedProject = importedProject;
			this.parentCollection = null;
			this.parentProject = project;
			this.propertyGroup = xmlElement;
			this.read_only = readOnly;
			this.isDynamic = isDynamic;

			if (FromXml) {
				this.properties = new List <BuildProperty> ();
				foreach (XmlNode xn in propertyGroup.ChildNodes) {
					if (!(xn is XmlElement))
						continue;
					
					XmlElement xe = (XmlElement) xn;
					BuildProperty bp = new BuildProperty (parentProject, xe);
					AddProperty (bp);
				} 
			} else
				this.propertiesByName = new Dictionary <string, BuildProperty> (StringComparer.OrdinalIgnoreCase);

			DefinedInFileName = importedProject != null ? importedProject.FullFileName :
						(project != null ? project.FullFileName : null);
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
			if (!FromXml)
				throw new InvalidOperationException ("This method is only valid for persisted property groups.");

			if (treatPropertyValueAsLiteral)
				propertyValue = Utilities.Escape (propertyValue);

			XmlElement element = propertyGroup.OwnerDocument.CreateElement (propertyName, Project.XmlNamespace);
			propertyGroup.AppendChild (element);

			BuildProperty property = new BuildProperty (parentProject, element);
			property.Value = propertyValue;
			AddProperty (property);

			parentProject.MarkProjectAsDirty ();
			parentProject.NeedToReevaluate ();

			return property;
		}

		internal void AddProperty (BuildProperty property)
		{
			if (FromXml)
				properties.Add (property);
			else {
				if (propertiesByName.ContainsKey (property.Name)) {
					BuildProperty existing = propertiesByName [property.Name];
					if (property.PropertyType <= existing.PropertyType) {
						propertiesByName.Remove (property.Name);
						propertiesByName.Add (property.Name, property);
					}
				} else
					propertiesByName.Add (property.Name, property);
			}
		}
		
		public void Clear ()
		{
			if (FromXml) {
				propertyGroup.RemoveAll ();
				properties = new List <BuildProperty> ();
			} else
				propertiesByName = new Dictionary <string, BuildProperty> ();
		}

		[MonoTODO]
		public BuildPropertyGroup Clone (bool deepClone)
		{
			BuildPropertyGroup bpg = new BuildPropertyGroup (propertyGroup, parentProject, importedProject, read_only);
			if (FromXml) {
				foreach (BuildProperty bp in properties) {
					if (deepClone)
						bpg.AddProperty (bp.Clone (true));
					else
						bpg.AddNewProperty (bp.Name, bp.FinalValue);
				}
			} else {
				foreach (BuildProperty bp in propertiesByName.Values) {
					if (deepClone)
						bpg.AddProperty (bp.Clone (true));
					else
						bpg.AddNewProperty (bp.Name, bp.FinalValue);
				}
			}

			return bpg;
		}

		public IEnumerator GetEnumerator ()
		{
			if (FromXml)
				foreach (BuildProperty bp in properties)
					yield return bp;
			else 
				foreach (KeyValuePair <string, BuildProperty> kvp in propertiesByName)
					yield return kvp.Value;
		}

		public void RemoveProperty (BuildProperty property)
		{
			if (property == null)
				throw new ArgumentNullException ("property");

			if (FromXml) {
				if (!property.FromXml)
					throw new InvalidOperationException ("The specified property does not belong to the current property group.");

				property.XmlElement.ParentNode.RemoveChild (property.XmlElement);
				properties.Remove (property);
			} else
				propertiesByName.Remove (property.Name);
		}

		public void RemoveProperty (string propertyName)
		{
			if (FromXml) {
				foreach (BuildProperty bp in properties)
					if (bp.Name == propertyName) {
						RemoveProperty (bp);
						break;
					}
			} else
				propertiesByName.Remove (propertyName);
		}

		public void SetProperty (string propertyName,
					 string propertyValue)
		{
			SetProperty (propertyName, propertyValue, false);
		}
		
		public void SetProperty (string propertyName,
					 string propertyValue,
					 bool treatPropertyValueAsLiteral)
		{
			if (read_only)
				return;
			if (FromXml)
				throw new InvalidOperationException (
					"This method is only valid for virtual property groups, not <PropertyGroup> elements.");

			if (treatPropertyValueAsLiteral)
				propertyValue = Utilities.Escape (propertyValue);

			if (propertiesByName.ContainsKey (propertyName))
				propertiesByName.Remove (propertyName);

			BuildProperty bp = new BuildProperty (propertyName, propertyValue);
			if (Char.IsDigit (propertyName [0]))
				throw new ArgumentException (String.Format (
					"The name \"{0}\" contains an invalid character \"{1}\".", propertyName, propertyName [0]));

			AddProperty (bp);

			if (IsGlobal)
				parentProject.NeedToReevaluate ();
		}
		
		internal void Evaluate ()
		{
			if (!isDynamic && evaluated)
				return;

			foreach (BuildProperty bp in properties)
				if (ConditionParser.ParseAndEvaluate (bp.Condition, parentProject))
					bp.Evaluate ();

			evaluated = true;
		}
		
		public string Condition {
			get {
				if (!FromXml)
					return String.Empty;
				return propertyGroup.GetAttribute ("Condition");
			}
			set {
				if (!FromXml)
					throw new InvalidOperationException (
					"Cannot set a condition on an object not represented by an XML element in the project file.");
				propertyGroup.SetAttribute ("Condition", value);
			}
		}

		public int Count {
			get {
				if (FromXml)
					return properties.Count;
				else
					return propertiesByName.Count;
			}
		}

		public bool IsImported {
			get {
				return importedProject != null;
			}
		}

		internal bool FromXml {
			get {
				return propertyGroup != null;
			}
		}

		bool IsGlobal {
			get {
				return parentProject != null && propertyGroup == null;
			}
		}

		public BuildProperty this [string propertyName] {
			get {
				if (FromXml)
					throw new InvalidOperationException ("Properties in persisted property groups cannot be accessed by name.");
				
				if (propertiesByName.ContainsKey (propertyName))
					return propertiesByName [propertyName];
				else
					return null;
			}
			set {
				propertiesByName [propertyName] = value;
			}
		}

		internal string DefinedInFileName { get; private set; }

		internal GroupingCollection GroupingCollection {
			get { return parentCollection; }
			set { parentCollection = value; }
		}

		internal XmlElement XmlElement {
			get { return propertyGroup; }
		}

		internal IEnumerable<string> GetAttributes ()
		{
			foreach (XmlAttribute attrib in XmlElement.Attributes)
				yield return attrib.Value;

			foreach (BuildProperty bp in properties) {
				foreach (string attr in bp.GetAttributes ())
					yield return attr;
			}
		}
	}
}
