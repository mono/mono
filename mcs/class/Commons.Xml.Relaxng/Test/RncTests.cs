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
using System.Xml;
using Commons.Xml.Relaxng;
using Commons.Xml.Relaxng.Rnc;
using NUnit.Framework;

namespace MonoTests.Commons.Xml.Relaxng
{
	[TestFixture]
	public class RncTests
	{
		[Test]
		public void TestRelaxngRnc ()
		{
			RncParser parser = new RncParser (new NameTable ());
			using (StreamReader sr = new StreamReader ("Test/XmlFiles/relaxng.rnc")) {
				RelaxngGrammar g = parser.Parse (sr);
				g.Compile ();
			}
		}
	}
}
