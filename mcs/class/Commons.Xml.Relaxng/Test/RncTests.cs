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

namespace MonoTests.Commons.Xml.Relaxng
{
	[TestFixture]
	public class RncTests
	{
		void Compile (string file)
		{
			using (StreamReader sr = new StreamReader (file)) {
				Compile (sr);
			}
		}

		void Compile (TextReader reader)
		{
			RncParser parser = new RncParser (new NameTable ());
			RelaxngPattern g = parser.Parse (reader);
			g.Compile ();
		}

		[Test]
		public void TestRelaxngRnc ()
		{
			Compile ("Test/XmlFiles/relaxng.rnc");
		}

		[Test]
		public void TestAtomRnc ()
		{
			Compile ("Test/XmlFiles/atom.rnc");
		}

		[Test]
		public void TestInfocardRnc ()
		{
			Compile ("Test/XmlFiles/schemas-xmlsoap-or-ws-2005-05-identity.rnc");
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
	}
}
