using System.Xml.Serialization;
#if USE_MSUNITTEST
#if WINDOWS_PHONE || NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCategoryAttribute;
using AssertionException = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.UnitTestAssertException;
#else // !WINDOWS_PHONE && !NETFX_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute;
#endif // WINDOWS_PHONE || NETFX_CORE
#else // !USE_MSUNITTEST
using NUnit.Framework;
#endif // USE_MSUNITTEST

namespace MonoTests.System.Data
{
	[TestFixture]
	public class XmlExportOfTypedDataSetTest
	{
#if NET_2_0
#if !WINDOWS_PHONE && !NETFX_CORE
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
#endif
	}
}
