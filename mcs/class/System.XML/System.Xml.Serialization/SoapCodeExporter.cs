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

		#region Methods

		[MonoTODO]
		public void AddMappingMetadata (CodeAttributeDeclarationCollection metadata, XmlMemberMapping member)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AddMappingMetadata (CodeAttributeDeclarationCollection metadata, XmlMemberMapping member, bool forceUseMemberName)
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
