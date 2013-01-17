// 
// System.Xml.Serialization.XmlCodeExporter 
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
using System.Collections;
using System.Xml.Schema;
using System.CodeDom.Compiler;

namespace System.Xml.Serialization 
{

	public class XmlCodeExporter 
#if NET_2_0
		: CodeExporter
#endif
	{
		#region Fields

#if NET_2_0
		// CodeGenerationOptions options;
#else
		XmlMapCodeGenerator codeGenerator;
#endif

		#endregion

		#region Constructors

		public XmlCodeExporter (CodeNamespace codeNamespace): this (codeNamespace, null)
		{
		}

		public XmlCodeExporter (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit)
		{
#if NET_2_0
			codeGenerator = new XmlMapCodeGenerator (codeNamespace, codeCompileUnit, CodeGenerationOptions.GenerateProperties);
#else
			codeGenerator = new XmlMapCodeGenerator (codeNamespace, codeCompileUnit, CodeGenerationOptions.None);
#endif
		}

#if NET_2_0
		public XmlCodeExporter (CodeNamespace codeNamespace, 
								CodeCompileUnit codeCompileUnit, 
								CodeGenerationOptions options)
		: this (codeNamespace, codeCompileUnit, null, options, null)
		{
		}
		
		public XmlCodeExporter (CodeNamespace codeNamespace, 
								CodeCompileUnit codeCompileUnit, 
								CodeGenerationOptions options, 
								Hashtable mappings)
		: this (codeNamespace, codeCompileUnit, null, options, mappings)
		{
			
		}
		
		[MonoTODO]// FIXME: mappings?
		public XmlCodeExporter (CodeNamespace codeNamespace, 
								CodeCompileUnit codeCompileUnit, 
								CodeDomProvider codeProvider, 
								CodeGenerationOptions options, 
								Hashtable mappings)
		{
			codeGenerator = new XmlMapCodeGenerator (codeNamespace, codeCompileUnit, codeProvider, options, mappings);
		}
#endif

		#endregion // Constructors

		#region Properties

#if !NET_2_0
		public CodeAttributeDeclarationCollection IncludeMetadata {
			get { return codeGenerator.IncludeMetadata; }
		}
#endif

		#endregion Properties

		#region Methods

		public void AddMappingMetadata (CodeAttributeDeclarationCollection metadata, XmlMemberMapping member, string ns)
		{
			AddMappingMetadata (metadata, member, ns, false);
		}

		public void AddMappingMetadata (CodeAttributeDeclarationCollection metadata, XmlTypeMapping mapping, string ns)
		{
			if ( (mapping.TypeData.SchemaType == SchemaTypes.Primitive ||
			      mapping.TypeData.SchemaType == SchemaTypes.Array) 
				&& mapping.Namespace != XmlSchema.Namespace)
			{
				CodeAttributeDeclaration ratt = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlRoot");
				ratt.Arguments.Add (MapCodeGenerator.GetArg (mapping.ElementName));
				ratt.Arguments.Add (MapCodeGenerator.GetArg ("Namespace", mapping.Namespace));
				ratt.Arguments.Add (MapCodeGenerator.GetArg ("IsNullable", mapping.IsNullable));
				metadata.Add (ratt);
			}
		}

		public void AddMappingMetadata (CodeAttributeDeclarationCollection metadata, XmlMemberMapping member, string ns, bool forceUseMemberName)
		{
			CodeAttributeDeclaration att;
			TypeData memType = member.TypeMapMember.TypeData;
			
			if (member.Any)
			{
				XmlTypeMapElementInfoList list = (XmlTypeMapElementInfoList)((XmlTypeMapMemberElement)member.TypeMapMember).ElementInfo;
				foreach (XmlTypeMapElementInfo info in list)
				{
					if (info.IsTextElement)
						metadata.Add (new CodeAttributeDeclaration ("System.Xml.Serialization.XmlText"));
					else {
						att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlAnyElement");
						if (!info.IsUnnamedAnyElement) {
							att.Arguments.Add (MapCodeGenerator.GetArg ("Name", info.ElementName));
							if (info.Namespace != ns) att.Arguments.Add (MapCodeGenerator.GetArg ("Namespace", member.Namespace));
						}
						metadata.Add (att);
					}
				}
			}
			else if (member.TypeMapMember is XmlTypeMapMemberList)
			{
				// Array parameter
				XmlTypeMapMemberList list = member.TypeMapMember as XmlTypeMapMemberList;
				ListMap listMap = (ListMap) list.ListTypeMapping.ObjectMap;
				
				codeGenerator.AddArrayAttributes (metadata, list, ns, forceUseMemberName);
				codeGenerator.AddArrayItemAttributes (metadata, listMap, memType.ListItemTypeData, list.Namespace, 0);
			}
			else if (member.TypeMapMember is XmlTypeMapMemberElement) {
				codeGenerator.AddElementMemberAttributes ((XmlTypeMapMemberElement) member.TypeMapMember, ns, metadata, forceUseMemberName);
			}
			else if (member.TypeMapMember is XmlTypeMapMemberAttribute) {
				codeGenerator.AddAttributeMemberAttributes ((XmlTypeMapMemberAttribute) member.TypeMapMember, ns, metadata, forceUseMemberName);
			}
			else
				throw new NotSupportedException ("Schema type not supported");
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
	
	class XmlMapCodeGenerator : MapCodeGenerator
	{
		public XmlMapCodeGenerator (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit, CodeGenerationOptions options)
		: base (codeNamespace, codeCompileUnit, options)
		{
		}

		public XmlMapCodeGenerator (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit, CodeDomProvider codeProvider, CodeGenerationOptions options, Hashtable mappings)
		: base (codeNamespace, codeCompileUnit, codeProvider, options, mappings)
		{
		}
		
		protected override void GenerateClass (XmlTypeMapping map, CodeTypeDeclaration codeClass, bool isTopLevel)
		{
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlTypeAttribute");
			if (map.XmlType != map.TypeData.TypeName) att.Arguments.Add (GetArg (map.XmlType));
			if (map.XmlTypeNamespace != "") att.Arguments.Add (GetArg ("Namespace", map.XmlTypeNamespace));
			if (!map.IncludeInSchema) att.Arguments.Add (GetArg ("IncludeInSchema", false));
			AddCustomAttribute (codeClass, att, false);

			CodeAttributeDeclaration ratt = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlRootAttribute");
			if (map.ElementName != map.XmlType) ratt.Arguments.Add (GetArg (map.ElementName));
			if (isTopLevel) {
				ratt.Arguments.Add (GetArg ("Namespace", map.Namespace));
				ratt.Arguments.Add (GetArg ("IsNullable", map.IsNullable));
			} else {
				if (map.Namespace != "") 
					ratt.Arguments.Add (GetArg ("Namespace", map.Namespace));
			}
			AddCustomAttribute (codeClass, ratt, isTopLevel);
		}
		
		protected override void GenerateClassInclude (CodeAttributeDeclarationCollection attributes, XmlTypeMapping map)
		{
			CodeAttributeDeclaration iatt = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlIncludeAttribute");
			iatt.Arguments.Add (new CodeAttributeArgument (new CodeTypeOfExpression(map.TypeData.FullTypeName)));
			attributes.Add (iatt);
		}
		
		protected override void GenerateAnyAttribute (CodeTypeMember codeField)
		{
			AddCustomAttribute (codeField, "System.Xml.Serialization.XmlAnyAttribute");
		}
		
		protected override void GenerateAttributeMember (CodeAttributeDeclarationCollection attributes, XmlTypeMapMemberAttribute attinfo, string defaultNamespace, bool forceUseMemberName)
		{
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlAttributeAttribute");
			if (forceUseMemberName || attinfo.Name != attinfo.AttributeName) att.Arguments.Add (GetArg (attinfo.AttributeName));
			if (attinfo.Namespace != defaultNamespace) att.Arguments.Add (GetArg ("Namespace", attinfo.Namespace));
			if (attinfo.Form == XmlSchemaForm.Qualified) att.Arguments.Add (GetEnumArg ("Form","System.Xml.Schema.XmlSchemaForm",attinfo.Form.ToString()));
			if (!TypeTranslator.IsDefaultPrimitiveTpeData(attinfo.TypeData)) att.Arguments.Add (GetArg ("DataType",attinfo.TypeData.XmlType));
			attributes.Add (att);
			
			if (attinfo.Ignore)
				attributes.Add (new CodeAttributeDeclaration ("System.Xml.Serialization.XmlIgnoreAttribute"));
		}
		
		protected override void GenerateElementInfoMember (CodeAttributeDeclarationCollection attributes, XmlTypeMapMemberElement member, XmlTypeMapElementInfo einfo, TypeData defaultType, string defaultNamespace, bool addAlwaysAttr, bool forceUseMemberName)
		{
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlElementAttribute");
			if (forceUseMemberName || einfo.ElementName != member.Name) att.Arguments.Add (GetArg (einfo.ElementName));
			if (einfo.TypeData.FullTypeName != defaultType.FullTypeName) att.Arguments.Add (GetTypeArg ("Type", einfo.TypeData.FullTypeName));
			if (einfo.Namespace != defaultNamespace) att.Arguments.Add (GetArg ("Namespace", einfo.Namespace));
			if (einfo.Form == XmlSchemaForm.Unqualified) att.Arguments.Add (GetEnumArg ("Form", "System.Xml.Schema.XmlSchemaForm", einfo.Form.ToString()));
			if (einfo.IsNullable) att.Arguments.Add (GetArg ("IsNullable", true));
			if (!TypeTranslator.IsDefaultPrimitiveTpeData(einfo.TypeData)) att.Arguments.Add (GetArg ("DataType",einfo.TypeData.XmlType));
			if (addAlwaysAttr || att.Arguments.Count > 0) attributes.Add (att);
		}
		
		protected override void GenerateElementMember (CodeAttributeDeclarationCollection attributes, XmlTypeMapMemberElement member)
		{
			if (member.ChoiceMember != null) {
				CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlChoiceIdentifier");
				att.Arguments.Add (GetArg(member.ChoiceMember));
				attributes.Add (att);
			}

			if (member.Ignore)
				attributes.Add (new CodeAttributeDeclaration ("System.Xml.Serialization.XmlIgnoreAttribute"));
		}
		
		protected override void GenerateArrayElement (CodeAttributeDeclarationCollection attributes, XmlTypeMapMemberElement member, string defaultNamespace, bool forceUseMemberName)
		{
			XmlTypeMapElementInfo einfo = (XmlTypeMapElementInfo) member.ElementInfo[0];
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlArray");
			if (forceUseMemberName || (einfo.ElementName != member.Name)) att.Arguments.Add (GetArg ("ElementName", einfo.ElementName));
			if (einfo.Namespace != defaultNamespace) att.Arguments.Add (GetArg ("Namespace", einfo.Namespace));
			if (einfo.Form == XmlSchemaForm.Unqualified) att.Arguments.Add (MapCodeGenerator.GetEnumArg ("Form", "System.Xml.Schema.XmlSchemaForm", einfo.Form.ToString()));
			if (einfo.IsNullable) att.Arguments.Add (GetArg ("IsNullable", true));
			if (att.Arguments.Count > 0) attributes.Add (att);
		}
		
		protected override void GenerateArrayItemAttributes (CodeAttributeDeclarationCollection attributes, ListMap listMap, TypeData type, XmlTypeMapElementInfo ainfo, string defaultName, string defaultNamespace, int nestingLevel)
		{
			bool needsType = (listMap.ItemInfo.Count > 1) ||
							 (ainfo.TypeData.FullTypeName != type.FullTypeName && !listMap.IsMultiArray);

			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlArrayItem");
			if (ainfo.ElementName != defaultName) att.Arguments.Add (GetArg ("ElementName", ainfo.ElementName));
			if (ainfo.Namespace != defaultNamespace && ainfo.Namespace != XmlSchema.Namespace) att.Arguments.Add (GetArg ("Namespace", ainfo.Namespace));
			if (needsType) att.Arguments.Add (GetTypeArg ("Type", ainfo.TypeData.FullTypeName));
			if (!ainfo.IsNullable) att.Arguments.Add (GetArg ("IsNullable", false));
			if (ainfo.Form == XmlSchemaForm.Unqualified) att.Arguments.Add (MapCodeGenerator.GetEnumArg ("Form", "System.Xml.Schema.XmlSchemaForm", ainfo.Form.ToString()));
			if (att.Arguments.Count > 0 && nestingLevel > 0) att.Arguments.Add (GetArg ("NestingLevel", nestingLevel));
			
			if (att.Arguments.Count > 0) attributes.Add (att);
		}

		protected override void GenerateTextElementAttribute (CodeAttributeDeclarationCollection attributes, XmlTypeMapElementInfo einfo, TypeData defaultType)
		{
			CodeAttributeDeclaration uatt = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlTextAttribute");
			if (einfo.TypeData.FullTypeName != defaultType.FullTypeName) uatt.Arguments.Add (GetTypeArg ("Type", einfo.TypeData.FullTypeName));
			attributes.Add (uatt);
		}
		
		protected override void GenerateUnnamedAnyElementAttribute (CodeAttributeDeclarationCollection attributes, XmlTypeMapElementInfo einfo, string defaultNamespace)
		{
			CodeAttributeDeclaration uatt = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlAnyElement");
			if (!einfo.IsUnnamedAnyElement) uatt.Arguments.Add (GetArg ("Name", einfo.ElementName));
			if (einfo.Namespace != defaultNamespace) uatt.Arguments.Add (GetArg ("Namespace", einfo.Namespace));
			attributes.Add (uatt);
		}

		protected override void GenerateEnum (XmlTypeMapping map, CodeTypeDeclaration codeEnum, bool isTopLevel)
		{
			GenerateClass (map, codeEnum, isTopLevel);
		}
		
		protected override void GenerateEnumItem (CodeMemberField codeField, EnumMap.EnumMapMember emem)
		{
			if (emem.EnumName != emem.XmlName)
			{
				CodeAttributeDeclaration xatt = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlEnumAttribute");
				xatt.Arguments.Add (GetArg (emem.XmlName));

				AddCustomAttribute (codeField, xatt, true);
			}
		}
		
		protected override void GenerateSpecifierMember (CodeTypeMember codeField)
		{
			AddCustomAttribute (codeField, "System.Xml.Serialization.XmlIgnore");
		}

	}
}
