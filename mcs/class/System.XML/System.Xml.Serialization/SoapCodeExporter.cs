// 
// System.Xml.Serialization.SoapCodeExporter 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
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

		#endregion

		#region Constructors

		public SoapCodeExporter (CodeNamespace codeNamespace)
		{
			includeMetadata = new CodeAttributeDeclarationCollection ();
			this.codeNamespace = codeNamespace;
		}

		public SoapCodeExporter (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit)
			: this (codeNamespace)
		{
			this.codeCompileUnit = codeCompileUnit;
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
				att.Arguments.Add (new CodeAttributeArgument ("ElementName", new CodePrimitiveExpression(member.ElementName)));
			if (!TypeTranslator.IsDefaultPrimitiveTpeData (memType))
				att.Arguments.Add (new CodeAttributeArgument ("DataType", new CodePrimitiveExpression(member.TypeName)));
				
			if (att.Arguments.Count > 0) 
				metadata.Add (att);
		}

		[MonoTODO]
		public void ExportMembersMapping (XmlMembersMapping xmlMembersMapping)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ExportTypeMapping (XmlTypeMapping xmlTypeMapping)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
