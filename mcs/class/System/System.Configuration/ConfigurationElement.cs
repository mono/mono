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

#if NET_2_0 && XML_DEP
#if XML_DEP
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
		
		protected ConfigurationElement ()
		{
			map = GetMap (GetType());
		}
		
		internal string RawXml {
			get { return rawXml; }
			set { rawXml = value; }
		}

		protected internal virtual ConfigurationPropertyCollection CollectionKeyProperties {
			get {
				return map.KeyProperties;
			}
		}

		protected internal object this [ConfigurationProperty property] {
			get {
				if (values == null || !values.ContainsKey (property)) {
					if (property.IsElement) {
						object elem = Activator.CreateInstance (property.Type);
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
				ConfigurationProperty prop = map.Properties [property_name];
				if (prop == null) throw new InvalidOperationException ("Property '" + property_name + "' not found in configuration section");
				return this [prop];
			}

			set {
				ConfigurationProperty prop = map.Properties [property_name];
				if (prop == null) throw new InvalidOperationException ("Property '" + property_name + "' not found in configuration section");
				this [prop] = value;
			}
		}

		protected internal virtual ConfigurationPropertyCollection Properties {
			get {
				return map.Properties;
			}
		}

		[MonoTODO]
		public override bool Equals (object compareTo)
		{
			return base.Equals (compareTo);
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public bool HasValue (string key)
		{
			if (values == null) return false;
			ConfigurationProperty prop = map.Properties [key];
			if (prop == null) return false;
			return values.ContainsKey (prop);
		}
		
		internal bool HasValues ()
		{
			return values != null && values.Count > 0;
		}

		[MonoTODO]
		public string PropertyFileName ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int PropertyLineNumber ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void Deserialize (XmlReader reader, bool serializeCollectionKey)
		{
			Hashtable readProps = new Hashtable ();
			
			reader.MoveToContent ();
			if (!map.HasProperties) {
				reader.Skip ();
				return;
			}
			
			while (reader.MoveToNextAttribute ())
			{
				ConfigurationProperty prop = map.Properties [reader.LocalName];
				if (prop == null) {
					if (!HandleUnrecognizedAttribute (reader.LocalName, reader.Value))
						throw new ConfigurationException ("Unrecognized attribute '" + reader.LocalName + "'.");
					continue;
				}
				
				if (readProps.Contains (prop))
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
					
					ConfigurationProperty prop = map.Properties [reader.LocalName];
					if (prop == null) {
						if (!HandleUnrecognizedElement (reader.LocalName, reader))
							throw new ConfigurationException ("Unrecognized element '" + reader.LocalName + "'.");
						continue;
					}
					
					if (!prop.IsElement)
						throw new ConfigurationException ("Property '" + prop.Name + "' is not a ConfigurationElement.");
					
					if (readProps.Contains (prop))
						throw new ConfigurationException ("The element <" + prop.Name + "> may only appear once in this section.");
					
					ConfigurationElement val = this [prop] as ConfigurationElement;
					val.Deserialize (reader, serializeCollectionKey);
					readProps [prop] = prop.Name;
				}
			}
			
			modified = false;
				
			if (readProps.Count > 0) {
				readProperties = new string [readProps.Count];
				readProps.Values.CopyTo ((object[])readProperties, 0);
			}
		}

		protected virtual bool HandleUnrecognizedAttribute (string name, string value)
		{
			return false;
		}

		protected virtual bool HandleUnrecognizedElement (string element, XmlReader reader)
		{
			return false;
		}

		[MonoTODO]
		protected internal virtual void InitializeDefault ()
		{
			values = null;
		}

		protected internal virtual bool IsModified ()
		{
			return modified;
		}

		[MonoTODO]
		protected internal virtual void ReadXml (XmlReader reader, object context)
		{
			Deserialize (reader, false);
		}

		[MonoTODO]
		protected internal virtual void Reset (ConfigurationElement parent_element, object context)
		{
			if (parent_element != null) {
				if (!map.HasProperties) return;
				values = null;
				foreach (ConfigurationProperty prop in map.Properties) {
					if (parent_element.HasValue (prop.Name))
						this [prop] = parent_element [prop.Name];
				}
			}
			else
				InitializeDefault ();
		}

		protected internal virtual void ResetModified ()
		{
			modified = false;
		}

		[MonoTODO ("Return value?")]
		protected internal virtual bool Serialize (XmlWriter writer, bool serializeCollectionKey)
		{
			if (values == null || !map.HasProperties) return true;
			
			ArrayList elems = new ArrayList ();
			foreach (DictionaryEntry entry in values)
			{
				ConfigurationProperty prop = (ConfigurationProperty) entry.Key;
				if (serializeCollectionKey && !prop.IsKey) continue;
				if (prop.IsElement) continue;
				
				if (!object.Equals (entry.Value, prop.DefaultValue))
					writer.WriteAttributeString (prop.Name, prop.ConvertToString (entry.Value));
			}
			if (serializeCollectionKey) return true;
			
			foreach (DictionaryEntry entry in values)
			{
				ConfigurationProperty prop = (ConfigurationProperty) entry.Key;
				if (!prop.IsElement) continue;
				
				ConfigurationElement val = entry.Value as ConfigurationElement;
				if (val != null && val.HasValues ())
					val.SerializeToXmlElement (writer, prop.Name);
			}
			return true;
		}
				
		[MonoTODO]
		protected internal virtual bool SerializeAttributeOnRemove (
				ConfigurationProperty property)
		{
			throw new NotImplementedException ();
		}

		protected internal virtual bool SerializeToXmlElement (
				XmlWriter writer, string elementName)
		{
			writer.WriteStartElement (elementName);
			Serialize (writer, false);
			writer.WriteEndElement ();
			return true;
		}

		protected internal virtual void UnMerge (
				ConfigurationElement source, ConfigurationElement parent,
				bool serializeCollectionKey, object context,
				ConfigurationUpdateMode updateMode)
		{
			if (source.map != parent.map)
				throw new ConfigurationException ("Can't unmerge two elements of different type");
			
			ElementMap map = source.map;
			if (!map.HasProperties) return;
			
			foreach (ConfigurationProperty prop in map.Properties)
			{
				if (!source.HasValue (prop.Name)) continue;
				
				object sourceValue = source [prop];
				if 	(!parent.HasValue (prop.Name) || updateMode == ConfigurationUpdateMode.Full) {
					this [prop] = sourceValue;
					continue;
				}
				else if (sourceValue != null) {
					object parentValue = parent [prop];
					if (prop.IsElement) {
						if (parentValue != null) {
							ConfigurationElement copy = (ConfigurationElement) Activator.CreateInstance (prop.Type);
							copy.UnMerge ((ConfigurationElement) sourceValue, (ConfigurationElement) parentValue, serializeCollectionKey, context, updateMode);
							this [prop] = copy;
						}
						else
							this [prop] = sourceValue;
					}
					else {
						if (!object.Equals (sourceValue, parentValue) || 
							(updateMode == ConfigurationUpdateMode.Modified && source.IsReadFromConfig (prop.Name)))
							this [prop] = sourceValue;
					}
				}
			}
		}
		
		bool IsReadFromConfig (string propName)
		{
			return readProperties != null && Array.IndexOf (readProperties, propName) != -1;
		}

		[MonoTODO]
		protected virtual void ValidateRequiredProperties (
				ConfigurationPropertyCollection properties,
				bool serialize_collection_key)
		{
			throw new NotImplementedException ();
		}

		protected internal virtual string WriteXml (
				ConfigurationElement parent,
				object context, string name,
				ConfigurationUpdateMode updateMode)
		{
			ConfigurationElement elem;
			if (parent != null) {
				elem = (ConfigurationElement) Activator.CreateInstance (GetType());
				elem.UnMerge (this, parent, false, context, updateMode);
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
				
				if (typeof(ConfigurationElementCollection).IsAssignableFrom (t))
					map = new CollectionElementMap (t);
				else
					map = new ElementMap (t);
				elementMaps [t] = map;
				return map;
			}
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
				ConfigurationPropertyAttribute at = (ConfigurationPropertyAttribute) Attribute.GetCustomAttribute (prop, typeof(ConfigurationPropertyAttribute)) as ConfigurationPropertyAttribute;
				if (at == null) continue;
				string name = at.Name != null ? at.Name : prop.Name;
				
				ConfigurationValidationAttribute validator = (ConfigurationValidationAttribute) Attribute.GetCustomAttribute (t, typeof(ConfigurationValidationAttribute)) as ConfigurationValidationAttribute;
				TypeConverter converter = TypeDescriptor.GetConverter (prop.PropertyType);
				ConfigurationProperty cp = new ConfigurationProperty (name, prop.PropertyType, at.DefaultValue, converter, validator, at.Flags);
				
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
	
	internal class CollectionElementMap: ElementMap
	{
		public CollectionElementMap (Type t): base (t)
		{
		}
	}
}

#endif
#endif
