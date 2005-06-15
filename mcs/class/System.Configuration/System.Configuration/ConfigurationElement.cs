//
// System.Configuration.ConfigurationElement.cs
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

#if NET_2_0
using System.Collections;
using System.Xml;
using System.Reflection;
using System.IO;
using System.ComponentModel;

namespace System.Configuration
{
	public abstract class ConfigurationElement
	{
		static Hashtable elementMaps = new Hashtable ();
		Hashtable values;
		string[] readProperties;
		string rawXml;
		bool modified;
		ElementMap map;
		ConfigurationPropertyCollection keyProps;
		bool readOnly;
		
		protected ConfigurationElement ()
		{
		}
		
		protected internal virtual void Init ()
		{
		}
		
		internal string RawXml {
			get { return rawXml; }
			set { rawXml = value; }
		}

		internal ConfigurationPropertyCollection GetKeyProperties ()
		{
			if (keyProps != null) return keyProps;
			
			if (map.Properties == Properties)
				keyProps = map.KeyProperties;
			else {
				keyProps = new ConfigurationPropertyCollection ();
				foreach (ConfigurationProperty prop in Properties) {
					if (prop.IsKey)
						keyProps.Add (prop);
				}
			}
			return keyProps;
		}

		protected internal object this [ConfigurationProperty property] {
			get {
				if (values == null || !values.ContainsKey (property)) {
					if (property.IsElement) {
						object elem = CreateElement (property.Type);
						this [property] = elem;
						return elem;
					}
					else
						return property.DefaultValue;
				}
				else
					return values [property];
			}

			set {
				if (object.Equals (value, property.DefaultValue)) {
					if (values == null) return;
					values.Remove (property);
				}
				else {
					if (values == null) values = new Hashtable ();
					values [property] = value;
				}
				modified = true;
			}
		}

		protected internal object this [string property_name] {
			get {
				ConfigurationProperty prop = Properties [property_name];
				if (prop == null) throw new InvalidOperationException ("Property '" + property_name + "' not found in configuration section");
				return this [prop];
			}

			set {
				ConfigurationProperty prop = Properties [property_name];
				if (prop == null) throw new InvalidOperationException ("Property '" + property_name + "' not found in configuration section");
				this [prop] = value;
			}
		}

		protected internal virtual ConfigurationPropertyCollection Properties {
			get {
				if (map == null)
					map = GetMap (GetType());
				return map.Properties;
			}
		}

		public override bool Equals (object compareTo)
		{
			ConfigurationElement other = compareTo as ConfigurationElement;
			if (other == null) return false;
			if (GetType() != other.GetType()) return false;
			
			foreach (ConfigurationProperty prop in Properties) {
				if (!object.Equals (this [prop], other [prop]))
					return false;
			}
			return true;
		}

		public override int GetHashCode ()
		{
			int code = 0;
			foreach (ConfigurationProperty prop in Properties)
				code += this [prop].GetHashCode ();
			return code;
		}

		internal bool HasValue (string key)
		{
			if (values == null) return false;
			ConfigurationProperty prop = Properties [key];
			if (prop == null) return false;
			return values.ContainsKey (prop);
		}
		
		internal virtual bool HasValues ()
		{
			return values != null && values.Count > 0;
		}

		protected internal virtual void DeserializeElement (XmlReader reader, bool serializeCollectionKey)
		{
			Hashtable readProps = new Hashtable ();
			
			reader.MoveToContent ();
			while (reader.MoveToNextAttribute ())
			{
				ConfigurationProperty prop = Properties [reader.LocalName];
				if (prop == null || (serializeCollectionKey && !prop.IsKey)) {
					if (!OnDeserializeUnrecognizedAttribute (reader.LocalName, reader.Value))
						throw new ConfigurationException ("Unrecognized attribute '" + reader.LocalName + "'.");
					continue;
				}
				
				if (readProps.ContainsKey (prop))
					throw new ConfigurationException ("The attribute '" + prop.Name + "' may only appear once in this element.");
				
				object val = prop.ConvertFromString (reader.Value);
				if (!object.Equals (val, prop.DefaultValue))
					this [prop] = val;
				readProps [prop] = prop.Name;
			}
			
			reader.MoveToElement ();
			if (reader.IsEmptyElement) {
				reader.Skip ();
			}
			else {
				reader.ReadStartElement ();
				reader.MoveToContent ();
				
				while (reader.NodeType != XmlNodeType.EndElement)
				{
					if (reader.NodeType != XmlNodeType.Element) {
						reader.Skip ();
						continue;
					}
					
					ConfigurationProperty prop = Properties [reader.LocalName];
					if (prop == null || (serializeCollectionKey && !prop.IsKey)) {
						if (!OnDeserializeUnrecognizedElement (reader.LocalName, reader))
							throw new ConfigurationException ("Unrecognized element '" + reader.LocalName + "'.");
						continue;
					}
					
					if (!prop.IsElement)
						throw new ConfigurationException ("Property '" + prop.Name + "' is not a ConfigurationElement.");
					
					if (readProps.Contains (prop))
						throw new ConfigurationException ("The element <" + prop.Name + "> may only appear once in this section.");
					
					ConfigurationElement val = this [prop] as ConfigurationElement;
					val.DeserializeElement (reader, serializeCollectionKey);
					readProps [prop] = prop.Name;
				}
			}
			
			modified = false;
				
			if (readProps.Count > 0) {
				readProperties = new string [readProps.Count];
				readProps.Values.CopyTo ((object[])readProperties, 0);
				
				foreach (ConfigurationProperty prop in Properties)
					if (prop.IsRequired && !readProps.ContainsKey (prop)) {
						object val = OnRequiredPropertyNotFound (prop.Name);
						if (!object.Equals (val, prop.DefaultValue))
							this [prop] = val;
					}
			}
			
			PostDeserialize ();
		}

