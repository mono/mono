// 
// System.Xml.Serialization.XmlSchemaExporter 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
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
		Hashtable exportedMaps = new Hashtable();
		Hashtable exportedElements = new Hashtable();
		bool encodedFormat = false;
		XmlDocument xmlDoc;
		
		#endregion

		#region Constructors

		public XmlSchemaExporter (XmlSchemas schemas)
		{
			this.schemas = schemas;
		}

		internal XmlSchemaExporter (XmlSchemas schemas, bool encodedFormat)
		{
			this.encodedFormat = encodedFormat;
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
			ExportMembersMapping (xmlMembersMapping, true);
		}
		
		internal void ExportMembersMapping (XmlMembersMapping xmlMembersMapping, bool exportEnclosingType)
		{
			XmlSchema schema = GetSchema (xmlMembersMapping.Namespace);
			ClassMap cmap = (ClassMap) xmlMembersMapping.ObjectMap;

			if (xmlMembersMapping.HasWrapperElement && exportEnclosingType)
			{
				XmlSchemaComplexType stype = new XmlSchemaComplexType ();
	
				XmlSchemaSequence particle;
				XmlSchemaAnyAttribute anyAttribute;
				ExportMembersMapSchema (schema, cmap, null, stype.Attributes, out particle, out anyAttribute);
				stype.Particle = particle;
				stype.AnyAttribute = anyAttribute;
				
				if (encodedFormat)
				{
					stype.Name = xmlMembersMapping.ElementName;
					schema.Items.Add (stype);
				}
				else
				{
					XmlSchemaElement selem = new XmlSchemaElement ();
					selem.Name = xmlMembersMapping.ElementName;
					selem.SchemaType = stype;
					schema.Items.Add (selem);
				}
			}
			else
			{
				ICollection members = cmap.ElementMembers;
				if (members != null)
				{
					XmlSchemaObjectCollection itemsCol = schema.Items;
					
					// In encoded format, the schema elements are not needed
					if (encodedFormat) itemsCol = new XmlSchemaObjectCollection ();
					
					foreach (XmlTypeMapMemberElement member in members)
					{
						XmlSchemaElement exe = FindElement (itemsCol, ((XmlTypeMapElementInfo)member.ElementInfo [0]).ElementName);
						XmlSchemaElement elem;
						
						Type memType = member.GetType();
						if (member is XmlTypeMapMemberFlatList)
							throw new InvalidOperationException ("Unwrapped arrays not supported as parameters");
						else if (memType == typeof(XmlTypeMapMemberElement))
							elem = (XmlSchemaElement) AddSchemaElement (itemsCol, schema, (XmlTypeMapElementInfo) member.ElementInfo [0], member.DefaultValue, false);
						else
							elem = (XmlSchemaElement) AddSchemaElement (itemsCol, schema, (XmlTypeMapElementInfo) member.ElementInfo [0], false);
							
						if (exe != null)
						{
							if (exe.SchemaTypeName.Equals (elem.SchemaTypeName))
								itemsCol.Remove (elem);
							else
							{
								string s = "The XML element named '" + ((XmlTypeMapElementInfo)member.ElementInfo [0]).ElementName + "' ";
								s += "from namespace '" + schema.TargetNamespace + "' references distinct types " + elem.SchemaTypeName.Name + " and " + exe.SchemaTypeName.Name + ". ";
								s += "Use XML attributes to specify another XML name or namespace for the element or types.";
								throw new InvalidOperationException (s);
							}
						}
					}
				}
			}
			
			if (encodedFormat) 
				ImportNamespace (schema, XmlSerializer.EncodingNamespace);
				
			CompileSchemas ();
		}

		[MonoTODO]
		public XmlQualifiedName ExportTypeMapping (XmlMembersMapping xmlMembersMapping)
		{
			throw new NotImplementedException ();
		}

		public void ExportTypeMapping (XmlTypeMapping xmlTypeMapping)
		{
			if (!xmlTypeMapping.IncludeInSchema) return;
			if (IsElementExported (xmlTypeMapping)) return;
			
			if (encodedFormat)
			{
				ExportClassSchema (xmlTypeMapping);
				XmlSchema schema = GetSchema (xmlTypeMapping.XmlTypeNamespace);
				ImportNamespace (schema, XmlSerializer.EncodingNamespace);
			}
			else
			{
				XmlSchema schema = GetSchema (xmlTypeMapping.Namespace);
				XmlTypeMapElementInfo einfo = new XmlTypeMapElementInfo (null, xmlTypeMapping.TypeData);
				einfo.Namespace = xmlTypeMapping.Namespace;
				einfo.ElementName = xmlTypeMapping.ElementName;
				if (xmlTypeMapping.TypeData.IsComplexType)
					einfo.MappedType = xmlTypeMapping;
				einfo.IsNullable = false;
				AddSchemaElement (schema.Items, schema, einfo, false);
				SetElementExported (xmlTypeMapping);
			}
			
			CompileSchemas ();
		}

		void ExportClassSchema (XmlTypeMapping map)
		{
			if (IsMapExported (map)) return;
			SetMapExported (map);

			XmlSchema schema = GetSchema (map.XmlTypeNamespace);
			XmlSchemaComplexType stype = new XmlSchemaComplexType ();
			stype.Name = map.XmlType;
			schema.Items.Add (stype);

			ClassMap cmap = (ClassMap)map.ObjectMap;

			if (cmap.HasSimpleContent)
			{
				XmlSchemaSimpleContent simple = new XmlSchemaSimpleContent ();
				stype.ContentModel = simple;
				XmlSchemaSimpleContentExtension ext = new XmlSchemaSimpleContentExtension ();
				simple.Content = ext;
				XmlSchemaSequence particle;
				XmlSchemaAnyAttribute anyAttribute;
				ExportMembersMapSchema (schema, cmap, map.BaseMap, ext.Attributes, out particle, out anyAttribute);
				ext.AnyAttribute = anyAttribute;
				if (map.BaseMap == null)
					ext.BaseTypeName = cmap.SimpleContentBaseType;
				else
					ext.BaseTypeName = new XmlQualifiedName (map.BaseMap.XmlType, map.BaseMap.XmlTypeNamespace);
			}
			else if (map.BaseMap != null && map.BaseMap.IncludeInSchema)
			{
				XmlSchemaComplexContent cstype = new XmlSchemaComplexContent ();
				XmlSchemaComplexContentExtension ext = new XmlSchemaComplexContentExtension ();
				ext.BaseTypeName = new XmlQualifiedName (map.BaseMap.XmlType, map.BaseMap.XmlTypeNamespace);
				cstype.Content = ext;
				stype.ContentModel = cstype;

				XmlSchemaSequence particle;
				XmlSchemaAnyAttribute anyAttribute;
				ExportMembersMapSchema (schema, cmap, map.BaseMap, ext.Attributes, out particle, out anyAttribute);
				ext.Particle = particle;
				ext.AnyAttribute = anyAttribute;

				ImportNamespace (schema, map.BaseMap.Namespace);
				ExportClassSchema (map.BaseMap);
			}
			else
			{
				XmlSchemaSequence particle;
				XmlSchemaAnyAttribute anyAttribute;
				ExportMembersMapSchema (schema, cmap, map.BaseMap, stype.Attributes, out particle, out anyAttribute);
				stype.Particle = particle;
				stype.AnyAttribute = anyAttribute;
				stype.IsMixed = cmap.XmlTextCollector != null;
			}
			
			foreach (XmlTypeMapping dmap in map.DerivedTypes)
				if (dmap.TypeData.SchemaType == SchemaTypes.Class) ExportClassSchema (dmap);
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
					else if (memType == typeof(XmlTypeMapMemberElement))
					{
						XmlSchemaElement selem = (XmlSchemaElement) AddSchemaElement (seq.Items, schema, (XmlTypeMapElementInfo) member.ElementInfo [0], member.DefaultValue, true);
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
				foreach (XmlTypeMapMemberAttribute attr in attributes) {
					if (baseMap != null && DefinedInBaseMap (baseMap, attr)) continue;
					outAttributes.Add (GetSchemaAttribute (schema, attr));
				}
			}

			XmlTypeMapMember anyAttrMember = map.DefaultAnyAttributeMember;
			if (anyAttrMember != null)
				anyAttribute = new XmlSchemaAnyAttribute ();
			else
				anyAttribute = null;
		}
		
		XmlSchemaElement FindElement (XmlSchemaObjectCollection col, string name)
		{
			foreach (XmlSchemaObject ob in col)
			{
				XmlSchemaElement elem = ob as XmlSchemaElement;
				if (elem != null && elem.Name == name) return elem;
			}
			return null;
		}

		XmlSchemaAttribute GetSchemaAttribute (XmlSchema currentSchema, XmlTypeMapMemberAttribute attinfo)
		{
			XmlSchemaAttribute sat = new XmlSchemaAttribute ();
			if (attinfo.DefaultValue != System.DBNull.Value) sat.DefaultValue = XmlCustomFormatter.ToXmlString (attinfo.TypeData, attinfo.DefaultValue);

			ImportNamespace (currentSchema, attinfo.Namespace);

			XmlSchema memberSchema = GetSchema (attinfo.Namespace);
			if (currentSchema == memberSchema || encodedFormat)
			{
				sat.Name = attinfo.AttributeName;
				if (attinfo.TypeData.SchemaType == SchemaTypes.Enum)
				{
					ImportNamespace (currentSchema, attinfo.DataTypeNamespace);
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
			return AddSchemaElement (destcol, currentSchema, einfo, System.DBNull.Value, isTypeMember);
		}
		
		XmlSchemaParticle AddSchemaElement (XmlSchemaObjectCollection destcol, XmlSchema currentSchema, XmlTypeMapElementInfo einfo, object defaultValue, bool isTypeMember)
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
				
				if ((einfo.TypeData.Type.IsPrimitive && einfo.TypeData.Type != typeof(string)) ||
					einfo.TypeData.Type.IsEnum || encodedFormat) 
					selem.MinOccurs = 1;
			}

			XmlSchema memberSchema = null;
			
			if (!encodedFormat)
			{
				memberSchema = GetSchema (einfo.Namespace);
				ImportNamespace (currentSchema, einfo.Namespace);
			}		

			if (currentSchema == memberSchema || encodedFormat)
			{
				if (isTypeMember) selem.IsNillable = einfo.IsNullable;
				selem.Name = einfo.ElementName;
				XmlQualifiedName typeName = new XmlQualifiedName (einfo.TypeData.XmlType, einfo.DataTypeNamespace);

				if (defaultValue != System.DBNull.Value)
					selem.DefaultValue = XmlCustomFormatter.ToXmlString (einfo.TypeData, defaultValue);

				switch (einfo.TypeData.SchemaType)
				{
					case SchemaTypes.XmlNode: 
						selem.SchemaType = GetSchemaXmlNodeType ();
						break;

					case SchemaTypes.XmlSerializable:
						selem.SchemaType = GetSchemaXmlSerializableType ();
						break;

					case SchemaTypes.Enum:
						selem.SchemaTypeName = new XmlQualifiedName (einfo.MappedType.XmlType, einfo.MappedType.XmlTypeNamespace);
						ImportNamespace (currentSchema, einfo.MappedType.XmlTypeNamespace);
						ExportEnumSchema (einfo.MappedType);
						break;

					case SchemaTypes.Array: 
						XmlQualifiedName atypeName = ExportArraySchema (einfo.MappedType, currentSchema.TargetNamespace); 
						selem.SchemaTypeName = atypeName;
						ImportNamespace (currentSchema, atypeName.Namespace);
						break;

					case SchemaTypes.Class:
						if (einfo.MappedType.TypeData.Type != typeof(object)) {
							selem.SchemaTypeName = new XmlQualifiedName (einfo.MappedType.XmlType, einfo.MappedType.XmlTypeNamespace);
							ImportNamespace (currentSchema, einfo.MappedType.XmlTypeNamespace);
							ExportClassSchema (einfo.MappedType);
						}
						else if (encodedFormat)
							selem.SchemaTypeName = new XmlQualifiedName (einfo.MappedType.XmlType, einfo.MappedType.XmlTypeNamespace);
						break;

					case SchemaTypes.Primitive:
						selem.SchemaTypeName = new XmlQualifiedName (einfo.TypeData.XmlType, einfo.DataTypeNamespace);;
						break;
				}
			}
			else
			{
				selem.RefName = new XmlQualifiedName (einfo.ElementName, einfo.Namespace);
				AddSchemaElement (memberSchema.Items, memberSchema, einfo, defaultValue, false);
			}
			return selem;
		}

		void ImportNamespace (XmlSchema schema, string ns)
		{
			if (ns == "" || ns == schema.TargetNamespace || ns == XmlSchema.Namespace) return;

			foreach (XmlSchemaObject sob in schema.Includes)
				if ((sob is XmlSchemaImport) && ((XmlSchemaImport)sob).Namespace == ns) return;

			XmlSchemaImport imp = new XmlSchemaImport ();
			imp.Namespace = ns;
			schema.Includes.Add (imp);
		}

		bool DefinedInBaseMap (XmlTypeMapping map, XmlTypeMapMember member)
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
				selem.MinOccursString = "0";
				selem.MaxOccursString = "unbounded";
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

		XmlQualifiedName ExportArraySchema (XmlTypeMapping map, string defaultNamespace)
		{
			ListMap lmap = (ListMap) map.ObjectMap;

			if (encodedFormat)
			{
				string name, ns, schemaNs;
				lmap.GetArrayType (-1, out name, out ns);				
				if (ns == XmlSchema.Namespace) schemaNs = defaultNamespace;
				else schemaNs = ns;

				if (IsMapExported (map)) return new XmlQualifiedName (lmap.GetSchemaArrayName (), schemaNs);
				SetMapExported (map);

				XmlSchema schema = GetSchema (schemaNs);
				XmlSchemaComplexType stype = new XmlSchemaComplexType ();
				stype.Name = lmap.GetSchemaArrayName ();
				schema.Items.Add (stype);
				
				XmlSchemaComplexContent content = new XmlSchemaComplexContent();
				content.IsMixed = false;
				stype.ContentModel = content;
				
				XmlSchemaComplexContentRestriction rest = new XmlSchemaComplexContentRestriction ();
				content.Content = rest;
				rest.BaseTypeName = new XmlQualifiedName ("Array", XmlSerializer.EncodingNamespace);
				XmlSchemaAttribute at = new XmlSchemaAttribute ();
				rest.Attributes.Add (at);
				at.RefName = new XmlQualifiedName ("arrayType", XmlSerializer.EncodingNamespace);
				
				XmlAttribute arrayType = Document.CreateAttribute ("arrayType", XmlSerializer.WsdlNamespace);
				arrayType.Value = ns + (ns != "" ? ":" : "") + name;
				at.UnhandledAttributes = new XmlAttribute [] { arrayType };
				ImportNamespace (schema, XmlSerializer.WsdlNamespace);
			
				XmlTypeMapElementInfo einfo = (XmlTypeMapElementInfo) lmap.ItemInfo[0];
				if (einfo.MappedType != null)
				{
					switch (einfo.TypeData.SchemaType)
					{
						case SchemaTypes.Enum:
							ExportEnumSchema (einfo.MappedType);
							break;
						case SchemaTypes.Array: 
							ExportArraySchema (einfo.MappedType, schemaNs); 
							break;
						case SchemaTypes.Class:
							if (einfo.MappedType.TypeData.Type != typeof(object))
								ExportClassSchema (einfo.MappedType);
							break;
					}
				}
				
				return new XmlQualifiedName (lmap.GetSchemaArrayName (), schemaNs);
			}
			else
			{
				if (IsMapExported (map)) return new XmlQualifiedName (map.XmlType, map.XmlTypeNamespace);
				
				SetMapExported (map);
				XmlSchema schema = GetSchema (map.Namespace);
				XmlSchemaComplexType stype = new XmlSchemaComplexType ();
				stype.Name = map.ElementName;
				schema.Items.Add (stype);

				XmlSchemaSequence seq = new XmlSchemaSequence ();
				XmlSchemaParticle spart = AddSchemaArrayElement (seq.Items, schema, lmap.ItemInfo);
				if (spart is XmlSchemaChoice)
					stype.Particle = spart;
				else
					stype.Particle = seq;
					
				return new XmlQualifiedName (map.XmlType, map.XmlTypeNamespace);
			}
		}
		
		XmlDocument Document
		{
			get
			{
				if (xmlDoc == null) xmlDoc = new XmlDocument ();
				return xmlDoc;
			}
		}

		bool IsMapExported (XmlTypeMapping map)
		{
			if (exportedMaps.ContainsKey (GetMapKey(map))) return true;
			if (map.TypeData.Type == typeof(object)) return true;
			return false;
		}

		void SetMapExported (XmlTypeMapping map)
		{
			exportedMaps [GetMapKey(map)] = map;
		}

		bool IsElementExported (XmlTypeMapping map)
		{
			if (exportedElements.ContainsKey (GetMapKey(map))) return true;
			if (map.TypeData.Type == typeof(object)) return true;
			return false;
		}

		void SetElementExported (XmlTypeMapping map)
		{
			exportedElements [GetMapKey(map)] = map;
		}
		
		string GetMapKey (XmlTypeMapping map)
		{
			return map.TypeData.FullTypeName + " " + map.Namespace;
		}

		void CompileSchemas ()
		{
//			foreach (XmlSchema sc in schemas)
//				sc.Compile (null);
		}

		XmlSchema GetSchema (string ns)
		{
			XmlSchema schema = schemas [ns];
			if (schema == null)
			{
				schema = new XmlSchema ();
				schema.TargetNamespace = ns;
				if (!encodedFormat)
					schema.ElementFormDefault = XmlSchemaForm.Qualified;
				schemas.Add (schema);
			}
			return schema;
		}

		#endregion // Methods
	}
}
