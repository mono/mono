// 
// System.Xml.Serialization.XmlReflectionImporter 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Erik LeBel (eriklebel@yahoo.ca)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
// (C) 2003 Erik LeBel
//

using System.Reflection;
using System.Collections;

namespace System.Xml.Serialization {
	public class XmlReflectionImporter {

		string defaultNamespace;
		XmlAttributeOverrides attributeOverrides;
		ArrayList includedTypes;
		Hashtable clrTypes = new Hashtable ();
		Hashtable schemaTypes = new Hashtable ();
		int arrayChoiceCount = 1;

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

		public XmlTypeMapping ImportTypeMapping (Type type)
		{
			return ImportTypeMapping (type, null, "");
		}

		public XmlTypeMapping ImportTypeMapping (Type type, string defaultNamespace)
		{
			return ImportTypeMapping (type, null, defaultNamespace);
		}

		public XmlTypeMapping ImportTypeMapping (Type type, XmlRootAttribute group)
		{
			return ImportTypeMapping (type, group, "");
		}

		public XmlTypeMapping ImportTypeMapping (Type type, XmlRootAttribute root, string defaultNamespace)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (type == typeof (void))
				throw new InvalidOperationException ("Type " + type.Name + " may not be serialized.");

			switch (TypeTranslator.GetTypeData(type).SchemaType)
			{
				case SchemaTypes.Class: return ImportClassMapping (type, root, defaultNamespace);
				case SchemaTypes.Array: return ImportListMapping (type, root, defaultNamespace, null, 0);
				case SchemaTypes.XmlNode: return ImportXmlNodeMapping (type, root, defaultNamespace);
				case SchemaTypes.DataSet:
				default: throw new NotSupportedException ("Type " + type.FullName + " not supported for XML stialization");
			}
		}

		XmlTypeMapping CreateTypeMapping (TypeData typeData, XmlRootAttribute root, string defaultXmlType, string defaultNamespace)
		{
			string membersNamespace = defaultNamespace;
			string elementName;
			XmlAttributes atts = null;
			if (defaultXmlType == null) defaultXmlType = typeData.ElementName;

			if (!typeData.IsListType)
			{
				if (attributeOverrides != null) 
					atts = attributeOverrides[typeData.Type];
			}

			if (atts == null) 
				atts = new XmlAttributes (typeData.Type);

			if (atts.XmlRoot != null && root == null)
				root = atts.XmlRoot;

			if (atts.XmlType != null)
			{
				if (atts.XmlType.Namespace != null && atts.XmlType.Namespace != string.Empty)
					membersNamespace = atts.XmlType.Namespace;

				if (atts.XmlType.TypeName != null && atts.XmlType.TypeName != string.Empty)
					defaultXmlType = atts.XmlType.TypeName;
			}

			elementName = defaultXmlType;

			if (root != null)
			{
				if (root.ElementName != null && root.ElementName != String.Empty)
					elementName = root.ElementName;
				if (root.Namespace != null && root.Namespace != String.Empty)
					membersNamespace = root.Namespace;
			}

			if (membersNamespace == null) membersNamespace = "";
			XmlTypeMapping map = new XmlTypeMapping (elementName, membersNamespace, typeData, defaultXmlType);
			return map;
		}

