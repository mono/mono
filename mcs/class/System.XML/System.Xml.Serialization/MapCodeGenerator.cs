// 
// System.Xml.Serialization.MapCodeGenerator 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Ximian, Inc., 2003
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
using System.Globalization;
using System.Xml.Schema;

namespace System.Xml.Serialization {
	internal class MapCodeGenerator {

		CodeNamespace codeNamespace;
		CodeCompileUnit codeCompileUnit;
		CodeAttributeDeclarationCollection includeMetadata;
		XmlTypeMapping exportedAnyType = null;
		protected bool includeArrayTypes;
		ICodeGenerator codeGen;
		CodeGenerationOptions options;

		Hashtable exportedMaps = new Hashtable ();
		Hashtable includeMaps = new Hashtable ();

		public MapCodeGenerator (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit)
		{
			this.codeCompileUnit = codeCompileUnit;
			this.codeNamespace = codeNamespace;
			this.options = CodeGenerationOptions.GenerateOldAsync;
		}

		public MapCodeGenerator (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit, ICodeGenerator codeGen, CodeGenerationOptions options, Hashtable mappings)
		{
			this.codeCompileUnit = codeCompileUnit;
			this.codeNamespace = codeNamespace;
			this.codeGen = codeGen;
			this.options = options;
//			this.mappings = mappings;
		}

		public CodeAttributeDeclarationCollection IncludeMetadata 
		{
			get 
			{ 
				if (includeMetadata != null) return includeMetadata;
				includeMetadata = new CodeAttributeDeclarationCollection ();
				
				foreach (XmlTypeMapping map in includeMaps.Values)
					GenerateClassInclude (includeMetadata, map);
				
				return includeMetadata; 
			}
		}
		
		#region Code generation methods

		public void ExportMembersMapping (XmlMembersMapping xmlMembersMapping)
		{
			CodeTypeDeclaration dummyClass = new CodeTypeDeclaration ();
			ExportMembersMapCode (dummyClass, (ClassMap)xmlMembersMapping.ObjectMap, xmlMembersMapping.Namespace, null);
		}

		public void ExportTypeMapping (XmlTypeMapping xmlTypeMapping)
		{
			ExportMapCode (xmlTypeMapping);
			RemoveInclude (xmlTypeMapping);
		}

		void ExportMapCode (XmlTypeMapping map)
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
					ExportClassCode (map);
					break;

