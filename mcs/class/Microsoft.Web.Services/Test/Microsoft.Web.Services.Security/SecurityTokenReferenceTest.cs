//
// SecurityTokenReferenceTest.cs - NUnit Test Cases for SecurityTokenReference
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using Microsoft.Web.Services.Security;
using System;
using System.Xml;

namespace MonoTests.MS.Web.Services.Security {

	[TestFixture]
	public class SecurityTokenReferenceTest : Assertion {

		private const string Empty = "<wsse:SecurityTokenReference xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\" />";
		private const string ReferenceMono = "<wsse:SecurityTokenReference xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\"><wsse:Reference URI=\"#mono\" /></wsse:SecurityTokenReference>";
		private const string KeyId = "<wsse:SecurityTokenReference xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\"><wsse:KeyIdentifier>AA==</wsse:KeyIdentifier></wsse:SecurityTokenReference>";
		private const string Full = "<wsse:SecurityTokenReference xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\"><wsse:KeyIdentifier>AA==</wsse:KeyIdentifier><wsse:Reference URI=\"#mono\" /></wsse:SecurityTokenReference>";
		private const string BadReferenceURI = "<wsse:SecurityTokenReference xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\"><wsse:Reference URI=\"mono\" /></wsse:SecurityTokenReference>";

		[Test]
		public void ConstructorEmpty ()
		{
			SecurityTokenReference str = new SecurityTokenReference ();
			AssertNull ("SecurityTokenReference ().KeyIdentifier", str.KeyIdentifier);
			AssertNull ("SecurityTokenReference ().Reference", str.Reference);
		}


		[Test]
		public void ConstructorXmlElement () 
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (Empty);
			SecurityTokenReference str = new SecurityTokenReference (doc.DocumentElement);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void ConstructorXmlElementNull () 
		{
			XmlElement xel = null; // resolve ambiguity
			SecurityTokenReference str = new SecurityTokenReference (xel);
		}

		[Test]
		public void Reference () 
		{
			SecurityTokenReference str = new SecurityTokenReference ();
			str.Reference = null;
			AssertNull ("Reference=null", str.Reference);
			str.Reference = "mono";
			AssertEquals ("Reference=mono", "mono", str.Reference);
			str.Reference = "#mono";
			AssertEquals ("Reference=#mono", "#mono", str.Reference);
			str.Reference = null;
			AssertNull ("Reference=null(2)", str.Reference);
		}

		[Test]
		public void KeyIdentifier () 
		{
			SecurityTokenReference str = new SecurityTokenReference ();
			str.KeyIdentifier = null;
			AssertNull ("KeyIdentifier=null", str.KeyIdentifier);
			str.KeyIdentifier = new KeyIdentifier (new byte [1] { 0x00 });
			AssertNotNull ("KeyIdentifier=null", str.KeyIdentifier);
			str.KeyIdentifier = null;
			AssertNull ("KeyIdentifier=null(2)", str.KeyIdentifier);
		}

		[Test] 
		public void GetXmlEmpty () 
		{
			SecurityTokenReference str = new SecurityTokenReference ();
			XmlDocument doc = new XmlDocument ();
			XmlElement xel = str.GetXml (doc);
			XmlElement xele = str.GetXml ();
			AssertEquals ("GetXml()==GetXml(XmlDocument)", xel.OuterXml, xele.OuterXml);
			AssertEquals ("GetXml().OuterXml", Empty, xele.OuterXml);
		}

		[Test] 
		public void GetXml_Empty () 
		{
			SecurityTokenReference str = new SecurityTokenReference ();
			XmlDocument doc = new XmlDocument ();
			XmlElement xel = str.GetXml (doc);
			AssertEquals ("GetXml", Empty, xel.OuterXml);
		}

		[Test] 
		public void GetXml_Reference () {
			SecurityTokenReference str = new SecurityTokenReference ();
			str.Reference = "mono";
			XmlDocument doc = new XmlDocument ();
			XmlElement xel = str.GetXml (doc);
			AssertEquals ("GetXml", ReferenceMono, xel.OuterXml);
		}

		[Test] 
		public void GetXml_KeyId () 
		{
			SecurityTokenReference str = new SecurityTokenReference ();
			str.KeyIdentifier = new KeyIdentifier (new byte [1] { 0x00 });
			XmlDocument doc = new XmlDocument ();
			XmlElement xel = str.GetXml (doc);
			AssertEquals ("GetXml", KeyId, xel.OuterXml);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void GetXmlNull () 
		{
			SecurityTokenReference str = new SecurityTokenReference ();
			str.GetXml (null);
		}

		[Test]
		public void LoadXml_Empty () 
		{
			SecurityTokenReference str = new SecurityTokenReference ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (Empty);
			str.LoadXml (doc.DocumentElement);
			// roundtrip
			XmlElement xel = str.GetXml (doc);
			AssertEquals ("LoadXml", Empty, xel.OuterXml);
		}

		[Test]
		public void LoadXml_Reference () 
		{
			SecurityTokenReference str = new SecurityTokenReference ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (ReferenceMono);
			str.LoadXml (doc.DocumentElement);
			// roundtrip
			XmlElement xel = str.GetXml (doc);
			AssertEquals ("LoadXml", ReferenceMono, xel.OuterXml);
		}

		[Test]
		public void LoadXml_KeyId () 
		{
			SecurityTokenReference str = new SecurityTokenReference ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (KeyId);
			str.LoadXml (doc.DocumentElement);
			// roundtrip
			XmlElement xel = str.GetXml (doc);
			AssertEquals ("LoadXml", KeyId, xel.OuterXml);
		}

		[Test]
		public void LoadXml_Full () 
		{
			SecurityTokenReference str = new SecurityTokenReference ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (Full);
			str.LoadXml (doc.DocumentElement);
			// roundtrip
			XmlElement xel = str.GetXml (doc);
			AssertEquals ("LoadXml", Full, xel.OuterXml);
		}

		[Test]
		public void LoadXml_BadReferenceURI () 
		{
			SecurityTokenReference str = new SecurityTokenReference ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (BadReferenceURI);
			str.LoadXml (doc.DocumentElement);
			// roundtrip
			XmlElement xel = str.GetXml (doc);
			// can't duplicate this bad behaviour
			Assert ("LoadXml", BadReferenceURI != xel.OuterXml);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] 
		public void LoadXml_BadLocalName () 
		{
			SecurityTokenReference str = new SecurityTokenReference ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<wsse:SecurityTokenRef xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\" />");
			str.LoadXml (doc.DocumentElement);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] 
		public void LoadXml_BadNamespace () 
		{
			SecurityTokenReference str = new SecurityTokenReference ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<wsse:SecurityTokenRef xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2202/07/secext\" />");
			str.LoadXml (doc.DocumentElement);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void LoadXmlNull () 
		{
			SecurityTokenReference str = new SecurityTokenReference ();
			str.LoadXml (null);
		}
	}
}