		XmlTypeMapping ImportClassMapping (Type type, XmlRootAttribute root, string defaultNamespace)
		{
			TypeData typeData = TypeTranslator.GetTypeData (type);
			XmlTypeMapping map = GetRegisteredClrType (type, defaultNamespace);
			if (map != null) return map;

			map = CreateTypeMapping (typeData, root, null, defaultNamespace);
			
			RegisterClrType (map, type, defaultNamespace);
			RegisterSchemaType (map, map.XmlType, defaultNamespace);

			map.ObjectMap = new ClassMap ();

			// Import members

			ICollection members = GetReflectionMembers (type);
			foreach (XmlReflectionMember rmember in members)
			{
				if (rmember.XmlAttributes.XmlIgnore) continue;
				AddMember (map, rmember, map.Namespace);
			}

			// Import derived classes

			XmlIncludeAttribute[] includes = (XmlIncludeAttribute[])type.GetCustomAttributes (typeof (XmlIncludeAttribute), false);
			for (int n=0; n<includes.Length; n++)
			{
				Type includedType = includes[n].Type;
				if (!includedType.IsSubclassOf(type)) throw CreateError (map, "Type '" + includedType.FullName + "' is not a subclass of '" + type.FullName + "'");
				map.DerivedTypes.Add (ImportTypeMapping (includedType, root, defaultNamespace));
			}

			if (type == typeof (object) && includedTypes != null)
			{
				foreach (Type intype in includedTypes)
					map.DerivedTypes.Add (ImportTypeMapping (intype, defaultNamespace));
			}

			// Register this map as a derived class of object

			if (typeData.Type != typeof(object))
				ImportTypeMapping (typeof(object)).DerivedTypes.Add (map);
			
			return map;
		}


		XmlTypeMapping ImportListMapping (Type type, XmlRootAttribute root, string defaultNamespace, XmlAttributes atts, int nestingLevel)
		{
			TypeData typeData = TypeTranslator.GetTypeData (type);
			ListMap obmap = new ListMap ();

			if (atts == null) atts = new XmlAttributes();
			Type itemType = typeData.ListItemType;
			bool isMultiArray = (type.IsArray && itemType.IsArray);

			XmlTypeMapElementInfoList list = new XmlTypeMapElementInfoList();

			foreach (XmlArrayItemAttribute att in atts.XmlArrayItems)
			{
				if (att.NestingLevel != nestingLevel) continue;
				Type elemType = (att.Type != null) ? att.Type : itemType;
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (null, TypeTranslator.GetTypeData(elemType));
				elem.DataType = att.DataType;
				elem.Namespace = att.Namespace != null ? att.Namespace : "";
				elem.Form = att.Form;
				elem.IsNullable = att.IsNullable;
				elem.NestingLevel = att.NestingLevel;

				if (isMultiArray)
					elem.MappedType = ImportListMapping (elemType, null, elem.Namespace, atts, nestingLevel + 1);
				else if (elem.TypeData.IsComplexType)
					elem.MappedType = ImportTypeMapping (elemType, null, elem.Namespace);

				if (att.ElementName != null) elem.ElementName = att.ElementName;
				else if (elem.MappedType != null) elem.ElementName = elem.MappedType.ElementName;
				else elem.ElementName = TypeTranslator.GetTypeData(elemType).ElementName;

				list.Add (elem);
			}

			if (list.Count == 0)
			{
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (null, TypeTranslator.GetTypeData (itemType));
				if (isMultiArray)
					elem.MappedType = ImportListMapping (itemType, null, defaultNamespace, atts, nestingLevel + 1);
				else if (elem.TypeData.IsComplexType)
					elem.MappedType = ImportTypeMapping (itemType, null, defaultNamespace);

				if (elem.MappedType != null) elem.ElementName = elem.MappedType.ElementName;
				else elem.ElementName = TypeTranslator.GetTypeData(itemType).ElementName ;

				elem.Namespace = (defaultNamespace != null) ? defaultNamespace : "";
				elem.IsNullable = true;	// By default, items are nullable
				list.Add (elem);
			}

			obmap.ItemInfo = list;

			// If there can be different element names (types) in the array, then its name cannot
			// be "ArrayOfXXX" it must be something like ArrayOfChoiceNNN

			string baseName;
			if (list.Count > 1)
				baseName = "ArrayOfChoice" + (arrayChoiceCount++);
			else
			{
				XmlTypeMapElementInfo elem = ((XmlTypeMapElementInfo)list[0]);
				if (elem.MappedType != null) baseName = "ArrayOf" + elem.MappedType.ElementName;
				else baseName = "ArrayOf" + elem.ElementName;
			}

			// Avoid name colisions

			int nameCount = 1;
			string name = baseName;

			do {
				XmlTypeMapping foundMap = GetRegisteredSchemaType (name, defaultNamespace);
				if (foundMap == null) nameCount = -1;
				else if (obmap.Equals (foundMap.ObjectMap)) return foundMap;
				else name = baseName + (nameCount++);
			}
			while (nameCount != -1);

			XmlTypeMapping map = CreateTypeMapping (typeData, root, name, defaultNamespace);
			map.ObjectMap = obmap;

			// Register this map as a derived class of object

			RegisterSchemaType (map, name, defaultNamespace);
			if (typeData.Type != typeof(object))
				ImportTypeMapping (typeof(object)).DerivedTypes.Add (map);

			return map;
		}

