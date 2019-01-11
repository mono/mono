//
// MonoTests.System.Xml.XsdParticleValidationTests.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C)2003 Atsushi Enomoto
//
using System;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

using ValidationException = System.Xml.Schema.XmlSchemaValidationException;

using MonoTests.Helpers;

namespace MonoTests.System.Xml
{
//	using XmlValidatingReader = XmlTextReader;

	[TestFixture]
	public class XsdParticleValidationTests
	{
		XmlSchema schema;
		XmlReader xr;
		XmlValidatingReader xvr;

		private void PrepareReader1 (string xsdUrl, string xml)
		{
			schema = XmlSchema.Read (new XmlTextReader (TestResourceHelper.GetFullPathOfResource ("Test/XmlFiles/XsdValidation/" + xsdUrl)), null);
			xr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xvr = new XmlValidatingReader (xr);
			xvr.Schemas.Add (schema);
//			xvr = xr as XmlValidatingReader;
		}

		[Test]
		public void ValidateRootElementOnlyValid ()
		{
			PrepareReader1 ("1.xsd", "<root xmlns='urn:foo' />");
			xvr.Read ();
			PrepareReader1 ("1.xsd", "<root xmlns='urn:foo'></root>");
			xvr.Read ();
			xvr.Read ();
		}

		[Test]
		// LAMESPEC: MS.NET throws XmlSchemaException, not -ValidationException.
		[ExpectedException (typeof (XmlSchemaException))]
		public void ValidateRootElementOnlyInvalid ()
		{
			PrepareReader1 ("1.xsd", "<invalid xmlns='urn:foo' />");
			xvr.Read ();
		}

		[Test]
		[ExpectedException (typeof (ValidationException))]
		public void ValidateRootElementOnlyInvalid2 ()
		{
			PrepareReader1 ("1.xsd", "<root xmlns='urn:foo'><invalid_child/></root>");
			xvr.Read ();
			xvr.Read ();
		}

		[Test]
		public void ValidateElementContainsElementValid1 ()
		{
			PrepareReader1 ("2.xsd", "<root xmlns='urn:foo'><child/></root>");
			while (!xvr.EOF)
				xvr.Read ();
		}

		[Test]
		public void ValidateElementContainsElementValid2 ()
		{
			PrepareReader1 ("2.xsd", "<root xmlns='urn:foo'><child/><child/></root>");
			while (!xvr.EOF)
				xvr.Read ();
		}

		[Test]
		[ExpectedException (typeof (ValidationException))]
		public void ValidateElementContainsElementInvalid1 ()
		{
			PrepareReader1 ("2.xsd", "<root xmlns='urn:foo'></root>");
			while (!xvr.EOF)
				xvr.Read ();
		}

		[Test]
		[ExpectedException (typeof (ValidationException))]
		public void ValidateElementContainsElementInvalid2 ()
		{
			PrepareReader1 ("2.xsd", "<root xmlns='urn:foo'><child/><child/><child/></root>");
			while (!xvr.EOF)
				xvr.Read ();
		}

		[Test]
		public void ValidateSequenceValid ()
		{
			PrepareReader1 ("3.xsd", "<root xmlns='urn:foo'><child1/><child2/></root>");
			while (!xvr.EOF)
				xvr.Read ();

			PrepareReader1 ("3.xsd", "<root xmlns='urn:foo'><child1/><child2/><child1/><child2/></root>");
			while (!xvr.EOF)
				xvr.Read ();
		}

		[Test]
		[ExpectedException (typeof (ValidationException))]
		public void ValidateSequenceInvalid1 ()
		{
			PrepareReader1 ("3.xsd", "<root xmlns='urn:foo'></root>");
			while (!xvr.EOF)
				xvr.Read ();
		}

		[Test]
		[ExpectedException (typeof (ValidationException))]
		public void ValidateSequenceInvalid2 ()
		{
			PrepareReader1 ("3.xsd", "<root xmlns='urn:foo'><child1/></root>");
			while (!xvr.EOF)
				xvr.Read ();
		}

		[Test]
		[ExpectedException (typeof (ValidationException))]
		public void ValidateSequenceInvalid3 ()
		{
			PrepareReader1 ("3.xsd", "<root xmlns='urn:foo'><child1/><child2/><child1/></root>");
			while (!xvr.EOF)
				xvr.Read ();
		}

		[Test]
		[ExpectedException (typeof (ValidationException))]
		public void ValidateSequenceInvalid4 ()
		{
			PrepareReader1 ("3.xsd", "<root xmlns='urn:foo'><child1/><child2/><child1/><child2/><child1/></root>");
			while (!xvr.EOF)
				xvr.Read ();
		}

		[Test]
		[ExpectedException (typeof (ValidationException))]
		public void ValidateSequenceInvalid5 ()
		{
			PrepareReader1 ("3.xsd", "<root xmlns='urn:foo'><child1/><child2/><child1/><child2/><child1/><child2/></root>");
			while (!xvr.EOF)
				xvr.Read ();
		}

		[Test]
		public void ValidateChoiceValid ()
		{
			PrepareReader1 ("4.xsd", "<root xmlns='urn:foo'><child1/></root>");
			while (!xvr.EOF)
				xvr.Read ();

			PrepareReader1 ("4.xsd", "<root xmlns='urn:foo'><child2/></root>");
			while (!xvr.EOF)
				xvr.Read ();

			PrepareReader1 ("4.xsd", "<root xmlns='urn:foo'><child1/><child2/></root>");
			while (!xvr.EOF)
				xvr.Read ();

			PrepareReader1 ("4.xsd", "<root xmlns='urn:foo'><child2/><child2/></root>");
			while (!xvr.EOF)
				xvr.Read ();

			PrepareReader1 ("4.xsd", "<root xmlns='urn:foo'><child2/><child2/><child2/><child2/></root>");
			while (!xvr.EOF)
				xvr.Read ();

			PrepareReader1 ("4.xsd", "<root xmlns='urn:foo'><child2/><child2/><child1/></root>");
			while (!xvr.EOF)
				xvr.Read ();

			PrepareReader1 ("4.xsd", "<root xmlns='urn:foo'></root>");
			while (!xvr.EOF)
				xvr.Read ();

		}

		[Test]
		[ExpectedException (typeof (ValidationException))]
		public void ValidateChoiceInvalid1 ()
		{
			PrepareReader1 ("4.xsd", "<root xmlns='urn:foo'><child1/><child1/><child1/></root>");
			while (!xvr.EOF)
				xvr.Read ();
		}

		[Test]
		[ExpectedException (typeof (ValidationException))]
		public void ValidateChoiceInvalid2 ()
		{
			PrepareReader1 ("3.xsd", "<root xmlns='urn:foo'><child2/><child2/><child2/><child2/><child2/></root>");
			while (!xvr.EOF)
				xvr.Read ();
		}

		[Test]
		[ExpectedException (typeof (ValidationException))]
		public void ValidateChoiceInvalid3 ()
		{
			PrepareReader1 ("3.xsd", "<root xmlns='urn:foo'><child2/><child2/><child2/><child1/></root>");
			while (!xvr.EOF)
				xvr.Read ();
		}

		[Test]
		[ExpectedException (typeof (ValidationException))]
		public void ValidateChoiceInvalid4 ()
		{
			PrepareReader1 ("3.xsd", "<root xmlns='urn:foo'><child1/><child2/><child2/><child2/></root>");
			while (!xvr.EOF)
				xvr.Read ();
		}

	}
}
