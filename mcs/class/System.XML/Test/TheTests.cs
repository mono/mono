using NUnit.Framework;
using System;
using System.Threading;
using System.Globalization;

namespace MonoTests.System.Xml
{
	public class RunXmlTextReaderTests : XmlTextReaderTests
	{
		protected override void RunTest ()
		{
			TestEmptyElement ();
			TestEmptyElementWithWhitespace ();
			TestEmptyElementWithStartAndEndTag ();
			TestEmptyElementWithStartAndEndTagWithWhitespace ();
			TestNestedEmptyTag ();
			TestNestedText ();
			TestEmptyElementWithAttribute ();
			TestStartAndEndTagWithAttribute ();
			TestEmptyElementWithTwoAttributes ();
			TestProcessingInstructionBeforeDocumentElement ();
			TestCommentBeforeDocumentElement ();
			TestPredefinedEntities ();
			TestEntityReference ();
			TestEntityReferenceInsideText ();
			TestCharacterReferences ();
			TestEntityReferenceInAttribute ();
			TestPredefinedEntitiesInAttribute ();
			TestCharacterReferencesInAttribute ();
			TestCDATA ();
			TestEmptyElementInNamespace ();
			TestEmptyElementInDefaultNamespace ();
			TestChildElementInNamespace ();
			TestChildElementInDefaultNamespace ();
			TestAttributeInNamespace ();
			TestIsName ();
			TestIsNameToken ();
		}
	}
}

namespace MonoTests.System.Xml
{
	public class RunXmlNamespaceManagerTests : XmlNamespaceManagerTests
	{
		protected override void RunTest ()
		{
			TestNewNamespaceManager ();
			TestAddNamespace ();
			TestPushScope ();
			TestPopScope ();
		}
	}
}

namespace MonoTests.System.Xml
{
	public class RunXmlDocumentTests : XmlDocumentTests
	{
		protected override void RunTest ()
		{
			TestDocumentElement ();
		}
	}
}

namespace MonoTests
{
	public class RunAllTests
	{
		public static void AddAllTests (TestSuite suite)
		{
			suite.AddTest (new MonoTests.System.Xml.RunXmlTextReaderTests ());
			suite.AddTest (new MonoTests.System.Xml.RunXmlNamespaceManagerTests ());
			suite.AddTest (new MonoTests.System.Xml.RunXmlDocumentTests ());
		}
	}
}

class MainApp
{
	public static void Main()
	{
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		TestResult result = new TestResult ();
		TestSuite suite = new TestSuite ();
		MonoTests.RunAllTests.AddAllTests (suite);
		suite.Run (result);
		MonoTests.MyTestRunner.Print (result);
	}
}