		XmlTypeMapping ImportXmlNodeMapping (Type type, XmlRootAttribute root, string defaultNamespace)
		{
			XmlTypeMapping map = CreateTypeMapping (TypeTranslator.GetTypeData (type), root, null, defaultNamespace);
			return map;
		}


		public ICollection GetReflectionMembers (Type type)
		{
			ArrayList members = new ArrayList();
			PropertyInfo[] properties = type.GetProperties (BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo prop in properties)
			{
				if (!prop.CanRead || !prop.CanWrite) continue;
				XmlAttributes atts = attributeOverrides[type, prop.Name];
				if (atts == null) atts = new XmlAttributes (prop);
				if (atts.XmlIgnore) continue;
				XmlReflectionMember member = new XmlReflectionMember(prop.Name, prop.PropertyType, atts);
				members.Add (member);
			}

			FieldInfo[] fields = type.GetFields (BindingFlags.Instance | BindingFlags.Public);
			foreach (FieldInfo field in fields)
			{
				XmlAttributes atts = attributeOverrides[type, field.Name];
				if (atts == null) atts = new XmlAttributes (field);
				if (atts.XmlIgnore) continue;
				XmlReflectionMember member = new XmlReflectionMember(field.Name, field.FieldType, atts);
				members.Add (member);
			}
			return members;
		}
		
		private void AddMember (XmlTypeMapping map, XmlReflectionMember rmember, string defaultNamespace)
		{
			XmlTypeMapMember mapMember;
			XmlAttributes atts = rmember.XmlAttributes;
			TypeData typeData = TypeTranslator.GetTypeData (rmember.MemberType);

			if (atts.XmlAnyAttribute != null)
			{
			}
			else if (atts.XmlAnyElements != null)
			{
			}
			if (atts.XmlAttribute != null)
			{
				// An attribute

				if (atts.XmlElements != null && atts.XmlElements.Count > 0)
					throw CreateError (map, "XmlAttributeAttribute and XmlElementAttribute cannot be applied to the same member");

				XmlTypeMapMemberAttribute mapAttribute = new XmlTypeMapMemberAttribute ();
				if (atts.XmlAttribute.AttributeName == null) 
					mapAttribute.AttributeName = rmember.MemberName;
				else 
					mapAttribute.AttributeName = atts.XmlAttribute.AttributeName;

				mapAttribute.DataType = atts.XmlAttribute.DataType;
				mapAttribute.Form = atts.XmlAttribute.Form;
				mapAttribute.Namespace = (atts.XmlAttribute != null) ? atts.XmlAttribute.Namespace : "";
				mapMember = mapAttribute;
			}
			else if (typeData.SchemaType == SchemaTypes.Array)
			{
				if (atts.XmlElements.Count > 0)
				{
					// A flat list

					// TODO: check that it does not have XmlArrayAttribute
					XmlTypeMapMemberFlatList member = new XmlTypeMapMemberFlatList ();
					member.ListMap = new ListMap ();
					member.ListMap.ItemInfo = ImportElementInfo (rmember.MemberName, defaultNamespace, typeData.ListItemType, member, atts);
					member.ElementInfo = member.ListMap.ItemInfo;
					mapMember = member;
				}
				else
				{
					// A list

					XmlTypeMapMemberList member = new XmlTypeMapMemberList ();
					member.ElementName = (atts.XmlArray != null && atts.XmlArray.ElementName != null) ? atts.XmlArray.ElementName : rmember.MemberName;
					member.Namespace = (atts.XmlArray != null && atts.XmlArray.Namespace != null) ? atts.XmlArray.Namespace : defaultNamespace;
					member.ListTypeMapping = ImportListMapping (rmember.MemberType, null, member.Namespace, atts, 0);

					// Creates an ElementInfo that identifies the array instance. 
					member.ElementInfo = new XmlTypeMapElementInfoList();
					XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (member, typeData);
					elem.ElementName = member.ElementName;
					elem.Namespace = member.Namespace;
					elem.MappedType = member.ListTypeMapping;
					member.ElementInfo.Add (elem);
					mapMember = member;
				}
			}
			else
			{
				// An element

				XmlTypeMapMemberElement member = new XmlTypeMapMemberElement ();
				member.ElementInfo = ImportElementInfo (rmember.MemberName, defaultNamespace, rmember.MemberType, member, atts);
				mapMember = member;
			}

			mapMember.TypeData = typeData;
			mapMember.Name = rmember.MemberName;
			((ClassMap)map.ObjectMap).AddMember (mapMember);
		}

