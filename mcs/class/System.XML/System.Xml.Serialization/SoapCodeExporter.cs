// 
// System.Xml.Serialization.SoapCodeExporter 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.CodeDom;

namespace System.Xml.Serialization {
	public class SoapCodeExporter {

		#region Fields

		CodeNamespace codeNamespace;
		CodeCompileUnit codeCompileUnit;
		CodeAttributeDeclarationCollection includeMetadata;
		SoapMapCodeGenerator codeGenerator;

		#endregion

		#region Constructors

		public SoapCodeExporter (CodeNamespace codeNamespace): this (codeNamespace, null)
		{
		}

		public SoapCodeExporter (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit)
		{
			includeMetadata = new CodeAttributeDeclarationCollection ();
			this.codeCompileUnit = codeCompileUnit;
			this.codeNamespace = codeNamespace;
			
			codeGenerator = new SoapMapCodeGenerator (codeNamespace, codeCompileUnit);
		}

		#endregion // Constructors

		#region Properties

		public CodeAttributeDeclarationCollection IncludeMetadata {
			get { return includeMetadata; }
		}
		
		#endregion // Properties

		#region Methods

		public void AddMappingMetadata (CodeAttributeDeclarationCollection metadata, XmlMemberMapping member)
		{
			AddMappingMetadata (metadata, member, false);
		}

		public void AddMappingMetadata (CodeAttributeDeclarationCollection metadata, XmlMemberMapping member, bool forceUseMemberName)
		{
			TypeData memType = member.TypeMapMember.TypeData;
			
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.SoapElement");
			if (forceUseMemberName || (member.ElementName != member.MemberName))
				att.Arguments.Add (new CodeAttributeArgument (new CodePrimitiveExpression(member.ElementName)));
			if (!TypeTranslator.IsDefaultPrimitiveTpeData (memType))
				att.Arguments.Add (new CodeAttributeArgument ("DataType", new CodePrimitiveExpression(member.TypeName)));
				
			if (att.Arguments.Count > 0) 
				metadata.Add (att);
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

	class SoapMapCodeGenerator : MapCodeGenerator
	{
		public SoapMapCodeGenerator (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit)
		: base (codeNamespace, codeCompileUnit)
		{
		}

		protected override void GenerateClass (XmlTypeMapping map, CodeTypeDeclaration codeClass)
		{
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.SoapType");
			if (map.XmlType != map.TypeData.TypeName) att.Arguments.Add (GetArg (map.XmlType));
			if (map.XmlTypeNamespace != "") att.Arguments.Add (GetArg ("Namespace", map.XmlTypeNamespace));
			AddCustomAttribute (codeClass, att, false);
		}
		
		protected override void GenerateClassInclude (CodeTypeDeclaration codeClass, XmlTypeMapping map)
		{
			CodeAttributeDeclaration iatt = new CodeAttributeDeclaration ("System.Xml.Serialization.SoapInclude");
			iatt.Arguments.Add (new CodeAttributeArgument (new CodeTypeOfExpression(map.TypeData.FullTypeName)));
			AddCustomAttribute (codeClass, iatt, true);
		}
	
		protected override void GenerateDefaultAttribute (CodeMemberField codeField, object defaultValue)
		{
			AddCustomAttribute (codeField, "System.ComponentModel.DefaultValue", GetArg (defaultValue));
			codeField.InitExpression = new CodePrimitiveExpression (defaultValue);
		}
		
		protected override void GenerateAttributeMember (CodeMemberField codeField, XmlTypeMapMemberAttribute attinfo, string defaultNamespace)
		{
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.SoapAttribute");
			if (attinfo.Name != attinfo.AttributeName) att.Arguments.Add (GetArg (attinfo.AttributeName));
			if (attinfo.Namespace != defaultNamespace) att.Arguments.Add (GetArg ("Namespace", attinfo.Namespace));
			if (!TypeTranslator.IsDefaultPrimitiveTpeData(attinfo.TypeData)) att.Arguments.Add (GetArg ("DataType",attinfo.TypeData.XmlType));
			AddCustomAttribute (codeField, att, true);
		}
		
		protected override void GenerateElementInfoMember (CodeMemberField codeField, XmlTypeMapMemberElement member, XmlTypeMapElementInfo einfo, TypeData defaultType, string defaultNamespace, bool addAlwaysAttr)
		{
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.SoapElement");
			if (einfo.ElementName != member.Name) att.Arguments.Add (GetArg (einfo.ElementName));
//			if (einfo.IsNullable) att.Arguments.Add (GetArg ("IsNullable", true));	MS seems to ignore this
			if (!TypeTranslator.IsDefaultPrimitiveTpeData(einfo.TypeData)) att.Arguments.Add (GetArg ("DataType",einfo.TypeData.XmlType));
			AddCustomAttribute (codeField, att, addAlwaysAttr);
		}
		
		protected override void GenerateEnum (XmlTypeMapping map, CodeTypeDeclaration codeEnum)
		{
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.SoapType");
			if (map.ElementName != map.TypeData.TypeName) att.Arguments.Add (GetArg (map.ElementName));
			if (map.Namespace != "") att.Arguments.Add (GetArg ("Namespace", map.Namespace));
			AddCustomAttribute (codeEnum, att, false);
		}		
		
		protected override void GenerateEnumItem (CodeMemberField codeField, EnumMap.EnumMapMember emem)
		{
			if (emem.EnumName != emem.XmlName)
			{
				CodeAttributeDeclaration xatt = new CodeAttributeDeclaration ("System.Xml.Serialization.SoapEnum");
				xatt.Arguments.Add (GetArg ("Name", emem.XmlName));
				AddCustomAttribute (codeField, xatt, true);
			}
		}		
	}
}
