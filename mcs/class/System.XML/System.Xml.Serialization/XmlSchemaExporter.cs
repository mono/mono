// 
// System.Xml.Serialization.XmlSchemaExporter 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml;

namespace System.Xml.Serialization {
	public class XmlSchemaExporter {

		#region Fields

		XmlSchemas schemas;

		#endregion

		#region Constructors

		public XmlSchemaExporter (XmlSchemas schemas)
		{
			this.schemas = schemas;
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public string ExportAnyType (string ns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ExportMembersMapping (XmlMembersMapping xmlMembersMapping)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlQualifiedName ExportTypeMapping (XmlMembersMapping xmlMembersMapping)
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
