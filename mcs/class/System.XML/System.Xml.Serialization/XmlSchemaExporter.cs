// 
// System.Xml.Serialization.XmlSchemaExporter 
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
	public class XmlSchemaExporter {

		#region Fields

		XmlSchemas schemas;
		Hashtable exportedMaps;

		#endregion

		#region Constructors

		public XmlSchemaExporter (XmlSchemas schemas)
		{
			this.schemas = schemas;
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public string ExportAnyType (string ns)
		{
			throw new NotImplementedException ();
		}

		public void ExportMembersMapping (XmlMembersMapping xmlMembersMapping)
		{
			exportedMaps = new Hashtable ();
			XmlSchema schema = GetSchema (xmlMembersMapping.Namespace);

			XmlSchemaElement selem = new XmlSchemaElement ();
			selem.Name = xmlMembersMapping.ElementName;
			schema.Items.Add (selem);

			XmlSchemaComplexType stype = new XmlSchemaComplexType ();

			XmlSchemaSequence particle;
			XmlSchemaAnyAttribute anyAttribute;
			ExportMembersMapSchema (schema, (ClassMap)xmlMembersMapping.ObjectMap, null, stype.Attributes, out particle, out anyAttribute);
			stype.Particle = particle;
			stype.AnyAttribute = anyAttribute;

			selem.SchemaType = stype;

			CompileSchemas ();
		}

		[MonoTODO]
		public XmlQualifiedName ExportTypeMapping (XmlMembersMapping xmlMembersMapping)
		{
			throw new NotImplementedException ();
		}

		public void ExportTypeMapping (XmlTypeMapping xmlTypeMapping)
		{
			exportedMaps = new Hashtable ();
			XmlSchema schema = GetSchema (null);
			XmlTypeMapElementInfo einfo = new XmlTypeMapElementInfo (null, xmlTypeMapping.TypeData);
			einfo.Namespace = "";
			einfo.ElementName = xmlTypeMapping.ElementName;
			einfo.MappedType = xmlTypeMapping;
			einfo.IsNullable = false;
			AddSchemaElement (schema.Items, schema, einfo, false);
			CompileSchemas ();
		}

		void ExportClassSchema (XmlTypeMapping map)
		{
			if (IsMapExported (map)) return;
			SetMapExported (map);

			XmlSchema schema = GetSchema (map.Namespace);
			XmlSchemaComplexType stype = new XmlSchemaComplexType ();
			stype.Name = map.ElementName;
			schema.Items.Add (stype);
			if (map.BaseMap != null)
			{
				XmlSchemaComplexContent cstype = new XmlSchemaComplexContent ();
				cstype.IsMixed = true;
				XmlSchemaComplexContentExtension ext = new XmlSchemaComplexContentExtension ();
				ext.BaseTypeName = new XmlQualifiedName (map.BaseMap.XmlType, map.BaseMap.Namespace);
				cstype.Content = ext;
				stype.ContentModel = cstype;

				XmlSchemaSequence particle;
				XmlSchemaAnyAttribute anyAttribute;
				ExportMembersMapSchema (schema, (ClassMap)map.ObjectMap, map.BaseMap, ext.Attributes, out particle, out anyAttribute);
				ext.Particle = particle;
				ext.AnyAttribute = anyAttribute;

				ImportNamespace (schema, map.BaseMap.Namespace);
				ExportClassSchema (map.BaseMap);
			}
			else
			{
				XmlSchemaSequence particle;
				XmlSchemaAnyAttribute anyAttribute;
				ExportMembersMapSchema (schema, (ClassMap)map.ObjectMap, map.BaseMap, stype.Attributes, out particle, out anyAttribute);
				stype.Particle = particle;
				stype.AnyAttribute = anyAttribute;
				stype.IsMixed = true;
			}
		}

		void ExportMembersMapSchema (XmlSchema schema, ClassMap map, XmlTypeMapping baseMap, XmlSchemaObjectCollection outAttributes, out XmlSchemaSequence particle, out XmlSchemaAnyAttribute anyAttribute)
		{
			particle = null;
			XmlSchemaSequence seq = new XmlSchemaSequence ();

			ICollection members = map.ElementMembers;
			if (members != null)
			{
				foreach (XmlTypeMapMemberElement member in members)
				{
					if (baseMap != null && DefinedInBaseMap (baseMap, member)) continue;

					Type memType = member.GetType();
					if (memType == typeof(XmlTypeMapMemberFlatList))
					{
						AddSchemaArrayElement (seq.Items, schema, member.ElementInfo);
					}
					else if (memType == typeof(XmlTypeMapMemberAnyElement))
					{
						AddSchemaArrayElement (seq.Items, schema, member.ElementInfo);
					}
					else if (memType == typeof(XmlTypeMapMemberAnyAttribute))
					{
						// Ignore
					}
					else if (memType == typeof(XmlTypeMapMemberElement))
					{
						XmlSchemaElement selem = (XmlSchemaElement) AddSchemaElement (seq.Items, schema, (XmlTypeMapElementInfo) member.ElementInfo [0], true);
						if (selem != null && member.DefaultValue != System.DBNull.Value)
							selem.DefaultValue = XmlCustomFormatter.ToXmlString (member.TypeData, member.DefaultValue);
					}
					else
					{
						AddSchemaElement (seq.Items, schema, (XmlTypeMapElementInfo) member.ElementInfo [0], true);
					}
				}
			}

			if (seq.Items.Count > 0)
				particle = seq;

			// Write attributes

			ICollection attributes = map.AttributeMembers;
			if (attributes != null)
			{
				foreach (XmlTypeMapMemberAttribute attr in attributes) 
					outAttributes.Add (GetSchemaAttribute (schema, attr));
			}

			XmlTypeMapMember anyAttrMember = map.DefaultAnyAttributeMember;
			if (anyAttrMember != null)
				anyAttribute = new XmlSchemaAnyAttribute ();
			else
				anyAttribute = null;
		}

		XmlSchemaAttribute GetSchemaAttribute (XmlSchema currentSchema, XmlTypeMapMemberAttribute attinfo)
		{
			XmlSchemaAttribute sat = new XmlSchemaAttribute ();
			if (attinfo.DefaultValue != System.DBNull.Value) sat.DefaultValue = XmlCustomFormatter.ToXmlString (attinfo.TypeData, attinfo.DefaultValue);

			ImportNamespace (currentSchema, attinfo.Namespace);

			XmlSchema memberSchema = GetSchema (attinfo.Namespace);
			if (currentSchema == memberSchema)
			{
				sat.Name = attinfo.AttributeName;
				if (attinfo.TypeData.SchemaType == SchemaTypes.Enum)
				{
					ExportEnumSchema (attinfo.MappedType);
					sat.SchemaTypeName = new XmlQualifiedName (attinfo.TypeData.XmlType, attinfo.DataTypeNamespace);;
				}
				else if (attinfo.TypeData.SchemaType == SchemaTypes.Array && TypeTranslator.IsPrimitive (attinfo.TypeData.ListItemType))
				{
					sat.SchemaType = GetSchemaSimpleListType (attinfo.TypeData);
				}
				else
					sat.SchemaTypeName = new XmlQualifiedName (attinfo.TypeData.XmlType, attinfo.DataTypeNamespace);;
			}
			else
			{
				sat.RefName = new XmlQualifiedName (attinfo.AttributeName, attinfo.Namespace);
				memberSchema.Items.Add (GetSchemaAttribute (memberSchema, attinfo));
			}
			return sat;
		}

		XmlSchemaParticle AddSchemaElement (XmlSchemaObjectCollection destcol, XmlSchema currentSchema, XmlTypeMapElementInfo einfo, bool isTypeMember)
		{
			if (einfo.IsTextElement) return null;

			if (einfo.IsUnnamedAnyElement)
			{
				XmlSchemaAny any = new XmlSchemaAny ();
				any.MinOccurs = 0;
				any.MaxOccurs = 1;
				destcol.Add (any);
				return any;
			}
			
			XmlSchemaElement selem = new XmlSchemaElement ();
			destcol.Add (selem);

			if (isTypeMember)
			{
				selem.MaxOccurs = 1;
				selem.MinOccurs = einfo.IsNullable ? 1 : 0;
				if (einfo.TypeData.Type.IsPrimitive && einfo.TypeData.Type != typeof(string) ||
					einfo.TypeData.Type.IsEnum) 
					selem.MinOccurs = 1;
			}

			XmlSchema memberSchema = GetSchema (einfo.Namespace);
			ImportNamespace (currentSchema, einfo.Namespace);

			if (currentSchema == memberSchema)
			{
				if (isTypeMember) selem.IsNillable = einfo.IsNullable;
				selem.Name = einfo.ElementName;
				XmlQualifiedName typeName = new XmlQualifiedName (einfo.TypeData.XmlType, einfo.DataTypeNamespace);
				switch (einfo.TypeData.SchemaType)
				{
					case SchemaTypes.XmlNode: 
						selem.SchemaType = GetSchemaXmlNodeType ();
						break;

					case SchemaTypes.XmlSerializable:
						selem.SchemaType = GetSchemaXmlSerializableType ();
						break;

					case SchemaTypes.Enum:
						selem.SchemaTypeName = new XmlQualifiedName (einfo.MappedType.XmlType, einfo.MappedType.Namespace);
						ImportNamespace (currentSchema, einfo.MappedType.Namespace);
						ExportEnumSchema (einfo.MappedType);
						break;

					case SchemaTypes.Array: 
						selem.SchemaTypeName = new XmlQualifiedName (einfo.MappedType.XmlType, einfo.MappedType.Namespace);;
						ImportNamespace (currentSchema, einfo.MappedType.Namespace);
						ExportArraySchema (einfo.MappedType); 
						break;

					case SchemaTypes.Class:
						if (einfo.MappedType.TypeData.Type != typeof(object)) {
							selem.SchemaTypeName = new XmlQualifiedName (einfo.MappedType.XmlType, einfo.MappedType.Namespace);;
							ImportNamespace (currentSchema, einfo.MappedType.Namespace);
							ExportClassSchema (einfo.MappedType);
						}
						break;

					case SchemaTypes.Primitive:
						selem.SchemaTypeName = new XmlQualifiedName (einfo.TypeData.XmlType, einfo.DataTypeNamespace);;
						break;
				}
			}
			else
			{
				selem.RefName = new XmlQualifiedName (einfo.ElementName, einfo.Namespace);
				AddSchemaElement (memberSchema.Items, memberSchema, einfo, false);
			}
			return selem;
		}

		void ImportNamespace (XmlSchema schema, string ns)
		{
			if (ns == "" || ns == schema.TargetNamespace) return;

			foreach (XmlSchemaObject sob in schema.Includes)
				if ((sob is XmlSchemaImport) && ((XmlSchemaImport)sob).Namespace == ns) return;

			XmlSchemaImport imp = new XmlSchemaImport ();
			imp.Namespace = ns;
			schema.Includes.Add (imp);
		}

		bool DefinedInBaseMap (XmlTypeMapping map, XmlTypeMapMemberElement member)
		{
			if (((ClassMap)map.ObjectMap).FindMember (member.Name) != null)
				return true;
			else if (map.BaseMap != null)
				return DefinedInBaseMap (map.BaseMap, member);
			else
				return false;
		}

		XmlSchemaType GetSchemaXmlNodeType ()
		{
			XmlSchemaComplexType stype = new XmlSchemaComplexType ();
			stype.IsMixed = true;
			XmlSchemaSequence seq = new XmlSchemaSequence ();
			seq.Items.Add (new XmlSchemaAny ());
			stype.Particle = seq;
			return stype;
		}

		XmlSchemaType GetSchemaXmlSerializableType ()
		{
			XmlSchemaComplexType stype = new XmlSchemaComplexType ();
			XmlSchemaSequence seq = new XmlSchemaSequence ();
			XmlSchemaElement selem = new XmlSchemaElement ();
			selem.RefName = new XmlQualifiedName ("schema",XmlSchema.Namespace);
			seq.Items.Add (selem);
			seq.Items.Add (new XmlSchemaAny ());
			stype.Particle = seq;
			return stype;
		}

		XmlSchemaSimpleType GetSchemaSimpleListType (TypeData typeData)
		{
			XmlSchemaSimpleType stype = new XmlSchemaSimpleType ();
			XmlSchemaSimpleTypeList list = new XmlSchemaSimpleTypeList ();
			TypeData itemTypeData = TypeTranslator.GetTypeData (typeData.ListItemType);
			list.ItemTypeName = new XmlQualifiedName (itemTypeData.XmlType, XmlSchema.Namespace);
			stype.Content = list;
			return stype;
		}

		XmlSchemaParticle AddSchemaArrayElement (XmlSchemaObjectCollection destcol, XmlSchema currentSchema, XmlTypeMapElementInfoList infos)
		{
			int numInfos = infos.Count;
			if (numInfos > 0 && ((XmlTypeMapElementInfo)infos[0]).IsTextElement) numInfos--;
			if (numInfos == 0) return null;

			if (numInfos == 1)
			{
				XmlSchemaParticle selem = AddSchemaElement (destcol, currentSchema, (XmlTypeMapElementInfo) infos[infos.Count-1], true);
				if (selem.MinOccursString == string.Empty) selem.MinOccursString = "0";
				if (selem.MaxOccursString == string.Empty) selem.MaxOccursString = "unbounded";
				return selem;
			}
			else
			{
				XmlSchemaChoice schoice = new XmlSchemaChoice ();
				destcol.Add (schoice);
				schoice.MinOccursString = "0";
				schoice.MaxOccursString = "unbounded";
				foreach (XmlTypeMapElementInfo einfo in infos)
				{
					if (einfo.IsTextElement) continue;
					AddSchemaElement (schoice.Items, currentSchema, einfo, true);
				}
				return schoice;
			}
		}

		void ExportEnumSchema (XmlTypeMapping map)
		{
			if (IsMapExported (map)) return;
			SetMapExported (map);

			XmlSchema schema = GetSchema (map.Namespace);
			XmlSchemaSimpleType stype = new XmlSchemaSimpleType ();
			stype.Name = map.ElementName;
			schema.Items.Add (stype);

			XmlSchemaSimpleTypeRestriction rest = new XmlSchemaSimpleTypeRestriction ();
			rest.BaseTypeName = new XmlQualifiedName ("string",XmlSchema.Namespace);
			EnumMap emap = (EnumMap) map.ObjectMap;

			foreach (EnumMap.EnumMapMember emem in emap.Members)
			{
				XmlSchemaEnumerationFacet ef = new XmlSchemaEnumerationFacet ();
				ef.Value = emem.XmlName;
				rest.Facets.Add (ef);
			}
			stype.Content = rest;
		}

		void ExportArraySchema (XmlTypeMapping map)
		{
			if (IsMapExported (map)) return;
			SetMapExported (map);

			XmlSchema schema = GetSchema (map.Namespace);
			XmlSchemaComplexType stype = new XmlSchemaComplexType ();
			stype.IsMixed = true;
			stype.Name = map.ElementName;
			schema.Items.Add (stype);

			ListMap lmap = (ListMap) map.ObjectMap;
			XmlSchemaSequence seq = new XmlSchemaSequence ();
			XmlSchemaParticle spart = AddSchemaArrayElement (seq.Items, schema, lmap.ItemInfo);
			if (spart is XmlSchemaChoice)
				stype.Particle = spart;
			else
				stype.Particle = seq;
		}

		bool IsMapExported (XmlTypeMapping map)
		{
			if (exportedMaps.Contains (map)) return true;
			if (map.TypeData.Type == typeof(object)) return true;
			return false;
		}

		void SetMapExported (XmlTypeMapping map)
		{
			exportedMaps.Add (map,map);
		}

		void CompileSchemas ()
		{
			foreach (XmlSchema sc in schemas)
				sc.Compile (null);
		}

		XmlSchema GetSchema (string ns)
		{
			XmlSchema schema = schemas [ns];
			if (schema == null)
			{
				schema = new XmlSchema ();
				schema.TargetNamespace = ns;
				schema.ElementFormDefault = XmlSchemaForm.Qualified;
				schemas.Add (schema);
			}
			return schema;
		}

		#endregion // Methods
	}
}
