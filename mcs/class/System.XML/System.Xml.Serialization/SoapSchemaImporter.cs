// 
// System.Xml.Serialization.SoapSchemaImporter 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml;

namespace System.Xml.Serialization {
	public class SoapSchemaImporter {

		#region Fields

		XmlSchemas schemas;
		CodeIdentifiers typeIdentifiers;

		#endregion

		#region Constructors

		public SoapSchemaImporter (XmlSchemas schemas)
		{
			this.schemas = schemas;
		}

		public SoapSchemaImporter (XmlSchemas schemas, CodeIdentifiers typeIdentifiers)
			: this (schemas)
		{
			this.typeIdentifiers = typeIdentifiers;
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public XmlTypeMapping ImportDerivedTypeMapping (XmlQualifiedName name, Type baseType, bool baseTypeCanBeIndirect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlMembersMapping ImportMembersMapping (string name, string ns, SoapSchemaMember member)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlMembersMapping ImportMembersMapping (string name, string ns, SoapSchemaMember[] members)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlMembersMapping ImportMembersMapping (string name, string ns, SoapSchemaMember[] members, bool hasWrapperElement)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlMembersMapping ImportMembersMapping (string name, string ns, SoapSchemaMember[] members, bool hasWrapperElement, Type baseType, bool baseTypeCanBeIndirect)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
