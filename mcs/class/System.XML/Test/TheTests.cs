using NUnit.Framework;
using System;
using System.Threading;
using System.Globalization;

namespace Ximian.Mono.Tests
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

namespace Ximian.Mono.Tests
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

namespace Ximian.Mono.Tests
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
			suite.AddTest (new Ximian.Mono.Tests.RunXmlTextReaderTests ());
			suite.AddTest (new Ximian.Mono.Tests.RunXmlNamespaceManagerTests ());
			suite.AddTest (new Ximian.Mono.Tests.RunXmlDocumentTests ());
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

