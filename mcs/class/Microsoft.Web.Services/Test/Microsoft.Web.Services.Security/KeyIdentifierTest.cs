//
// KeyIdentifierTest.cs - NUnit Test Cases for KeyIdentifier
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
	public class KeyIdentifierTest : Assertion {

		private static byte[] array = { 0x00 };
		private const string ValueXml = "<wsse:KeyIdentifier xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\">AA==</wsse:KeyIdentifier>";
		private const string ValueTypeXml = "<wsse:KeyIdentifier xmlns:vt=\"http://www.go-mono.com/\" ValueType=\"vt:mono\" xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\">AA==</wsse:KeyIdentifier>";
		private const string WellKnownValueTypeXml = "<wsse:KeyIdentifier ValueType=\"wsse:well-known\" xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\">AA==</wsse:KeyIdentifier>";
		private const string BadCustomValueTypeXml = "<wsse:KeyIdentifier ValueType=\"custom\" xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\">AA==</wsse:KeyIdentifier>";

		[Test]
		public void ConstructorByteArray () 
		{
			KeyIdentifier ki = new KeyIdentifier (array);
			Assert ("Value", (ki.Value [0] == array [0]));
			AssertNull ("ValueType", ki.ValueType);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void ConstructorByteArrayNull () 
		{
			byte[] array = null; // resolve ambiguity
			KeyIdentifier ki = new KeyIdentifier (array);
		}

		private XmlQualifiedName GetQName () 
		{
			return new XmlQualifiedName ("mono", "http://www.go-mono.com/");
		}

		[Test]
		public void ConstructorXmlQualifiedName () 
		{
			KeyIdentifier ki = new KeyIdentifier (array, GetQName ());
			Assert ("Value", (ki.Value [0] == array [0]));
			AssertNotNull ("ValueType", ki.ValueType);
			Assert ("ValueType.IsEmpty", !ki.ValueType.IsEmpty);
			AssertEquals ("ValueType.Name", "mono", ki.ValueType.Name);
			AssertEquals ("ValueType.Namespace", "http://www.go-mono.com/", ki.ValueType.Namespace);
		}

		[Test]
		public void ConstructorXmlQualifiedNameNull () 
		{
			KeyIdentifier ki = new KeyIdentifier (array, null);
			Assert ("Value", (ki.Value [0] == array [0]));
			AssertNull ("ValueType", ki.ValueType);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void ConstructorXmlElementNull () 
		{
			XmlElement xel = null; // resolve ambiguity
			KeyIdentifier ki = new KeyIdentifier (xel);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void ValueNull () 
		{
			KeyIdentifier ki = new KeyIdentifier (array);
			ki.Value = null;
		}

		[Test]
		public void ValueTypeNull () 
		{
			KeyIdentifier ki = new KeyIdentifier (array);
			ki.ValueType = null;
		}

		[Test] 
		public void GetXml_ValueOnly () 
		{
			KeyIdentifier ki = new KeyIdentifier (array);
			XmlDocument doc = new XmlDocument ();
			XmlElement xel = ki.GetXml (doc);
			AssertEquals ("GetXml_ValueOnly", ValueXml, xel.OuterXml);
		}

		[Test] 
		public void GetXml_ValueAndValueType ()
		{
			KeyIdentifier ki = new KeyIdentifier (array, GetQName ());
			XmlDocument doc = new XmlDocument ();
			XmlElement xel = ki.GetXml (doc);
			AssertEquals ("GetXml_ValueAndValueType", ValueTypeXml, xel.OuterXml);
		}

		[Test] 
		public void GetXml_WellKnownValueType () 
		{
			KeyIdentifier ki = new KeyIdentifier (array, new XmlQualifiedName ("well-known", WSSecurity.NamespaceURI));
			XmlDocument doc = new XmlDocument ();
			XmlElement xel = ki.GetXml (doc);
			AssertEquals ("GetXml_ValueAndValueType", WellKnownValueTypeXml, xel.OuterXml);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void GetXmlNull () 
		{
			KeyIdentifier ki = new KeyIdentifier (array);
			ki.GetXml (null);
		}

		[Test]
		public void LoadXml_Value ()
		{
			KeyIdentifier ki = new KeyIdentifier (array);
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (ValueXml);
			ki.LoadXml (doc.DocumentElement);
			// roundtrip
			XmlElement xel = ki.GetXml (doc);
			AssertEquals ("LoadXml_Value", ValueXml, xel.OuterXml);
		}

		[Test]
		public void LoadXml_ValueType () 
		{
			KeyIdentifier ki = new KeyIdentifier (array);
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (ValueTypeXml);
			ki.LoadXml (doc.DocumentElement);
			// roundtrip
			XmlElement xel = ki.GetXml (doc);
			AssertEquals ("LoadXml_ValueType", ValueTypeXml, xel.OuterXml);
		}

		[Test]
		public void LoadXml_WellKnownValueType () 
		{
			KeyIdentifier ki = new KeyIdentifier (array);
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (WellKnownValueTypeXml);
			ki.LoadXml (doc.DocumentElement);
			// roundtrip
			XmlElement xel = ki.GetXml (doc);
			AssertEquals ("LoadXml_ValueType", WellKnownValueTypeXml, xel.OuterXml);
		}

		[Test]
		[ExpectedException (typeof (SecurityFormatException))] 
		public void LoadXml_BadCustomValueType () 
		{
			KeyIdentifier ki = new KeyIdentifier (array);
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (BadCustomValueTypeXml);
			ki.LoadXml (doc.DocumentElement);
			// roundtrip
			XmlElement xel = ki.GetXml (doc);
			AssertEquals ("LoadXml_ValueType", BadCustomValueTypeXml, xel.OuterXml);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] 
		public void LoadXml_BadLocalName () 
		{
			KeyIdentifier ki = new KeyIdentifier (array);
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<wsse:KeyId xmlns:vt=\"http://www.go-mono.com/\" ValueType=\"vt:mono\" xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\">AA==</wsse:KeyId>");
			ki.LoadXml (doc.DocumentElement);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] 
		public void LoadXml_BadNamespace () 
		{
			KeyIdentifier ki = new KeyIdentifier (array);
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<wsse:KeyIdentifier xmlns:vt=\"http://www.go-mono.com/\" ValueType=\"vt:mono\" xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2202/07/secext\">AA==</wsse:KeyIdentifier>");
			ki.LoadXml (doc.DocumentElement);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void LoadXmlNull () 
		{
			KeyIdentifier ki = new KeyIdentifier (array);
			ki.LoadXml (null);
		}
	}
}