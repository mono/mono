// 
// System.Xml.Serialization.SoapReflectionImporter 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Collections;

namespace System.Xml.Serialization {
	public class SoapReflectionImporter {

		SoapAttributeOverrides attributeOverrides;
		string initialDefaultNamespace;
		ArrayList includedTypes;
		ArrayList relatedMaps = new ArrayList ();
		ReflectionHelper helper = new ReflectionHelper();
		internal const string EncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";

		#region Constructors

		public SoapReflectionImporter (): this (null, null)
		{ 
		}

		public SoapReflectionImporter (SoapAttributeOverrides attributeOverrides): this (attributeOverrides, null)
		{ 
		}

		public SoapReflectionImporter (string defaultNamespace): this (null, defaultNamespace)
		{
		}

		public SoapReflectionImporter (SoapAttributeOverrides attributeOverrides, string defaultNamespace)
		{ 
			if (defaultNamespace == null) initialDefaultNamespace = String.Empty;
			else initialDefaultNamespace = defaultNamespace;

			if (attributeOverrides == null) this.attributeOverrides = new SoapAttributeOverrides();
			else this.attributeOverrides = attributeOverrides;
		}

		#endregion // Constructors

		#region Methods

		public XmlMembersMapping ImportMembersMapping (string elementName, string ns, XmlReflectionMember[] members)
		{
			return ImportMembersMapping (elementName, ns, members, true, true, false);
		}

		public XmlMembersMapping ImportMembersMapping (string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool writeAccessors)
		{ 
			return ImportMembersMapping (elementName, ns, members, hasWrapperElement, writeAccessors, false);
		}

		public XmlMembersMapping ImportMembersMapping (string elementName, string ns, XmlReflectionMember[] members, bool hasWrapperElement, bool writeAccessors, bool validate)
		{ 
			XmlMemberMapping[] mapping = new XmlMemberMapping[members.Length];
			for (int n=0; n<members.Length; n++)
			{
				XmlTypeMapMember mapMem = CreateMapMember (members[n], ns);
				mapping[n] = new XmlMemberMapping (members[n], mapMem);
			}
			XmlMembersMapping mps = new XmlMembersMapping (elementName, ns, hasWrapperElement, mapping);
			mps.RelatedMaps = relatedMaps;
			mps.Format = SerializationFormat.Encoded;
			return mps;
		}

		public XmlTypeMapping ImportTypeMapping (Type type)
		{ 
			return ImportTypeMapping (type, null);
		}

		public XmlTypeMapping ImportTypeMapping (Type type, string defaultNamespace)
		{ 
			if (type == null)
				throw new ArgumentNullException ("type");

			if (type == typeof (void))
				throw new InvalidOperationException ("Type " + type.Name + " may not be serialized.");

			if (defaultNamespace == null) defaultNamespace = initialDefaultNamespace;
			if (defaultNamespace == null) defaultNamespace = string.Empty;

			XmlTypeMapping map;
			switch (TypeTranslator.GetTypeData(type).SchemaType)
			{
				case SchemaTypes.Class: map = ImportClassMapping (type, defaultNamespace); break;
				case SchemaTypes.Array: map = ImportListMapping (type, defaultNamespace); break;
				case SchemaTypes.XmlNode: throw CreateTypeException (type);
				case SchemaTypes.Primitive: map = ImportPrimitiveMapping (type, defaultNamespace); break;
				case SchemaTypes.Enum: map = ImportEnumMapping (type, defaultNamespace); break;
				case SchemaTypes.XmlSerializable:
				default: throw new NotSupportedException ("Type " + type.FullName + " not supported for XML stialization");
			}
			map.RelatedMaps = relatedMaps;
			map.Format = SerializationFormat.Encoded;
			return map;
		}

		XmlTypeMapping CreateTypeMapping (TypeData typeData, string defaultXmlType, string defaultNamespace)
		{
			string membersNamespace = defaultNamespace;
			string elementName;
			SoapAttributes atts = null;
			if (defaultXmlType == null) defaultXmlType = typeData.XmlType;

			if (!typeData.IsListType)
			{
				if (attributeOverrides != null) 
					atts = attributeOverrides[typeData.Type];

				if (atts != null && typeData.SchemaType == SchemaTypes.Primitive)
					throw new InvalidOperationException ("SoapType attribute may not be specified for the type " + typeData.FullTypeName);
			}

			if (atts == null) 
				atts = new SoapAttributes (typeData.Type);

			if (atts.SoapType != null)
			{
				if (atts.SoapType.Namespace != null && atts.SoapType.Namespace != string.Empty)
					membersNamespace = atts.SoapType.Namespace;

				if (atts.SoapType.TypeName != null && atts.SoapType.TypeName != string.Empty)
					defaultXmlType = atts.SoapType.TypeName;
			}

			elementName = defaultXmlType;

			if (membersNamespace == null) membersNamespace = "";
			XmlTypeMapping map = new XmlTypeMapping (elementName, membersNamespace, typeData, defaultXmlType);

			relatedMaps.Add (map);
			return map;
		}

