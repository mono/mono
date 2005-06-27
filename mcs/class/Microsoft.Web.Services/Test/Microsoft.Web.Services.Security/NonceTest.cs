//
// NonceTest.cs - NUnit Test Cases for Nonce
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
	public class NonceTest : Assertion {

		private const string Sample = "<wsse:Nonce xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/12/secext\">GWSRuy1IC4zb2jsA+Sz/rw==</wsse:Nonce>";
		private const string Zero = "<wsse:Nonce xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/12/secext\"></wsse:Nonce>";


		// NOTE: Nonce doesn't have a public constructor in WSE1 (*) so tests can only be runned with WSE2
		//	 (*) this is bad because we can't reuse Nonce inside other security protocols
#if !WSE1
		[Test]
		public void ConstructorInt () 
		{
			Nonce n = new Nonce (16);
		}

		[Test]
		public void ConstructorIntZero () 
		{
			Nonce n = new Nonce (0);
			AssertEquals ("Nonce(0).Value", String.Empty, n.Value);
			AssertNotNull ("Nonce(0).GetValueBytes()", n.GetValueBytes ());
			XmlDocument doc = new XmlDocument ();
			XmlElement xel = n.GetXml (doc);
			AssertEquals ("Nonce(0).GetXml", Zero, xel.OuterXml);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))] 
		public void ConstructorNegativeInt () 
		{
			Nonce n = new Nonce (-1);
		}

		[Test]
		public void ConstructorXmlElement () 
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (Sample);
			Nonce n = new Nonce (doc.DocumentElement);
			// roundtrip
			XmlElement xel = n.GetXml (doc);
			AssertEquals ("ConstructorXmlElement", Sample, xel.OuterXml);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void ConstructorXmlElementNull () 
		{
			Nonce n = new Nonce (null);
		}

		[Test]
		public void Value () 
		{
			Nonce n = new Nonce (16);
			AssertNotNull ("Value", n.Value);
			byte[] v = n.GetValueBytes ();
			AssertNotNull ("GetValueBytes", v);
			AssertEquals ("Value==base64(GetValueBytes)", n.Value, Convert.ToBase64String (v));
		}

		[Test] 
		public void GetXml ()
		{
			Nonce n = new Nonce (16);
			XmlDocument doc = new XmlDocument ();
			XmlElement xel = n.GetXml (doc);
			Assert ("GetXml.StartsWith", xel.OuterXml.StartsWith ("<wsse:Nonce xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/12/secext\">"));
			Assert ("GetXml.EndsWith", xel.OuterXml.EndsWith ("==</wsse:Nonce>"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void GetXmlNull () 
		{
			Nonce n = new Nonce (16);
			n.GetXml (null);
		}

		[Test]
		public void LoadXml () 
		{
			Nonce n = new Nonce (16);
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (Sample);
			n.LoadXml (doc.DocumentElement);
			// roundtrip
			XmlElement xel = n.GetXml (doc);
			AssertEquals ("LoadXml", Sample, xel.OuterXml);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] 
		public void LoadXml_BadLocalName () 
		{
			Nonce n = new Nonce (16);
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<wsse:SecurityTokenRef xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\" />");
			n.LoadXml (doc.DocumentElement);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] 
		public void LoadXml_BadNamespace () 
		{
			Nonce n = new Nonce (16);
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<wsse:SecurityTokenRef xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2202/07/secext\" />");
			n.LoadXml (doc.DocumentElement);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void LoadXmlNull () 
		{
			Nonce n = new Nonce (16);
			n.LoadXml (null);
		}
#endif
	}
}