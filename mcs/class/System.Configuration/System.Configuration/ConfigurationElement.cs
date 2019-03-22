//
// System.Configuration.ConfigurationElement.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
// 	Lluis Sanchez Gual (lluis@novell.com)
// 	Martin Baulig <martin.baulig@xamarin.com>
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
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)
//

using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml;

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
		Configuration _configuration;
		bool elementPresent;

		internal Configuration Configuration {
			get { return _configuration; }
			set { _configuration = value; }
		}

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
		}

		// TODO: remove `protected` and tests to list of friendly assemblies.
		internal protected void SetRawXmlAndDeserialize (string rawXml, string rawXmlFileSource)
		{
			this.rawXml = rawXml;
			if (rawXml == null)
				return;
			using (var rawXmlReader = new ConfigXmlTextReader (rawXml, rawXmlFileSource))
				DeserializeRawXml (rawXmlReader);
		}

		protected virtual void DeserializeRawXml (XmlReader rawXmlReader)
		{
			DeserializeElement (rawXmlReader, false);
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

		protected ContextInformation EvaluationContext {
			get {
				if (Configuration != null)
					return Configuration.EvaluationContext;
				throw new ConfigurationErrorsException (
					"This element is not currently associated with any context.");
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
		protected virtual void ListErrors (IList errorList)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void SetPropertyValue (ConfigurationProperty prop, object value, bool ignoreLocks)
		{
			try {
				if (value != null) {
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
			}
			catch (Exception e) {
				throw new ConfigurationErrorsException (String.Format ("The value for the property '{0}' on type {1} is not valid.", prop.Name, this.ElementInformation.Type), e);
			}
		}

		internal ConfigurationPropertyCollection GetKeyProperties ()
		{
			if (keyProps != null) return keyProps;
			
			ConfigurationPropertyCollection tmpkeyProps = new ConfigurationPropertyCollection ();
				foreach (ConfigurationProperty prop in Properties) {
					if (prop.IsKey)
					tmpkeyProps.Add (prop);
				}

			return keyProps = tmpkeyProps;
		}

		internal ConfigurationElementCollection GetDefaultCollection ()
		{
			if (defaultCollection != null) return defaultCollection;

			ConfigurationProperty defaultCollectionProp = null;

			foreach (ConfigurationProperty prop in Properties) {
				if (prop.IsDefaultCollection) {
					defaultCollectionProp = prop;
					break;
				}
			}

			if (defaultCollectionProp != null) {
				defaultCollection = this [defaultCollectionProp] as ConfigurationElementCollection;
			}

			return defaultCollection;
		}

		protected internal object this [ConfigurationProperty prop] {
			get { return this [prop.Name]; }
			set { this [prop.Name] = value; }
		}

		protected internal object this [string propertyName] {
			get {
				PropertyInformation pi = ElementInformation.Properties [propertyName];
				if (pi == null)
					throw new InvalidOperationException ("Property '" + propertyName + "' not found in configuration element");

				return pi.Value;
			}

			set {
				PropertyInformation pi = ElementInformation.Properties [propertyName];
				if (pi == null)
					throw new InvalidOperationException ("Property '" + propertyName + "' not found in configuration element");

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
			object o;
			
			foreach (ConfigurationProperty prop in Properties) {
				o = this [prop];
				if (o == null)
					continue;
				
				code += o.GetHashCode ();
			}
			
			return code;
		}

		internal virtual bool HasLocalModifications ()
		{
			foreach (PropertyInformation pi in ElementInformation.Properties)
				if (pi.ValueOrigin == PropertyValueOrigin.SetHere && pi.IsModified)
					return true;
			
			return false;
		}
		
		protected internal virtual void DeserializeElement (XmlReader reader, bool serializeCollectionKey)
		{
			Hashtable readProps = new Hashtable ();
			
			reader.MoveToContent ();
			elementPresent = true;
			
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
						LockItem = (reader.Value.ToLowerInvariant () == "true");
					}
					else if (reader.LocalName == "xmlns") {
						/* ignore */
					} else if (this is ConfigurationSection && reader.LocalName == "configSource") {
						/* ignore */
					} else if (!OnDeserializeUnrecognizedAttribute (reader.LocalName, reader.Value))
						throw new ConfigurationErrorsException ("Unrecognized attribute '" + reader.LocalName + "'.", reader);

					continue;
				}
				
				if (readProps.ContainsKey (prop))
					throw new ConfigurationErrorsException ("The attribute '" + prop.Name + "' may only appear once in this element.", reader);

				string value = null;
				try {
					value = reader.Value;
					ValidateValue (prop.Property, value);
					prop.SetStringValue (value);
				} catch (ConfigurationErrorsException) {
					throw;
				} catch (ConfigurationException) {
					throw;
				} catch (Exception ex) {
					string msg = String.Format ("The value for the property '{0}' is not valid. The error is: {1}", prop.Name, ex.Message);
					throw new ConfigurationErrorsException (msg, reader);
				}
				readProps [prop] = prop.Name;
			
				ConfigXmlTextReader _reader = reader as ConfigXmlTextReader;
				if (_reader != null){
					prop.Source = _reader.Filename;
					prop.LineNumber = _reader.LineNumber;
				}
			}
			
			reader.MoveToElement ();
			if (reader.IsEmptyElement) {
				reader.Skip ();
			} else {
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
						if (!OnDeserializeUnrecognizedElement (reader.LocalName, reader)) {
							if (prop == null) {
								ConfigurationElementCollection c = GetDefaultCollection ();
								if (c != null && c.OnDeserializeUnrecognizedElement (reader.LocalName, reader))
									continue;
							}
							throw new ConfigurationErrorsException ("Unrecognized element '" + reader.LocalName + "'.", reader);
						}
						continue;
					}
					
					if (!prop.IsElement)
						throw new ConfigurationErrorsException ("Property '" + prop.Name + "' is not a ConfigurationElement.");
					
					if (readProps.Contains (prop))
						throw new ConfigurationErrorsException ("The element <" + prop.Name + "> may only appear once in this section.", reader);
					
					ConfigurationElement val = (ConfigurationElement) prop.Value;
					val.DeserializeElement (reader, serializeCollectionKey);
					readProps [prop] = prop.Name;

					if(depth == reader.Depth)
						reader.Read();

				} while (depth < reader.Depth);				
			}
			
			modified = false;
				
			foreach (PropertyInformation prop in ElementInformation.Properties) {
				if (String.IsNullOrEmpty(prop.Name) || readProps.ContainsKey (prop))
					continue;

				if (prop.IsRequired) {
					// TODO: It seems, the following behaviour is wronng.
					//       One need to compare with original .Net Framework.
					object val = OnRequiredPropertyNotFound (prop.Name);
					if (!object.Equals (val, prop.DefaultValue)) {
						prop.Value = val;
						prop.IsModified = false;
					}
				}
				else {
					prop.Reset (null);
				}
			}

			PostDeserialize ();
		}

		protected virtual bool OnDeserializeUnrecognizedAttribute (string name, string value)
		{
			return false;
		}

		protected virtual bool OnDeserializeUnrecognizedElement (string elementName, XmlReader reader)
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
			if (modified)
				return true;

			foreach (PropertyInformation prop in ElementInformation.Properties) {
				if (!prop.IsElement)
					continue;
				var element = prop.Value as ConfigurationElement;
				if ((element == null) || !element.IsModified ())
					continue;

				modified = true;
				break;
			}

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
			elementPresent = false;

			if (parentElement != null)
				ElementInformation.Reset (parentElement.ElementInformation);
			else
				InitializeDefault ();
		}

		protected internal virtual void ResetModified ()
		{
			modified = false;

			foreach (PropertyInformation p in ElementInformation.Properties) {
				p.IsModified = false;

				var element = p.Value as ConfigurationElement;
				if (element != null)
					element.ResetModified ();
			}
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
				if (prop.IsElement)
					continue;

				if (saveContext == null)
					throw new InvalidOperationException ();
				if (!saveContext.HasValue (prop))
					continue;

				writer.WriteAttributeString (prop.Name, prop.GetStringValue ());
				wroteData = true;
			}
			
			foreach (PropertyInformation prop in ElementInformation.Properties)
			{
				if (!prop.IsElement)
					continue;
				
				ConfigurationElement val = (ConfigurationElement) prop.Value;
				if (val != null)
					wroteData = val.SerializeToXmlElement (writer, prop.Name) || wroteData;
			}

			return wroteData;
		}

		protected internal virtual bool SerializeToXmlElement (
				XmlWriter writer, string elementName)
		{
			if (saveContext == null)
				throw new InvalidOperationException ();
			if (!saveContext.HasValues ())
				return false;

			if (elementName != null && elementName != "")
				writer.WriteStartElement (elementName);

			bool res = SerializeElement (writer, false);

			if (elementName != null && elementName != "")
				writer.WriteEndElement ();

			return res;
		}

		protected internal virtual void Unmerge (
				ConfigurationElement sourceElement, ConfigurationElement parentElement,
				ConfigurationSaveMode saveMode)
		{
			if (parentElement != null && sourceElement.GetType() != parentElement.GetType())
				throw new ConfigurationErrorsException ("Can't unmerge two elements of different type");

			bool isMinimalOrModified = saveMode == ConfigurationSaveMode.Minimal ||
				saveMode == ConfigurationSaveMode.Modified;

			foreach (PropertyInformation prop in sourceElement.ElementInformation.Properties)
			{
				if (prop.ValueOrigin == PropertyValueOrigin.Default)
					continue;
				
				PropertyInformation unmergedProp = ElementInformation.Properties [prop.Name];
				
				object sourceValue = prop.Value;
				if (parentElement == null || !parentElement.HasValue (prop.Name)) {
					unmergedProp.Value = sourceValue;
					continue;
				}

				if (sourceValue == null)
					continue;

				object parentValue = parentElement [prop.Name];
				if (!prop.IsElement) {
					if (!object.Equals (sourceValue, parentValue) || 
					    (saveMode == ConfigurationSaveMode.Full) ||
					    (saveMode == ConfigurationSaveMode.Modified && prop.ValueOrigin == PropertyValueOrigin.SetHere))
						unmergedProp.Value = sourceValue;
					continue;
				}

				var sourceElementValue = (ConfigurationElement) sourceValue;
				if (isMinimalOrModified && !sourceElementValue.IsModified ())
					continue;
				if (parentValue == null) {
					unmergedProp.Value = sourceValue;
					continue;
				}

				var parentElementValue = (ConfigurationElement) parentValue;
				ConfigurationElement copy = (ConfigurationElement) unmergedProp.Value;
				copy.Unmerge (sourceElementValue, parentElementValue, saveMode);
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

		internal bool IsElementPresent
		{
			get {	return elementPresent;	}
		}

		void ValidateValue (ConfigurationProperty p, string value)
		{
			ConfigurationValidatorBase validator;
			if (p == null || (validator = p.Validator) == null)
				return;
			
			if (!validator.CanValidate (p.Type))
				throw new ConfigurationErrorsException (
					String.Format ("Validator does not support type {0}", p.Type));
			validator.Validate (p.ConvertFromString (value));
		}

		/*
		 * FIXME: LAMESPEC
		 * 
		 * SerializeElement() and SerializeToXmlElement() need to emit different output
		 * based on the ConfigurationSaveMode that's being used.  Unfortunately, neither
		 * of these methods take it as an argument and there seems to be no documented way
		 * how to get it.
		 * 
		 * The parent element is needed because the element could be set to a different
		 * than the default value in a parent configuration file, then set locally to that
		 * same value.  This makes the element appear locally modified (so it's included
		 * with ConfigurationSaveMode.Modified), but it should not be emitted with
		 * ConfigurationSaveMode.Minimal.
		 * 
		 * In theory, we could save it into some private field in Unmerge(), but the
		 * problem is that Unmerge() is kinda expensive and we also need a way of
		 * determining whether or not the configuration has changed in Configuration.Save(),
		 * prior to opening the output file for writing.
		 * 
		 * There are two places from where HasValues() is called:
		 * a) From Configuration.Save() / SaveAs() to check whether the configuration needs
		 *    to be saved.  This check is done prior to opening the file for writing.
		 * b) From SerializeToXmlElement() to check whether to emit the element, using the
		 *    parent and mode values from the cached 'SaveContext'.
		 * 
		 */

		/*
		 * Check whether property 'prop' should be included in the serialized XML
		 * based on the current ConfigurationSaveMode.
		 */
		internal bool HasValue (ConfigurationElement parent, PropertyInformation prop,
		                        ConfigurationSaveMode mode)
		{
			if (prop.ValueOrigin == PropertyValueOrigin.Default)
				return false;
			
			if (mode == ConfigurationSaveMode.Modified &&
			    prop.ValueOrigin == PropertyValueOrigin.SetHere && prop.IsModified) {
				// Value has been modified locally, so we always emit it
				// with ConfigurationSaveMode.Modified.
				return true;
			}

			/*
			 * Ok, now we have to check whether we're different from the inherited
			 * value - which could either be a value that's set in a parent
			 * configuration file or the default value.
			 */
			
			var hasParentValue = parent != null && parent.HasValue (prop.Name);
			var parentOrDefault = hasParentValue ? parent [prop.Name] : prop.DefaultValue;

			if (!prop.IsElement)
				return !object.Equals (prop.Value, parentOrDefault);

			/*
			 * Ok, it's an element that has been set in a parent configuration file.			 * 
			 * Recursively call HasValues() to check whether it's been locally modified.
			 */
			var element = (ConfigurationElement) prop.Value;
			var parentElement = (ConfigurationElement) parentOrDefault;
			
			return element.HasValues (parentElement, mode);
		}

		/*
		 * Check whether this element should be included in the serialized XML
		 * based on the current ConfigurationSaveMode.
		 * 
		 * The 'parent' value is needed to determine whether the element currently
		 * has a different value from what's been set in the parent configuration
		 * hierarchy.
		 */
		internal virtual bool HasValues (ConfigurationElement parent, ConfigurationSaveMode mode)
		{
			if (mode == ConfigurationSaveMode.Full)
				return true;
			if (modified && (mode == ConfigurationSaveMode.Modified))
				return true;
			
			foreach (PropertyInformation prop in ElementInformation.Properties) {
				if (HasValue (parent, prop, mode))
					return true;
			}
			
			return false;
		}

		/*
		 * Cache the current 'parent' and 'mode' values for later use in SerializeToXmlElement()
		 * and SerializeElement().
		 * 
		 * Make sure to call base when overriding this in a derived class.
		 */
		internal virtual void PrepareSave (ConfigurationElement parent, ConfigurationSaveMode mode)
		{
			saveContext = new SaveContext (this, parent, mode);

			foreach (PropertyInformation prop in ElementInformation.Properties)
			{
				if (!prop.IsElement)
					continue;

				var elem = (ConfigurationElement)prop.Value;
				if (parent == null || !parent.HasValue (prop.Name))
					elem.PrepareSave (null, mode);
				else {
					var parentValue = (ConfigurationElement)parent [prop.Name];
					elem.PrepareSave (parentValue, mode);
				}
			}
		}

		SaveContext saveContext;

		class SaveContext {
			public readonly ConfigurationElement Element;
			public readonly ConfigurationElement Parent;
			public readonly ConfigurationSaveMode Mode;

			public SaveContext (ConfigurationElement element, ConfigurationElement parent,
			                    ConfigurationSaveMode mode)
			{
				this.Element = element;
				this.Parent = parent;
				this.Mode = mode;
			}

			public bool HasValues ()
			{
				if (Mode == ConfigurationSaveMode.Full)
					return true;
				return Element.HasValues (Parent, Mode);
			}

			public bool HasValue (PropertyInformation prop)
			{
				if (Mode == ConfigurationSaveMode.Full)
					return true;
				return Element.HasValue (Parent, prop, Mode);
			}
		}
	}
	
	internal class ElementMap
	{
		static readonly Hashtable elementMaps = Hashtable.Synchronized (new Hashtable ());

		readonly ConfigurationPropertyCollection properties;
		readonly ConfigurationCollectionAttribute collectionAttribute;

		public static ElementMap GetMap (Type t)
		{
			ElementMap map = elementMaps [t] as ElementMap;
			if (map != null) return map;
			map = new ElementMap (t);
			elementMaps [t] = map;
			return map;
		}
		
		public ElementMap (Type t)
		{
			properties = new ConfigurationPropertyCollection ();
		
			collectionAttribute = Attribute.GetCustomAttribute (t, typeof(ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute;
			
			PropertyInfo[] props = t.GetProperties (BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance);
			foreach (PropertyInfo prop in props)
			{
				ConfigurationPropertyAttribute at = Attribute.GetCustomAttribute (prop, typeof(ConfigurationPropertyAttribute)) as ConfigurationPropertyAttribute;
				if (at == null) continue;
				string name = at.Name != null ? at.Name : prop.Name;

				ConfigurationValidatorAttribute validatorAttr = Attribute.GetCustomAttribute (prop, typeof (ConfigurationValidatorAttribute)) as ConfigurationValidatorAttribute;
				ConfigurationValidatorBase validator = validatorAttr != null ? validatorAttr.ValidatorInstance : null;

				TypeConverterAttribute convertAttr = (TypeConverterAttribute) Attribute.GetCustomAttribute (prop, typeof (TypeConverterAttribute));
				TypeConverter converter = convertAttr != null ? (TypeConverter) Activator.CreateInstance (Type.GetType (convertAttr.ConverterTypeName), true) : null;
				ConfigurationProperty cp = new ConfigurationProperty (name, prop.PropertyType, at.DefaultValue, converter, validator, at.Options);

				cp.CollectionAttribute = Attribute.GetCustomAttribute (prop, typeof(ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute;				
				properties.Add (cp);
			}
		}

		public ConfigurationCollectionAttribute CollectionAttribute
		{
			get { return collectionAttribute; }
		}
		
		public bool HasProperties
		{
			get { return properties.Count > 0; }
		}
		
		public ConfigurationPropertyCollection Properties
		{
			get {
				return properties;
			}
		}
	}
}

