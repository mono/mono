// 
// System.Xml.Serialization.XmlCodeExporter 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.CodeDom;
using System.Collections;
using System.Xml.Schema;

namespace System.Xml.Serialization {
	public class XmlCodeExporter {

		#region Fields

		CodeNamespace codeNamespace;
		CodeCompileUnit codeCompileUnit;
		CodeAttributeDeclarationCollection includeMetadata;

		Hashtable exportedMaps = new Hashtable ();

		#endregion

		#region Constructors

		public XmlCodeExporter (CodeNamespace codeNamespace)
		{

			includeMetadata = new CodeAttributeDeclarationCollection ();
			this.codeNamespace = codeNamespace;
		}

		public XmlCodeExporter (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit)
			: this (codeNamespace)
		{
			this.codeCompileUnit = codeCompileUnit;
		}

		#endregion // Constructors

		#region Properties

		public CodeAttributeDeclarationCollection IncludeMetadata {
			get { return includeMetadata; }
		}

		#endregion Properties

		#region Methods

		[MonoTODO]
		public void AddMappingMetadata (CodeAttributeDeclarationCollection metadata, XmlMemberMapping member, string ns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AddMappingMetadata (CodeAttributeDeclarationCollection metadata, XmlTypeMapping member, string ns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AddMappingMetadata (CodeAttributeDeclarationCollection metadata, XmlMemberMapping member, string ns, bool forceUseMemberName)
		{
			throw new NotImplementedException ();
		}

		public void ExportMembersMapping (XmlMembersMapping xmlMembersMapping)
		{
			CodeTypeDeclaration dummyClass = new CodeTypeDeclaration ();
			ExportMembersMapCode (dummyClass, (ClassMap)xmlMembersMapping.ObjectMap, xmlMembersMapping.Namespace, null);
		}

		public void ExportTypeMapping (XmlTypeMapping xmlTypeMapping)
		{
			ExportMapCode (xmlTypeMapping, true);
		}

		void ExportMapCode (XmlTypeMapping map, bool isRoot)
		{
			switch (map.TypeData.SchemaType)
			{
				case SchemaTypes.Enum:
					ExportEnumCode (map);
					break;

				case SchemaTypes.Array:
					ExportArrayCode (map);
					break;

				case SchemaTypes.Class:
					ExportClassCode (map, isRoot);
					break;

				case SchemaTypes.XmlSerializable:
				case SchemaTypes.XmlNode:
				case SchemaTypes.Primitive:
					// Ignore
					break;
			}
		}

		void ExportClassCode (XmlTypeMapping map, bool isRoot)
		{
			if (IsMapExported (map)) return;
			SetMapExported (map);

			CodeTypeDeclaration codeClass = new CodeTypeDeclaration (map.TypeData.TypeName);
			AddCodeType (codeClass, map.Documentation);
			codeClass.Attributes = MemberAttributes.Public;

			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlType");
			if (map.XmlType != map.TypeData.TypeName) att.Arguments.Add (GetArg (map.XmlType));
			if (map.XmlTypeNamespace != "") att.Arguments.Add (GetArg ("Namespace", map.XmlTypeNamespace));
			AddCustomAttribute (codeClass, att, false);

			if (map.ElementName != map.XmlType || map.Namespace != map.XmlTypeNamespace) {
				CodeAttributeDeclaration ratt = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlRoot");
				ratt.Arguments.Add (GetArg (map.ElementName));
				ratt.Arguments.Add (GetArg ("Namespace", map.Namespace));
				AddCustomAttribute (codeClass, ratt, false);
			}

			ExportMembersMapCode (codeClass, (ClassMap)map.ObjectMap, map.Namespace, map.BaseMap);

			if (map.BaseMap != null)
			{
				CodeTypeReference ctr = new CodeTypeReference (map.BaseMap.TypeData.FullTypeName);
				codeClass.BaseTypes.Add (ctr);
				ExportMapCode (map.BaseMap, false);
			}

			foreach (XmlTypeMapping tm in map.DerivedTypes)
			{
				CodeAttributeDeclaration iatt = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlInclude");
				iatt.Arguments.Add (new CodeAttributeArgument (new CodeTypeOfExpression(tm.TypeData.FullTypeName)));
				AddCustomAttribute (codeClass, iatt, true);
				ExportMapCode (tm, false);
			}
		}

		void ExportMembersMapCode (CodeTypeDeclaration codeClass, ClassMap map, string defaultNamespace, XmlTypeMapping baseMap)
		{
			ICollection members = map.ElementMembers;
				
			if (members != null)
			{
				foreach (XmlTypeMapMemberElement member in members)
				{
					if (baseMap != null && DefinedInBaseMap (baseMap, member)) continue;

					Type memType = member.GetType();
					if (memType == typeof(XmlTypeMapMemberList))
					{
						AddArrayElementFieldMember (codeClass, (XmlTypeMapMemberList) member, defaultNamespace);
					}
					else if (memType == typeof(XmlTypeMapMemberFlatList))
					{
						AddElementFieldMember (codeClass, member, defaultNamespace);
					}
					else if (memType == typeof(XmlTypeMapMemberAnyElement))
					{
						AddAnyElementFieldMember (codeClass, member, defaultNamespace);
					}
					else if (memType == typeof(XmlTypeMapMemberElement))
					{
						AddElementFieldMember (codeClass, member, defaultNamespace);
					}
					else
					{
						throw new InvalidOperationException ("Member type " + memType + " not supported");
					}
				}
			}

			// Write attributes

			ICollection attributes = map.AttributeMembers;
			if (attributes != null)
			{
				foreach (XmlTypeMapMemberAttribute attr in attributes) {
					if (baseMap != null && DefinedInBaseMap (baseMap, attr)) continue;
					AddAttributeFieldMember (codeClass, attr, defaultNamespace);
				}
			}

			XmlTypeMapMember anyAttrMember = map.DefaultAnyAttributeMember;
			if (anyAttrMember != null)
			{
				CodeMemberField codeField = new CodeMemberField (anyAttrMember.TypeData.FullTypeName, anyAttrMember.Name);
				AddComments (codeField, anyAttrMember.Documentation);
				codeField.Attributes = MemberAttributes.Public;
				AddCustomAttribute (codeField, "System.Xml.Serialization.XmlAnyAttribute");
				codeClass.Members.Add (codeField);
			}
		}

		CodeMemberField CreateFieldMember (string type, string name, object defaultValue, string comments)
		{
			CodeMemberField codeField = new CodeMemberField (type, name);
			codeField.Attributes = MemberAttributes.Public;
			AddComments (codeField, comments);

			if (defaultValue != System.DBNull.Value)
			{
				AddCustomAttribute (codeField, "System.ComponentModel.DefaultValue", GetArg (defaultValue));
				codeField.InitExpression = new CodePrimitiveExpression (defaultValue);
			}
			return codeField;
		}

		void AddAttributeFieldMember (CodeTypeDeclaration codeClass, XmlTypeMapMemberAttribute attinfo, string defaultNamespace)
		{
			CodeMemberField codeField = CreateFieldMember (attinfo.TypeData.FullTypeName, attinfo.Name, attinfo.DefaultValue, attinfo.Documentation);
			codeClass.Members.Add (codeField);

			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlAttribute");
			if (attinfo.Name != attinfo.AttributeName) att.Arguments.Add (GetArg (attinfo.AttributeName));
			if (attinfo.Namespace != defaultNamespace) att.Arguments.Add (GetArg ("Namespace", attinfo.Namespace));
			if (attinfo.Form != XmlSchemaForm.None) att.Arguments.Add (GetArg ("Form",attinfo.Form));
			if (!TypeTranslator.IsDefaultPrimitiveTpeData(attinfo.TypeData)) att.Arguments.Add (GetArg ("DataType",attinfo.TypeData.XmlType));
			AddCustomAttribute (codeField, att, true);

			if (attinfo.MappedType != null)
				ExportMapCode (attinfo.MappedType, false);
		}

		void AddElementFieldMember (CodeTypeDeclaration codeClass, XmlTypeMapMemberElement member, string defaultNamespace)
		{
			CodeMemberField codeField = CreateFieldMember (member.TypeData.FullTypeName, member.Name, member.DefaultValue, member.Documentation);
			codeClass.Members.Add (codeField);
			TypeData defaultType = member.TypeData;
			bool addAlwaysAttr = false;
			
			if (member is XmlTypeMapMemberFlatList)
			{
				defaultType = defaultType.ListItemTypeData;
				addAlwaysAttr = true;
			}

			foreach (XmlTypeMapElementInfo einfo in member.ElementInfo)
			{
				if (ExportExtraElementAttributes (codeField, einfo, defaultNamespace, defaultType))
					continue;

				CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlElement");
				if (einfo.ElementName != member.Name) att.Arguments.Add (GetArg (einfo.ElementName));
				if (einfo.TypeData.FullTypeName != defaultType.FullTypeName) att.Arguments.Add (GetTypeArg ("Type", einfo.TypeData.FullTypeName));
				if (einfo.Namespace != defaultNamespace) att.Arguments.Add (GetArg ("Namespace", einfo.Namespace));
				if (einfo.IsNullable) att.Arguments.Add (GetArg ("IsNullable", true));
				if (!TypeTranslator.IsDefaultPrimitiveTpeData(einfo.TypeData)) att.Arguments.Add (GetArg ("DataType",einfo.TypeData.XmlType));
				AddCustomAttribute (codeField, att, addAlwaysAttr);

				if (einfo.MappedType != null) ExportMapCode (einfo.MappedType, false);
			}

			if (member.ChoiceMember != null)
				AddCustomAttribute (codeField, "System.Xml.Serialization.XmlChoiceIdentifier", GetArg(member.ChoiceMember));
		}

		void AddAnyElementFieldMember (CodeTypeDeclaration codeClass, XmlTypeMapMemberElement member, string defaultNamespace)
		{
			CodeMemberField codeField = CreateFieldMember (member.TypeData.FullTypeName, member.Name, member.DefaultValue, member.Documentation);
			codeClass.Members.Add (codeField);

			foreach (XmlTypeMapElementInfo einfo in member.ElementInfo)
			{
				ExportExtraElementAttributes (codeField, einfo, defaultNamespace, einfo.TypeData);
			}
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

		void AddArrayElementFieldMember (CodeTypeDeclaration codeClass, XmlTypeMapMemberList member, string defaultNamespace)
		{
			CodeMemberField codeField = new CodeMemberField (member.TypeData.FullTypeName, member.Name);
			AddComments (codeField, member.Documentation);
			codeField.Attributes = MemberAttributes.Public;
			codeClass.Members.Add (codeField);
			XmlTypeMapElementInfo einfo = (XmlTypeMapElementInfo) member.ElementInfo[0];

			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlArray");
			if (einfo.ElementName != member.Name) att.Arguments.Add (GetArg ("ElementName", einfo.ElementName));
			if (einfo.Namespace != defaultNamespace) att.Arguments.Add (GetArg ("Namespace", einfo.Namespace));
			if (einfo.IsNullable) att.Arguments.Add (GetArg ("IsNullable", true));
			AddCustomAttribute (codeField, att, false);

			ListMap listMap = (ListMap) member.ListTypeMapping.ObjectMap;

			AddArrayItemAttributes (codeField, listMap, member.TypeData.ListItemTypeData, defaultNamespace, 0);
		}

		void AddArrayItemAttributes (CodeMemberField codeField, ListMap listMap, TypeData type, string defaultNamespace, int nestingLevel)
		{
			foreach (XmlTypeMapElementInfo ainfo in listMap.ItemInfo)
			{
				string defaultName;
				if (ainfo.MappedType != null) defaultName = ainfo.MappedType.ElementName;
				else defaultName = ainfo.TypeData.XmlType;

				bool needsType = (listMap.ItemInfo.Count > 1) ||
								 (ainfo.TypeData.FullTypeName != type.FullTypeName && !listMap.IsMultiArray);

				CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlArrayItem");
				if (ainfo.ElementName != defaultName) att.Arguments.Add (GetArg ("ElementName", ainfo.ElementName));
				if (ainfo.Namespace != defaultNamespace && ainfo.Namespace != XmlSchema.Namespace) att.Arguments.Add (GetArg ("Namespace", ainfo.Namespace));
				if (needsType) att.Arguments.Add (GetTypeArg ("Type", ainfo.TypeData.FullTypeName));
				if (ainfo.IsNullable) att.Arguments.Add (GetArg ("IsNullable", true));
				if (att.Arguments.Count > 0 && nestingLevel > 0) att.Arguments.Add (GetArg ("NestingLevel", nestingLevel));
				AddCustomAttribute (codeField, att, false);

				if (ainfo.MappedType != null) ExportMapCode (ainfo.MappedType, false);
			}

			if (listMap.IsMultiArray)
			{
				XmlTypeMapping nmap = listMap.NestedArrayMapping;
				AddArrayItemAttributes (codeField, (ListMap) nmap.ObjectMap, nmap.TypeData.ListItemTypeData, defaultNamespace, nestingLevel + 1);
			}
		}

		void ExportArrayCode (XmlTypeMapping map)
		{
			ListMap listMap = (ListMap) map.ObjectMap;
			foreach (XmlTypeMapElementInfo ainfo in listMap.ItemInfo)
			{
				if (ainfo.MappedType != null) ExportMapCode (ainfo.MappedType, false);
			}
		}

		bool ExportExtraElementAttributes (CodeMemberField codeField, XmlTypeMapElementInfo einfo, string defaultNamespace, TypeData defaultType)
		{
			if (einfo.IsTextElement) {
				CodeAttributeDeclaration uatt = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlText");
				if (einfo.TypeData.FullTypeName != defaultType.FullTypeName) uatt.Arguments.Add (GetTypeArg ("Type", einfo.TypeData.FullTypeName));
				AddCustomAttribute (codeField, uatt, true);
				return true;
			}
			else if (einfo.IsUnnamedAnyElement) {
				CodeAttributeDeclaration uatt = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlAnyElement");
				if (!einfo.IsUnnamedAnyElement) uatt.Arguments.Add (GetArg ("Name", einfo.ElementName));
				if (einfo.Namespace != defaultNamespace) uatt.Arguments.Add (GetArg ("Namespace", einfo.Namespace));
				AddCustomAttribute (codeField, uatt, true);
				return true;
			}
			return false;
		}

		void ExportEnumCode (XmlTypeMapping map)
		{
			if (IsMapExported (map)) return;
			SetMapExported (map);

			CodeTypeDeclaration codeEnum = new CodeTypeDeclaration (map.TypeData.TypeName);
			codeEnum.Attributes = MemberAttributes.Public;
			codeEnum.IsEnum = true;
			AddCodeType (codeEnum, map.Documentation);

			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlTypeAttribute");
			if (map.ElementName != map.TypeData.TypeName) att.Arguments.Add (GetArg ("Name", map.ElementName));
			if (map.Namespace != "") att.Arguments.Add (GetArg ("Namespace", map.Namespace));
			AddCustomAttribute (codeEnum, att, false);

			EnumMap emap = (EnumMap) map.ObjectMap;

			foreach (EnumMap.EnumMapMember emem in emap.Members)
			{
				CodeMemberField codeField = new CodeMemberField ("", emem.EnumName);
				AddComments (codeField, emem.Documentation);

				if (emem.EnumName != emem.XmlName)
				{
					CodeAttributeDeclaration xatt = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlEnum");
					xatt.Arguments.Add (GetArg ("Name", emem.XmlName));

					AddCustomAttribute (codeField, xatt, true);
				}
				codeEnum.Members.Add (codeField);
			}
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

		void AddCustomAttribute (CodeTypeMember ctm, CodeAttributeDeclaration att, bool addIfNoParams)
		{
			if (att.Arguments.Count == 0 && !addIfNoParams) return;
			
			if (ctm.CustomAttributes == null) ctm.CustomAttributes = new CodeAttributeDeclarationCollection ();
			ctm.CustomAttributes.Add (att);
		}

		void AddCustomAttribute (CodeTypeMember ctm, string name, params CodeAttributeArgument[] args)
		{
			if (ctm.CustomAttributes == null) ctm.CustomAttributes = new CodeAttributeDeclarationCollection ();
			ctm.CustomAttributes.Add (new CodeAttributeDeclaration (name, args));
		}

		CodeAttributeArgument GetArg (string name, object value)
		{
			return new CodeAttributeArgument (name, new CodePrimitiveExpression(value));
		}

		CodeAttributeArgument GetArg (object value)
		{
			return new CodeAttributeArgument (new CodePrimitiveExpression(value));
		}

		CodeAttributeArgument GetTypeArg (string name, string typeName)
		{
			return new CodeAttributeArgument (name, new CodeTypeOfExpression(typeName));
		}

		void AddComments (CodeTypeMember member, string comments)
		{
			if (comments == null || comments == "") member.Comments.Add (new CodeCommentStatement ("<remarks/>", true));
			else member.Comments.Add (new CodeCommentStatement ("<remarks>\n" + comments + "\n</remarks>", true));
		}

		void AddCodeType (CodeTypeDeclaration type, string comments)
		{
			AddComments (type, comments);
			codeNamespace.Types.Add (type);
		}

		#endregion // Methods
	}
}
