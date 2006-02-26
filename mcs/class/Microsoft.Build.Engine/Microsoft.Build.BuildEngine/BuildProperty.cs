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
		XmlAttribute	condition;
		string		finalValue;
		string		value;
		string		name;
		PropertyType	propertyType;
	
		public BuildProperty ()
			: this (null, null)
		{
		}

		public BuildProperty (string propertyName,
				string propertyValue)
		{
			this.name = propertyName;
			this.value = propertyValue;
		}

		public BuildProperty Clone (bool deepClone)
		{
			BuildProperty bp;
			
			bp = new BuildProperty ();
			bp.condition = this.condition;
			bp.finalValue = this.finalValue;
			bp.name = this.name;
			bp.propertyElement = this.propertyElement;
			bp.propertyType = this.propertyType;
			bp.value = this.value;
			
			return bp;
		}

		public static implicit operator string (BuildProperty propertyToCast)
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

		
		internal void BindToXml (XmlElement propertyElement)
		{
			if (propertyElement == null)
				throw new ArgumentNullException ("propertyElement");
			this.propertyElement = propertyElement;
			this.condition = propertyElement.GetAttributeNode ("Condition");
			this.name = propertyElement.Name;
			this.value = propertyElement.InnerText;
		}
		
		internal void UpdateXml ()
		{
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

		public string FinalValue {
			get {
				if (finalValue == null) {
					return this.@value;
				} else
					return finalValue;
			}
			internal set {
				finalValue = value;
			}
		}

		public string Name {
			get {
				return name;
			}
		}

		public string Value {
			get {
				return value;
			}
			set {
				this.@value = value;
			}
		}

		internal PropertyType PropertyType {
			get { return propertyType; }
			set { propertyType = value; }
		}
	}

	internal enum PropertyType {
		Reserved,
		CommandLine,
		Normal,
		Environment
	}
}

#endif