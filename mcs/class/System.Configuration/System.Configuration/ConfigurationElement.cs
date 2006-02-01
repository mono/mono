//
// System.Configuration.ConfigurationElement.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
// 	Lluis Sanchez Gual (lluis@novell.com)
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
		string rawXml;
		bool modified;
		ElementMap map;
		ConfigurationPropertyCollection keyProps;
		ConfigurationElementCollection defaultCollection;
		bool readOnly;
		ElementInformation elementInfo;
		ConfigurationElementProperty elementProperty;

		protected ConfigurationElement ()
		{
		}
		
		internal virtual void InitFromProperty (PropertyInformation propertyInfo)
		{
			elementInfo = new ElementInformation (this, propertyInfo);
			Init ();
		}
		
		public ElementInformation ElementInformation {
			get {
				if (elementInfo == null)
					elementInfo = new ElementInformation (this, null);
				return elementInfo;
			}
		}

		internal string RawXml {
			get { return rawXml; }
			set { rawXml = value; }
		}

		protected internal virtual void Init ()
		{
		}

		protected internal virtual ConfigurationElementProperty ElementProperty {
			get {
				if (elementProperty == null)
					elementProperty = new ConfigurationElementProperty (ElementInformation.Validator);
				return elementProperty;
			}
		}

		[MonoTODO]
		protected ContextInformation EvaluationContext {
			get {
				throw new NotImplementedException ();
			}
		}

		ConfigurationLockCollection lockAllAttributesExcept;
		public ConfigurationLockCollection LockAllAttributesExcept {
			get {
				if (lockAllAttributesExcept == null) {
					lockAllAttributesExcept = new ConfigurationLockCollection (this, ConfigurationLockType.Attribute | ConfigurationLockType.Exclude);
				}

				return lockAllAttributesExcept;
			}
		}

		ConfigurationLockCollection lockAllElementsExcept;
		public ConfigurationLockCollection LockAllElementsExcept {
			get {
				if (lockAllElementsExcept == null) {
					lockAllElementsExcept = new ConfigurationLockCollection (this, ConfigurationLockType.Element | ConfigurationLockType.Exclude);
				}

				return lockAllElementsExcept;
			}
		}

		ConfigurationLockCollection lockAttributes;
		public ConfigurationLockCollection LockAttributes {
			get {
				if (lockAttributes == null) {
					lockAttributes = new ConfigurationLockCollection (this, ConfigurationLockType.Attribute);
				}

				return lockAttributes;
			}
		}

		ConfigurationLockCollection lockElements;
		public ConfigurationLockCollection LockElements {
			get {
				if (lockElements == null) {
					lockElements = new ConfigurationLockCollection (this, ConfigurationLockType.Element);
				}

				return lockElements;
			}
		}

		bool lockItem;
		public bool LockItem {
			get { return lockItem; }
			set { lockItem = value; }
		}

		[MonoTODO]
		protected void ListErrors (IList list)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void SetPropertyValue (ConfigurationProperty prop, object value, bool ignoreLocks)
		{
			try {
				/* XXX all i know for certain is that Validation happens here */
				prop.Validate (value);

				/* XXX presumably the actual setting of the
				 * property happens here instead of in the
				 * set_Item code below, but that would mean
				 * the Value needs to be stuffed in the
				 * property, not the propertyinfo (or else the
				 * property needs a ref to the property info
				 * to correctly set the value). */
			}
			catch (Exception e) {
				throw new ConfigurationErrorsException (String.Format ("The value for the property '{0}' is not valid. The error is: {1}", prop.Name, e.Message), e);
			}
		}

		internal ConfigurationPropertyCollection GetKeyProperties ()
		{
			if (keyProps != null) return keyProps;
			
			if (map != null && map.Properties == Properties)
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

		internal ConfigurationElementCollection GetDefaultCollection ()
		{
			if (defaultCollection != null) return defaultCollection;

			ConfigurationProperty defaultCollectionProp = null;

			if (map != null && map.Properties == Properties) {
				defaultCollectionProp = map.DefaultCollectionProperty;
			}
			else {
				foreach (ConfigurationProperty prop in Properties) {
					if (prop.IsDefaultCollection) {
						defaultCollectionProp = prop;
						break;
					}
				}
			}

			if (defaultCollectionProp != null) {
				defaultCollection = this [defaultCollectionProp] as ConfigurationElementCollection;
			}

			return defaultCollection;
		}

		protected internal object this [ConfigurationProperty property] {
			get { return this [property.Name]; }
			set { this [property.Name] = value; }
		}

		protected internal object this [string property_name] {
			get {
				PropertyInformation pi = ElementInformation.Properties [property_name];
				if (pi == null)
					throw new InvalidOperationException ("Property '" + property_name + "' not found in configuration element");

				return pi.Value;
			}

			set {
				PropertyInformation pi = ElementInformation.Properties [property_name];
				if (pi == null)
					throw new InvalidOperationException ("Property '" + property_name + "' not found in configuration element");

				SetPropertyValue (pi.Property, value, false);

				pi.Value = value;
				modified = true;
			}
		}

		protected internal virtual ConfigurationPropertyCollection Properties {
			get {
				if (map == null)
					map = ElementMap.GetMap (GetType());
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

		internal virtual bool HasValues ()
		{
			foreach (PropertyInformation pi in ElementInformation.Properties)
				if (pi.ValueOrigin != PropertyValueOrigin.Default)
					return true;
			return false;
		}
		
		protected internal virtual void DeserializeElement (XmlReader reader, bool serializeCollectionKey)
		{
			Hashtable readProps = new Hashtable ();
			
			reader.MoveToContent ();
			while (reader.MoveToNextAttribute ())
			{
				PropertyInformation prop = ElementInformation.Properties [reader.LocalName];
				if (prop == null || (serializeCollectionKey && !prop.IsKey)) {
					/* handle the built in ConfigurationElement attributes here */
					if (reader.LocalName == "lockAllAttributesExcept") {
						LockAllAttributesExcept.SetFromList (reader.Value);
					}
					else if (reader.LocalName == "lockAllElementsExcept") {
						LockAllElementsExcept.SetFromList (reader.Value);
					}
					else if (reader.LocalName == "lockAttributes") {
						LockAttributes.SetFromList (reader.Value);
					}
					else if (reader.LocalName == "lockElements") {
						LockElements.SetFromList (reader.Value);
					}
					else if (reader.LocalName == "lockItem") {
						LockItem = (reader.Value.ToLower() == "true");
					}
					else if (reader.LocalName == "xmlns") {
						/* ignore */
					}
					else if (!OnDeserializeUnrecognizedAttribute (reader.LocalName, reader.Value))
						throw new ConfigurationException ("Unrecognized attribute '" + reader.LocalName + "'.");

					continue;
				}
				
				if (readProps.ContainsKey (prop))
					throw new ConfigurationException ("The attribute '" + prop.Name + "' may only appear once in this element.");
				
				prop.SetStringValue (reader.Value);
				readProps [prop] = prop.Name;
			}
			
			reader.MoveToElement ();
			if (reader.IsEmptyElement) {
				reader.Skip ();
			}
			else {

				int depth = reader.Depth;

				reader.ReadStartElement ();
				reader.MoveToContent ();

				do {
					if (reader.NodeType != XmlNodeType.Element) {
						reader.Skip ();
						continue;
					}
					
					PropertyInformation prop = ElementInformation.Properties [reader.LocalName];
					if (prop == null || (serializeCollectionKey && !prop.IsKey)) {
						if (prop == null) {
							ConfigurationElementCollection c = GetDefaultCollection ();
							if (c != null && c.OnDeserializeUnrecognizedElement (reader.LocalName, reader))
								continue;
						}

						if (!OnDeserializeUnrecognizedElement (reader.LocalName, reader)) {
							throw new ConfigurationException ("Unrecognized element '" + reader.LocalName + "'.");
						}
						continue;
					}
					
					if (!prop.IsElement)
						throw new ConfigurationException ("Property '" + prop.Name + "' is not a ConfigurationElement.");
					
					if (readProps.Contains (prop))
						throw new ConfigurationException ("The element <" + prop.Name + "> may only appear once in this section.");
					
					ConfigurationElement val = (ConfigurationElement) prop.Value;
					val.DeserializeElement (reader, serializeCollectionKey);
					readProps [prop] = prop.Name;

					reader.Read();

				} while (depth < reader.Depth);

				if (reader.NodeType == XmlNodeType.EndElement)
					reader.Read ();
			}
			
			modified = false;
				
			foreach (PropertyInformation prop in ElementInformation.Properties)
				if (prop.IsRequired && !readProps.ContainsKey (prop)) {
					object val = OnRequiredPropertyNotFound (prop.Name);
					if (!object.Equals (val, prop.DefaultValue)) {
						prop.Value = val;
						prop.IsModified = false;
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

		protected internal virtual void Reset (ConfigurationElement parentElement)
		{
			if (parentElement != null)
				ElementInformation.Reset (parentElement.ElementInformation);
			else
				InitializeDefault ();
		}

		protected internal virtual void ResetModified ()
		{
			modified = false;
			foreach (PropertyInformation p in ElementInformation.Properties)
				p.IsModified = false;
		}

		protected internal virtual bool SerializeElement (XmlWriter writer, bool serializeCollectionKey)
		{
			PreSerialize (writer);
			
			if (serializeCollectionKey) {
				ConfigurationPropertyCollection props = GetKeyProperties ();
				foreach (ConfigurationProperty prop in props)
					writer.WriteAttributeString (prop.Name, prop.ConvertToString (this[prop.Name]));
				return props.Count > 0;
			}
			
			bool wroteData = false;
			
			foreach (PropertyInformation prop in ElementInformation.Properties)
			{
				if (prop.IsElement || prop.ValueOrigin == PropertyValueOrigin.Default)
					continue;
				
				if (!object.Equals (prop.Value, prop.DefaultValue)) {
					writer.WriteAttributeString (prop.Name, prop.GetStringValue ());
					wroteData = true;
				}
			}
			
			foreach (PropertyInformation prop in ElementInformation.Properties)
			{
				if (!prop.IsElement)
					continue;
				
				ConfigurationElement val = (ConfigurationElement) prop.Value;
				if (val != null && val.HasValues ()) {
					wroteData = val.SerializeToXmlElement (writer, prop.Name) || wroteData;
				}
			}
			return wroteData;
		}
				
		protected internal virtual bool SerializeToXmlElement (
				XmlWriter writer, string elementName)
		{
			if (elementName != null && elementName != "")
				writer.WriteStartElement (elementName);
			bool res = SerializeElement (writer, false);
			if (elementName != null && elementName != "")
				writer.WriteEndElement ();
			return res;
		}

		protected internal virtual void Unmerge (
				ConfigurationElement source, ConfigurationElement parent,
				ConfigurationSaveMode updateMode)
		{
			if (parent != null && source.GetType() != parent.GetType())
				throw new ConfigurationException ("Can't unmerge two elements of different type");
			
			foreach (PropertyInformation prop in source.ElementInformation.Properties)
			{
				if (prop.ValueOrigin == PropertyValueOrigin.Default)
					continue;
				
				PropertyInformation unmergedProp = ElementInformation.Properties [prop.Name];
				
				object sourceValue = prop.Value;
				if 	(parent == null || !parent.HasValue (prop.Name)) {
					unmergedProp.Value = sourceValue;
					continue;
				}
				else if (sourceValue != null) {
					object parentValue = parent [prop.Name];
					if (prop.IsElement) {
						if (parentValue != null) {
							ConfigurationElement copy = (ConfigurationElement) unmergedProp.Value;
							copy.Unmerge ((ConfigurationElement) sourceValue, (ConfigurationElement) parentValue, updateMode);
						}
						else
							unmergedProp.Value = sourceValue;
					}
					else {
						if (!object.Equals (sourceValue, parentValue) || 
							(updateMode == ConfigurationSaveMode.Full) ||
							(updateMode == ConfigurationSaveMode.Modified && prop.ValueOrigin == PropertyValueOrigin.SetHere))
							unmergedProp.Value = sourceValue;
					}
				}
			}
		}
		
		internal bool HasValue (string propName)
		{
			PropertyInformation info = ElementInformation.Properties [propName];
			return info != null && info.ValueOrigin != PropertyValueOrigin.Default;
		}
		
		internal bool IsReadFromConfig (string propName)
		{
			PropertyInformation info = ElementInformation.Properties [propName];
			return info != null && info.ValueOrigin == PropertyValueOrigin.SetHere;
		}
	}
	
	internal class ElementMap
	{
		static Hashtable elementMaps = new Hashtable ();
		
		ConfigurationPropertyCollection properties;
		ConfigurationPropertyCollection keyProperties;
		ConfigurationProperty defaultCollectionProperty;

		ConfigurationCollectionAttribute collectionAttribute;
		
		public static ElementMap GetMap (Type t)
		{
			lock (elementMaps) {
				ElementMap map = elementMaps [t] as ElementMap;
				if (map != null) return map;
				map = new ElementMap (t);
				elementMaps [t] = map;
				return map;
			}
		}
		
		public ElementMap (Type t)
		{
			ReflectProperties (t);
		}
		
		protected void ReflectProperties (Type t)
		{
			collectionAttribute = Attribute.GetCustomAttribute (t, typeof(ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute;
			
			PropertyInfo[] props = t.GetProperties ();
			foreach (PropertyInfo prop in props)
			{
				ConfigurationPropertyAttribute at = Attribute.GetCustomAttribute (prop, typeof(ConfigurationPropertyAttribute)) as ConfigurationPropertyAttribute;
				if (at == null) continue;
				string name = at.Name != null ? at.Name : prop.Name;

				if (
				    /* if we have no default value, don't bother to check further */
				    at.DefaultValue != null && at.DefaultValue != ConfigurationProperty.NoDefaultValue
				    )
				{
					try {
						Convert.ChangeType (at.DefaultValue, prop.PropertyType);
					}
					catch {
						throw new ConfigurationErrorsException (String.Format ("The default value for property '{0}' has a different type than the one of the property itself",
												       name));
					}
				}

				ConfigurationValidatorAttribute validatorAttr = Attribute.GetCustomAttribute (t, typeof(ConfigurationValidatorAttribute)) as ConfigurationValidatorAttribute;
				ConfigurationValidatorBase validator = validatorAttr != null ? validatorAttr.ValidatorInstance : new DefaultValidator();
				
				TypeConverter converter = TypeDescriptor.GetConverter (prop.PropertyType);
				ConfigurationProperty cp = new ConfigurationProperty (name, prop.PropertyType, at.DefaultValue, converter, validator, at.Options);
				
				cp.CollectionAttribute = Attribute.GetCustomAttribute (prop, typeof(ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute;
				
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
		
		public ConfigurationCollectionAttribute CollectionAttribute {
			get { return collectionAttribute; }
		}
		
		public ConfigurationProperty DefaultCollectionProperty {
			get {
				if (defaultCollectionProperty == null) {
					if (properties != null)
						foreach (ConfigurationProperty p in properties) {
							if (p.IsDefaultCollection) defaultCollectionProperty = p;
							break;
						}
				}
				return defaultCollectionProperty;
			}
		}
	}
}

#endif
