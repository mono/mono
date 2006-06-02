//
// BuildProperty.cs: Represents a property
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
using System.Text;
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	public class BuildProperty {
	
		XmlElement	propertyElement;
		string		finalValue;
		bool		isImported;
		string		value;
		string		name;
		Project		parentProject;
		PropertyType	propertyType;

		private BuildProperty ()
		{
		}

		public BuildProperty (string propertyName, string propertyValue)
			: this (propertyName, propertyValue, PropertyType.Normal)
		{
			if (propertyName == null)
				throw new ArgumentNullException (null,
					"Parameter \"propertyName\" cannot be null.");
			if (propertyValue == null)
				throw new ArgumentNullException (null,
					"Parameter \"propertyValue\" cannot be null.");
		}

		internal BuildProperty (string propertyName,
				string propertyValue, PropertyType propertyType)
		{
			this.name = propertyName;
			this.value = propertyValue;
			this.finalValue = propertyValue;
			this.propertyType = propertyType;
			this.isImported = false;
		}

		internal BuildProperty (Project parentProject, XmlElement propertyElement)
		{
			if (propertyElement == null)
				throw new ArgumentNullException ("propertyElement");

			this.propertyElement = propertyElement;
			this.propertyType = PropertyType.Normal;
			this.parentProject = parentProject;
			this.name = propertyElement.Name;
			this.value = propertyElement.InnerText;
			this.isImported = false;
		}

		[MonoTODO]
		public BuildProperty Clone (bool deepClone)
		{
			if (deepClone) {
				if (FromXml == false) {
					return (BuildProperty) this.MemberwiseClone ();
				} else {
					throw new NotImplementedException ();
				}
			} else {
				if (FromXml == false)
					throw new InvalidOperationException ("A shallow clone of this object cannot be created.");
				throw new NotImplementedException ();
			}
		}

		public static explicit operator string (BuildProperty propertyToCast)
		{
			if (propertyToCast == null)
				throw new ArgumentNullException ("propertyToCast");
			return propertyToCast.ToString ();
		}

		public override string ToString ()
		{
			if (finalValue != null)
				return finalValue;
			else
				return Value;
		}

		internal void Evaluate ()
		{
			if (FromXml) {
				OldExpression exp = new OldExpression (parentProject);
				exp.ParseSource (Value);
				finalValue = (string) exp.ConvertTo (typeof (string));
				parentProject.EvaluatedProperties.AddProperty (this);
			}
		}

		private bool FromXml {
			get {
				return propertyElement != null;
			}
		}
	
		public string Condition {
			get {
				if (FromXml)
					return propertyElement.GetAttribute ("Condition");
				else
					return String.Empty;
			}
			set {
				if (FromXml)
					propertyElement.SetAttribute ("Condition", value);
			}
		}

		public string FinalValue {
			get {
				if (finalValue == null)
					return this.@value;
				else
					return finalValue;
			}
		}
		
		public bool IsImported {
			get { return isImported; }
		}

		public string Name {
			get { return name; }
		}

		public string Value {
			get {
				return value;
			}
			set {
				this.@value = value;
				if (FromXml) {
					propertyElement.InnerText = value;
				} else {
					finalValue = value;
				}
			}
		}

		internal PropertyType PropertyType {
			get {
				return propertyType;
			}
		}
	}

	internal enum PropertyType {
		Reserved,
		Global,
		Normal,
		Environment
	}
}

#endif
