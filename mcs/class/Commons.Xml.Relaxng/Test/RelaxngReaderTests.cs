//
// RelaxngReaderTests.cs
//
// Authors:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2003 Atsushi Enomoto
//

using System;
using System.IO;
using System.Xml;
using Commons.Xml.Relaxng;
using NUnit.Framework;

namespace MonoTests.Commons.Xml.Relaxng
{
	[TestFixture]
	public class RelaxngReaderTests : Assertion
	{
		RelaxngReader reader;

		[SetUp]
		public void SetUp ()
		{
		}
		
		private void loadGrammarFromUrl (string url)
		{
			reader = new RelaxngReader (new XmlTextReader (url));
		}
		
		[Test]
		public void SimpleRead ()
		{
			loadGrammarFromUrl ("Test/XmlFiles/SimpleElementPattern1.rng");
			RelaxngPattern p = reader.ReadPattern ();

			AssertEquals (RelaxngPatternType.Element, p.PatternType);
		}

/*
		[Test]
		public void ValidateRelaxngGrammar ()
		{
			loadGrammarFromUrl ("XmlFiles/relaxng.rng");
			RelaxngPattern p = reader.ReadPattern ();

			AssertEquals (RelaxngPatternType.Grammar, p.PatternType);
		}
*/

	}
}