		XmlTypeMapElementInfoList ImportElementInfo (string defaultName, string defaultNamespace, Type defaultType, XmlTypeMapMemberElement member, XmlAttributes atts)
		{
			XmlTypeMapElementInfoList list = new XmlTypeMapElementInfoList();

			if (atts.XmlElements.Count == 0)
			{
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (member, TypeTranslator.GetTypeData(defaultType));
				elem.ElementName = defaultName;
				elem.Namespace = defaultNamespace;
				if (elem.TypeData.IsComplexType)
					elem.MappedType = ImportTypeMapping (defaultType, null, defaultNamespace);
				list.Add (elem);
			}

			foreach (XmlElementAttribute att in atts.XmlElements)
			{
				Type elemType = (att.Type != null) ? att.Type : defaultType;
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (member, TypeTranslator.GetTypeData(elemType));
				elem.ElementName = (att.ElementName != null) ? att.ElementName : defaultName;
				elem.DataType = att.DataType;
				elem.Namespace = (att.Namespace != null) ? att.Namespace : defaultNamespace;
				elem.Form = att.Form;
				elem.IsNullable = att.IsNullable;
				if (elem.TypeData.IsComplexType)
					elem.MappedType = ImportTypeMapping (elemType, null, elem.Namespace);
				list.Add (elem);
			}
			return list;
		}

		public void IncludeType (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (includedTypes == null) includedTypes = new ArrayList ();
			includedTypes.Add (type);
		}

		void RegisterSchemaType (XmlTypeMapping map, string xmlType, string ns)
		{
			string mapKey = xmlType + "/" + ns;
			if (!schemaTypes.ContainsKey (xmlType))
				schemaTypes.Add (mapKey, map);
		}

		XmlTypeMapping GetRegisteredSchemaType (string xmlType, string ns)
		{
			string mapKey = xmlType + "/" + ns;
			return schemaTypes[mapKey] as XmlTypeMapping;
		}

		void RegisterClrType (XmlTypeMapping map, Type type, string ns)
		{
			if (type == typeof(object)) ns = "";
			string mapKey = type.FullName + "/" + ns;
			if (!clrTypes.ContainsKey (mapKey))
				clrTypes.Add (mapKey, map);
		}

		XmlTypeMapping GetRegisteredClrType (Type type, string ns)
		{
			if (type == typeof(object)) ns = "";
			string mapKey = type.FullName + "/" + ns;
			return clrTypes[mapKey] as XmlTypeMapping;
		}

		Exception CreateError (XmlTypeMapping map, string message)
		{
			throw new InvalidOperationException ("There was an error reflecting '" + map.TypeFullName + "': " + message);
		}

		#endregion // Methods
	}
}
