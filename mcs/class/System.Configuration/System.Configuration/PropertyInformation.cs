//
// System.Configuration.PropertyInformation.cs
//
// Authors:
//  Lluis Sanchez Gual (lluis@novell.com)
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
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using System.ComponentModel;

namespace System.Configuration
{
	public sealed class PropertyInformation
	{
		bool isLocked;
		bool isModified;
		int lineNumber;
		string source;
		object val;
		PropertyValueOrigin origin;
		readonly ConfigurationElement owner;
		readonly ConfigurationProperty property;
		
		internal PropertyInformation (ConfigurationElement owner, ConfigurationProperty property)
		{
			if (owner == null)
				throw new ArgumentNullException ("owner");
			if (property == null)
				throw new ArgumentNullException ("property");
			this.owner = owner;
			this.property = property;
		}
		
		public TypeConverter Converter {
			get { return property.Converter; }
		}
		
		public object DefaultValue {
			get { return property.DefaultValue; }
		}
		
		public string Description {
			get { return property.Description; }
		}
		
		public bool IsKey {
			get { return property.IsKey; }
		}
		
		[MonoTODO]
		public bool IsLocked {
			get { return isLocked; }
			internal set { isLocked = value; }
		}
		
		public bool IsModified {
			get { return isModified; }
			internal set { isModified = value; }
		}
		
		public bool IsRequired {
			get { return property.IsRequired; }
		}
		
		public int LineNumber {
			get { return lineNumber; }
			internal set { lineNumber = value; }
		}
		
		public string Name {
			get { return property.Name; }
		}
		
		public string Source {
			get { return source; }
			internal set { source = value; }
		}
		
		public Type Type {
			get { return property.Type; }
		}
		
		public ConfigurationValidatorBase Validator {
			get { return property.Validator; }
		}
		
		public object Value {
			get {
				if (origin == PropertyValueOrigin.Default) {
					if (property.IsElement) {
						ConfigurationElement elem = (ConfigurationElement) Activator.CreateInstance (Type, true);
						elem.InitFromProperty (this);
						if (owner != null && owner.IsReadOnly ())
							elem.SetReadOnly ();
						val = elem;
						origin = PropertyValueOrigin.Inherited;
					}
					else {
						return DefaultValue;
					}
				}
				return val;
			}
			set {
				val = value;
				isModified = true;
				origin = PropertyValueOrigin.SetHere;
			}
		}
		
		internal void Reset (PropertyInformation parentProperty)
		{
			if (parentProperty != null) {
				if (property.IsElement) {
					((ConfigurationElement)Value).Reset ((ConfigurationElement) parentProperty.Value);
				}
				else {
					val = parentProperty.Value;
					origin = PropertyValueOrigin.Inherited;
				}
			} else {
				origin = PropertyValueOrigin.Default;
			}
		}
		
		internal bool IsElement {
			get { return property.IsElement; }
		}
		
		public PropertyValueOrigin ValueOrigin {
			get { return origin; }
		}
		
		internal string GetStringValue ()
		{
			return property.ConvertToString (Value);
		}
		
		internal void SetStringValue (string value)
		{
			val = property.ConvertFromString (value);
			if (!object.Equals (val, DefaultValue))
				origin = PropertyValueOrigin.SetHere;
		}
		
		internal ConfigurationProperty Property {
			get { return property; }
		}
	}
}

