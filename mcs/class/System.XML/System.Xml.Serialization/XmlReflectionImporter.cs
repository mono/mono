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

using System.Collections;
using System.Globalization;
using System.Reflection;
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

/*		void Reset ()
		{
			helper = new ReflectionHelper();
			arrayChoiceCount = 1;
		}
*/
		
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
			return ImportMembersMapping (elementName, ns, members, hasWrapperElement, true);
		}

#if NET_2_0
		[MonoTODO]
		public
#endif
		XmlMembersMapping ImportMembersMapping (string elementName, 
			string ns, 
			XmlReflectionMember[] members, 
			bool hasWrapperElement, 
			bool writeAccessors)
		{
			return ImportMembersMapping (elementName, ns, members, hasWrapperElement, writeAccessors, true);
		}

#if NET_2_0
		[MonoTODO]
		public
#endif
		XmlMembersMapping ImportMembersMapping (string elementName, 
			string ns, 
			XmlReflectionMember[] members, 
			bool hasWrapperElement, 
			bool writeAccessors, 
			bool validate)
		{
			return ImportMembersMapping (elementName, ns, members, hasWrapperElement, writeAccessors, validate, XmlMappingAccess.Read | XmlMappingAccess.Write);
		}

#if NET_2_0
		[MonoTODO] // FIXME: handle writeAccessors, validate, and mapping access
		public
