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
		SoapMapCodeGenerator codeGenerator;

		#endregion

		#region Constructors

		public SoapCodeExporter (CodeNamespace codeNamespace): this (codeNamespace, null)
		{
		}

		public SoapCodeExporter (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit)
		{
			this.codeCompileUnit = codeCompileUnit;
			this.codeNamespace = codeNamespace;
			
			codeGenerator = new SoapMapCodeGenerator (codeNamespace, codeCompileUnit);
		}

		#endregion // Constructors

		#region Properties

		public CodeAttributeDeclarationCollection IncludeMetadata {
			get { return codeGenerator.IncludeMetadata; }
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
		
		protected override void GenerateClassInclude (CodeAttributeDeclarationCollection attributes, XmlTypeMapping map)
		{
			CodeAttributeDeclaration iatt = new CodeAttributeDeclaration ("System.Xml.Serialization.SoapInclude");
			iatt.Arguments.Add (new CodeAttributeArgument (new CodeTypeOfExpression(map.TypeData.FullTypeName)));
			attributes.Add (iatt);
		}
	
		protected override void GenerateAttributeMember (CodeAttributeDeclarationCollection attributes, XmlTypeMapMemberAttribute attinfo, string defaultNamespace, bool forceUseMemberName)
		{
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.SoapAttribute");
			if (attinfo.Name != attinfo.AttributeName) att.Arguments.Add (GetArg (attinfo.AttributeName));
			if (attinfo.Namespace != defaultNamespace) att.Arguments.Add (GetArg ("Namespace", attinfo.Namespace));
			if (!TypeTranslator.IsDefaultPrimitiveTpeData(attinfo.TypeData)) att.Arguments.Add (GetArg ("DataType",attinfo.TypeData.XmlType));
			attributes.Add (att);
		}
		
		protected override void GenerateElementInfoMember (CodeAttributeDeclarationCollection attributes, XmlTypeMapMemberElement member, XmlTypeMapElementInfo einfo, TypeData defaultType, string defaultNamespace, bool addAlwaysAttr, bool forceUseMemberName)
		{
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.SoapElement");
			if (forceUseMemberName || einfo.ElementName != member.Name) att.Arguments.Add (GetArg (einfo.ElementName));
//			if (einfo.IsNullable) att.Arguments.Add (GetArg ("IsNullable", true));	MS seems to ignore this
			if (!TypeTranslator.IsDefaultPrimitiveTpeData(einfo.TypeData)) att.Arguments.Add (GetArg ("DataType",einfo.TypeData.XmlType));
			if (addAlwaysAttr || att.Arguments.Count > 0) attributes.Add (att);
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
		
		protected override void GenerateSpecifierMember (CodeMemberField codeField)
		{
			AddCustomAttribute (codeField, "System.Xml.Serialization.SoapIgnore");
		}
	}
}
