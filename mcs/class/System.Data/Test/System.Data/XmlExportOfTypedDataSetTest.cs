using System.Xml.Serialization;
using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class XmlExportOfTypedDataSetTest
	{
#if NET_2_0
		[Test]
		public void ExportXmlSerializable_NestedClassMapping () {

			XmlSchemas schemas = new XmlSchemas ();

			XmlReflectionMember xmlReflectionMember = new XmlReflectionMember ();
			XmlSchemaExporter xmlSchemaExporter = new XmlSchemaExporter (schemas);
			XmlReflectionImporter xmlReflectionImporter = new XmlReflectionImporter ();

			//Export mapping for DataSet1 class.
			xmlReflectionMember.MemberType = typeof (DataSet1);
			XmlMembersMapping xmlMembersMapping = xmlReflectionImporter.ImportMembersMapping ("DataSet1Response", "ResponseNamespace",
				new XmlReflectionMember [] { xmlReflectionMember }, true);

			xmlSchemaExporter.ExportMembersMapping (xmlMembersMapping);

			//Export mapping for nested of DataSet1 class.
			xmlReflectionMember.MemberType = typeof (DataSet1.DataTable1DataTable);
			xmlMembersMapping = xmlReflectionImporter.ImportMembersMapping ("DataTable1DataTableResponse", "ResponseNamespace",
				new XmlReflectionMember [] { xmlReflectionMember }, true);

			xmlSchemaExporter.ExportMembersMapping (xmlMembersMapping);

		}
#endif
	}
}
