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
using System.Collections;
using System.Xml.Schema;

namespace System.Xml.Serialization {
	public class XmlReflectionImporter {

		string initialDefaultNamespace;
		XmlAttributeOverrides attributeOverrides;
		ArrayList includedTypes;
		ReflectionHelper helper = new ReflectionHelper();
		int arrayChoiceCount = 1;
		ArrayList relatedMaps = new ArrayList ();
		bool allowPrivateTypes = false;

		static readonly string errSimple = "Cannot serialize object of type '{0}'. Base " +
			"type '{1}' has simpleContent and can be only extended by adding XmlAttribute " +
			"elements. Please consider changing XmlText member of the base class to string array";

		static readonly string errSimple2 = "Cannot serialize object of type '{0}'. " +
			"Consider changing type of XmlText member '{1}' from '{2}' to string or string array";

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
				this.initialDefaultNamespace = String.Empty;
			else
				this.initialDefaultNamespace = defaultNamespace;

			if (attributeOverrides == null)
				this.attributeOverrides = new XmlAttributeOverrides();
			else
				this.attributeOverrides = attributeOverrides;
		}

		void Reset ()
		{
			helper = new ReflectionHelper();
			arrayChoiceCount = 1;
		}
		
		internal bool AllowPrivateTypes
		{
			get { return allowPrivateTypes; }
			set { allowPrivateTypes = value; }
		}

		#endregion // Constructors

		#region Methods

		public XmlMembersMapping ImportMembersMapping (string elementName,
			string ns,
			XmlReflectionMember [] members,
			bool hasWrapperElement)
		{
//			Reset ();	Disabled. See ChangeLog

			XmlMemberMapping[] mapping = new XmlMemberMapping[members.Length];
			for (int n=0; n<members.Length; n++)
			{
				XmlTypeMapMember mapMem = CreateMapMember (members[n], ns);
				mapping[n] = new XmlMemberMapping (members[n].MemberName, ns, mapMem, false);
			}
			XmlMembersMapping mps = new XmlMembersMapping (elementName, ns, hasWrapperElement, false, mapping);
			mps.RelatedMaps = relatedMaps;
			mps.Format = SerializationFormat.Literal;
			Type[] extraTypes = includedTypes != null ? (Type[])includedTypes.ToArray(typeof(Type)) : null;
			mps.Source = new MembersSerializationSource (elementName, hasWrapperElement, members, false, true, ns, extraTypes);
			if (allowPrivateTypes) mps.Source.CanBeGenerated = false;
			return mps;
		}

#if NET_2_0
		[MonoTODO]
		public XmlMembersMapping ImportMembersMapping (string elementName, 
			string ns, 
			XmlReflectionMember[] members, 
			bool hasWrapperElement, 
			bool rpc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlMembersMapping ImportMembersMapping (string elementName, 
			string ns, 
			XmlReflectionMember[] members, 
			bool hasWrapperElement, 
			bool rpc, 
			bool openModel)
		{
			throw new NotImplementedException ();
		}
#endif

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

		public XmlTypeMapping ImportTypeMapping (Type type, XmlRootAttribute root, string defaultNamespace)
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
				case SchemaTypes.Class: map = ImportClassMapping (type, root, defaultNamespace); break;
				case SchemaTypes.Array: map = ImportListMapping (type, root, defaultNamespace, null, 0); break;
				case SchemaTypes.XmlNode: map = ImportXmlNodeMapping (type, root, defaultNamespace); break;
				case SchemaTypes.Primitive: map = ImportPrimitiveMapping (type, root, defaultNamespace); break;
				case SchemaTypes.Enum: map = ImportEnumMapping (type, root, defaultNamespace); break;
				case SchemaTypes.XmlSerializable: map = ImportXmlSerializableMapping (type, root, defaultNamespace); break;
				default: throw new NotSupportedException ("Type " + type.FullName + " not supported for XML stialization");
			}

