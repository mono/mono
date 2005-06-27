//
// ConfigurationBaseTest.cs: ConfigurationBase Unit Tests
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Configuration;
using System.Xml;

using WSEC = Microsoft.Web.Services.Configuration;

using NUnit.Framework;

namespace MonoTests.MS.Web.Services.Configuration {

	[TestFixture]
	public class ConfigurationBaseTest : WSEC.ConfigurationBase {

		[Test]
		public void CheckForChildNodes_NoChildNode () 
		{
			string xml = "<node attrib=\"value\"/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			CheckForChildNodes (doc.DocumentElement);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CheckForChildNodes_Null ()
		{
			CheckForChildNodes (null);
		}

		[Test]
		[ExpectedException (typeof (ConfigurationException))]
		public void CheckForChildNodes_OneNode () 
		{
			string xml = "<test><childnode/></test>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			CheckForChildNodes (doc.DocumentElement);
		}

		[Test]
		public void CheckForDuplicateChildNodes_NoDupes () 
		{
			string xml = "<test><childnode/></test>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			CheckForDuplicateChildNodes (doc.DocumentElement);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CheckForDuplicateChildNodes_Null () 
		{
			CheckForDuplicateChildNodes (null);
		}

		[Test]
		[ExpectedException (typeof (ConfigurationException))]
		public void CheckForDuplicateChildNodes_Dupes () 
		{
			string xml = "<test><childnode/><childnode/></test>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			CheckForDuplicateChildNodes (doc.DocumentElement);
		}

		[Test]
		public void CheckForUnrecognizedAttributes_NoAttribute () 
		{
			string xml = "<node/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			CheckForUnrecognizedAttributes (doc.DocumentElement);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CheckForUnrecognizedAttributes_Null () 
		{
			CheckForUnrecognizedAttributes (null);
		}

		[Test]
		[ExpectedException (typeof (ConfigurationException))]
		public void CheckForUnrecognizedAttributes_OneAttribute () 
		{
			string xml = "<test attrib=\"value\"/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			CheckForUnrecognizedAttributes (doc.DocumentElement);
		}

		[Test]
		public void GetAndRemoveAttribute_AttribPresentNotRequired () 
		{
			string xml = "<test attrib=\"value\"/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XmlNode xn = GetAndRemoveAttribute (doc.DocumentElement, "attrib", false);
			Assertion.AssertEquals ("GetAndRemoveAttribute_AttribPresentNotRequired", "attrib=\"value\"", xn.OuterXml);
		}

		[Test]
		public void GetAndRemoveAttribute_AttribPresentAndRequired () 
		{
			string xml = "<test attrib=\"value\"/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XmlNode xn = GetAndRemoveAttribute (doc.DocumentElement, "attrib", true);
			Assertion.AssertEquals ("GetAndRemoveAttribute_AttribPresentAndRequired", "attrib=\"value\"", xn.OuterXml);
		}

		[Test]
		public void GetAndRemoveAttribute_AttribNotPresentNotRequired () 
		{
			string xml = "<test/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XmlNode xn = GetAndRemoveAttribute (doc.DocumentElement, "attrib", false);
			Assertion.AssertNull ("GetAndRemoveAttribute_AttribNotPresentNotRequired", xn);
		}

		[Test]
		[ExpectedException (typeof (ConfigurationException))]
		public void GetAndRemoveAttribute_AttribNotPresentAndRequired () 
		{
			string xml = "<test/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XmlNode xn = GetAndRemoveAttribute (doc.DocumentElement, "attrib", true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetAndRemoveAttribute_NullNode () 
		{
			GetAndRemoveAttribute (null, "attrib", true);
		}

		[Test]
		public void GetAndRemoveAttribute_NullAttrib () 
		{
			string xml = "<test/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XmlNode xn = GetAndRemoveAttribute (doc.DocumentElement, null, false);
			Assertion.AssertNull ("GetAndRemoveAttribute_NullAttrib", xn);
		}

		[Test]
		public void GetAndRemoveBoolAttribute_AttribPresentNotRequired () 
		{
			string xml = "<test attrib=\"true\"/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			bool result = false;
			XmlNode xn = GetAndRemoveBoolAttribute (doc.DocumentElement, "attrib", false, ref result);
			Assertion.AssertEquals ("GetAndRemoveBoolAttribute_AttribPresentNotRequired", "attrib=\"true\"", xn.OuterXml);
			Assertion.Assert ("GetAndRemoveBoolAttribute_AttribPresentNotRequired", result);
		}

		[Test]
		public void GetAndRemoveBoolAttribute_AttribPresentAndRequired () 
		{
			string xml = "<test attrib=\"true\"/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			bool result = false;
			XmlNode xn = GetAndRemoveBoolAttribute (doc.DocumentElement, "attrib", true, ref result);
			Assertion.AssertEquals ("GetAndRemoveBoolAttribute_AttribPresentAndRequired", "attrib=\"true\"", xn.OuterXml);
			Assertion.Assert ("GetAndRemoveBoolAttribute_AttribPresentAndRequired", result);
		}

		[Test]
		public void GetAndRemoveBoolAttribute_AttribNotPresentNotRequired () 
		{
			string xml = "<test/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			bool result = false;
			XmlNode xn = GetAndRemoveBoolAttribute (doc.DocumentElement, "attrib", false, ref result);
			Assertion.AssertNull ("GetAndRemoveBoolAttribute_AttribNotPresentNotRequired", xn);
			Assertion.Assert ("GetAndRemoveBoolAttribute_AttribNotPresentNotRequired", !result);
		}

		[Test]
		[ExpectedException (typeof (ConfigurationException))]
		public void GetAndRemoveBoolAttribute_AttribNotPresentAndRequired () 
		{
			string xml = "<test/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			bool result = false;
			XmlNode xn = GetAndRemoveBoolAttribute (doc.DocumentElement, "attrib", true, ref result);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetAndRemoveBoolAttribute_NullNode () 
		{
			bool result = false;
			GetAndRemoveBoolAttribute (null, "attrib", true, ref result);
		}

		[Test]
		public void GetAndRemoveBoolAttribute_NullAttrib () 
		{
			string xml = "<test/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			bool result = false;
			XmlNode xn = GetAndRemoveBoolAttribute (doc.DocumentElement, null, false, ref result);
			Assertion.AssertNull ("GetAndRemoveBoolAttribute_NullAttrib", xn);
			Assertion.Assert ("GetAndRemoveBoolAttribute_NullAttrib", !result);
		}

		[Test]
		public void GetAndRemoveStringAttribute_AttribPresentNotRequired () 
		{
			string xml = "<test attrib=\"true\"/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			string result = null;
			XmlNode xn = GetAndRemoveStringAttribute (doc.DocumentElement, "attrib", false, ref result);
			Assertion.AssertEquals ("GetAndRemoveStringAttribute_AttribPresentNotRequired", "attrib=\"true\"", xn.OuterXml);
			Assertion.AssertEquals ("GetAndRemoveStringAttribute_AttribPresentNotRequired", "true", result);
		}

		[Test]
		public void GetAndRemoveStringAttribute_AttribPresentAndRequired () 
		{
			string xml = "<test attrib=\"true\"/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			string result = null;
			XmlNode xn = GetAndRemoveStringAttribute (doc.DocumentElement, "attrib", true, ref result);
			Assertion.AssertEquals ("GetAndRemoveStringAttribute_AttribPresentAndRequired", "attrib=\"true\"", xn.OuterXml);
			Assertion.AssertEquals ("GetAndRemoveStringAttribute_AttribPresentAndRequired", "true", result);
		}

		[Test]
		public void GetAndRemoveStringAttribute_AttribNotPresentNotRequired () 
		{
			string xml = "<test/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			string result = null;
			XmlNode xn = GetAndRemoveStringAttribute (doc.DocumentElement, "attrib", false, ref result);
			Assertion.AssertNull ("GetAndRemoveBoolAttribute_AttribNotPresentNotRequired", xn);
			Assertion.AssertNull ("GetAndRemoveBoolAttribute_AttribNotPresentNotRequired", result);
		}

		[Test]
		[ExpectedException (typeof (ConfigurationException))]
		public void GetAndRemoveStringAttribute_AttribNotPresentAndRequired () 
		{
			string xml = "<test/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			string result = null;
			XmlNode xn = GetAndRemoveStringAttribute (doc.DocumentElement, "attrib", true, ref result);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetAndRemoveStringAttribute_NullNode () 
		{
			string result = null;
			GetAndRemoveStringAttribute (null, "attrib", true, ref result);
		}

		[Test]
		public void GetAndRemoveStringAttribute_NullAttrib () 
		{
			string xml = "<test/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			string result = null;
			XmlNode xn = GetAndRemoveStringAttribute (doc.DocumentElement, null, false, ref result);
			Assertion.AssertNull ("GetAndRemoveStringAttribute_NullAttrib", xn);
			Assertion.AssertNull ("GetAndRemoveStringAttribute_NullAttrib", result);
		}

		[Test]
		public void ThrowIfElement_NoElement () 
		{
			string xml = "<element><!-- comment --></element>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			ThrowIfElement (doc.DocumentElement.ChildNodes [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ThrowIfElement_Null () 
		{
			ThrowIfElement (null);
		}

		[Test]
		[ExpectedException (typeof (ConfigurationException))]
		public void ThrowIfElement_Element () 
		{
			string xml = "<element/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			ThrowIfElement (doc.DocumentElement);
		}

		[Test]
		public void ThrowIfNotComment_NoComment () 
		{
			string xml = "<element><!-- comment --></element>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			ThrowIfNotComment (doc.DocumentElement.ChildNodes [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ThrowIfNotComment_Null () 
		{
			ThrowIfNotComment (null);
		}

		[Test]
		[ExpectedException (typeof (ConfigurationException))]
		public void ThrowIfNotComment_Element () 
		{
			string xml = "<element/>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			ThrowIfNotComment (doc.DocumentElement);
		}
	}
}