		protected virtual bool OnDeserializeUnrecognizedAttribute (string name, string value)
		{
			return false;
		}

		protected virtual bool OnDeserializeUnrecognizedElement (string element, XmlReader reader)
		{
			return false;
		}
		
		protected virtual object OnRequiredPropertyNotFound (string name)
		{
			throw new ConfigurationErrorsException ("Required attribute '" + name + "' not found.");
		}
		
		protected virtual void PreSerialize (XmlWriter writer)
		{
		}

		protected virtual void PostDeserialize ()
		{
		}

		protected internal virtual void InitializeDefault ()
		{
			values = null;
		}

		protected internal virtual bool IsModified ()
		{
			return modified;
		}
		
		protected internal virtual void SetReadOnly ()
		{
			readOnly = true;
		}
		
		public virtual bool IsReadOnly ()
		{
			return readOnly;
		}

		protected internal virtual void DeserializeSection (XmlReader reader)
 		{
			DeserializeElement (reader, false);
		}

		protected internal virtual void Reset (ConfigurationElement parentElement)
		{
			if (parentElement != null) {
				values = null;
				foreach (ConfigurationProperty prop in Properties) {
					if (parentElement.HasValue (prop.Name)) {
						if (prop.IsElement) {
							ConfigurationElement parentValue = parentElement [prop.Name] as ConfigurationElement;
							ConfigurationElement value = CreateElement (parentValue.GetType());
							value.Reset (parentValue);
							this [prop] = value;
						}
						else
							this [prop] = parentElement [prop.Name];
					}
				}
			}
			else
				InitializeDefault ();
		}

		protected internal virtual void ResetModified ()
		{
			modified = false;
		}

		protected internal virtual bool SerializeElement (XmlWriter writer, bool serializeCollectionKey)
		{
			PreSerialize (writer);
			
			if (values == null)
				return false;
			
			if (serializeCollectionKey) {
				ConfigurationPropertyCollection props = GetKeyProperties ();
				foreach (ConfigurationProperty prop in props)
					writer.WriteAttributeString (prop.Name, prop.ConvertToString (this[prop]));
				return props.Count > 0;
			}
			
			bool wroteData = false;
			
			foreach (DictionaryEntry entry in values)
			{
				ConfigurationProperty prop = (ConfigurationProperty) entry.Key;
				if (prop.IsElement) continue;
				
				if (!object.Equals (entry.Value, prop.DefaultValue)) {
					writer.WriteAttributeString (prop.Name, prop.ConvertToString (entry.Value));
					wroteData = true;
				}
			}
			
			foreach (DictionaryEntry entry in values)
			{
				ConfigurationProperty prop = (ConfigurationProperty) entry.Key;
				if (!prop.IsElement) continue;
				
				ConfigurationElement val = entry.Value as ConfigurationElement;
				if (val != null && val.HasValues ()) {
					wroteData = val.SerializeToXmlElement (writer, prop.Name) || wroteData;
				}
			}
			return wroteData;
		}
				
		protected internal virtual bool SerializeToXmlElement (
				XmlWriter writer, string elementName)
		{
			writer.WriteStartElement (elementName);
			bool res = SerializeElement (writer, false);
			writer.WriteEndElement ();
			return res;
		}

