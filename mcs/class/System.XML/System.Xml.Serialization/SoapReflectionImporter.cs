// 
// System.Xml.Serialization.SoapReflectionImporter 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

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
				mapping[n] = new XmlMemberMapping (members[n].MemberName, ns, mapMem, true);
			}
			XmlMembersMapping mps = new XmlMembersMapping (elementName, ns, hasWrapperElement, writeAccessors, mapping);
			mps.RelatedMaps = relatedMaps;
			mps.Format = SerializationFormat.Encoded;
			Type[] extraTypes = includedTypes != null ? (Type[])includedTypes.ToArray(typeof(Type)) : null;
			mps.Source = new MembersSerializationSource (elementName, hasWrapperElement, members, writeAccessors, false, null, extraTypes);
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
				default: throw new NotSupportedException ("Type " + type.FullName + " not supported for XML serialization");
			}
			map.RelatedMaps = relatedMaps;
			map.Format = SerializationFormat.Encoded;
			Type[] extraTypes = includedTypes != null ? (Type[])includedTypes.ToArray(typeof(Type)) : null;
			map.Source = new SoapTypeSerializationSource (type, attributeOverrides, defaultNamespace, extraTypes);
			return map;
		}

		XmlTypeMapping CreateTypeMapping (TypeData typeData, string defaultXmlType, string defaultNamespace)
		{
			string membersNamespace = defaultNamespace;
			bool includeInSchema = true;

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

				includeInSchema = atts.SoapType.IncludeInSchema;
			}

			if (membersNamespace == null) membersNamespace = "";
			XmlTypeMapping map = new XmlTypeMapping (defaultXmlType, membersNamespace, typeData, defaultXmlType, membersNamespace);
			map.IncludeInSchema = includeInSchema;
			relatedMaps.Add (map);

			return map;
		}

		XmlTypeMapping ImportClassMapping (Type type, string defaultNamespace)
		{
			if (type.IsValueType) throw CreateStructException (type);
			if (type == typeof (object)) defaultNamespace = XmlSchema.Namespace;

			ReflectionHelper.CheckSerializableType (type, false);
				
			TypeData typeData = TypeTranslator.GetTypeData (type);
			XmlTypeMapping map = helper.GetRegisteredClrType (type, GetTypeNamespace (typeData, defaultNamespace));
			if (map != null) return map;

			map = CreateTypeMapping (typeData, null, defaultNamespace);
			helper.RegisterClrType (map, type, map.Namespace);
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

			// Import included classes

			SoapIncludeAttribute[] includes = (SoapIncludeAttribute[])type.GetCustomAttributes (typeof (SoapIncludeAttribute), false);
			for (int n=0; n<includes.Length; n++)
			{
				Type includedType = includes[n].Type;
				ImportTypeMapping (includedType);
			}

			if (type == typeof (object) && includedTypes != null)
			{
				foreach (Type intype in includedTypes)
					map.DerivedTypes.Add (ImportTypeMapping (intype));
			}

			// Register inheritance relations

			if (type.BaseType != null)
			{
				XmlTypeMapping bmap = ImportClassMapping (type.BaseType, defaultNamespace);
				
				if (type.BaseType != typeof (object))
					map.BaseMap = bmap;
					
				// At this point, derived classes of this map must be already registered
				
				RegisterDerivedMap (bmap, map);
			}
			
			return map;
		}
		
		void RegisterDerivedMap (XmlTypeMapping map, XmlTypeMapping derivedMap)
		{
			map.DerivedTypes.Add (derivedMap);
			map.DerivedTypes.AddRange (derivedMap.DerivedTypes);
			
			if (map.BaseMap != null)
				RegisterDerivedMap (map.BaseMap, derivedMap);
			else {
				XmlTypeMapping obmap = ImportTypeMapping (typeof(object));
				if (obmap != map)
					obmap.DerivedTypes.Add (derivedMap);
			}
		}

		string GetTypeNamespace (TypeData typeData, string defaultNamespace)
		{
			string membersNamespace = defaultNamespace;

			SoapAttributes atts = null;

			if (!typeData.IsListType)
			{
				if (attributeOverrides != null)
					atts = attributeOverrides[typeData.Type];
			}

			if (atts == null)
				atts = new SoapAttributes (typeData.Type);

			if (atts.SoapType != null)
			{
				if (atts.SoapType.Namespace != null && atts.SoapType.Namespace != string.Empty)
					membersNamespace = atts.SoapType.Namespace;
			}

			if (membersNamespace == null) return "";
			else return membersNamespace;
		}
		
		XmlTypeMapping ImportListMapping (Type type, string defaultNamespace)
		{
			TypeData typeData = TypeTranslator.GetTypeData (type);
			XmlTypeMapping map = helper.GetRegisteredClrType (type, XmlSerializer.EncodingNamespace);
			if (map != null) return map;

			ListMap obmap = new ListMap ();
			TypeData itemTypeData = typeData.ListItemTypeData;

			map = CreateTypeMapping (typeData, "Array", XmlSerializer.EncodingNamespace);
			helper.RegisterClrType (map, type, XmlSerializer.EncodingNamespace);
			map.MultiReferenceType = true;
			map.ObjectMap = obmap;

			XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (null, itemTypeData);
			
			if (elem.TypeData.IsComplexType) {
				elem.MappedType = ImportTypeMapping (typeData.ListItemType, defaultNamespace);
				elem.TypeData = elem.MappedType.TypeData;
			}
				
			elem.ElementName = "Item";
			elem.Namespace = string.Empty;
			elem.IsNullable = true;	// By default, items are nullable

			XmlTypeMapElementInfoList list = new XmlTypeMapElementInfoList();
			list.Add (elem);

			obmap.ItemInfo = list;
			XmlTypeMapping objMap = ImportTypeMapping (typeof(object), defaultNamespace);
			objMap.DerivedTypes.Add (map);

			// Register any of the including types as a derived class of object
			SoapIncludeAttribute[] includes = (SoapIncludeAttribute[])type.GetCustomAttributes (typeof (SoapIncludeAttribute), false);
			for (int i = 0; i < includes.Length; i++)
			{
				Type includedType = includes[i].Type;
				objMap.DerivedTypes.Add(ImportTypeMapping (includedType, defaultNamespace));
			}
			
			return map;
		}
		
		XmlTypeMapping ImportPrimitiveMapping (Type type, string defaultNamespace)
		{
			TypeData typeData = TypeTranslator.GetTypeData (type);
			XmlTypeMapping map = helper.GetRegisteredClrType (type, GetTypeNamespace (typeData, defaultNamespace));
			if (map != null) return map;
			map = CreateTypeMapping (typeData, null, defaultNamespace);
			helper.RegisterClrType (map, type, map.Namespace);
			return map;
		}


		XmlTypeMapping ImportEnumMapping (Type type, string defaultNamespace)
		{
			TypeData typeData = TypeTranslator.GetTypeData (type);
			XmlTypeMapping map = helper.GetRegisteredClrType (type, GetTypeNamespace (typeData, defaultNamespace));
			if (map != null) return map;
			
			ReflectionHelper.CheckSerializableType (type, false);
				
			map = CreateTypeMapping (typeData, null, defaultNamespace);
			helper.RegisterClrType (map, type, map.Namespace);

			map.MultiReferenceType = true;
			
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
			ImportTypeMapping (typeof(object), defaultNamespace).DerivedTypes.Add (map);
			return map;
		}

		ICollection GetReflectionMembers (Type type)
		{
			ArrayList members = new ArrayList();
			PropertyInfo[] properties = type.GetProperties (BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo prop in properties)
			{
				if (!prop.CanRead) continue;
				if (!prop.CanWrite && TypeTranslator.GetTypeData (prop.PropertyType).SchemaType != SchemaTypes.Array)
					continue;
					
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
					mapAttribute.MappedType = ImportTypeMapping (typeData.Type, defaultNamespace);

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
					elem.MappedType = ImportTypeMapping (typeData.Type, defaultNamespace);
				
				infoList.Add (elem);
				((XmlTypeMapMemberElement)mapMember).ElementInfo = infoList;
			}

			mapMember.TypeData = typeData;
			mapMember.Name = rmember.MemberName;
			mapMember.IsReturnValue = rmember.IsReturnValue;
			return mapMember;
		}
		
		public void IncludeType (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (includedTypes == null) includedTypes = new ArrayList ();
			if (!includedTypes.Contains (type))
				includedTypes.Add (type);
		}

		public void IncludeTypes (ICustomAttributeProvider provider)
		{ 
			object[] ats = provider.GetCustomAttributes (typeof(SoapIncludeAttribute), true);
			foreach (SoapIncludeAttribute at in ats)
				IncludeType (at.Type);
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
