// 
// System.Xml.Serialization.XmlSchemaImporter
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml;
using System.Xml.Schema;
using System.Collections;

namespace System.Xml.Serialization {
	public class XmlSchemaImporter {

		#region Fields

		XmlSchemas schemas;
		CodeIdentifiers typeIdentifiers;
		CodeIdentifiers elemIdentifiers = new CodeIdentifiers ();
		Hashtable mappedTypes = new Hashtable ();
		Hashtable dataMappedTypes = new Hashtable ();
		Queue pendingMaps = new Queue ();

		static readonly XmlQualifiedName anyType = new XmlQualifiedName ("anyType",XmlSchema.Namespace);
		XmlSchemaElement anyElement = null;

		class MapFixup
		{
			public XmlTypeMapping Map;
			public XmlSchemaComplexType SchemaType;
			public XmlQualifiedName TypeName;
		}

		#endregion

		#region Constructors

		public XmlSchemaImporter (XmlSchemas schemas)
		{
			this.schemas = schemas;
			typeIdentifiers = new CodeIdentifiers ();
		}

		public XmlSchemaImporter (XmlSchemas schemas, CodeIdentifiers typeIdentifiers)
			: this (schemas)
		{
			this.typeIdentifiers = typeIdentifiers;
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public XmlMembersMapping ImportAnyType (XmlQualifiedName typeName, string elementName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlTypeMapping ImportDerivedTypeMapping (XmlQualifiedName name, Type baseType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlTypeMapping ImportDerivedTypeMapping (XmlQualifiedName name, bool baseTypeCanBeIndirect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlTypeMapping ImportDerivedTypeMapping (XmlQualifiedName name,
								Type baseType,
								bool baseTypeCanBeIndirect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlMembersMapping ImportMembersMapping (XmlQualifiedName name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlMembersMapping ImportMembersMapping (XmlQualifiedName[] names)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlMembersMapping ImportMembersMapping (XmlQualifiedName[] names, Type baseType, bool baseTypeCanBeIndirect)
		{
			throw new NotImplementedException ();
		}

		public XmlTypeMapping ImportTypeMapping (XmlQualifiedName name)
		{
			XmlSchemaElement elem = (XmlSchemaElement) schemas.Find (name, typeof (XmlSchemaElement));
			if (elem == null) return null;

			// The root element must be an element with complex type

			XmlQualifiedName qname;
			XmlSchemaType stype;
			if (elem.SchemaType != null)
			{
				stype = elem.SchemaType;
				qname = name;
			}
			else
			{
				if (elem.SchemaTypeName.IsEmpty) return null;
				if (elem.SchemaTypeName.Namespace == XmlSchema.Namespace) return null;
				object type = schemas.Find (elem.SchemaTypeName, typeof (XmlSchemaComplexType));
				if (type == null) type = schemas.Find (elem.SchemaTypeName, typeof (XmlSchemaSimpleType));
				if (type == null) throw new InvalidOperationException ("Schema type '" + elem.SchemaTypeName + "' not found");
				stype = (XmlSchemaType) type;
				qname = stype.QualifiedName;
			}

			if (stype is XmlSchemaSimpleType) return null;

			XmlTypeMapping map = ImportType (qname, (XmlSchemaComplexType)stype, name);
			BuildPendingMaps ();
			return map;
		}

		public XmlTypeMapping ImportType (XmlQualifiedName name, XmlQualifiedName root)
		{
			XmlTypeMapping map = GetRegisteredTypeMapping (name);
			if (map != null) return map;

			XmlSchemaType type = (XmlSchemaType) schemas.Find (name, typeof (XmlSchemaComplexType));
			if (type == null) type = (XmlSchemaType) schemas.Find (name, typeof (XmlSchemaSimpleType));

			return ImportType (name, type, root);
		}

		XmlTypeMapping ImportType (XmlQualifiedName name, XmlSchemaType stype, XmlQualifiedName root)
		{
			XmlTypeMapping map = GetRegisteredTypeMapping (name);
			if (map != null) return map;

			if (stype is XmlSchemaComplexType)
				return ImportClassComplexType (name, (XmlSchemaComplexType) stype, root);
			else if (stype is XmlSchemaSimpleType)
				return ImportClassSimpleType (name, (XmlSchemaSimpleType) stype, root);

			throw new NotSupportedException ("Schema type not supported: " + stype.GetType ());
		}

		XmlTypeMapping ImportClassComplexType (XmlQualifiedName typeQName, XmlSchemaComplexType stype, XmlQualifiedName root)
		{
//			if (root != null) typeQName = root;

			XmlTypeMapping map;

			// The need for fixups: If the complex type is an array, then to get the type of the
			// array we need first to get the type of the items of the array.
			// But if one of the item types or its children has a referece to this type array,
			// then we enter in an infinite loop. This does not happen with class types because
			// the class map is registered before parsing the children. We can't do the same
			// with the array type because to register the array map we need the type of the array.

			if (root == null && CanBeArray (typeQName, stype))
			{
				TypeData typeData;
				ListMap listMap = BuildArrayMap (typeQName, stype, out typeData);
				if (listMap != null)
				{
					map = CreateArrayTypeMapping (typeQName, typeData);
					map.ObjectMap = listMap;
					return map;
				}

				// After all, it is not an array. Create a class map then.
			}
			else if (CanBeAnyElement (stype))
			{
				return GetTypeMapping (TypeTranslator.GetTypeData(typeof(XmlElement)));
			}
			else if (CanBeIXmlSerializable (stype))
			{
				return GetTypeMapping (TypeTranslator.GetTypeData(typeof(object)));
			}

			// Register the map right now but do not build it,
			// This will avoid loops.

			map = CreateTypeMapping (typeQName, SchemaTypes.Class, root);
			RegisterMapFixup (map, typeQName, stype);
			return map;
		}

		void RegisterMapFixup (XmlTypeMapping map, XmlQualifiedName typeQName, XmlSchemaComplexType stype)
		{
			MapFixup fixup = new MapFixup ();
			fixup.Map = map;
			fixup.SchemaType = stype;
			fixup.TypeName = typeQName;
			pendingMaps.Enqueue (fixup);
		}

		void BuildPendingMaps ()
		{
			while (pendingMaps.Count > 0) {
				MapFixup fixup  = (MapFixup) pendingMaps.Dequeue ();
				if (fixup.Map.ObjectMap == null) {
					BuildClassMap (fixup.Map, fixup.TypeName, fixup.SchemaType);
					if (fixup.Map.ObjectMap == null) pendingMaps.Enqueue (fixup);
				}
			}
		}

		void BuildPendingMap (XmlTypeMapping map)
		{
			if (map.ObjectMap != null) return;

			foreach (MapFixup fixup in pendingMaps)
			{
				if (fixup.Map == map) {
					BuildClassMap (fixup.Map, fixup.TypeName, fixup.SchemaType);
					return;
				}
			}
			throw new InvalidOperationException ("Can't complete map of type " + map.XmlType + " : " + map.Namespace);
		}

		void BuildClassMap (XmlTypeMapping map, XmlQualifiedName typeQName, XmlSchemaComplexType stype)
		{
			CodeIdentifiers classIds = new CodeIdentifiers();
			classIds.AddReserved (map.TypeData.TypeName);

			ClassMap cmap = new ClassMap ();
			map.ObjectMap = cmap;
			bool isMixed = stype.IsMixed;

			if (stype.Particle != null)
			{
				ImportParticleComplexContent (typeQName, cmap, stype.Particle, classIds, isMixed);
			}
			else
			{
				if (stype.ContentModel is XmlSchemaSimpleContent) {
					ImportSimpleContent (typeQName, map, (XmlSchemaSimpleContent)stype.ContentModel, classIds, isMixed);
				}
				else if (stype.ContentModel is XmlSchemaComplexContent) {
					ImportComplexContent (typeQName, map, (XmlSchemaComplexContent)stype.ContentModel, classIds, isMixed);
				}
			}

			ImportAttributes (typeQName, cmap, stype.Attributes, stype.AnyAttribute, classIds);
		}

		void ImportAttributes (XmlQualifiedName typeQName, ClassMap cmap, XmlSchemaObjectCollection atts, XmlSchemaAnyAttribute anyat, CodeIdentifiers classIds)
		{
			if (anyat != null)
			{
    			XmlTypeMapMemberAnyAttribute member = new XmlTypeMapMemberAnyAttribute ();
				member.Name = classIds.AddUnique ("AnyAttribute", member);
				member.TypeData = TypeTranslator.GetTypeData (typeof(XmlAttribute[]));
				cmap.AddMember (member);
			}
			
			foreach (XmlSchemaObject at in atts)
			{
				if (at is XmlSchemaAttribute)
				{
					string ns;
					XmlSchemaAttribute attr = (XmlSchemaAttribute)at;
					XmlSchemaAttribute refAttr = GetRefAttribute (typeQName, attr, out ns);
					XmlTypeMapMemberAttribute member = new XmlTypeMapMemberAttribute ();
					member.Name = classIds.AddUnique (CodeIdentifier.MakeValid (refAttr.Name), member);
					member.AttributeName = refAttr.Name;
					member.Namespace = ns;
					member.Form = refAttr.Form;
					member.TypeData = GetAttributeTypeData (typeQName, attr);
					if (refAttr.DefaultValue != null) member.DefaultValue = refAttr.DefaultValue;
					if (member.TypeData.IsComplexType)
						member.MappedType = GetTypeMapping (member.TypeData);
					cmap.AddMember (member);
				}
				else if (at is XmlSchemaAttributeGroupRef)
				{
					XmlSchemaAttributeGroupRef gref = (XmlSchemaAttributeGroupRef)at;
					XmlSchemaAttributeGroup grp = (XmlSchemaAttributeGroup) schemas.Find (gref.RefName, typeof(XmlSchemaAttributeGroup));
					ImportAttributes (typeQName, cmap, grp.Attributes, grp.AnyAttribute, classIds);
				}
			}
		}

		ListMap BuildArrayMap (XmlQualifiedName typeQName, XmlSchemaComplexType stype, out TypeData arrayTypeData)
		{
			ClassMap cmap = new ClassMap ();
			CodeIdentifiers classIds = new CodeIdentifiers();
			ImportParticleComplexContent (typeQName, cmap, stype.Particle, classIds, false);

			XmlTypeMapMemberFlatList list = (cmap.AllMembers.Count == 1) ? cmap.AllMembers[0] as XmlTypeMapMemberFlatList : null;
			if (list != null && list.ChoiceMember == null)
			{
				arrayTypeData = list.TypeData;
				return list.ListMap;
			}
			else
			{
				arrayTypeData = null;
				return null;
			}
		}

		void ImportParticleComplexContent (XmlQualifiedName typeQName, ClassMap cmap, XmlSchemaParticle particle, CodeIdentifiers classIds, bool isMixed)
		{
			ImportParticleContent (typeQName, cmap, particle, classIds, false, ref isMixed);
			if (isMixed)
			{
				XmlTypeMapMemberFlatList member = new XmlTypeMapMemberFlatList ();
				member.Name = classIds.AddUnique ("Text", member);
				member.TypeData = TypeTranslator.GetTypeData (typeof(string[]));
				member.ElementInfo.Add (CreateTextElementInfo (typeQName.Namespace, member, member.TypeData.ListItemTypeData));
				member.IsXmlTextCollector = true;
				member.ListMap = new ListMap ();
				member.ListMap.ItemInfo = member.ElementInfo;
				cmap.AddMember (member);
			}
		}
		
		void ImportParticleContent (XmlQualifiedName typeQName, ClassMap cmap, XmlSchemaParticle particle, CodeIdentifiers classIds, bool multiValue, ref bool isMixed)
		{
			if (particle is XmlSchemaGroupRef)
				particle = GetRefGroupParticle ((XmlSchemaGroupRef)particle);

			if (particle.MaxOccurs > 1) multiValue = true;
			
			if (particle is XmlSchemaSequence) {
				ImportSequenceContent (typeQName, cmap, ((XmlSchemaSequence)particle).Items, classIds, multiValue, ref isMixed);
			}
			else if (particle is XmlSchemaChoice) {
				if (((XmlSchemaChoice)particle).Items.Count == 1)
					ImportSequenceContent (typeQName, cmap, ((XmlSchemaChoice)particle).Items, classIds, multiValue, ref isMixed);
				else
					ImportChoiceContent (typeQName, cmap, (XmlSchemaChoice)particle, classIds, multiValue);
			}
			else if (particle is XmlSchemaAll) {
				ImportSequenceContent (typeQName, cmap, ((XmlSchemaAll)particle).Items, classIds, multiValue, ref isMixed);
			}
		}

		void ImportSequenceContent (XmlQualifiedName typeQName, ClassMap cmap, XmlSchemaObjectCollection items, CodeIdentifiers classIds, bool multiValue, ref bool isMixed)
		{
			foreach (XmlSchemaObject item in items)
			{
				XmlTypeMapMember mapMember;

				if (item is XmlSchemaElement)
				{
					string ns;
					XmlSchemaElement elem = (XmlSchemaElement) item;
					TypeData typeData = GetElementTypeData (typeQName, elem);
					XmlSchemaElement refElem = GetRefElement (typeQName, elem, out ns);

					if (elem.MaxOccurs == 1 && !multiValue)
					{
						XmlTypeMapMemberElement member = null;
						if (typeData.SchemaType != SchemaTypes.Array)
						{
							member = new XmlTypeMapMemberElement ();
							if (refElem.DefaultValue != null) member.DefaultValue = refElem.DefaultValue;
						}
						else if (GetTypeMapping (typeData).IsSimpleType)
						{
							// It is a simple list (space separated list).
							// Since this is not supported, map as a single item value
							// TODO: improve this
							member = new XmlTypeMapMemberElement ();
							typeData = typeData.ListItemTypeData;
						}
						else
							member = new XmlTypeMapMemberList ();

						member.Name = classIds.AddUnique(CodeIdentifier.MakeValid(refElem.Name), member);
						member.TypeData = typeData;
						member.ElementInfo.Add (CreateElementInfo (ns, member, refElem.Name, typeData, refElem.IsNillable));
						cmap.AddMember (member);
					}
					else
					{
						XmlTypeMapMemberFlatList member = new XmlTypeMapMemberFlatList ();
						member.ListMap = new ListMap ();
						member.Name = classIds.AddUnique(CodeIdentifier.MakeValid(refElem.Name), member);
						member.TypeData = typeData.ListTypeData;
						member.ElementInfo.Add (CreateElementInfo (ns, member, refElem.Name, typeData, refElem.IsNillable));
						member.ListMap.ItemInfo = member.ElementInfo;
						cmap.AddMember (member);
					}
				}
				else if (item is XmlSchemaAny)
				{
					XmlSchemaAny elem = (XmlSchemaAny) item;
					XmlTypeMapMemberAnyElement member = new XmlTypeMapMemberAnyElement ();
					member.Name = classIds.AddUnique ("Any", member);

					Type ctype;
					if (elem.MaxOccurs > 1 || multiValue)
						ctype = isMixed ? typeof(XmlNode[]) : typeof(XmlElement[]);
					else
						ctype = isMixed ? typeof(XmlNode) : typeof(XmlElement);

					member.TypeData = TypeTranslator.GetTypeData (ctype);
					XmlTypeMapElementInfo einfo = new XmlTypeMapElementInfo (member, member.TypeData);
					einfo.IsUnnamedAnyElement = true;
					member.ElementInfo.Add (einfo);

					if (isMixed)
					{
						einfo = CreateTextElementInfo (typeQName.Namespace, member, member.TypeData);
						member.ElementInfo.Add (einfo);
						member.IsXmlTextCollector = true;
						isMixed = false;	//Allow only one XmlTextAttribute
					}
					
					cmap.AddMember (member);
				}
				else if (item is XmlSchemaParticle) {
					ImportParticleContent (typeQName, cmap, (XmlSchemaParticle)item, classIds, multiValue, ref isMixed);
				}
			}
		}

		void ImportChoiceContent (XmlQualifiedName typeQName, ClassMap cmap, XmlSchemaChoice choice, CodeIdentifiers classIds, bool multiValue)
		{
			XmlTypeMapElementInfoList choices = new XmlTypeMapElementInfoList ();
			multiValue = ImportChoices (typeQName, null, choices, choice.Items) || multiValue;
			if (choices.Count == 0) return;

			if (choice.MaxOccurs > 1) multiValue = true;

			XmlTypeMapMemberElement member;
			if (multiValue)
			{
				member = new XmlTypeMapMemberFlatList ();
				member.Name = classIds.AddUnique ("Items", member);
				ListMap listMap = new ListMap ();
				listMap.ItemInfo = choices;
				((XmlTypeMapMemberFlatList)member).ListMap = listMap;
			}
			else
			{
				member = new XmlTypeMapMemberElement ();
				member.Name = classIds.AddUnique ("Item", member);
			}
			
			// If all choices have the same type, use that type for the member.
			// If not use System.Object.
			// If there are at least two choices with the same type, use a choice
			// identifier attribute

			TypeData typeData = null;
			bool twoEqual = false;
			bool allEqual = true;
			Hashtable types = new Hashtable ();
			foreach (XmlTypeMapElementInfo einfo in choices)
			{
				if (types.ContainsKey (einfo.TypeData)) twoEqual = true;
				else types.Add (einfo.TypeData, einfo);

				TypeData choiceType = einfo.TypeData;
				if (choiceType.SchemaType == SchemaTypes.Class)
				{
					// When comparing class types, use the most generic class in the
					// inheritance hierarchy

					XmlTypeMapping choiceMap = GetTypeMapping (choiceType);
					BuildPendingMap (choiceMap);
					while (choiceMap.BaseMap != null) {
						choiceMap = choiceMap.BaseMap;
						BuildPendingMap (choiceMap);
						choiceType = choiceMap.TypeData;
					}
				}
				
				if (typeData == null) typeData = choiceType;
				else if (typeData != choiceType) allEqual = false;
			}

			if (!allEqual)
				typeData = TypeTranslator.GetTypeData (typeof(object));

			if (twoEqual)
			{
				// Create the choice member
				XmlTypeMapMemberElement choiceMember = new XmlTypeMapMemberElement ();
				choiceMember.Name = classIds.AddUnique (member.Name + "ElementName", choiceMember);
				member.ChoiceMember = choiceMember.Name;

				// Create the choice enum
				XmlTypeMapping enumMap = CreateTypeMapping (new XmlQualifiedName (member.Name + "ChoiceType", typeQName.Namespace), SchemaTypes.Enum, null);
				CodeIdentifiers codeIdents = new CodeIdentifiers ();
				EnumMap.EnumMapMember[] members = new EnumMap.EnumMapMember [choices.Count];
				for (int n=0; n<choices.Count; n++)
				{
					XmlTypeMapElementInfo it =(XmlTypeMapElementInfo) choices[n];
					string xmlName = (it.Namespace != null && it.Namespace != "") ? it.Namespace + ":" + it.ElementName : it.ElementName;
					string enumName = codeIdents.AddUnique (CodeIdentifier.MakeValid (it.ElementName), it);
					members [n] = new EnumMap.EnumMapMember (xmlName, enumName);
				}
				enumMap.ObjectMap = new EnumMap (members, false);

				choiceMember.TypeData = multiValue ? enumMap.TypeData.ListTypeData : enumMap.TypeData;
				choiceMember.ElementInfo.Add (CreateElementInfo (typeQName.Namespace, choiceMember, choiceMember.Name, choiceMember.TypeData, false));
				cmap.AddMember (choiceMember);
			}

			if (multiValue)
				typeData = typeData.ListTypeData;

			member.ElementInfo = choices;
			member.TypeData = typeData;
			cmap.AddMember (member);
		}

		bool ImportChoices (XmlQualifiedName typeQName, XmlTypeMapMember member, XmlTypeMapElementInfoList choices, XmlSchemaObjectCollection items)
		{
			bool multiValue = false;
			foreach (XmlSchemaObject titem in items)
			{
				XmlSchemaObject item = titem;
				if (item is XmlSchemaGroupRef)
					item = GetRefGroupParticle ((XmlSchemaGroupRef)item);

				if (item is XmlSchemaElement)
				{
					string ns;
					XmlSchemaElement elem = (XmlSchemaElement) item;
					TypeData typeData = GetElementTypeData (typeQName, elem);
					XmlSchemaElement refElem = GetRefElement (typeQName, elem, out ns);
					choices.Add (CreateElementInfo (ns, member, refElem.Name, typeData, refElem.IsNillable));
					if (elem.MaxOccurs > 1) multiValue = true;
				}
				else if (item is XmlSchemaAny)
				{
					XmlTypeMapElementInfo einfo = new XmlTypeMapElementInfo (member, TypeTranslator.GetTypeData(typeof(XmlElement)));
					einfo.IsUnnamedAnyElement = true;
					choices.Add (einfo);
				}
				else if (item is XmlSchemaChoice) {
					multiValue = ImportChoices (typeQName, member, choices, ((XmlSchemaChoice)item).Items) || multiValue;
				}
				else if (item is XmlSchemaSequence) {
					multiValue = ImportChoices (typeQName, member, choices, ((XmlSchemaSequence)item).Items) || multiValue;
				}
			}
			return multiValue;
		}

		void ImportSimpleContent (XmlQualifiedName typeQName, XmlTypeMapping map, XmlSchemaSimpleContent content, CodeIdentifiers classIds, bool isMixed)
		{
			ClassMap cmap = (ClassMap)map.ObjectMap;
			
			XmlQualifiedName qname = GetContentBaseType (content.Content);

			XmlTypeMapMemberElement member = new XmlTypeMapMemberElement ();
			member.Name = classIds.AddUnique("Value", member);
			member.TypeData = FindBuiltInType (qname);
			member.ElementInfo.Add (CreateTextElementInfo (typeQName.Namespace, member, member.TypeData));
			member.IsXmlTextCollector = true;
			cmap.AddMember (member);

			XmlSchemaSimpleContentExtension ext = content.Content as XmlSchemaSimpleContentExtension;
			if (ext != null)
				ImportAttributes (typeQName, cmap, ext.Attributes, ext.AnyAttribute, classIds);
		}

		TypeData FindBuiltInType (XmlQualifiedName qname)
		{
			if (qname.Namespace == XmlSchema.Namespace)
				return TypeTranslator.GetPrimitiveTypeData (qname.Name);

			XmlSchemaComplexType ct = (XmlSchemaComplexType) schemas.Find (qname, typeof(XmlSchemaComplexType));
			if (ct != null)
			{
				XmlSchemaSimpleContent sc = ct.ContentModel as XmlSchemaSimpleContent;
				if (sc == null) throw new InvalidOperationException ("Invalid schema");
				return FindBuiltInType (GetContentBaseType (sc.Content));
			}
			
			XmlSchemaSimpleType st = (XmlSchemaSimpleType) schemas.Find (qname, typeof(XmlSchemaSimpleType));
			if (st != null)
				return FindBuiltInType (qname, st);

			throw new InvalidOperationException ("Definition of type " + qname + " not found");
		}

		TypeData FindBuiltInType (XmlQualifiedName qname, XmlSchemaSimpleType st)
		{
			if (CanBeEnum (st))
				return ImportType (qname, null).TypeData;

			if (st.Content is XmlSchemaSimpleTypeRestriction) {
				return FindBuiltInType (GetContentBaseType (st.Content));
			}
			else if (st.Content is XmlSchemaSimpleTypeList) {
				return FindBuiltInType (GetContentBaseType (st.Content)).ListTypeData;
			}
			else if (st.Content is XmlSchemaSimpleTypeUnion)
			{
				// Check if all types of the union are equal. If not, then will use anyType.
				XmlSchemaSimpleTypeUnion uni = (XmlSchemaSimpleTypeUnion) st.Content;
				TypeData utype = null;

				// Anonymous types are unique
				if (uni.BaseTypes.Count != 0 && uni.MemberTypes.Length != 0)
					return FindBuiltInType (anyType);

				foreach (XmlQualifiedName mt in uni.MemberTypes)
				{
					TypeData qn = FindBuiltInType (mt);
					if (utype != null && qn != utype) return FindBuiltInType (anyType);
					else utype = qn;
				}
				return utype;
			}
			else
				return null;
		}

		XmlQualifiedName GetContentBaseType (XmlSchemaObject ob)
		{
			if (ob is XmlSchemaSimpleContentExtension)
				return ((XmlSchemaSimpleContentExtension)ob).BaseTypeName;
			else if (ob is XmlSchemaSimpleContentRestriction)
				return ((XmlSchemaSimpleContentRestriction)ob).BaseTypeName;
			else if (ob is XmlSchemaSimpleTypeRestriction)
				return ((XmlSchemaSimpleTypeRestriction)ob).BaseTypeName;
			else if (ob is XmlSchemaSimpleTypeList)
				return ((XmlSchemaSimpleTypeList)ob).ItemTypeName;
			else
				return null;
		}

		void ImportComplexContent (XmlQualifiedName typeQName, XmlTypeMapping map, XmlSchemaComplexContent content, CodeIdentifiers classIds, bool isMixed)
		{
			XmlSchemaComplexContentExtension ext = content.Content as XmlSchemaComplexContentExtension;

			ClassMap cmap = (ClassMap)map.ObjectMap;
			XmlQualifiedName qname;

			if (ext != null) qname = ext.BaseTypeName;
			else qname = ((XmlSchemaComplexContentRestriction)content.Content).BaseTypeName;
			
			// Add base map members to this map

			XmlTypeMapping baseMap = ImportType (qname, null);
			BuildPendingMap (baseMap);
			ClassMap baseClassMap = (ClassMap)baseMap.ObjectMap;

			foreach (XmlTypeMapMember member in baseClassMap.AllMembers)
				cmap.AddMember (member);

			if (baseClassMap.XmlTextCollector != null) isMixed = false;
			else if (content.IsMixed) isMixed = true;

			map.BaseMap = baseMap;
			baseMap.DerivedTypes.Add (map);

			if (ext != null) {
				// Add the members of this map
				ImportParticleComplexContent (typeQName, cmap, ext.Particle, classIds, isMixed);
				ImportAttributes (typeQName, cmap, ext.Attributes, ext.AnyAttribute, classIds);
			}
			else {
				if (isMixed) ImportParticleComplexContent (typeQName, cmap, null, classIds, true);
			}
		}

		XmlTypeMapping ImportClassSimpleType (XmlQualifiedName typeQName, XmlSchemaSimpleType stype, XmlQualifiedName root)
		{
			if (CanBeEnum (stype))
			{
				// Create an enum map

				CodeIdentifiers codeIdents = new CodeIdentifiers ();
				XmlSchemaSimpleTypeRestriction rest = (XmlSchemaSimpleTypeRestriction)stype.Content;

				XmlTypeMapping enumMap = CreateTypeMapping (typeQName, SchemaTypes.Enum, null);
				codeIdents.AddReserved (enumMap.TypeData.TypeName);

				EnumMap.EnumMapMember[] members = new EnumMap.EnumMapMember [rest.Facets.Count];
				for (int n=0; n<rest.Facets.Count; n++)
				{
					XmlSchemaEnumerationFacet enu = (XmlSchemaEnumerationFacet) rest.Facets[n];
					string enumName = codeIdents.AddUnique(CodeIdentifier.MakeValid (enu.Value), enu);
					members [n] = new EnumMap.EnumMapMember (enu.Value, enumName);
				}
				enumMap.ObjectMap = new EnumMap (members, false);
				enumMap.IsSimpleType = true;
				return enumMap;
			}

			if (stype.Content is XmlSchemaSimpleTypeList)
			{
				XmlSchemaSimpleTypeList slist = (XmlSchemaSimpleTypeList)stype.Content;
				TypeData arrayTypeData = FindBuiltInType (slist.ItemTypeName, stype);

				ListMap listMap = new ListMap ();
				listMap.ItemInfo = new XmlTypeMapElementInfoList ();
				listMap.ItemInfo.Add (CreateElementInfo (typeQName.Namespace, null, "Item", arrayTypeData.ListItemTypeData, false));

				XmlTypeMapping map = CreateArrayTypeMapping (typeQName, arrayTypeData);
				map.ObjectMap = listMap;
				map.IsSimpleType = true;
				return map;
			}

			// It is an extension of a primitive or known type
			
			TypeData typeData = FindBuiltInType (typeQName, stype);
			return GetTypeMapping (typeData);
		}

		bool CanBeEnum (XmlSchemaSimpleType stype)
		{
			if (stype.Content is XmlSchemaSimpleTypeRestriction)
			{
				XmlSchemaSimpleTypeRestriction rest = (XmlSchemaSimpleTypeRestriction)stype.Content;
				foreach (object ob in rest.Facets)
					if (!(ob is XmlSchemaEnumerationFacet)) return false;
				return true;
			}
			return false;
		}

		bool CanBeArray (XmlQualifiedName typeQName, XmlSchemaComplexType stype)
		{
			return !stype.IsMixed && CanBeArray (typeQName, stype.Particle, false);
		}

		bool CanBeArray (XmlQualifiedName typeQName, XmlSchemaParticle particle, bool multiValue)
		{
			// To be an array, there can't be a direct child of type typeQName

			if (particle == null) return false;

			multiValue = multiValue || particle.MaxOccurs > 1;

			if (particle is XmlSchemaGroupRef)
				return CanBeArray (typeQName, GetRefGroupParticle ((XmlSchemaGroupRef)particle), multiValue);

			if (particle is XmlSchemaElement)
			{
				XmlSchemaElement elem = (XmlSchemaElement)particle;
				if (!elem.RefName.IsEmpty)
					return CanBeArray (typeQName, FindRefElement (elem), multiValue);
				else
					return multiValue && !typeQName.Equals (((XmlSchemaElement)particle).SchemaTypeName);
			}

			if (particle is XmlSchemaAny)
				return multiValue;

			if (particle is XmlSchemaSequence)
			{
				XmlSchemaSequence seq = particle as XmlSchemaSequence;
				if (seq.Items.Count != 1) return false;
				return CanBeArray (typeQName, (XmlSchemaParticle)seq.Items[0], multiValue);
			}

			if (particle is XmlSchemaChoice)
			{
				// Can be array if all choices have different types
				ArrayList types = new ArrayList ();
				if(!CheckChoiceType (typeQName, particle, types, ref multiValue)) return false;
				return multiValue;
			}

			return false;
		}

		bool CheckChoiceType (XmlQualifiedName typeQName, XmlSchemaParticle particle, ArrayList types, ref bool multiValue)
		{
			XmlQualifiedName type = null;

			multiValue = multiValue || particle.MaxOccurs > 1;

			if (particle is XmlSchemaGroupRef)
				return CheckChoiceType (typeQName, GetRefGroupParticle ((XmlSchemaGroupRef)particle), types, ref multiValue);

			if (particle is XmlSchemaElement) {
				string ns;
				XmlSchemaElement elem = (XmlSchemaElement)particle;
				XmlSchemaElement refElem = GetRefElement (typeQName, elem, out ns);
				if (refElem.SchemaType != null) return true;
				type = refElem.SchemaTypeName;
			}
			else if (particle is XmlSchemaAny) {
				type = anyType;
			}
			else if (particle is XmlSchemaSequence)
			{
				XmlSchemaSequence seq = particle as XmlSchemaSequence;
				foreach (XmlSchemaParticle par in seq.Items)
					if (!CheckChoiceType (typeQName, par, types, ref multiValue)) return false;
				return true;
			}
			else if (particle is XmlSchemaChoice)
			{
				foreach (XmlSchemaParticle choice in ((XmlSchemaChoice)particle).Items)
					if (!CheckChoiceType (typeQName, choice, types, ref multiValue)) return false;
				return true;
			}

			if (typeQName.Equals (type)) return false;

			// For primitive types, compare using CLR types, since several
			// xml types can be mapped to a single CLR type

			string t;
			if (type.Namespace == XmlSchema.Namespace)
				t = TypeTranslator.GetPrimitiveTypeData (type.Name).FullTypeName + ":" + type.Namespace;

			else
				t = type.Name + ":" + type.Namespace;

			if (types.Contains (t)) return false;
			types.Add (t);
			return true;
		}
		
		bool CanBeAnyElement (XmlSchemaComplexType stype)
		{
			XmlSchemaSequence seq = stype.Particle as XmlSchemaSequence;
			return (seq != null) && (seq.Items.Count == 1) && (seq.Items[0] is XmlSchemaAny);
		}

		bool CanBeIXmlSerializable (XmlSchemaComplexType stype)
		{
			XmlSchemaSequence seq = stype.Particle as XmlSchemaSequence;
			if (seq == null) return false;
			if (seq.Items.Count != 2) return false;
			XmlSchemaElement elem = seq.Items[0] as XmlSchemaElement;
			if (elem == null) return false;
			if (elem.RefName != new XmlQualifiedName ("schema",XmlSchema.Namespace)) return false;
			return (seq.Items[1] is XmlSchemaAny);
		}

		XmlTypeMapElementInfo CreateElementInfo (string ns, XmlTypeMapMember member, string name, TypeData typeData, bool isNillable)
		{
			XmlTypeMapElementInfo einfo = new XmlTypeMapElementInfo (member, typeData);
			einfo.ElementName = name;
			einfo.Namespace = ns;
			einfo.IsNullable = isNillable;
			if (einfo.TypeData.IsComplexType)
				einfo.MappedType = GetTypeMapping (typeData);
			return einfo;
		}

		XmlTypeMapElementInfo CreateTextElementInfo (string ns, XmlTypeMapMember member, TypeData typeData)
		{
			XmlTypeMapElementInfo einfo = new XmlTypeMapElementInfo (member, typeData);
			einfo.IsTextElement = true;
			einfo.WrappedElement = false;
			if (typeData.IsComplexType)
				einfo.MappedType = GetTypeMapping (typeData);
			return einfo;
		}

		XmlTypeMapping CreateTypeMapping (XmlQualifiedName typeQName, SchemaTypes schemaType, XmlQualifiedName root)
		{
			string typeName = CodeIdentifier.MakeValid (typeQName.Name);
			typeName = typeIdentifiers.AddUnique (typeName, null);

			TypeData typeData = new TypeData (typeName, typeName, typeName, schemaType, null);

			XmlQualifiedName elemName = (root != null) ? root : typeQName;
			
			XmlTypeMapping map = new XmlTypeMapping (elemName.Name, elemName.Namespace, typeData, typeQName.Name, typeQName.Namespace);
			mappedTypes [typeQName] = map;
			dataMappedTypes [typeData] = map;

			return map;
		}

		XmlTypeMapping CreateArrayTypeMapping (XmlQualifiedName typeQName, TypeData arrayTypeData)
		{
			XmlTypeMapping map = new XmlTypeMapping (arrayTypeData.XmlType, typeQName.Namespace, arrayTypeData, arrayTypeData.XmlType, typeQName.Namespace);
			mappedTypes [typeQName] = map;
			dataMappedTypes [arrayTypeData] = map;

			return map;
		}

		XmlSchemaElement GetRefElement (XmlQualifiedName typeQName, XmlSchemaElement elem, out string ns)
		{
			if (!elem.RefName.IsEmpty)
			{
				ns = elem.RefName.Namespace;
				return FindRefElement (elem);
			}
			else
			{
				ns = typeQName.Namespace;
				return elem;
			}
		}

		XmlSchemaAttribute GetRefAttribute (XmlQualifiedName typeQName, XmlSchemaAttribute attr, out string ns)
		{
			if (!attr.RefName.IsEmpty)
			{
				ns = attr.RefName.Namespace;
				return (XmlSchemaAttribute) schemas.Find (attr.RefName, typeof(XmlSchemaAttribute));
			}
			else
			{
				ns = typeQName.Namespace;
				return attr;
			}
		}

		TypeData GetElementTypeData (XmlQualifiedName typeQName, XmlSchemaElement elem)
		{
			if (!elem.RefName.IsEmpty) {
				XmlSchemaElement refElem = FindRefElement (elem);
				if (refElem == null) throw new InvalidOperationException ("Ref type not found: " + elem.RefName);
				return GetElementTypeData (typeQName, refElem);
			}
			
			if (!elem.SchemaTypeName.IsEmpty) return GetTypeData (elem.SchemaTypeName);
			else if (elem.SchemaType == null) return TypeTranslator.GetTypeData (typeof(object));
			else return GetTypeData (elem.SchemaType, typeQName, elem.Name);
		}

		TypeData GetAttributeTypeData (XmlQualifiedName typeQName, XmlSchemaAttribute attr)
		{
			if (!attr.RefName.IsEmpty)
				return GetAttributeTypeData (typeQName, (XmlSchemaAttribute)schemas.Find (attr.RefName, typeof (XmlSchemaAttribute)));

			if (!attr.SchemaTypeName.IsEmpty) return GetTypeData (attr.SchemaTypeName);
			else return GetTypeData (attr.SchemaType, typeQName, attr.Name);
		}

		TypeData GetTypeData (XmlQualifiedName typeQName)
		{
			if (typeQName.Namespace == XmlSchema.Namespace)
				return TypeTranslator.GetPrimitiveTypeData (typeQName.Name);

			return ImportType (typeQName, null).TypeData;
		}

		TypeData GetTypeData (XmlSchemaType stype, XmlQualifiedName typeQNname, string propertyName)
		{
			string baseName = typeQNname.Name + typeIdentifiers.MakeRightCase (propertyName);
			baseName = elemIdentifiers.AddUnique (baseName, stype);
			
			XmlQualifiedName newName;
			newName = new XmlQualifiedName (baseName, typeQNname.Namespace);

			XmlTypeMapping map = ImportType (newName, stype, null);
			return map.TypeData;
		}

		XmlTypeMapping GetTypeMapping (TypeData typeData)
		{
			XmlTypeMapping map = (XmlTypeMapping) dataMappedTypes [typeData];
			if (map != null) return map;
			
			if (map == null && typeData.IsListType)
			{
				// Create an array map for the type

				XmlTypeMapping itemMap = GetTypeMapping (typeData.ListItemTypeData);
				
				map = new XmlTypeMapping (typeData.XmlType, itemMap.Namespace, typeData, typeData.XmlType, itemMap.Namespace);
				ListMap listMap = new ListMap ();
				listMap.ItemInfo = new XmlTypeMapElementInfoList();
				listMap.ItemInfo.Add (CreateElementInfo (itemMap.Namespace, null, typeData.ListItemTypeData.XmlType, typeData.ListItemTypeData, false));
				map.ObjectMap = listMap;
				
				mappedTypes [new XmlQualifiedName(map.ElementName, map.Namespace)] = map;
				dataMappedTypes [typeData] = map;
				return map;
			}
			else if (typeData.SchemaType == SchemaTypes.Primitive || typeData.Type == typeof(object) || typeof(XmlNode).IsAssignableFrom(typeData.Type))
			{
				map = new XmlTypeMapping (typeData.XmlType, XmlSchema.Namespace, typeData, typeData.XmlType, XmlSchema.Namespace);
				dataMappedTypes [typeData] = map;
				return map;
			}
			
			throw new InvalidOperationException ("Map for type " + typeData.TypeName + " not found");
		}

		XmlTypeMapping GetRegisteredTypeMapping (XmlQualifiedName typeQName)
		{
			return (XmlTypeMapping) mappedTypes [typeQName];
		}

		XmlSchemaParticle GetRefGroupParticle (XmlSchemaGroupRef refGroup)
		{
			XmlSchemaGroup grp = (XmlSchemaGroup) schemas.Find (refGroup.RefName, typeof (XmlSchemaGroup));
			return grp.Particle;
		}

		XmlSchemaElement FindRefElement (XmlSchemaElement elem)
		{
			if (elem.RefName.Namespace == XmlSchema.Namespace)
			{
				if (anyElement != null) return anyElement;
				anyElement = new XmlSchemaElement ();
				anyElement.Name = "any";
				anyElement.SchemaTypeName = anyType;
				return anyElement;
			}
			return (XmlSchemaElement) schemas.Find (elem.RefName, typeof(XmlSchemaElement));
		}

		#endregion // Methods
	}
}
