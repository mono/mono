//
// ReferenceListTest.cs - NUnit Test Cases for ReferenceList
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using Microsoft.Web.Services.Security;
using System;
using System.Collections;
using System.Xml;

namespace MonoTests.MS.Web.Services.Security {

	[TestFixture]
	public class ReferenceListTest : Assertion {

		private const string Empty = "<xenc:ReferenceList xmlns:xenc=\"http://www.w3.org/2001/04/xmlenc#\" />";
		private const string One = "<xenc:ReferenceList xmlns:xenc=\"http://www.w3.org/2001/04/xmlenc#\"><xenc:DataReference URI=\"#mono\" /></xenc:ReferenceList>";
		private const string NotLocalUri = "<xenc:ReferenceList xmlns:xenc=\"http://www.w3.org/2001/04/xmlenc#\"><xenc:DataReference URI=\"mono\" /></xenc:ReferenceList>";
		private const string NotDataReference = "<xenc:ReferenceList xmlns:xenc=\"http://www.w3.org/2001/04/xmlenc#\"><xenc:Reference URI=\"#mono\" /></xenc:ReferenceList>";

		[Test]
		public void Constructor () 
		{
			ReferenceList rl = new ReferenceList ();
			AssertNotNull ("Constructor()", rl);
		}

		[Test]
		public void ConstructorXmlElement () 
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (Empty);
			ReferenceList rl = new ReferenceList (doc.DocumentElement);
			AssertNotNull ("Constructor(XmlElement)", rl);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void ConstructorXmlElementNull () 
		{
			ReferenceList rl = new ReferenceList (null);
		}

		[Test]
		public void Add () 
		{
			ReferenceList rl = new ReferenceList ();
			rl.Add ("mono");
			Assert ("Add/Contains(mono)", rl.Contains ("mono"));
			Assert ("Add/Contains(#mono)", rl.Contains ("#mono"));
		}

		[Test]
		public void AddSharp () 
		{
			ReferenceList rl = new ReferenceList ();
			rl.Add ("#mono");
			Assert ("Add/Contains(mono)", rl.Contains ("mono"));
			Assert ("Add/Contains(#mono)", rl.Contains ("#mono"));
			Assert ("Add/Contains(##mono)", !rl.Contains ("##mono"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void AddNull ()
		{
			ReferenceList rl = new ReferenceList ();
			rl.Add (null);
		}

		[Test] 
		public void Contains () 
		{
			ReferenceList rl = new ReferenceList ();
			rl.Add ("mono");
			Assert ("Contains(mono)", rl.Contains ("mono"));
			Assert ("Contains(#mono)", rl.Contains ("#mono"));
			Assert ("!Contains(nomo)", !rl.Contains ("nomo"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void ContainsNull () 
		{
			ReferenceList rl = new ReferenceList ();
			Assert ("Contains", rl.Contains (null));
		}

		[Test]
		public void GetEnumetator ()
		{
			ReferenceList rl = new ReferenceList ();
			IEnumerator e = rl.GetEnumerator ();
			AssertNotNull ("GetEnumerator", e);
		}

		[Test]
		public void ListWithEnumerator () 
		{
			ReferenceList rl = new ReferenceList ();
			for (int i=0; i < 16; i++) {
				rl.Add ("mono" + i.ToString ());
			}
			int n = 0;
			foreach (string s in rl) {
				Assert (s.StartsWith ("mono"));
				n++;
			}
			AssertEquals ("Count", 16, n);
		}

		[Test]
		public void GetXml_Empty () 
		{
			ReferenceList rl = new ReferenceList ();
			XmlDocument doc = new XmlDocument ();
			XmlElement xel = rl.GetXml (doc);
			AssertEquals ("GetXml_Empty", Empty, xel.OuterXml);
		}

		[Test]
		public void GetXml_One () 
		{
			ReferenceList rl = new ReferenceList ();
			rl.Add ("mono");
			XmlDocument doc = new XmlDocument ();
			XmlElement xel = rl.GetXml (doc);
			AssertEquals ("GetXml_One", One, xel.OuterXml);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void GetXmlNull () 
		{
			ReferenceList rl = new ReferenceList ();
			rl.GetXml (null);
		}

		[Test]
		public void LoadXml_Empty () 
		{
			ReferenceList rl = new ReferenceList ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (Empty);
			rl.LoadXml (doc.DocumentElement);
			Assert ("LoadXml_Empty-Contains(mono)", !rl.Contains ("mono"));
			Assert ("LoadXml_Empty-Contains(#mono)", !rl.Contains ("#mono"));
		}

		[Test]
		public void LoadXml_One () 
		{
			ReferenceList rl = new ReferenceList ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (One);
			rl.LoadXml (doc.DocumentElement);
			Assert ("LoadXml_One", rl.Contains ("mono"));
			Assert ("LoadXml_One-Contains(mono)", rl.Contains ("mono"));
			Assert ("LoadXml_One-Contains(#mono)", rl.Contains ("#mono"));
		}

		[Test]
		public void LoadXml_NotLocalUri () 
		{
			ReferenceList rl = new ReferenceList ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (NotLocalUri); // mono, not #mono
			rl.LoadXml (doc.DocumentElement);
			Assert ("LoadXml_NotLocalUri-Contains(mono)", rl.Contains ("mono"));
			Assert ("LoadXml_NotLocalUri-Contains(#mono)", rl.Contains ("#mono"));
		}

		[Test]
		public void LoadXml_NotDataReference () 
		{
			ReferenceList rl = new ReferenceList ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (NotDataReference);
			rl.LoadXml (doc.DocumentElement);
			Assert ("LoadXml_NotDataReference(mono)", !rl.Contains ("mono"));
			Assert ("LoadXml_NotDataReference(#mono)", !rl.Contains ("#mono"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] 
		public void LoadXml_BadLocalName () 
		{
			ReferenceList rl = new ReferenceList ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<xenc:RefList xmlns:xenc=\"http://www.w3.org/2001/04/xmlenc#\" />");
			rl.LoadXml (doc.DocumentElement);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] 
		public void LoadXml_BadNamespace () 
		{
			ReferenceList rl = new ReferenceList ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<xenc:ReferenceList xmlns:xenc=\"http://www.w3.org/2201/04/xmlenc#\" />");
			rl.LoadXml (doc.DocumentElement);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void LoadXmlNull () 
		{
			ReferenceList rl = new ReferenceList ();
			rl.LoadXml (null);
		}
	}
}