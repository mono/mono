//
// RncTests.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2004 Novell Inc.
//

using System;
using System.IO;
using System.Text;
using System.Xml;
using Commons.Xml.Relaxng;
using Commons.Xml.Relaxng.Rnc;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.Commons.Xml.Relaxng
{
	[TestFixture]
	public class RncTests
	{
		RelaxngPattern Compile (string file)
		{
			using (StreamReader sr = new StreamReader (file)) {
				return Compile (sr, file);
			}
		}

		RelaxngPattern Compile (TextReader reader)
		{
			return Compile (reader, null);
		}

		RelaxngPattern Compile (TextReader reader, string baseUri)
		{
			RncParser parser = new RncParser (new NameTable ());
			RelaxngPattern g = parser.Parse (reader, baseUri);
			g.Compile ();
			return g;
		}

		[Test]
		public void TestRelaxngRnc ()
		{
			Compile (TestResourceHelper.GetFullPathOfResource ("Test/XmlFiles/relaxng.rnc"));
		}

		[Test]
		public void TestAtomRnc ()
		{
			Compile (TestResourceHelper.GetFullPathOfResource ("Test/XmlFiles/atom.rnc"));
		}

		[Test]
		public void TestInfocardRnc ()
		{
			Compile (TestResourceHelper.GetFullPathOfResource ("Test/XmlFiles/schemas-xmlsoap-or-ws-2005-05-identity.rnc"));
		}

		[Test]
		// Make sure that it is not rejected by ambiguity between
		// foreign attribute and foreign element.
		public void Annotations ()
		{
			string rnc = @"
namespace s = ""urn:foo""
mine =
  [
    s:foo []
    s:foo = ""value""
  ]
  element foo { empty }

start = mine";
			Compile (new StringReader (rnc));
		}

		[Test]
		public void SurrogateLiteral ()
		{
			Compile (new StringReader ("element foo { \"\\x{10FFFF}\" }"));
		}

		[Test]
		public void InheritDefaultNamespace ()
		{
			RelaxngPattern g = Compile (TestResourceHelper.GetFullPathOfResource ("Test/XmlFiles/include-default-namespace.rnc"));
			XmlReader xtr = new XmlTextReader (TestResourceHelper.GetFullPathOfResource ("Test/XmlFiles/include-default-namespace.xml"));
			RelaxngValidatingReader r = new RelaxngValidatingReader (xtr, g);
			try {
				while (!r.EOF)
					r.Read ();
			} finally {
				r.Close ();
			}
		}
		
		[Test]
		public void SimpleDefaultNamespace ()
		{
			var g = RncParser.ParseRnc (new StringReader ("element e { empty }"));
			var x = XmlReader.Create (new StringReader ("<e/>"));
			var r = new RelaxngValidatingReader (x, g); 
			try {
				while (!r.EOF)
					r.Read ();
			} finally {
				r.Close ();
			}
		}
	}
}
