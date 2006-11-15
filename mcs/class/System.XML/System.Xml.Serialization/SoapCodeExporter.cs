// 
// System.Xml.Serialization.SoapCodeExporter 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
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

using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;

namespace System.Xml.Serialization 
{
	public class SoapCodeExporter
#if NET_2_0
		: CodeExporter
#endif
	{
		#region Fields

#if !NET_2_0
		SoapMapCodeGenerator codeGenerator;
#endif

		#endregion

		#region Constructors

		public SoapCodeExporter (CodeNamespace codeNamespace): this (codeNamespace, null)
		{
		}

		public SoapCodeExporter (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit)
		{
			codeGenerator = new SoapMapCodeGenerator (codeNamespace, codeCompileUnit);
		}

#if NET_2_0

		public SoapCodeExporter (CodeNamespace codeNamespace, 
								CodeCompileUnit codeCompileUnit, 
								CodeGenerationOptions options)
		: this (codeNamespace, codeCompileUnit, null, options, null)
		{
		}
		
		public SoapCodeExporter (CodeNamespace codeNamespace, 
								CodeCompileUnit codeCompileUnit, 
								CodeGenerationOptions options, 
								Hashtable mappings)
		: this (codeNamespace, codeCompileUnit, null, options, mappings)
		{
			
		}
		
		[MonoTODO]// FIXME: mappings?
		public SoapCodeExporter (CodeNamespace codeNamespace, 
								CodeCompileUnit codeCompileUnit, 
								CodeDomProvider codeProvider, 
								CodeGenerationOptions options, 
								Hashtable mappings)
		{
			codeGenerator = new SoapMapCodeGenerator (codeNamespace, codeCompileUnit, codeProvider, options, mappings);
		}

#endif

#endregion // Constructors

		#region Properties

#if !NET_2_0
		public CodeAttributeDeclarationCollection IncludeMetadata {
			get { return codeGenerator.IncludeMetadata; }
		}
#endif
		
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
			codeGenerator.ExportTypeMapping (xmlTypeMapping, true);
		}


		#endregion // Methods
	}

	class SoapMapCodeGenerator : MapCodeGenerator
	{
		public SoapMapCodeGenerator (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit)
		: base (codeNamespace, codeCompileUnit, CodeGenerationOptions.None)
		{
			includeArrayTypes = true;
		}

		public SoapMapCodeGenerator (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit, CodeDomProvider codeProvider, CodeGenerationOptions options, Hashtable mappings)
		: base (codeNamespace, codeCompileUnit, codeProvider, options, mappings)
		{
		}

		protected override void GenerateClass (XmlTypeMapping map, CodeTypeDeclaration codeClass, bool isTopLevel)
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

		protected override void GenerateEnum (XmlTypeMapping map, CodeTypeDeclaration codeEnum, bool isTopLevel)
		{
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.SoapType");
			if (map.XmlType != map.TypeData.TypeName) att.Arguments.Add (GetArg (map.XmlType));
			if (map.XmlTypeNamespace != "") att.Arguments.Add (GetArg ("Namespace", map.XmlTypeNamespace));
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
		
		protected override void GenerateSpecifierMember (CodeTypeMember codeField)
		{
			AddCustomAttribute (codeField, "System.Xml.Serialization.SoapIgnore");
		}
	}
}