				case SchemaTypes.XmlSerializable:
				case SchemaTypes.XmlNode:
				case SchemaTypes.Primitive:
					// Ignore
					break;
			}
		}

		void ExportClassCode (XmlTypeMapping map)
		{
			CodeTypeDeclaration codeClass;
			
			if (IsMapExported (map)) {
				// Regenerate attributes, since things may have changed
				codeClass = GetMapDeclaration (map);
				if (codeClass != null) {
					codeClass.CustomAttributes = null;
					GenerateClass (map, codeClass);
					ExportDerivedTypeAttributes (map, codeClass);
				}
				return;
			}

			if (map.TypeData.Type == typeof(object))
			{
				exportedAnyType = map;
				SetMapExported (map, null);
				foreach (XmlTypeMapping dmap in exportedAnyType.DerivedTypes) {
					if (IsMapExported (dmap) || !dmap.IncludeInSchema) continue;
					ExportTypeMapping (dmap);
					AddInclude (dmap);
				}
				return;
			}
			
			codeClass = new CodeTypeDeclaration (map.TypeData.TypeName);
			SetMapExported (map, codeClass);

			AddCodeType (codeClass, map.Documentation);
			codeClass.Attributes = MemberAttributes.Public;

			GenerateClass (map, codeClass);
			ExportDerivedTypeAttributes (map, codeClass);
			
			ExportMembersMapCode (codeClass, (ClassMap)map.ObjectMap, map.XmlTypeNamespace, map.BaseMap);

			if (map.BaseMap != null && map.BaseMap.TypeData.SchemaType != SchemaTypes.XmlNode)
			{
				CodeTypeReference ctr = new CodeTypeReference (map.BaseMap.TypeData.FullTypeName);
				codeClass.BaseTypes.Add (ctr);
				if (map.BaseMap.IncludeInSchema) {
					ExportMapCode (map.BaseMap);
					AddInclude (map.BaseMap);
				}
			}
			ExportDerivedTypes (map, codeClass);
		}
		
		void ExportDerivedTypeAttributes (XmlTypeMapping map, CodeTypeDeclaration codeClass)
		{
			foreach (XmlTypeMapping tm in map.DerivedTypes)
			{
				GenerateClassInclude (codeClass.CustomAttributes, tm);
				ExportDerivedTypeAttributes (tm, codeClass);
			}
		}

		void ExportDerivedTypes (XmlTypeMapping map, CodeTypeDeclaration codeClass)
		{
			foreach (XmlTypeMapping tm in map.DerivedTypes)
			{
				if (codeClass.CustomAttributes == null) 
					codeClass.CustomAttributes = new CodeAttributeDeclarationCollection ();

				ExportMapCode (tm);
				ExportDerivedTypes (tm, codeClass);
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
				CodeTypeMember codeField = CreateFieldMember (codeClass, anyAttrMember.TypeData, anyAttrMember.Name);
				AddComments (codeField, anyAttrMember.Documentation);
				codeField.Attributes = MemberAttributes.Public;
				GenerateAnyAttribute (codeField);
			}
		}
		
		CodeTypeMember CreateFieldMember (CodeTypeDeclaration codeClass, Type type, string name)
		{
			return CreateFieldMember (codeClass, new CodeTypeReference(type), name, System.DBNull.Value, null, null);
		}

		CodeTypeMember CreateFieldMember (CodeTypeDeclaration codeClass, TypeData type, string name)
		{
			return CreateFieldMember (codeClass, GetDomType (type), name, System.DBNull.Value, null, null);
		}

		CodeTypeMember CreateFieldMember (CodeTypeDeclaration codeClass, XmlTypeMapMember member)
		{
			return CreateFieldMember (codeClass, GetDomType (member.TypeData), member.Name, member.DefaultValue, member.TypeData, member.Documentation);
		}
		
		CodeTypeMember CreateFieldMember (CodeTypeDeclaration codeClass, CodeTypeReference type, string name, object defaultValue, TypeData defaultType, string documentation)
		{
			CodeMemberField codeField = null;
			CodeTypeMember codeProp = null;
			
			if ((options & CodeGenerationOptions.GenerateProperties) > 0) {
				string field = CodeIdentifier.MakeCamel (name + "Field");
				codeField = new CodeMemberField (type, field);
				codeField.Attributes = MemberAttributes.Private;
				codeClass.Members.Add (codeField);
				
				CodeMemberProperty prop = new CodeMemberProperty ();
				prop.Name = name;
				prop.Type = type;
				prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
				codeProp = prop;
				prop.HasGet = prop.HasSet = true;
				
				CodeExpression ce = new CodeFieldReferenceExpression (new CodeThisReferenceExpression(), field);
				prop.SetStatements.Add (new CodeAssignStatement (ce, new CodePropertySetValueReferenceExpression()));
				prop.GetStatements.Add (new CodeMethodReturnStatement (ce));
 			}
			else {
				codeField = new CodeMemberField (type, name);
				codeField.Attributes = MemberAttributes.Public;
				codeProp = codeField;
			}
			
			if (defaultValue != System.DBNull.Value)
				GenerateDefaultAttribute (codeField, codeProp, defaultType, defaultValue);

			AddComments (codeProp, documentation);
			codeClass.Members.Add (codeProp);
			return codeProp;
		}

		void AddAttributeFieldMember (CodeTypeDeclaration codeClass, XmlTypeMapMemberAttribute attinfo, string defaultNamespace)
		{
			CodeTypeMember codeField = CreateFieldMember (codeClass, attinfo);

			CodeAttributeDeclarationCollection attributes = codeField.CustomAttributes;
			if (attributes == null) attributes = new CodeAttributeDeclarationCollection ();
			
			GenerateAttributeMember (attributes, attinfo, defaultNamespace, false);
			if (attributes.Count > 0) codeField.CustomAttributes = attributes;

			if (attinfo.MappedType != null) {
				ExportMapCode (attinfo.MappedType);
				RemoveInclude (attinfo.MappedType);
			}

			if (attinfo.TypeData.IsValueType && attinfo.IsOptionalValueType)
			{
				codeField = CreateFieldMember (codeClass, typeof(bool), attinfo.Name + "Specified");
				codeField.Attributes = MemberAttributes.Public;
				GenerateSpecifierMember (codeField);
			}
		}
		
		public void AddAttributeMemberAttributes (XmlTypeMapMemberAttribute attinfo, string defaultNamespace, CodeAttributeDeclarationCollection attributes, bool forceUseMemberName)
		{
			GenerateAttributeMember (attributes, attinfo, defaultNamespace, forceUseMemberName);
		}

		void AddElementFieldMember (CodeTypeDeclaration codeClass, XmlTypeMapMemberElement member, string defaultNamespace)
		{
			CodeTypeMember codeField = CreateFieldMember (codeClass, member);
			
			CodeAttributeDeclarationCollection attributes = codeField.CustomAttributes;
			if (attributes == null) attributes = new CodeAttributeDeclarationCollection ();
			
			AddElementMemberAttributes (member, defaultNamespace, attributes, false);
			if (attributes.Count > 0) codeField.CustomAttributes = attributes;
			
			if (member.TypeData.IsValueType && member.IsOptionalValueType)
			{
				codeField = CreateFieldMember (codeClass, typeof(bool), member.Name + "Specified");
				codeField.Attributes = MemberAttributes.Public;
				GenerateSpecifierMember (codeField);
			}
		}

		public void AddElementMemberAttributes (XmlTypeMapMemberElement member, string defaultNamespace, CodeAttributeDeclarationCollection attributes, bool forceUseMemberName)
		{
			TypeData defaultType = member.TypeData;
			bool addAlwaysAttr = false;
			
			if (member is XmlTypeMapMemberFlatList)
			{
				defaultType = defaultType.ListItemTypeData;
				addAlwaysAttr = true;
			}
			
			foreach (XmlTypeMapElementInfo einfo in member.ElementInfo)
			{
				if (ExportExtraElementAttributes (attributes, einfo, defaultNamespace, defaultType))
					continue;

				GenerateElementInfoMember (attributes, member, einfo, defaultType, defaultNamespace, addAlwaysAttr, forceUseMemberName);
				if (einfo.MappedType != null) {
					ExportMapCode (einfo.MappedType);
					RemoveInclude (einfo.MappedType);
				}
			}

			GenerateElementMember (attributes, member);
		}

		void AddAnyElementFieldMember (CodeTypeDeclaration codeClass, XmlTypeMapMemberElement member, string defaultNamespace)
		{
			CodeTypeMember codeField = CreateFieldMember (codeClass, member);

			CodeAttributeDeclarationCollection attributes = new CodeAttributeDeclarationCollection ();
			foreach (XmlTypeMapElementInfo einfo in member.ElementInfo)
				ExportExtraElementAttributes (attributes, einfo, defaultNamespace, einfo.TypeData);
				
			if (attributes.Count > 0) codeField.CustomAttributes = attributes;
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
			CodeTypeMember codeField = CreateFieldMember (codeClass, member.TypeData, member.Name);
			codeField.Attributes = MemberAttributes.Public;
			
			CodeAttributeDeclarationCollection attributes = new CodeAttributeDeclarationCollection ();
			AddArrayAttributes (attributes, member, defaultNamespace, false);

			ListMap listMap = (ListMap) member.ListTypeMapping.ObjectMap;
			AddArrayItemAttributes (attributes, listMap, member.TypeData.ListItemTypeData, defaultNamespace, 0);
			
			if (attributes.Count > 0) codeField.CustomAttributes = attributes;
		}

		public void AddArrayAttributes (CodeAttributeDeclarationCollection attributes, XmlTypeMapMemberElement member, string defaultNamespace, bool forceUseMemberName)
		{
			GenerateArrayElement (attributes, member, defaultNamespace, forceUseMemberName);
		}

		public void AddArrayItemAttributes (CodeAttributeDeclarationCollection attributes, ListMap listMap, TypeData type, string defaultNamespace, int nestingLevel)
		{
			foreach (XmlTypeMapElementInfo ainfo in listMap.ItemInfo)
			{
				string defaultName;
				if (ainfo.MappedType != null) defaultName = ainfo.MappedType.ElementName;
				else defaultName = ainfo.TypeData.XmlType;

				GenerateArrayItemAttributes (attributes, listMap, type, ainfo, defaultName, defaultNamespace, nestingLevel);
				if (ainfo.MappedType != null) {
					if (!IsMapExported (ainfo.MappedType) && includeArrayTypes)
						AddInclude (ainfo.MappedType);
					ExportMapCode (ainfo.MappedType);
				}
			}

			if (listMap.IsMultiArray)
			{
				XmlTypeMapping nmap = listMap.NestedArrayMapping;
				AddArrayItemAttributes (attributes, (ListMap) nmap.ObjectMap, nmap.TypeData.ListItemTypeData, defaultNamespace, nestingLevel + 1);
			}
		}
		
		void ExportArrayCode (XmlTypeMapping map)
		{
			ListMap listMap = (ListMap) map.ObjectMap;
			foreach (XmlTypeMapElementInfo ainfo in listMap.ItemInfo)
			{
				if (ainfo.MappedType != null) {
					if (!IsMapExported (ainfo.MappedType) && includeArrayTypes)
						AddInclude (ainfo.MappedType);
					ExportMapCode (ainfo.MappedType);
				}
			}
		}

		bool ExportExtraElementAttributes (CodeAttributeDeclarationCollection attributes, XmlTypeMapElementInfo einfo, string defaultNamespace, TypeData defaultType)
		{
			if (einfo.IsTextElement) {
				GenerateTextElementAttribute (attributes, einfo, defaultType);
				return true;
			}
			else if (einfo.IsUnnamedAnyElement) {
				GenerateUnnamedAnyElementAttribute (attributes, einfo, defaultNamespace);
				return true;
			}
			return false;
		}

		void ExportEnumCode (XmlTypeMapping map)
		{
			if (IsMapExported (map)) return;

			CodeTypeDeclaration codeEnum = new CodeTypeDeclaration (map.TypeData.TypeName);
			SetMapExported (map, codeEnum);
			
			codeEnum.Attributes = MemberAttributes.Public;
			codeEnum.IsEnum = true;
			AddCodeType (codeEnum, map.Documentation);

			GenerateEnum (map, codeEnum);
			EnumMap emap = (EnumMap) map.ObjectMap;
			
			if (emap.IsFlags)
				codeEnum.CustomAttributes.Add (new CodeAttributeDeclaration ("System.Flags"));

			int flag = 1;
			foreach (EnumMap.EnumMapMember emem in emap.Members)
			{
				CodeMemberField codeField = new CodeMemberField ("", emem.EnumName);
				if (emap.IsFlags) {
					codeField.InitExpression = new CodePrimitiveExpression (flag);
					flag *= 2;
				}
				
				AddComments (codeField, emem.Documentation);

				GenerateEnumItem (codeField, emem);
				codeEnum.Members.Add (codeField);
			}
		}
		
		void AddInclude (XmlTypeMapping map)
		{
			if (!includeMaps.ContainsKey (map.TypeData.FullTypeName))
				includeMaps [map.TypeData.FullTypeName] = map;
		}

		void RemoveInclude (XmlTypeMapping map)
		{
			includeMaps.Remove (map.TypeData.FullTypeName);
		}

		#endregion
		
		#region Helper methods
		
		bool IsMapExported (XmlTypeMapping map)
		{
			if (exportedMaps.Contains (map.TypeData.FullTypeName)) return true;
			return false;
		}

		void SetMapExported (XmlTypeMapping map, CodeTypeDeclaration declaration)
		{
			exportedMaps.Add (map.TypeData.FullTypeName, declaration);
		}

		CodeTypeDeclaration GetMapDeclaration (XmlTypeMapping map)
		{
			return exportedMaps [map.TypeData.FullTypeName] as CodeTypeDeclaration;
		}

		public static void AddCustomAttribute (CodeTypeMember ctm, CodeAttributeDeclaration att, bool addIfNoParams)
		{
			if (att.Arguments.Count == 0 && !addIfNoParams) return;
			
			if (ctm.CustomAttributes == null) ctm.CustomAttributes = new CodeAttributeDeclarationCollection ();
			ctm.CustomAttributes.Add (att);
		}

		public static void AddCustomAttribute (CodeTypeMember ctm, string name, params CodeAttributeArgument[] args)
		{
			if (ctm.CustomAttributes == null) ctm.CustomAttributes = new CodeAttributeDeclarationCollection ();
			ctm.CustomAttributes.Add (new CodeAttributeDeclaration (name, args));
		}

		public static CodeAttributeArgument GetArg (string name, object value)
		{
			return new CodeAttributeArgument (name, new CodePrimitiveExpression(value));
		}

		public static CodeAttributeArgument GetArg (object value)
		{
			return new CodeAttributeArgument (new CodePrimitiveExpression(value));
		}

		public static CodeAttributeArgument GetTypeArg (string name, string typeName)
		{
			return new CodeAttributeArgument (name, new CodeTypeOfExpression(typeName));
		}

		public static CodeAttributeArgument GetEnumArg (string name, string enumType, string enumValue)
		{
			return new CodeAttributeArgument (name, new CodeFieldReferenceExpression (new CodeTypeReferenceExpression(enumType), enumValue));
		}
		
		public static void AddComments (CodeTypeMember member, string comments)
		{
			if (comments == null || comments == "") member.Comments.Add (new CodeCommentStatement ("<remarks/>", true));
			else member.Comments.Add (new CodeCommentStatement ("<remarks>\n" + comments + "\n</remarks>", true));
		}

		void AddCodeType (CodeTypeDeclaration type, string comments)
		{
			AddComments (type, comments);
			codeNamespace.Types.Add (type);
		}
		
		CodeTypeReference GetDomType (TypeData data)
		{
			if (data.SchemaType == SchemaTypes.Array)
				return new CodeTypeReference (GetDomType (data.ListItemTypeData),1);
			else
				return new CodeTypeReference (data.FullTypeName);
		}
		
		#endregion
		
		#region Overridable methods
		
		protected virtual void GenerateClass (XmlTypeMapping map, CodeTypeDeclaration codeClass)
		{
		}
		
		protected virtual void GenerateClassInclude (CodeAttributeDeclarationCollection attributes, XmlTypeMapping map)
		{
		}
		
		protected virtual void GenerateAnyAttribute (CodeTypeMember codeField)
		{
		}
		
		protected virtual void GenerateDefaultAttribute (CodeMemberField internalField, CodeTypeMember externalField, TypeData typeData, object defaultValue)
		{
			if (typeData.Type == null)
			{
				// It must be an enumeration defined in the schema.
				if (typeData.SchemaType != SchemaTypes.Enum) 
					throw new InvalidOperationException ("Type " + typeData.TypeName + " not supported");

				IFormattable defaultValueFormattable = defaultValue as IFormattable;
				CodeFieldReferenceExpression fref = new CodeFieldReferenceExpression (new CodeTypeReferenceExpression (typeData.FullTypeName), defaultValueFormattable != null ? defaultValueFormattable.ToString(null, CultureInfo.InvariantCulture) : defaultValue.ToString ());
				CodeAttributeArgument arg = new CodeAttributeArgument (fref);
				AddCustomAttribute (externalField, "System.ComponentModel.DefaultValue", arg);
				internalField.InitExpression = fref;
			}
			else
			{
				AddCustomAttribute (externalField, "System.ComponentModel.DefaultValue", GetArg (defaultValue));
				internalField.InitExpression = new CodePrimitiveExpression (defaultValue);
			}
		}
		
		protected virtual void GenerateAttributeMember (CodeAttributeDeclarationCollection attributes, XmlTypeMapMemberAttribute attinfo, string defaultNamespace, bool forceUseMemberName)
		{
		}
		
		protected virtual void GenerateElementInfoMember (CodeAttributeDeclarationCollection attributes, XmlTypeMapMemberElement member, XmlTypeMapElementInfo einfo, TypeData defaultType, string defaultNamespace, bool addAlwaysAttr, bool forceUseMemberName)
		{
		}
		
		protected virtual void GenerateElementMember (CodeAttributeDeclarationCollection attributes, XmlTypeMapMemberElement member)
		{
		}
		
		protected virtual void GenerateArrayElement (CodeAttributeDeclarationCollection attributes, XmlTypeMapMemberElement member, string defaultNamespace, bool forceUseMemberName)
		{
		}
		
		protected virtual void GenerateArrayItemAttributes (CodeAttributeDeclarationCollection attributes, ListMap listMap, TypeData type, XmlTypeMapElementInfo ainfo, string defaultName, string defaultNamespace, int nestingLevel)
		{
		}

		protected virtual void GenerateTextElementAttribute (CodeAttributeDeclarationCollection attributes, XmlTypeMapElementInfo einfo, TypeData defaultType)
		{
		}
		
		protected virtual void GenerateUnnamedAnyElementAttribute (CodeAttributeDeclarationCollection attributes, XmlTypeMapElementInfo einfo, string defaultNamespace)
		{
		}
		
		protected virtual void GenerateEnum (XmlTypeMapping map, CodeTypeDeclaration codeEnum)
		{
		}		
		
		protected virtual void GenerateEnumItem (CodeMemberField codeField, EnumMap.EnumMapMember emem)
		{
		}

		protected virtual void GenerateSpecifierMember (CodeTypeMember codeField)
		{
		}

		#endregion
	}
}
