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
		bool encodedFormat;
		XmlMapCodeGenerator codeGenerator;

		Hashtable exportedMaps = new Hashtable ();

		#endregion

		#region Constructors

		public XmlCodeExporter (CodeNamespace codeNamespace): this (codeNamespace, null)
		{
		}

		public XmlCodeExporter (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit)
		{
			includeMetadata = new CodeAttributeDeclarationCollection ();
			this.codeCompileUnit = codeCompileUnit;
			this.codeNamespace = codeNamespace;
			
			codeGenerator = new XmlMapCodeGenerator (codeNamespace, codeCompileUnit);
		}

		#endregion // Constructors

		#region Properties

		public CodeAttributeDeclarationCollection IncludeMetadata {
			get { return includeMetadata; }
		}

		#endregion Properties

		#region Methods

		public void AddMappingMetadata (CodeAttributeDeclarationCollection metadata, XmlMemberMapping member, string ns)
		{
			AddMappingMetadata (metadata, member, ns, false);
		}

		[MonoTODO]
		public void AddMappingMetadata (CodeAttributeDeclarationCollection metadata, XmlTypeMapping member, string ns)
		{
			throw new NotImplementedException ();
		}

		public void AddMappingMetadata (CodeAttributeDeclarationCollection metadata, XmlMemberMapping member, string ns, bool forceUseMemberName)
		{
			CodeAttributeDeclaration att;
			TypeData memType = member.TypeMapMember.TypeData;
			
			if (memType.SchemaType == SchemaTypes.Array)
			{
				// Array parameter
				att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlArray");
				if (forceUseMemberName || (member.ElementName != member.MemberName)) att.Arguments.Add (MapCodeGenerator.GetArg ("ElementName", member.ElementName));
				if (member.Namespace != ns) att.Arguments.Add (MapCodeGenerator.GetArg ("Namespace", member.Namespace));
				if (att.Arguments.Count > 0) metadata.Add (att);
			}
			else if (!member.Any)
			{
				att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlElement");
				if (forceUseMemberName || (member.ElementName != member.MemberName)) att.Arguments.Add (MapCodeGenerator.GetArg ("ElementName", member.ElementName));
				if (member.Namespace != ns) att.Arguments.Add (MapCodeGenerator.GetArg ("Namespace", member.Namespace));
				if (!TypeTranslator.IsDefaultPrimitiveTpeData (memType)) att.Arguments.Add (MapCodeGenerator.GetArg ("DataType", member.TypeName));
				if (att.Arguments.Count > 0) metadata.Add (att);
			}
		}

		public void ExportMembersMapping (XmlMembersMapping xmlMembersMapping)
		{
			codeGenerator.ExportMembersMapping (xmlMembersMapping);
		}

		public void ExportTypeMapping (XmlTypeMapping xmlTypeMapping)
		{
			codeGenerator.ExportTypeMapping (xmlTypeMapping);
		}

		#endregion // Methods
	}
	
	class XmlMapCodeGenerator : MapCodeGenerator
	{
		public XmlMapCodeGenerator (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit)
		: base (codeNamespace, codeCompileUnit)
		{
		}

		protected override void GenerateClass (XmlTypeMapping map, CodeTypeDeclaration codeClass)
		{
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlType");
			if (map.XmlType != map.TypeData.TypeName) att.Arguments.Add (GetArg (map.XmlType));
			if (map.XmlTypeNamespace != "") att.Arguments.Add (GetArg ("Namespace", map.XmlTypeNamespace));
			AddCustomAttribute (codeClass, att, false);

			CodeAttributeDeclaration ratt = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlRoot");
			if (map.ElementName != map.XmlType) ratt.Arguments.Add (GetArg (map.ElementName));
			if (map.Namespace != "") ratt.Arguments.Add (GetArg ("Namespace", map.Namespace));
			AddCustomAttribute (codeClass, ratt, false);
		}
		
		protected override void GenerateClassInclude (CodeTypeDeclaration codeClass, XmlTypeMapping map)
		{
			CodeAttributeDeclaration iatt = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlInclude");
			iatt.Arguments.Add (new CodeAttributeArgument (new CodeTypeOfExpression(map.TypeData.FullTypeName)));
			AddCustomAttribute (codeClass, iatt, true);
		}
		
		protected override void GenerateAnyAttribute (CodeMemberField codeField)
		{
			AddCustomAttribute (codeField, "System.Xml.Serialization.XmlAnyAttribute");
		}
		
		protected override void GenerateDefaultAttribute (CodeMemberField codeField, object defaultValue)
		{
			AddCustomAttribute (codeField, "System.ComponentModel.DefaultValue", GetArg (defaultValue));
			codeField.InitExpression = new CodePrimitiveExpression (defaultValue);
		}
		
		protected override void GenerateAttributeMember (CodeMemberField codeField, XmlTypeMapMemberAttribute attinfo, string defaultNamespace)
		{
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlAttribute");
			if (attinfo.Name != attinfo.AttributeName) att.Arguments.Add (GetArg (attinfo.AttributeName));
			if (attinfo.Namespace != defaultNamespace) att.Arguments.Add (GetArg ("Namespace", attinfo.Namespace));
			if (attinfo.Form != XmlSchemaForm.None) att.Arguments.Add (GetArg ("Form",attinfo.Form));
			if (!TypeTranslator.IsDefaultPrimitiveTpeData(attinfo.TypeData)) att.Arguments.Add (GetArg ("DataType",attinfo.TypeData.XmlType));
			AddCustomAttribute (codeField, att, true);
		}
		
		protected override void GenerateElementInfoMember (CodeMemberField codeField, XmlTypeMapMemberElement member, XmlTypeMapElementInfo einfo, TypeData defaultType, string defaultNamespace, bool addAlwaysAttr)
		{
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlElement");
			if (einfo.ElementName != member.Name) att.Arguments.Add (GetArg (einfo.ElementName));
			if (einfo.TypeData.FullTypeName != defaultType.FullTypeName) att.Arguments.Add (GetTypeArg ("Type", einfo.TypeData.FullTypeName));
			if (einfo.Namespace != defaultNamespace) att.Arguments.Add (GetArg ("Namespace", einfo.Namespace));
			if (einfo.IsNullable) att.Arguments.Add (GetArg ("IsNullable", true));
			if (!TypeTranslator.IsDefaultPrimitiveTpeData(einfo.TypeData)) att.Arguments.Add (GetArg ("DataType",einfo.TypeData.XmlType));
			AddCustomAttribute (codeField, att, addAlwaysAttr);
		}
		
		protected override void GenerateElementMember (CodeMemberField codeField, XmlTypeMapMemberElement member)
		{
			if (member.ChoiceMember != null)
				AddCustomAttribute (codeField, "System.Xml.Serialization.XmlChoiceIdentifier", GetArg(member.ChoiceMember));
		}
		
		protected override void GenerateArrayElement (CodeMemberField codeField, XmlTypeMapMemberElement member, string defaultNamespace)
		{
			XmlTypeMapElementInfo einfo = (XmlTypeMapElementInfo) member.ElementInfo[0];
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlArray");
			if (einfo.ElementName != member.Name) att.Arguments.Add (GetArg ("ElementName", einfo.ElementName));
			if (einfo.Namespace != defaultNamespace) att.Arguments.Add (GetArg ("Namespace", einfo.Namespace));
			if (einfo.IsNullable) att.Arguments.Add (GetArg ("IsNullable", true));
			AddCustomAttribute (codeField, att, false);
		}
		
		protected override void GenerateArrayItemAttributes (CodeMemberField codeField, ListMap listMap, TypeData type, XmlTypeMapElementInfo ainfo, string defaultName, string defaultNamespace, int nestingLevel)
		{
			bool needsType = (listMap.ItemInfo.Count > 1) ||
							 (ainfo.TypeData.FullTypeName != type.FullTypeName && !listMap.IsMultiArray);

			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlArrayItem");
			if (ainfo.ElementName != defaultName) att.Arguments.Add (GetArg ("ElementName", ainfo.ElementName));
			if (ainfo.Namespace != defaultNamespace && ainfo.Namespace != XmlSchema.Namespace) att.Arguments.Add (GetArg ("Namespace", ainfo.Namespace));
			if (needsType) att.Arguments.Add (GetTypeArg ("Type", ainfo.TypeData.FullTypeName));
			if (ainfo.IsNullable) att.Arguments.Add (GetArg ("IsNullable", true));
			if (att.Arguments.Count > 0 && nestingLevel > 0) att.Arguments.Add (GetArg ("NestingLevel", nestingLevel));
			AddCustomAttribute (codeField, att, false);
		}

		protected override void GenerateTextElementAttribute (CodeMemberField codeField, XmlTypeMapElementInfo einfo, TypeData defaultType)
		{
			CodeAttributeDeclaration uatt = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlText");
			if (einfo.TypeData.FullTypeName != defaultType.FullTypeName) uatt.Arguments.Add (GetTypeArg ("Type", einfo.TypeData.FullTypeName));
			AddCustomAttribute (codeField, uatt, true);
		}
		
		protected override void GenerateUnnamedAnyElementAttribute (CodeMemberField codeField, XmlTypeMapElementInfo einfo, string defaultNamespace)
		{
			CodeAttributeDeclaration uatt = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlAnyElement");
			if (!einfo.IsUnnamedAnyElement) uatt.Arguments.Add (GetArg ("Name", einfo.ElementName));
			if (einfo.Namespace != defaultNamespace) uatt.Arguments.Add (GetArg ("Namespace", einfo.Namespace));
			AddCustomAttribute (codeField, uatt, true);
		}
		
		protected override void GenerateEnum (XmlTypeMapping map, CodeTypeDeclaration codeEnum)
		{
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlTypeAttribute");
			if (map.ElementName != map.TypeData.TypeName) att.Arguments.Add (GetArg ("TypeName", map.ElementName));
			if (map.Namespace != "") att.Arguments.Add (GetArg ("Namespace", map.Namespace));
			AddCustomAttribute (codeEnum, att, false);
		}		
		
		protected override void GenerateEnumItem (CodeMemberField codeField, EnumMap.EnumMapMember emem)
		{
			if (emem.EnumName != emem.XmlName)
			{
				CodeAttributeDeclaration xatt = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlEnum");
				xatt.Arguments.Add (GetArg ("Name", emem.XmlName));

				AddCustomAttribute (codeField, xatt, true);
			}
		}
		
		protected override void GenerateSpecifierMember (CodeMemberField codeField)
		{
			AddCustomAttribute (codeField, "System.Xml.Serialization.XmlIgnore");
		}

	}
}