		protected internal virtual void Unmerge (
				ConfigurationElement source, ConfigurationElement parent,
				bool serializeCollectionKey,
				ConfigurationSaveMode updateMode)
		{
			if (parent != null && source.GetType() != parent.GetType())
				throw new ConfigurationException ("Can't unmerge two elements of different type");
			
			foreach (ConfigurationProperty prop in source.Properties)
			{
				if (!source.HasValue (prop.Name)) continue;
				
				object sourceValue = source [prop];
				if 	(parent == null || !parent.HasValue (prop.Name)) {
					this [prop] = sourceValue;
					continue;
				}
				else if (sourceValue != null) {
					object parentValue = parent [prop];
					if (prop.IsElement) {
						if (parentValue != null) {
							ConfigurationElement copy = (ConfigurationElement) CreateElement (prop.Type);
							copy.Unmerge ((ConfigurationElement) sourceValue, (ConfigurationElement) parentValue, serializeCollectionKey, updateMode);
							this [prop] = copy;
						}
						else
							this [prop] = sourceValue;
					}
					else {
						if (!object.Equals (sourceValue, parentValue) || 
							(updateMode == ConfigurationSaveMode.Full) ||
							(updateMode == ConfigurationSaveMode.Modified && source.IsReadFromConfig (prop.Name)))
							this [prop] = sourceValue;
					}
				}
			}
		}
		
		bool IsReadFromConfig (string propName)
		{
			return readProperties != null && Array.IndexOf (readProperties, propName) != -1;
		}

		protected internal virtual string SerializeSection (ConfigurationElement parentElement, string name, ConfigurationSaveMode saveMode)
		{
			ConfigurationElement elem;
			if (parentElement != null) {
				elem = (ConfigurationElement) CreateElement (GetType());
				elem.Unmerge (this, parentElement, false, saveMode);
			}
			else
				elem = this;
			
			StringWriter sw = new StringWriter ();
			XmlTextWriter tw = new XmlTextWriter (sw);
			tw.Formatting = Formatting.Indented;
			elem.SerializeToXmlElement (tw, name);
			tw.Close ();
			return sw.ToString ();
		}
		
		internal static ElementMap GetMap (Type t)
		{
			lock (elementMaps) {
				ElementMap map = elementMaps [t] as ElementMap;
				if (map != null) return map;
				map = new ElementMap (t);
				elementMaps [t] = map;
				return map;
			}
		}
		
		ConfigurationElement CreateElement (Type t)
		{
			ConfigurationElement elem = (ConfigurationElement) Activator.CreateInstance (t);
			elem.Init ();
			if (IsReadOnly ())
				elem.SetReadOnly ();
			return elem;
		}
	}
	
	internal class ElementMap
	{
		ConfigurationPropertyCollection properties;
		ConfigurationPropertyCollection keyProperties;
		
		public ElementMap (Type t)
		{
			ReflectProperties (t);
		}
		
		protected void ReflectProperties (Type t)
		{
			PropertyInfo[] props = t.GetProperties ();
			foreach (PropertyInfo prop in props)
			{
				ConfigurationPropertyAttribute at = Attribute.GetCustomAttribute (prop, typeof(ConfigurationPropertyAttribute)) as ConfigurationPropertyAttribute;
				if (at == null) continue;
				string name = at.Name != null ? at.Name : prop.Name;
				
				ConfigurationValidatorAttribute validatorAttr = Attribute.GetCustomAttribute (t, typeof(ConfigurationValidatorAttribute)) as ConfigurationValidatorAttribute;
				ConfigurationValidatorBase validator = validatorAttr != null ? validatorAttr.ValidatorInstance : null;
				
				TypeConverter converter = TypeDescriptor.GetConverter (prop.PropertyType);
				ConfigurationProperty cp = new ConfigurationProperty (name, prop.PropertyType, at.DefaultValue, converter, validator, at.Options);
				
				if (properties == null) properties = new ConfigurationPropertyCollection ();
				properties.Add (cp);
			}
		}
		
		public bool HasProperties
		{
			get { return properties != null && properties.Count > 0; }
		}
		
		public ConfigurationPropertyCollection Properties
		{
			get {
				if (properties == null) properties = new ConfigurationPropertyCollection ();
				return properties;
			}
		}
		
		public ConfigurationPropertyCollection KeyProperties {
			get {
				if (keyProperties == null) {
					keyProperties = new ConfigurationPropertyCollection ();
					
					if (properties != null)
						foreach (ConfigurationProperty p in properties)
							if (p.IsKey) keyProperties.Add (p);
				}
				return keyProperties;
			}
		}
	}
}

#endif
