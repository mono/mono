// 
// System.Xml.Serialization.XmlSchemaImporter 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml;

namespace System.Xml.Serialization {
	public class XmlSchemaImporter {

		#region Fields

		XmlSchemas schemas;
		CodeIdentifiers typeIdentifiers;

		#endregion

		#region Constructors

		public XmlSchemaImporter (XmlSchemas schemas)
		{
			this.schemas = schemas;
		}

		public XmlSchemaImporter (XmlSchemas schemas, CodeIdentifiers typeIdentifiers)
			: this (schemas)
		{
			this.typeIdentifiers = typeIdentifiers;
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public XmlMembersMapping ImportAnyType (XmlQualifiedName typeName, string elementName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlTypeMapping ImportDerivedTypeMapping (XmlQualifiedName name, Type baseType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlTypeMapping ImportDerivedTypeMapping (XmlQualifiedName name, bool baseTypeCanBeIndirect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlMembersMapping ImportMembersMapping (XmlQualifiedName name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlMembersMapping ImportMembersMapping (XmlQualifiedName[] names)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlMembersMapping ImportMembersMapping (XmlQualifiedName[] names, Type baseType, bool baseTypeCanBeIndirect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlTypeMapping ImportTypeMapping (XmlQualifiedName name)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
