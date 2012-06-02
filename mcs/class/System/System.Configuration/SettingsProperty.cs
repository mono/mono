//
// System.Web.UI.WebControls.SettingsProperty.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

using System;

namespace System.Configuration
{
	public class SettingsProperty
	{
		public SettingsProperty (SettingsProperty propertyToCopy)
			: this (propertyToCopy.Name,
				propertyToCopy.PropertyType,
				propertyToCopy.Provider,
				propertyToCopy.IsReadOnly,
				propertyToCopy.DefaultValue,
				propertyToCopy.SerializeAs,
				new SettingsAttributeDictionary (propertyToCopy.Attributes),
				propertyToCopy.ThrowOnErrorDeserializing,
				propertyToCopy.ThrowOnErrorSerializing)
		{
		}

		public SettingsProperty (string name)
			: this (name,
				null,
				null,
				false,
				null,
				SettingsSerializeAs.String,
				new SettingsAttributeDictionary(),
				false,
				false)
		{
		}

		public SettingsProperty (string name,
					 Type propertyType,
					 SettingsProvider provider,
					 bool isReadOnly,
					 object defaultValue,
					 SettingsSerializeAs serializeAs,
					 SettingsAttributeDictionary attributes,
					 bool throwOnErrorDeserializing,
					 bool throwOnErrorSerializing)
		{
			this.name = name;
			this.propertyType = propertyType;
			this.provider = provider;
			this.isReadOnly = isReadOnly;
			this.defaultValue = defaultValue;
			this.serializeAs = serializeAs;
			this.attributes = attributes;
			this.throwOnErrorDeserializing = throwOnErrorDeserializing;
			this.throwOnErrorSerializing = throwOnErrorSerializing;
		}

		public virtual SettingsAttributeDictionary Attributes {
			get {
				return attributes;
			}
		}

		public virtual object DefaultValue {
			get {
				return defaultValue;
			}
			set {
				defaultValue = value;
			}
		}

		public virtual bool IsReadOnly {
			get {
				return isReadOnly;
			}
			set {
				isReadOnly = value;
			}
		}

		public virtual string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		public virtual Type PropertyType {
			get {
				return propertyType;
			}
			set {
				propertyType = value;
			}
		}

		public virtual SettingsProvider Provider {
			get {
				return provider;
			}
			set {
				provider = value;
			}
		}

		public virtual SettingsSerializeAs SerializeAs {
			get {
				return serializeAs;
			}
			set {
				serializeAs = value;
			}
		}

		public bool ThrowOnErrorDeserializing {
			get {
				return throwOnErrorDeserializing;
			}
			set {
				throwOnErrorDeserializing = value;
			}
		}

		public bool ThrowOnErrorSerializing {
			get {
				return throwOnErrorSerializing;
			}
			set {
				throwOnErrorSerializing = value;
			}
		}

		string name;
		Type propertyType;
		SettingsProvider provider;
		bool isReadOnly;
		object defaultValue;
		SettingsSerializeAs serializeAs;
		SettingsAttributeDictionary attributes;
		bool throwOnErrorDeserializing;
		bool throwOnErrorSerializing;
	}

}