		XmlTypeMapping ImportClassMapping (Type type, string defaultNamespace)
		{
			if (type.IsValueType) throw CreateStructException (type);
			if (type == typeof (object)) defaultNamespace = XmlSchema.Namespace;

			TypeData typeData = TypeTranslator.GetTypeData (type);
			XmlTypeMapping map = helper.GetRegisteredClrType (type, defaultNamespace);
			if (map != null) return map;

			map = CreateTypeMapping (typeData, null, defaultNamespace);
			helper.RegisterClrType (map, type, defaultNamespace);
			map.MultiReferenceType = true;

			ClassMap classMap = new ClassMap ();
			map.ObjectMap = classMap;

			// Import members

			try
			{
				ICollection members = GetReflectionMembers (type);
				foreach (XmlReflectionMember rmember in members)
				{
					if (rmember.SoapAttributes.SoapIgnore) continue;
					classMap.AddMember (CreateMapMember (rmember, map.Namespace));
				}
			}
			catch (Exception ex) 
			{
				throw helper.CreateError (map, ex.Message);
			}

			// Import derived classes

			SoapIncludeAttribute[] includes = (SoapIncludeAttribute[])type.GetCustomAttributes (typeof (SoapIncludeAttribute), false);
			for (int n=0; n<includes.Length; n++)
			{
				Type includedType = includes[n].Type;
				if (!includedType.IsSubclassOf(type)) throw helper.CreateError (map, "Type '" + includedType.FullName + "' is not a subclass of '" + type.FullName + "'");

				XmlTypeMapping derived = ImportTypeMapping (includedType, defaultNamespace);
				map.DerivedTypes.Add (derived);
				if (type != typeof (object)) derived.BaseMap = map;
				map.DerivedTypes.AddRange (derived.DerivedTypes);
			}

			if (type == typeof (object) && includedTypes != null)
			{
				foreach (Type intype in includedTypes)
					map.DerivedTypes.Add (ImportTypeMapping (intype));
			}

			// Register this map as a derived class of object

			if (typeData.Type != typeof(object))
				ImportTypeMapping (typeof(object)).DerivedTypes.Add (map);
			
			return map;
		}

		XmlTypeMapping ImportListMapping (Type type, string defaultNamespace)
		{
			TypeData typeData = TypeTranslator.GetTypeData (type);
			XmlTypeMapping map = helper.GetRegisteredClrType (type, EncodingNamespace);
			if (map != null) return map;

			ListMap obmap = new ListMap ();

			map = CreateTypeMapping (typeData, "Array", EncodingNamespace);
			helper.RegisterClrType (map, type, EncodingNamespace);
			map.MultiReferenceType = true;
			map.ObjectMap = obmap;

			Type itemType = typeData.ListItemType;
			TypeData itemTypeData = TypeTranslator.GetTypeData (itemType);
			XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (null, itemTypeData);
			
			if (elem.TypeData.IsComplexType) {
				elem.MappedType = ImportTypeMapping (itemType);
				elem.TypeData = elem.MappedType.TypeData;
			}
				
			elem.ElementName = "Item";
			elem.Namespace = string.Empty;
			elem.IsNullable = true;	// By default, items are nullable

			XmlTypeMapElementInfoList list = new XmlTypeMapElementInfoList();
			list.Add (elem);

			obmap.ItemInfo = list;
			ImportTypeMapping (typeof(object)).DerivedTypes.Add (map);

			return map;
		}
		
		XmlTypeMapping ImportPrimitiveMapping (Type type, string defaultNamespace)
		{
			XmlTypeMapping map = helper.GetRegisteredClrType (type, defaultNamespace);
			if (map != null) return map;
			map = CreateTypeMapping (TypeTranslator.GetTypeData (type), null, defaultNamespace);
			helper.RegisterClrType (map, type, defaultNamespace);
			return map;
		}