#endif
		XmlMembersMapping ImportMembersMapping (string elementName, 
			string ns, 
			XmlReflectionMember[] members, 
			bool hasWrapperElement, 
			bool writeAccessors, 
			bool validate,
			XmlMappingAccess access)
		{
//			Reset ();	Disabled. See ChangeLog

			XmlMemberMapping[] mapping = new XmlMemberMapping[members.Length];
			for (int n=0; n<members.Length; n++)
			{
				XmlTypeMapMember mapMem = CreateMapMember (null, members[n], ns);
				mapping[n] = new XmlMemberMapping (members[n].MemberName, ns, mapMem, false);
			}
			elementName = XmlConvert.EncodeLocalName (elementName);
			XmlMembersMapping mps = new XmlMembersMapping (elementName, ns, hasWrapperElement, false, mapping);
			mps.RelatedMaps = relatedMaps;
			mps.Format = SerializationFormat.Literal;
			Type[] extraTypes = includedTypes != null ? (Type[])includedTypes.ToArray(typeof(Type)) : null;
			mps.Source = new MembersSerializationSource (elementName, hasWrapperElement, members, false, true, ns, extraTypes);
			if (allowPrivateTypes) mps.Source.CanBeGenerated = false;
			return mps;
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

		public XmlTypeMapping ImportTypeMapping (Type type, XmlRootAttribute root, string defaultNamespace)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (type == typeof (void))
				throw new NotSupportedException ("The type " + type.FullName + " may not be serialized.");

			return ImportTypeMapping (TypeTranslator.GetTypeData (type), root, 
				defaultNamespace);
		}

		internal XmlTypeMapping ImportTypeMapping (TypeData typeData, string defaultNamespace)
		{
			return ImportTypeMapping (typeData, (XmlRootAttribute) null, 
				defaultNamespace);
		}

		private XmlTypeMapping ImportTypeMapping (TypeData typeData, XmlRootAttribute root, string defaultNamespace)
		{
			if (typeData == null)
				throw new ArgumentNullException ("typeData");

			if (typeData.Type == null)
				throw new ArgumentException ("Specified TypeData instance does not have Type set.");

			if (defaultNamespace == null) defaultNamespace = initialDefaultNamespace;
			if (defaultNamespace == null) defaultNamespace = string.Empty;

			try {
				XmlTypeMapping map;

				switch (typeData.SchemaType) {
					case SchemaTypes.Class: map = ImportClassMapping (typeData, root, defaultNamespace); break;
					case SchemaTypes.Array: map = ImportListMapping (typeData, root, defaultNamespace, null, 0); break;
					case SchemaTypes.XmlNode: map = ImportXmlNodeMapping (typeData, root, defaultNamespace); break;
					case SchemaTypes.Primitive: map = ImportPrimitiveMapping (typeData, root, defaultNamespace); break;
					case SchemaTypes.Enum: map = ImportEnumMapping (typeData, root, defaultNamespace); break;
					case SchemaTypes.XmlSerializable: map = ImportXmlSerializableMapping (typeData, root, defaultNamespace); break;
					default: throw new NotSupportedException ("Type " + typeData.Type.FullName + " not supported for XML stialization");
				}

#if NET_2_0
				// bug #372780
				map.SetKey (typeData.Type.ToString ());
#endif
				map.RelatedMaps = relatedMaps;
				map.Format = SerializationFormat.Literal;
				Type[] extraTypes = includedTypes != null ? (Type[]) includedTypes.ToArray (typeof (Type)) : null;
				map.Source = new XmlTypeSerializationSource (typeData.Type, root, attributeOverrides, defaultNamespace, extraTypes);
				if (allowPrivateTypes) map.Source.CanBeGenerated = false;
				return map;
			} catch (InvalidOperationException ex) {
				throw new InvalidOperationException (string.Format (CultureInfo.InvariantCulture,
					"There was an error reflecting type '{0}'.", typeData.Type.FullName), ex);
			}
		}

		XmlTypeMapping CreateTypeMapping (TypeData typeData, XmlRootAttribute root, string defaultXmlType, string defaultNamespace)
		{
			string rootNamespace = defaultNamespace;
			string typeNamespace = null;
			string elementName;
			bool includeInSchema = true;
			XmlAttributes atts = null;
			bool nullable = CanBeNull (typeData);

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
				if (atts.XmlType.Namespace != null)
					typeNamespace = atts.XmlType.Namespace;

				if (atts.XmlType.TypeName != null && atts.XmlType.TypeName != string.Empty)
					defaultXmlType = XmlConvert.EncodeLocalName (atts.XmlType.TypeName);
					
				includeInSchema = atts.XmlType.IncludeInSchema;
			}

			elementName = defaultXmlType;

			if (root != null)
			{
				if (root.ElementName.Length != 0)
					elementName = XmlConvert.EncodeLocalName(root.ElementName);
				if (root.Namespace != null)
					rootNamespace = root.Namespace;
				nullable = root.IsNullable;
			}

			if (rootNamespace == null) rootNamespace = "";
			if (typeNamespace == null) typeNamespace = rootNamespace;
			
			XmlTypeMapping map;
			switch (typeData.SchemaType) {
				case SchemaTypes.XmlSerializable:
					map = new XmlSerializableMapping (root, elementName, rootNamespace, typeData, defaultXmlType, typeNamespace);
					break;
				case SchemaTypes.Primitive:
					if (!typeData.IsXsdType)
						map = new XmlTypeMapping (elementName, rootNamespace, 
							typeData, defaultXmlType, XmlSerializer.WsdlTypesNamespace);
					else
						map = new XmlTypeMapping (elementName, rootNamespace, 
							typeData, defaultXmlType, typeNamespace);
					break;
				default:
					map = new XmlTypeMapping (elementName, rootNamespace, typeData, defaultXmlType, typeNamespace);
					break;
			}

			map.IncludeInSchema = includeInSchema;
			map.IsNullable = nullable;
			relatedMaps.Add (map);
			
			return map;
		}

		XmlTypeMapping ImportClassMapping (Type type, XmlRootAttribute root, string defaultNamespace)
		{
			TypeData typeData = TypeTranslator.GetTypeData (type);
			return ImportClassMapping (typeData, root, defaultNamespace);
		}

		XmlTypeMapping ImportClassMapping (TypeData typeData, XmlRootAttribute root, string defaultNamespace)
		{
			Type type = typeData.Type;

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

			ICollection members = GetReflectionMembers (type);
			foreach (XmlReflectionMember rmember in members)
			{
				string ns = map.XmlTypeNamespace;
				if (rmember.XmlAttributes.XmlIgnore) continue;
				if (rmember.DeclaringType != null && rmember.DeclaringType != type) {
					XmlTypeMapping bmap = ImportClassMapping (rmember.DeclaringType, root, defaultNamespace);
					ns = bmap.XmlTypeNamespace;
				}

				try {
					XmlTypeMapMember mem = CreateMapMember (type, rmember, ns);
					mem.CheckOptionalValueType (type);
					classMap.AddMember (mem);
				} catch (Exception ex) {
					throw new InvalidOperationException (string.Format (
						CultureInfo.InvariantCulture, "There was an error" +
						" reflecting field '{0}'.", rmember.MemberName), ex);
				}
			}

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
			return ImportListMapping (typeData, root, defaultNamespace, atts, nestingLevel);
		}

		XmlTypeMapping ImportListMapping (TypeData typeData, XmlRootAttribute root, string defaultNamespace, XmlAttributes atts, int nestingLevel)
		{
			Type type = typeData.Type;
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
				if (att.Namespace != null && att.Form == XmlSchemaForm.Unqualified)
					throw new InvalidOperationException ("XmlArrayItemAttribute.Form must not be Unqualified when it has an explicit Namespace value.");
				if (att.NestingLevel != nestingLevel) continue;
				Type elemType = (att.Type != null) ? att.Type : itemType;
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (null, TypeTranslator.GetTypeData(elemType, att.DataType));
				elem.Namespace = att.Namespace != null ? att.Namespace : defaultNamespace;
				if (elem.Namespace == null) elem.Namespace = "";
				elem.Form = att.Form;
				if (att.Form == XmlSchemaForm.Unqualified)
					elem.Namespace = string.Empty;
				elem.IsNullable = att.IsNullable && CanBeNull (elem.TypeData);
				elem.NestingLevel = att.NestingLevel;

				if (isMultiArray) {
					elem.MappedType = ImportListMapping (elemType, null, elem.Namespace, atts, nestingLevel + 1);
				} else if (elem.TypeData.IsComplexType) {
					elem.MappedType = ImportTypeMapping (elemType, null, elem.Namespace);
				}

				if (att.ElementName.Length != 0) {
					elem.ElementName = XmlConvert.EncodeLocalName (att.ElementName);
				} else if (elem.MappedType != null) {
					elem.ElementName = elem.MappedType.ElementName;
				} else {
					elem.ElementName = TypeTranslator.GetTypeData (elemType).XmlType;
				}

				list.Add (elem);
			}

			if (list.Count == 0)
			{
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (null, TypeTranslator.GetTypeData (itemType));
				if (isMultiArray)
					elem.MappedType = ImportListMapping (itemType, null, defaultNamespace, atts, nestingLevel + 1);
				else if (elem.TypeData.IsComplexType)
					elem.MappedType = ImportTypeMapping (itemType, null, defaultNamespace);

				if (elem.MappedType != null) {
					elem.ElementName = elem.MappedType.XmlType;
				} else {
					elem.ElementName = TypeTranslator.GetTypeData (itemType).XmlType;
				}

				elem.Namespace = (defaultNamespace != null) ? defaultNamespace : "";
				elem.IsNullable = CanBeNull (elem.TypeData);
				list.Add (elem);
			}

			obmap.ItemInfo = list;

			// If there can be different element names (types) in the array, then its name cannot
			// be "ArrayOfXXX" it must be something like ArrayOfChoiceNNN

			string baseName;
			if (list.Count > 1) {
				baseName = "ArrayOfChoice" + (arrayChoiceCount++);
			} else {
				XmlTypeMapElementInfo elem = ((XmlTypeMapElementInfo) list[0]);
				if (elem.MappedType != null) {
					baseName = TypeTranslator.GetArrayName (elem.MappedType.XmlType);
				} else {
					baseName = TypeTranslator.GetArrayName (elem.ElementName);
				}
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

		XmlTypeMapping ImportXmlNodeMapping (TypeData typeData, XmlRootAttribute root, string defaultNamespace)
		{
			Type type = typeData.Type;
			XmlTypeMapping map = helper.GetRegisteredClrType (type, GetTypeNamespace (typeData, root, defaultNamespace));
			if (map != null) return map;

			map = CreateTypeMapping (typeData, root, null, defaultNamespace);
			helper.RegisterClrType (map, type, map.XmlTypeNamespace);
			
			if (type.BaseType != null)
			{
				XmlTypeMapping bmap = ImportTypeMapping (type.BaseType, root, defaultNamespace);
				if (type.BaseType != typeof (object))
					map.BaseMap = bmap;
				
				RegisterDerivedMap (bmap, map);
			}

			return map;
		}

		XmlTypeMapping ImportPrimitiveMapping (TypeData typeData, XmlRootAttribute root, string defaultNamespace)
		{
			Type type = typeData.Type;
			XmlTypeMapping map = helper.GetRegisteredClrType (type, GetTypeNamespace (typeData, root, defaultNamespace));
			if (map != null) return map;
			map = CreateTypeMapping (typeData, root, null, defaultNamespace);
			helper.RegisterClrType (map, type, map.XmlTypeNamespace);
			return map;
		}

		XmlTypeMapping ImportEnumMapping (TypeData typeData, XmlRootAttribute root, string defaultNamespace)
		{
			Type type = typeData.Type;
			XmlTypeMapping map = helper.GetRegisteredClrType (type, GetTypeNamespace (typeData, root, defaultNamespace));
			if (map != null) return map;
			
			if (!allowPrivateTypes)
				ReflectionHelper.CheckSerializableType (type, false);
				
			map = CreateTypeMapping (typeData, root, null, defaultNamespace);
			map.IsNullable = false;
			helper.RegisterClrType (map, type, map.XmlTypeNamespace);

			string [] names = Enum.GetNames (type);
			ArrayList members = new ArrayList();
			foreach (string name in names)
			{
				FieldInfo field = type.GetField (name);
				string xmlName = null;
				if (field.IsDefined(typeof(XmlIgnoreAttribute), false))
					continue;
				object[] atts = field.GetCustomAttributes (typeof(XmlEnumAttribute), false);
				if (atts.Length > 0) xmlName = ((XmlEnumAttribute)atts[0]).Name;
				if (xmlName == null) xmlName = name;
				long value = ((IConvertible) field.GetValue (null)).ToInt64 (CultureInfo.InvariantCulture);
				members.Add (new EnumMap.EnumMapMember (xmlName, name, value));
			}

			bool isFlags = type.IsDefined (typeof (FlagsAttribute), false);
			map.ObjectMap = new EnumMap ((EnumMap.EnumMapMember[])members.ToArray (typeof(EnumMap.EnumMapMember)), isFlags);
			ImportTypeMapping (typeof(object)).DerivedTypes.Add (map);
			return map;
		}

		XmlTypeMapping ImportXmlSerializableMapping (TypeData typeData, XmlRootAttribute root, string defaultNamespace)
		{
			Type type = typeData.Type;
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
#if TARGET_JVM
			// This statement ensures fields are ordered starting from the base type.
			for (int ti=0; ti<typeList.Count; ti++) {
				for (int i=0; i<tfields.Length; i++) {
					FieldInfo field = tfields[i];
					if (field.DeclaringType == typeList[ti])
						fieldList.Add (field);
				}
			}
#else
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
#endif
			// Read all Properties via reflection.
			ArrayList propList = new ArrayList();
			PropertyInfo[] tprops = type.GetProperties (BindingFlags.Instance | BindingFlags.Public);
#if TARGET_JVM
			// This statement ensures properties are ordered starting from the base type.
			for (int ti=0; ti<typeList.Count; ti++) {
				for (int i=0; i<tprops.Length; i++) {
					PropertyInfo prop = tprops[i];
					if (!prop.CanRead) continue;
					if (prop.GetIndexParameters().Length > 0) continue;
					if (prop.DeclaringType == typeList[ti])
						propList.Add (prop);
				}
			}
#else
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
				if (prop.GetIndexParameters().Length > 0) continue;
				propList.Insert(currentIndex++, prop);
			}
#endif
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
						member.DeclaringType = field.DeclaringType;
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
						if (!prop.CanWrite && (TypeTranslator.GetTypeData (prop.PropertyType).SchemaType != SchemaTypes.Array || prop.PropertyType.IsArray)) continue;
						XmlReflectionMember member = new XmlReflectionMember(prop.Name, prop.PropertyType, atts);
						member.DeclaringType = prop.DeclaringType;
						members.Add(member);
					}
					else break;
				}
			}
			return members;		
		}
		
		private XmlTypeMapMember CreateMapMember (Type declaringType, XmlReflectionMember rmember, string defaultNamespace)
		{
			XmlTypeMapMember mapMember;
			XmlAttributes atts = rmember.XmlAttributes;
			TypeData typeData = TypeTranslator.GetTypeData (rmember.MemberType);

			if (atts.XmlArray != null) {
				if (atts.XmlArray.Namespace != null && atts.XmlArray.Form == XmlSchemaForm.Unqualified)
					throw new InvalidOperationException ("XmlArrayAttribute.Form must not be Unqualified when it has an explicit Namespace value.");
				if (typeData.SchemaType != SchemaTypes.Array &&
				    !(typeData.SchemaType == SchemaTypes.Primitive && typeData.Type == typeof (byte [])))
					throw new InvalidOperationException ("XmlArrayAttribute can be applied to members of array or collection type.");
			}

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
				// no XmlNode type check is done here (seealso: bug #553032).
				XmlTypeMapMemberAnyElement member = new XmlTypeMapMemberAnyElement();
				member.ElementInfo = ImportAnyElementInfo (defaultNamespace, rmember, member, atts);
				mapMember = member;
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
				if (atts.XmlAttribute.AttributeName.Length == 0) 
					mapAttribute.AttributeName = rmember.MemberName;
				else 
					mapAttribute.AttributeName = atts.XmlAttribute.AttributeName;

				mapAttribute.AttributeName = XmlConvert.EncodeLocalName (mapAttribute.AttributeName);

				if (typeData.IsComplexType)
					mapAttribute.MappedType = ImportTypeMapping (typeData.Type, null, defaultNamespace);

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

					// check that it does not have XmlArrayAttribute
					if (atts.XmlArray != null)
						throw new InvalidOperationException ("XmlArrayAttribute cannot be used with members which also attributed with XmlElementAttribute or XmlTextAttribute.");

					XmlTypeMapMemberFlatList member = new XmlTypeMapMemberFlatList ();
					member.ListMap = new ListMap ();
					member.ListMap.ItemInfo = ImportElementInfo (declaringType, XmlConvert.EncodeLocalName (rmember.MemberName), defaultNamespace, typeData.ListItemType, member, atts);
					member.ElementInfo = member.ListMap.ItemInfo;
					member.ListMap.ChoiceMember = member.ChoiceMember;
					mapMember = member;
				}
				else
				{
					// A list

					XmlTypeMapMemberList member = new XmlTypeMapMemberList ();

					// Creates an ElementInfo that identifies the array instance. 
					member.ElementInfo = new XmlTypeMapElementInfoList();
					XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (member, typeData);
					elem.ElementName = XmlConvert.EncodeLocalName((atts.XmlArray != null && atts.XmlArray.ElementName.Length != 0) ? atts.XmlArray.ElementName : rmember.MemberName);
					// note that it could be changed below (when Form is Unqualified)
					elem.Namespace = (atts.XmlArray != null && atts.XmlArray.Namespace != null) ? atts.XmlArray.Namespace : defaultNamespace;
					elem.MappedType = ImportListMapping (rmember.MemberType, null, elem.Namespace, atts, 0);
					elem.IsNullable = (atts.XmlArray != null) ? atts.XmlArray.IsNullable : false;
					elem.Form = (atts.XmlArray != null) ? atts.XmlArray.Form : XmlSchemaForm.Qualified;
					// This is a bit tricky, but is done
					// after filling descendant members, so
					// that array items could be serialized
					// with proper namespace.
					if (atts.XmlArray != null && atts.XmlArray.Form == XmlSchemaForm.Unqualified)
						elem.Namespace = String.Empty;

					member.ElementInfo.Add (elem);
					mapMember = member;
				}
			}
			else
			{
				// An element

				XmlTypeMapMemberElement member = new XmlTypeMapMemberElement ();
 				member.ElementInfo = ImportElementInfo (declaringType, XmlConvert.EncodeLocalName(rmember.MemberName), defaultNamespace, rmember.MemberType, member, atts);
				mapMember = member;
			}

			mapMember.DefaultValue = GetDefaultValue (typeData, atts.XmlDefaultValue);
			mapMember.TypeData = typeData;
			mapMember.Name = rmember.MemberName;
			mapMember.IsReturnValue = rmember.IsReturnValue;
			return mapMember;
		}

		XmlTypeMapElementInfoList ImportElementInfo (Type cls, string defaultName, string defaultNamespace, Type defaultType, XmlTypeMapMemberElement member, XmlAttributes atts)
		{
			EnumMap choiceEnumMap = null;
			Type choiceEnumType = null;
			
			XmlTypeMapElementInfoList list = new XmlTypeMapElementInfoList();
			ImportTextElementInfo (list, defaultType, member, atts, defaultNamespace);
			
			if (atts.XmlChoiceIdentifier != null) {
				if (cls == null)
					throw new InvalidOperationException ("XmlChoiceIdentifierAttribute not supported in this context.");
					
				member.ChoiceMember = atts.XmlChoiceIdentifier.MemberName;
				MemberInfo[] mems = cls.GetMember (member.ChoiceMember, BindingFlags.Instance|BindingFlags.Public);
				
				if (mems.Length == 0)
					throw new InvalidOperationException ("Choice member '" + member.ChoiceMember + "' not found in class '" + cls);
					
				if (mems[0] is PropertyInfo) {
					PropertyInfo pi = (PropertyInfo)mems[0];
					if (!pi.CanWrite || !pi.CanRead)
						throw new InvalidOperationException ("Choice property '" + member.ChoiceMember + "' must be read/write.");
					choiceEnumType = pi.PropertyType;
				}
				else choiceEnumType = ((FieldInfo)mems[0]).FieldType;
				
				member.ChoiceTypeData = TypeTranslator.GetTypeData (choiceEnumType);
				
				if (choiceEnumType.IsArray)
					choiceEnumType = choiceEnumType.GetElementType ();
				
				choiceEnumMap = ImportTypeMapping (choiceEnumType).ObjectMap as EnumMap;
				if (choiceEnumMap == null)
					throw new InvalidOperationException ("The member '" + mems[0].Name + "' is not a valid target for XmlChoiceIdentifierAttribute.");
			}
			
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
				elem.Form = att.Form;
				if (elem.Form != XmlSchemaForm.Unqualified)
					elem.Namespace = (att.Namespace != null) ? att.Namespace : defaultNamespace;
				elem.IsNullable = att.IsNullable;

				if (elem.IsNullable && !elem.TypeData.IsNullable)
					throw new InvalidOperationException ("IsNullable may not be 'true' for value type " + elem.TypeData.FullTypeName + " in member '" + defaultName + "'");
					
				if (elem.TypeData.IsComplexType)
				{
					if (att.DataType.Length != 0) throw new InvalidOperationException (
						string.Format(CultureInfo.InvariantCulture, "'{0}' is "
							+ "an invalid value for '{1}.{2}' of type '{3}'. "
							+ "The property may only be specified for primitive types.",
							att.DataType, cls.FullName, defaultName, 
							elem.TypeData.FullTypeName));
					elem.MappedType = ImportTypeMapping (elemType, null, elem.Namespace);
				}

				if (att.ElementName.Length != 0)  {
					elem.ElementName = XmlConvert.EncodeLocalName(att.ElementName);
				} else if (multiType) {
					if (elem.MappedType != null) {
						elem.ElementName = elem.MappedType.ElementName;
					} else {
						elem.ElementName = TypeTranslator.GetTypeData (elemType).XmlType;
					}
				} else {
					elem.ElementName = defaultName;
				}

				if (choiceEnumMap != null) {
					string cname = choiceEnumMap.GetEnumName (choiceEnumType.FullName, elem.ElementName);
					if (cname == null)
						throw new InvalidOperationException (string.Format (
							CultureInfo.InvariantCulture, "Type {0} is missing"
							+ " enumeration value '{1}' for element '{1} from"
							+ " namespace '{2}'.", choiceEnumType, elem.ElementName,
							elem.Namespace));
					elem.ChoiceValue = Enum.Parse (choiceEnumType, cname);
				}
					
				list.Add (elem);
			}
			return list;
		}

		XmlTypeMapElementInfoList ImportAnyElementInfo (string defaultNamespace, XmlReflectionMember rmember, XmlTypeMapMemberElement member, XmlAttributes atts)
		{
			XmlTypeMapElementInfoList list = new XmlTypeMapElementInfoList();

			ImportTextElementInfo (list, rmember.MemberType, member, atts, defaultNamespace);

			foreach (XmlAnyElementAttribute att in atts.XmlAnyElements)
			{
				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (member, TypeTranslator.GetTypeData(typeof(XmlElement)));
				if (att.Name.Length != 0) 
				{
					elem.ElementName = XmlConvert.EncodeLocalName(att.Name);
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

		void ImportTextElementInfo (XmlTypeMapElementInfoList list, Type defaultType, XmlTypeMapMemberElement member, XmlAttributes atts, string defaultNamespace)
		{
			if (atts.XmlText != null)
			{
				member.IsXmlTextCollector = true;
				if (atts.XmlText.Type != null) {
					TypeData td = TypeTranslator.GetTypeData (defaultType);
					if ((td.SchemaType == SchemaTypes.Primitive || td.SchemaType == SchemaTypes.Enum) && atts.XmlText.Type != defaultType) {
						throw new InvalidOperationException ("The type for XmlText may not be specified for primitive types.");
					}
					defaultType = atts.XmlText.Type;
				}
				if (defaultType == typeof(XmlNode)) defaultType = typeof(XmlText);	// Nodes must be text nodes

				XmlTypeMapElementInfo elem = new XmlTypeMapElementInfo (member, TypeTranslator.GetTypeData(defaultType, atts.XmlText.DataType));

				if (elem.TypeData.SchemaType != SchemaTypes.Primitive &&
					elem.TypeData.SchemaType != SchemaTypes.Enum &&
				    elem.TypeData.SchemaType != SchemaTypes.XmlNode &&
				    !(elem.TypeData.SchemaType == SchemaTypes.Array && elem.TypeData.ListItemTypeData.SchemaType == SchemaTypes.XmlNode)
				 )
					throw new InvalidOperationException ("XmlText cannot be used to encode complex types");

				if (elem.TypeData.IsComplexType)
					elem.MappedType = ImportTypeMapping (defaultType, null, defaultNamespace);
				elem.IsTextElement = true;
				elem.WrappedElement = false;
				list.Add (elem);
			}
		}
		
		bool CanBeNull (TypeData type)
		{
#if !NET_2_0	// idiotic compatibility
			if (type.Type == typeof (XmlQualifiedName))
				return false;
#endif
			return !type.Type.IsValueType || type.IsNullable;
		}
		
		public void IncludeType (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (includedTypes == null) includedTypes = new ArrayList ();
			if (!includedTypes.Contains (type))
				includedTypes.Add (type);
			
			if (relatedMaps.Count > 0) {
				foreach (XmlTypeMapping map in (ArrayList) relatedMaps.Clone ()) {
					if (map.TypeData.Type == typeof(object))
						map.DerivedTypes.Add (ImportTypeMapping (type));
				}
			}
		}

		public void IncludeTypes (ICustomAttributeProvider provider)
		{ 
			object[] ats = provider.GetCustomAttributes (typeof(XmlIncludeAttribute), true);
			
			foreach (XmlIncludeAttribute at in ats)
				IncludeType (at.Type);
		}

		private object GetDefaultValue (TypeData typeData, object defaultValue)
		{
			if (defaultValue == DBNull.Value || typeData.SchemaType != SchemaTypes.Enum)
				return defaultValue;

			// get string representation of enum value
			string namedValue = Enum.Format (typeData.Type, defaultValue, "g");
			// get decimal representation of enum value
			string decimalValue = Enum.Format (typeData.Type, defaultValue, "d");

			// if decimal representation matches string representation, then
			// the value is not defined in the enum type (as the "g" format
			// will return the decimal equivalent of the value if the value
			// is not equal to a combination of named enumerated constants
			if (namedValue == decimalValue) {
				string msg = string.Format (CultureInfo.InvariantCulture,
					"Value '{0}' cannot be converted to {1}.", defaultValue,
					defaultValue.GetType ().FullName);
				throw new InvalidOperationException (msg);
			}

			// XmlSerializer expects integral enum value
			//return namedValue.Replace (',', ' ');
			return defaultValue;
		}

		#endregion // Methods
	}
}
