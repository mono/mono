//
// System.Xml.Serialization.SoapSchemaExporter
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
// 	Lluis Sanchez Gual (lluis@ximian.com)
//
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System;

namespace System.Xml.Serialization
{
	public class SoapSchemaExporter
	{
		XmlSchemaExporter _exporter;
		
		public SoapSchemaExporter (XmlSchemas schemas)
		{
			_exporter = new XmlSchemaExporter(schemas, true);
		}

		public void ExportMembersMapping (XmlMembersMapping xmlMembersMapping)
		{
			_exporter.ExportMembersMapping (xmlMembersMapping, false);
		}

		public void ExportMembersMapping (XmlMembersMapping xmlMembersMapping,
						  bool exportEnclosingType)
		{
			_exporter.ExportMembersMapping (xmlMembersMapping, exportEnclosingType);
		}

		public void ExportTypeMapping (XmlTypeMapping xmlTypeMapping)
		{
			_exporter.ExportTypeMapping (xmlTypeMapping);
		}
	}
}
