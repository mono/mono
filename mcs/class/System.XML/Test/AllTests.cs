// Author:
//   Mario Martinez (mariom925@home.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	/// <summary>
	///   Combines all unit tests for the System.XML.dll assembly
	///   into one test suite.
	/// </summary>
	public class AllTests : TestCase
	{
		public AllTests (string name) : base (name) {}

		public static ITest Suite {
			get {
				TestSuite suite =  new TestSuite ();
				suite.AddTest (new TestSuite (typeof (XmlProcessingInstructionTests)));
				suite.AddTest (new TestSuite (typeof (XmlTextTests)));
				suite.AddTest (new TestSuite (typeof (XmlTextReaderTests)));
				suite.AddTest (new TestSuite (typeof (XmlWriterTests)));
				suite.AddTest (new TestSuite (typeof (XmlTextWriterTests)));
				suite.AddTest (new TestSuite (typeof (XmlNamespaceManagerTests)));
				suite.AddTest (new TestSuite (typeof (XmlAttributeTests)));
				suite.AddTest (new TestSuite (typeof (XmlAttributeCollectionTests)));
				suite.AddTest (new TestSuite (typeof (XmlDocumentTests)));
				suite.AddTest (new TestSuite (typeof (XmlDocumentFragmentTests)));
				suite.AddTest (new TestSuite (typeof (NameTableTests)));
				suite.AddTest (new TestSuite (typeof (XmlElementTests)));
				suite.AddTest (new TestSuite (typeof (XmlEntityReferenceTests)));
				suite.AddTest (new TestSuite (typeof (XmlNodeTests)));
				suite.AddTest (new TestSuite (typeof (XmlNodeListTests)));
				suite.AddTest (new TestSuite (typeof (XmlCharacterDataTests)));
				suite.AddTest (new TestSuite (typeof (XmlCommentTests)));
				suite.AddTest (new TestSuite (typeof (XmlCDataSectionTests)));
				suite.AddTest (new TestSuite (typeof (XmlWhitespaceTests)));
				suite.AddTest (new TestSuite (typeof (XmlSignificantWhitespaceTests)));
				suite.AddTest (new TestSuite (typeof (XmlDeclarationTests)));
				suite.AddTest (new TestSuite (typeof (XmlDocumentTypeTests)));
				suite.AddTest (new TestSuite (typeof (XPathNavigatorTests)));
				suite.AddTest (new TestSuite (typeof (SelectNodesTests)));
				suite.AddTest (new TestSuite (typeof (XPathNavigatorMatchesTests)));
				suite.AddTest (new TestSuite (typeof (XPathNavigatorEvaluateTests)));
				return suite;
			}
		}
	}
}