		XmlTypeMapping ImportEnumMapping (Type type, string defaultNamespace)
		{
			XmlTypeMapping map = helper.GetRegisteredClrType (type, defaultNamespace);
			if (map != null) return map;
			map = CreateTypeMapping (TypeTranslator.GetTypeData (type), null, defaultNamespace);
			helper.RegisterClrType (map, type, defaultNamespace);

			string [] names = Enum.GetNames (type);
			EnumMap.EnumMapMember[] members = new EnumMap.EnumMapMember[names.Length];
			for (int n=0; n<names.Length; n++)
			{
				MemberInfo[] mem = type.GetMember (names[n]);
				string xmlName = names[n];
				object[] atts = mem[0].GetCustomAttributes (typeof(SoapEnumAttribute), false);
				if (atts.Length > 0) xmlName = ((SoapEnumAttribute)atts[0]).Name;
				members[n] = new EnumMap.EnumMapMember (xmlName, names[n]);
			}

			bool isFlags = type.GetCustomAttributes (typeof(FlagsAttribute),false).Length > 0;
			map.ObjectMap = new EnumMap (members, isFlags);
			ImportTypeMapping (typeof(object)).DerivedTypes.Add (map);
			return map;
		}

		public ICollection GetReflectionMembers (Type type)
		{
			ArrayList members = new ArrayList();
			PropertyInfo[] properties = type.GetProperties (BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo prop in properties)
			{
				if (!prop.CanRead) continue;
				SoapAttributes atts = attributeOverrides[type, prop.Name];
				if (atts == null) atts = new SoapAttributes (prop);
				if (atts.SoapIgnore) continue;
				XmlReflectionMember member = new XmlReflectionMember(prop.Name, prop.PropertyType, atts);
				members.Add (member);
			}

			FieldInfo[] fields = type.GetFields (BindingFlags.Instance | BindingFlags.Public);
			foreach (FieldInfo field in fields)
			{
				SoapAttributes atts = attributeOverrides[type, field.Name];
				if (atts == null) atts = new SoapAttributes (field);
				if (atts.SoapIgnore) continue;
				XmlReflectionMember member = new XmlReflectionMember(field.Name, field.FieldType, atts);
				members.Add (member);
			}
			return members;
		}
		
		private XmlTypeMapMember CreateMapMember (XmlReflectionMember rmember, string defaultNamespace)
		{
			XmlTypeMapMember mapMember;
			SoapAttributes atts = rmember.SoapAttributes;
			TypeData typeData = TypeTranslator.GetTypeData (rmember.MemberType);

			if (atts.SoapAttribute != null)
			{
				// An attribute

				if (atts.SoapElement != null)
					throw new Exception ("SoapAttributeAttribute and SoapElementAttribute cannot be applied to the same member");

				XmlTypeMapMemberAttribute mapAttribute = new XmlTypeMapMemberAttribute ();
				if (atts.SoapAttribute.AttributeName == null) 
					mapAttribute.AttributeName = rmember.MemberName;
				else 
					mapAttribute.AttributeName = atts.SoapAttribute.AttributeName;

				mapAttribute.Namespace = (atts.SoapAttribute.Namespace != null) ? atts.SoapAttribute.Namespace : "";
				if (typeData.IsComplexType)
					mapAttribute.MappedType = ImportTypeMapping (typeData.Type);

				typeData = TypeTranslator.GetTypeData (rmember.MemberType, atts.SoapAttribute.DataType);
				mapMember = mapAttribute;
			}
			else
			{
				if (typeData.SchemaType == SchemaTypes.Array) mapMember = new XmlTypeMapMemberList ();
				else mapMember = new XmlTypeMapMemberElement ();

				if (atts.SoapElement != null && atts.SoapElement.DataType != null)
					typeData = TypeTranslator.GetTypeData (rmember.MemberType, atts.SoapElement.DataType);

				// Creates an ElementInfo that identifies the element
				XmlTypeMapElementInfoList infoList = new XmlTypeMapElementInfoList();
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (mapMember, typeData);

				elem.ElementName = (atts.SoapElement != null && atts.SoapElement.ElementName != null) ? atts.SoapElement.ElementName : rmember.MemberName;
				elem.Namespace = string.Empty;
				elem.IsNullable = (atts.SoapElement != null) ? atts.SoapElement.IsNullable : false;
				if (typeData.IsComplexType)
					elem.MappedType = ImportTypeMapping (typeData.Type);
				
				infoList.Add (elem);
				((XmlTypeMapMemberElement)mapMember).ElementInfo = infoList;
			}

			mapMember.TypeData = typeData;
			mapMember.Name = rmember.MemberName;
			return mapMember;
		}
		
		public void IncludeType (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (includedTypes == null) includedTypes = new ArrayList ();
			includedTypes.Add (type);
		}

		[MonoTODO]
		public void IncludeTypes (ICustomAttributeProvider provider)
		{ 
			throw new NotImplementedException ();
		}

		Exception CreateTypeException (Type type)
		{
			return new NotSupportedException ("The type " + type.FullName + " may not be serialized with SOAP-encoded messages. Set the Use for your message to Literal");
		}

		Exception CreateStructException (Type type)
		{
			return new NotSupportedException ("Cannot serialize " + type.FullName + ". Nested structs are not supported with encoded SOAP");
		}


		

		#endregion // Methods
	}
}