			map.RelatedMaps = relatedMaps;
			map.Format = SerializationFormat.Literal;
			Type[] extraTypes = includedTypes != null ? (Type[])includedTypes.ToArray(typeof(Type)) : null;
			map.Source = new XmlTypeSerializationSource (type, root, attributeOverrides, defaultNamespace, extraTypes);
			if (allowPrivateTypes) map.Source.CanBeGenerated = false;
			return map;
		}

		XmlTypeMapping CreateTypeMapping (TypeData typeData, XmlRootAttribute root, string defaultXmlType, string defaultNamespace)
		{
			string rootNamespace = defaultNamespace;
			string typeNamespace = null;
			string elementName;
			bool includeInSchema = true;
			XmlAttributes atts = null;
			bool nullable = true;

			if (defaultXmlType == null) defaultXmlType = typeData.XmlType;

			if (!typeData.IsListType)
			{
				if (attributeOverrides != null) 
					atts = attributeOverrides[typeData.Type];

				if (atts != null && typeData.SchemaType == SchemaTypes.Primitive)
					throw new InvalidOperationException ("XmlRoot and XmlType attributes may not be specified for the type " + typeData.FullTypeName);
			}

			if (atts == null) 
				atts = new XmlAttributes (typeData.Type);

			if (atts.XmlRoot != null && root == null)
				root = atts.XmlRoot;

			if (atts.XmlType != null)
			{
				if (atts.XmlType.Namespace != null && atts.XmlType.Namespace != string.Empty && typeData.SchemaType != SchemaTypes.Enum)
					typeNamespace = atts.XmlType.Namespace;

				if (atts.XmlType.TypeName != null && atts.XmlType.TypeName != string.Empty)
					defaultXmlType = atts.XmlType.TypeName;
					
				includeInSchema = atts.XmlType.IncludeInSchema;
			}

			elementName = defaultXmlType;

			if (root != null)
			{
				if (root.ElementName != null && root.ElementName != String.Empty)
					elementName = root.ElementName;
				if (root.Namespace != null && root.Namespace != String.Empty)
					rootNamespace = root.Namespace;
				nullable = root.IsNullable;
			}

			if (rootNamespace == null) rootNamespace = "";
			if (typeNamespace == null || typeNamespace.Length == 0) typeNamespace = rootNamespace;
			
			XmlTypeMapping map = new XmlTypeMapping (elementName, rootNamespace, typeData, defaultXmlType, typeNamespace);
			map.IncludeInSchema = includeInSchema;
			map.IsNullable = nullable;
			relatedMaps.Add (map);
			
			return map;
		}

		XmlTypeMapping ImportClassMapping (Type type, XmlRootAttribute root, string defaultNamespace)
		{
			TypeData typeData = TypeTranslator.GetTypeData (type);
			XmlTypeMapping map = helper.GetRegisteredClrType (type, GetTypeNamespace (typeData, root, defaultNamespace));
			if (map != null) return map;

			if (!allowPrivateTypes)
				ReflectionHelper.CheckSerializableType (type, false);
			
			map = CreateTypeMapping (typeData, root, null, defaultNamespace);
			helper.RegisterClrType (map, type, map.XmlTypeNamespace);
			helper.RegisterSchemaType (map, map.XmlType, map.XmlTypeNamespace);

			// Import members

			ClassMap classMap = new ClassMap ();
			map.ObjectMap = classMap;

//			try
//			{
				ICollection members = GetReflectionMembers (type);
				foreach (XmlReflectionMember rmember in members)
				{
					if (rmember.XmlAttributes.XmlIgnore) continue;
					XmlTypeMapMember mem = CreateMapMember (rmember, map.XmlTypeNamespace);
					mem.CheckOptionalValueType (type);
					classMap.AddMember (mem);
				}
//			}
//			catch (Exception ex) {
//				throw helper.CreateError (map, ex.Message);
//			}

			// Import extra classes

			if (type == typeof (object) && includedTypes != null)
			{
				foreach (Type intype in includedTypes)
					map.DerivedTypes.Add (ImportTypeMapping (intype, defaultNamespace));
			}

			// Register inheritance relations

			if (type.BaseType != null)
			{
				XmlTypeMapping bmap = ImportClassMapping (type.BaseType, root, defaultNamespace);
				ClassMap cbmap = bmap.ObjectMap as ClassMap;
				
				if (type.BaseType != typeof (object)) {
					map.BaseMap = bmap;
					if (!cbmap.HasSimpleContent)
						classMap.SetCanBeSimpleType (false);
				}
				
				// At this point, derived classes of this map must be already registered
				
				RegisterDerivedMap (bmap, map);
				
				if (cbmap.HasSimpleContent && classMap.ElementMembers != null && classMap.ElementMembers.Count != 1)
					throw new InvalidOperationException (String.Format (errSimple, map.TypeData.TypeName, map.BaseMap.TypeData.TypeName));
			}
			
			ImportIncludedTypes (type, defaultNamespace);
			
			if (classMap.XmlTextCollector != null && !classMap.HasSimpleContent)
			{
				XmlTypeMapMember mem = classMap.XmlTextCollector;
				if (mem.TypeData.Type != typeof(string) && 
				   mem.TypeData.Type != typeof(string[]) && 
				   mem.TypeData.Type != typeof(object[]) && 
				   mem.TypeData.Type != typeof(XmlNode[]))
				   
					throw new InvalidOperationException (String.Format (errSimple2, map.TypeData.TypeName, mem.Name, mem.TypeData.TypeName));
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

		string GetTypeNamespace (TypeData typeData, XmlRootAttribute root, string defaultNamespace)
		{
			string typeNamespace = null;
			
			XmlAttributes atts = null;
			if (!typeData.IsListType)
			{
				if (attributeOverrides != null)
					atts = attributeOverrides[typeData.Type];
			}

			if (atts == null)
				atts = new XmlAttributes (typeData.Type);

   			if (atts.XmlType != null)
   			{
   				if (atts.XmlType.Namespace != null && atts.XmlType.Namespace.Length != 0 && typeData.SchemaType != SchemaTypes.Enum)
   					typeNamespace = atts.XmlType.Namespace;
			}

			if (typeNamespace != null && typeNamespace.Length != 0) return typeNamespace;
			
   			if (atts.XmlRoot != null && root == null)
   				root = atts.XmlRoot;

			if (root != null)
			{
				if (root.Namespace != null && root.Namespace.Length != 0)
					return root.Namespace;
			}

			if (defaultNamespace == null) return "";
			else return defaultNamespace;
		}

		XmlTypeMapping ImportListMapping (Type type, XmlRootAttribute root, string defaultNamespace, XmlAttributes atts, int nestingLevel)
		{
			TypeData typeData = TypeTranslator.GetTypeData (type);
			ListMap obmap = new ListMap ();

			if (!allowPrivateTypes)
				ReflectionHelper.CheckSerializableType (type, true);
			
			if (atts == null) atts = new XmlAttributes();
			Type itemType = typeData.ListItemType;

			// warning: byte[][] should not be considered multiarray
			bool isMultiArray = (type.IsArray && (TypeTranslator.GetTypeData(itemType).SchemaType == SchemaTypes.Array) && itemType.IsArray);

			XmlTypeMapElementInfoList list = new XmlTypeMapElementInfoList();

			foreach (XmlArrayItemAttribute att in atts.XmlArrayItems)
			{
				if (att.NestingLevel != nestingLevel) continue;
				Type elemType = (att.Type != null) ? att.Type : itemType;
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (null, TypeTranslator.GetTypeData(elemType, att.DataType));
				elem.Namespace = att.Namespace != null ? att.Namespace : defaultNamespace;
				if (elem.Namespace == null) elem.Namespace = "";
				elem.Form = att.Form;
				elem.IsNullable = att.IsNullable && CanBeNull (elem.TypeData);
				elem.NestingLevel = att.NestingLevel;

				if (isMultiArray)
					elem.MappedType = ImportListMapping (elemType, null, elem.Namespace, atts, nestingLevel + 1);
				else if (elem.TypeData.IsComplexType)
					elem.MappedType = ImportTypeMapping (elemType, null, elem.Namespace);

				if (att.ElementName != null) elem.ElementName = att.ElementName;
				else if (elem.MappedType != null) elem.ElementName = elem.MappedType.ElementName;
				else elem.ElementName = TypeTranslator.GetTypeData(elemType).XmlType;

				list.Add (elem);
			}

			if (list.Count == 0)
			{
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (null, TypeTranslator.GetTypeData (itemType));
				if (isMultiArray)
					elem.MappedType = ImportListMapping (itemType, null, defaultNamespace, atts, nestingLevel + 1);
				else if (elem.TypeData.IsComplexType)
					elem.MappedType = ImportTypeMapping (itemType, null, defaultNamespace);

				if (elem.MappedType != null) elem.ElementName = elem.MappedType.XmlType;
				else elem.ElementName = TypeTranslator.GetTypeData(itemType).XmlType ;

				elem.Namespace = (defaultNamespace != null) ? defaultNamespace : "";
				elem.IsNullable = CanBeNull (elem.TypeData);
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
				if (elem.MappedType != null) baseName = TypeTranslator.GetArrayName (elem.MappedType.XmlType);
				else baseName = TypeTranslator.GetArrayName (elem.ElementName);
			}

			// Avoid name colisions

			int nameCount = 1;
			string name = baseName;

			do {
				XmlTypeMapping foundMap = helper.GetRegisteredSchemaType (name, defaultNamespace);
				if (foundMap == null) nameCount = -1;
				else if (obmap.Equals (foundMap.ObjectMap) && typeData.Type == foundMap.TypeData.Type) return foundMap;
				else name = baseName + (nameCount++);
			}
			while (nameCount != -1);

			XmlTypeMapping map = CreateTypeMapping (typeData, root, name, defaultNamespace);
			map.ObjectMap = obmap;
			
			// Register any of the including types as a derived class of object
			XmlIncludeAttribute[] includes = (XmlIncludeAttribute[])type.GetCustomAttributes (typeof (XmlIncludeAttribute), false);
			
			XmlTypeMapping objectMapping = ImportTypeMapping (typeof(object));
			for (int i = 0; i < includes.Length; i++)
			{
				Type includedType = includes[i].Type;
				objectMapping.DerivedTypes.Add(ImportTypeMapping (includedType, null, defaultNamespace));
			}
			
			// Register this map as a derived class of object

			helper.RegisterSchemaType (map, name, defaultNamespace);
			ImportTypeMapping (typeof(object)).DerivedTypes.Add (map);

			return map;
		}

		XmlTypeMapping ImportXmlNodeMapping (Type type, XmlRootAttribute root, string defaultNamespace)
		{
			XmlTypeMapping map = helper.GetRegisteredClrType (type, GetTypeNamespace (TypeTranslator.GetTypeData (type), root, defaultNamespace));
			if (map != null) return map;

			// Registers the maps for XmlNode and XmlElement

			XmlTypeMapping nodeMap = CreateTypeMapping (TypeTranslator.GetTypeData (typeof(XmlNode)), root, null, defaultNamespace);
			helper.RegisterClrType (nodeMap, typeof(XmlNode), nodeMap.XmlTypeNamespace);

			XmlTypeMapping elemMap = CreateTypeMapping (TypeTranslator.GetTypeData (typeof(XmlElement)), root, null, defaultNamespace);
			helper.RegisterClrType (elemMap, typeof(XmlElement), elemMap.XmlTypeNamespace);

			XmlTypeMapping textMap = CreateTypeMapping (TypeTranslator.GetTypeData (typeof(XmlText)), root, null, defaultNamespace);
			helper.RegisterClrType (textMap, typeof(XmlText), textMap.XmlTypeNamespace);

			XmlTypeMapping docMap = CreateTypeMapping (TypeTranslator.GetTypeData (typeof(XmlDocument)), root, null, defaultNamespace);
			helper.RegisterClrType (docMap, typeof(XmlDocument), textMap.XmlTypeNamespace);

			XmlTypeMapping obmap = ImportTypeMapping (typeof(object));
			obmap.DerivedTypes.Add (nodeMap);
			obmap.DerivedTypes.Add (elemMap);
			obmap.DerivedTypes.Add (textMap);
			obmap.DerivedTypes.Add (docMap);
			nodeMap.DerivedTypes.Add (elemMap);
			nodeMap.DerivedTypes.Add (textMap);
			nodeMap.DerivedTypes.Add (docMap);

			map = helper.GetRegisteredClrType (type, GetTypeNamespace (TypeTranslator.GetTypeData (type), root, defaultNamespace));
			if (map == null) throw new InvalidOperationException ("Objects of type '" + type + "' can't be serialized");
			return map;
		}

		XmlTypeMapping ImportPrimitiveMapping (Type type, XmlRootAttribute root, string defaultNamespace)
		{
			TypeData typeData = TypeTranslator.GetTypeData (type);
			XmlTypeMapping map = helper.GetRegisteredClrType (type, GetTypeNamespace (typeData, root, defaultNamespace));
			if (map != null) return map;
			map = CreateTypeMapping (typeData, root, null, defaultNamespace);
			helper.RegisterClrType (map, type, map.XmlTypeNamespace);
			return map;
		}

		XmlTypeMapping ImportEnumMapping (Type type, XmlRootAttribute root, string defaultNamespace)
		{
			TypeData typeData = TypeTranslator.GetTypeData (type);
			XmlTypeMapping map = helper.GetRegisteredClrType (type, GetTypeNamespace (typeData, root, defaultNamespace));
			if (map != null) return map;
			
			if (!allowPrivateTypes)
				ReflectionHelper.CheckSerializableType (type, false);
				
			map = CreateTypeMapping (typeData, root, null, defaultNamespace);
			helper.RegisterClrType (map, type, map.XmlTypeNamespace);

			string [] names = Enum.GetNames (type);
			ArrayList members = new ArrayList();
			foreach (string name in names)
			{
				MemberInfo[] mem = type.GetMember (name);
				string xmlName = null;
				object[] atts = mem[0].GetCustomAttributes (typeof(XmlIgnoreAttribute), false);
				if (atts.Length > 0) continue;
				atts = mem[0].GetCustomAttributes (typeof(XmlEnumAttribute), false);
				if (atts.Length > 0) xmlName = ((XmlEnumAttribute)atts[0]).Name;
				if (xmlName == null) xmlName = name;
				members.Add (new EnumMap.EnumMapMember (xmlName, name));
			}

			bool isFlags = type.GetCustomAttributes (typeof(FlagsAttribute),false).Length > 0;
			map.ObjectMap = new EnumMap ((EnumMap.EnumMapMember[])members.ToArray (typeof(EnumMap.EnumMapMember)), isFlags);
			ImportTypeMapping (typeof(object)).DerivedTypes.Add (map);
			return map;
		}

		XmlTypeMapping ImportXmlSerializableMapping (Type type, XmlRootAttribute root, string defaultNamespace)
		{
			TypeData typeData = TypeTranslator.GetTypeData (type);
			XmlTypeMapping map = helper.GetRegisteredClrType (type, GetTypeNamespace (typeData, root, defaultNamespace));
			if (map != null) return map;
			
			if (!allowPrivateTypes)
				ReflectionHelper.CheckSerializableType (type, false);
				
			map = CreateTypeMapping (typeData, root, null, defaultNamespace);
			helper.RegisterClrType (map, type, map.XmlTypeNamespace);
			return map;
		}

		void ImportIncludedTypes (Type type, string defaultNamespace)
		{
			XmlIncludeAttribute[] includes = (XmlIncludeAttribute[])type.GetCustomAttributes (typeof (XmlIncludeAttribute), false);
			for (int n=0; n<includes.Length; n++)
			{
				Type includedType = includes[n].Type;
				ImportTypeMapping (includedType, null, defaultNamespace);
			}
		}

		ICollection GetReflectionMembers (Type type)
		{
			// First we want to find the inheritance hierarchy in reverse order.
			Type currentType = type;
			ArrayList typeList = new ArrayList();
			typeList.Add(currentType);
			while (currentType != typeof(object))
			{
				currentType = currentType.BaseType; // Read the base type.
				typeList.Insert(0, currentType); // Insert at 0 to reverse the order.
			}

			// Read all Fields via reflection.
			ArrayList fieldList = new ArrayList();
			FieldInfo[] tfields = type.GetFields (BindingFlags.Instance | BindingFlags.Public);
			currentType = null;
			int currentIndex = 0;
			foreach (FieldInfo field in tfields)
			{
				// This statement ensures fields are ordered starting from the base type.
				if (currentType != field.DeclaringType)
				{
					currentType = field.DeclaringType;
					currentIndex=0;
				}
				fieldList.Insert(currentIndex++, field);
			}

			// Read all Properties via reflection.
			ArrayList propList = new ArrayList();
			PropertyInfo[] tprops = type.GetProperties (BindingFlags.Instance | BindingFlags.Public);
			currentType = null;
			currentIndex = 0;
			foreach (PropertyInfo prop in tprops)
			{
				// This statement ensures properties are ordered starting from the base type.
				if (currentType != prop.DeclaringType)
				{
					currentType = prop.DeclaringType;
					currentIndex = 0;
				}
				if (!prop.CanRead) continue;
				if (!prop.CanWrite && TypeTranslator.GetTypeData (prop.PropertyType).SchemaType != SchemaTypes.Array) continue;
				if (prop.GetIndexParameters().Length > 0) continue;
				propList.Insert(currentIndex++, prop);
			}

			ArrayList members = new ArrayList();
			int fieldIndex=0;
			int propIndex=0;
			// We now step through the type hierarchy from the base (object) through
			// to the supplied class, as each step outputting all Fields, and then
			// all Properties.  This is the exact same ordering as .NET 1.0/1.1.
			foreach (Type t in typeList)
			{
				// Add any fields matching the current DeclaringType.
				while (fieldIndex < fieldList.Count)
				{
					FieldInfo field = (FieldInfo)fieldList[fieldIndex];
					if (field.DeclaringType==t)
					{
						fieldIndex++;
						XmlAttributes atts = attributeOverrides[type, field.Name];
						if (atts == null) atts = new XmlAttributes (field);
						if (atts.XmlIgnore) continue;
						XmlReflectionMember member = new XmlReflectionMember(field.Name, field.FieldType, atts);
						members.Add(member);
					}
					else break;
				}

				// Add any properties matching the current DeclaringType.
				while (propIndex < propList.Count)
				{
					PropertyInfo prop = (PropertyInfo)propList[propIndex];
					if (prop.DeclaringType==t)
					{
						propIndex++;
						XmlAttributes atts = attributeOverrides[type, prop.Name];
						if (atts == null) atts = new XmlAttributes (prop);
						if (atts.XmlIgnore) continue;
						XmlReflectionMember member = new XmlReflectionMember(prop.Name, prop.PropertyType, atts);
						members.Add(member);
					}
					else break;
				}
			}
			return members;		
		}
		
		private XmlTypeMapMember CreateMapMember (XmlReflectionMember rmember, string defaultNamespace)
		{
			XmlTypeMapMember mapMember;
			XmlAttributes atts = rmember.XmlAttributes;
			TypeData typeData = TypeTranslator.GetTypeData (rmember.MemberType);

			if (atts.XmlAnyAttribute != null)
			{
				if ( (rmember.MemberType.FullName == "System.Xml.XmlAttribute[]") ||
					 (rmember.MemberType.FullName == "System.Xml.XmlNode[]") )
				{
					mapMember = new XmlTypeMapMemberAnyAttribute();
				}
				else
					throw new InvalidOperationException ("XmlAnyAttributeAttribute can only be applied to members of type XmlAttribute[] or XmlNode[]");
			}
			else if (atts.XmlAnyElements != null && atts.XmlAnyElements.Count > 0)
			{
				if ( (rmember.MemberType.FullName == "System.Xml.XmlElement[]") ||
					 (rmember.MemberType.FullName == "System.Xml.XmlNode[]") ||
					 (rmember.MemberType.FullName == "System.Xml.XmlElement"))
				{
					XmlTypeMapMemberAnyElement member = new XmlTypeMapMemberAnyElement();
					member.ElementInfo = ImportAnyElementInfo (defaultNamespace, rmember, member, atts);
					mapMember = member;
				}
				else
					throw new InvalidOperationException ("XmlAnyElementAttribute can only be applied to members of type XmlElement, XmlElement[] or XmlNode[]");
			}
			else if (atts.Xmlns)
			{
				XmlTypeMapMemberNamespaces mapNamespaces = new XmlTypeMapMemberNamespaces ();
				mapMember = mapNamespaces;
			}
			else if (atts.XmlAttribute != null)
			{
				// An attribute

				if (atts.XmlElements != null && atts.XmlElements.Count > 0)
					throw new Exception ("XmlAttributeAttribute and XmlElementAttribute cannot be applied to the same member");

				XmlTypeMapMemberAttribute mapAttribute = new XmlTypeMapMemberAttribute ();
				if (atts.XmlAttribute.AttributeName == null) 
					mapAttribute.AttributeName = rmember.MemberName;
				else 
					mapAttribute.AttributeName = atts.XmlAttribute.AttributeName;

				if (typeData.IsComplexType)
					mapAttribute.MappedType = ImportTypeMapping (typeData.Type, null, mapAttribute.Namespace);
				
				if (atts.XmlAttribute.Namespace != null && atts.XmlAttribute.Namespace != defaultNamespace)
				{
					if (atts.XmlAttribute.Form == XmlSchemaForm.Unqualified)
						throw new InvalidOperationException ("The Form property may not be 'Unqualified' when an explicit Namespace property is present");
					mapAttribute.Form = XmlSchemaForm.Qualified;
					mapAttribute.Namespace = atts.XmlAttribute.Namespace;
				}
				else
				{
					mapAttribute.Form = atts.XmlAttribute.Form;
					if (atts.XmlAttribute.Form == XmlSchemaForm.Qualified)
						mapAttribute.Namespace = defaultNamespace;
					else
						mapAttribute.Namespace = "";
				}
				
				typeData = TypeTranslator.GetTypeData(rmember.MemberType, atts.XmlAttribute.DataType);
				mapMember = mapAttribute;
			}
			else if (typeData.SchemaType == SchemaTypes.Array)
			{
				// If the member has a single XmlElementAttribute and the type is the type of the member,
				// then it is not a flat list
				
				if (atts.XmlElements.Count > 1 ||
				   (atts.XmlElements.Count == 1 && atts.XmlElements[0].Type != typeData.Type) ||
				   (atts.XmlText != null))
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

					// Creates an ElementInfo that identifies the array instance. 
					member.ElementInfo = new XmlTypeMapElementInfoList();
					XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (member, typeData);
					elem.ElementName = (atts.XmlArray != null && atts.XmlArray.ElementName != null) ? atts.XmlArray.ElementName : rmember.MemberName;
					elem.Namespace = (atts.XmlArray != null && atts.XmlArray.Namespace != null) ? atts.XmlArray.Namespace : defaultNamespace;
					elem.MappedType = ImportListMapping (rmember.MemberType, null, elem.Namespace, atts, 0);
					elem.IsNullable = (atts.XmlArray != null) ? atts.XmlArray.IsNullable : false;
					elem.Form = (atts.XmlArray != null) ? atts.XmlArray.Form : XmlSchemaForm.Qualified;

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

			mapMember.DefaultValue = atts.XmlDefaultValue;
			mapMember.TypeData = typeData;
			mapMember.Name = rmember.MemberName;
			mapMember.IsReturnValue = rmember.IsReturnValue;
			return mapMember;
		}

		XmlTypeMapElementInfoList ImportElementInfo (string defaultName, string defaultNamespace, Type defaultType, XmlTypeMapMemberElement member, XmlAttributes atts)
		{
			XmlTypeMapElementInfoList list = new XmlTypeMapElementInfoList();

			ImportTextElementInfo (list, defaultType, member, atts);
			
			if (atts.XmlElements.Count == 0 && list.Count == 0)
			{
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (member, TypeTranslator.GetTypeData(defaultType));
				elem.ElementName = defaultName;
				elem.Namespace = defaultNamespace;
				if (elem.TypeData.IsComplexType)
					elem.MappedType = ImportTypeMapping (defaultType, null, defaultNamespace);
				list.Add (elem);
			}

			bool multiType = (atts.XmlElements.Count > 1);
			foreach (XmlElementAttribute att in atts.XmlElements)
			{
				Type elemType = (att.Type != null) ? att.Type : defaultType;
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (member, TypeTranslator.GetTypeData(elemType, att.DataType));
				elem.ElementName = (att.ElementName != null) ? att.ElementName : defaultName;
				elem.Namespace = (att.Namespace != null) ? att.Namespace : defaultNamespace;
				elem.Form = att.Form;
				elem.IsNullable = att.IsNullable;
				
				if (elem.IsNullable && elem.TypeData.IsValueType)
					throw new InvalidOperationException ("IsNullable may not be 'true' for value type " + elem.TypeData.FullTypeName + " in member '" + defaultName + "'");
					
				if (elem.TypeData.IsComplexType)
				{
					if (att.DataType != null) throw new InvalidOperationException ("'" + att.DataType + "' is an invalid value for the XmlElementAttribute.DateTime property. The property may only be specified for primitive types.");
					elem.MappedType = ImportTypeMapping (elemType, null, elem.Namespace);
				}

				if (att.ElementName != null) 
					elem.ElementName = att.ElementName;
				else if (multiType) {
					if (elem.MappedType != null) elem.ElementName = elem.MappedType.ElementName;
					else elem.ElementName = TypeTranslator.GetTypeData(elemType).XmlType;
				}
				else
					elem.ElementName = defaultName;

				list.Add (elem);
			}
			return list;
		}

		XmlTypeMapElementInfoList ImportAnyElementInfo (string defaultNamespace, XmlReflectionMember rmember, XmlTypeMapMemberElement member, XmlAttributes atts)
		{
			XmlTypeMapElementInfoList list = new XmlTypeMapElementInfoList();

			ImportTextElementInfo (list, rmember.MemberType, member, atts);

			foreach (XmlAnyElementAttribute att in atts.XmlAnyElements)
			{
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (member, TypeTranslator.GetTypeData(typeof(XmlElement)));
				if (att.Name != null && att.Name != string.Empty) 
				{
					elem.ElementName = att.Name;
					elem.Namespace = (att.Namespace != null) ? att.Namespace : "";
				}
				else 
				{
					elem.IsUnnamedAnyElement = true;
					elem.Namespace = defaultNamespace;
					if (att.Namespace != null) 
						throw new InvalidOperationException ("The element " + rmember.MemberName + " has been attributed with an XmlAnyElementAttribute and a namespace '" + att.Namespace + "', but no name. When a namespace is supplied, a name is also required. Supply a name or remove the namespace.");
				}
				list.Add (elem);
			}
			return list;
		}

		void ImportTextElementInfo (XmlTypeMapElementInfoList list, Type defaultType, XmlTypeMapMemberElement member, XmlAttributes atts)
		{
			if (atts.XmlText != null)
			{
				member.IsXmlTextCollector = true;
				if (atts.XmlText.Type != null) defaultType = atts.XmlText.Type;
				if (defaultType == typeof(XmlNode)) defaultType = typeof(XmlText);	// Nodes must be text nodes

				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (member, TypeTranslator.GetTypeData(defaultType, atts.XmlText.DataType));

				if (elem.TypeData.SchemaType != SchemaTypes.Primitive &&
					elem.TypeData.SchemaType != SchemaTypes.Enum &&
				    elem.TypeData.SchemaType != SchemaTypes.XmlNode &&
				    !(elem.TypeData.SchemaType == SchemaTypes.Array && elem.TypeData.ListItemTypeData.SchemaType == SchemaTypes.XmlNode)
				 )
					throw new InvalidOperationException ("XmlText cannot be used to encode complex types");

				elem.IsTextElement = true;
				elem.WrappedElement = false;
				list.Add (elem);
			}
		}
		
		bool CanBeNull (TypeData type)
		{
			return (type.SchemaType != SchemaTypes.Primitive || type.Type == typeof (string));
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
			object[] ats = provider.GetCustomAttributes (typeof(XmlIncludeAttribute), true);
			foreach (XmlIncludeAttribute at in ats)
				IncludeType (at.Type);
		}

		#endregion // Methods
	}
}
