// 
// System.Xml.Serialization.XmlReflectionImporter 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Erik LeBel (eriklebel@yahoo.ca)
//
// Copyright (C) Tim Coleman, 2002
// (C) 2003 Erik LeBel
//

using System.Reflection;

namespace System.Xml.Serialization {
	public class XmlReflectionImporter {

		string defaultNamespace;
		XmlAttributeOverrides attributeOverrides;
		TypeTranslator typeTranslator =	new TypeTranslator ();

		#region Constructors

		public XmlReflectionImporter ()
			: this (null, null)
		{
		}

		public XmlReflectionImporter (string defaultNamespace)
			: this (null, defaultNamespace)
		{
		}

		public XmlReflectionImporter (XmlAttributeOverrides attributeOverrides)
			: this (attributeOverrides, null)
		{
		}

		public XmlReflectionImporter (XmlAttributeOverrides attributeOverrides, string defaultNamespace)
		{
			if (defaultNamespace == null)
				this.defaultNamespace = String.Empty;
			else
				this.defaultNamespace = defaultNamespace;

			if (attributeOverrides == null)
				this.attributeOverrides = new XmlAttributeOverrides();
			else
				this.attributeOverrides = attributeOverrides;
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public XmlMembersMapping ImportMembersMapping (string elementName,
							       string ns,
							       XmlReflectionMember [] members,
							       bool hasWrapperElement)
		{
			throw new NotImplementedException ();
		}

		public XmlTypeMapping ImportTypeMapping (Type type)
		{
			return ImportTypeMapping (type, null, null);
		}

		public XmlTypeMapping ImportTypeMapping (Type type, string defaultNamespace)
		{
			return ImportTypeMapping (type, null, defaultNamespace);
		}

		public XmlTypeMapping ImportTypeMapping (Type type, XmlRootAttribute group)
		{
			return ImportTypeMapping (type, group, null);
		}

		public XmlTypeMapping ImportTypeMapping (Type type, XmlRootAttribute group, string defaultNamespace)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (type == typeof (void))
				throw new InvalidOperationException ("Type " + type.Name +
								      " may not be serialized.");

			XmlAttributes atts = new XmlAttributes (type);
			TypeData data = typeTranslator.GetTypeData (type);
			string elementName = data.ElementName;
			string typeName = data.TypeName;
			string typeFullName = data.FullTypeName;
			string nameSpc = (defaultNamespace != null) ? defaultNamespace : this.defaultNamespace;

			if (group != null)
			{
				if (group.ElementName != null && group.ElementName != String.Empty)
					elementName = group.ElementName;
				if (group.Namespace != null && group.Namespace != String.Empty)
					nameSpc = group.Namespace;
			}
			
			return new XmlTypeMapping (elementName, nameSpc, typeFullName, typeName);
		}

		private XmlTypeMapping ImportTypeMapping (TypeData data, string ns)
		{
			return ImportTypeMapping (data.Type, null, ns);
		}

		public void IncludeType (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			TypeData data = typeTranslator.GetTypeData (type);
			ImportTypeMapping (data, defaultNamespace);
		}

		public void IncludeTypes (ICustomAttributeProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");

			Type ixml = typeof (IXmlSerializable);
			object [] customAttrs = provider.GetCustomAttributes (typeof (XmlIncludeAttribute), false);
			foreach (XmlIncludeAttribute att in customAttrs) {
				Type type = att.Type;
				if (ixml.IsAssignableFrom (type)) {
					string fmt = "Type {0} is derived from {1} and therefore cannot " +
						     "be used with attribute XmlInclude";
					throw new InvalidOperationException (String.Format (fmt, type, ixml));
				}
				IncludeType (type);
			}
		}

		#endregion // Methods
	}
}
