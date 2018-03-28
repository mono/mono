//
// System.Configuration.ConfigurationProperty.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

using System;
using System.ComponentModel;

namespace System.Configuration
{
	public sealed class ConfigurationProperty
	{
		internal static readonly object NoDefaultValue = new object ();
		
		string name;
		Type type;
		object default_value;
		TypeConverter converter;
		ConfigurationValidatorBase validation;
		ConfigurationPropertyOptions flags;
		string description;
		ConfigurationCollectionAttribute collectionAttribute;
		
		public ConfigurationProperty (string name, Type type)
			: this (name, type, NoDefaultValue, TypeDescriptor.GetConverter (type), new DefaultValidator(), ConfigurationPropertyOptions.None, null)
		{ }

		public ConfigurationProperty (string name, Type type, object defaultValue)
			: this (name, type, defaultValue, TypeDescriptor.GetConverter (type), new DefaultValidator(), ConfigurationPropertyOptions.None, null)
		{ }

		public ConfigurationProperty (
					string name, Type type, object defaultValue,
					ConfigurationPropertyOptions options)
			:this (name, type, defaultValue, TypeDescriptor.GetConverter (type), new DefaultValidator(), options, null)
		{ }
		
		public ConfigurationProperty (
					string name, Type type, object defaultValue,
					TypeConverter typeConverter,
					ConfigurationValidatorBase validator,
					ConfigurationPropertyOptions options)
			: this (name, type, defaultValue, typeConverter, validator, options, null)
		{ }

		public ConfigurationProperty (
					string name, Type type, object defaultValue,
					TypeConverter typeConverter,
					ConfigurationValidatorBase validator,
					ConfigurationPropertyOptions options,
					string description)
		{
			this.name = name;
			this.converter = typeConverter != null ? typeConverter : TypeDescriptor.GetConverter (type);
			if (defaultValue != null) {
				if (defaultValue == NoDefaultValue) {
					switch (Type.GetTypeCode (type)) {
					case TypeCode.Object:
						defaultValue = null;
						break;
					case TypeCode.String:
						defaultValue = String.Empty;
						break;
					default:
						defaultValue = Activator.CreateInstance (type);
						break;
					}
				}
				else
					if (!type.IsAssignableFrom (defaultValue.GetType ())) {
						if (!this.converter.CanConvertFrom (defaultValue.GetType ()))
							throw new ConfigurationErrorsException (String.Format ("The default value for property '{0}' has a different type than the one of the property itself: expected {1} but was {2}",
													   name, type, defaultValue.GetType ()));

						defaultValue = this.converter.ConvertFrom (defaultValue);
					}
			}
			this.default_value = defaultValue;
			this.flags = options;
			this.type = type;
			this.validation = validator != null ? validator : new DefaultValidator ();
			this.description = description;
		}

		public TypeConverter Converter {
			get { return converter; }
		}

		public object DefaultValue {                        
			get { return default_value; }
		}

		public bool IsKey {                        
			get { return (flags & ConfigurationPropertyOptions.IsKey) != 0; }
		}

		public bool IsRequired {
			get { return (flags & ConfigurationPropertyOptions.IsRequired) != 0; }
		}

		public bool IsDefaultCollection {
			get { return (flags & ConfigurationPropertyOptions.IsDefaultCollection) != 0; }
		}

		public string Name {
			get { return name; }
		}

		public string Description {
			get { return description; }
		}

		public Type Type {
			get { return type; }
		}

		public ConfigurationValidatorBase Validator {
			get { return validation; }
		}

		internal object ConvertFromString (string value)
		{
			if (converter != null)
				return converter.ConvertFromInvariantString (value);
			else
				throw new NotImplementedException ();
		}

		internal string ConvertToString (object value)
		{
			if (converter != null)
				return converter.ConvertToInvariantString (value);
			else
				throw new NotImplementedException ();
		}
		
		internal bool IsElement {
			get {
				return (typeof(ConfigurationElement).IsAssignableFrom (type));
			}
		}
		
		internal ConfigurationCollectionAttribute CollectionAttribute {
			get { return collectionAttribute; }
			set { collectionAttribute = value; }
		}

		internal void Validate (object value)
		{
			if (validation != null)
				validation.Validate (value);
		}
	}
}

