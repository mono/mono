// 
// System.Xml.Serialization.XmlCodeExporter 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.CodeDom;

namespace System.Xml.Serialization {
	public class XmlCodeExporter {

		#region Fields

		CodeNamespace codeNamespace;
		CodeCompileUnit codeCompileUnit;
		CodeAttributeDeclarationCollection includeMetadata;

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

		[MonoTODO]
		public void ExportMembersMapping (XmlMembersMapping xmlMembersMapping)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlQualifiedName ExportTypeMapping (XmlTypeMapping xmlTypeMapping)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
