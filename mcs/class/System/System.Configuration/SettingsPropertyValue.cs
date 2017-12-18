//
// System.Web.UI.WebControls.SettingsPropertyValue.cs
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
using System.Globalization;
using System.IO;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;
#if (XML_DEP)
using System.Xml;
using System.Xml.Serialization;
#endif

namespace System.Configuration
{

	public class SettingsPropertyValue
	{
		public SettingsPropertyValue (SettingsProperty property)
		{
			this.property = property;
			needPropertyValue = true;
			needSerializedValue = true;
		}

		public bool Deserialized {
			get {
				return deserialized;
			}
			set {
				deserialized = value;
			}
		}

		public bool IsDirty {
			get {
				return dirty;
			}
			set {
				dirty = value;
			}
		}

		public string Name {
			get {
				return property.Name;
			}
		}

		public SettingsProperty Property {
			get {
				return property;
			}
		}

		public object PropertyValue {
			get {
				if (needPropertyValue) {
					propertyValue = GetDeserializedValue (serializedValue);
					if (propertyValue == null) {
						propertyValue = GetDeserializedDefaultValue ();
						serializedValue = null;
						needSerializedValue = true;
						defaulted = true;
					}
					needPropertyValue = false;
				}

				if (propertyValue != null &&
					!(propertyValue is string) &&
					!(propertyValue is DateTime) &&
					!property.PropertyType.IsPrimitive)
					dirty = true;

				return propertyValue;
			}
			set {
				propertyValue = value;
				dirty = true;
				needPropertyValue = false;
				needSerializedValue = true;
				defaulted = false;
			}
		}

		public object SerializedValue {
			get {
				if ((needSerializedValue || IsDirty) && !UsingDefaultValue) {
					switch (property.SerializeAs)
					{
					case SettingsSerializeAs.String:
						serializedValue = TypeDescriptor.GetConverter (property.PropertyType).ConvertToInvariantString (propertyValue);
						break;
#if (XML_DEP)
					case SettingsSerializeAs.Xml:
						if (propertyValue != null) {
							XmlSerializer serializer = new XmlSerializer (propertyValue.GetType ());
							StringWriter w = new StringWriter(CultureInfo.InvariantCulture);
	
							serializer.Serialize (w, propertyValue);
							serializedValue = w.ToString();
						}
						else
							serializedValue = null;
						break;
#endif
					case SettingsSerializeAs.Binary:
						if (propertyValue != null) {
							BinaryFormatter bf = new BinaryFormatter ();
							MemoryStream ms = new MemoryStream ();
							bf.Serialize (ms, propertyValue);
							serializedValue = ms.ToArray();
						}
						else
							serializedValue = null;
						break;
					default:
						serializedValue = null;
						break;
					}

					needSerializedValue = false;
					dirty = false;
				}

				return serializedValue;
			}
			set {
				serializedValue = value;
				needPropertyValue = true;
				needSerializedValue = false;
			}
		}

		public bool UsingDefaultValue {
			get {
				return defaulted;
			}
		}

		internal object Reset ()
		{
			propertyValue = GetDeserializedDefaultValue ();
			dirty = true;
			defaulted = true;
			needPropertyValue = true;
			needSerializedValue = true;
			return propertyValue;
		}

		private object GetDeserializedDefaultValue ()
		{
			if (property.DefaultValue == null)
				if (property.PropertyType != null && property.PropertyType.IsValueType)
					return Activator.CreateInstance (property.PropertyType);
				else
					return null;

			if (property.DefaultValue is string && ((string) property.DefaultValue).Length == 0)
				if (property.PropertyType != typeof (string))
					return Activator.CreateInstance (property.PropertyType);
				else
					return string.Empty;

			if (property.DefaultValue is string && ((string) property.DefaultValue).Length > 0)
				return GetDeserializedValue (property.DefaultValue);

			if (!property.PropertyType.IsAssignableFrom (property.DefaultValue.GetType ())) {
				TypeConverter converter = TypeDescriptor.GetConverter (property.PropertyType);
				return converter.ConvertFrom (null, CultureInfo.InvariantCulture, property.DefaultValue);
			}
			return property.DefaultValue;
		}

		private object GetDeserializedValue (object serializedValue)
		{
			if (serializedValue == null)
				return null;

			object deserializedObject = null;

			try {
				switch (property.SerializeAs) {
					case SettingsSerializeAs.String:
						if (serializedValue is string)
							deserializedObject = TypeDescriptor.GetConverter (property.PropertyType).ConvertFromInvariantString ((string) serializedValue);
						break;
#if (XML_DEP)
					case SettingsSerializeAs.Xml:
						XmlSerializer serializer = new XmlSerializer (property.PropertyType);
						StringReader str = new StringReader ((string) serializedValue);
						deserializedObject = serializer.Deserialize (XmlReader.Create (str));
						break;
#endif
					case SettingsSerializeAs.Binary:
						BinaryFormatter bf = new BinaryFormatter ();
						MemoryStream ms;
						if (serializedValue is string)
							ms = new MemoryStream (Convert.FromBase64String ((string) serializedValue));
						else
							ms = new MemoryStream ((byte []) serializedValue);
						deserializedObject = bf.Deserialize (ms);
						break;
				}
			}
			catch (Exception e) {
				if (property.ThrowOnErrorDeserializing)
					throw e;
			}

			return deserializedObject;
		}

		readonly SettingsProperty property;
		object propertyValue;
		object serializedValue;
		bool needSerializedValue;
		bool needPropertyValue;
		bool dirty;
		bool defaulted = false;
		bool deserialized;
	}

}

